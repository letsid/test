using System;
using System.Xml;
using ClassicUO.Configuration;
using ClassicUO.Data;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Gumps;

internal class ContainerGump : TextContainerGump
{
	private class GumpPicContainer : GumpPic
	{
		public GumpPicContainer(int x, int y, ushort graphic, ushort hue)
			: base(x, y, graphic, hue)
		{
		}

		public override bool Contains(int x, int y)
		{
			float num = ((base.Graphic == 2330 || base.Graphic == 2350) ? 1f : UIManager.ContainerScale);
			x = (int)((float)x / num);
			y = (int)((float)y / num);
			return base.Contains(x, y);
		}
	}

	private long _corpseEyeTicks;

	private ContainerData _data;

	private int _eyeCorspeOffset;

	private GumpPic _eyeGumpPic;

	private GumpPicContainer _gumpPicContainer;

	private readonly bool _hideIfEmpty;

	private HitBox _hitBox;

	private bool _isMinimized;

	public ushort Graphic { get; }

	public override GumpType GumpType => GumpType.Container;

	public bool IsMinimized
	{
		get
		{
			return _isMinimized;
		}
		set
		{
			_isMinimized = value;
			_gumpPicContainer.Graphic = (value ? _data.IconizedGraphic : Graphic);
			float scale = GetScale();
			base.Width = (_gumpPicContainer.Width = (int)((float)_gumpPicContainer.Width * scale));
			base.Height = (_gumpPicContainer.Height = (int)((float)_gumpPicContainer.Height * scale));
			foreach (Control child in base.Children)
			{
				child.IsVisible = !value;
			}
			_gumpPicContainer.IsVisible = true;
			SetInScreen();
		}
	}

	public bool IsChessboard
	{
		get
		{
			if (Graphic != 2330)
			{
				return Graphic == 2350;
			}
			return true;
		}
	}

	public ContainerGump()
		: base(0u, 0u)
	{
	}

	public ContainerGump(uint serial, ushort gumpid, bool playsound)
		: base(serial, 0u)
	{
		Item item = World.Items.Get(serial);
		if (item == null)
		{
			Dispose();
			return;
		}
		Graphic = gumpid;
		if (item == World.Player.FindItemByLayer(Layer.Backpack) && Client.Version >= ClientVersion.CV_705301 && ProfileManager.CurrentProfile != null)
		{
			GumpsLoader instance = GumpsLoader.Instance;
			Rectangle bounds;
			switch (ProfileManager.CurrentProfile.BackpackStyle)
			{
			case 1:
				if (instance.GetGumpTexture(30558u, out bounds) != null)
				{
					Graphic = 30558;
				}
				break;
			case 2:
				if (instance.GetGumpTexture(30560u, out bounds) != null)
				{
					Graphic = 30560;
				}
				break;
			case 3:
				if (instance.GetGumpTexture(30562u, out bounds) != null)
				{
					Graphic = 30562;
				}
				break;
			default:
				if (instance.GetGumpTexture(60u, out bounds) != null)
				{
					Graphic = 60;
				}
				break;
			}
		}
		BuildGump();
		if (Graphic == 9)
		{
			if (World.Player.ManualOpenedCorpses.Contains(base.LocalSerial))
			{
				World.Player.ManualOpenedCorpses.Remove(base.LocalSerial);
			}
			else if (World.Player.AutoOpenedCorpses.Contains(base.LocalSerial) && ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.SkipEmptyCorpse)
			{
				base.IsVisible = false;
				_hideIfEmpty = true;
			}
		}
		if (_data.OpenSound != 0 && playsound)
		{
			Client.Game.Scene.Audio.PlaySound(_data.OpenSound);
		}
	}

	private void BuildGump()
	{
		CanMove = true;
		base.CanCloseWithRightClick = true;
		base.WantUpdateSize = false;
		Item item = World.Items.Get(base.LocalSerial);
		if (item == null)
		{
			Dispose();
			return;
		}
		float scale = GetScale();
		_data = ContainerManager.Get(Graphic);
		ushort graphic = _data.Graphic;
		_gumpPicContainer?.Dispose();
		_hitBox?.Dispose();
		_hitBox = new HitBox((int)((float)_data.MinimizerArea.X * scale), (int)((float)_data.MinimizerArea.Y * scale), (int)((float)_data.MinimizerArea.Width * scale), (int)((float)_data.MinimizerArea.Height * scale));
		_hitBox.MouseUp += HitBoxOnMouseUp;
		Add(_hitBox);
		Add(_gumpPicContainer = new GumpPicContainer(0, 0, graphic, 0));
		if ((_data.Flags & 1) != 0)
		{
			ushort hue = ((item.Hue > 0) ? item.Hue : ((ushort)_data.UncoloredHue));
			_gumpPicContainer.Hue = hue;
			_gumpPicContainer.IsPartialHue = (_data.Flags & 2) != 0;
		}
		_gumpPicContainer.MouseDoubleClick += GumpPicContainerOnMouseDoubleClick;
		if (Graphic == 9)
		{
			_eyeGumpPic?.Dispose();
			Add(_eyeGumpPic = new GumpPic((int)(45f * scale), (int)(30f * scale), 69, 0));
			_eyeGumpPic.Width = (int)((float)_eyeGumpPic.Width * scale);
			_eyeGumpPic.Height = (int)((float)_eyeGumpPic.Height * scale);
		}
		base.Width = (_gumpPicContainer.Width = (int)((float)_gumpPicContainer.Width * scale));
		base.Height = (_gumpPicContainer.Height = (int)((float)_gumpPicContainer.Height * scale));
	}

	private void HitBoxOnMouseUp(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtonType.Left && !IsMinimized && !ItemHold.Enabled)
		{
			Point lDragOffset = Mouse.LDragOffset;
			if (Math.Abs(lDragOffset.X) < 5 && Math.Abs(lDragOffset.Y) < 5)
			{
				IsMinimized = true;
			}
		}
	}

	private void GumpPicContainerOnMouseDoubleClick(object sender, MouseDoubleClickEventArgs e)
	{
		if (e.Button == MouseButtonType.Left && IsMinimized)
		{
			IsMinimized = false;
			e.Result = true;
		}
	}

	protected override void OnMouseUp(int x, int y, MouseButtonType button)
	{
		if (button != MouseButtonType.Left || UIManager.IsMouseOverWorld)
		{
			return;
		}
		Entity entity = SelectedObject.Object as Entity;
		uint serial = ((entity != null) ? entity.Serial : 0u);
		uint num = base.LocalSerial;
		if (TargetManager.IsTargeting && !ItemHold.Enabled && SerialHelper.IsValid(serial))
		{
			TargetManager.Target(serial);
			Mouse.CancelDoubleClick = true;
			if (TargetManager.TargetingState == CursorTarget.SetTargetClientSide)
			{
				UIManager.Add(new InspectorGump(World.Get(serial)));
			}
			return;
		}
		Entity entity2 = World.Items.Get(num);
		if (entity2 == null)
		{
			return;
		}
		entity2 = World.Get(((Item)entity2).RootContainer);
		if (entity2 == null)
		{
			return;
		}
		bool flag = entity2.Distance <= 3 || World.Player.NotorietyFlag == NotorietyFlag.Staff;
		if (flag && SerialHelper.IsValid(serial))
		{
			flag = false;
			if (ItemHold.Enabled && !ItemHold.IsFixedPosition)
			{
				flag = true;
				Item item = World.Items.Get(serial);
				if (item != null)
				{
					if (item.ItemData.IsContainer)
					{
						num = item.Serial;
						x = 65535;
						y = 65535;
					}
					else if (item.ItemData.IsStackable && item.Graphic == ItemHold.Graphic && item.Hue == ItemHold.Hue)
					{
						num = item.Serial;
						x = item.X;
						y = item.Y;
					}
					else
					{
						switch (item.Graphic)
						{
						case 3834:
						case 8786:
						case 8787:
						case 9100:
						case 9120:
						case 11600:
							num = item.Serial;
							x = item.X;
							y = item.Y;
							break;
						}
					}
				}
			}
		}
		if (!flag && ItemHold.Enabled && !ItemHold.IsFixedPosition)
		{
			Client.Game.Scene.Audio.PlaySound(81);
		}
		if (flag && ItemHold.Enabled && !ItemHold.IsFixedPosition)
		{
			ContainerGump gump = UIManager.GetGump<ContainerGump>(num);
			if (gump != null && (entity == null || (entity.Serial != num && entity is Item item2 && !item2.ItemData.IsContainer)))
			{
				if (gump.IsChessboard)
				{
					y += 20;
				}
				Rectangle bounds = ContainerManager.Get(gump.Graphic).Bounds;
				Rectangle bounds2;
				Texture2D obj = (gump.IsChessboard ? GumpsLoader.Instance.GetGumpTexture((ushort)(ItemHold.DisplayedGraphic - 11369), out bounds2) : ArtLoader.Instance.GetStaticTexture(ItemHold.DisplayedGraphic, out bounds2));
				float scale = GetScale();
				bounds.X = (int)((float)bounds.X * scale);
				bounds.Y = (int)((float)bounds.Y * scale);
				bounds.Width = (int)((float)bounds.Width * scale);
				bounds.Height = (int)((float)(bounds.Height + (gump.IsChessboard ? 20 : 0)) * scale);
				if (obj != null)
				{
					int num2;
					int num3;
					if (ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.ScaleItemsInsideContainers)
					{
						num2 = (int)((float)bounds2.Width * scale);
						num3 = (int)((float)bounds2.Height * scale);
					}
					else
					{
						num2 = bounds2.Width;
						num3 = bounds2.Height;
					}
					if (ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.RelativeDragAndDropItems)
					{
						x += ItemHold.MouseOffset.X;
						y += ItemHold.MouseOffset.Y;
					}
					x -= num2 >> 1;
					y -= num3 >> 1;
					if (x + num2 > bounds.Width)
					{
						x = bounds.Width - num2;
					}
					if (y + num3 > bounds.Height)
					{
						y = bounds.Height - num3;
					}
				}
				if (x < bounds.X)
				{
					x = bounds.X;
				}
				if (y < bounds.Y)
				{
					y = bounds.Y;
				}
				x = (int)((float)x / scale);
				y = (int)((float)y / scale);
			}
			GameActions.DropItem(ItemHold.Serial, x, y, 0, num);
			Mouse.CancelDoubleClick = true;
		}
		else if (!ItemHold.Enabled && SerialHelper.IsValid(serial) && !DelayedObjectClickManager.IsEnabled)
		{
			Point lDragOffset = Mouse.LDragOffset;
			DelayedObjectClickManager.Set(serial, Mouse.Position.X - lDragOffset.X - base.ScreenCoordinateX, Mouse.Position.Y - lDragOffset.Y - base.ScreenCoordinateY, Time.Ticks + 350);
		}
	}

	public override void Update(double totalTime, double frameTime)
	{
		base.Update(totalTime, frameTime);
		if (base.IsDisposed)
		{
			return;
		}
		Item item = World.Items.Get(base.LocalSerial);
		if (item == null || item.IsDestroyed)
		{
			Dispose();
			return;
		}
		if (UIManager.MouseOverControl != null && UIManager.MouseOverControl.RootParent == this && ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.HighlightContainerWhenSelected)
		{
			SelectedObject.SelectedContainer = item;
		}
		if (Graphic == 9 && (double)_corpseEyeTicks < totalTime)
		{
			_eyeCorspeOffset = ((_eyeCorspeOffset == 0) ? 1 : 0);
			_corpseEyeTicks = (long)totalTime + 750;
			_eyeGumpPic.Graphic = (ushort)(69 + _eyeCorspeOffset);
			float scale = GetScale();
			_eyeGumpPic.Width = (int)((float)_eyeGumpPic.Width * scale);
			_eyeGumpPic.Height = (int)((float)_eyeGumpPic.Height * scale);
		}
	}

	protected override void UpdateContents()
	{
		Clear();
		BuildGump();
		IsMinimized = IsMinimized;
		ItemsOnAdded();
	}

	public override void Save(XmlTextWriter writer)
	{
		base.Save(writer);
		writer.WriteAttributeString("graphic", Graphic.ToString());
		writer.WriteAttributeString("isminimized", IsMinimized.ToString());
	}

	public override void Restore(XmlElement xml)
	{
		base.Restore(xml);
		Client.Game.GetScene<GameScene>()?.DoubleClickDelayed(base.LocalSerial);
		Dispose();
	}

	private float GetScale()
	{
		if (!IsChessboard)
		{
			return UIManager.ContainerScale;
		}
		return 1f;
	}

	private void ItemsOnAdded()
	{
		Entity entity = World.Get(base.LocalSerial);
		if (entity == null)
		{
			return;
		}
		bool flag = entity.Graphic == 8198;
		if (!entity.IsEmpty && _hideIfEmpty && !base.IsVisible)
		{
			base.IsVisible = true;
		}
		for (LinkedObject linkedObject = entity.Items; linkedObject != null; linkedObject = linkedObject.Next)
		{
			Item item = (Item)linkedObject;
			if (((item.Layer == Layer.Invalid && item.ItemData.Layer != 15 && item.ItemData.Layer != 16 && item.ItemData.Layer != 11) || (flag && Constants.BAD_CONTAINER_LAYERS[(uint)item.Layer])) && item.Amount > 0)
			{
				ItemGump itemGump = new ItemGump(item.Serial, (ushort)(item.DisplayedGraphic - (IsChessboard ? 11369 : 0)), item.Hue, item.X, item.Y, IsChessboard);
				itemGump.IsVisible = !IsMinimized;
				float scale = GetScale();
				if (ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.ScaleItemsInsideContainers)
				{
					itemGump.Width = (int)((float)itemGump.Width * scale);
					itemGump.Height = (int)((float)itemGump.Height * scale);
				}
				itemGump.X = (int)((float)(short)item.X * scale);
				itemGump.Y = (int)((float)((short)item.Y - (IsChessboard ? 20 : 0)) * scale);
				Add(itemGump);
			}
		}
	}

	public void CheckItemControlPosition(Item item)
	{
		Rectangle bounds = _data.Bounds;
		int x = bounds.X;
		int y = bounds.Y;
		int num = bounds.Width;
		int num2 = bounds.Height + (IsChessboard ? 20 : 0);
		if ((IsChessboard ? GumpsLoader.Instance.GetGumpTexture((ushort)(item.DisplayedGraphic - (IsChessboard ? 11369 : 0)), out var bounds2) : ArtLoader.Instance.GetStaticTexture(item.DisplayedGraphic, out bounds2)) != null)
		{
			float scale = GetScale();
			num -= (int)((float)bounds2.Width / scale);
			num2 -= (int)((float)bounds2.Height / scale);
		}
		if (item.X < x)
		{
			item.X = (ushort)x;
		}
		else if (item.X > num)
		{
			item.X = (ushort)num;
		}
		if (item.Y < y)
		{
			item.Y = (ushort)y;
		}
		else if (item.Y > num2)
		{
			item.Y = (ushort)num2;
		}
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		base.Draw(batcher, x, y);
		if (CUOEnviroment.Debug && !IsMinimized)
		{
			Rectangle bounds = _data.Bounds;
			float scale = GetScale();
			ushort num = (ushort)((float)bounds.X * scale);
			ushort num2 = (ushort)((float)bounds.Y * scale);
			ushort num3 = (ushort)((float)bounds.Width * scale);
			ushort num4 = (ushort)((float)bounds.Height * scale);
			Vector3 hueVector = ShaderHueTranslator.GetHueVector(0);
			batcher.DrawRectangle(SolidColorTextureCache.GetTexture(Color.Red), x + num, y + num2, num3 - num, num4 - num2, hueVector);
		}
		return true;
	}

	public override void Dispose()
	{
		Item item = World.Items.Get(base.LocalSerial);
		if (item != null)
		{
			if (World.Player != null)
			{
				Profile currentProfile = ProfileManager.CurrentProfile;
				if (currentProfile != null && currentProfile.OverrideContainerLocationSetting == 3)
				{
					UIManager.SavePosition(item, base.Location);
				}
			}
			for (LinkedObject linkedObject = item.Items; linkedObject != null; linkedObject = linkedObject.Next)
			{
				Item item2 = (Item)linkedObject;
				if (item2.Container == (uint)item)
				{
					UIManager.GetGump<ContainerGump>(item2)?.Dispose();
				}
			}
		}
		base.Dispose();
	}

	protected override void CloseWithRightClick()
	{
		base.CloseWithRightClick();
		if (_data.ClosedSound != 0)
		{
			Client.Game.Scene.Audio.PlaySound(_data.ClosedSound);
		}
	}

	protected override void OnDragEnd(int x, int y)
	{
		if (ProfileManager.CurrentProfile.OverrideContainerLocation && ProfileManager.CurrentProfile.OverrideContainerLocationSetting >= 2)
		{
			Point overrideContainerLocationPosition = new Point(base.X + (base.Width >> 1), base.Y + (base.Height >> 1));
			ProfileManager.CurrentProfile.OverrideContainerLocationPosition = overrideContainerLocationPosition;
		}
		base.OnDragEnd(x, y);
	}
}

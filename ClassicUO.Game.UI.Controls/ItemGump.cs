using System;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Controls;

internal class ItemGump : Control
{
	private ushort _graphic;

	private readonly bool _is_gump;

	public ushort Graphic
	{
		get
		{
			return _graphic;
		}
		set
		{
			_graphic = value;
			if ((_is_gump ? GumpsLoader.Instance.GetGumpTexture(value, out var bounds) : ArtLoader.Instance.GetStaticTexture(value, out bounds)) == null)
			{
				Dispose();
				return;
			}
			base.Width = bounds.Width;
			base.Height = bounds.Height;
			IsPartialHue = !_is_gump && TileDataLoader.Instance.StaticData[value].IsPartialHue;
		}
	}

	public ushort Hue { get; set; }

	public bool IsPartialHue { get; set; }

	public bool HighlightOnMouseOver { get; set; }

	public bool CanPickUp { get; set; }

	public ItemGump(uint serial, ushort graphic, ushort hue, int x, int y, bool is_gump = false)
	{
		_is_gump = is_gump;
		AcceptMouseInput = true;
		base.X = (short)x;
		base.Y = (short)y;
		HighlightOnMouseOver = true;
		CanPickUp = true;
		base.LocalSerial = serial;
		base.WantUpdateSize = false;
		CanMove = false;
		Graphic = graphic;
		Hue = hue;
		SetTooltip(serial);
	}

	public override void Update(double totalTime, double frameTime)
	{
		if (base.IsDisposed)
		{
			return;
		}
		base.Update(totalTime, frameTime);
		if (World.InGame)
		{
			if (CanPickUp && !ItemHold.Enabled && Mouse.LButtonPressed && UIManager.LastControlMouseDown(MouseButtonType.Left) == this && ((Mouse.LastLeftButtonClickTime != uint.MaxValue && Mouse.LastLeftButtonClickTime != 0 && Mouse.LastLeftButtonClickTime + 350 < Time.Ticks) || CanPickup()))
			{
				AttemptPickUp();
			}
			else if (base.MouseIsOver)
			{
				SelectedObject.Object = World.Get(base.LocalSerial);
			}
		}
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		if (base.IsDisposed)
		{
			return false;
		}
		base.Draw(batcher, x, y);
		bool partial = IsPartialHue;
		ushort hue = Hue;
		if (HighlightOnMouseOver && base.MouseIsOver)
		{
			hue = (ushort)((!ProfileManager.CurrentProfile.HighlightGameObjects) ? 53 : ProfileManager.CurrentProfile.HighlightGameObjectsColor);
			partial = false;
		}
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(hue, partial, 1f);
		Rectangle bounds;
		Texture2D texture2D = (_is_gump ? GumpsLoader.Instance.GetGumpTexture(Graphic, out bounds) : ArtLoader.Instance.GetStaticTexture(Graphic, out bounds));
		if (texture2D != null)
		{
			Rectangle destinationRectangle = new Rectangle(x, y, base.Width, base.Height);
			batcher.Draw(texture2D, destinationRectangle, bounds, hueVector);
			Item item = World.Items.Get(base.LocalSerial);
			if (item != null && !item.IsMulti && !item.IsCoin && item.Amount > 1 && item.ItemData.IsStackable)
			{
				destinationRectangle.X += 5;
				destinationRectangle.Y += 5;
				batcher.Draw(texture2D, destinationRectangle, bounds, hueVector);
			}
		}
		return true;
	}

	public override bool Contains(int x, int y)
	{
		if ((_is_gump ? GumpsLoader.Instance.GetGumpTexture(Graphic, out var bounds) : ArtLoader.Instance.GetStaticTexture(Graphic, out bounds)) == null)
		{
			return false;
		}
		x -= base.Offset.X;
		y -= base.Offset.Y;
		if (ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.ScaleItemsInsideContainers)
		{
			float containerScale = UIManager.ContainerScale;
			x = (int)((float)x / containerScale);
			y = (int)((float)y / containerScale);
		}
		if (_is_gump)
		{
			if (GumpsLoader.Instance.PixelCheck(Graphic, x, y))
			{
				return true;
			}
			Item item = World.Items.Get(base.LocalSerial);
			if (item != null && !item.IsCoin && item.Amount > 1 && item.ItemData.IsStackable && GumpsLoader.Instance.PixelCheck(Graphic, x - 5, y - 5))
			{
				return true;
			}
		}
		else
		{
			if (ArtLoader.Instance.PixelCheck(Graphic, x, y))
			{
				return true;
			}
			Item item2 = World.Items.Get(base.LocalSerial);
			if (item2 != null && !item2.IsCoin && item2.Amount > 1 && item2.ItemData.IsStackable && ArtLoader.Instance.PixelCheck(Graphic, x - 5, y - 5))
			{
				return true;
			}
		}
		return false;
	}

	protected override void OnMouseUp(int x, int y, MouseButtonType button)
	{
		SelectedObject.Object = World.Get(base.LocalSerial);
		base.OnMouseUp(x, y, button);
	}

	protected override void OnMouseOver(int x, int y)
	{
		SelectedObject.Object = World.Get(base.LocalSerial);
	}

	private bool CanPickup()
	{
		Point lDragOffset = Mouse.LDragOffset;
		if (Math.Abs(lDragOffset.X) < 5 && Math.Abs(lDragOffset.Y) < 5)
		{
			return false;
		}
		SplitMenuGump gump = UIManager.GetGump<SplitMenuGump>(base.LocalSerial);
		if (gump == null)
		{
			return true;
		}
		gump.X = Mouse.LClickPosition.X - 80;
		gump.Y = Mouse.LClickPosition.Y - 40;
		UIManager.AttemptDragControl(gump, attemptAlwaysSuccessful: true);
		gump.BringOnTop();
		return false;
	}

	protected override bool OnMouseDoubleClick(int x, int y, MouseButtonType button)
	{
		if (button != MouseButtonType.Left || TargetManager.IsTargeting)
		{
			return false;
		}
		Item item = World.Items.Get(base.LocalSerial);
		Item item2;
		if (!Keyboard.Ctrl && ProfileManager.CurrentProfile.DoubleClickToLootInsideContainers && item != null && !item.IsDestroyed && !item.ItemData.IsContainer && item.IsEmpty && (item2 = World.Items.Get(item.RootContainer)) != null && item2 != World.Player.FindItemByLayer(Layer.Backpack))
		{
			GameActions.GrabItem(base.LocalSerial, item.Amount);
		}
		else
		{
			GameActions.DoubleClick(base.LocalSerial);
		}
		return true;
	}

	private void AttemptPickUp()
	{
		if (CanPickUp)
		{
			Rectangle bounds;
			if (!_is_gump)
			{
				ArtLoader.Instance.GetStaticTexture(Graphic, out bounds);
			}
			else
			{
				GumpsLoader.Instance.GetGumpTexture(Graphic, out bounds);
			}
			int num = bounds.Width >> 1;
			int num2 = bounds.Height >> 1;
			if (ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.ScaleItemsInsideContainers)
			{
				float containerScale = UIManager.ContainerScale;
				num = (int)((float)num * containerScale);
				num2 = (int)((float)num2 * containerScale);
			}
			if (ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.RelativeDragAndDropItems)
			{
				GameActions.PickUp(offset: new Point(num - (Mouse.LClickPosition.X - base.ScreenCoordinateX), num2 - (Mouse.LClickPosition.Y - base.ScreenCoordinateY)), serial: base.LocalSerial, x: num, y: num2, amount: -1, is_gump: _is_gump);
				return;
			}
			uint localSerial = base.LocalSerial;
			int x = num;
			int y = num2;
			bool is_gump = _is_gump;
			GameActions.PickUp(localSerial, x, y, -1, null, is_gump);
		}
	}
}

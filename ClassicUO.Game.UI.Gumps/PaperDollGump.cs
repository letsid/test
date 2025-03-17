using System;
using System.Collections.Generic;
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
using ClassicUO.Network;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Gumps;

internal class PaperDollGump : TextContainerGump
{
	public class PaperdollGumpPositioning
	{
		public ushort GumpID;

		public int x_Title;

		public int y_Title;

		public PaperdollGumpPositioning(ushort GumpID, int X_Title, int Y_Title)
		{
			this.GumpID = GumpID;
			x_Title = X_Title;
			y_Title = Y_Title;
		}
	}

	private enum Buttons
	{
		Help,
		Options,
		LogOut,
		Journal,
		Skills,
		ActionBook,
		PeaceWarToggle,
		Status
	}

	private class EquipmentSlot : Control
	{
		private class ItemGumpFixed : ItemGump
		{
			private readonly Point _originalSize;

			private readonly Point _point;

			private readonly Rectangle _rect;

			public ItemGumpFixed(Item item, int w, int h)
				: base(item.Serial, item.DisplayedGraphic, item.Hue, item.X, item.Y)
			{
				base.Width = w;
				base.Height = h;
				base.WantUpdateSize = false;
				_rect = ArtLoader.Instance.GetRealArtBounds(item.DisplayedGraphic);
				_originalSize.X = base.Width;
				_originalSize.Y = base.Height;
				if (_rect.Width < base.Width)
				{
					_originalSize.X = _rect.Width;
					_point.X = (base.Width >> 1) - (_originalSize.X >> 1);
				}
				if (_rect.Height < base.Height)
				{
					_originalSize.Y = _rect.Height;
					_point.Y = (base.Height >> 1) - (_originalSize.Y >> 1);
				}
			}

			public override bool Draw(UltimaBatcher2D batcher, int x, int y)
			{
				Item item = World.Items.Get(base.LocalSerial);
				if (item == null)
				{
					Dispose();
				}
				if (base.IsDisposed)
				{
					return false;
				}
				Vector3 hueVector = ShaderHueTranslator.GetHueVector((base.MouseIsOver && base.HighlightOnMouseOver) ? 53 : item.Hue, item.ItemData.IsPartialHue, 1f, gump: true);
				Rectangle bounds;
				Texture2D staticTexture = ArtLoader.Instance.GetStaticTexture(item.DisplayedGraphic, out bounds);
				if (staticTexture != null)
				{
					batcher.Draw(staticTexture, new Rectangle(x + _point.X, y + _point.Y, _originalSize.X, _originalSize.Y), new Rectangle(bounds.X + _rect.X, bounds.Y + _rect.Y, _rect.Width, _rect.Height), hueVector);
					return true;
				}
				return false;
			}

			public override bool Contains(int x, int y)
			{
				return true;
			}
		}

		private ItemGumpFixed _itemGump;

		private readonly PaperDollGump _paperDollGump;

		public Layer Layer { get; }

		public EquipmentSlot(uint serial, int x, int y, Layer layer, PaperDollGump paperDollGump)
		{
			base.X = x;
			base.Y = y;
			base.LocalSerial = serial;
			base.Width = 19;
			base.Height = 20;
			_paperDollGump = paperDollGump;
			Layer = layer;
			Add(new GumpPicTiled(0, 0, 19, 20, 9274)
			{
				AcceptMouseInput = false
			});
			Add(new GumpPic(0, 0, 9028, 0)
			{
				AcceptMouseInput = false
			});
			AcceptMouseInput = true;
			base.WantUpdateSize = false;
		}

		public override void Update(double totalTime, double frameTime)
		{
			Item item = World.Items.Get(base.LocalSerial);
			if (item == null || item.IsDestroyed)
			{
				_itemGump?.Dispose();
				_itemGump = null;
			}
			Mobile mobile = World.Mobiles.Get(_paperDollGump.LocalSerial);
			if (mobile != null)
			{
				Item item2 = mobile.FindItemByLayer(Layer);
				if (item != item2 || _itemGump == null)
				{
					if (_itemGump != null)
					{
						_itemGump.Dispose();
						_itemGump = null;
					}
					item = item2;
					if (item != null)
					{
						base.LocalSerial = item2.Serial;
						ItemGumpFixed itemGumpFixed = new ItemGumpFixed(item, 18, 18);
						itemGumpFixed.X = 0;
						itemGumpFixed.Y = 0;
						itemGumpFixed.Width = 18;
						itemGumpFixed.Height = 18;
						itemGumpFixed.HighlightOnMouseOver = false;
						itemGumpFixed.CanPickUp = World.InGame && (World.Player.Serial == _paperDollGump.LocalSerial || _paperDollGump.CanLift);
						ItemGumpFixed c = itemGumpFixed;
						_itemGump = itemGumpFixed;
						Add(c);
					}
				}
			}
			base.Update(totalTime, frameTime);
		}
	}

	public static Dictionary<int, PaperdollGumpPositioning> paperDollGumps = new Dictionary<int, PaperdollGumpPositioning>
	{
		{
			0,
			new PaperdollGumpPositioning(2000, 30, 262)
		},
		{
			1,
			new PaperdollGumpPositioning(5612, 30, 262)
		},
		{
			2,
			new PaperdollGumpPositioning(5613, 30, 262)
		},
		{
			3,
			new PaperdollGumpPositioning(5614, 30, 262)
		},
		{
			4,
			new PaperdollGumpPositioning(5615, 30, 262)
		},
		{
			5,
			new PaperdollGumpPositioning(5616, 30, 262)
		},
		{
			6,
			new PaperdollGumpPositioning(5617, 30, 262)
		},
		{
			7,
			new PaperdollGumpPositioning(5618, 30, 262)
		},
		{
			8,
			new PaperdollGumpPositioning(5619, 30, 262)
		},
		{
			9,
			new PaperdollGumpPositioning(5620, 30, 262)
		}
	};

	private static readonly ushort[] PeaceModeBtnGumps = new ushort[3] { 2021, 2022, 2023 };

	private static readonly ushort[] WarModeBtnGumps = new ushort[3] { 2024, 2025, 2026 };

	private GumpPic _combatBook;

	private GumpPic _anziehenButton;

	private HitBox _hitBox;

	private bool _isWarMode;

	private bool _isMinimized;

	private PaperDollInteractable _paperDollInteractable;

	private GumpPic _partyManifestPic;

	private GumpPic _picBase;

	private GumpPic _profilePic;

	private readonly EquipmentSlot[] _slots = new EquipmentSlot[6];

	private Label _titleLabel;

	private GumpPic _virtueMenuPic;

	private Button _warModeBtn;

	public override GumpType GumpType => GumpType.PaperDoll;

	public bool IsMinimized
	{
		get
		{
			return _isMinimized;
		}
		set
		{
			if (_isMinimized == value)
			{
				return;
			}
			_isMinimized = value;
			_picBase.Graphic = (ushort)(value ? 2030 : ((ushort)(2000u + ((base.LocalSerial != (uint)World.Player) ? 1u : 0u))));
			foreach (Control child in base.Children)
			{
				child.IsVisible = !value;
			}
			_picBase.IsVisible = true;
			base.WantUpdateSize = true;
		}
	}

	public bool CanLift { get; set; }

	public int PaperdollGump { get; set; }

	public PaperDollGump()
		: base(0u, 0u)
	{
		CanMove = true;
		base.CanCloseWithRightClick = true;
	}

	public PaperDollGump(uint serial, bool canLift, int paperdollGump)
		: this()
	{
		base.LocalSerial = serial;
		CanLift = canLift;
		PaperdollGump = paperdollGump;
		BuildGump();
	}

	public override void Dispose()
	{
		UIManager.SavePosition(base.LocalSerial, base.Location);
		if (base.LocalSerial == (uint)World.Player)
		{
			if (_virtueMenuPic != null)
			{
				_virtueMenuPic.MouseDoubleClick -= VirtueMenu_MouseDoubleClickEvent;
			}
			if (_partyManifestPic != null)
			{
				_partyManifestPic.MouseDoubleClick -= PartyManifest_MouseDoubleClickEvent;
			}
		}
		Clear();
		base.Dispose();
	}

	private void _hitBox_MouseUp(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtonType.Left && !IsMinimized)
		{
			IsMinimized = true;
		}
	}

	private void BuildGump()
	{
		_picBase?.Dispose();
		_hitBox?.Dispose();
		if (base.LocalSerial == (uint)World.Player)
		{
			Add(_picBase = new GumpPic(0, 0, paperDollGumps[ProfileManager.CurrentProfile.PaperdollGump].GumpID, 0));
			_picBase.MouseDoubleClick += _picBase_MouseDoubleClick;
			Button button = new Button(0, 2031, 2032, 2033, "", 0);
			button.X = 185;
			button.Y = 44;
			button.ButtonAction = ButtonAction.Activate;
			Add(button);
			Button button2 = new Button(1, 2006, 2007, 2008, "", 0);
			button2.X = 185;
			button2.Y = 71;
			button2.ButtonAction = ButtonAction.Activate;
			Add(button2);
			Button button3 = new Button(2, 2009, 2010, 2011, "", 0);
			button3.X = 185;
			button3.Y = 98;
			button3.ButtonAction = ButtonAction.Activate;
			Add(button3);
			Button button4 = new Button(3, 2012, 2013, 2014, "", 0);
			button4.X = 185;
			button4.Y = 125;
			button4.ButtonAction = ButtonAction.Activate;
			Add(button4);
			Button button5 = new Button(4, 2015, 2016, 2017, "", 0);
			button5.X = 185;
			button5.Y = 152;
			button5.ButtonAction = ButtonAction.Activate;
			Add(button5);
			Button button6 = new Button(5, 2034, 2035, 2036, "", 0);
			button6.X = 185;
			button6.Y = 179;
			button6.ButtonAction = ButtonAction.Activate;
			Add(button6);
			_isWarMode = World.Mobiles.Get(base.LocalSerial)?.InWarMode ?? false;
			ushort[] array = (_isWarMode ? WarModeBtnGumps : PeaceModeBtnGumps);
			Button button7 = new Button(6, array[0], array[1], array[2], "", 0);
			button7.X = 185;
			button7.Y = 206;
			button7.ButtonAction = ButtonAction.Activate;
			Button c = button7;
			_warModeBtn = button7;
			Add(c);
			int num = 25;
			_ = World.ClientFeatures.PaperdollBooks;
			if (Client.Version >= ClientVersion.CV_7000)
			{
				Add(_anziehenButton = new GumpPic(4, 202, 11048, 0));
				_anziehenButton.MouseDoubleClick += delegate
				{
					GameActions.Say(".anziehen", ushort.MaxValue, MessageType.Regular, 3);
				};
			}
			Add(_profilePic = new GumpPic(num, 196, 2002, 0));
			_profilePic.MouseDoubleClick += Profile_MouseDoubleClickEvent;
			num += 14;
			Add(_partyManifestPic = new GumpPic(num, 196, 2002, 0));
			_partyManifestPic.MouseDoubleClick += PartyManifest_MouseDoubleClickEvent;
			_hitBox = new HitBox(228, 260, 16, 16);
			_hitBox.MouseUp += _hitBox_MouseUp;
			Add(_hitBox);
		}
		else
		{
			if (paperDollGumps[PaperdollGump].GumpID == 2000)
			{
				Add(_picBase = new GumpPic(0, 0, 2001, 0));
			}
			else
			{
				Add(_picBase = new GumpPic(0, 0, paperDollGumps[PaperdollGump].GumpID, 0));
			}
			Add(_profilePic = new GumpPic(25, 196, 2002, 0));
			_profilePic.MouseDoubleClick += Profile_MouseDoubleClickEvent;
		}
		Button button8 = new Button(7, 2027, 2028, 2029, "", 0);
		button8.X = 185;
		button8.Y = 233;
		button8.ButtonAction = ButtonAction.Activate;
		Add(button8);
		if (base.LocalSerial == (uint)World.Player)
		{
			Add(_virtueMenuPic = new GumpPic(80, 4, 113, 0));
			_virtueMenuPic.MouseDoubleClick += VirtueMenu_MouseDoubleClickEvent;
			Add(_slots[0] = new EquipmentSlot(0u, 2, 75, Layer.Helmet, this));
			Add(_slots[1] = new EquipmentSlot(0u, 2, 96, Layer.Earrings, this));
			Add(_slots[2] = new EquipmentSlot(0u, 2, 117, Layer.Necklace, this));
			Add(_slots[3] = new EquipmentSlot(0u, 2, 138, Layer.Ring, this));
			Add(_slots[4] = new EquipmentSlot(0u, 2, 159, Layer.Bracelet, this));
			Add(_slots[5] = new EquipmentSlot(0u, 2, 180, Layer.Tunic, this));
		}
		_paperDollInteractable = new PaperDollInteractable(8, 19, base.LocalSerial, this);
		Add(_paperDollInteractable);
		Label label = new Label("", isunicode: true, 450, 200, 1, FontStyle.BlackBorder);
		label.X = 30;
		label.Y = 262;
		_titleLabel = label;
		Add(_titleLabel);
		RequestUpdateContents();
	}

	private void _picBase_MouseDoubleClick(object sender, MouseDoubleClickEventArgs e)
	{
		if (e.Button == MouseButtonType.Left && IsMinimized)
		{
			IsMinimized = false;
		}
	}

	public void UpdateTitle(string text)
	{
		_titleLabel.Text = text;
	}

	private void VirtueMenu_MouseDoubleClickEvent(object sender, MouseDoubleClickEventArgs args)
	{
		if (args.Button == MouseButtonType.Left)
		{
			GameActions.ReplyGump(World.Player, 461u, 1, new uint[1] { base.LocalSerial }, new Tuple<ushort, string>[0]);
		}
	}

	private void Profile_MouseDoubleClickEvent(object o, MouseDoubleClickEventArgs args)
	{
		if (args.Button == MouseButtonType.Left)
		{
			GameActions.RequestProfile(base.LocalSerial);
		}
	}

	private void PartyManifest_MouseDoubleClickEvent(object sender, MouseDoubleClickEventArgs args)
	{
		if (args.Button != MouseButtonType.Left)
		{
			return;
		}
		if (CUOEnviroment.IsOutlands)
		{
			NetClient.Socket.Send_ASCIISpeechRequest("party", MessageType.Command, 0, 0);
			return;
		}
		PartyGump gump = UIManager.GetGump<PartyGump>(null);
		if (gump == null)
		{
			int x = Client.Game.Window.ClientBounds.Width / 2 - 272;
			int y = Client.Game.Window.ClientBounds.Height / 2 - 240;
			UIManager.Add(new PartyGump(x, y, World.Party.CanLoot));
		}
		else
		{
			gump.BringOnTop();
		}
	}

	public override void Update(double totalTime, double frameTime)
	{
		if (base.IsDisposed)
		{
			return;
		}
		Mobile mobile = World.Mobiles.Get(base.LocalSerial);
		if (mobile != null && mobile.IsDestroyed)
		{
			Dispose();
			return;
		}
		if (mobile != null && _isWarMode != mobile.InWarMode && base.LocalSerial == (uint)World.Player)
		{
			_isWarMode = mobile.InWarMode;
			ushort[] array = (_isWarMode ? WarModeBtnGumps : PeaceModeBtnGumps);
			_warModeBtn.ButtonGraphicNormal = array[0];
			_warModeBtn.ButtonGraphicPressed = array[1];
			_warModeBtn.ButtonGraphicOver = array[2];
		}
		base.Update(totalTime, frameTime);
		if (_paperDollInteractable != null && (CanLift || base.LocalSerial == World.Player.Serial))
		{
			bool flag = SelectedObject.Object is Item item && (item.Layer == Layer.Backpack || item.ItemData.IsContainer);
			if ((_paperDollInteractable.HasFakeItem && !ItemHold.Enabled) || flag)
			{
				_paperDollInteractable.SetFakeItem(value: false);
			}
			else if (!_paperDollInteractable.HasFakeItem && ItemHold.Enabled && !ItemHold.IsFixedPosition && UIManager.MouseOverControl?.RootParent == this && ItemHold.ItemData.AnimID != 0 && mobile != null && mobile.FindItemByLayer((Layer)ItemHold.ItemData.Layer) == null)
			{
				_paperDollInteractable.SetFakeItem(value: true);
			}
		}
	}

	protected override void OnMouseExit(int x, int y)
	{
		_paperDollInteractable?.SetFakeItem(value: false);
	}

	protected override void OnMouseUp(int x, int y, MouseButtonType button)
	{
		if (button == MouseButtonType.Left && World.InGame)
		{
			Mobile mobile = World.Mobiles.Get(base.LocalSerial);
			if (ItemHold.Enabled)
			{
				if (CanLift || base.LocalSerial == World.Player.Serial)
				{
					if (SelectedObject.Object is Item item && (item.Layer == Layer.Backpack || item.ItemData.IsContainer))
					{
						GameActions.DropItem(ItemHold.Serial, 65535, 65535, 0, item.Serial);
						Mouse.CancelDoubleClick = true;
					}
					else if (ItemHold.ItemData.IsWearable && mobile.FindItemByLayer((Layer)ItemHold.ItemData.Layer) == null)
					{
						GameActions.Equip((base.LocalSerial != (uint)World.Player) ? mobile : World.Player);
						Mouse.CancelDoubleClick = true;
					}
				}
			}
			else
			{
				if (!(SelectedObject.Object is Item item2))
				{
					return;
				}
				if (TargetManager.IsTargeting)
				{
					TargetManager.Target(item2.Serial);
					Mouse.CancelDoubleClick = true;
					Mouse.LastLeftButtonClickTime = 0u;
					if (TargetManager.TargetingState == CursorTarget.SetTargetClientSide)
					{
						UIManager.Add(new InspectorGump(item2));
					}
				}
				else if (!DelayedObjectClickManager.IsEnabled)
				{
					Point lDragOffset = Mouse.LDragOffset;
					DelayedObjectClickManager.Set(item2.Serial, Mouse.Position.X - lDragOffset.X - base.ScreenCoordinateX, Mouse.Position.Y - lDragOffset.Y - base.ScreenCoordinateY, Time.Ticks + 350);
				}
			}
		}
		else
		{
			base.OnMouseUp(x, y, button);
		}
	}

	public override void Save(XmlTextWriter writer)
	{
		base.Save(writer);
		writer.WriteAttributeString("isminimized", IsMinimized.ToString());
	}

	public override void Restore(XmlElement xml)
	{
		base.Restore(xml);
		if (base.LocalSerial == (uint)World.Player)
		{
			BuildGump();
			Client.Game.GetScene<GameScene>()?.DoubleClickDelayed(base.LocalSerial);
			IsMinimized = bool.Parse(xml.GetAttribute("isminimized"));
		}
		else
		{
			Dispose();
		}
	}

	protected override void UpdateContents()
	{
		Mobile mobile = World.Mobiles.Get(base.LocalSerial);
		if (mobile != null && mobile.Title != _titleLabel.Text)
		{
			UpdateTitle(mobile.Title);
		}
		_paperDollInteractable.Update();
		if (mobile != null && mobile == World.Player)
		{
			for (int i = 0; i < _slots.Length; i++)
			{
				int layer = (int)_slots[i].Layer;
				_slots[i].LocalSerial = mobile.FindItemByLayer((Layer)layer)?.Serial ?? 0;
			}
		}
	}

	public override void OnButtonClick(int buttonID)
	{
		if (ItemHold.Enabled && !ItemHold.IsFixedPosition)
		{
			OnMouseUp(0, 0, MouseButtonType.Left);
			return;
		}
		switch ((Buttons)buttonID)
		{
		case Buttons.Help:
			GameActions.RequestHelp();
			break;
		case Buttons.Options:
			GameActions.OpenSettings();
			break;
		case Buttons.LogOut:
			Client.Game.GetScene<GameScene>()?.RequestQuitGame();
			break;
		case Buttons.Journal:
			GameActions.OpenJournal();
			break;
		case Buttons.Skills:
			GameActions.OpenSkills();
			break;
		case Buttons.ActionBook:
			GameActions.OpenActionBook();
			break;
		case Buttons.PeaceWarToggle:
			GameActions.ToggleWarMode();
			break;
		case Buttons.Status:
			if (base.LocalSerial == (uint)World.Player)
			{
				StatusGumpBase statusGump = StatusGumpBase.GetStatusGump();
				if (statusGump == null)
				{
					UIManager.Add(StatusGumpBase.AddStatusGump(Mouse.Position.X - 100, Mouse.Position.Y - 25));
				}
				else
				{
					statusGump.BringOnTop();
				}
			}
			else if (UIManager.GetGump<BaseHealthBarGump>(base.LocalSerial) == null)
			{
				if (ProfileManager.CurrentProfile.CustomBarsToggled)
				{
					Rectangle rectangle = new Rectangle(0, 0, 120, 36);
					HealthBarGumpCustom healthBarGumpCustom = new HealthBarGumpCustom(base.LocalSerial);
					healthBarGumpCustom.X = Mouse.Position.X - (rectangle.Width >> 1);
					healthBarGumpCustom.Y = Mouse.Position.Y - 5;
					UIManager.Add(healthBarGumpCustom);
				}
				else
				{
					GumpsLoader.Instance.GetGumpTexture(2052u, out var bounds);
					HealthBarGump healthBarGump = new HealthBarGump(base.LocalSerial);
					healthBarGump.X = Mouse.Position.X - (bounds.Width >> 1);
					healthBarGump.Y = Mouse.Position.Y - 5;
					UIManager.Add(healthBarGump);
				}
			}
			break;
		}
	}
}

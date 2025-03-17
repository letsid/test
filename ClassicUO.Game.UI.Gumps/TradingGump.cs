using System;
using ClassicUO.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Network;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Gumps;

internal sealed class TradingGump : TextContainerGump
{
	private uint _gold;

	private uint _platinum;

	private uint _hisGold;

	private uint _hisPlatinum;

	private readonly Label[] _hisCoins = new Label[2];

	private GumpPic _hisPic;

	private bool _imAccepting;

	private bool _heIsAccepting;

	private DataBox _myBox;

	private DataBox _hisBox;

	private Checkbox _myCheckbox;

	private readonly Label[] _myCoins = new Label[2];

	private readonly StbTextBox[] _myCoinsEntries = new StbTextBox[2];

	private readonly string _name;

	public uint ID1 { get; }

	public uint ID2 { get; }

	public uint Gold
	{
		get
		{
			return _gold;
		}
		set
		{
			if (_gold != value)
			{
				_gold = value;
				if (Client.Version >= ClientVersion.CV_704565)
				{
					_myCoins[0].Text = _gold.ToString();
				}
			}
		}
	}

	public uint Platinum
	{
		get
		{
			return _platinum;
		}
		set
		{
			if (_platinum != value)
			{
				_platinum = value;
				if (Client.Version >= ClientVersion.CV_704565)
				{
					_myCoins[1].Text = _platinum.ToString();
				}
			}
		}
	}

	public uint HisGold
	{
		get
		{
			return _hisGold;
		}
		set
		{
			if (_hisGold != value)
			{
				_hisGold = value;
				if (Client.Version >= ClientVersion.CV_704565)
				{
					_hisCoins[0].Text = _hisGold.ToString();
				}
			}
		}
	}

	public uint HisPlatinum
	{
		get
		{
			return _hisPlatinum;
		}
		set
		{
			if (_hisPlatinum != value)
			{
				_hisPlatinum = value;
				if (Client.Version >= ClientVersion.CV_704565)
				{
					_hisCoins[1].Text = _hisPlatinum.ToString();
				}
			}
		}
	}

	public bool ImAccepting
	{
		get
		{
			return _imAccepting;
		}
		set
		{
			if (_imAccepting != value)
			{
				_imAccepting = value;
				SetCheckboxes();
			}
		}
	}

	public bool HeIsAccepting
	{
		get
		{
			return _heIsAccepting;
		}
		set
		{
			if (_heIsAccepting != value)
			{
				_heIsAccepting = value;
				SetCheckboxes();
			}
		}
	}

	public TradingGump(uint local, string name, uint id1, uint id2)
		: base(local, 0u)
	{
		CanMove = true;
		base.CanCloseWithRightClick = true;
		AcceptMouseInput = true;
		_name = name;
		ID1 = id1;
		ID2 = id2;
		BuildGump();
	}

	protected override void UpdateContents()
	{
		Entity entity = World.Get(ID1);
		if (entity == null)
		{
			return;
		}
		foreach (Control child in _myBox.Children)
		{
			child.Dispose();
		}
		ArtLoader instance = ArtLoader.Instance;
		for (LinkedObject linkedObject = entity.Items; linkedObject != null; linkedObject = linkedObject.Next)
		{
			Item item = (Item)linkedObject;
			ItemGump itemGump = new ItemGump(item.Serial, item.DisplayedGraphic, item.Hue, item.X, item.Y)
			{
				HighlightOnMouseOver = true
			};
			int num = itemGump.X;
			int num2 = itemGump.Y;
			if (instance.GetStaticTexture(item.DisplayedGraphic, out var bounds) != null)
			{
				if (num + bounds.Width > _myBox.Width)
				{
					num = _myBox.Width - bounds.Width;
				}
				if (num2 + bounds.Height > _myBox.Height)
				{
					num2 = _myBox.Height - bounds.Height;
				}
			}
			if (num < 0)
			{
				num = 0;
			}
			if (num2 < 0)
			{
				num2 = 0;
			}
			itemGump.X = num;
			itemGump.Y = num2;
			_myBox.Add(itemGump);
		}
		entity = World.Get(ID2);
		if (entity == null)
		{
			return;
		}
		foreach (Control child2 in _hisBox.Children)
		{
			child2.Dispose();
		}
		for (LinkedObject linkedObject2 = entity.Items; linkedObject2 != null; linkedObject2 = linkedObject2.Next)
		{
			Item item2 = (Item)linkedObject2;
			ItemGump itemGump2 = new ItemGump(item2.Serial, item2.DisplayedGraphic, item2.Hue, item2.X, item2.Y)
			{
				HighlightOnMouseOver = true
			};
			int num3 = itemGump2.X;
			int num4 = itemGump2.Y;
			if (instance.GetStaticTexture(item2.DisplayedGraphic, out var bounds2) != null)
			{
				if (num3 + bounds2.Width > _myBox.Width)
				{
					num3 = _myBox.Width - bounds2.Width;
				}
				if (num4 + bounds2.Height > _myBox.Height)
				{
					num4 = _myBox.Height - bounds2.Height;
				}
			}
			if (num3 < 0)
			{
				num3 = 0;
			}
			if (num4 < 0)
			{
				num4 = 0;
			}
			itemGump2.X = num3;
			itemGump2.Y = num4;
			_hisBox.Add(itemGump2);
		}
	}

	protected override void OnMouseUp(int x, int y, MouseButtonType button)
	{
		if (button != MouseButtonType.Left)
		{
			return;
		}
		if (ItemHold.Enabled && !ItemHold.IsFixedPosition)
		{
			if (_myBox == null || !_myBox.Bounds.Contains(x, y))
			{
				return;
			}
			Rectangle bounds;
			Texture2D staticTexture = ArtLoader.Instance.GetStaticTexture(ItemHold.DisplayedGraphic, out bounds);
			x -= _myBox.X;
			y -= _myBox.Y;
			if (staticTexture != null)
			{
				x -= bounds.Width >> 1;
				y -= bounds.Height >> 1;
				if (x + bounds.Width > _myBox.Width)
				{
					x = _myBox.Width - bounds.Width;
				}
				if (y + bounds.Height > _myBox.Height)
				{
					y = _myBox.Height - bounds.Height;
				}
			}
			if (x < 0)
			{
				x = 0;
			}
			if (y < 0)
			{
				y = 0;
			}
			GameActions.DropItem(ItemHold.Serial, x, y, 0, ID1);
		}
		else
		{
			if (!(SelectedObject.Object is Item item))
			{
				return;
			}
			if (TargetManager.IsTargeting)
			{
				TargetManager.Target(item.Serial);
				Mouse.CancelDoubleClick = true;
				if (TargetManager.TargetingState == CursorTarget.SetTargetClientSide)
				{
					UIManager.Add(new InspectorGump(item));
				}
			}
			else if (!DelayedObjectClickManager.IsEnabled)
			{
				Point lDragOffset = Mouse.LDragOffset;
				DelayedObjectClickManager.Set(item.Serial, Mouse.Position.X - lDragOffset.X - base.ScreenCoordinateX, Mouse.Position.Y - lDragOffset.Y - base.ScreenCoordinateY, Time.Ticks + 350);
			}
		}
	}

	public override void Dispose()
	{
		base.Dispose();
		GameActions.CancelTrade(ID1);
	}

	private void SetCheckboxes()
	{
		_myCheckbox?.Dispose();
		_hisPic?.Dispose();
		int num;
		int num2;
		int x;
		int y;
		if (Client.Version >= ClientVersion.CV_704565)
		{
			num = 37;
			num2 = 29;
			x = 258;
			y = 240;
		}
		else
		{
			num = 52;
			num2 = 29;
			x = 266;
			y = 160;
		}
		if (ImAccepting)
		{
			Checkbox checkbox = new Checkbox(2153, 2154, "", 0, 0);
			checkbox.X = num;
			checkbox.Y = num2;
			_myCheckbox = checkbox;
		}
		else
		{
			Checkbox checkbox2 = new Checkbox(2151, 2152, "", 0, 0);
			checkbox2.X = num;
			checkbox2.Y = num2;
			_myCheckbox = checkbox2;
		}
		_myCheckbox.ValueChanged -= MyCheckboxOnValueChanged;
		_myCheckbox.ValueChanged += MyCheckboxOnValueChanged;
		Add(_myCheckbox);
		_hisPic = (HeIsAccepting ? new GumpPic(x, y, 2153, 0) : new GumpPic(x, y, 2151, 0));
		Add(_hisPic);
	}

	private void BuildGump()
	{
		uint my_gold_entry;
		uint my_plat_entry;
		int x;
		int y;
		int x2;
		int y2;
		if (Client.Version >= ClientVersion.CV_704565)
		{
			Add(new GumpPic(0, 0, 2186, 0));
			Label label = new Label(World.Player.Name, isunicode: false, 1153, 0, 3);
			label.X = 73;
			label.Y = 32;
			Add(label);
			int num = 250 - FontsLoader.Instance.GetWidthASCII(3, _name);
			Label label2 = new Label(_name, isunicode: false, 1153, 0, 3);
			label2.X = num;
			label2.Y = 244;
			Add(label2);
			Label[] myCoins = _myCoins;
			Label label3 = new Label("0", isunicode: false, 1153, 0, 9);
			label3.X = 43;
			label3.Y = 67;
			myCoins[0] = label3;
			Add(_myCoins[0]);
			Label[] myCoins2 = _myCoins;
			Label label4 = new Label("0", isunicode: false, 1153, 0, 9);
			label4.X = 180;
			label4.Y = 67;
			myCoins2[1] = label4;
			Add(_myCoins[1]);
			Label[] hisCoins = _hisCoins;
			Label label5 = new Label("0", isunicode: false, 1153, 0, 9);
			label5.X = 180;
			label5.Y = 190;
			hisCoins[0] = label5;
			Add(_hisCoins[0]);
			Label[] hisCoins2 = _hisCoins;
			Label label6 = new Label("0", isunicode: false, 1153, 0, 9);
			label6.X = 180;
			label6.Y = 210;
			hisCoins2[1] = label6;
			Add(_hisCoins[1]);
			StbTextBox[] myCoinsEntries = _myCoinsEntries;
			StbTextBox stbTextBox = new StbTextBox(9, -1, 100, isunicode: false, FontStyle.None, 0);
			stbTextBox.X = 43;
			stbTextBox.Y = 190;
			stbTextBox.Width = 100;
			stbTextBox.Height = 20;
			stbTextBox.NumbersOnly = true;
			stbTextBox.Tag = 0;
			myCoinsEntries[0] = stbTextBox;
			_myCoinsEntries[0].SetText("0");
			Add(_myCoinsEntries[0]);
			StbTextBox[] myCoinsEntries2 = _myCoinsEntries;
			StbTextBox stbTextBox2 = new StbTextBox(9, -1, 100, isunicode: false, FontStyle.None, 0);
			stbTextBox2.X = 43;
			stbTextBox2.Y = 210;
			stbTextBox2.Width = 100;
			stbTextBox2.Height = 20;
			stbTextBox2.NumbersOnly = true;
			stbTextBox2.Tag = 1;
			myCoinsEntries2[1] = stbTextBox2;
			_myCoinsEntries[1].SetText("0");
			Add(_myCoinsEntries[1]);
			my_gold_entry = 0u;
			my_plat_entry = 0u;
			_myCoinsEntries[0].TextChanged += OnTextChanged;
			_myCoinsEntries[1].TextChanged += OnTextChanged;
			x = 30;
			y = 110;
			x2 = 192;
			y2 = 110;
		}
		else
		{
			Add(new GumpPic(0, 0, 2150, 0));
			Label label7 = new Label(World.Player.Name, isunicode: false, 902, 0, 1);
			label7.X = 84;
			label7.Y = 40;
			Add(label7);
			int num2 = 260 - FontsLoader.Instance.GetWidthASCII(1, _name);
			Label label8 = new Label(_name, isunicode: false, 902, 0, 1);
			label8.X = num2;
			label8.Y = 170;
			Add(label8);
			x = 45;
			y = 70;
			x2 = 192;
			y2 = 70;
		}
		if (Client.Version < ClientVersion.CV_500A)
		{
			ColorBox colorBox = new ColorBox(110, 60, 0);
			colorBox.X = 45;
			colorBox.Y = 90;
			Add(colorBox);
			ColorBox colorBox2 = new ColorBox(110, 60, 0);
			colorBox2.X = 192;
			colorBox2.Y = 70;
			Add(colorBox2);
		}
		Add(_myBox = new DataBox(x, y, 110, 80)
		{
			WantUpdateSize = false,
			ContainsByBounds = true,
			AcceptMouseInput = true,
			CanMove = true
		});
		Add(_hisBox = new DataBox(x2, y2, 110, 80)
		{
			WantUpdateSize = false,
			ContainsByBounds = true,
			AcceptMouseInput = true,
			CanMove = true
		});
		SetCheckboxes();
		RequestUpdateContents();
		void OnTextChanged(object sender, EventArgs e)
		{
			StbTextBox stbTextBox3 = (StbTextBox)sender;
			bool flag = false;
			if (stbTextBox3 != null)
			{
				uint result;
				if (string.IsNullOrEmpty(stbTextBox3.Text))
				{
					stbTextBox3.SetText("0");
					if ((int)stbTextBox3.Tag == 0)
					{
						if (my_gold_entry != 0)
						{
							my_gold_entry = 0u;
							flag = true;
						}
					}
					else if (my_plat_entry != 0)
					{
						my_plat_entry = 0u;
						flag = true;
					}
				}
				else if (uint.TryParse(stbTextBox3.Text, out result))
				{
					if ((int)stbTextBox3.Tag == 0)
					{
						if (result > Gold)
						{
							result = Gold;
							flag = true;
						}
						if (my_gold_entry != result)
						{
							flag = true;
						}
						my_gold_entry = result;
					}
					else
					{
						if (result > Platinum)
						{
							result = Platinum;
							flag = true;
						}
						if (my_plat_entry != result)
						{
							flag = true;
						}
						my_plat_entry = result;
					}
					if (flag)
					{
						stbTextBox3.SetText(result.ToString());
					}
				}
				if (flag)
				{
					NetClient.Socket.Send_TradeUpdateGold(ID1, my_gold_entry, my_plat_entry);
				}
			}
		}
	}

	private void MyCheckboxOnValueChanged(object sender, EventArgs e)
	{
		ImAccepting = !ImAccepting;
		GameActions.AcceptTrade(ID1, ImAccepting);
	}
}

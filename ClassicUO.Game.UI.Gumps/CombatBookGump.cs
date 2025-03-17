using System;
using System.Collections.Generic;
using ClassicUO.Data;
using ClassicUO.Game.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Resources;
using ClassicUO.Utility;

namespace ClassicUO.Game.UI.Gumps;

internal class CombatBookGump : Gump
{
	private readonly int _abilityCount = 32;

	private float _clickTiming;

	private int _dictionaryPagesCount = 3;

	private Control _lastPressed;

	private GumpPic _pageCornerLeft;

	private GumpPic _pageCornerRight;

	private GumpPic _primAbility;

	private GumpPic _secAbility;

	public CombatBookGump(int x, int y)
		: base(0u, 0u)
	{
		base.X = x;
		base.Y = y;
		CanMove = true;
		base.CanCloseWithRightClick = true;
		if (Client.Version < ClientVersion.CV_7000)
		{
			if (Client.Version < ClientVersion.CV_500A)
			{
				_abilityCount = 29;
			}
			else
			{
				_abilityCount = 13;
				_dictionaryPagesCount = 1;
			}
		}
		BuildGump();
	}

	private void BuildGump()
	{
		Add(new GumpPic(0, 0, 11010, 0));
		Add(_pageCornerLeft = new GumpPic(50, 8, 2235, 0));
		_pageCornerLeft.LocalSerial = 0u;
		_pageCornerLeft.Page = int.MaxValue;
		_pageCornerLeft.MouseUp += PageCornerOnMouseClick;
		_pageCornerLeft.MouseDoubleClick += PageCornerOnMouseDoubleClick;
		Add(_pageCornerRight = new GumpPic(321, 8, 2236, 0));
		_pageCornerRight.LocalSerial = 1u;
		_pageCornerRight.Page = 1;
		_pageCornerRight.MouseUp += PageCornerOnMouseClick;
		_pageCornerRight.MouseDoubleClick += PageCornerOnMouseDoubleClick;
		int num = 0;
		int num2 = _dictionaryPagesCount + 1;
		for (int i = 1; i <= _dictionaryPagesCount; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				int num3 = 96;
				int num4 = 52;
				int num5 = 0;
				int num6 = 9;
				if (j % 2 != 0)
				{
					num3 = 259;
					num4 = 215;
					num6 = 4;
				}
				Label label = new Label(ResGumps.Index, isunicode: false, 648, 0, 6);
				label.X = num3;
				label.Y = 6;
				Label c = label;
				Add(c, i);
				for (int k = 0; k < num6; k++)
				{
					if (num >= _abilityCount)
					{
						break;
					}
					HoveredLabel hoveredLabel = new HoveredLabel(AbilityData.Abilities[num].Name, isunicode: false, 648, 51, 648, 0, 9);
					hoveredLabel.X = num4;
					hoveredLabel.Y = 42 + num5;
					hoveredLabel.AcceptMouseInput = true;
					hoveredLabel.LocalSerial = (uint)num2++;
					hoveredLabel.Tag = num;
					c = hoveredLabel;
					c.MouseUp += delegate(object? s, MouseEventArgs e)
					{
						if (s is HoveredLabel lastPressed && e.Button == MouseButtonType.Left)
						{
							_clickTiming += 350f;
							if (_clickTiming > 0f)
							{
								_lastPressed = lastPressed;
							}
						}
					};
					Add(c, i);
					c.SetTooltip(ClilocLoader.Instance.GetString(1061693 + num), 150);
					num5 += 15;
					num++;
				}
				if (num6 == 4)
				{
					byte b = (byte)(((byte)World.Player.PrimaryAbility & 0x7F) - 1);
					_primAbility = new GumpPic(215, 105, (ushort)(20992 + b), 0);
					Label label2 = new Label(ResGumps.PrimaryAbilityIcon, isunicode: false, 648, 80, 6);
					label2.X = 265;
					label2.Y = 105;
					c = label2;
					Add(_primAbility, i);
					Add(c, i);
					_primAbility.SetTooltip(ClilocLoader.Instance.GetString(1028838 + b));
					_primAbility.DragBegin += OnGumpicDragBeginPrimary;
					byte b2 = (byte)(((byte)World.Player.SecondaryAbility & 0x7F) - 1);
					_secAbility = new GumpPic(215, 150, (ushort)(20992 + b2), 0);
					Label label3 = new Label(ResGumps.SecondaryAbilityIcon, isunicode: false, 648, 80, 6);
					label3.X = 265;
					label3.Y = 150;
					c = label3;
					Add(_secAbility, i);
					Add(c, i);
					_secAbility.SetTooltip(ClilocLoader.Instance.GetString(1028838 + b2));
					_secAbility.DragBegin += OnGumpicDragBeginSecondary;
				}
			}
		}
		int num7 = _dictionaryPagesCount + 1;
		_dictionaryPagesCount += _abilityCount;
		int num8 = 0;
		while (num8 < _abilityCount && num8 < AbilityData.Abilities.Length)
		{
			GumpPic gumpPic = new GumpPic(62, 40, (ushort)(20992 + num8), 0);
			Add(gumpPic, num7);
			gumpPic.SetTooltip(ClilocLoader.Instance.GetString(1061693 + num8), 150);
			Label label4 = new Label(StringHelper.CapitalizeAllWords(AbilityData.Abilities[num8].Name), isunicode: false, 648, 80, 6);
			label4.X = 110;
			label4.Y = 34;
			Label c2 = label4;
			Add(c2, num7);
			GumpPicTiled gumpPicTiled = new GumpPicTiled(2101);
			gumpPicTiled.X = 62;
			gumpPicTiled.Y = 88;
			gumpPicTiled.Width = 128;
			Add(gumpPicTiled, num7);
			List<ushort> itemsList = GetItemsList((byte)num8);
			int num9 = TileDataLoader.Instance.StaticData.Length;
			int num10 = 62;
			int num11 = 98;
			for (int l = 0; l < itemsList.Count; l++)
			{
				if (l == 6)
				{
					num10 = 215;
					num11 = 34;
				}
				ushort num12 = itemsList[l];
				if (num12 < num9)
				{
					Label label5 = new Label(StringHelper.CapitalizeAllWords(TileDataLoader.Instance.StaticData[num12].Name), isunicode: false, 648, 0, 9);
					label5.X = num10;
					label5.Y = num11;
					c2 = label5;
					Add(c2, num7);
					num11 += 16;
				}
			}
			num8++;
			num7++;
		}
	}

	private void OnGumpicDragBeginPrimary(object sender, EventArgs e)
	{
		if (!UIManager.IsDragging)
		{
			GetSpellFloatingButton(AbilityData.Abilities[((byte)World.Player.PrimaryAbility & 0x7F) - 1].Index)?.Dispose();
			UseAbilityButtonGump useAbilityButtonGump = new UseAbilityButtonGump(primary: true);
			useAbilityButtonGump.X = Mouse.LClickPosition.X - 22;
			useAbilityButtonGump.Y = Mouse.LClickPosition.Y - 22;
			UIManager.Add(useAbilityButtonGump);
			UIManager.AttemptDragControl(useAbilityButtonGump, attemptAlwaysSuccessful: true);
		}
	}

	private void OnGumpicDragBeginSecondary(object sender, EventArgs e)
	{
		if (!UIManager.IsDragging)
		{
			GetSpellFloatingButton(AbilityData.Abilities[((byte)World.Player.SecondaryAbility & 0x7F) - 1].Index)?.Dispose();
			UseAbilityButtonGump useAbilityButtonGump = new UseAbilityButtonGump(primary: false);
			useAbilityButtonGump.X = Mouse.LClickPosition.X - 22;
			useAbilityButtonGump.Y = Mouse.LClickPosition.Y - 22;
			UIManager.Add(useAbilityButtonGump);
			UIManager.AttemptDragControl(useAbilityButtonGump, attemptAlwaysSuccessful: true);
		}
	}

	private static UseAbilityButtonGump GetSpellFloatingButton(int id)
	{
		for (LinkedListNode<Gump> linkedListNode = UIManager.Gumps.Last; linkedListNode != null; linkedListNode = linkedListNode.Previous)
		{
			if (linkedListNode.Value is UseAbilityButtonGump useAbilityButtonGump && useAbilityButtonGump.Index == id)
			{
				return useAbilityButtonGump;
			}
		}
		return null;
	}

	public override void Update(double totalTime, double frameTime)
	{
		base.Update(totalTime, frameTime);
		if (base.IsDisposed)
		{
			return;
		}
		for (int i = 0; i < 2; i++)
		{
			int num = ((byte)((i == 0) ? World.Player.PrimaryAbility : World.Player.SecondaryAbility) & 0x7F) - 1;
			ref AbilityDefinition reference = ref AbilityData.Abilities[num];
			if (i == 0)
			{
				if (_primAbility.Graphic != reference.Icon)
				{
					_primAbility.Graphic = reference.Icon;
				}
			}
			else if (_secAbility.Graphic != reference.Icon)
			{
				_secAbility.Graphic = reference.Icon;
			}
		}
		if (!base.IsDisposed && _lastPressed != null)
		{
			_clickTiming -= (float)frameTime;
			if (_clickTiming <= 0f)
			{
				_clickTiming = 0f;
				SetActivePage((int)_lastPressed.LocalSerial);
				_lastPressed = null;
			}
		}
	}

	private void PageCornerOnMouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtonType.Left && sender is Control control)
		{
			SetActivePage((control.LocalSerial == 0) ? (base.ActivePage - 1) : (base.ActivePage + 1));
		}
	}

	private void PageCornerOnMouseDoubleClick(object sender, MouseDoubleClickEventArgs e)
	{
		if (e.Button == MouseButtonType.Left && sender is Control control)
		{
			SetActivePage((control.LocalSerial == 0) ? 1 : _dictionaryPagesCount);
		}
	}

	private void SetActivePage(int page)
	{
		if (page < 1)
		{
			page = 1;
		}
		else if (page > _dictionaryPagesCount)
		{
			page = _dictionaryPagesCount;
		}
		base.ActivePage = page;
		_pageCornerLeft.Page = ((base.ActivePage == 1) ? int.MaxValue : 0);
		_pageCornerRight.Page = ((base.ActivePage == _dictionaryPagesCount) ? int.MaxValue : 0);
		Client.Game.Scene.Audio.PlaySound(85);
	}

	private List<ushort> GetItemsList(byte index)
	{
		List<ushort> list = new List<ushort>();
		switch (index)
		{
		case 0:
			list.Add(3908);
			list.Add(5048);
			list.Add(3935);
			list.Add(5119);
			list.Add(9927);
			list.Add(5181);
			list.Add(5040);
			list.Add(5121);
			list.Add(3939);
			list.Add(9932);
			list.Add(11554);
			list.Add(16497);
			list.Add(16502);
			list.Add(16494);
			list.Add(16491);
			break;
		case 1:
			list.Add(3779);
			list.Add(5115);
			list.Add(3912);
			list.Add(3910);
			list.Add(5185);
			list.Add(9924);
			list.Add(5127);
			list.Add(5040);
			list.Add(3720);
			list.Add(5125);
			list.Add(11552);
			list.Add(16499);
			list.Add(16498);
			break;
		case 2:
			list.Add(5048);
			list.Add(3912);
			list.Add(5183);
			list.Add(5179);
			list.Add(3933);
			list.Add(5113);
			list.Add(3722);
			list.Add(9930);
			list.Add(3920);
			list.Add(11556);
			list.Add(16487);
			list.Add(16500);
			break;
		case 3:
			list.Add(5050);
			list.Add(3914);
			list.Add(3935);
			list.Add(3714);
			list.Add(5092);
			list.Add(5179);
			list.Add(5127);
			list.Add(5177);
			list.Add(9926);
			list.Add(4021);
			list.Add(10146);
			list.Add(11556);
			list.Add(11560);
			list.Add(5109);
			list.Add(16500);
			list.Add(16495);
			break;
		case 4:
			list.Add(5111);
			list.Add(3718);
			list.Add(3781);
			list.Add(3908);
			list.Add(3573);
			list.Add(3714);
			list.Add(3933);
			list.Add(5125);
			list.Add(11558);
			list.Add(11560);
			list.Add(5109);
			list.Add(9934);
			list.Add(16493);
			list.Add(16494);
			break;
		case 5:
			list.Add(3918);
			list.Add(3914);
			list.Add(9927);
			list.Add(3573);
			list.Add(5044);
			list.Add(3720);
			list.Add(9930);
			list.Add(5117);
			list.Add(16501);
			list.Add(16495);
			break;
		case 6:
			list.Add(3718);
			list.Add(5187);
			list.Add(3916);
			list.Add(5046);
			list.Add(5119);
			list.Add(9931);
			list.Add(3722);
			list.Add(9929);
			list.Add(9933);
			list.Add(10148);
			list.Add(10153);
			list.Add(16488);
			list.Add(16493);
			list.Add(16496);
			break;
		case 7:
			list.Add(5111);
			list.Add(3779);
			list.Add(3922);
			list.Add(9928);
			list.Add(5121);
			list.Add(9929);
			list.Add(11553);
			list.Add(16490);
			list.Add(16488);
			break;
		case 8:
			list.Add(3910);
			list.Add(9925);
			list.Add(9931);
			list.Add(5181);
			list.Add(9926);
			list.Add(5123);
			list.Add(3920);
			list.Add(5042);
			list.Add(16499);
			list.Add(16502);
			list.Add(16496);
			list.Add(16491);
			break;
		case 9:
			list.Add(5117);
			list.Add(9932);
			list.Add(9933);
			list.Add(16492);
			break;
		case 10:
			list.Add(5050);
			list.Add(3918);
			list.Add(5046);
			list.Add(9924);
			list.Add(9925);
			list.Add(5113);
			list.Add(3569);
			list.Add(9928);
			list.Add(3939);
			list.Add(5042);
			list.Add(16497);
			list.Add(16498);
			break;
		case 11:
			list.Add(3781);
			list.Add(5187);
			list.Add(5185);
			list.Add(5092);
			list.Add(5044);
			list.Add(3922);
			list.Add(5123);
			list.Add(4021);
			list.Add(11553);
			list.Add(16490);
			break;
		case 12:
			list.Add(5115);
			list.Add(5183);
			list.Add(3916);
			list.Add(5177);
			list.Add(3569);
			list.Add(10157);
			list.Add(11559);
			list.Add(9934);
			list.Add(16501);
			break;
		case 13:
			list.Add(10146);
			break;
		case 14:
			list.Add(10148);
			list.Add(10150);
			list.Add(10151);
			break;
		case 15:
			list.Add(10147);
			list.Add(10158);
			list.Add(10159);
			list.Add(11557);
			break;
		case 16:
			list.Add(10151);
			list.Add(10157);
			list.Add(11561);
			break;
		case 17:
			list.Add(10152);
			break;
		case 18:
		case 20:
			list.Add(10155);
			break;
		case 19:
			list.Add(10152);
			list.Add(10153);
			list.Add(10158);
			list.Add(11554);
			break;
		case 21:
			list.Add(10149);
			break;
		case 22:
			list.Add(10149);
			list.Add(10159);
			break;
		case 23:
			list.Add(11555);
			list.Add(11558);
			list.Add(11559);
			list.Add(11561);
			break;
		case 24:
		case 27:
			list.Add(11550);
			break;
		case 25:
			list.Add(11551);
			break;
		case 26:
			list.Add(11551);
			list.Add(11552);
			break;
		case 28:
			list.Add(11557);
			break;
		case 29:
			list.Add(16492);
			break;
		case 30:
			list.Add(16487);
			break;
		}
		return list;
	}
}

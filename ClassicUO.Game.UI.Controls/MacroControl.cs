using System;
using System.Collections.Generic;
using System.Linq;
using ClassicUO.Game.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using ClassicUO.Utility;
using SDL2;

namespace ClassicUO.Game.UI.Controls;

internal class MacroControl : Control
{
	private enum buttonsOption
	{
		AddBtn,
		RemoveBtn,
		CreateNewMacro,
		OpenMacroOptions,
		ChangeMacroName
	}

	private class MacroEntry : Control
	{
		private readonly MacroControl _control;

		private readonly string[] _items;

		public MacroEntry(MacroControl control, MacroObject obj, string[] items)
		{
			_control = control;
			_items = items;
			List<string> list = new List<string>();
			for (int i = 0; i < items.Length; i++)
			{
				string description = ((MacroType)i).GetDescription();
				if (string.IsNullOrEmpty(description))
				{
					list.Add(_items[i]);
				}
				else
				{
					list.Add(description);
				}
			}
			Dictionary<int, string> dictionary = new Dictionary<int, string>();
			for (int j = 0; j < list.Count; j++)
			{
				if (list[j] != "Keine Funktion")
				{
					dictionary.Add(j, list[j]);
				}
			}
			ComboBoxDict comboBoxDict = new ComboBoxDict(0, 0, 200, dictionary, (int)obj.Code, 200, showArrow: true, "", 9)
			{
				Tag = obj
			};
			comboBoxDict.OnOptionSelected += BoxOnOnOptionSelected;
			Add(comboBoxDict);
			base.Width = comboBoxDict.Width;
			base.Height = comboBoxDict.Height;
			AddSubMacro(obj);
			base.WantUpdateSize = true;
		}

		private void AddSubMacro(MacroObject obj)
		{
			if (obj == null || obj.Code == MacroType.None)
			{
				return;
			}
			switch (obj.SubMenuType)
			{
			case 1:
			{
				int count = 0;
				int offset = 0;
				Macro.GetBoundByCode(obj.Code, ref count, ref offset);
				string[] array = new string[count];
				for (int i = 0; i < count; i++)
				{
					array[i] = _allSubHotkeysNames[i + offset];
				}
				Combobox combobox = new Combobox(20, base.Height, 180, array, (int)(obj.SubCode - offset), 300, showArrow: true, "", 9);
				combobox.OnOptionSelected += delegate(object? senderr, int ee)
				{
					Macro.GetBoundByCode(obj.Code, ref count, ref offset);
					MacroSubType macroSubType = (MacroSubType)(offset + ee);
					obj.SubCode = macroSubType;
					if (obj.Code == MacroType.Useitem)
					{
						switch (macroSubType)
						{
						case MacroSubType.ID:
							GameActions.Say(".usetype", ushort.MaxValue, MessageType.Regular, 3);
							break;
						case MacroSubType.Name:
						case MacroSubType.NameExakt:
							GameActions.Say(".usename", ushort.MaxValue, MessageType.Regular, 3);
							break;
						default:
							GameActions.Say(".usetype", ushort.MaxValue, MessageType.Regular, 3);
							break;
						}
					}
				};
				Add(combobox);
				base.Height += combobox.Height;
				if (obj.Code != MacroType.Useitem)
				{
					break;
				}
				ResizePic resizePic3 = new ResizePic(3000);
				resizePic3.X = 16;
				resizePic3.Y = base.Height;
				resizePic3.Width = 240;
				resizePic3.Height = 60;
				ResizePic resizePic4 = resizePic3;
				Add(resizePic4);
				StbTextBox stbTextBox3 = new StbTextBox(byte.MaxValue, 80, 236, isunicode: true, FontStyle.BlackBorder, 0);
				stbTextBox3.X = resizePic4.X + 4;
				stbTextBox3.Y = resizePic4.Y + 4;
				stbTextBox3.Width = resizePic4.Width - 4;
				stbTextBox3.Height = resizePic4.Height - 4;
				StbTextBox stbTextBox4 = stbTextBox3;
				stbTextBox4.SetText(obj.HasString() ? ((MacroObjectString)obj).Text : string.Empty);
				stbTextBox4.TextChanged += delegate(object? ssss, EventArgs eee)
				{
					if (obj.HasString())
					{
						((MacroObjectString)obj).Text = ((StbTextBox)ssss).Text;
					}
				};
				Add(stbTextBox4);
				base.WantUpdateSize = true;
				base.Height += resizePic4.Height;
				break;
			}
			case 2:
			{
				ResizePic resizePic = new ResizePic(3000);
				resizePic.X = 16;
				resizePic.Y = base.Height;
				resizePic.Width = 240;
				resizePic.Height = 60;
				ResizePic resizePic2 = resizePic;
				Add(resizePic2);
				StbTextBox stbTextBox = new StbTextBox(byte.MaxValue, 80, 236, isunicode: true, FontStyle.BlackBorder, 0);
				stbTextBox.X = resizePic2.X + 4;
				stbTextBox.Y = resizePic2.Y + 4;
				stbTextBox.Width = resizePic2.Width - 4;
				stbTextBox.Height = resizePic2.Height - 4;
				StbTextBox stbTextBox2 = stbTextBox;
				stbTextBox2.SetText(obj.HasString() ? ((MacroObjectString)obj).Text : string.Empty);
				stbTextBox2.TextChanged += delegate(object? sss, EventArgs eee)
				{
					if (obj.HasString())
					{
						((MacroObjectString)obj).Text = ((StbTextBox)sss).Text;
					}
				};
				Add(stbTextBox2);
				base.WantUpdateSize = true;
				base.Height += resizePic2.Height;
				break;
			}
			}
			_control._databox.ReArrangeChildren();
		}

		private void BoxOnOnOptionSelected(object sender, int e)
		{
			base.WantUpdateSize = true;
			ComboBoxDict comboBoxDict = (ComboBoxDict)sender;
			MacroObject macroObject = (MacroObject)comboBoxDict.Tag;
			if (e == 0)
			{
				_control.Macro.Remove(macroObject);
				comboBoxDict.Tag = null;
				Dispose();
				_control.SetupMacroUI();
				return;
			}
			MacroObject macroObject2 = Macro.Create((MacroType)e);
			_control.Macro.Insert(macroObject, macroObject2);
			_control.Macro.Remove(macroObject);
			comboBoxDict.Tag = macroObject2;
			for (int i = 1; i < base.Children.Count; i++)
			{
				base.Children[i]?.Dispose();
			}
			base.Height = comboBoxDict.Height;
			AddSubMacro(macroObject2);
		}
	}

	private static readonly string[] _allHotkeysNames = Enum.GetNames(typeof(MacroType));

	private static readonly string[] _allSubHotkeysNames = Enum.GetNames(typeof(MacroSubType));

	private readonly DataBox _databox;

	private readonly HotkeyBox _hotkeyBox;

	public Macro Macro { get; }

	public MacroControl(string name, bool isFastAssign = false)
	{
		CanMove = true;
		_hotkeyBox = new HotkeyBox();
		_hotkeyBox.HotkeyChanged += BoxOnHotkeyChanged;
		_hotkeyBox.HotkeyCancelled += BoxOnHotkeyCancelled;
		Add(_hotkeyBox);
		Add(new NiceButton(0, _hotkeyBox.Height + 3, 170, 25, ButtonAction.Activate, ResGumps.CreateMacroButton, 0, TEXT_ALIGN_TYPE.TS_LEFT)
		{
			ButtonParameter = 2,
			IsSelectable = false
		});
		if (!isFastAssign)
		{
			Add(new NiceButton(0, _hotkeyBox.Height + 30, 70, 25, ButtonAction.Activate, ResGumps.Add)
			{
				ButtonParameter = 0,
				IsSelectable = false
			});
			Add(new NiceButton(72, _hotkeyBox.Height + 30, 70, 25, ButtonAction.Activate, ResGumps.Remove, 0, TEXT_ALIGN_TYPE.TS_LEFT)
			{
				ButtonParameter = 1,
				IsSelectable = false
			});
			Add(new NiceButton(144, _hotkeyBox.Height + 30, 120, 25, ButtonAction.Activate, "Ändere Makronamen", 0, TEXT_ALIGN_TYPE.TS_LEFT)
			{
				ButtonParameter = 4,
				IsSelectable = false
			});
		}
		else
		{
			Add(new NiceButton(0, _hotkeyBox.Height + 30, 170, 25, ButtonAction.Activate, ResGumps.OpenMacroSettings)
			{
				ButtonParameter = 3,
				IsSelectable = false
			});
		}
		int h = (isFastAssign ? 80 : 280);
		ScrollArea scrollArea = new ScrollArea(10, _hotkeyBox.Bounds.Bottom + 80, isFastAssign ? 230 : 280, h, normalScrollbar: true);
		Add(scrollArea);
		_databox = new DataBox(0, 0, 280, 280)
		{
			WantUpdateSize = true
		};
		scrollArea.Add(_databox);
		Macro = Client.Game.GetScene<GameScene>().Macros.FindMacro(name) ?? Macro.CreateEmptyMacro(name);
		SetupKeyByDefault();
		SetupMacroUI();
	}

	private void AddEmptyMacro()
	{
		MacroObject macroObject = (MacroObject)Macro.Items;
		if (macroObject.Code == MacroType.None)
		{
			return;
		}
		while (macroObject.Next != null)
		{
			MacroObject macroObject2 = (MacroObject)macroObject.Next;
			if (macroObject2.Code == MacroType.None)
			{
				return;
			}
			macroObject = macroObject2;
		}
		MacroObject macroObject3 = Macro.Create(MacroType.None);
		Macro.PushToBack(macroObject3);
		_databox.Add(new MacroEntry(this, macroObject3, _allHotkeysNames));
		_databox.WantUpdateSize = true;
		_databox.ReArrangeChildren();
	}

	private void RemoveLastCommand()
	{
		if (_databox.Children.Count != 0)
		{
			LinkedObject last = Macro.GetLast();
			Macro.Remove(last);
			_databox.Children[_databox.Children.Count - 1].Dispose();
			SetupMacroUI();
		}
		if (_databox.Children.Count == 0)
		{
			AddEmptyMacro();
		}
	}

	private void SetupMacroUI()
	{
		if (Macro == null)
		{
			return;
		}
		_databox.Clear();
		_databox.Children.Clear();
		if (Macro.Items == null)
		{
			Macro.Items = Macro.Create(MacroType.None);
		}
		for (MacroObject macroObject = (MacroObject)Macro.Items; macroObject != null; macroObject = (MacroObject)macroObject.Next)
		{
			_databox.Add(new MacroEntry(this, macroObject, _allHotkeysNames));
			if (macroObject.Next != null && macroObject.Code == MacroType.None)
			{
				break;
			}
		}
		_databox.WantUpdateSize = true;
		_databox.ReArrangeChildren();
	}

	private void SetupKeyByDefault()
	{
		if (Macro != null && _hotkeyBox != null && Macro.Key != 0)
		{
			SDL.SDL_Keymod sDL_Keymod = SDL.SDL_Keymod.KMOD_NONE;
			if (Macro.Alt)
			{
				sDL_Keymod |= SDL.SDL_Keymod.KMOD_ALT;
			}
			if (Macro.Shift)
			{
				sDL_Keymod |= SDL.SDL_Keymod.KMOD_SHIFT;
			}
			if (Macro.Ctrl)
			{
				sDL_Keymod |= SDL.SDL_Keymod.KMOD_CTRL;
			}
			_hotkeyBox.SetKey(Macro.Key, sDL_Keymod);
		}
	}

	private void BoxOnHotkeyChanged(object sender, EventArgs e)
	{
		bool shift = (_hotkeyBox.Mod & SDL.SDL_Keymod.KMOD_SHIFT) != 0;
		bool alt = (_hotkeyBox.Mod & SDL.SDL_Keymod.KMOD_ALT) != 0;
		bool ctrl = (_hotkeyBox.Mod & SDL.SDL_Keymod.KMOD_CTRL) != 0;
		if (_hotkeyBox.Key == SDL.SDL_Keycode.SDLK_UNKNOWN)
		{
			return;
		}
		Macro macro = Client.Game.GetScene<GameScene>().Macros.FindMacro(_hotkeyBox.Key, alt, ctrl, shift);
		if (macro != null)
		{
			if (Macro != macro)
			{
				SetupKeyByDefault();
				UIManager.Add(new MessageBoxGump(250, 150, string.Format(ResGumps.ThisKeyCombinationAlreadyExists, macro.Name), null, hasBackground: false, MessageButtonType.OK, 1));
			}
		}
		else
		{
			Macro macro2 = Macro;
			macro2.Key = _hotkeyBox.Key;
			macro2.Shift = shift;
			macro2.Alt = alt;
			macro2.Ctrl = ctrl;
		}
	}

	private void BoxOnHotkeyCancelled(object sender, EventArgs e)
	{
		Macro macro = Macro;
		bool flag2 = (macro.Shift = false);
		bool alt = (macro.Ctrl = flag2);
		macro.Alt = alt;
		macro.Key = SDL.SDL_Keycode.SDLK_UNKNOWN;
	}

	public override void OnButtonClick(int buttonID)
	{
		switch (buttonID)
		{
		case 0:
			AddEmptyMacro();
			break;
		case 1:
			RemoveLastCommand();
			break;
		case 2:
			UIManager.Gumps.OfType<MacroButtonGump>().FirstOrDefault((MacroButtonGump s) => s._macro == Macro)?.Dispose();
			UIManager.Add(new MacroButtonGump(Macro, Mouse.Position.X, Mouse.Position.Y));
			break;
		case 3:
			UIManager.Gumps.OfType<MacroGump>().FirstOrDefault()?.Dispose();
			GameActions.OpenSettings(4);
			break;
		case 4:
			UIManager.Add(new EntryDialog(250, 150, ResGumps.MacroName, delegate(string name)
			{
				if (!string.IsNullOrWhiteSpace(name))
				{
					MacroManager macros = Client.Game.GetScene<GameScene>().Macros;
					if (macros.FindMacro(name) == null)
					{
						Macro.Name = name;
						macros.Save();
						UIManager.GetGump<OptionsGump>(null)?.Dispose();
						GameActions.OpenSettings(4);
					}
				}
			})
			{
				CanCloseWithRightClick = true
			});
			break;
		}
	}
}

using System;
using System.Linq;
using ClassicUO.Configuration;
using ClassicUO.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using SDL2;

namespace ClassicUO.Game.UI.Gumps.Login;

internal class CharacterSelectionGump : Gump
{
	private enum Buttons
	{
		New,
		Delete,
		Next,
		Prev
	}

	private class CharacterEntryGump : Control
	{
		private readonly Label _label;

		private readonly Action<uint> _loginFn;

		private readonly Action<uint> _selectedFn;

		public uint CharacterIndex { get; }

		public ushort Hue
		{
			get
			{
				return _label.Hue;
			}
			set
			{
				_label.Hue = value;
			}
		}

		public CharacterEntryGump(uint index, string character, Action<uint> selectedFn, Action<uint> loginFn)
		{
			CharacterIndex = index;
			_selectedFn = selectedFn;
			_loginFn = loginFn;
			ResizePic resizePic = new ResizePic(3000);
			resizePic.X = 0;
			resizePic.Y = 0;
			resizePic.Width = 280;
			resizePic.Height = 30;
			Add(resizePic);
			Label label = new Label(character, isunicode: false, 847, 270, 5, FontStyle.None, TEXT_ALIGN_TYPE.TS_CENTER);
			label.X = 0;
			Label c = label;
			_label = label;
			Add(c);
			AcceptMouseInput = true;
		}

		protected override bool OnMouseDoubleClick(int x, int y, MouseButtonType button)
		{
			if (button == MouseButtonType.Left)
			{
				_loginFn(CharacterIndex);
				return true;
			}
			return false;
		}

		protected override void OnMouseUp(int x, int y, MouseButtonType button)
		{
			if (button == MouseButtonType.Left)
			{
				_selectedFn(CharacterIndex);
			}
		}
	}

	private const ushort SELECTED_COLOR = 33;

	private const ushort NORMAL_COLOR = 847;

	private uint _selectedCharacter;

	public CharacterSelectionGump()
		: base(0u, 0u)
	{
		base.CanCloseWithRightClick = false;
		int num = 0;
		int num2 = 150;
		int num3 = 0;
		int num4 = 106;
		LoginScene scene = Client.Game.GetScene<LoginScene>();
		string lastCharName = LastCharacterManager.GetLastCharacter(LoginScene.Account, World.ServerName);
		string value = scene.Characters.FirstOrDefault((string o) => o == lastCharName);
		_ = World.ClientLockedFeatures.Flags;
		_ = World.ClientFeatures.Flags;
		if (Client.Version >= ClientVersion.CV_6040 || (Client.Version >= ClientVersion.CV_5020 && scene.Characters.Length > 5))
		{
			num4 = 96;
			num2 = 125;
			num3 = 45;
		}
		if (!string.IsNullOrEmpty(value))
		{
			_selectedCharacter = (uint)Array.IndexOf(scene.Characters, value);
		}
		else if (scene.Characters.Length != 0)
		{
			_selectedCharacter = 0u;
		}
		ResizePic resizePic = new ResizePic(2600);
		resizePic.X = 160;
		resizePic.Y = 70;
		resizePic.Width = 408;
		resizePic.Height = 343 + num3;
		Add(resizePic, 1);
		bool isunicode;
		bool num5 = (isunicode = string.Compare(Settings.GlobalSettings.Language, "CHT", StringComparison.InvariantCultureIgnoreCase) == 0 || string.Compare(Settings.GlobalSettings.Language, "KOR", StringComparison.InvariantCultureIgnoreCase) == 0 || string.Compare(Settings.GlobalSettings.Language, "JPN", StringComparison.InvariantCultureIgnoreCase) == 0);
		byte font = (byte)(num5 ? 1u : 2u);
		ushort hue = (ushort)(num5 ? 65535u : 902u);
		Label label = new Label(ClilocLoader.Instance.GetString(3000050, "Character Selection"), isunicode, hue, 0, font);
		label.X = 267;
		label.Y = num4;
		Add(label, 1);
		int i = 0;
		int num6 = 0;
		for (; i < scene.Characters.Length; i++)
		{
			string text = scene.Characters[i];
			if (!string.IsNullOrEmpty(text))
			{
				num6++;
				if (num6 > World.ClientFeatures.MaxChars || (World.ClientLockedFeatures.Flags != 0 && !World.ClientLockedFeatures.CharSlots7 && num6 == 6 && !World.ClientLockedFeatures.CharSlots6))
				{
					break;
				}
				CharacterEntryGump characterEntryGump = new CharacterEntryGump((uint)i, text, SelectCharacter, LoginCharacter);
				characterEntryGump.X = 224;
				characterEntryGump.Y = num2 + num * 40;
				characterEntryGump.Hue = (ushort)((i == _selectedCharacter) ? 33 : 847);
				Add(characterEntryGump, 1);
				num++;
			}
		}
		if (CanCreateChar(scene))
		{
			Button button = new Button(0, 5533, 5535, 5534, "", 0);
			button.X = 224;
			button.Y = 350 + num3;
			button.ButtonAction = ButtonAction.Activate;
			Add(button, 1);
		}
		Button button2 = new Button(1, 5530, 5532, 5531, "", 0);
		button2.X = 442;
		button2.Y = 350 + num3;
		button2.ButtonAction = ButtonAction.Activate;
		Add(button2, 1);
		Button button3 = new Button(3, 5537, 5539, 5538, "", 0);
		button3.X = 586;
		button3.Y = 445;
		button3.ButtonAction = ButtonAction.Activate;
		Add(button3, 1);
		Button button4 = new Button(2, 5540, 5542, 5541, "", 0);
		button4.X = 610;
		button4.Y = 445;
		button4.ButtonAction = ButtonAction.Activate;
		Add(button4, 1);
		AcceptKeyboardInput = true;
		ChangePage(1);
	}

	private bool CanCreateChar(LoginScene scene)
	{
		if (scene.Characters != null)
		{
			int num = scene.Characters.Count(string.IsNullOrEmpty);
			if (num >= 0 && scene.Characters.Length - num < World.ClientFeatures.MaxChars)
			{
				return true;
			}
		}
		return false;
	}

	protected override void OnKeyDown(SDL.SDL_Keycode key, SDL.SDL_Keymod mod)
	{
		if (key == SDL.SDL_Keycode.SDLK_RETURN || key == SDL.SDL_Keycode.SDLK_KP_ENTER)
		{
			LoginCharacter(_selectedCharacter);
		}
	}

	public override void OnButtonClick(int buttonID)
	{
		LoginScene scene = Client.Game.GetScene<LoginScene>();
		switch ((Buttons)buttonID)
		{
		case Buttons.Delete:
			DeleteCharacter(scene);
			break;
		case Buttons.New:
			if (CanCreateChar(scene))
			{
				scene.StartCharCreation();
			}
			break;
		case Buttons.Next:
			LoginCharacter(_selectedCharacter);
			break;
		case Buttons.Prev:
			scene.StepBack();
			break;
		}
		base.OnButtonClick(buttonID);
	}

	private void DeleteCharacter(LoginScene loginScene)
	{
		string text = loginScene.Characters[_selectedCharacter];
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		LoadingGump loadingGump = base.Children.OfType<LoadingGump>().FirstOrDefault();
		if (loadingGump != null)
		{
			Remove(loadingGump);
		}
		Add(new LoadingGump(string.Format(ResGumps.PermanentlyDelete0, text), LoginButtons.OK | LoginButtons.Cancel, delegate(int buttonID)
		{
			if (buttonID == 2)
			{
				loginScene.DeleteCharacter(_selectedCharacter);
			}
			else
			{
				ChangePage(1);
			}
		}), 2);
		ChangePage(2);
	}

	private void SelectCharacter(uint index)
	{
		_selectedCharacter = index;
		foreach (CharacterEntryGump item in FindControls<CharacterEntryGump>())
		{
			item.Hue = (ushort)((item.CharacterIndex == index) ? 33 : 847);
		}
	}

	private void LoginCharacter(uint index)
	{
		LoginScene scene = Client.Game.GetScene<LoginScene>();
		if (scene.Characters.Length > index && !string.IsNullOrEmpty(scene.Characters[index]))
		{
			scene.SelectCharacter(index);
		}
	}
}

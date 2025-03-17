using ClassicUO.Configuration;
using ClassicUO.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;
using SDL2;

namespace ClassicUO.Game.UI.Gumps.Login;

internal class LoginGump : Gump
{
	private class PasswordStbTextBox : StbTextBox
	{
		private new Point _caretScreenPosition;

		private new readonly RenderedText _rendererCaret;

		private new readonly RenderedText _rendererText;

		internal string RealText
		{
			get
			{
				return base.Text;
			}
			set
			{
				SetText(value);
			}
		}

		public new ushort Hue
		{
			get
			{
				return _rendererText.Hue;
			}
			set
			{
				if (_rendererText.Hue != value)
				{
					_rendererText.Hue = value;
					_rendererCaret.Hue = value;
					_rendererText.CreateTexture();
					_rendererCaret.CreateTexture();
				}
			}
		}

		public PasswordStbTextBox(byte font, int max_char_count = -1, int maxWidth = 0, bool isunicode = true, FontStyle style = FontStyle.None, ushort hue = 0, TEXT_ALIGN_TYPE align = TEXT_ALIGN_TYPE.TS_LEFT)
			: base(font, max_char_count, maxWidth, isunicode, style, hue, align)
		{
			_rendererText = RenderedText.Create(string.Empty, hue, font, isunicode, style, align, maxWidth, 30);
			_rendererCaret = RenderedText.Create("_", hue, font, isunicode, ((style & FontStyle.BlackBorder) != 0) ? FontStyle.BlackBorder : FontStyle.None, align, 0, 30);
			base.NoSelection = true;
		}

		protected override void DrawCaret(UltimaBatcher2D batcher, int x, int y)
		{
			if (base.HasKeyboardFocus)
			{
				_rendererCaret.Draw(batcher, x + _caretScreenPosition.X, y + _caretScreenPosition.Y, 1f, 0);
			}
		}

		protected override void OnMouseDown(int x, int y, MouseButtonType button)
		{
			base.OnMouseDown(x, y, button);
			if (button == MouseButtonType.Left)
			{
				UpdateCaretScreenPosition();
			}
		}

		protected override void OnKeyDown(SDL.SDL_Keycode key, SDL.SDL_Keymod mod)
		{
			base.OnKeyDown(key, mod);
			UpdateCaretScreenPosition();
		}

		public override void Dispose()
		{
			_rendererText?.Destroy();
			_rendererCaret?.Destroy();
			base.Dispose();
		}

		protected override void OnTextInput(string c)
		{
			base.OnTextInput(c);
		}

		protected override void OnTextChanged()
		{
			if (base.Text.Length > 0)
			{
				_rendererText.Text = new string('*', base.Text.Length);
			}
			else
			{
				_rendererText.Text = string.Empty;
			}
			base.OnTextChanged();
			UpdateCaretScreenPosition();
		}

		internal override void OnFocusEnter()
		{
			base.OnFocusEnter();
			base.CaretIndex = base.Text?.Length ?? 0;
			UpdateCaretScreenPosition();
		}

		private new void UpdateCaretScreenPosition()
		{
			_caretScreenPosition = _rendererText.GetCaretPosition(base.Stb.CursorIndex);
		}

		public override bool Draw(UltimaBatcher2D batcher, int x, int y)
		{
			if (batcher.ClipBegin(x, y, base.Width, base.Height))
			{
				DrawSelection(batcher, x, y);
				_rendererText.Draw(batcher, x, y, 1f, 0);
				DrawCaret(batcher, x, y);
				batcher.ClipEnd();
			}
			return true;
		}
	}

	private enum Buttons
	{
		NextArrow,
		Quit,
		Credits
	}

	private readonly ushort _buttonNormal;

	private readonly ushort _buttonOver;

	private readonly Checkbox _checkboxAutologin;

	private readonly Checkbox _checkboxSaveAccount;

	private readonly Button _nextArrow0;

	private readonly PasswordStbTextBox _passwordFake;

	private readonly StbTextBox _textboxAccount;

	private float _time;

	public LoginGump(LoginScene scene)
		: base(0u, 0u)
	{
		base.CanCloseWithRightClick = false;
		AcceptKeyboardInput = false;
		int num4;
		int num5;
		byte font;
		ushort color;
		int num3;
		if (Client.Version < ClientVersion.CV_706400)
		{
			_buttonNormal = 5540;
			_buttonOver = 5541;
			if (Client.Version >= ClientVersion.CV_500A)
			{
				Add(new GumpPic(0, 0, 9001, 0));
			}
			Add(new GumpPic(0, 4, 5536, 0)
			{
				AcceptKeyboardInput = false
			});
			Button button = new Button(1, 5513, 5515, 5514, "", 0);
			button.X = 555;
			button.Y = 4;
			button.ButtonAction = ButtonAction.Activate;
			Add(button);
			Label label = new Label("Wusstest du schon?", isunicode: false, 100, 500, 2);
			label.X = 60;
			label.Y = 150;
			Add(label);
			string oneTip = UOFileManager.GetOneTip();
			if (!string.IsNullOrEmpty(oneTip))
			{
				int num = oneTip.IndexOf(']') + 1;
				string text = oneTip.Remove(num);
				string text2 = oneTip.Remove(0, num);
				ushort num2 = 0;
				Label label2 = new Label(text2, isunicode: true, text switch
				{
					"[BLAU]" => 101, 
					"[GELB]" => 154, 
					"[GRUEN]" => 163, 
					_ => 0, 
				}, 500, 0, FontStyle.BlackBorder);
				label2.X = 60;
				label2.Y = 170;
				Add(label2);
			}
			ResizePic resizePic = new ResizePic(5054);
			resizePic.X = 128;
			resizePic.Y = 288;
			resizePic.Width = 451;
			resizePic.Height = 157;
			Add(resizePic);
			if (Client.Version < ClientVersion.CV_500A)
			{
				Add(new GumpPic(286, 45, 1418, 0));
			}
			Button button2 = new Button(2, 5507, 5509, 5508, "", 0);
			button2.X = 60;
			button2.Y = 385;
			button2.ButtonAction = ButtonAction.Activate;
			Add(button2);
			Label label3 = new Label(ResGumps.LoginToUO, isunicode: false, 902, 0, 2);
			label3.X = 253;
			label3.Y = 305;
			Add(label3);
			Label label4 = new Label(ResGumps.Account, isunicode: false, 902, 0, 2);
			label4.X = 183;
			label4.Y = 345;
			Add(label4);
			Label label5 = new Label(ResGumps.Password, isunicode: false, 902, 0, 2);
			label5.X = 183;
			label5.Y = 385;
			Add(label5);
			Button button3 = new Button(0, 5540, 5542, 5541, "", 0);
			button3.X = 610;
			button3.Y = 445;
			button3.ButtonAction = ButtonAction.Activate;
			Button c = button3;
			_nextArrow0 = button3;
			Add(c);
			num3 = 328;
			num4 = 343;
			num5 = 40;
			Label label6 = new Label("UO Version " + Settings.GlobalSettings.ClientVersion + ".", isunicode: false, 846, 0, 9);
			label6.X = 286;
			label6.Y = 453;
			Add(label6);
			Label label7 = new Label(string.Format(ResGumps.CUOVersion0, CUOEnviroment.Version), isunicode: false, 846, 0, 9);
			label7.X = 286;
			label7.Y = 465;
			Add(label7);
			Checkbox checkbox = new Checkbox(210, 211, ResGumps.Autologin, 1, 902, isunicode: false);
			checkbox.X = 150;
			checkbox.Y = 417;
			Checkbox c2 = checkbox;
			_checkboxAutologin = checkbox;
			Add(c2);
			Checkbox checkbox2 = new Checkbox(210, 211, ResGumps.SaveAccount, 1, 902, isunicode: false);
			checkbox2.X = _checkboxAutologin.X + _checkboxAutologin.Width + 10;
			checkbox2.Y = 417;
			c2 = checkbox2;
			_checkboxSaveAccount = checkbox2;
			Add(c2);
			font = 1;
			color = 902;
		}
		else
		{
			_buttonNormal = 1485;
			_buttonOver = 1483;
			Add(new GumpPic(0, 0, 334, 0));
			Button button4 = new Button(1, 1482, 1481, 1480, "", 0);
			button4.X = 25;
			button4.Y = 240;
			button4.ButtonAction = ButtonAction.Activate;
			Add(button4);
			Button button5 = new Button(2, 1488, 1487, 1486, "", 0);
			button5.X = 530;
			button5.Y = 125;
			button5.ButtonAction = ButtonAction.Activate;
			Add(button5);
			Button button6 = new Button(0, 1485, 1484, 1483, "", 0);
			button6.X = 280;
			button6.Y = 365;
			button6.ButtonAction = ButtonAction.Activate;
			Button c = button6;
			_nextArrow0 = button6;
			Add(c);
			num3 = 218;
			num4 = 283;
			num5 = 50;
			Label label8 = new Label("UO Version " + Settings.GlobalSettings.ClientVersion + ".", isunicode: false, 1153, 0, 9);
			label8.X = 286;
			label8.Y = 453;
			Add(label8);
			Label label9 = new Label(string.Format(ResGumps.CUOVersion0, CUOEnviroment.Version), isunicode: false, 1153, 0, 9);
			label9.X = 286;
			label9.Y = 465;
			Add(label9);
			Checkbox checkbox3 = new Checkbox(210, 211, ResGumps.Autologin, 9, 1153, isunicode: false);
			checkbox3.X = 150;
			checkbox3.Y = 417;
			Checkbox c2 = checkbox3;
			_checkboxAutologin = checkbox3;
			Add(c2);
			Checkbox checkbox4 = new Checkbox(210, 211, ResGumps.SaveAccount, 9, 1153, isunicode: false);
			checkbox4.X = _checkboxAutologin.X + _checkboxAutologin.Width + 10;
			checkbox4.Y = 417;
			c2 = checkbox4;
			_checkboxSaveAccount = checkbox4;
			Add(c2);
			font = 9;
			color = 1153;
		}
		ResizePic resizePic2 = new ResizePic(3000);
		resizePic2.X = num3;
		resizePic2.Y = num4;
		resizePic2.Width = 210;
		resizePic2.Height = 30;
		Add(resizePic2);
		ResizePic resizePic3 = new ResizePic(3000);
		resizePic3.X = num3;
		resizePic3.Y = num4 + num5;
		resizePic3.Width = 210;
		resizePic3.Height = 30;
		Add(resizePic3);
		num3 += 7;
		StbTextBox stbTextBox = new StbTextBox(5, 16, 190, isunicode: false, FontStyle.None, 847);
		stbTextBox.X = num3;
		stbTextBox.Y = num4;
		stbTextBox.Width = 190;
		stbTextBox.Height = 25;
		StbTextBox c3 = stbTextBox;
		_textboxAccount = stbTextBox;
		Add(c3);
		_textboxAccount.SetText(Settings.GlobalSettings.Username);
		PasswordStbTextBox passwordStbTextBox = new PasswordStbTextBox(5, 16, 190, isunicode: false, FontStyle.None, 847);
		passwordStbTextBox.X = num3;
		passwordStbTextBox.Y = num4 + num5 + 2;
		passwordStbTextBox.Width = 190;
		passwordStbTextBox.Height = 25;
		PasswordStbTextBox c4 = passwordStbTextBox;
		_passwordFake = passwordStbTextBox;
		Add(c4);
		_passwordFake.RealText = Crypter.Decrypt(Settings.GlobalSettings.Password);
		_checkboxSaveAccount.IsChecked = Settings.GlobalSettings.SaveAccount;
		_checkboxAutologin.IsChecked = Settings.GlobalSettings.AutoLogin;
		int y = 442;
		Add(new HtmlControl(505, y, 100, 15, hasbackground: false, hasscrollbar: false, useflagscrollbar: false, "<body link=\"#FF00FF00\" vlink=\"#FF00FF00\" ><a href=\"https://www.Alathair.de\">Website", 50, ishtml: true, 1, isunicode: true, FontStyle.BlackBorder));
		Add(new HtmlControl(105, y, 100, 150, hasbackground: false, hasscrollbar: false, useflagscrollbar: false, "<body link=\"#7289da\" vlink=\"#5469b3\" ><a href=\"https://www.Alathair.de/discord\">Offizieller Alathairdiscord", 50, ishtml: true, 1, isunicode: true, FontStyle.BlackBorder));
		Checkbox checkbox5 = new Checkbox(210, 211, "Music", font, color, isunicode: false);
		checkbox5.X = _checkboxSaveAccount.X + _checkboxSaveAccount.Width + 10;
		checkbox5.Y = 417;
		checkbox5.IsChecked = Settings.GlobalSettings.LoginMusic;
		Checkbox loginmusic_checkbox = checkbox5;
		Add(loginmusic_checkbox);
		HSliderBar login_music = new HSliderBar(loginmusic_checkbox.X + loginmusic_checkbox.Width + 10, loginmusic_checkbox.Y + 4, 80, 0, 100, Settings.GlobalSettings.LoginMusicVolume, HSliderBarStyle.MetalWidgetRecessedBar, hasText: true, font, color, unicode: false);
		Add(login_music);
		login_music.IsVisible = Settings.GlobalSettings.LoginMusic;
		loginmusic_checkbox.ValueChanged += delegate
		{
			Settings.GlobalSettings.LoginMusic = loginmusic_checkbox.IsChecked;
			scene.Audio.UpdateCurrentMusicVolume(isLogin: true);
			login_music.IsVisible = Settings.GlobalSettings.LoginMusic;
		};
		login_music.ValueChanged += delegate
		{
			Settings.GlobalSettings.LoginMusicVolume = login_music.Value;
			scene.Audio.UpdateCurrentMusicVolume(isLogin: true);
		};
		if (!string.IsNullOrEmpty(_textboxAccount.Text))
		{
			_passwordFake.SetKeyboardFocus();
		}
		else
		{
			_textboxAccount.SetKeyboardFocus();
		}
	}

	public override void OnKeyboardReturn(int textID, string text)
	{
		SaveCheckboxStatus();
		LoginScene scene = Client.Game.GetScene<LoginScene>();
		if (scene.CurrentLoginStep == LoginSteps.Main)
		{
			scene.Connect(_textboxAccount.Text, _passwordFake.RealText);
		}
	}

	private void SaveCheckboxStatus()
	{
		Settings.GlobalSettings.SaveAccount = _checkboxSaveAccount.IsChecked;
		Settings.GlobalSettings.AutoLogin = _checkboxAutologin.IsChecked;
	}

	public override void Update(double totalTime, double frameTime)
	{
		if (base.IsDisposed)
		{
			return;
		}
		base.Update(totalTime, frameTime);
		if ((double)_time < totalTime)
		{
			_time = (float)totalTime + 1000f;
			_nextArrow0.ButtonGraphicNormal = ((_nextArrow0.ButtonGraphicNormal == _buttonNormal) ? _buttonOver : _buttonNormal);
		}
		if (_passwordFake.HasKeyboardFocus)
		{
			if (_passwordFake.Hue != 33)
			{
				_passwordFake.Hue = 33;
			}
		}
		else if (_passwordFake.Hue != 0)
		{
			_passwordFake.Hue = 0;
		}
		if (_textboxAccount.HasKeyboardFocus)
		{
			if (_textboxAccount.Hue != 33)
			{
				_textboxAccount.Hue = 33;
			}
		}
		else if (_textboxAccount.Hue != 0)
		{
			_textboxAccount.Hue = 0;
		}
	}

	public override void OnButtonClick(int buttonID)
	{
		switch ((Buttons)buttonID)
		{
		case Buttons.NextArrow:
			SaveCheckboxStatus();
			if (!_textboxAccount.IsDisposed)
			{
				Client.Game.GetScene<LoginScene>().Connect(_textboxAccount.Text, _passwordFake.RealText);
			}
			break;
		case Buttons.Quit:
			Client.Game.Exit();
			break;
		case Buttons.Credits:
			UIManager.Add(new CreditsGump());
			break;
		}
	}
}

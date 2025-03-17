using System;
using System.Collections.Generic;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Network;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using ClassicUO.Utility.Platforms;
using SDL2;

namespace ClassicUO.Game.UI.Gumps;

internal class SystemChatControl : Control
{
	private class ChatLineTime
	{
		private uint _createdTime;

		private RenderedText _renderedText;

		private string Text => _renderedText?.Text ?? string.Empty;

		public bool IsDisposed
		{
			get
			{
				if (_renderedText != null)
				{
					return _renderedText.IsDestroyed;
				}
				return true;
			}
		}

		public int TextHeight => _renderedText?.Height ?? 0;

		public ChatLineTime(string text, byte font, bool isunicode, ushort hue)
		{
			_renderedText = RenderedText.Create(text, hue, font, isunicode, FontStyle.BlackBorder, TEXT_ALIGN_TYPE.TS_LEFT, 395, 30);
			_createdTime = Time.Ticks + 10000;
		}

		public void Update(double totalTime, double frameTime)
		{
			if (Time.Ticks > _createdTime)
			{
				Destroy();
			}
		}

		public bool Draw(UltimaBatcher2D batcher, int x, int y)
		{
			if (!IsDisposed)
			{
				return _renderedText.Draw(batcher, x, y, 1f, 0);
			}
			return false;
		}

		public override string ToString()
		{
			return Text;
		}

		public void Destroy()
		{
			if (!IsDisposed)
			{
				_renderedText?.Destroy();
				_renderedText = null;
			}
		}
	}

	private const int MAX_MESSAGE_LENGHT = 100;

	private const int CHAT_X_OFFSET = 3;

	private const int CHAT_HEIGHT = 15;

	private static readonly List<Tuple<ChatMode, string>> _messageHistory = new List<Tuple<ChatMode, string>>();

	private static int _messageHistoryIndex = -1;

	private readonly Label _currentChatModeLabel;

	private bool _isActive;

	private ChatMode _mode;

	private readonly LinkedList<ChatLineTime> _textEntries;

	private readonly AlphaBlendControl _trans;

	public readonly StbTextBox TextBoxControl;

	public bool IsActive
	{
		get
		{
			return _isActive;
		}
		set
		{
			StbTextBox textBoxControl = TextBoxControl;
			bool flag2 = (TextBoxControl.IsEditable = value);
			bool isActive = (textBoxControl.IsVisible = flag2);
			_isActive = isActive;
			if (_isActive)
			{
				TextBoxControl.Width = _trans.Width - 3;
				TextBoxControl.ClearText();
			}
			SetFocus();
		}
	}

	public ChatMode Mode
	{
		get
		{
			return _mode;
		}
		set
		{
			_mode = value;
			if (!IsActive)
			{
				return;
			}
			switch (value)
			{
			case ChatMode.Default:
				DisposeChatModePrefix();
				TextBoxControl.Hue = ProfileManager.CurrentProfile.SpeechHue;
				TextBoxControl.ClearText();
				break;
			case ChatMode.Whisper:
				if (World.Player.NotorietyFlag == NotorietyFlag.Staff)
				{
					AppendChatModePrefix(ResGumps.Staff, ProfileManager.CurrentProfile.WhisperHue, TextBoxControl.Text);
				}
				else
				{
					AppendChatModePrefix(ResGumps.Whisper, ProfileManager.CurrentProfile.WhisperHue, TextBoxControl.Text);
				}
				break;
			case ChatMode.Staff:
				AppendChatModePrefix(ResGumps.Staff, ProfileManager.CurrentProfile.WhisperHue, TextBoxControl.Text);
				break;
			case ChatMode.Emote:
				AppendChatModePrefix(ResGumps.Emote, ProfileManager.CurrentProfile.EmoteHue, TextBoxControl.Text);
				break;
			case ChatMode.Yell:
				AppendChatModePrefix(ResGumps.Yell, ProfileManager.CurrentProfile.YellHue, TextBoxControl.Text);
				break;
			case ChatMode.Party:
				AppendChatModePrefix(ResGumps.Party, ProfileManager.CurrentProfile.PartyMessageHue, TextBoxControl.Text);
				break;
			case ChatMode.Guild:
				AppendChatModePrefix(ResGumps.Guild, ProfileManager.CurrentProfile.GuildMessageHue, TextBoxControl.Text);
				break;
			case ChatMode.Alliance:
				AppendChatModePrefix(ResGumps.Alliance, ProfileManager.CurrentProfile.AllyMessageHue, TextBoxControl.Text);
				break;
			case ChatMode.ClientCommand:
				AppendChatModePrefix(ResGumps.Command, 1161, TextBoxControl.Text);
				break;
			case ChatMode.UOAMChat:
				DisposeChatModePrefix();
				AppendChatModePrefix(ResGumps.UOAM, 83, TextBoxControl.Text);
				break;
			case ChatMode.UOChat:
				DisposeChatModePrefix();
				AppendChatModePrefix(ResGumps.Chat, ProfileManager.CurrentProfile.ChatMessageHue, TextBoxControl.Text);
				break;
			case ChatMode.Prompt:
				break;
			}
		}
	}

	public SystemChatControl(int x, int y, int w, int h)
	{
		base.X = x;
		base.Y = y;
		base.Width = w;
		base.Height = h;
		_textEntries = new LinkedList<ChatLineTime>();
		base.CanCloseWithRightClick = false;
		AcceptMouseInput = false;
		AcceptKeyboardInput = false;
		StbTextBox stbTextBox = new StbTextBox(ProfileManager.CurrentProfile.ChatFont, 100, base.Width, isunicode: true, FontStyle.BlackBorder | FontStyle.Fixed, 33);
		stbTextBox.X = x;
		stbTextBox.Y = base.Height - 15 + y;
		stbTextBox.Width = base.Width - x;
		stbTextBox.Height = 15;
		TextBoxControl = stbTextBox;
		TextBoxControl.TextChanged += OnTextboxTextChanged;
		float alpha = ((ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.HideChatGradient) ? 0f : 0.5f);
		AlphaBlendControl alphaBlendControl = new AlphaBlendControl(alpha);
		alphaBlendControl.X = TextBoxControl.X;
		alphaBlendControl.Y = TextBoxControl.Y;
		alphaBlendControl.Width = base.Width;
		alphaBlendControl.Height = 20;
		alphaBlendControl.IsVisible = !ProfileManager.CurrentProfile.ActivateChatAfterEnter;
		alphaBlendControl.AcceptMouseInput = true;
		AlphaBlendControl c = alphaBlendControl;
		_trans = alphaBlendControl;
		Add(c);
		Add(TextBoxControl);
		Label label = new Label(string.Empty, isunicode: true, 0, 0, byte.MaxValue, FontStyle.BlackBorder);
		label.X = TextBoxControl.X;
		label.Y = TextBoxControl.Y;
		label.IsVisible = false;
		Label c2 = label;
		_currentChatModeLabel = label;
		Add(c2);
		base.WantUpdateSize = false;
		MessageManager.MessageReceived += ChatOnMessageReceived;
		Mode = ChatMode.Default;
		IsActive = !ProfileManager.CurrentProfile.ActivateChatAfterEnter;
		SetFocus();
	}

	public void OnTextboxTextChanged(object? sender, EventArgs e)
	{
		if ((Mode != 0 && (Mode != ChatMode.Whisper || World.Player.NotorietyFlag == NotorietyFlag.Staff) && Mode != ChatMode.Emote && Mode != ChatMode.Yell) || string.IsNullOrEmpty(TextBoxControl.Text))
		{
			return;
		}
		char c = TextBoxControl.Text[0];
		if (c == '.')
		{
			return;
		}
		if (TextBoxControl.Text.Length == 1)
		{
			switch (c)
			{
			case '!':
			case '.':
			case ':':
			case ';':
				return;
			}
		}
		World.Player.UpdateLastChatInput();
	}

	public void SetFocus()
	{
		TextBoxControl.IsEditable = true;
		TextBoxControl.SetKeyboardFocus();
		TextBoxControl.IsEditable = _isActive;
		_trans.IsVisible = _isActive;
	}

	public void ToggleChatVisibility()
	{
		IsActive = !IsActive;
	}

	private void ChatOnMessageReceived(object sender, MessageEventArgs e)
	{
		if (e.TextType == TextType.CLIENT)
		{
			return;
		}
		MessageType type = e.Type;
		if (type <= MessageType.Label)
		{
			if (type != 0)
			{
				if (type == MessageType.System)
				{
					goto IL_006a;
				}
				if (type == MessageType.Label && (e.Parent == null || !SerialHelper.IsValid(e.Parent.Serial)))
				{
					AddLine(e.Text, e.Font, e.Hue, e.IsUnicode);
					return;
				}
			}
			else if (e.Parent == null || !SerialHelper.IsValid(e.Parent.Serial))
			{
				goto IL_006a;
			}
		}
		else
		{
			switch (type)
			{
			case MessageType.Party:
				AddLine(string.Format(ResGumps.PartyName0Text1, e.Name, e.Text), e.Font, ProfileManager.CurrentProfile.PartyMessageHue, e.IsUnicode);
				return;
			case MessageType.Guild:
				AddLine(string.Format(ResGumps.GuildName0Text1, e.Name, e.Text), e.Font, ProfileManager.CurrentProfile.GuildMessageHue, e.IsUnicode);
				return;
			case MessageType.Alliance:
				AddLine(string.Format(ResGumps.AllianceName0Text1, e.Name, e.Text), e.Font, ProfileManager.CurrentProfile.AllyMessageHue, e.IsUnicode);
				return;
			}
		}
		if ((e.Parent == null || !SerialHelper.IsValid(e.Parent.Serial)) && (string.IsNullOrEmpty(e.Name) || e.Name.Equals("system", StringComparison.InvariantCultureIgnoreCase)))
		{
			AddLine(e.Text, e.Font, e.Hue, e.IsUnicode);
		}
		return;
		IL_006a:
		if (ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.ShowSystemMessagesInGameWindow)
		{
			if (!string.IsNullOrEmpty(e.Name) && !e.Name.Equals("system", StringComparison.InvariantCultureIgnoreCase))
			{
				AddLine(e.Name + ": " + e.Text, e.Font, e.Hue, e.IsUnicode);
			}
			else
			{
				AddLine(e.Text, e.Font, e.Hue, e.IsUnicode);
			}
		}
	}

	public override void Dispose()
	{
		MessageManager.MessageReceived -= ChatOnMessageReceived;
		base.Dispose();
	}

	private void AppendChatModePrefix(string labelText, ushort hue, string text)
	{
		if (!_currentChatModeLabel.IsVisible)
		{
			_currentChatModeLabel.Hue = hue;
			_currentChatModeLabel.Text = labelText;
			_currentChatModeLabel.IsVisible = true;
			_currentChatModeLabel.Location = TextBoxControl.Location;
			TextBoxControl.X = _currentChatModeLabel.Width;
			TextBoxControl.Hue = hue;
			int num = (string.IsNullOrEmpty(text) ? (-1) : TextBoxControl.Text.IndexOf(text));
			string text2 = string.Empty;
			if (num > 0)
			{
				text2 = TextBoxControl.Text.Substring(num, TextBoxControl.Text.Length - labelText.Length - 1);
			}
			TextBoxControl.SetText(text2);
		}
	}

	private void DisposeChatModePrefix()
	{
		if (_currentChatModeLabel.IsVisible)
		{
			TextBoxControl.Hue = 33;
			TextBoxControl.X -= _currentChatModeLabel.Width;
			_currentChatModeLabel.IsVisible = false;
		}
	}

	public void AddLine(string text, byte font, ushort hue, bool isunicode)
	{
		if (_textEntries.Count >= 30)
		{
			LinkedListNode<ChatLineTime> first = _textEntries.First;
			first.Value.Destroy();
			_textEntries.Remove(first);
		}
		_textEntries.AddLast(new ChatLineTime(text, font, isunicode, hue));
	}

	internal void Resize()
	{
		if (TextBoxControl != null)
		{
			TextBoxControl.X = 3;
			TextBoxControl.Y = base.Height - 15 - 3;
			TextBoxControl.Width = base.Width - 3;
			TextBoxControl.Height = 18;
			_trans.X = TextBoxControl.X - 3;
			_trans.Y = TextBoxControl.Y;
			_trans.Width = base.Width;
			_trans.Height = 20;
		}
	}

	public override void Update(double totalTime, double frameTime)
	{
		LinkedListNode<ChatLineTime> linkedListNode = _textEntries.First;
		while (linkedListNode != null)
		{
			LinkedListNode<ChatLineTime>? next = linkedListNode.Next;
			linkedListNode.Value.Update(totalTime, frameTime);
			if (linkedListNode.Value.IsDisposed)
			{
				_textEntries.Remove(linkedListNode);
			}
			linkedListNode = next;
		}
		UpdateMode();
		base.Update(totalTime, frameTime);
	}

	public void UpdateMode()
	{
		if (Mode == ChatMode.Default && IsActive)
		{
			if (TextBoxControl.Text.Length > 0)
			{
				switch (TextBoxControl.Text[0])
				{
				case '/':
				{
					int i;
					for (i = 1; i < TextBoxControl.Text.Length && TextBoxControl.Text[i] != ' '; i++)
					{
					}
					if (i < TextBoxControl.Text.Length && int.TryParse(TextBoxControl.Text.Substring(1, i), out var result) && result > 0 && result < 31)
					{
						if (World.Party.Members[result - 1] != null && World.Party.Members[result - 1].Serial != 0)
						{
							AppendChatModePrefix(string.Format(ResGumps.Tell0, World.Party.Members[result - 1].Name), ProfileManager.CurrentProfile.PartyMessageHue, string.Empty);
						}
						else
						{
							AppendChatModePrefix(ResGumps.TellEmpty, ProfileManager.CurrentProfile.PartyMessageHue, string.Empty);
						}
						Mode = ChatMode.Party;
						TextBoxControl.SetText($"{result} ");
					}
					else
					{
						Mode = ChatMode.Party;
					}
					break;
				}
				case ',':
					if (ChatManager.ChatIsEnabled == ChatStatus.Enabled)
					{
						Mode = ChatMode.UOChat;
					}
					break;
				case ':':
					if (TextBoxControl.Text.Length > 1 && TextBoxControl.Text[1] == ' ')
					{
						Mode = ChatMode.Emote;
					}
					break;
				case ';':
					if (TextBoxControl.Text.Length > 1 && TextBoxControl.Text[1] == ' ')
					{
						Mode = ChatMode.Whisper;
					}
					break;
				case '!':
					if (TextBoxControl.Text.Length > 1 && TextBoxControl.Text[1] == ' ')
					{
						Mode = ChatMode.Yell;
					}
					break;
				}
			}
		}
		else if (Mode == ChatMode.ClientCommand && TextBoxControl.Text.Length == 1 && TextBoxControl.Text[0] == '-')
		{
			Mode = ChatMode.UOAMChat;
		}
		if (ProfileManager.CurrentProfile.SpeechHue != TextBoxControl.Hue)
		{
			TextBoxControl.Hue = ProfileManager.CurrentProfile.SpeechHue;
		}
		_trans.Alpha = ((ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.HideChatGradient) ? 0f : 0.5f);
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		int num = TextBoxControl.Y + y - 20;
		LinkedListNode<ChatLineTime> linkedListNode = _textEntries.Last;
		while (linkedListNode != null)
		{
			LinkedListNode<ChatLineTime>? previous = linkedListNode.Previous;
			if (linkedListNode.Value.IsDisposed)
			{
				_textEntries.Remove(linkedListNode);
			}
			else
			{
				num -= linkedListNode.Value.TextHeight;
				if (num >= y)
				{
					linkedListNode.Value.Draw(batcher, x + 2, num);
				}
			}
			linkedListNode = previous;
		}
		return base.Draw(batcher, x, y);
	}

	protected override void OnKeyDown(SDL.SDL_Keycode key, SDL.SDL_Keymod mod)
	{
		switch (key)
		{
		case SDL.SDL_Keycode.SDLK_q:
		{
			if (!Keyboard.Ctrl || _messageHistoryIndex <= -1 || ProfileManager.CurrentProfile.DisableCtrlQWBtn)
			{
				break;
			}
			GameScene scene = Client.Game.GetScene<GameScene>();
			if (scene != null && scene.Macros.FindMacro(key, alt: false, ctrl: true, shift: false) == null && IsActive)
			{
				if (_messageHistoryIndex > 0)
				{
					_messageHistoryIndex--;
				}
				Mode = _messageHistory[_messageHistoryIndex].Item1;
				TextBoxControl.SetText(_messageHistory[_messageHistoryIndex].Item2);
			}
			break;
		}
		case SDL.SDL_Keycode.SDLK_w:
		{
			if (!Keyboard.Ctrl || ProfileManager.CurrentProfile.DisableCtrlQWBtn)
			{
				break;
			}
			GameScene scene = Client.Game.GetScene<GameScene>();
			if (scene != null && scene.Macros.FindMacro(key, alt: false, ctrl: true, shift: false) == null && IsActive)
			{
				if (_messageHistoryIndex < _messageHistory.Count - 1)
				{
					_messageHistoryIndex++;
					Mode = _messageHistory[_messageHistoryIndex].Item1;
					TextBoxControl.SetText(_messageHistory[_messageHistoryIndex].Item2);
				}
				else
				{
					TextBoxControl.ClearText();
				}
			}
			break;
		}
		case SDL.SDL_Keycode.SDLK_BACKSPACE:
			if (!Keyboard.Ctrl && !Keyboard.Alt && !Keyboard.Shift && string.IsNullOrEmpty(TextBoxControl.Text))
			{
				Mode = ChatMode.Default;
			}
			break;
		case SDL.SDL_Keycode.SDLK_ESCAPE:
			if (MessageManager.PromptData.Prompt != 0)
			{
				if (MessageManager.PromptData.Prompt == ConsolePrompt.ASCII)
				{
					NetClient.Socket.Send_ASCIIPromptResponse(string.Empty, cancel: true);
				}
				else if (MessageManager.PromptData.Prompt == ConsolePrompt.Unicode)
				{
					NetClient.Socket.Send_UnicodePromptResponse(string.Empty, Settings.GlobalSettings.Language, cancel: true);
				}
				MessageManager.PromptData = default(PromptData);
			}
			break;
		}
	}

	public override void OnKeyboardReturn(int textID, string text)
	{
		if ((!IsActive && ProfileManager.CurrentProfile.ActivateChatAfterEnter) || (Mode != 0 && string.IsNullOrEmpty(text)))
		{
			TextBoxControl.ClearText();
			text = string.Empty;
			Mode = ChatMode.Default;
		}
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		ChatMode mode = Mode;
		TextBoxControl.ClearText();
		_messageHistory.Add(new Tuple<ChatMode, string>(Mode, text));
		_messageHistoryIndex = _messageHistory.Count;
		Mode = ChatMode.Default;
		if (MessageManager.PromptData.Prompt != 0)
		{
			if (MessageManager.PromptData.Prompt == ConsolePrompt.ASCII)
			{
				NetClient.Socket.Send_ASCIIPromptResponse(text, text.Length < 1);
			}
			else if (MessageManager.PromptData.Prompt == ConsolePrompt.Unicode)
			{
				NetClient.Socket.Send_UnicodePromptResponse(text, Settings.GlobalSettings.Language, text.Length < 1);
			}
			MessageManager.PromptData = default(PromptData);
		}
		else
		{
			switch (mode)
			{
			case ChatMode.Default:
				GameActions.Say(text, ProfileManager.CurrentProfile.SpeechHue, MessageType.Regular, 3);
				break;
			case ChatMode.Whisper:
				GameActions.Say(text, ProfileManager.CurrentProfile.WhisperHue, MessageType.Whisper, 3);
				break;
			case ChatMode.Staff:
				GameActions.Say(text, ProfileManager.CurrentProfile.WhisperHue, MessageType.Whisper, 3);
				break;
			case ChatMode.Emote:
				text = ResGeneral.EmoteChar + text + ResGeneral.EmoteChar;
				GameActions.Say(text, ProfileManager.CurrentProfile.EmoteHue, MessageType.Emote, 3);
				break;
			case ChatMode.Yell:
				GameActions.Say(text, ProfileManager.CurrentProfile.YellHue, MessageType.Yell, 3);
				break;
			case ChatMode.Party:
				switch (text.ToLower())
				{
				case "add":
					GameActions.RequestPartyInviteByTarget();
					break;
				case "loot":
					if (World.Party.Leader != 0)
					{
						World.Party.CanLoot = !World.Party.CanLoot;
					}
					else
					{
						MessageManager.HandleMessage(null, ResGumps.YouAreNotInAParty, "System", ushort.MaxValue, MessageType.Regular, 3, TextType.SYSTEM);
					}
					break;
				case "quit":
					if (World.Party.Leader == 0)
					{
						MessageManager.HandleMessage(null, ResGumps.YouAreNotInAParty, "System", ushort.MaxValue, MessageType.Regular, 3, TextType.SYSTEM);
					}
					else
					{
						GameActions.RequestPartyQuit();
					}
					break;
				case "accept":
					if (World.Party.Leader == 0 && World.Party.Inviter != 0)
					{
						GameActions.RequestPartyAccept(World.Party.Inviter);
						World.Party.Leader = World.Party.Inviter;
						World.Party.Inviter = 0u;
					}
					else
					{
						MessageManager.HandleMessage(null, ResGumps.NoOneHasInvitedYouToBeInAParty, "System", ushort.MaxValue, MessageType.Regular, 3, TextType.SYSTEM);
					}
					break;
				case "decline":
					if (World.Party.Leader == 0 && World.Party.Inviter != 0)
					{
						NetClient.Socket.Send_PartyDecline(World.Party.Inviter);
						World.Party.Leader = 0u;
						World.Party.Inviter = 0u;
					}
					else
					{
						MessageManager.HandleMessage(null, ResGumps.NoOneHasInvitedYouToBeInAParty, "System", ushort.MaxValue, MessageType.Regular, 3, TextType.SYSTEM);
					}
					break;
				case "rem":
					if (World.Party.Leader != 0 && World.Party.Leader == (uint)World.Player)
					{
						GameActions.RequestPartyRemoveMemberByTarget();
					}
					else
					{
						MessageManager.HandleMessage(null, ResGumps.YouAreNotPartyLeader, "System", ushort.MaxValue, MessageType.Regular, 3, TextType.SYSTEM);
					}
					break;
				default:
					if (World.Party.Leader != 0)
					{
						uint serial = 0u;
						int i;
						for (i = 0; i < text.Length && text[i] != ' '; i++)
						{
						}
						if (i < text.Length && int.TryParse(text.Substring(0, i), out var result) && result > 0 && result < 31 && World.Party.Members[result - 1] != null && World.Party.Members[result - 1].Serial != 0)
						{
							serial = World.Party.Members[result - 1].Serial;
						}
						GameActions.SayParty(text, serial);
					}
					else
					{
						GameActions.Print(string.Format(ResGumps.NoteToSelf0, text), 0, MessageType.System, 3, unicode: false);
					}
					break;
				}
				break;
			case ChatMode.Guild:
				GameActions.Say(text, ProfileManager.CurrentProfile.GuildMessageHue, MessageType.Guild, 3);
				break;
			case ChatMode.Alliance:
				GameActions.Say(text, ProfileManager.CurrentProfile.AllyMessageHue, MessageType.Alliance, 3);
				break;
			case ChatMode.ClientCommand:
			{
				string[] array = text.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if (array.Length != 0)
				{
					CommandManager.Execute(array[0], array);
				}
				break;
			}
			case ChatMode.UOAMChat:
				UoAssist.SignalMessage(text);
				break;
			case ChatMode.UOChat:
				NetClient.Socket.Send_ChatMessageCommand(text);
				break;
			}
		}
		DisposeChatModePrefix();
	}
}

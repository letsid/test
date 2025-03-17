using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ClassicUO.Configuration;
using ClassicUO.Data;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.Game.UI.Gumps.CharCreation;
using ClassicUO.Game.UI.Gumps.Login;
using ClassicUO.IO;
using ClassicUO.IO.Resources;
using ClassicUO.Network;
using ClassicUO.Network.Encryption;
using ClassicUO.Resources;
using ClassicUO.Utility;
using ClassicUO.Utility.Logging;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.Scenes;

internal sealed class LoginScene : Scene
{
	private Gump _currentGump;

	private LoginSteps _lastLoginStep;

	private uint _pingTime;

	private long _reconnectTime;

	private int _reconnectTryCounter = 1;

	private bool _autoLogin;

	public bool Reconnect { get; set; }

	public LoginSteps CurrentLoginStep { get; set; }

	public ServerListEntry[] Servers { get; private set; }

	public CityInfo[] Cities { get; set; }

	public string[] Characters { get; private set; }

	public string PopupMessage { get; set; }

	public byte ServerIndex { get; private set; }

	public static string Account { get; private set; }

	public string Password { get; private set; }

	public bool CanAutologin
	{
		get
		{
			if (!_autoLogin)
			{
				return Reconnect;
			}
			return true;
		}
	}

	public LoginScene()
		: base(1, canresize: false, maximized: false, loadaudio: true)
	{
	}

	public override void Load()
	{
		base.Load();
		_autoLogin = Settings.GlobalSettings.AutoLogin;
		UIManager.Add(new LoginBackground());
		UIManager.Add(_currentGump = new LoginGump(this));
		NetClient.Socket.Disconnected += NetClient_Disconnected;
		NetClient.LoginSocket.Connected += NetClient_Connected;
		NetClient.LoginSocket.Disconnected += Login_NetClient_Disconnected;
		base.Audio.PlayMusic(base.Audio.LoginMusicIndex, iswarmode: false, is_login: true);
		if (((CanAutologin && CurrentLoginStep != 0) || CUOEnviroment.SkipLoginScreen) && !string.IsNullOrEmpty(Settings.GlobalSettings.Username))
		{
			CUOEnviroment.SkipLoginScreen = false;
			Connect(Settings.GlobalSettings.Username, Crypter.Decrypt(Settings.GlobalSettings.Password));
		}
		if (Client.Game.IsWindowMaximized())
		{
			Client.Game.RestoreWindow();
		}
		Client.Game.SetWindowSize(640, 480);
	}

	public override void Unload()
	{
		UIManager.GetGump<LoginBackground>(null)?.Dispose();
		_currentGump?.Dispose();
		NetClient.Socket.Disconnected -= NetClient_Disconnected;
		NetClient.LoginSocket.Connected -= NetClient_Connected;
		NetClient.LoginSocket.Disconnected -= Login_NetClient_Disconnected;
		UIManager.GameCursor.IsLoading = false;
		base.Unload();
	}

	public override void Update(double totalTime, double frameTime)
	{
		base.Update(totalTime, frameTime);
		if (_lastLoginStep != CurrentLoginStep)
		{
			UIManager.GameCursor.IsLoading = false;
			Gump currentGump = _currentGump;
			UIManager.Add(_currentGump = GetGumpForStep());
			currentGump.Dispose();
			_lastLoginStep = CurrentLoginStep;
		}
		if (Reconnect && (CurrentLoginStep == LoginSteps.PopUpMessage || CurrentLoginStep == LoginSteps.Main) && !NetClient.Socket.IsConnected && !NetClient.LoginSocket.IsConnected && (double)_reconnectTime < totalTime)
		{
			if (!string.IsNullOrEmpty(Account))
			{
				Connect(Account, Crypter.Decrypt(Settings.GlobalSettings.Password));
			}
			else if (!string.IsNullOrEmpty(Settings.GlobalSettings.Username))
			{
				Connect(Settings.GlobalSettings.Username, Crypter.Decrypt(Settings.GlobalSettings.Password));
			}
			int num = Settings.GlobalSettings.ReconnectTime * 1000;
			if (num < 1000)
			{
				num = 1000;
			}
			_reconnectTime = (long)totalTime + num;
			_reconnectTryCounter++;
		}
		if (!CUOEnviroment.NoServerPing && (CurrentLoginStep == LoginSteps.CharacterCreation || CurrentLoginStep == LoginSteps.CharacterSelection) && Time.Ticks > _pingTime)
		{
			if (NetClient.Socket != null && NetClient.Socket.IsConnected)
			{
				NetClient.Socket.Statistics.SendPing();
			}
			else if (NetClient.LoginSocket != null && NetClient.LoginSocket.IsConnected)
			{
				NetClient.LoginSocket.Statistics.SendPing();
			}
			_pingTime = Time.Ticks + 60000;
		}
	}

	private Gump GetGumpForStep()
	{
		foreach (Item value in World.Items.Values)
		{
			World.RemoveItem(value);
		}
		foreach (Mobile value2 in World.Mobiles.Values)
		{
			World.RemoveMobile(value2);
		}
		World.Mobiles.Clear();
		World.Items.Clear();
		switch (CurrentLoginStep)
		{
		case LoginSteps.Main:
			PopupMessage = null;
			return new LoginGump(this);
		case LoginSteps.Connecting:
		case LoginSteps.VerifyingAccount:
		case LoginSteps.LoginInToServer:
		case LoginSteps.EnteringBritania:
		case LoginSteps.CharacterCreationDone:
		case LoginSteps.PopUpMessage:
			UIManager.GameCursor.IsLoading = CurrentLoginStep != LoginSteps.PopUpMessage;
			return GetLoadingScreen();
		case LoginSteps.CharacterSelection:
			return new CharacterSelectionGump();
		case LoginSteps.ServerSelection:
			_pingTime = Time.Ticks + 60000;
			if (Servers.Length != 0)
			{
				SelectServer(0);
			}
			return GetLoadingScreen();
		case LoginSteps.CharacterCreation:
			_pingTime = Time.Ticks + 60000;
			return new CharCreationGump(this);
		default:
			return null;
		}
	}

	private LoadingGump GetLoadingScreen()
	{
		string labelText = "No Text";
		LoginButtons showButtons = LoginButtons.None;
		if (!string.IsNullOrEmpty(PopupMessage))
		{
			labelText = PopupMessage;
			showButtons = LoginButtons.OK;
			PopupMessage = null;
		}
		else
		{
			switch (CurrentLoginStep)
			{
			case LoginSteps.Connecting:
				labelText = ClilocLoader.Instance.GetString(3000002, ResGeneral.Connecting);
				break;
			case LoginSteps.VerifyingAccount:
				labelText = ClilocLoader.Instance.GetString(3000003, ResGeneral.VerifyingAccount);
				showButtons = LoginButtons.Cancel;
				break;
			case LoginSteps.LoginInToServer:
				labelText = ClilocLoader.Instance.GetString(3000053, ResGeneral.LoggingIntoShard);
				break;
			case LoginSteps.EnteringBritania:
				labelText = ClilocLoader.Instance.GetString(3000001, ResGeneral.EnteringBritannia);
				break;
			case LoginSteps.CharacterCreationDone:
				labelText = ResGeneral.CreatingCharacter;
				break;
			}
		}
		return new LoadingGump(labelText, showButtons, OnLoadingGumpButtonClick);
	}

	private void OnLoadingGumpButtonClick(int buttonId)
	{
		if (buttonId == 2 || buttonId == 4)
		{
			StepBack();
		}
	}

	public async void Connect(string account, string password)
	{
		if (CurrentLoginStep != LoginSteps.Connecting)
		{
			Account = account;
			Password = password;
			if (Settings.GlobalSettings.SaveAccount)
			{
				Settings.GlobalSettings.Username = Account;
				Settings.GlobalSettings.Password = Crypter.Encrypt(Password);
				Settings.GlobalSettings.Save();
			}
			Log.Trace($"Start login to: {Settings.GlobalSettings.IP},{Settings.GlobalSettings.Port}");
			if (!Reconnect)
			{
				CurrentLoginStep = LoginSteps.Connecting;
			}
			if (!(await NetClient.LoginSocket.Connect(Settings.GlobalSettings.IP, Settings.GlobalSettings.Port)))
			{
				PopupMessage = ResGeneral.CheckYourConnectionAndTryAgain;
				CurrentLoginStep = LoginSteps.PopUpMessage;
				Log.Error("No Internet Access");
			}
		}
	}

	public int GetServerIndexByName(string name)
	{
		if (!string.IsNullOrWhiteSpace(name))
		{
			for (int i = 0; i < Servers.Length; i++)
			{
				if (Servers[i].Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
				{
					return i;
				}
			}
		}
		return -1;
	}

	public int GetServerIndexFromSettings()
	{
		string lastServerName = Settings.GlobalSettings.LastServerName;
		int num = GetServerIndexByName(lastServerName);
		if (num == -1)
		{
			num = Settings.GlobalSettings.LastServerNum;
		}
		if (num < 0 || num >= Servers.Length)
		{
			num = 0;
		}
		return num;
	}

	public void SelectServer(byte index)
	{
		if (CurrentLoginStep != LoginSteps.ServerSelection)
		{
			return;
		}
		for (byte b = 0; b < Servers.Length; b++)
		{
			if (Servers[b].Index == index)
			{
				ServerIndex = b;
				break;
			}
		}
		Settings.GlobalSettings.LastServerNum = (ushort)(1 + ServerIndex);
		Settings.GlobalSettings.LastServerName = Servers[ServerIndex].Name;
		Settings.GlobalSettings.Save();
		CurrentLoginStep = LoginSteps.LoginInToServer;
		World.ServerName = Servers[ServerIndex].Name;
		NetClient.LoginSocket.Send_SelectServer(index);
	}

	public void SelectCharacter(uint index)
	{
		if (CurrentLoginStep == LoginSteps.CharacterSelection)
		{
			LastCharacterManager.Save(Account, World.ServerName, Characters[index]);
			CurrentLoginStep = LoginSteps.EnteringBritania;
			NetClient.Socket.Send_SelectCharacter(index, Characters[index], NetClient.Socket.LocalIP);
		}
	}

	public void StartCharCreation()
	{
		if (CurrentLoginStep == LoginSteps.CharacterSelection)
		{
			CurrentLoginStep = LoginSteps.CharacterCreation;
		}
	}

	public void CreateCharacter(PlayerMobile character)
	{
		int i;
		for (i = 0; i < Characters.Length && !string.IsNullOrEmpty(Characters[i]); i++)
		{
		}
		character.Name = character.Name.TrimEnd();
		LastCharacterManager.Save(Account, World.ServerName, character.Name);
		NetClient.Socket.Send_CreateCharacter(character, 0, NetClient.Socket.LocalIP, ServerIndex, (uint)i, 0);
		CurrentLoginStep = LoginSteps.CharacterCreationDone;
	}

	public void DeleteCharacter(uint index)
	{
		if (CurrentLoginStep == LoginSteps.CharacterSelection)
		{
			NetClient.Socket.Send_DeleteCharacter((byte)index, NetClient.Socket.LocalIP);
		}
	}

	public void StepBack()
	{
		PopupMessage = null;
		if (Characters != null && CurrentLoginStep != LoginSteps.CharacterCreation)
		{
			CurrentLoginStep = LoginSteps.LoginInToServer;
		}
		switch (CurrentLoginStep)
		{
		case LoginSteps.Connecting:
		case LoginSteps.VerifyingAccount:
		case LoginSteps.ServerSelection:
			DisposeAllServerEntries();
			CurrentLoginStep = LoginSteps.Main;
			NetClient.LoginSocket.Disconnect();
			break;
		case LoginSteps.LoginInToServer:
			DisposeAllServerEntries();
			CurrentLoginStep = LoginSteps.Main;
			NetClient.LoginSocket.Disconnect();
			NetClient.Socket.Disconnect();
			Characters = null;
			break;
		case LoginSteps.CharacterCreation:
			CurrentLoginStep = LoginSteps.CharacterSelection;
			break;
		case LoginSteps.CharacterSelection:
		case LoginSteps.PopUpMessage:
			NetClient.LoginSocket.Disconnect();
			NetClient.Socket.Disconnect();
			Characters = null;
			DisposeAllServerEntries();
			CurrentLoginStep = LoginSteps.Main;
			break;
		case LoginSteps.EnteringBritania:
		case LoginSteps.CharacterCreationDone:
			break;
		}
	}

	public CityInfo GetCity(int index)
	{
		if (index < Cities.Length)
		{
			return Cities[index];
		}
		return null;
	}

	private void NetClient_Connected(object sender, EventArgs e)
	{
		Log.Info("Connected!");
		CurrentLoginStep = LoginSteps.VerifyingAccount;
		uint localIP = NetClient.LoginSocket.LocalIP;
		EncryptionHelper.Initialize(is_login: true, localIP, (ENCRYPTION_TYPE)Settings.GlobalSettings.Encryption);
		if (Client.Version >= ClientVersion.CV_6040)
		{
			ClientVersion version = Client.Version;
			byte major = (byte)((uint)version >> 24);
			byte minor = (byte)((uint)version >> 16);
			byte build = (byte)((uint)version >> 8);
			byte extra = (byte)version;
			NetClient.LoginSocket.Send_Seed(localIP, major, minor, build, extra);
		}
		else
		{
			NetClient.LoginSocket.Send_Seed_Old(localIP);
		}
		NetClient.LoginSocket.Send_FirstLogin(Account, Password);
	}

	private void NetClient_Disconnected(object sender, SocketError e)
	{
		Log.Warn("Disconnected (game socket)!");
		if (CurrentLoginStep != LoginSteps.CharacterCreation)
		{
			Characters = null;
			DisposeAllServerEntries();
			PopupMessage = string.Format(ResGeneral.ConnectionLost0, StringHelper.AddSpaceBeforeCapital(e.ToString()));
			CurrentLoginStep = LoginSteps.PopUpMessage;
		}
	}

	private void Login_NetClient_Disconnected(object sender, SocketError e)
	{
		Log.Warn("Disconnected (login socket)!");
		if (e != 0)
		{
			Characters = null;
			DisposeAllServerEntries();
			if (Settings.GlobalSettings.Reconnect)
			{
				Reconnect = true;
				PopupMessage = string.Format(ResGeneral.ReconnectPleaseWait01, _reconnectTryCounter, StringHelper.AddSpaceBeforeCapital(e.ToString()));
				UIManager.GetGump<LoadingGump>(null)?.SetText(PopupMessage);
			}
			else
			{
				PopupMessage = string.Format(ResGeneral.ConnectionLost0, StringHelper.AddSpaceBeforeCapital(e.ToString()));
			}
			CurrentLoginStep = LoginSteps.PopUpMessage;
		}
	}

	public void ServerListReceived(ref StackDataReader p)
	{
		p.ReadUInt8();
		ushort num = p.ReadUInt16BE();
		DisposeAllServerEntries();
		Servers = new ServerListEntry[num];
		for (ushort num2 = 0; num2 < num; num2++)
		{
			Servers[num2] = ServerListEntry.Create(ref p);
		}
		CurrentLoginStep = LoginSteps.ServerSelection;
		if (CanAutologin && Servers.Length != 0)
		{
			int serverIndexFromSettings = GetServerIndexFromSettings();
			SelectServer((byte)Servers[serverIndexFromSettings].Index);
		}
	}

	public void UpdateCharacterList(ref StackDataReader p)
	{
		ParseCharacterList(ref p);
		if (CurrentLoginStep != LoginSteps.PopUpMessage)
		{
			PopupMessage = null;
		}
		CurrentLoginStep = LoginSteps.CharacterSelection;
		UIManager.GetGump<CharacterSelectionGump>(null)?.Dispose();
		_currentGump?.Dispose();
		UIManager.Add(_currentGump = new CharacterSelectionGump());
		if (!string.IsNullOrWhiteSpace(PopupMessage))
		{
			Gump g = null;
			g = new LoadingGump(PopupMessage, LoginButtons.OK, delegate
			{
				g.Dispose();
			})
			{
				IsModal = true
			};
			UIManager.Add(g);
			PopupMessage = null;
		}
	}

	public void ReceiveCharacterList(ref StackDataReader p)
	{
		ParseCharacterList(ref p);
		ParseCities(ref p);
		World.ClientFeatures.SetFlags((CharacterListFlags)p.ReadUInt32BE());
		CurrentLoginStep = LoginSteps.CharacterSelection;
		uint index = 0u;
		bool flag = false;
		bool canAutologin = CanAutologin;
		if (_autoLogin)
		{
			_autoLogin = false;
		}
		string lastCharacter = LastCharacterManager.GetLastCharacter(Account, World.ServerName);
		for (byte b = 0; b < Characters.Length; b++)
		{
			if (Characters[b].Length > 0)
			{
				flag = true;
				if (Characters[b] == lastCharacter)
				{
					index = b;
					break;
				}
			}
		}
		if (canAutologin && flag)
		{
			SelectCharacter(index);
		}
		else if (!flag)
		{
			StartCharCreation();
		}
	}

	public void HandleErrorCode(ref StackDataReader p)
	{
		byte code = p.ReadUInt8();
		PopupMessage = ServerErrorMessages.GetError(p[0], code);
		CurrentLoginStep = LoginSteps.PopUpMessage;
	}

	public void HandleRelayServerPacket(ref StackDataReader p)
	{
		long newAddress = p.ReadUInt32LE();
		ushort port = p.ReadUInt16BE();
		uint seed = p.ReadUInt32BE();
		NetClient.LoginSocket.Disconnect();
		EncryptionHelper.Initialize(is_login: false, seed, (ENCRYPTION_TYPE)Settings.GlobalSettings.Encryption);
		NetClient.Socket.Connect(new IPAddress(newAddress), port).ContinueWith(delegate(Task<bool> t)
		{
			if (!t.IsFaulted)
			{
				NetClient.Socket.EnableCompression();
				Span<byte> span = stackalloc byte[4]
				{
					(byte)(seed >> 24),
					(byte)(seed >> 16),
					(byte)(seed >> 8),
					(byte)seed
				};
				StackDataWriter stackDataWriter = new StackDataWriter(span);
				NetClient.Socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten, ignorePlugin: true, skip_encryption: true);
				stackDataWriter.Dispose();
				NetClient.Socket.Send_SecondLogin(Account, Password, seed);
			}
		}, TaskContinuationOptions.ExecuteSynchronously);
	}

	private void ParseCharacterList(ref StackDataReader p)
	{
		int num = p.ReadUInt8();
		Characters = new string[num];
		for (ushort num2 = 0; num2 < num; num2++)
		{
			Characters[num2] = p.ReadASCII(30).TrimEnd('\0');
			p.Skip(30);
		}
	}

	private void ParseCities(ref StackDataReader p)
	{
		byte b = p.ReadUInt8();
		Cities = new CityInfo[b];
		bool flag = Client.Version >= ClientVersion.CV_70130;
		string[] array = null;
		if (!flag)
		{
			array = ReadCityTextFile(b);
		}
		Point[] array2 = new Point[10]
		{
			new Point(105, 130),
			new Point(245, 90),
			new Point(165, 200),
			new Point(395, 160),
			new Point(200, 305),
			new Point(335, 250),
			new Point(160, 395),
			new Point(100, 250),
			new Point(270, 130),
			new Point(65535, 65535)
		};
		for (int i = 0; i < b; i++)
		{
			CityInfo cityInfo;
			if (flag)
			{
				byte index = p.ReadUInt8();
				string city = p.ReadASCII(32);
				string building = p.ReadASCII(32);
				ushort x = (ushort)p.ReadUInt32BE();
				ushort y = (ushort)p.ReadUInt32BE();
				sbyte z = (sbyte)p.ReadUInt32BE();
				uint map = p.ReadUInt32BE();
				uint number = p.ReadUInt32BE();
				p.Skip(4);
				cityInfo = new CityInfo(index, city, building, ClilocLoader.Instance.GetString((int)number), x, y, z, map, flag);
			}
			else
			{
				byte index2 = p.ReadUInt8();
				string city2 = p.ReadASCII(31);
				string building2 = p.ReadASCII(31);
				cityInfo = new CityInfo(index2, city2, building2, (array != null) ? array[i] : string.Empty, (ushort)array2[i % array2.Length].X, (ushort)array2[i % array2.Length].Y, 0, 0u, flag);
			}
			Cities[i] = cityInfo;
		}
	}

	private string[] ReadCityTextFile(int count)
	{
		string uOFilePath = UOFileManager.GetUOFilePath("citytext.enu");
		if (!File.Exists(uOFilePath))
		{
			return null;
		}
		string[] array = new string[count];
		byte[] array2 = new byte[4];
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		using (FileStream fileStream = File.OpenRead(uOFilePath))
		{
			int num = 0;
			while (fileStream.Position < fileStream.Length && fileStream.Read(array2, 0, 4) != -1)
			{
				if (Encoding.UTF8.GetString(array2, 0, 4) == "END\0")
				{
					stringBuilder.Clear();
					while (fileStream.Position < fileStream.Length)
					{
						char c = (char)fileStream.ReadByte();
						if (c == '<')
						{
							fileStream.Position--;
							break;
						}
						stringBuilder.Append(c);
					}
					stringBuilder2.Clear();
					while (fileStream.Position < fileStream.Length)
					{
						char value;
						while ((value = (char)fileStream.ReadByte()) != 0)
						{
							stringBuilder2.Append(value);
						}
						if (stringBuilder2.Length != 0)
						{
							string value2 = stringBuilder2?.ToString() + "\n\n";
							stringBuilder2.Clear();
							stringBuilder2.Append(value2);
						}
						long position = fileStream.Position;
						byte num2 = (byte)fileStream.ReadByte();
						fileStream.Position = position;
						if (num2 == 46)
						{
							break;
						}
						int num3 = fileStream.Read(array2, 0, 4);
						fileStream.Position = position;
						if (num3 == -1 || Encoding.UTF8.GetString(array2, 0, 4) == "END\0")
						{
							break;
						}
					}
					if (array.Length <= num)
					{
						break;
					}
					array[num++] = stringBuilder2.ToString();
				}
				else
				{
					fileStream.Position -= 3L;
				}
			}
		}
		return array;
	}

	private void DisposeAllServerEntries()
	{
		if (Servers == null)
		{
			return;
		}
		for (int i = 0; i < Servers.Length; i++)
		{
			if (Servers[i] != null)
			{
				Servers[i].Dispose();
				Servers[i] = null;
			}
		}
		Servers = null;
	}
}

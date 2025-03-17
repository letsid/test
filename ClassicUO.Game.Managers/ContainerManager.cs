using System.Collections.Generic;
using System.IO;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.IO.Resources;
using ClassicUO.Utility;

namespace ClassicUO.Game.Managers;

internal static class ContainerManager
{
	private static readonly Dictionary<ushort, ContainerData> _data;

	public static int DefaultX { get; }

	public static int DefaultY { get; }

	public static int X { get; private set; }

	public static int Y { get; private set; }

	static ContainerManager()
	{
		_data = new Dictionary<ushort, ContainerData>();
		DefaultX = 40;
		DefaultY = 40;
		X = 40;
		Y = 40;
		BuildContainerFile(force: false);
	}

	public static ContainerData Get(ushort graphic)
	{
		if (!_data.TryGetValue(graphic, out var value))
		{
			value = (_data[graphic] = new ContainerData(graphic, 0, 0, 44, 65, 186, 159, 0));
		}
		return value;
	}

	public static void CalculateContainerPosition(uint serial, ushort g)
	{
		if (UIManager.GetGumpCachePosition(serial, out var pos))
		{
			X = pos.X;
			Y = pos.Y;
		}
		else
		{
			if (GumpsLoader.Instance.GetGumpTexture(g, out var bounds) == null)
			{
				return;
			}
			float containerScale = UIManager.ContainerScale;
			int num = (int)((float)bounds.Width * containerScale);
			int num2 = (int)((float)bounds.Height * containerScale);
			if (ProfileManager.CurrentProfile.OverrideContainerLocation)
			{
				switch (ProfileManager.CurrentProfile.OverrideContainerLocationSetting)
				{
				case 0:
					SetPositionNearGameObject(g, serial, num, num2);
					break;
				case 1:
					X = Client.Game.Window.ClientBounds.Width - num;
					Y = 0;
					break;
				case 2:
				case 3:
					X = ProfileManager.CurrentProfile.OverrideContainerLocationPosition.X - (num >> 1);
					Y = ProfileManager.CurrentProfile.OverrideContainerLocationPosition.Y - (num2 >> 1);
					break;
				}
				if (X + num > Client.Game.Window.ClientBounds.Width)
				{
					X -= num;
				}
				if (Y + num2 > Client.Game.Window.ClientBounds.Height)
				{
					Y -= num2;
				}
				return;
			}
			int num3 = 0;
			for (int i = 0; i < 4; i++)
			{
				if (num3 != 0)
				{
					break;
				}
				if (X + num + 20 > Client.Game.Window.ClientBounds.Width)
				{
					X = 40;
					if (Y + num2 + 800 > Client.Game.Window.ClientBounds.Height)
					{
						Y = 40;
					}
					else
					{
						Y += 800;
					}
				}
				else if (Y + num2 + 20 > Client.Game.Window.ClientBounds.Height)
				{
					if (X + num + 800 > Client.Game.Window.ClientBounds.Width)
					{
						X = 40;
					}
					else
					{
						X += 800;
					}
					Y = 40;
				}
				else
				{
					num3 = i + 1;
				}
			}
			switch (num3)
			{
			case 0:
				X = DefaultX;
				Y = DefaultY;
				break;
			case 1:
				X += 20;
				Y += 20;
				break;
			}
		}
	}

	private static void SetPositionNearGameObject(ushort g, uint serial, int width, int height)
	{
		Item item = World.Items.Get(serial);
		if (item == null)
		{
			return;
		}
		Item item2 = World.Player.FindItemByLayer(Layer.Bank);
		if (item2 != null && serial == (uint)item2)
		{
			X = World.Player.RealScreenPosition.X + ProfileManager.CurrentProfile.GameWindowPosition.X + 40;
			Y = World.Player.RealScreenPosition.Y + ProfileManager.CurrentProfile.GameWindowPosition.Y - (height >> 1);
		}
		else if (item.OnGround)
		{
			X = item.RealScreenPosition.X + ProfileManager.CurrentProfile.GameWindowPosition.X + 40;
			Y = item.RealScreenPosition.Y + ProfileManager.CurrentProfile.GameWindowPosition.Y - (height >> 1);
		}
		else if (SerialHelper.IsMobile(item.Container))
		{
			Mobile mobile = World.Mobiles.Get(item.Container);
			if (mobile != null)
			{
				X = mobile.RealScreenPosition.X + ProfileManager.CurrentProfile.GameWindowPosition.X + 40;
				Y = mobile.RealScreenPosition.Y + ProfileManager.CurrentProfile.GameWindowPosition.Y - (height >> 1);
			}
		}
		else
		{
			ContainerGump gump = UIManager.GetGump<ContainerGump>(item.Container);
			if (gump != null)
			{
				X = gump.X + (width >> 1);
				Y = gump.Y;
			}
		}
	}

	public static void BuildContainerFile(bool force)
	{
		string text = Path.Combine(CUOEnviroment.ExecutablePath, "Data", "Client");
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		text = Path.Combine(text, "containers.txt");
		if (!File.Exists(text) || force)
		{
			MakeDefault();
			using StreamWriter streamWriter = new StreamWriter(File.Create(text));
			streamWriter.WriteLine("# FORMAT");
			streamWriter.WriteLine("# GRAPHIC OPEN_SOUND_ID CLOSE_SOUND_ID LEFT TOP RIGHT BOTTOM ICONIZED_GRAPHIC [0 if not exists] MINIMIZER_AREA_X [0 if not exists] MINIMIZER_AREA_Y [0 if not exists]");
			streamWriter.WriteLine("# LEFT = X,  TOP = Y,  RIGHT = X + WIDTH,  BOTTOM = Y + HEIGHT");
			streamWriter.WriteLine();
			streamWriter.WriteLine();
			foreach (KeyValuePair<ushort, ContainerData> datum in _data)
			{
				streamWriter.WriteLine($"{datum.Value.Graphic} {datum.Value.OpenSound} {datum.Value.ClosedSound} {datum.Value.Bounds.X} {datum.Value.Bounds.Y} {datum.Value.Bounds.Width} {datum.Value.Bounds.Height} {datum.Value.IconizedGraphic} {datum.Value.MinimizerArea.X} {datum.Value.MinimizerArea.Y} {datum.Value.UncoloredHue} {datum.Value.Flags}");
			}
		}
		_data.Clear();
		TextFileParser textFileParser = new TextFileParser(File.ReadAllText(text), new char[3] { ' ', '\t', ',' }, new char[2] { '#', ';' }, new char[2] { '"', '"' });
		while (!textFileParser.IsEOF())
		{
			List<string> list = textFileParser.ReadTokens();
			if (list != null && list.Count != 0 && ushort.TryParse(list[0], out var result) && ushort.TryParse(list[1], out var result2) && ushort.TryParse(list[2], out var result3) && int.TryParse(list[3], out var result4) && int.TryParse(list[4], out var result5) && int.TryParse(list[5], out var result6) && int.TryParse(list[6], out var result7))
			{
				ushort result8 = 0;
				int result9 = 0;
				int result10 = 0;
				int result11 = 0;
				int result12 = 0;
				if (list.Count >= 8 && ushort.TryParse(list[7], out result8) && list.Count >= 9 && int.TryParse(list[8], out result9) && list.Count >= 10 && int.TryParse(list[9], out result10) && list.Count >= 11 && int.TryParse(list[10], out result11) && list.Count >= 12)
				{
					int.TryParse(list[11], out result12);
				}
				_data[result] = new ContainerData(result, result2, result3, result4, result5, result6, result7, result8, result9, result10, result11, result12);
			}
		}
	}

	private static void MakeDefault()
	{
		_data.Clear();
		_data[7] = new ContainerData(7, 0, 0, 30, 30, 270, 170, 0);
		_data[9] = new ContainerData(9, 0, 0, 20, 85, 124, 196, 0);
		_data[60] = new ContainerData(60, 72, 88, 44, 65, 186, 159, 80, 105, 162);
		_data[61] = new ContainerData(61, 72, 88, 29, 34, 137, 128, 0);
		_data[62] = new ContainerData(62, 47, 46, 33, 36, 142, 148, 0);
		_data[63] = new ContainerData(63, 79, 88, 19, 47, 182, 123, 0);
		_data[64] = new ContainerData(64, 45, 44, 16, 38, 152, 125, 0);
		_data[65] = new ContainerData(65, 79, 88, 40, 30, 139, 123, 0);
		_data[66] = new ContainerData(66, 45, 44, 18, 105, 162, 178, 0);
		_data[67] = new ContainerData(67, 45, 44, 16, 51, 184, 124, 0);
		_data[68] = new ContainerData(68, 45, 44, 20, 10, 170, 100, 0);
		_data[71] = new ContainerData(71, 0, 0, 16, 10, 148, 138, 0);
		_data[72] = new ContainerData(72, 47, 46, 16, 10, 154, 94, 0);
		_data[73] = new ContainerData(73, 45, 44, 18, 105, 162, 178, 0);
		_data[74] = new ContainerData(74, 45, 44, 18, 105, 162, 178, 0);
		_data[75] = new ContainerData(75, 45, 44, 16, 51, 184, 124, 0);
		_data[76] = new ContainerData(76, 45, 44, 46, 74, 196, 184, 0);
		_data[77] = new ContainerData(77, 47, 46, 76, 12, 140, 68, 0);
		_data[78] = new ContainerData(78, 45, 44, 24, 18, 100, 152, 0);
		_data[79] = new ContainerData(79, 45, 44, 24, 18, 100, 152, 0);
		_data[81] = new ContainerData(81, 47, 46, 16, 10, 154, 94, 0);
		_data[82] = new ContainerData(82, 0, 0, 0, 0, 110, 62, 0);
		_data[258] = new ContainerData(258, 79, 88, 35, 10, 190, 95, 0);
		_data[259] = new ContainerData(259, 72, 88, 41, 21, 173, 104, 0);
		_data[260] = new ContainerData(260, 47, 46, 10, 10, 160, 105, 0);
		_data[261] = new ContainerData(261, 47, 46, 10, 10, 160, 105, 0);
		_data[262] = new ContainerData(262, 47, 46, 10, 10, 160, 105, 0);
		_data[263] = new ContainerData(263, 47, 46, 10, 10, 160, 105, 0);
		_data[264] = new ContainerData(264, 79, 88, 10, 10, 160, 105, 0);
		_data[265] = new ContainerData(265, 45, 44, 10, 10, 160, 105, 0);
		_data[266] = new ContainerData(266, 45, 44, 10, 10, 160, 105, 0);
		_data[267] = new ContainerData(267, 45, 44, 10, 10, 160, 105, 0);
		_data[268] = new ContainerData(268, 47, 46, 10, 10, 160, 105, 0);
		_data[269] = new ContainerData(269, 47, 46, 10, 10, 160, 105, 0);
		_data[270] = new ContainerData(270, 47, 46, 10, 10, 160, 105, 0);
		_data[278] = new ContainerData(278, 0, 0, 40, 25, 140, 110, 0);
		_data[282] = new ContainerData(282, 0, 0, 10, 65, 125, 160, 0);
		_data[283] = new ContainerData(283, 0, 0, 45, 10, 175, 95, 0);
		_data[284] = new ContainerData(284, 0, 0, 37, 10, 175, 105, 0);
		_data[285] = new ContainerData(285, 0, 0, 43, 10, 165, 110, 0);
		_data[286] = new ContainerData(286, 0, 0, 30, 22, 263, 106, 0);
		_data[287] = new ContainerData(287, 0, 0, 45, 10, 175, 95, 0);
		_data[288] = new ContainerData(288, 0, 0, 56, 30, 160, 107, 0);
		_data[289] = new ContainerData(289, 0, 0, 77, 32, 162, 107, 0);
		_data[291] = new ContainerData(291, 0, 0, 36, 19, 111, 157, 0);
		_data[1156] = new ContainerData(1156, 0, 0, 0, 45, 175, 125, 0);
		_data[1422] = new ContainerData(1422, 0, 0, 50, 150, 348, 250, 0);
		_data[1747] = new ContainerData(1747, 0, 0, 10, 65, 125, 160, 0);
		_data[1748] = new ContainerData(1748, 0, 0, 10, 65, 125, 160, 0);
		_data[1749] = new ContainerData(1749, 0, 0, 10, 65, 125, 160, 0);
		_data[1750] = new ContainerData(1750, 0, 0, 10, 65, 125, 160, 0);
		_data[1765] = new ContainerData(1765, 0, 0, 66, 74, 306, 520, 0);
		_data[1766] = new ContainerData(1766, 0, 0, 66, 74, 306, 520, 0);
		_data[1767] = new ContainerData(1767, 0, 0, 50, 60, 548, 308, 0);
		_data[1768] = new ContainerData(1768, 0, 0, 50, 60, 548, 308, 0);
		_data[1769] = new ContainerData(1769, 0, 0, 60, 80, 318, 324, 0);
		_data[1770] = new ContainerData(1770, 0, 0, 50, 60, 548, 308, 0);
		_data[2330] = new ContainerData(2330, 0, 0, 0, 0, 282, 230, 0);
		_data[2350] = new ContainerData(2350, 0, 0, 0, 0, 282, 210, 0);
		_data[9834] = new ContainerData(9834, 0, 0, 16, 51, 184, 124, 0);
		_data[9835] = new ContainerData(9835, 0, 0, 16, 51, 184, 124, 0);
		_data[10851] = new ContainerData(10851, 391, 457, 60, 33, 460, 348, 0);
		_data[19724] = new ContainerData(19724, 0, 0, 25, 65, 220, 155, 0);
		_data[30558] = new ContainerData(30558, 72, 88, 44, 65, 186, 159, 30559, 105, 178);
		_data[30560] = new ContainerData(30560, 72, 88, 44, 65, 186, 159, 30561, 105, 178);
		_data[30562] = new ContainerData(30562, 72, 88, 44, 65, 186, 159, 30563, 105, 178);
		_data[30586] = new ContainerData(30586, 0, 0, 32, 40, 184, 116, 0);
		_data[40153] = new ContainerData(40153, 0, 0, 10, 10, 160, 105, 0);
		_data[40155] = new ContainerData(40155, 0, 0, 50, 60, 548, 308, 0);
		_data[40157] = new ContainerData(40157, 0, 0, 50, 60, 548, 308, 0);
		_data[40159] = new ContainerData(40159, 0, 0, 50, 60, 548, 308, 0);
		_data[40163] = new ContainerData(40163, 0, 0, 50, 60, 548, 308, 0);
		_data[40164] = new ContainerData(40164, 0, 0, 44, 65, 186, 159, 0);
		_data[40165] = new ContainerData(40165, 0, 0, 44, 65, 186, 159, 0);
		_data[40167] = new ContainerData(40167, 0, 0, 44, 65, 186, 159, 0);
		_data[60] = new ContainerData(60, 72, 88, 44, 65, 186, 159, 0, 0, 0, 2801, 3);
		_data[61] = new ContainerData(61, 72, 88, 29, 34, 137, 128, 0, 0, 0, 2801, 3);
		_data[62] = new ContainerData(62, 47, 46, 33, 36, 142, 148, 0, 0, 0, 0, 3);
		_data[63] = new ContainerData(63, 79, 88, 19, 54, 184, 128, 0, 0, 0, 0, 3);
		_data[64] = new ContainerData(64, 74, 941, 16, 38, 152, 125, 0, 0, 0, 0, 3);
		_data[65] = new ContainerData(65, 79, 88, 25, 35, 151, 140, 0, 0, 0, 0, 1);
		_data[66] = new ContainerData(66, 45, 44, 18, 105, 162, 178, 0, 0, 0, 0, 3);
		_data[67] = new ContainerData(67, 45, 44, 16, 51, 184, 124, 0, 0, 0, 344, 3);
		_data[68] = new ContainerData(68, 45, 44, 20, 10, 170, 100, 0, 0, 0, 0, 3);
		_data[72] = new ContainerData(72, 47, 46, 16, 10, 154, 94, 0, 0, 0, 0, 3);
		_data[73] = new ContainerData(73, 45, 44, 18, 105, 162, 178, 0, 0, 0, 0, 3);
		_data[74] = new ContainerData(74, 45, 44, 18, 105, 162, 178, 0, 0, 0, 0, 3);
		_data[75] = new ContainerData(75, 74, 941, 16, 51, 184, 124, 0, 0, 0, 49, 3);
		_data[77] = new ContainerData(77, 47, 46, 71, 7, 147, 80, 0, 0, 0, 0, 3);
		_data[78] = new ContainerData(78, 45, 44, 24, 24, 96, 152, 0, 0, 0, 0, 3);
		_data[79] = new ContainerData(79, 45, 44, 24, 24, 96, 152, 0, 0, 0, 0, 3);
		_data[81] = new ContainerData(81, 47, 46, 16, 10, 154, 94, 0, 0, 0, 0, 3);
		_data[130] = new ContainerData(130, 47, 46, 20, 85, 128, 200, 0, 0, 0, 0, 1);
		_data[131] = new ContainerData(131, 47, 46, 33, 74, 142, 188, 0, 0, 0, 0, 3);
		_data[132] = new ContainerData(132, 72, 88, 20, 100, 130, 210, 0, 0, 0, 0, 3);
		_data[133] = new ContainerData(133, 72, 88, 29, 34, 137, 128, 0, 0, 0, 0, 3);
		_data[134] = new ContainerData(134, 45, 44, 30, 67, 113, 205, 0, 0, 0, 0, 3);
		_data[137] = new ContainerData(137, 45, 44, 30, 67, 113, 205, 0, 0, 0, 0, 3);
		_data[138] = new ContainerData(138, 79, 88, 17, 100, 122, 187, 0, 0, 0, 0, 1);
		_data[139] = new ContainerData(139, 47, 46, 20, 85, 128, 200, 0, 0, 0, 0, 3);
		_data[140] = new ContainerData(140, 45, 44, 23, 89, 179, 195, 0, 0, 0, 2795, 3);
		_data[141] = new ContainerData(141, 47, 46, 14, 100, 156, 174, 0, 0, 0, 0, 3);
		_data[142] = new ContainerData(142, 47, 46, 12, 88, 135, 204, 0, 0, 0, 0, 1);
		_data[143] = new ContainerData(143, 45, 44, 23, 89, 179, 195, 0, 0, 0, 0, 3);
		_data[144] = new ContainerData(144, 47, 46, 14, 100, 156, 174, 0, 0, 0, 0, 3);
		_data[145] = new ContainerData(145, 47, 46, 20, 85, 128, 200, 0, 0, 0, 0, 1);
		_data[146] = new ContainerData(146, 47, 46, 30, 67, 113, 205, 0, 0, 0, 0, 3);
		_data[147] = new ContainerData(147, 79, 88, 18, 62, 83, 200, 0);
	}
}

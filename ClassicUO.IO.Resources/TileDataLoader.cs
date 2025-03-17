using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ClassicUO.Data;
using ClassicUO.Utility;
using ClassicUO.Utility.Collections;

namespace ClassicUO.IO.Resources;

internal class TileDataLoader : UOFileLoader
{
	private static TileDataLoader _instance;

	private static StaticTiles[] _staticData;

	private static LandTiles[] _landData;

	public OrderedDictionary<int, bool> NoDrawTiles = new OrderedDictionary<int, bool>();

	public byte[] NoDrawTilesVerificationBuffer;

	public OrderedDictionary<int, bool> AnimPHueTiles = new OrderedDictionary<int, bool>();

	public bool ShowNoDrawTiles;

	public static TileDataLoader Instance => _instance ?? (_instance = new TileDataLoader());

	public ref LandTiles[] LandData => ref _landData;

	public ref StaticTiles[] StaticData => ref _staticData;

	private TileDataLoader()
	{
	}

	public unsafe override Task Load()
	{
		return Task.Run(delegate
		{
			string uOFilePath = UOFileManager.GetUOFilePath("tiledata.mul");
			FileSystemHelper.EnsureFileExists(uOFilePath);
			UOFileMul uOFileMul = new UOFileMul(uOFilePath);
			bool flag = Client.Version < ClientVersion.CV_7090;
			int num = (flag ? Marshal.SizeOf<LandGroupOld>() : Marshal.SizeOf<LandGroupNew>());
			int num2 = (flag ? Marshal.SizeOf<StaticGroupOld>() : Marshal.SizeOf<StaticGroupNew>());
			int num3 = (int)((uOFileMul.Length - 512 * num) / num2);
			if (num3 > 2048)
			{
				num3 = 2048;
			}
			uOFileMul.Seek(0);
			_landData = new LandTiles[16384];
			_staticData = new StaticTiles[num3 * 32];
			byte* ptr = stackalloc byte[20];
			for (int i = 0; i < 512; i++)
			{
				uOFileMul.Skip(4);
				for (int j = 0; j < 32; j++)
				{
					if (uOFileMul.Position + (flag ? 4 : 8) + 2 + 20 > uOFileMul.Length)
					{
						goto end_IL_0158;
					}
					int num4 = i * 32 + j;
					ulong flags = (flag ? uOFileMul.ReadUInt() : uOFileMul.ReadULong());
					ushort textId = uOFileMul.ReadUShort();
					for (int k = 0; k < 20; k++)
					{
						ptr[k] = uOFileMul.ReadByte();
					}
					string name = string.Intern(Encoding.UTF8.GetString(ptr, 20).TrimEnd('\0'));
					LandData[num4] = new LandTiles(flags, textId, name);
				}
				continue;
				end_IL_0158:
				break;
			}
			for (int l = 0; l < num3; l++)
			{
				if (uOFileMul.Position >= uOFileMul.Length)
				{
					break;
				}
				uOFileMul.Skip(4);
				for (int m = 0; m < 32; m++)
				{
					if (uOFileMul.Position + (flag ? 4 : 8) + 13 + 20 > uOFileMul.Length)
					{
						goto end_IL_0276;
					}
					int num5 = l * 32 + m;
					ulong flags2 = (flag ? uOFileMul.ReadUInt() : uOFileMul.ReadULong());
					byte weight = uOFileMul.ReadByte();
					byte layer = uOFileMul.ReadByte();
					int count = uOFileMul.ReadInt();
					ushort animId = uOFileMul.ReadUShort();
					ushort hue = uOFileMul.ReadUShort();
					ushort lightIndex = uOFileMul.ReadUShort();
					byte height = uOFileMul.ReadByte();
					for (int n = 0; n < 20; n++)
					{
						ptr[n] = uOFileMul.ReadByte();
					}
					string name2 = string.Intern(Encoding.UTF8.GetString(ptr, 20).TrimEnd('\0'));
					StaticData[num5] = new StaticTiles(flags2, weight, layer, count, animId, hue, lightIndex, height, name2);
				}
				continue;
				end_IL_0276:
				break;
			}
			uOFileMul.Dispose();
		});
	}
}

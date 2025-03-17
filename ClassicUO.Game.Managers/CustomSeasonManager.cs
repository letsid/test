using System;
using System.Collections.Generic;
using System.IO;
using ClassicUO.Game.GameObjects;
using ClassicUO.IO;

namespace ClassicUO.Game.Managers;

internal class CustomSeasonManager
{
	public class SeasonRect
	{
		public ushort StartX { get; set; }

		public ushort StartY { get; set; }

		public ushort EndX { get; set; }

		public ushort EndY { get; set; }

		public SeasonRect(ushort startX, ushort startY, ushort endX, ushort endY)
		{
			StartX = startX;
			StartY = startY;
			EndX = endX;
			EndY = endY;
		}
	}

	public class IdxEntry
	{
		public uint LookUp { get; set; }

		public uint Size { get; set; }

		public SeasonIdxNumbers Type { get; set; }

		public ushort Number { get; set; }

		public IdxEntry(uint lookUp, uint size, SeasonIdxNumbers type, ushort number)
		{
			LookUp = lookUp;
			Size = size;
			Type = type;
			Number = number;
		}
	}

	public class SeasonArea
	{
		public List<SeasonRect> Rects { get; set; } = new List<SeasonRect>();

		public byte[] Hitbox { get; set; }

		public int HitboxStartX { get; set; }

		public int HitboxWidth { get; set; }

		public int HitboxStartY { get; set; }

		public Dictionary<ushort, ushort> DispToDispMap { get; set; } = new Dictionary<ushort, ushort>();

		public Dictionary<ushort, List<ushort>> DispToDispListMap { get; set; } = new Dictionary<ushort, List<ushort>>();

		public Dictionary<ushort, ushort> DispToColorMap { get; set; } = new Dictionary<ushort, ushort>();

		public Dictionary<ushort, List<ushort>> DispToColorListMap { get; set; } = new Dictionary<ushort, List<ushort>>();

		public Dictionary<ushort, ushort> LandToLandMap { get; set; } = new Dictionary<ushort, ushort>();

		public Dictionary<ushort, List<ushort>> LandToLandListMap { get; set; } = new Dictionary<ushort, List<ushort>>();

		public ushort GetDispID(ushort dispID, int x, int y, out bool found)
		{
			if (DispToDispMap.ContainsKey(dispID))
			{
				found = true;
				return DispToDispMap[dispID];
			}
			if (DispToDispListMap.ContainsKey(dispID))
			{
				List<ushort> list = DispToDispListMap[dispID];
				int index = Math.Abs((x * 1337 + y) % list.Count);
				found = true;
				return list[index];
			}
			found = false;
			return dispID;
		}

		public ushort GetLandID(ushort dispID, int x, int y, out bool found)
		{
			if (LandToLandMap.ContainsKey(dispID))
			{
				found = true;
				return LandToLandMap[dispID];
			}
			if (LandToLandListMap.ContainsKey(dispID))
			{
				List<ushort> list = LandToLandListMap[dispID];
				int index = Math.Abs((x * 1337 + y) % list.Count);
				found = true;
				return list[index];
			}
			found = false;
			return dispID;
		}

		public ushort GetColor(ushort dispID, ushort color, int x, int y, out bool found)
		{
			if (DispToColorMap.ContainsKey(dispID))
			{
				found = true;
				return DispToColorMap[dispID];
			}
			if (DispToColorListMap.ContainsKey(dispID))
			{
				List<ushort> list = DispToColorListMap[dispID];
				int index = Math.Abs((x * 1337 + y) % list.Count);
				found = true;
				return list[index];
			}
			found = false;
			return color;
		}

		public bool Contains(int x, int y)
		{
			if (Hitbox != null && Hitbox.Length != 0)
			{
				int num = x - HitboxStartX;
				int num2 = (y - HitboxStartY) * HitboxWidth + num;
				if (num2 >= 0 && num2 < Hitbox.Length)
				{
					return Hitbox[num2] != 0;
				}
			}
			return false;
		}

		internal void CalculateHitbox()
		{
			int num = 10000;
			int num2 = 0;
			int num3 = 10000;
			int num4 = 0;
			foreach (SeasonRect rect in Rects)
			{
				if (rect.StartX < num)
				{
					num = rect.StartX;
				}
				if (rect.EndX > num2)
				{
					num2 = rect.EndX;
				}
				if (rect.StartY < num3)
				{
					num3 = rect.StartY;
				}
				if (rect.EndY > num4)
				{
					num4 = rect.EndY;
				}
			}
			int num5 = num2 - num;
			int num6 = num4 - num3;
			HitboxStartX = num;
			HitboxStartY = num3;
			HitboxWidth = num5;
			Hitbox = new byte[num5 * num6];
			for (int i = 0; i < Hitbox.Length; i++)
			{
				Hitbox[i] = 0;
			}
			foreach (SeasonRect rect2 in Rects)
			{
				for (int j = rect2.StartX; j < rect2.EndX; j++)
				{
					int num7 = j - num;
					for (int k = rect2.StartY; k < rect2.EndY; k++)
					{
						int num8 = k - num3;
						Hitbox[num8 * num5 + num7] = 1;
					}
				}
			}
		}
	}

	public enum SeasonIdxNumbers : ushort
	{
		SeasonIdxNumbers_Season = 42334,
		SeasonIdxNumbers_SeasonArea = 60069,
		SeasonIdxNumbers_Rects = 51086,
		SeasonIdxNumbers_DispToDispList = 3543,
		SeasonIdxNumbers_DispToColorList = 3287,
		SeasonIdxNumbers_DispToRandomDispList = 55255,
		SeasonIdxNumbers_DispToRandomColorList = 51159,
		SeasonIdxNumbers_LandToLandList = 1911,
		SeasonIdxNumbers_LandToRandomLandList = 30583
	}

	private static List<SeasonArea> Areas { get; set; } = new List<SeasonArea>();

	public static ushort RemapStaticWorldObject(GameObject staticObject, ushort originalGraphic)
	{
		foreach (SeasonArea area in Areas)
		{
			if (area.Contains(staticObject.X, staticObject.Y))
			{
				bool found;
				ushort dispID = area.GetDispID(originalGraphic, staticObject.X, staticObject.Y, out found);
				if (found)
				{
					return dispID;
				}
			}
		}
		return originalGraphic;
	}

	public static ushort RemapLandTiles(Land tile, ushort originalGraphic)
	{
		foreach (SeasonArea area in Areas)
		{
			if (area.Contains(tile.X, tile.Y))
			{
				bool found;
				ushort landID = area.GetLandID(originalGraphic, tile.X, tile.Y, out found);
				if (found)
				{
					return landID;
				}
			}
		}
		return originalGraphic;
	}

	public static ushort RemapColor(GameObject staticTile, ushort originalGraphic, ushort color)
	{
		foreach (SeasonArea area in Areas)
		{
			if (area.Contains(staticTile.X, staticTile.Y))
			{
				bool found;
				ushort color2 = area.GetColor(originalGraphic, color, staticTile.X, staticTile.Y, out found);
				if (found && color2 != ushort.MaxValue)
				{
					return color2;
				}
			}
		}
		return color;
	}

	public static void InitializeSeason()
	{
		Areas.Clear();
		using FileStream input = File.Open(UOFileManager.GetUOFilePath("seasonsidx.mul"), FileMode.Open);
		BinaryReader binaryReader = new BinaryReader(input);
		long num = binaryReader.BaseStream.Length / 12;
		List<IdxEntry> list = new List<IdxEntry>();
		for (long num2 = 0L; num2 < num; num2++)
		{
			uint lookUp = binaryReader.ReadUInt32();
			uint size = binaryReader.ReadUInt32();
			SeasonIdxNumbers type = (SeasonIdxNumbers)binaryReader.ReadUInt16();
			ushort number = binaryReader.ReadUInt16();
			list.Add(new IdxEntry(lookUp, size, type, number));
		}
		IdxEntry idxEntry = null;
		foreach (IdxEntry item2 in list)
		{
			if (item2.Type == SeasonIdxNumbers.SeasonIdxNumbers_Season && (Season)item2.Number == World.Season)
			{
				idxEntry = item2;
				break;
			}
		}
		if (idxEntry == null)
		{
			return;
		}
		using FileStream input2 = File.Open(UOFileManager.GetUOFilePath("seasons.mul"), FileMode.Open);
		BinaryReader binaryReader2 = new BinaryReader(input2);
		binaryReader2.BaseStream.Seek(idxEntry.LookUp, SeekOrigin.Begin);
		List<uint> list2 = new List<uint>();
		uint num3 = idxEntry.Size / 4;
		for (int i = 0; i < num3; i++)
		{
			list2.Add(binaryReader2.ReadUInt32());
		}
		foreach (uint item3 in list2)
		{
			IdxEntry idxEntry2 = list[(int)item3];
			if (idxEntry2.Type != SeasonIdxNumbers.SeasonIdxNumbers_SeasonArea)
			{
				continue;
			}
			SeasonArea seasonArea = new SeasonArea();
			binaryReader2.BaseStream.Seek(idxEntry2.LookUp, SeekOrigin.Begin);
			List<uint> list3 = new List<uint>();
			uint num4 = idxEntry2.Size / 4;
			for (int j = 0; j < num4; j++)
			{
				list3.Add(binaryReader2.ReadUInt32());
			}
			foreach (uint item4 in list3)
			{
				IdxEntry idxEntry3 = list[(int)item4];
				binaryReader2.BaseStream.Seek(idxEntry3.LookUp, SeekOrigin.Begin);
				if (idxEntry3.Type == SeasonIdxNumbers.SeasonIdxNumbers_Rects)
				{
					uint num5 = idxEntry3.Size / 8;
					for (int k = 0; k < num5; k++)
					{
						ushort startX = binaryReader2.ReadUInt16();
						ushort startY = binaryReader2.ReadUInt16();
						ushort endX = binaryReader2.ReadUInt16();
						ushort endY = binaryReader2.ReadUInt16();
						SeasonRect item = new SeasonRect(startX, startY, endX, endY);
						seasonArea.Rects.Add(item);
					}
				}
				else if (idxEntry3.Type == SeasonIdxNumbers.SeasonIdxNumbers_DispToDispList)
				{
					uint num6 = idxEntry3.Size / 4;
					for (int l = 0; l < num6; l++)
					{
						ushort key = binaryReader2.ReadUInt16();
						ushort value = binaryReader2.ReadUInt16();
						seasonArea.DispToDispMap.Add(key, value);
					}
				}
				else if (idxEntry3.Type == SeasonIdxNumbers.SeasonIdxNumbers_LandToLandList)
				{
					uint num7 = idxEntry3.Size / 4;
					for (int m = 0; m < num7; m++)
					{
						ushort key2 = binaryReader2.ReadUInt16();
						ushort value2 = binaryReader2.ReadUInt16();
						seasonArea.LandToLandMap.Add(key2, value2);
					}
				}
				else if (idxEntry3.Type == SeasonIdxNumbers.SeasonIdxNumbers_DispToColorList)
				{
					uint num8 = idxEntry3.Size / 4;
					for (int n = 0; n < num8; n++)
					{
						ushort key3 = binaryReader2.ReadUInt16();
						ushort value3 = binaryReader2.ReadUInt16();
						seasonArea.DispToColorMap.Add(key3, value3);
					}
				}
				else if (idxEntry3.Type == SeasonIdxNumbers.SeasonIdxNumbers_DispToRandomDispList)
				{
					ushort number2 = idxEntry3.Number;
					uint num9 = idxEntry3.Size / 2;
					List<ushort> list4 = new List<ushort>();
					for (int num10 = 0; num10 < num9; num10++)
					{
						list4.Add(binaryReader2.ReadUInt16());
					}
					seasonArea.DispToDispListMap[number2] = list4;
				}
				else if (idxEntry3.Type == SeasonIdxNumbers.SeasonIdxNumbers_DispToRandomColorList)
				{
					ushort number3 = idxEntry3.Number;
					uint num11 = idxEntry3.Size / 2;
					List<ushort> list5 = new List<ushort>();
					for (int num12 = 0; num12 < num11; num12++)
					{
						list5.Add(binaryReader2.ReadUInt16());
					}
					seasonArea.DispToColorListMap[number3] = list5;
				}
				else if (idxEntry3.Type == SeasonIdxNumbers.SeasonIdxNumbers_LandToRandomLandList)
				{
					ushort number4 = idxEntry3.Number;
					uint num13 = idxEntry3.Size / 2;
					List<ushort> list6 = new List<ushort>();
					for (int num14 = 0; num14 < num13; num14++)
					{
						list6.Add(binaryReader2.ReadUInt16());
					}
					seasonArea.LandToLandListMap[number4] = list6;
				}
			}
			seasonArea.CalculateHitbox();
			Areas.Add(seasonArea);
		}
	}
}

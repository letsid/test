using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.IO;
using ClassicUO.Network;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.Managers;

internal class HouseCustomizationManager
{
	public static readonly List<CustomHouseWallCategory> Walls;

	public static readonly List<CustomHouseFloor> Floors;

	public static readonly List<CustomHouseDoor> Doors;

	public static readonly List<CustomHouseMiscCategory> Miscs;

	public static readonly List<CustomHouseStair> Stairs;

	public static readonly List<CustomHouseTeleport> Teleports;

	public static readonly List<CustomHouseRoofCategory> Roofs;

	public static readonly List<CustomHousePlaceInfo> ObjectsInfo;

	public int Category = -1;

	public int MaxPage = 1;

	public int CurrentFloor = 1;

	public int FloorCount = 4;

	public int RoofZ = 1;

	public int MinHouseZ = -120;

	public int Components;

	public int Fixtures;

	public int MaxComponets;

	public int MaxFixtures;

	public bool Erasing;

	public bool SeekTile;

	public bool ShowWindow;

	public bool CombinedStair;

	public readonly int[] FloorVisionState = new int[4];

	public ushort SelectedGraphic;

	public readonly uint Serial;

	public Point StartPos;

	public Point EndPos;

	public CUSTOM_HOUSE_GUMP_STATE State;

	static HouseCustomizationManager()
	{
		Walls = new List<CustomHouseWallCategory>();
		Floors = new List<CustomHouseFloor>();
		Doors = new List<CustomHouseDoor>();
		Miscs = new List<CustomHouseMiscCategory>();
		Stairs = new List<CustomHouseStair>();
		Teleports = new List<CustomHouseTeleport>();
		Roofs = new List<CustomHouseRoofCategory>();
		ObjectsInfo = new List<CustomHousePlaceInfo>();
		ParseFileWithCategory<CustomHouseWall, CustomHouseWallCategory>(Walls, UOFileManager.GetUOFilePath("walls.txt"));
		ParseFile(Floors, UOFileManager.GetUOFilePath("floors.txt"));
		ParseFile(Doors, UOFileManager.GetUOFilePath("doors.txt"));
		ParseFileWithCategory<CustomHouseMisc, CustomHouseMiscCategory>(Miscs, UOFileManager.GetUOFilePath("misc.txt"));
		ParseFile(Stairs, UOFileManager.GetUOFilePath("stairs.txt"));
		ParseFile(Teleports, UOFileManager.GetUOFilePath("teleprts.txt"));
		ParseFileWithCategory<CustomHouseRoof, CustomHouseRoofCategory>(Roofs, UOFileManager.GetUOFilePath("roof.txt"));
		ParseFile(ObjectsInfo, UOFileManager.GetUOFilePath("suppinfo.txt"));
	}

	public HouseCustomizationManager(uint serial)
	{
		Serial = serial;
		InitializeHouse();
	}

	private void InitializeHouse()
	{
		Item item = World.Items.Get(Serial);
		if (item != null)
		{
			MinHouseZ = item.Z + 7;
			Rectangle? multiInfo = item.MultiInfo;
			if (multiInfo.HasValue)
			{
				StartPos.X = item.X + multiInfo.Value.X;
				StartPos.Y = item.Y + multiInfo.Value.Y;
				EndPos.X = item.X + multiInfo.Value.Width + 1;
				EndPos.Y = item.Y + multiInfo.Value.Height + 1;
			}
			int num = Math.Abs(EndPos.X - StartPos.X);
			int num2 = Math.Abs(EndPos.Y - StartPos.Y);
			if (num > 13 || num2 > 13)
			{
				FloorCount = 4;
			}
			else
			{
				FloorCount = 3;
			}
			int num3 = (num - 1) * (num2 - 1);
			MaxComponets = FloorCount * (num3 + 2 * (num + num2) - 4) - (int)((double)(FloorCount * num3) * -0.25) + 2 * num + 3 * num2 - 5;
			MaxFixtures = MaxComponets / 20;
		}
	}

	public void GenerateFloorPlace()
	{
		Item item = World.Items.Get(Serial);
		if (!(item != null) || !World.HouseManager.TryGetHouse(Serial, out var house))
		{
			return;
		}
		house.ClearCustomHouseComponents(CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_GENERIC_INTERNAL);
		foreach (Multi component in house.Components)
		{
			if (!component.IsCustom)
			{
				continue;
			}
			int num = -1;
			int num2 = item.Z + 7;
			int z = component.Z;
			bool flag = false;
			for (int i = 0; i < 4; i++)
			{
				int num3 = 0;
				if (z >= num2 - num3 && z < num2 + 20)
				{
					num = i;
					break;
				}
				num2 += 20;
			}
			if (num == -1)
			{
				flag = true;
				num = 0;
			}
			(int, int) tuple = SeekGraphicInCustomHouseObjectList(Floors, component.Graphic);
			int item2 = tuple.Item1;
			int item3 = tuple.Item2;
			CUSTOM_HOUSE_MULTI_OBJECT_FLAGS cUSTOM_HOUSE_MULTI_OBJECT_FLAGS = component.State;
			if (item2 != -1 && item3 != -1)
			{
				cUSTOM_HOUSE_MULTI_OBJECT_FLAGS |= CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_FLOOR;
				if (FloorVisionState[num] == 4)
				{
					cUSTOM_HOUSE_MULTI_OBJECT_FLAGS |= CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_IGNORE_IN_RENDER;
				}
				else if (FloorVisionState[num] == 3 || FloorVisionState[num] == 5)
				{
					cUSTOM_HOUSE_MULTI_OBJECT_FLAGS |= CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_TRANSPARENT;
				}
			}
			else
			{
				var (num4, num5) = SeekGraphicInCustomHouseObjectList(Stairs, component.Graphic);
				if (num4 != -1 && num5 != -1)
				{
					cUSTOM_HOUSE_MULTI_OBJECT_FLAGS |= CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_STAIR;
				}
				else
				{
					var (num6, num7) = SeekGraphicInCustomHouseObjectListWithCategory<CustomHouseRoof, CustomHouseRoofCategory>(Roofs, component.Graphic);
					if (num6 != -1 && num7 != -1)
					{
						cUSTOM_HOUSE_MULTI_OBJECT_FLAGS |= CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_ROOF;
					}
					else
					{
						int num8;
						int num9;
						(num8, num9) = SeekGraphicInCustomHouseObjectList(Doors, component.Graphic);
						if (num8 == -1 || num9 == -1)
						{
							(num8, num9) = SeekGraphicInCustomHouseObjectList(Teleports, component.Graphic);
							if (num8 != -1 && num9 != -1)
							{
								cUSTOM_HOUSE_MULTI_OBJECT_FLAGS |= CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_FLOOR;
							}
						}
						else
						{
							cUSTOM_HOUSE_MULTI_OBJECT_FLAGS |= CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_FIXTURE;
						}
					}
				}
				if (!flag)
				{
					if (FloorVisionState[num] == 2)
					{
						cUSTOM_HOUSE_MULTI_OBJECT_FLAGS |= CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_IGNORE_IN_RENDER;
					}
					else if (FloorVisionState[num] == 1)
					{
						cUSTOM_HOUSE_MULTI_OBJECT_FLAGS |= CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_TRANSPARENT;
					}
				}
			}
			if (!flag && FloorVisionState[num] == 6)
			{
				cUSTOM_HOUSE_MULTI_OBJECT_FLAGS |= CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_IGNORE_IN_RENDER;
			}
			component.State = cUSTOM_HOUSE_MULTI_OBJECT_FLAGS;
		}
		int num10 = item.Z + 7;
		for (int x = StartPos.X + 1; x < EndPos.X; x++)
		{
			int y;
			for (y = StartPos.Y + 1; y < EndPos.Y; y++)
			{
				IEnumerable<Multi> enumerable = house.Components.Where((Multi s) => s.X == x && s.Y == y);
				if (enumerable == null)
				{
					continue;
				}
				Multi multi = null;
				Multi multi2 = null;
				foreach (Multi item4 in enumerable)
				{
					if (item4.Z == num10 && (item4.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_FLOOR) != 0)
					{
						if (item4.IsCustom)
						{
							multi2 = item4;
						}
						else
						{
							multi = item4;
						}
					}
				}
				if (multi != null && multi2 == null)
				{
					Multi multi3 = house.Add(multi.Graphic, 0, (ushort)(item.X + (x - item.X)), (ushort)(item.Y + (y - item.Y)), (sbyte)num10, iscustom: true, ismovable: false);
					multi3.AlphaHue = byte.MaxValue;
					CUSTOM_HOUSE_MULTI_OBJECT_FLAGS cUSTOM_HOUSE_MULTI_OBJECT_FLAGS2 = CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_FLOOR;
					if (FloorVisionState[0] == 4)
					{
						cUSTOM_HOUSE_MULTI_OBJECT_FLAGS2 |= CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_IGNORE_IN_RENDER;
					}
					else if (FloorVisionState[0] == 3 || FloorVisionState[0] == 5)
					{
						cUSTOM_HOUSE_MULTI_OBJECT_FLAGS2 |= CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_TRANSPARENT;
					}
					multi3.State = cUSTOM_HOUSE_MULTI_OBJECT_FLAGS2;
				}
			}
		}
		for (int j = 0; j < FloorCount; j++)
		{
			int num11 = item.Z + 7 + j * 20;
			int num12 = num11 + 20;
			for (int k = 0; k < 2; k++)
			{
				List<Point> list = new List<Point>();
				for (int l = StartPos.X; l < EndPos.X + 1; l++)
				{
					for (int m = StartPos.Y; m < EndPos.Y + 1; m++)
					{
						IEnumerable<Multi> multiAt = house.GetMultiAt(l, m);
						if (multiAt == null)
						{
							continue;
						}
						foreach (Multi item5 in multiAt)
						{
							if (!item5.IsCustom)
							{
								continue;
							}
							if (k == 0)
							{
								if (j == 0 && item5.Z < num11)
								{
									item5.State |= CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_VALIDATED_PLACE;
									continue;
								}
								if ((item5.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_FLOOR) == 0)
								{
									continue;
								}
								if (j == 0 && item5.Z >= num11 && item5.Z < num12)
								{
									item5.State |= CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_VALIDATED_PLACE;
									continue;
								}
							}
							if ((item5.State & (CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_GENERIC_INTERNAL | CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_VALIDATED_PLACE)) == 0 && item5.Z >= num11 && item5.Z < num12)
							{
								if (!ValidateItemPlace(item, item5, num11, num12, list))
								{
									item5.State = item5.State | CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_VALIDATED_PLACE | CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_INCORRECT_PLACE;
								}
								else
								{
									item5.State |= CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_VALIDATED_PLACE;
								}
							}
						}
					}
				}
				if (j == 0 || k != 0)
				{
					continue;
				}
				foreach (Point item6 in list)
				{
					IEnumerable<Multi> multiAt2 = house.GetMultiAt(item6.X, item6.Y);
					if (multiAt2 == null)
					{
						continue;
					}
					foreach (Multi item7 in multiAt2)
					{
						if (item7.IsCustom && (item7.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_FLOOR) != 0 && item7.Z >= num11 && item7.Z < num12)
						{
							item7.State &= ~CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_INCORRECT_PLACE;
						}
					}
				}
				for (int n = StartPos.X; n < EndPos.X + 1; n++)
				{
					int num13 = 0;
					int num14 = 0;
					for (int num15 = StartPos.Y; num15 < EndPos.Y + 1; num15++)
					{
						IEnumerable<Multi> multiAt3 = house.GetMultiAt(n, num15);
						if (multiAt3 == null)
						{
							continue;
						}
						foreach (Multi item8 in multiAt3)
						{
							if (item8.IsCustom && (item8.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_FLOOR) != 0 && (item8.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_VALIDATED_PLACE) != 0 && (item8.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_INCORRECT_PLACE) == 0 && item8.Z >= num11 && item8.Z < num12)
							{
								num13 = num15;
								break;
							}
						}
						if (num13 != 0)
						{
							break;
						}
					}
					for (int num16 = EndPos.Y; num16 >= StartPos.Y; num16--)
					{
						IEnumerable<Multi> multiAt4 = house.GetMultiAt(n, num16);
						if (multiAt4 != null)
						{
							foreach (Multi item9 in multiAt4)
							{
								if (item9.IsCustom && (item9.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_FLOOR) != 0 && (item9.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_VALIDATED_PLACE) != 0 && (item9.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_INCORRECT_PLACE) == 0 && item9.Z >= num11 && item9.Z < num12)
								{
									num14 = num16;
									break;
								}
							}
							if (num14 != 0)
							{
								break;
							}
						}
					}
					for (int num17 = num13; num17 < num14; num17++)
					{
						IEnumerable<Multi> multiAt5 = house.GetMultiAt(n, num17);
						if (multiAt5 == null)
						{
							continue;
						}
						foreach (Multi item10 in multiAt5)
						{
							if (item10.IsCustom && (item10.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_FLOOR) != 0 && (item10.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_VALIDATED_PLACE) != 0 && item10.Z >= num11 && item10.Z < num12)
							{
								item10.State &= ~CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_INCORRECT_PLACE;
							}
						}
					}
				}
				for (int num18 = StartPos.Y; num18 < EndPos.Y + 1; num18++)
				{
					int num19 = 0;
					int num20 = 0;
					for (int num21 = StartPos.X; num21 < EndPos.X + 1; num21++)
					{
						IEnumerable<Multi> multiAt6 = house.GetMultiAt(num21, num18);
						if (multiAt6 == null)
						{
							continue;
						}
						foreach (Multi item11 in multiAt6)
						{
							if (item11.IsCustom && (item11.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_FLOOR) != 0 && (item11.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_VALIDATED_PLACE) != 0 && (item11.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_INCORRECT_PLACE) == 0 && item11.Z >= num11 && item11.Z < num12)
							{
								num19 = num21;
								break;
							}
						}
						if (num19 != 0)
						{
							break;
						}
					}
					for (int num22 = EndPos.X; num22 >= StartPos.X; num22--)
					{
						IEnumerable<Multi> multiAt7 = house.GetMultiAt(num22, num18);
						if (multiAt7 != null)
						{
							foreach (Multi item12 in multiAt7)
							{
								if (item12.IsCustom && (item12.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_FLOOR) != 0 && (item12.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_VALIDATED_PLACE) != 0 && (item12.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_INCORRECT_PLACE) == 0 && item12.Z >= num11 && item12.Z < num12)
								{
									num20 = num22;
									break;
								}
							}
							if (num20 != 0)
							{
								break;
							}
						}
					}
					for (int num23 = num19; num23 < num20; num23++)
					{
						IEnumerable<Multi> multiAt8 = house.GetMultiAt(num23, num18);
						if (multiAt8 == null)
						{
							continue;
						}
						foreach (Multi item13 in multiAt8)
						{
							if (item13.IsCustom && (item13.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_FLOOR) != 0 && (item13.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_VALIDATED_PLACE) != 0 && item13.Z >= num11 && item13.Z < num12)
							{
								item13.State &= ~CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_INCORRECT_PLACE;
							}
						}
					}
				}
			}
		}
		num10 = item.Z + 7 + 20;
		ushort num24 = 81;
		for (int num25 = 1; num25 < CurrentFloor; num25++)
		{
			for (int num26 = StartPos.X; num26 < EndPos.X; num26++)
			{
				for (int num27 = StartPos.Y; num27 < EndPos.Y; num27++)
				{
					ushort num28 = num24;
					if (num26 == StartPos.X || num27 == StartPos.Y)
					{
						num28++;
					}
					Multi multi4 = house.Add(1174, num28, (ushort)(item.X + (num26 - item.X)), (ushort)(item.Y + (num27 - item.Y)), (sbyte)num10, iscustom: true, ismovable: false);
					multi4.AlphaHue = byte.MaxValue;
					multi4.State = CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_GENERIC_INTERNAL | CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_TRANSPARENT;
					multi4.AddToTile();
				}
			}
			num24 += 5;
			num10 += 20;
		}
	}

	public void OnTargetWorld(GameObject place)
	{
		if (place == null)
		{
			return;
		}
		int num = 0;
		HouseCustomizationGump gump = UIManager.GetGump<HouseCustomizationGump>(Serial);
		if (CurrentFloor == 1)
		{
			num = -7;
		}
		if (SeekTile)
		{
			if (place is Multi)
			{
				SeekGraphic(place.Graphic);
			}
		}
		else
		{
			if (place.Z < World.Player.Z + num || place.Z >= World.Player.Z + 20)
			{
				return;
			}
			Item item = World.Items.Get(Serial);
			if (item == null || !World.HouseManager.TryGetHouse(Serial, out var house))
			{
				return;
			}
			if (Erasing)
			{
				if (!(place is Multi))
				{
					return;
				}
				if (CanEraseHere(place, out var type))
				{
					IEnumerable<Multi> multiAt = house.GetMultiAt(place.X, place.Y);
					if (multiAt == null || !multiAt.Any())
					{
						return;
					}
					int num2 = 7 + (CurrentFloor - 1) * 20;
					if (type == CUSTOM_HOUSE_BUILD_TYPE.CHBT_STAIR || type == CUSTOM_HOUSE_BUILD_TYPE.CHBT_ROOF)
					{
						num2 = place.Z - (item.Z + num2) + num2;
					}
					if (type == CUSTOM_HOUSE_BUILD_TYPE.CHBT_ROOF)
					{
						NetClient.Socket.Send_CustomHouseDeleteRoof(place.Graphic, place.X - item.X, place.Y - item.Y, num2);
					}
					else
					{
						NetClient.Socket.Send_CustomHouseDeleteItem(place.Graphic, place.X - item.X, place.Y - item.Y, num2);
					}
					place.Destroy();
				}
			}
			else if (SelectedGraphic != 0)
			{
				CustomBuildObject[] array = new CustomBuildObject[10];
				if (CanBuildHere(array, out var type2) && array.Length != 0)
				{
					int x = place.X;
					int y = place.Y;
					if (type2 == CUSTOM_HOUSE_BUILD_TYPE.CHBT_STAIR && CombinedStair)
					{
						if (gump.Page >= 0 && gump.Page < Stairs.Count)
						{
							CustomHouseStair customHouseStair = Stairs[gump.Page];
							ushort num3 = 0;
							if (SelectedGraphic == customHouseStair.North)
							{
								num3 = (ushort)customHouseStair.MultiNorth;
							}
							else if (SelectedGraphic == customHouseStair.East)
							{
								num3 = (ushort)customHouseStair.MultiEast;
							}
							else if (SelectedGraphic == customHouseStair.South)
							{
								num3 = (ushort)customHouseStair.MultiSouth;
							}
							else if (SelectedGraphic == customHouseStair.West)
							{
								num3 = (ushort)customHouseStair.MultiWest;
							}
							if (num3 != 0)
							{
								NetClient.Socket.Send_CustomHouseAddStair(num3, x - item.X, y - item.Y);
							}
						}
					}
					else
					{
						CustomBuildObject customBuildObject = array[0];
						int x2 = x - item.X + customBuildObject.X;
						int y2 = y - item.Y + customBuildObject.Y;
						IEnumerable<Multi> multiAt2 = house.GetMultiAt(x + customBuildObject.X, y + customBuildObject.Y);
						if (multiAt2.Any() || type2 == CUSTOM_HOUSE_BUILD_TYPE.CHBT_STAIR)
						{
							if (!CombinedStair)
							{
								int num4 = item.Z + 7 + (CurrentFloor - 1) * 20;
								int num5 = num4 + 20;
								if (CurrentFloor == 1)
								{
									num4 -= 7;
								}
								foreach (Multi item2 in multiAt2)
								{
									int num6 = num4;
									if ((item2.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_ROOF) != 0)
									{
										num6 -= 3;
									}
									if (item2.Z < num6 || item2.Z >= num5 || !item2.IsCustom || (item2.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_GENERIC_INTERNAL) != 0)
									{
										continue;
									}
									switch (type2)
									{
									case CUSTOM_HOUSE_BUILD_TYPE.CHBT_STAIR:
										if ((item2.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_STAIR) != 0)
										{
											item2.Destroy();
										}
										break;
									case CUSTOM_HOUSE_BUILD_TYPE.CHBT_ROOF:
										if ((item2.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_ROOF) != 0)
										{
											item2.Destroy();
										}
										break;
									case CUSTOM_HOUSE_BUILD_TYPE.CHBT_FLOOR:
										if ((item2.State & (CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_FLOOR | CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_FIXTURE)) != 0)
										{
											item2.Destroy();
										}
										break;
									default:
										if ((item2.State & (CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_FLOOR | CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_STAIR | CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_ROOF | CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_DONT_REMOVE)) == 0)
										{
											item2.Destroy();
										}
										break;
									}
								}
							}
							if (type2 == CUSTOM_HOUSE_BUILD_TYPE.CHBT_ROOF)
							{
								NetClient.Socket.Send_CustomHouseAddRoof(customBuildObject.Graphic, x2, y2, customBuildObject.Z);
							}
							else
							{
								NetClient.Socket.Send_CustomHouseAddItem(customBuildObject.Graphic, x2, y2);
							}
						}
					}
					int num7 = x - item.X;
					int num8 = y - item.Y;
					int num9 = item.Z + 7 + (CurrentFloor - 1) * 20;
					if (type2 == CUSTOM_HOUSE_BUILD_TYPE.CHBT_STAIR && !CombinedStair)
					{
						num9 = item.Z;
					}
					CustomBuildObject[] array2 = array;
					for (int i = 0; i < array2.Length; i++)
					{
						CustomBuildObject customBuildObject2 = array2[i];
						if (customBuildObject2.Graphic == 0)
						{
							break;
						}
						house.Add(customBuildObject2.Graphic, 0, (ushort)(item.X + num7 + customBuildObject2.X), (ushort)(item.Y + num8 + customBuildObject2.Y), (sbyte)(num9 + customBuildObject2.Z), iscustom: true, ismovable: false);
					}
				}
			}
			GenerateFloorPlace();
			gump.Update();
		}
	}

	private void SeekGraphic(ushort graphic)
	{
		CUSTOM_HOUSE_GUMP_STATE state = CUSTOM_HOUSE_GUMP_STATE.CHGS_WALL;
		var (num, num2) = ExistsInList(ref state, graphic);
		if (num != -1 && num2 != -1)
		{
			State = state;
			HouseCustomizationGump gump = UIManager.GetGump<HouseCustomizationGump>(Serial);
			if (State == CUSTOM_HOUSE_GUMP_STATE.CHGS_WALL || State == CUSTOM_HOUSE_GUMP_STATE.CHGS_ROOF || State == CUSTOM_HOUSE_GUMP_STATE.CHGS_MISC)
			{
				Category = num;
				gump.Page = num2;
			}
			else
			{
				Category = -1;
				gump.Page = num;
			}
			gump.UpdateMaxPage();
			SetTargetMulti();
			SelectedGraphic = graphic;
			gump.Update();
		}
	}

	public void SetTargetMulti()
	{
		TargetManager.SetTargetingMulti(0u, 0, 0, 0, 0, 0);
		Erasing = false;
		SeekTile = false;
		SelectedGraphic = 0;
		CombinedStair = false;
	}

	public bool CanBuildHere(CustomBuildObject[] list, out CUSTOM_HOUSE_BUILD_TYPE type)
	{
		type = CUSTOM_HOUSE_BUILD_TYPE.CHBT_NORMAL;
		if (SelectedGraphic == 0)
		{
			return false;
		}
		bool result = true;
		if (CombinedStair)
		{
			if (Components + 10 > MaxComponets)
			{
				return false;
			}
			var (num, num2) = SeekGraphicInCustomHouseObjectList(Stairs, SelectedGraphic);
			if (num == -1 || num2 == -1 || num >= Stairs.Count)
			{
				list[0].Graphic = SelectedGraphic;
				list[0].X = 0;
				list[0].Y = 0;
				list[0].Z = 0;
				return false;
			}
			CustomHouseStair customHouseStair = Stairs[num];
			if (SelectedGraphic == customHouseStair.North)
			{
				list[0].Graphic = (ushort)customHouseStair.Block;
				list[0].X = 0;
				list[0].Y = -3;
				list[0].Z = 0;
				list[1].Graphic = (ushort)customHouseStair.Block;
				list[1].X = 0;
				list[1].Y = -2;
				list[1].Z = 0;
				list[2].Graphic = (ushort)customHouseStair.Block;
				list[2].X = 0;
				list[2].Y = -1;
				list[2].Z = 0;
				list[3].Graphic = (ushort)customHouseStair.North;
				list[3].X = 0;
				list[3].Y = 0;
				list[3].Z = 0;
				list[4].Graphic = (ushort)customHouseStair.Block;
				list[4].X = 0;
				list[4].Y = -3;
				list[4].Z = 5;
				list[5].Graphic = (ushort)customHouseStair.Block;
				list[5].X = 0;
				list[5].Y = -2;
				list[5].Z = 5;
				list[6].Graphic = (ushort)customHouseStair.North;
				list[6].X = 0;
				list[6].Y = -1;
				list[6].Z = 5;
				list[7].Graphic = (ushort)customHouseStair.Block;
				list[7].X = 0;
				list[7].Y = -3;
				list[7].Z = 10;
				list[8].Graphic = (ushort)customHouseStair.North;
				list[8].X = 0;
				list[8].Y = -2;
				list[8].Z = 10;
				list[9].Graphic = (ushort)customHouseStair.North;
				list[9].X = 0;
				list[9].Y = -3;
				list[9].Z = 15;
			}
			else if (SelectedGraphic == customHouseStair.East)
			{
				list[0].Graphic = (ushort)customHouseStair.East;
				list[0].X = 0;
				list[0].Y = 0;
				list[0].Z = 0;
				list[1].Graphic = (ushort)customHouseStair.Block;
				list[1].X = 1;
				list[1].Y = 0;
				list[1].Z = 0;
				list[2].Graphic = (ushort)customHouseStair.Block;
				list[2].X = 2;
				list[2].Y = 0;
				list[2].Z = 0;
				list[3].Graphic = (ushort)customHouseStair.Block;
				list[3].X = 3;
				list[3].Y = 0;
				list[3].Z = 0;
				list[4].Graphic = (ushort)customHouseStair.East;
				list[4].X = 1;
				list[4].Y = 0;
				list[4].Z = 5;
				list[5].Graphic = (ushort)customHouseStair.Block;
				list[5].X = 2;
				list[5].Y = 0;
				list[5].Z = 5;
				list[6].Graphic = (ushort)customHouseStair.Block;
				list[6].X = 3;
				list[6].Y = 0;
				list[6].Z = 5;
				list[7].Graphic = (ushort)customHouseStair.East;
				list[7].X = 2;
				list[7].Y = 0;
				list[7].Z = 10;
				list[8].Graphic = (ushort)customHouseStair.Block;
				list[8].X = 3;
				list[8].Y = 0;
				list[8].Z = 10;
				list[9].Graphic = (ushort)customHouseStair.East;
				list[9].X = 3;
				list[9].Y = 0;
				list[9].Z = 15;
			}
			else if (SelectedGraphic == customHouseStair.South)
			{
				list[0].Graphic = (ushort)customHouseStair.South;
				list[0].X = 0;
				list[0].Y = 0;
				list[0].Z = 0;
				list[1].Graphic = (ushort)customHouseStair.Block;
				list[1].X = 0;
				list[1].Y = 1;
				list[1].Z = 0;
				list[2].Graphic = (ushort)customHouseStair.Block;
				list[2].X = 0;
				list[2].Y = 2;
				list[2].Z = 0;
				list[3].Graphic = (ushort)customHouseStair.Block;
				list[3].X = 0;
				list[3].Y = 3;
				list[3].Z = 0;
				list[4].Graphic = (ushort)customHouseStair.South;
				list[4].X = 0;
				list[4].Y = 1;
				list[4].Z = 5;
				list[5].Graphic = (ushort)customHouseStair.Block;
				list[5].X = 0;
				list[5].Y = 2;
				list[5].Z = 5;
				list[6].Graphic = (ushort)customHouseStair.Block;
				list[6].X = 0;
				list[6].Y = 3;
				list[6].Z = 5;
				list[7].Graphic = (ushort)customHouseStair.South;
				list[7].X = 0;
				list[7].Y = 2;
				list[7].Z = 10;
				list[8].Graphic = (ushort)customHouseStair.Block;
				list[8].X = 0;
				list[8].Y = 3;
				list[8].Z = 10;
				list[9].Graphic = (ushort)customHouseStair.South;
				list[9].X = 0;
				list[9].Y = 3;
				list[9].Z = 15;
			}
			else if (SelectedGraphic == customHouseStair.West)
			{
				list[0].Graphic = (ushort)customHouseStair.Block;
				list[0].X = -3;
				list[0].Y = 0;
				list[0].Z = 0;
				list[1].Graphic = (ushort)customHouseStair.Block;
				list[1].X = -2;
				list[1].Y = 0;
				list[1].Z = 0;
				list[2].Graphic = (ushort)customHouseStair.Block;
				list[2].X = -1;
				list[2].Y = 0;
				list[2].Z = 0;
				list[3].Graphic = (ushort)customHouseStair.West;
				list[3].X = 0;
				list[3].Y = 0;
				list[3].Z = 0;
				list[4].Graphic = (ushort)customHouseStair.Block;
				list[4].X = -3;
				list[4].Y = 0;
				list[4].Z = 5;
				list[5].Graphic = (ushort)customHouseStair.Block;
				list[5].X = -2;
				list[5].Y = 0;
				list[5].Z = 5;
				list[6].Graphic = (ushort)customHouseStair.West;
				list[6].X = -1;
				list[6].Y = 0;
				list[6].Z = 5;
				list[7].Graphic = (ushort)customHouseStair.Block;
				list[7].X = -3;
				list[7].Y = 0;
				list[7].Z = 10;
				list[8].Graphic = (ushort)customHouseStair.West;
				list[8].X = -2;
				list[8].Y = 0;
				list[8].Z = 10;
				list[9].Graphic = (ushort)customHouseStair.West;
				list[9].X = -3;
				list[9].Y = 0;
				list[9].Z = 15;
			}
			else
			{
				list[0].Graphic = SelectedGraphic;
				list[0].X = 0;
				list[0].Y = 0;
				list[0].Z = 0;
			}
			type = CUSTOM_HOUSE_BUILD_TYPE.CHBT_STAIR;
		}
		else
		{
			(int, int) tuple2 = SeekGraphicInCustomHouseObjectList(Doors, SelectedGraphic);
			int item = tuple2.Item1;
			int item2 = tuple2.Item2;
			bool flag = false;
			if (item == -1 || item2 == -1)
			{
				(int, int) tuple3 = SeekGraphicInCustomHouseObjectList(Teleports, SelectedGraphic);
				item = tuple3.Item1;
				item2 = tuple3.Item2;
				flag = item != -1 && item2 != -1;
				if (flag)
				{
					type = CUSTOM_HOUSE_BUILD_TYPE.CHBT_FLOOR;
				}
			}
			else
			{
				flag = true;
				type = CUSTOM_HOUSE_BUILD_TYPE.CHBT_NORMAL;
			}
			if (flag)
			{
				if (Fixtures + 1 > MaxFixtures)
				{
					result = false;
				}
			}
			else if (Components + 1 > MaxComponets)
			{
				result = false;
			}
			if (State == CUSTOM_HOUSE_GUMP_STATE.CHGS_ROOF)
			{
				list[0].Graphic = SelectedGraphic;
				list[0].X = 0;
				list[0].Y = 0;
				list[0].Z = (RoofZ - 2) * 3;
				type = CUSTOM_HOUSE_BUILD_TYPE.CHBT_ROOF;
			}
			else if (State == CUSTOM_HOUSE_GUMP_STATE.CHGS_STAIR)
			{
				list[0].Graphic = SelectedGraphic;
				list[0].X = 0;
				list[0].Y = 1;
				list[0].Z = 0;
				type = CUSTOM_HOUSE_BUILD_TYPE.CHBT_STAIR;
			}
			else
			{
				if (State == CUSTOM_HOUSE_GUMP_STATE.CHGS_FLOOR)
				{
					type = CUSTOM_HOUSE_BUILD_TYPE.CHBT_FLOOR;
				}
				list[0].Graphic = SelectedGraphic;
				list[0].X = 0;
				list[0].Y = 0;
				list[0].Z = 0;
			}
		}
		BaseGameObject @object = SelectedObject.Object;
		GameObject gobj = @object as GameObject;
		if (gobj != null)
		{
			if (gobj.Z < MinHouseZ)
			{
				if (CombinedStair)
				{
					if (gobj.X >= EndPos.X || gobj.Y >= EndPos.Y)
					{
						return false;
					}
				}
				else if (type != CUSTOM_HOUSE_BUILD_TYPE.CHBT_STAIR && (gobj.X > EndPos.X - 1 || gobj.Y > EndPos.Y - 1))
				{
					return false;
				}
			}
			Item item3 = World.Items.Get(Serial);
			int num3 = (item3?.Z ?? 0) + 7 + (CurrentFloor - 1) * 20;
			int num4 = num3 + 20;
			int num5 = ((State != CUSTOM_HOUSE_GUMP_STATE.CHGS_WALL) ? 1 : 0);
			Rectangle rect = new Rectangle(StartPos.X + num5, StartPos.Y + num5, EndPos.X, EndPos.Y);
			for (int i = 0; i < list.Length; i++)
			{
				CustomBuildObject item4 = list[i];
				if (type == CUSTOM_HOUSE_BUILD_TYPE.CHBT_STAIR)
				{
					if (!CombinedStair)
					{
						int num6 = gobj.X + item4.X;
						if (gobj.Y + item4.Y < EndPos.Y || num6 == StartPos.X || gobj.Z >= MinHouseZ)
						{
							return false;
						}
						if (gobj.Y + item4.Y != EndPos.Y)
						{
							list[0].Y = 0;
						}
						continue;
					}
					if (item4.Z != 0)
					{
						continue;
					}
				}
				if (!ValidateItemPlace(rect, item4.Graphic, gobj.X + item4.X, gobj.Y + item4.Y))
				{
					return false;
				}
				if (type == CUSTOM_HOUSE_BUILD_TYPE.CHBT_FLOOR || !(item3 != null) || !World.HouseManager.TryGetHouse(Serial, out var house))
				{
					continue;
				}
				foreach (Multi item5 in house.Components.Where((Multi s) => s.X == gobj.X + item4.X && s.Y == gobj.Y + item4.Y))
				{
					if (!item5.IsCustom || (item5.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_GENERIC_INTERNAL) != 0 || item5.Z < num3 || item5.Z >= num4)
					{
						continue;
					}
					if (type == CUSTOM_HOUSE_BUILD_TYPE.CHBT_STAIR)
					{
						if ((item5.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_FLOOR) == 0)
						{
							return false;
						}
					}
					else if ((item5.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_STAIR) != 0)
					{
						return false;
					}
				}
			}
			return result;
		}
		return false;
	}

	public bool CanEraseHere(GameObject place, out CUSTOM_HOUSE_BUILD_TYPE type)
	{
		type = CUSTOM_HOUSE_BUILD_TYPE.CHBT_NORMAL;
		if (place != null && place is Multi { IsCustom: not false } multi && (multi.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_GENERIC_INTERNAL) == 0)
		{
			if ((multi.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_FLOOR) != 0)
			{
				type = CUSTOM_HOUSE_BUILD_TYPE.CHBT_FLOOR;
			}
			else if ((multi.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_STAIR) != 0)
			{
				type = CUSTOM_HOUSE_BUILD_TYPE.CHBT_STAIR;
			}
			else if ((multi.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_ROOF) != 0)
			{
				type = CUSTOM_HOUSE_BUILD_TYPE.CHBT_ROOF;
			}
			else if (place.X < StartPos.X || place.X > EndPos.X || place.Y < StartPos.Y || place.Y > EndPos.Y || place.Z < MinHouseZ)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public (int, int) ExistsInList(ref CUSTOM_HOUSE_GUMP_STATE state, ushort graphic)
	{
		var (num, num2) = SeekGraphicInCustomHouseObjectListWithCategory<CustomHouseWall, CustomHouseWallCategory>(Walls, graphic);
		if (num == -1 || num2 == -1)
		{
			(num, num2) = SeekGraphicInCustomHouseObjectList(Floors, graphic);
			if (num == -1 || num2 == -1)
			{
				(num, num2) = SeekGraphicInCustomHouseObjectList(Doors, graphic);
				if (num == -1 || num2 == -1)
				{
					(num, num2) = SeekGraphicInCustomHouseObjectListWithCategory<CustomHouseMisc, CustomHouseMiscCategory>(Miscs, graphic);
					if (num == -1 || num2 == -1)
					{
						(num, num2) = SeekGraphicInCustomHouseObjectList(Stairs, graphic);
						if (num == -1 || num2 == -1)
						{
							(num, num2) = SeekGraphicInCustomHouseObjectListWithCategory<CustomHouseRoof, CustomHouseRoofCategory>(Roofs, graphic);
							if (num != -1 && num2 != -1)
							{
								state = CUSTOM_HOUSE_GUMP_STATE.CHGS_ROOF;
							}
						}
						else
						{
							state = CUSTOM_HOUSE_GUMP_STATE.CHGS_STAIR;
						}
					}
					else
					{
						var (num3, num4) = SeekGraphicInCustomHouseObjectList(Teleports, graphic);
						if (num3 != -1 && num4 != -1)
						{
							state = CUSTOM_HOUSE_GUMP_STATE.CHGS_FIXTURE;
							num = num3;
							num2 = num4;
						}
						else
						{
							state = CUSTOM_HOUSE_GUMP_STATE.CHGS_MISC;
						}
					}
				}
				else
				{
					state = CUSTOM_HOUSE_GUMP_STATE.CHGS_DOOR;
				}
			}
			else
			{
				state = CUSTOM_HOUSE_GUMP_STATE.CHGS_FLOOR;
			}
		}
		else
		{
			state = CUSTOM_HOUSE_GUMP_STATE.CHGS_WALL;
		}
		return (num, num2);
	}

	private bool ValidateItemPlace(Rectangle rect, ushort graphic, int x, int y)
	{
		if (!rect.Contains(x, y))
		{
			return false;
		}
		var (num, num2) = SeekGraphicInCustomHouseObjectList(ObjectsInfo, graphic);
		if (num != -1 && num2 != -1)
		{
			CustomHousePlaceInfo customHousePlaceInfo = ObjectsInfo[num];
			if (customHousePlaceInfo.CanGoW == 0 && x == StartPos.X)
			{
				return false;
			}
			if (customHousePlaceInfo.CanGoN == 0 && y == StartPos.Y)
			{
				return false;
			}
			if (customHousePlaceInfo.CanGoNWS == 0 && x == StartPos.X && y == StartPos.Y)
			{
				return false;
			}
		}
		return true;
	}

	public bool ValidateItemPlace(Item foundationItem, Multi item, int minZ, int maxZ, List<Point> validatedFloors)
	{
		if (item == null || !World.HouseManager.TryGetHouse(foundationItem, out var house) || !item.IsCustom)
		{
			return true;
		}
		if ((item.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_FLOOR) != 0)
		{
			if (ValidatePlaceStructure(foundationItem, house, house.GetMultiAt(item.X, item.Y), minZ - 20, maxZ - 20, 64) || ValidatePlaceStructure(foundationItem, house, house.GetMultiAt(item.X - 1, item.Y - 1), minZ - 20, maxZ - 20, 192) || ValidatePlaceStructure(foundationItem, house, house.GetMultiAt(item.X, item.Y - 1), minZ - 20, maxZ - 20, 320))
			{
				Point[] array = new Point[4]
				{
					new Point(-1, 0),
					new Point(0, -1),
					new Point(1, 0),
					new Point(0, 1)
				};
				for (int i = 0; i < 4; i++)
				{
					Point point = new Point(item.X + array[i].X, item.Y + array[i].Y);
					if (!existsInList(validatedFloors, point))
					{
						validatedFloors.Add(point);
					}
				}
				return true;
			}
			return false;
		}
		if ((item.State & (CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_STAIR | CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_ROOF | CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_FIXTURE)) != 0)
		{
			foreach (Multi item2 in house.GetMultiAt(item.X, item.Y))
			{
				if (item2 != item && (item2.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_FLOOR) != 0 && item2.Z >= minZ && item2.Z < maxZ && (item2.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_VALIDATED_PLACE) != 0 && (item2.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_INCORRECT_PLACE) == 0)
				{
					return true;
				}
			}
			return false;
		}
		var (num, num2) = SeekGraphicInCustomHouseObjectList(ObjectsInfo, item.Graphic);
		if (num != -1 && num2 != -1)
		{
			CustomHousePlaceInfo customHousePlaceInfo = ObjectsInfo[num];
			if (customHousePlaceInfo.CanGoW == 0 && item.X == StartPos.X)
			{
				return false;
			}
			if (customHousePlaceInfo.CanGoN == 0 && item.Y == StartPos.Y)
			{
				return false;
			}
			if (customHousePlaceInfo.CanGoNWS == 0 && item.X == StartPos.X && item.Y == StartPos.Y)
			{
				return false;
			}
			if (customHousePlaceInfo.Bottom == 0)
			{
				bool flag = false;
				if (customHousePlaceInfo.AdjUN != 0)
				{
					flag = ValidatePlaceStructure(foundationItem, house, house.GetMultiAt(item.X, item.Y + 1), minZ, maxZ, 6);
				}
				if (!flag && customHousePlaceInfo.AdjUE != 0)
				{
					flag = ValidatePlaceStructure(foundationItem, house, house.GetMultiAt(item.X - 1, item.Y), minZ, maxZ, 10);
				}
				if (!flag && customHousePlaceInfo.AdjUS != 0)
				{
					flag = ValidatePlaceStructure(foundationItem, house, house.GetMultiAt(item.X, item.Y - 1), minZ, maxZ, 18);
				}
				if (!flag && customHousePlaceInfo.AdjUW != 0)
				{
					flag = ValidatePlaceStructure(foundationItem, house, house.GetMultiAt(item.X + 1, item.Y), minZ, maxZ, 34);
				}
				if (!flag)
				{
					return false;
				}
			}
			if (customHousePlaceInfo.Top == 0)
			{
				bool flag2 = false;
				if (customHousePlaceInfo.AdjLN != 0)
				{
					flag2 = ValidatePlaceStructure(foundationItem, house, house.GetMultiAt(item.X, item.Y + 1), minZ, maxZ, 5);
				}
				if (!flag2 && customHousePlaceInfo.AdjLE != 0)
				{
					flag2 = ValidatePlaceStructure(foundationItem, house, house.GetMultiAt(item.X - 1, item.Y), minZ, maxZ, 9);
				}
				if (!flag2 && customHousePlaceInfo.AdjLS != 0)
				{
					flag2 = ValidatePlaceStructure(foundationItem, house, house.GetMultiAt(item.X, item.Y - 1), minZ, maxZ, 17);
				}
				if (!flag2 && customHousePlaceInfo.AdjLW != 0)
				{
					flag2 = ValidatePlaceStructure(foundationItem, house, house.GetMultiAt(item.X + 1, item.Y), minZ, maxZ, 33);
				}
				if (!flag2)
				{
					return false;
				}
			}
		}
		return true;
		static bool existsInList(List<Point> list, Point testedPoint)
		{
			foreach (Point item3 in list)
			{
				if (testedPoint == item3)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool ValidatePlaceStructure(Item foundationItem, House house, IEnumerable<Multi> multi, int minZ, int maxZ, int flags)
	{
		if (house == null)
		{
			return false;
		}
		foreach (Multi item in multi)
		{
			List<Point> validatedFloors = new List<Point>();
			if (!item.IsCustom || (item.State & (CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_FLOOR | CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_STAIR | CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_ROOF | CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_FIXTURE)) != 0 || item.Z < minZ || item.Z >= maxZ)
			{
				continue;
			}
			var (num, num2) = SeekGraphicInCustomHouseObjectList(ObjectsInfo, item.Graphic);
			if (num == -1 || num2 == -1)
			{
				continue;
			}
			CustomHousePlaceInfo customHousePlaceInfo = ObjectsInfo[num];
			if ((flags & 0x40) != 0)
			{
				if ((item.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_INCORRECT_PLACE) != 0 || customHousePlaceInfo.DirectSupports == 0)
				{
					continue;
				}
				if ((flags & 0x80) != 0)
				{
					if (customHousePlaceInfo.CanGoW != 0)
					{
						return true;
					}
					continue;
				}
				if ((flags & 0x100) == 0)
				{
					return true;
				}
				if (customHousePlaceInfo.CanGoN != 0)
				{
					return true;
				}
			}
			else
			{
				if (((flags & 2) == 0 || customHousePlaceInfo.Bottom == 0) && ((flags & 1) == 0 || customHousePlaceInfo.Top == 0))
				{
					continue;
				}
				if ((item.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_VALIDATED_PLACE) == 0)
				{
					item.State |= CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_VALIDATED_PLACE;
					if (!ValidateItemPlace(foundationItem, item, minZ, maxZ, validatedFloors))
					{
						item.State = item.State | CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_VALIDATED_PLACE | CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_INCORRECT_PLACE;
					}
					else
					{
						item.State |= CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_VALIDATED_PLACE;
					}
				}
				if ((item.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_INCORRECT_PLACE) != 0)
				{
					continue;
				}
				if ((flags & 2) != 0)
				{
					if ((flags & 4) != 0 && customHousePlaceInfo.AdjUN != 0)
					{
						return true;
					}
					if ((flags & 8) != 0 && customHousePlaceInfo.AdjUE != 0)
					{
						return true;
					}
					if ((flags & 0x10) != 0 && customHousePlaceInfo.AdjUS != 0)
					{
						return true;
					}
					if ((flags & 0x20) != 0 && customHousePlaceInfo.AdjUW != 0)
					{
						return true;
					}
				}
				else
				{
					if ((flags & 4) != 0 && customHousePlaceInfo.AdjLN != 0)
					{
						return true;
					}
					if ((flags & 8) != 0 && customHousePlaceInfo.AdjLE != 0)
					{
						return true;
					}
					if ((flags & 0x10) != 0 && customHousePlaceInfo.AdjLS != 0)
					{
						return true;
					}
					if ((flags & 0x20) != 0 && customHousePlaceInfo.AdjLW != 0)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private static void ParseFile<T>(List<T> list, string path) where T : CustomHouseObject, new()
	{
		FileInfo fileInfo = new FileInfo(path);
		if (!fileInfo.Exists)
		{
			return;
		}
		using StreamReader streamReader = File.OpenText(fileInfo.FullName);
		while (!streamReader.EndOfStream)
		{
			string text = streamReader.ReadLine();
			if (!string.IsNullOrWhiteSpace(text))
			{
				T val = new T();
				if (val.Parse(text) && (val.FeatureMask == 0 || ((uint)World.ClientLockedFeatures.Flags & (uint)val.FeatureMask) != 0))
				{
					list.Add(val);
				}
			}
		}
	}

	private static void ParseFileWithCategory<T, U>(List<U> list, string path) where T : CustomHouseObject, new() where U : CustomHouseObjectCategory<T>, new()
	{
		FileInfo fileInfo = new FileInfo(path);
		if (!fileInfo.Exists)
		{
			return;
		}
		using StreamReader streamReader = File.OpenText(fileInfo.FullName);
		while (!streamReader.EndOfStream)
		{
			string text = streamReader.ReadLine();
			if (string.IsNullOrWhiteSpace(text))
			{
				continue;
			}
			T val = new T();
			if (!val.Parse(text) || (val.FeatureMask != 0 && ((uint)World.ClientLockedFeatures.Flags & (uint)val.FeatureMask) == 0))
			{
				continue;
			}
			bool flag = false;
			foreach (U item in list)
			{
				if (item.Index == val.Category)
				{
					item.Items.Add(val);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				U val2 = new U
				{
					Index = val.Category
				};
				val2.Items.Add(val);
				list.Add(val2);
			}
		}
	}

	private static (int, int) SeekGraphicInCustomHouseObjectListWithCategory<T, U>(List<U> list, ushort graphic) where T : CustomHouseObject where U : CustomHouseObjectCategory<T>
	{
		for (int i = 0; i < list.Count; i++)
		{
			U val = list[i];
			for (int j = 0; j < val.Items.Count; j++)
			{
				if (val.Items[j].Contains(graphic) != -1)
				{
					return (i, j);
				}
			}
		}
		return (-1, -1);
	}

	private static (int, int) SeekGraphicInCustomHouseObjectList<T>(List<T> list, ushort graphic) where T : CustomHouseObject
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].Contains(graphic) != -1)
			{
				return (i, graphic);
			}
		}
		return (-1, -1);
	}
}

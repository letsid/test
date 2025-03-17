using System;
using System.Collections.Generic;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.IO.Resources;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game;

internal static class Pathfinder
{
	private enum PATH_STEP_STATE
	{
		PSS_NORMAL,
		PSS_DEAD_OR_GM,
		PSS_ON_SEA_HORSE,
		PSS_FLYING
	}

	[Flags]
	private enum PATH_OBJECT_FLAGS : uint
	{
		POF_IMPASSABLE_OR_SURFACE = 1u,
		POF_SURFACE = 2u,
		POF_BRIDGE = 4u,
		POF_NO_DIAGONAL = 8u
	}

	private class PathObject : IComparable<PathObject>
	{
		public uint Flags { get; }

		public int Z { get; }

		public int AverageZ { get; }

		public int Height { get; }

		public GameObject Object { get; }

		public PathObject(uint flags, int z, int avgZ, int h, GameObject obj)
		{
			Flags = flags;
			Z = z;
			AverageZ = avgZ;
			Height = h;
			Object = obj;
		}

		public int CompareTo(PathObject other)
		{
			int num = Z - other.Z;
			if (num == 0)
			{
				num = Height - other.Height;
			}
			return num;
		}
	}

	private class PathNode
	{
		public int X { get; set; }

		public int Y { get; set; }

		public int Z { get; set; }

		public int Direction { get; set; }

		public bool Used { get; set; }

		public int Cost { get; set; }

		public int DistFromStartCost { get; set; }

		public int DistFromGoalCost { get; set; }

		public PathNode Parent { get; set; }

		public void Reset()
		{
			Parent = null;
			Used = false;
			int num2 = (DistFromStartCost = 0);
			int num4 = (DistFromGoalCost = num2);
			int num6 = (Cost = num4);
			int num8 = (Direction = num6);
			int num10 = (Z = num8);
			int x = (Y = num10);
			X = x;
		}
	}

	private const int PATHFINDER_MAX_NODES = 10000;

	private static int _goalNode;

	private static bool _goalFound;

	private static int _activeOpenNodes;

	private static int _activeCloseNodes;

	private static int _pathfindDistance;

	private static readonly PathNode[] _openList = new PathNode[10000];

	private static readonly PathNode[] _closedList = new PathNode[10000];

	private static readonly PathNode[] _path = new PathNode[10000];

	private static int _pointIndex;

	private static int _pathSize;

	private static bool _run;

	private static readonly int[] _offsetX = new int[10] { 0, 1, 1, 1, 0, -1, -1, -1, 0, 1 };

	private static readonly int[] _offsetY = new int[10] { -1, -1, 0, 1, 1, 1, 0, -1, -1, -1 };

	private static readonly sbyte[] _dirOffset = new sbyte[2] { 1, -1 };

	private static Point _startPoint;

	private static Point _endPoint;

	public static bool AutoWalking { get; set; }

	public static bool PathindingCanBeCancelled { get; set; }

	public static bool BlockMoving { get; set; }

	public static bool FastRotation { get; set; }

	public static List<ushort> ShovableAnims { get; set; } = new List<ushort>();

	private static bool CreateItemList(List<PathObject> list, int x, int y, int stepState)
	{
		GameObject tile = World.Map.GetTile(x, y, load: false);
		if (tile == null)
		{
			return false;
		}
		bool flag = ProfileManager.CurrentProfile.IgnoreStaminaCheck || stepState == 1 || World.Player.IgnoreCharacters || World.Player.Stamina >= World.Player.StaminaMax || World.Map.Index != 0;
		if (!flag)
		{
			foreach (KeyValuePair<uint, Mobile> mobile3 in World.Mobiles)
			{
				Mobile value = mobile3.Value;
				if (value.Steps.Count != 0)
				{
					Mobile.Step step = value.Steps.Front();
					if (step.X == x && step.Y == y)
					{
						list.Add(new PathObject(1u, step.Z, step.Z + 16, 16, value));
					}
				}
			}
		}
		bool flag2 = World.Player.Graphic == 987;
		GameObject gameObject = tile;
		while (gameObject.TPrevious != null)
		{
			gameObject = gameObject.TPrevious;
		}
		while (gameObject != null)
		{
			if (World.CustomHouseManager == null || gameObject.Z >= World.Player.Z)
			{
				ushort graphic = gameObject.Graphic;
				if (!(gameObject is Land land))
				{
					if (!(gameObject is GameEffect))
					{
						bool flag3 = true;
						bool flag4 = false;
						GameObject gameObject2 = gameObject;
						if (!(gameObject2 is Mobile mobile))
						{
							if (!(gameObject2 is Item item))
							{
								if (gameObject2 is Multi multi)
								{
									Multi multi2 = multi;
									if ((World.CustomHouseManager != null && multi2.IsCustom && (multi2.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_GENERIC_INTERNAL) == 0) || multi2.IsHousePreview)
									{
										flag3 = false;
									}
									if ((multi2.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_IGNORE_IN_RENDER) != 0)
									{
										flag4 = true;
									}
								}
							}
							else
							{
								Item item2 = item;
								if (item2.IsMulti || item2.ItemData.IsInternal)
								{
									flag3 = false;
								}
								else
								{
									Item item3 = item;
									flag4 = (stepState == 1 && (item3.ItemData.IsDoor || item3.ItemData.Weight <= 90 || (flag2 && !item3.IsLocked))) || (ProfileManager.CurrentProfile.SmoothDoors && item3.ItemData.IsDoor) || (graphic >= 14662 && graphic <= 14692) || graphic == 130;
								}
							}
						}
						else
						{
							Mobile mobile2 = mobile;
							if (!flag && !mobile2.IsDead && !mobile2.IgnoreCharacters && mobile2.Steps.Count == 0 && !ShovableAnims.Contains(mobile2.Graphic))
							{
								list.Add(new PathObject(1u, mobile2.Z, mobile2.Z + 16, 16, mobile2));
							}
							flag3 = false;
						}
						if (flag3)
						{
							uint num = 0u;
							if (!(gameObject is Mobile))
							{
								ref StaticTiles reference = ref TileDataLoader.Instance.StaticData[gameObject.Graphic];
								if (stepState == 2)
								{
									if (reference.IsWet)
									{
										num = 6u;
									}
								}
								else
								{
									if (reference.IsImpassable || reference.IsSurface)
									{
										num = 1u;
									}
									if (!reference.IsImpassable)
									{
										if (reference.IsSurface)
										{
											num |= 2;
										}
										if (reference.IsBridge)
										{
											num |= 4;
										}
									}
									if (stepState == 1)
									{
										switch (graphic)
										{
										case 1682:
										case 1781:
										case 1782:
										case 2118:
											flag4 = true;
											break;
										case 2163:
											flag4 = true;
											break;
										}
									}
									if (flag4)
									{
										num &= 0xFFFFFFFEu;
									}
									if (stepState == 3 && reference.IsNoDiagonal)
									{
										num |= 8;
									}
								}
								if (num != 0)
								{
									int z = gameObject.Z;
									int height = reference.Height;
									int num2 = height;
									if (reference.IsBridge)
									{
										num2 /= 2;
									}
									list.Add(new PathObject(num, z, num2 + z, height, gameObject));
								}
							}
						}
					}
				}
				else if ((graphic < 430 && graphic != 2) || (graphic > 437 && graphic != 475))
				{
					uint num3 = 1u;
					if (stepState == 2)
					{
						if (land.TileData.IsWet)
						{
							num3 = 7u;
						}
					}
					else
					{
						if (!land.TileData.IsImpassable)
						{
							num3 = 7u;
						}
						if (stepState == 3 && land.TileData.IsNoDiagonal)
						{
							num3 |= 8;
						}
					}
					int minZ = land.MinZ;
					int averageZ = land.AverageZ;
					int h = averageZ - minZ;
					list.Add(new PathObject(num3, minZ, averageZ, h, gameObject));
				}
			}
			gameObject = gameObject.TNext;
		}
		return list.Count != 0;
	}

	private static int CalculateMinMaxZ(ref int minZ, ref int maxZ, int newX, int newY, int currentZ, int newDirection, int stepState)
	{
		minZ = -128;
		maxZ = currentZ;
		newDirection &= 7;
		int num = newDirection ^ 4;
		newX += _offsetX[num];
		newY += _offsetY[num];
		List<PathObject> list = new List<PathObject>();
		if (!CreateItemList(list, newX, newY, stepState) || list.Count == 0)
		{
			return 0;
		}
		foreach (PathObject item in list)
		{
			GameObject @object = item.Object;
			int averageZ = item.AverageZ;
			if (averageZ <= currentZ && @object is Land { IsStretched: not false } land)
			{
				int num2 = land.CalculateCurrentAverageZ(newDirection);
				if (minZ < num2)
				{
					minZ = num2;
				}
				if (maxZ < num2)
				{
					maxZ = num2;
				}
				continue;
			}
			if ((item.Flags & 1) != 0 && averageZ <= currentZ && minZ < averageZ)
			{
				minZ = averageZ;
			}
			if ((item.Flags & 4) != 0 && currentZ == averageZ)
			{
				int z = item.Z;
				int num3 = z + item.Height;
				if (maxZ < num3)
				{
					maxZ = num3;
				}
				if (minZ > z)
				{
					minZ = z;
				}
			}
		}
		maxZ += 2;
		return maxZ;
	}

	public static bool CalculateNewZ(int x, int y, ref sbyte z, int direction)
	{
		int num = 0;
		if (World.Player.IsDead || World.Player.Graphic == 987)
		{
			num = 1;
		}
		else if (World.Player.IsGargoyle && World.Player.IsFlying)
		{
			num = 3;
		}
		else
		{
			Item item = World.Player.FindItemByLayer(Layer.Mount);
			if (item != null && item.Graphic == 16051)
			{
				num = 2;
			}
		}
		int minZ = -128;
		int maxZ = z;
		CalculateMinMaxZ(ref minZ, ref maxZ, x, y, z, direction, num);
		List<PathObject> list = new List<PathObject>();
		if (World.CustomHouseManager != null && !new Rectangle(World.CustomHouseManager.StartPos.X, World.CustomHouseManager.StartPos.Y, World.CustomHouseManager.EndPos.X, World.CustomHouseManager.EndPos.Y).Contains(x, y))
		{
			return false;
		}
		if (!CreateItemList(list, x, y, num) || list.Count == 0)
		{
			return false;
		}
		list.Sort();
		list.Add(new PathObject(1u, 128, 128, 128, null));
		int num2 = -128;
		if (z < minZ)
		{
			z = (sbyte)minZ;
		}
		int num3 = 1000000;
		int num4 = -128;
		for (int i = 0; i < list.Count; i++)
		{
			PathObject pathObject = list[i];
			if ((pathObject.Flags & 8) != 0 && num == 3)
			{
				int averageZ = pathObject.AverageZ;
				if (Math.Abs(averageZ - z) <= 25)
				{
					num2 = ((averageZ != -128) ? averageZ : num4);
					break;
				}
			}
			if ((pathObject.Flags & 1) == 0)
			{
				continue;
			}
			int z2 = pathObject.Z;
			if (z2 - minZ >= 16)
			{
				for (int num5 = i - 1; num5 >= 0; num5--)
				{
					PathObject pathObject2 = list[num5];
					if ((pathObject2.Flags & 6) != 0)
					{
						int averageZ2 = pathObject2.AverageZ;
						if (averageZ2 >= num4 && z2 - averageZ2 >= 16 && ((averageZ2 <= maxZ && (pathObject2.Flags & 2) != 0) || ((pathObject2.Flags & 4) != 0 && pathObject2.Z <= maxZ)))
						{
							int num6 = Math.Abs(z - averageZ2);
							if (num6 < num3)
							{
								num3 = num6;
								num2 = averageZ2;
							}
						}
					}
				}
			}
			int averageZ3 = pathObject.AverageZ;
			if (minZ < averageZ3)
			{
				minZ = averageZ3;
			}
			if (num4 < averageZ3)
			{
				num4 = averageZ3;
			}
		}
		z = (sbyte)num2;
		return num2 != -128;
	}

	public static void GetNewXY(byte direction, ref int x, ref int y)
	{
		switch (direction & 7)
		{
		case 0:
			y--;
			break;
		case 1:
			x++;
			y--;
			break;
		case 2:
			x++;
			break;
		case 3:
			x++;
			y++;
			break;
		case 4:
			y++;
			break;
		case 5:
			x--;
			y++;
			break;
		case 6:
			x--;
			break;
		case 7:
			x--;
			y--;
			break;
		}
	}

	public static bool CanWalk(ref Direction direction, ref int x, ref int y, ref sbyte z)
	{
		int x2 = x;
		int y2 = y;
		sbyte z2 = z;
		byte b = (byte)direction;
		GetNewXY((byte)direction, ref x2, ref y2);
		bool flag = CalculateNewZ(x2, y2, ref z2, (int)direction);
		if ((sbyte)direction % 2 != 0)
		{
			if (flag)
			{
				for (int i = 0; i < 2 && flag; i++)
				{
					int x3 = x;
					int y3 = y;
					sbyte z3 = z;
					byte direction2 = (byte)(((int)direction + (int)_dirOffset[i]) % 8);
					GetNewXY(direction2, ref x3, ref y3);
					flag = CalculateNewZ(x3, y3, ref z3, direction2);
				}
			}
			if (!flag)
			{
				for (int j = 0; j < 2; j++)
				{
					if (flag)
					{
						break;
					}
					x2 = x;
					y2 = y;
					z2 = z;
					b = (byte)(((int)direction + (int)_dirOffset[j]) % 8);
					GetNewXY(b, ref x2, ref y2);
					flag = CalculateNewZ(x2, y2, ref z2, b);
				}
			}
		}
		if (flag)
		{
			x = x2;
			y = y2;
			z = z2;
			direction = (Direction)b;
		}
		return flag;
	}

	private static int GetGoalDistCost(Point point, int cost)
	{
		return Math.Max(Math.Abs(_endPoint.X - point.X), Math.Abs(_endPoint.Y - point.Y));
	}

	private static bool DoesNotExistOnOpenList(int x, int y, int z)
	{
		for (int i = 0; i < 10000; i++)
		{
			PathNode pathNode = _openList[i];
			if (pathNode.Used && pathNode.X == x && pathNode.Y == y && pathNode.Z == z)
			{
				return true;
			}
		}
		return false;
	}

	private static bool DoesNotExistOnClosedList(int x, int y, int z)
	{
		for (int i = 0; i < 10000; i++)
		{
			PathNode pathNode = _closedList[i];
			if (pathNode.Used && pathNode.X == x && pathNode.Y == y && pathNode.Z == z)
			{
				return true;
			}
		}
		return false;
	}

	private static int AddNodeToList(int list, int direction, int x, int y, int z, PathNode parent, int cost)
	{
		if (list == 0)
		{
			if (DoesNotExistOnClosedList(x, y, z))
			{
				return 0;
			}
			if (!DoesNotExistOnOpenList(x, y, z))
			{
				for (int i = 0; i < 10000; i++)
				{
					PathNode pathNode = _openList[i];
					if (!pathNode.Used)
					{
						pathNode.Used = true;
						pathNode.Direction = direction;
						pathNode.X = x;
						pathNode.Y = y;
						pathNode.Z = z;
						Point point = new Point(x, y);
						pathNode.DistFromGoalCost = GetGoalDistCost(point, cost);
						pathNode.DistFromStartCost = parent.DistFromStartCost + cost;
						pathNode.Cost = pathNode.DistFromGoalCost + pathNode.DistFromStartCost;
						pathNode.Parent = parent;
						if (ClassicUO.Utility.MathHelper.GetDistance(_endPoint, point) <= _pathfindDistance)
						{
							_goalFound = true;
							_goalNode = i;
						}
						_activeOpenNodes++;
						return i;
					}
				}
			}
			else
			{
				for (int j = 0; j < 10000; j++)
				{
					PathNode pathNode2 = _openList[j];
					if (pathNode2.Used && pathNode2.X == x && pathNode2.Y == y && pathNode2.Z == z)
					{
						int num = parent.DistFromStartCost + cost;
						if (pathNode2.DistFromStartCost > num)
						{
							pathNode2.Parent = parent;
							pathNode2.DistFromStartCost = num + cost;
							pathNode2.Cost = pathNode2.DistFromGoalCost + pathNode2.DistFromStartCost;
						}
						return j;
					}
				}
			}
		}
		else
		{
			parent.Used = false;
			for (int k = 0; k < 10000; k++)
			{
				PathNode pathNode3 = _closedList[k];
				if (!pathNode3.Used)
				{
					pathNode3.Used = true;
					pathNode3.DistFromGoalCost = parent.DistFromGoalCost;
					pathNode3.DistFromStartCost = parent.DistFromStartCost;
					pathNode3.Cost = pathNode3.DistFromGoalCost + pathNode3.DistFromStartCost;
					pathNode3.Direction = parent.Direction;
					pathNode3.X = parent.X;
					pathNode3.Y = parent.Y;
					pathNode3.Z = parent.Z;
					pathNode3.Parent = parent.Parent;
					_activeOpenNodes--;
					_activeCloseNodes++;
					return k;
				}
			}
		}
		return -1;
	}

	private static bool OpenNodes(PathNode node)
	{
		bool result = false;
		for (int i = 0; i < 8; i++)
		{
			Direction direction = (Direction)i;
			int x = node.X;
			int y = node.Y;
			sbyte z = (sbyte)node.Z;
			Direction direction2 = direction;
			if (!CanWalk(ref direction, ref x, ref y, ref z) || direction != direction2)
			{
				continue;
			}
			int num = i % 2;
			if (num != 0)
			{
				byte direction3 = (byte)i;
				int x2 = node.X;
				int y2 = node.Y;
				GetNewXY(direction3, ref x2, ref y2);
				if (x != x2 || y != y2)
				{
					num = -1;
				}
			}
			if (num >= 0 && AddNodeToList(0, (int)direction, x, y, z, node, (num == 0) ? 1 : 2) != -1)
			{
				result = true;
			}
		}
		return result;
	}

	private static int FindCheapestNode()
	{
		int num = 9999999;
		int num2 = -1;
		for (int i = 0; i < 10000; i++)
		{
			if (_openList[i].Used && _openList[i].Cost < num)
			{
				num2 = i;
				num = _openList[i].Cost;
			}
		}
		int result = -1;
		if (num2 != -1)
		{
			result = AddNodeToList(1, 0, 0, 0, 0, _openList[num2], 2);
		}
		return result;
	}

	private static bool FindPath(int maxNodes)
	{
		int num = 0;
		_closedList[0].Used = true;
		_closedList[0].X = _startPoint.X;
		_closedList[0].Y = _startPoint.Y;
		_closedList[0].Z = World.Player.Z;
		_closedList[0].Parent = null;
		_closedList[0].DistFromGoalCost = GetGoalDistCost(_startPoint, 0);
		_closedList[0].Cost = _closedList[0].DistFromGoalCost;
		if (GetGoalDistCost(_startPoint, 0) > 14)
		{
			_run = true;
		}
		while (AutoWalking)
		{
			OpenNodes(_closedList[num]);
			if (_goalFound)
			{
				int num2 = 0;
				PathNode pathNode = _openList[_goalNode];
				while (pathNode.Parent != null && pathNode != pathNode.Parent)
				{
					pathNode = pathNode.Parent;
					num2++;
				}
				num2 = (_pathSize = num2 + 1);
				pathNode = _openList[_goalNode];
				while (num2 != 0)
				{
					num2--;
					_path[num2] = pathNode;
					pathNode = pathNode.Parent;
				}
				break;
			}
			num = FindCheapestNode();
			if (num == -1)
			{
				return false;
			}
			if (_activeCloseNodes >= maxNodes)
			{
				return false;
			}
		}
		return true;
	}

	public static bool WalkTo(int x, int y, int z, int distance)
	{
		if (World.Player == null || World.Player.IsParalyzed)
		{
			return false;
		}
		for (int i = 0; i < 10000; i++)
		{
			if (_openList[i] == null)
			{
				_openList[i] = new PathNode();
			}
			_openList[i].Reset();
			if (_closedList[i] == null)
			{
				_closedList[i] = new PathNode();
			}
			_closedList[i].Reset();
		}
		int x2 = World.Player.X;
		int y2 = World.Player.Y;
		_startPoint.X = x2;
		_startPoint.Y = y2;
		_endPoint.X = x;
		_endPoint.Y = y;
		_goalNode = 0;
		_goalFound = false;
		_activeOpenNodes = 0;
		_activeCloseNodes = 0;
		_pathfindDistance = distance;
		_pathSize = 0;
		PathindingCanBeCancelled = true;
		StopAutoWalk();
		AutoWalking = true;
		if (FindPath(10000))
		{
			_pointIndex = 1;
			ProcessAutoWalk();
		}
		else
		{
			AutoWalking = false;
		}
		return _pathSize != 0;
	}

	public static void ProcessAutoWalk()
	{
		if (!AutoWalking || !World.InGame || World.Player.Walker.StepsCount >= 5 || World.Player.Walker.LastStepRequestTime > Time.Ticks)
		{
			return;
		}
		if (_pointIndex >= 0 && _pointIndex < _pathSize)
		{
			PathNode pathNode = _path[_pointIndex];
			World.Player.GetEndPosition(out var _, out var _, out var _, out var dir);
			if ((uint)dir == (byte)pathNode.Direction)
			{
				_pointIndex++;
			}
			if (!World.Player.Walk((Direction)pathNode.Direction, _run))
			{
				StopAutoWalk();
			}
		}
		else
		{
			StopAutoWalk();
		}
	}

	public static void StopAutoWalk()
	{
		AutoWalking = false;
		_run = false;
		_pathSize = 0;
	}
}

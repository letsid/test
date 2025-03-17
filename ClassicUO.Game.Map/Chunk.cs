using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.IO.Resources;
using ClassicUO.Utility;

namespace ClassicUO.Game.Map;

internal sealed class Chunk
{
	private static readonly QueuedPool<Chunk> _pool = new QueuedPool<Chunk>(300, delegate(Chunk c)
	{
		c.LastAccessTime = Time.Ticks + 3000;
		c.IsDestroyed = false;
	});

	public bool IsDestroyed;

	public long LastAccessTime;

	public LinkedListNode<int> Node;

	public int X;

	public int Y;

	public GameObject[,] Tiles { get; } = new GameObject[8, 8];

	public static Chunk Create(int x, int y)
	{
		Chunk one = _pool.GetOne();
		one.X = x;
		one.Y = y;
		return one;
	}

	public unsafe void Load(int index)
	{
		IsDestroyed = false;
		Map map = World.Map;
		ref IndexMap index2 = ref GetIndex(index);
		if (index2.MapAddress == 0L)
		{
			return;
		}
		MapBlock* ptr = (MapBlock*)index2.MapAddress;
		MapCells* ptr2 = (MapCells*)(&ptr->Cells);
		int num = X << 3;
		int num2 = Y << 3;
		for (int i = 0; i < 8; i++)
		{
			int num3 = i << 3;
			ushort y = (ushort)(num2 + i);
			int num4 = 0;
			while (num4 < 8)
			{
				ushort graphic = (ushort)(ptr2[num3].TileID & 0x3FFF);
				sbyte z = ptr2[num3].Z;
				Land land = Land.Create(graphic);
				ushort x = (ushort)(num + num4);
				land.ApplyStretch(map, x, y, z);
				land.X = x;
				land.Y = y;
				land.Z = z;
				land.UpdateScreenPosition();
				land.UpdateGraphicBySeason();
				AddGameObject(land, num4, i);
				num4++;
				num3++;
			}
		}
		if (index2.StaticAddress == 0L)
		{
			return;
		}
		StaticsBlock* ptr3 = (StaticsBlock*)index2.StaticAddress;
		if (ptr3 == null)
		{
			return;
		}
		int num5 = 0;
		int staticCount = (int)index2.StaticCount;
		while (num5 < staticCount)
		{
			if (ptr3->Color != 0 && ptr3->Color != ushort.MaxValue)
			{
				int num6 = (ptr3->Y << 3) + ptr3->X;
				if (num6 < 64)
				{
					Static @static = Static.Create(ptr3->Color, ptr3->Hue, num6);
					@static.X = (ushort)(num + ptr3->X);
					@static.Y = (ushort)(num2 + ptr3->Y);
					@static.Z = ptr3->Z;
					@static.UpdateScreenPosition();
					@static.UpdateGraphicBySeason();
					AddGameObject(@static, ptr3->X, ptr3->Y);
				}
			}
			num5++;
			ptr3++;
		}
	}

	private ref IndexMap GetIndex(int map)
	{
		MapLoader.Instance.SanitizeMapIndex(ref map);
		return ref MapLoader.Instance.GetIndex(map, X, Y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public GameObject GetHeadObject(int x, int y)
	{
		GameObject gameObject = Tiles[x, y];
		while (gameObject?.TPrevious != null)
		{
			gameObject = gameObject.TPrevious;
		}
		return gameObject;
	}

	public void AddGameObject(GameObject obj, int x, int y)
	{
		obj.RemoveFromTile();
		short num = obj.Z;
		sbyte b = -1;
		ushort num2 = obj.Graphic;
		if (!(obj is Land land))
		{
			if (!(obj is Mobile))
			{
				if (!(obj is Item item))
				{
					if (!(obj is GameEffect))
					{
						if (obj is Multi multi)
						{
							b = 1;
							if ((multi.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_GENERIC_INTERNAL) != 0)
							{
								num--;
								goto IL_010a;
							}
							if ((multi.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_PREVIEW) != 0)
							{
								b = 2;
								num++;
							}
						}
						goto IL_00d3;
					}
					num += 200;
				}
				else
				{
					if (!item.IsCorpse)
					{
						if (item.IsMulti)
						{
							num2 = item.MultiGraphic;
						}
						goto IL_00d3;
					}
					num++;
				}
			}
			else
			{
				num++;
			}
		}
		else
		{
			num = ((!land.IsStretched) ? ((short)(num - 1)) : ((short)(land.AverageZ - 1)));
			b = 0;
		}
		goto IL_010a;
		IL_00d3:
		ref StaticTiles reference = ref TileDataLoader.Instance.StaticData[num2];
		if (reference.IsBackground)
		{
			num--;
		}
		if (reference.Height != 0)
		{
			num++;
		}
		if (reference.IsMultiMovable)
		{
			num++;
		}
		goto IL_010a;
		IL_010a:
		obj.PriorityZ = num;
		if (Tiles[x, y] == null)
		{
			Tiles[x, y] = obj;
			obj.TPrevious = null;
			obj.TNext = null;
			return;
		}
		GameObject gameObject = Tiles[x, y];
		if (gameObject == obj)
		{
			if (gameObject.Previous != null)
			{
				gameObject = (GameObject)gameObject.Previous;
			}
			else
			{
				if (gameObject.Next == null)
				{
					return;
				}
				gameObject = (GameObject)gameObject.Next;
			}
		}
		while (gameObject?.TPrevious != null)
		{
			gameObject = gameObject.TPrevious;
		}
		GameObject gameObject2 = null;
		GameObject gameObject3 = gameObject;
		for (; gameObject != null; gameObject2 = gameObject, gameObject = gameObject.TNext)
		{
			int priorityZ = gameObject.PriorityZ;
			if (priorityZ > num)
			{
				break;
			}
			if (priorityZ != num)
			{
				continue;
			}
			switch (b)
			{
			case 1:
				if (gameObject is Land)
				{
					continue;
				}
				break;
			default:
				continue;
			case 0:
				break;
			}
			break;
		}
		if (gameObject2 != null)
		{
			obj.TPrevious = gameObject2;
			GameObject gameObject4 = (obj.TNext = gameObject2.TNext);
			gameObject2.TNext = obj;
			if (gameObject4 != null)
			{
				gameObject4.TPrevious = obj;
			}
		}
		else if (gameObject3 != null)
		{
			obj.TNext = gameObject3;
			gameObject3.TPrevious = obj;
			obj.TPrevious = null;
		}
	}

	public void RemoveGameObject(GameObject obj, int x, int y)
	{
		ref GameObject reference = ref Tiles[x, y];
		if (reference != null && obj != null)
		{
			if (reference == obj)
			{
				reference = obj.TNext;
			}
			if (obj.TNext != null)
			{
				obj.TNext.TPrevious = obj.TPrevious;
			}
			if (obj.TPrevious != null)
			{
				obj.TPrevious.TNext = obj.TNext;
			}
			obj.TPrevious = null;
			obj.TNext = null;
		}
	}

	public void Destroy()
	{
		for (int i = 0; i < 8; i++)
		{
			for (int j = 0; j < 8; j++)
			{
				if (Tiles[i, j] == null)
				{
					continue;
				}
				GameObject gameObject = GetHeadObject(i, j);
				while (gameObject != null)
				{
					GameObject tNext = gameObject.TNext;
					if (gameObject != World.Player)
					{
						gameObject.Destroy();
					}
					gameObject.TPrevious = null;
					gameObject.TNext = null;
					gameObject = tNext;
				}
				Tiles[i, j] = null;
			}
		}
		if (Node.Next != null || Node.Previous != null)
		{
			Node.List?.Remove(Node);
		}
		IsDestroyed = true;
		_pool.ReturnOne(this);
	}

	public void Clear()
	{
		for (int i = 0; i < 8; i++)
		{
			for (int j = 0; j < 8; j++)
			{
				if (Tiles[i, j] == null)
				{
					continue;
				}
				GameObject gameObject = GetHeadObject(i, j);
				while (gameObject != null)
				{
					GameObject tNext = gameObject.TNext;
					if (gameObject != World.Player)
					{
						gameObject.Destroy();
					}
					gameObject.TPrevious = null;
					gameObject.TNext = null;
					gameObject = tNext;
				}
				Tiles[i, j] = null;
			}
		}
		if (Node.Next != null || Node.Previous != null)
		{
			Node.List?.Remove(Node);
		}
		IsDestroyed = true;
	}

	public bool HasNoExternalData()
	{
		for (int i = 0; i < 8; i++)
		{
			for (int j = 0; j < 8; j++)
			{
				for (GameObject gameObject = GetHeadObject(i, j); gameObject != null; gameObject = gameObject.TNext)
				{
					if (!(gameObject is Land) && !(gameObject is Static))
					{
						return false;
					}
				}
			}
		}
		return true;
	}
}

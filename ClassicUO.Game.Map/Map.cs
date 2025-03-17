using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ClassicUO.Game.GameObjects;
using ClassicUO.IO.Resources;

namespace ClassicUO.Game.Map;

internal sealed class Map
{
	private static readonly Chunk[] _terrainChunks;

	private static readonly bool[] _blockAccessList;

	private readonly LinkedList<int> _usedIndices = new LinkedList<int>();

	public readonly int BlocksCount;

	public readonly int Index;

	static Map()
	{
		_blockAccessList = new bool[4096];
		int num = -1;
		int num2 = -1;
		for (int i = 0; i < MapLoader.Instance.MapBlocksSize.GetLength(0); i++)
		{
			if (num < MapLoader.Instance.MapBlocksSize[i, 0])
			{
				num = MapLoader.Instance.MapBlocksSize[i, 0];
			}
			if (num2 < MapLoader.Instance.MapBlocksSize[i, 1])
			{
				num2 = MapLoader.Instance.MapBlocksSize[i, 1];
			}
		}
		_terrainChunks = new Chunk[num * num2];
	}

	public Map(int index)
	{
		Index = index;
		BlocksCount = MapLoader.Instance.MapBlocksSize[Index, 0] * MapLoader.Instance.MapBlocksSize[Index, 1];
		ClearBockAccess();
	}

	public Chunk GetChunk(int block)
	{
		if (block >= 0 && block < BlocksCount)
		{
			return _terrainChunks[block];
		}
		return null;
	}

	public Chunk GetChunk(int x, int y, bool load = true)
	{
		if (x < 0 || y < 0)
		{
			return null;
		}
		int num = x >> 3;
		int num2 = y >> 3;
		int block = GetBlock(num, num2);
		if (block >= BlocksCount)
		{
			return null;
		}
		ref Chunk reference = ref _terrainChunks[block];
		if (reference == null)
		{
			if (!load)
			{
				return null;
			}
			LinkedListNode<int> node = _usedIndices.AddLast(block);
			reference = Chunk.Create(num, num2);
			reference.Load(Index);
			reference.Node = node;
		}
		else if (reference.IsDestroyed)
		{
			if (reference.Node != null && (reference.Node.Previous != null || reference.Node.Next != null))
			{
				reference.Node.List?.Remove(reference.Node);
			}
			LinkedListNode<int> node2 = _usedIndices.AddLast(block);
			reference.X = num;
			reference.Y = num2;
			reference.Load(Index);
			reference.Node = node2;
		}
		reference.LastAccessTime = Time.Ticks;
		return reference;
	}

	public GameObject GetTile(int x, int y, bool load = true)
	{
		return GetChunk(x, y, load)?.GetHeadObject(x % 8, y % 8);
	}

	public unsafe sbyte GetTileZ(int x, int y)
	{
		if (x < 0 || y < 0)
		{
			return -125;
		}
		ref IndexMap index = ref GetIndex(x >> 3, y >> 3);
		if (index.MapAddress == 0L)
		{
			return -125;
		}
		int num = x % 8;
		int num2 = y % 8;
		MapBlock* ptr = (MapBlock*)index.MapAddress;
		MapCells* ptr2 = (MapCells*)(&ptr->Cells);
		return ptr2[(num2 << 3) + num].Z;
	}

	public void GetMapZ(int x, int y, out sbyte groundZ, out sbyte staticZ)
	{
		Chunk chunk = GetChunk(x, y);
		groundZ = (staticZ = 0);
		if (chunk == null)
		{
			return;
		}
		for (GameObject gameObject = chunk.Tiles[x % 8, y % 8]; gameObject != null; gameObject = gameObject.TNext)
		{
			if (gameObject is Land)
			{
				groundZ = gameObject.Z;
			}
			else if (staticZ < gameObject.Z)
			{
				staticZ = gameObject.Z;
			}
		}
	}

	public void ClearBockAccess()
	{
		_blockAccessList.AsSpan().Fill(value: false);
	}

	public sbyte CalculateNearZ(sbyte defaultZ, int x, int y, int z)
	{
		ref bool reference = ref _blockAccessList[(x & 0x3F) + ((y & 0x3F) << 6)];
		if (reference)
		{
			return defaultZ;
		}
		reference = true;
		Chunk chunk = GetChunk(x, y, load: false);
		if (chunk != null)
		{
			GameObject gameObject = chunk.Tiles[x % 8, y % 8];
			while (gameObject != null && ((!(gameObject is Static) && !(gameObject is Multi)) || gameObject.Graphic >= TileDataLoader.Instance.StaticData.Length || !TileDataLoader.Instance.StaticData[gameObject.Graphic].IsRoof || Math.Abs(z - gameObject.Z) > 6))
			{
				gameObject = gameObject.TNext;
			}
			if (gameObject == null)
			{
				return defaultZ;
			}
			sbyte z2 = gameObject.Z;
			if (z2 < defaultZ)
			{
				defaultZ = z2;
			}
			defaultZ = CalculateNearZ(defaultZ, x - 1, y, z2);
			defaultZ = CalculateNearZ(defaultZ, x + 1, y, z2);
			defaultZ = CalculateNearZ(defaultZ, x, y - 1, z2);
			defaultZ = CalculateNearZ(defaultZ, x, y + 1, z2);
		}
		return defaultZ;
	}

	public ref IndexMap GetIndex(int blockX, int blockY)
	{
		int block = GetBlock(blockX, blockY);
		int map = Index;
		MapLoader.Instance.SanitizeMapIndex(ref map);
		IndexMap[] array = MapLoader.Instance.BlockData[map];
		if (block < array.Length)
		{
			return ref array[block];
		}
		return ref IndexMap.Invalid;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private int GetBlock(int blockX, int blockY)
	{
		return blockX * MapLoader.Instance.MapBlocksSize[Index, 1] + blockY;
	}

	public IEnumerable<Chunk> GetUsedChunks()
	{
		foreach (int usedIndex in _usedIndices)
		{
			yield return GetChunk(usedIndex);
		}
	}

	public void ClearUnusedBlocks()
	{
		int num = 0;
		long num2 = Time.Ticks - 3000;
		LinkedListNode<int> linkedListNode = _usedIndices.First;
		while (linkedListNode != null)
		{
			LinkedListNode<int> next = linkedListNode.Next;
			ref Chunk reference = ref _terrainChunks[linkedListNode.Value];
			if (reference != null && reference.LastAccessTime < num2 && reference.HasNoExternalData())
			{
				reference.Destroy();
				reference = null;
				if (++num >= 50)
				{
					break;
				}
			}
			linkedListNode = next;
		}
	}

	public void Destroy()
	{
		LinkedListNode<int> linkedListNode = _usedIndices.First;
		while (linkedListNode != null)
		{
			LinkedListNode<int>? next = linkedListNode.Next;
			ref Chunk reference = ref _terrainChunks[linkedListNode.Value];
			reference?.Destroy();
			reference = null;
			linkedListNode = next;
		}
		_usedIndices.Clear();
	}
}

using System;
using System.Buffers;
using System.Xml;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Map;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Gumps;

internal class MiniMapGump : Gump
{
	private bool _draw;

	private Texture2D _mapTexture;

	private int _lastMap = -1;

	private long _timeMS;

	private bool _useLargeMap;

	private ushort _x;

	private ushort _y;

	private static readonly uint[][] _blankGumpsPixels = new uint[4][];

	private const ushort SMALL_MAP_GRAPHIC = 5010;

	private const ushort BIG_MAP_GRAPHIC = 5011;

	public override GumpType GumpType => GumpType.MiniMap;

	public MiniMapGump()
		: base(0u, 0u)
	{
		CanMove = true;
		AcceptMouseInput = true;
		base.CanCloseWithRightClick = true;
	}

	public override void Save(XmlTextWriter writer)
	{
		base.Save(writer);
		writer.WriteAttributeString("isminimized", _useLargeMap.ToString());
	}

	public override void Restore(XmlElement xml)
	{
		base.Restore(xml);
		_useLargeMap = bool.Parse(xml.GetAttribute("isminimized"));
		CreateMap();
	}

	private void CreateMap()
	{
		Rectangle bounds;
		Texture2D gumpTexture = GumpsLoader.Instance.GetGumpTexture(_useLargeMap ? 5011u : 5010u, out bounds);
		int num = (_useLargeMap ? 1 : 0);
		if (_blankGumpsPixels[num] == null)
		{
			int num2 = bounds.Width * bounds.Height;
			uint[] array = ArrayPool<uint>.Shared.Rent(num2, zero: true);
			try
			{
				gumpTexture.GetData(0, bounds, array, 0, num2);
				_blankGumpsPixels[num] = new uint[num2];
				_blankGumpsPixels[num + 2] = new uint[num2];
				Array.Copy(array, 0, _blankGumpsPixels[num], 0, num2);
			}
			finally
			{
				ArrayPool<uint>.Shared.Return(array, clearArray: true);
			}
		}
		base.Width = bounds.Width;
		base.Height = bounds.Height;
		CreateMiniMapTexture(force: true);
	}

	public override void Update(double totalTime, double frameTime)
	{
		if (World.InGame)
		{
			if (_lastMap != World.MapIndex)
			{
				CreateMap();
				_lastMap = World.MapIndex;
			}
			if ((double)_timeMS < totalTime)
			{
				_draw = !_draw;
				_timeMS = (long)totalTime + 500;
			}
		}
	}

	public bool ToggleSize(bool? large = null)
	{
		if (large.HasValue)
		{
			_useLargeMap = large.Value;
		}
		else
		{
			_useLargeMap = !_useLargeMap;
		}
		if (_mapTexture != null && !_mapTexture.IsDisposed)
		{
			_mapTexture.Dispose();
		}
		CreateMap();
		return _useLargeMap;
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		if (base.IsDisposed)
		{
			return false;
		}
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(0);
		Rectangle bounds;
		Texture2D gumpTexture = GumpsLoader.Instance.GetGumpTexture(_useLargeMap ? 5011u : 5010u, out bounds);
		if (gumpTexture == null)
		{
			Dispose();
			return false;
		}
		batcher.Draw(gumpTexture, new Vector2(x, y), bounds, hueVector);
		CreateMiniMapTexture();
		batcher.Draw(_mapTexture, new Vector2(x, y), hueVector);
		if (_draw)
		{
			int num = base.Width >> 1;
			int num2 = base.Height >> 1;
			Texture2D texture = SolidColorTextureCache.GetTexture(Color.Red);
			foreach (Mobile value in World.Mobiles.Values)
			{
				if (!(value == World.Player))
				{
					int num3 = value.X - World.Player.X;
					int num4 = value.Y - World.Player.Y;
					int num5 = num3 - num4;
					int num6 = num3 + num4;
					hueVector = ShaderHueTranslator.GetHueVector(Notoriety.GetHue(value.NotorietyFlag));
					batcher.Draw(texture, new Rectangle(x + num + num5, y + num2 + num6, 2, 2), hueVector);
				}
			}
			hueVector = ShaderHueTranslator.GetHueVector(0);
			batcher.Draw(SolidColorTextureCache.GetTexture(Color.White), new Rectangle(x + num, y + num2, 2, 2), hueVector);
		}
		return base.Draw(batcher, x, y);
	}

	protected override bool OnMouseDoubleClick(int x, int y, MouseButtonType button)
	{
		if (button == MouseButtonType.Left)
		{
			ToggleSize(null);
			return true;
		}
		return false;
	}

	protected override void UpdateContents()
	{
		CreateMap();
	}

	private unsafe void CreateMiniMapTexture(bool force = false)
	{
		ushort x = World.Player.X;
		ushort y = World.Player.Y;
		if (_x != x || _y != y)
		{
			_x = x;
			_y = y;
		}
		else if (!force)
		{
			return;
		}
		int num = base.Width >> 2;
		int num2 = base.Height >> 2;
		int num3 = base.Width >> 1;
		int num4 = (x - num >> 3) - 1;
		int num5 = (y - num2 >> 3) - 1;
		int num6 = (x + num >> 3) + 1;
		int num7 = (y + num2 >> 3) + 1;
		if (num4 < 0)
		{
			num4 = 0;
		}
		if (num5 < 0)
		{
			num5 = 0;
		}
		int blocksCount = World.Map.BlocksCount;
		int num8 = MapLoader.Instance.MapBlocksSize[World.MapIndex, 1];
		int num9 = (_useLargeMap ? 1 : 0);
		_blankGumpsPixels[num9].CopyTo(_blankGumpsPixels[num9 + 2], 0);
		uint[] data = _blankGumpsPixels[num9 + 2];
		Point* ptr = stackalloc Point[2];
		ptr->X = 0;
		ptr->Y = 0;
		ptr[1].X = 0;
		ptr[1].Y = 1;
		for (int i = num4; i <= num6; i++)
		{
			int num10 = i * num8;
			for (int j = num5; j <= num7; j++)
			{
				int num11 = num10 + j;
				if (num11 >= blocksCount)
				{
					break;
				}
				ref IndexMap index = ref World.Map.GetIndex(i, j);
				if (index.MapAddress == 0L)
				{
					break;
				}
				MapBlock* ptr2 = (MapBlock*)index.MapAddress;
				MapCells* ptr3 = (MapCells*)(&ptr2->Cells);
				StaticsBlock* ptr4 = (StaticsBlock*)index.StaticAddress;
				uint staticCount = index.StaticCount;
				Chunk chunk = World.Map.GetChunk(num11);
				int num12 = i << 3;
				int num13 = j << 3;
				for (int k = 0; k < 8; k++)
				{
					int num14 = num12 + k - x + num3;
					for (int l = 0; l < 8; l++)
					{
						MapCells* num15 = ptr3 + ((l << 3) + k);
						int num16 = num15->TileID;
						bool flag = true;
						int z = num15->Z;
						for (int m = 0; m < staticCount; m++)
						{
							ref StaticsBlock reference = ref ptr4[m];
							if (reference.X == k && reference.Y == l && reference.Color > 0 && reference.Color != ushort.MaxValue && GameObject.CanBeDrawn(reference.Color) && reference.Z >= z)
							{
								num16 = ((reference.Hue > 0) ? ((ushort)(reference.Hue + 16384)) : reference.Color);
								flag = reference.Hue > 0;
								z = reference.Z;
							}
						}
						if (chunk != null)
						{
							GameObject gameObject = chunk.Tiles[k, l];
							while (gameObject?.TNext != null)
							{
								gameObject = gameObject.TNext;
							}
							while (gameObject != null)
							{
								if (gameObject is Multi)
								{
									if (gameObject.Hue == 0)
									{
										num16 = gameObject.Graphic;
										flag = false;
									}
									else
									{
										num16 = gameObject.Hue + 16384;
									}
									break;
								}
								gameObject = gameObject.TPrevious;
							}
						}
						if (!flag)
						{
							num16 += 16384;
						}
						int count = 2;
						num16 = ((!flag || num16 <= 16384) ? HuesLoader.Instance.GetRadarColorData(num16) : HuesLoader.Instance.GetColor16(16384, (ushort)(num16 - 16384)));
						int num17 = num13 + l - y;
						int x2 = num14 - num17;
						int y2 = num14 + num17;
						CreatePixels(data, 0x8000 | num16, x2, y2, base.Width, base.Height, ptr, count);
					}
				}
			}
		}
		if (_mapTexture == null || _mapTexture.IsDisposed)
		{
			_mapTexture = new Texture2D(Client.Game.GraphicsDevice, base.Width, base.Height, mipMap: false, SurfaceFormat.Color);
		}
		_mapTexture.SetData(data);
	}

	private unsafe void CreatePixels(uint[] data, int color, int x, int y, int w, int h, Point* table, int count)
	{
		int num = x;
		int num2 = y;
		for (int i = 0; i < count; i++)
		{
			num += table[i].X;
			num2 += table[i].Y;
			int num3 = num;
			if (num3 >= 0 && num3 < w)
			{
				int num4 = num2;
				if (num4 < 0 || num4 >= h)
				{
					break;
				}
				int num5 = num4 * w + num3;
				if (data[num5] == 4278716424u)
				{
					data[num5] = HuesHelper.Color16To32((ushort)color) | 0xFF000000u;
				}
			}
		}
	}

	public override bool Contains(int x, int y)
	{
		x -= base.Offset.X;
		y -= base.Offset.Y;
		if (x >= 0 && y >= 0 && x < base.Width && y < base.Height)
		{
			int num = (_useLargeMap ? 1 : 0) + 2;
			int num2 = y * base.Width + x;
			if (num2 < _blankGumpsPixels[num].Length)
			{
				return _blankGumpsPixels[num][num2] != 0;
			}
		}
		return false;
	}

	public override void Dispose()
	{
		_mapTexture?.Dispose();
		base.Dispose();
	}
}

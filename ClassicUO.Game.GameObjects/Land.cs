using System;
using System.Runtime.CompilerServices;
using ClassicUO.Configuration;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Map;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.GameObjects;

internal sealed class Land : GameObject
{
	private static readonly QueuedPool<Land> _pool = new QueuedPool<Land>(19200, delegate(Land l)
	{
		l.IsDestroyed = false;
		l.AlphaHue = byte.MaxValue;
		l.NormalTop = (l.NormalRight = (l.NormalLeft = (l.NormalBottom = Vector3.Zero)));
		l.YOffsets.Top = (l.YOffsets.Right = (l.YOffsets.Left = (l.YOffsets.Bottom = 0)));
		l.MinZ = (l.AverageZ = 0);
	});

	public sbyte AverageZ;

	public bool IsStretched;

	public sbyte MinZ;

	public Vector3 NormalTop;

	public Vector3 NormalRight;

	public Vector3 NormalLeft;

	public Vector3 NormalBottom;

	public ushort OriginalGraphic;

	public UltimaBatcher2D.YOffsets YOffsets;

	public ref LandTiles TileData => ref TileDataLoader.Instance.LandData[Graphic];

	public static Land Create(ushort graphic)
	{
		Land one = _pool.GetOne();
		one.Graphic = graphic;
		one.OriginalGraphic = graphic;
		one.IsStretched = one.TileData.TexID == 0 && one.TileData.IsWet;
		one.AllowedToDraw = graphic > 2;
		return one;
	}

	public override void Destroy()
	{
		if (!base.IsDestroyed)
		{
			base.Destroy();
			_pool.ReturnOne(this);
		}
	}

	public override void UpdateGraphicBySeason()
	{
		Graphic = CustomSeasonManager.RemapLandTiles(this, OriginalGraphic);
		AllowedToDraw = Graphic > 2;
	}

	public int CalculateCurrentAverageZ(int direction)
	{
		int directionZ = GetDirectionZ(((byte)(direction >> 1) + 1) & 3);
		if ((direction & 1) != 0)
		{
			return directionZ;
		}
		return directionZ + GetDirectionZ(direction >> 1) >> 1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private int GetDirectionZ(int direction)
	{
		return direction switch
		{
			1 => YOffsets.Right >> 2, 
			2 => YOffsets.Bottom >> 2, 
			3 => YOffsets.Left >> 2, 
			_ => Z, 
		};
	}

	public void ApplyStretch(ClassicUO.Game.Map.Map map, int x, int y, sbyte z)
	{
		if (IsStretched || TexmapsLoader.Instance.GetValidRefEntry(TileData.TexID).Length <= 0)
		{
			IsStretched = false;
			AverageZ = z;
			MinZ = z;
			return;
		}
		sbyte b = z;
		sbyte tileZ = map.GetTileZ(x + 1, y);
		sbyte tileZ2 = map.GetTileZ(x, y + 1);
		sbyte tileZ3 = map.GetTileZ(x + 1, y + 1);
		YOffsets.Top = b * 4;
		YOffsets.Right = tileZ * 4;
		YOffsets.Left = tileZ2 * 4;
		YOffsets.Bottom = tileZ3 * 4;
		if (Math.Abs(b - tileZ3) <= Math.Abs(tileZ2 - tileZ))
		{
			AverageZ = (sbyte)(b + tileZ3 >> 1);
		}
		else
		{
			AverageZ = (sbyte)(tileZ2 + tileZ >> 1);
		}
		MinZ = Math.Min(b, Math.Min(tileZ, Math.Min(tileZ2, tileZ3)));
		sbyte tileZ4 = map.GetTileZ(x, y - 1);
		sbyte tileZ5 = map.GetTileZ(x + 1, y - 1);
		sbyte tileZ6 = map.GetTileZ(x - 1, y);
		sbyte b2 = tileZ;
		sbyte tileZ7 = map.GetTileZ(x + 2, y);
		sbyte tileZ8 = map.GetTileZ(x - 1, y + 1);
		sbyte b3 = tileZ2;
		sbyte b4 = tileZ3;
		sbyte tileZ9 = map.GetTileZ(x + 2, y + 1);
		sbyte tileZ10 = map.GetTileZ(x, y + 2);
		sbyte tileZ11 = map.GetTileZ(x + 1, y + 2);
		IsStretched |= CalculateNormal(z, tileZ4, b2, b3, tileZ6, out NormalTop);
		IsStretched |= CalculateNormal(b2, tileZ5, tileZ7, b4, z, out NormalRight);
		IsStretched |= CalculateNormal(b4, b2, tileZ9, tileZ11, b3, out NormalBottom);
		IsStretched |= CalculateNormal(b3, z, b4, tileZ10, tileZ8, out NormalLeft);
	}

	private static bool CalculateNormal(sbyte tile, sbyte top, sbyte right, sbyte bottom, sbyte left, out Vector3 normal)
	{
		if (tile == top && tile == right && tile == bottom && tile == left)
		{
			normal.X = 0f;
			normal.Y = 0f;
			normal.Z = 1f;
			return false;
		}
		Vector3 vector = default(Vector3);
		Vector3 vector2 = default(Vector3);
		Vector3 result = default(Vector3);
		vector.X = -22f;
		vector.Y = -22f;
		vector.Z = (left - tile) * 4;
		vector2.X = -22f;
		vector2.Y = 22f;
		vector2.Z = (bottom - tile) * 4;
		Vector3.Cross(ref vector2, ref vector, out result);
		vector.X = -22f;
		vector.Y = 22f;
		vector.Z = (bottom - tile) * 4;
		vector2.X = 22f;
		vector2.Y = 22f;
		vector2.Z = (right - tile) * 4;
		Vector3.Cross(ref vector2, ref vector, out normal);
		Vector3.Add(ref result, ref normal, out result);
		vector.X = 22f;
		vector.Y = 22f;
		vector.Z = (right - tile) * 4;
		vector2.X = 22f;
		vector2.Y = -22f;
		vector2.Z = (top - tile) * 4;
		Vector3.Cross(ref vector2, ref vector, out normal);
		Vector3.Add(ref result, ref normal, out result);
		vector.X = 22f;
		vector.Y = -22f;
		vector.Z = (top - tile) * 4;
		vector2.X = -22f;
		vector2.Y = -22f;
		vector2.Z = (left - tile) * 4;
		Vector3.Cross(ref vector2, ref vector, out normal);
		Vector3.Add(ref result, ref normal, out result);
		Vector3.Normalize(ref result, out normal);
		return true;
	}

	public override bool Draw(UltimaBatcher2D batcher, int posX, int posY, float depth)
	{
		if (!AllowedToDraw || base.IsDestroyed)
		{
			return false;
		}
		ushort num = Hue;
		if (ProfileManager.CurrentProfile.HighlightGameObjects && SelectedObject.LastObject == this)
		{
			num = ProfileManager.CurrentProfile.HighlightGameObjectsColor;
		}
		else if (ProfileManager.CurrentProfile.NoColorObjectsOutOfRange && base.Distance > World.ClientViewRange)
		{
			num = 907;
		}
		else if (World.Player.IsDead && ProfileManager.CurrentProfile.EnableBlackWhiteEffect)
		{
			num = 910;
		}
		Vector3 hue = default(Vector3);
		if (num != 0)
		{
			hue.X = num - 1;
			hue.Y = ((!IsStretched) ? 1 : 6);
		}
		else
		{
			hue.X = 0f;
			hue.Y = (IsStretched ? 5 : 0);
		}
		hue.Z = 1f;
		if (IsStretched)
		{
			posY += Z << 2;
			GameObject.DrawLand(batcher, Graphic, posX, posY, ref YOffsets, ref NormalTop, ref NormalRight, ref NormalLeft, ref NormalBottom, hue, depth);
		}
		else
		{
			GameObject.DrawLand(batcher, Graphic, posX, posY, hue, depth);
		}
		return true;
	}

	public override bool CheckMouseSelection()
	{
		if (IsStretched)
		{
			return SelectedObject.IsPointInStretchedLand(ref YOffsets, RealScreenPosition.X, RealScreenPosition.Y + (Z << 2));
		}
		return SelectedObject.IsPointInLand(RealScreenPosition.X, RealScreenPosition.Y);
	}
}

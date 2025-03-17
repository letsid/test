using System;
using ClassicUO.Game.Managers;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.GameObjects;

internal sealed class MovingEffect : GameEffect
{
	private uint _lastMoveTime;

	private bool _packet70;

	public readonly bool FixedDir;

	public MovingEffect(EffectManager manager, uint src, uint trg, int xSource, int ySource, int zSource, int xTarget, int yTarget, int zTarget, ushort graphic, ushort hue, bool fixedDir, int duration, byte speed, bool packet70 = false)
		: base(manager, graphic, hue, duration, speed)
	{
		FixedDir = fixedDir;
		IntervalInMs = speed;
		_packet70 = packet70;
		Entity entity = World.Get(src);
		if (SerialHelper.IsValid(src) && entity != null)
		{
			SetSource(entity);
		}
		else
		{
			SetSource(xSource, ySource, zSource);
		}
		Entity entity2 = World.Get(trg);
		if (SerialHelper.IsValid(trg) && entity2 != null)
		{
			SetTarget(entity2);
		}
		else
		{
			SetTarget(xTarget, yTarget, zTarget);
		}
	}

	public override void Update(double totalTime, double frameTime)
	{
		base.Update(totalTime, frameTime);
		if (_lastMoveTime < Time.Ticks)
		{
			UpdateOffset();
			_lastMoveTime = Time.Ticks + IntervalInMs;
		}
	}

	private void UpdateOffset()
	{
		if (Target != null && Target.IsDestroyed)
		{
			Destroy();
			return;
		}
		int x = World.Player.X;
		int y = World.Player.Y;
		int z = World.Player.Z;
		(int x, int y, int z) source = GetSource();
		int item = source.x;
		int item2 = source.y;
		int item3 = source.z;
		int num = item - x;
		int num2 = item2 - y;
		int num3 = item3 - z;
		(int x, int y, int z) target = GetTarget();
		int item4 = target.x;
		int item5 = target.y;
		int item6 = target.z;
		int num4 = item4 - x;
		int num5 = item5 - y;
		int num6 = item6 - z;
		Vector2 value = new Vector2((num - num2) * 22, (num + num2) * 22 - num3 * 4);
		value.X += base.CumulativeOffset.X;
		value.Y += base.CumulativeOffset.Y;
		Vector2 value2 = new Vector2((num4 - num5) * 22, (num4 + num5) * 22 - num6 * 4);
		Vector2.Subtract(ref value2, ref value, out var result);
		Vector2.Distance(ref value, ref value2, out var result2);
		uint num7 = IntervalInMs;
		if (!_packet70 && num7 > 7)
		{
			num7 = 7u;
		}
		Vector2.Multiply(ref result, (float)num7 / result2, out var result3);
		if (Math.Sqrt(result3.X * result3.X + result3.Y * result3.Y) > Math.Sqrt(result.X * result.X + result.Y * result.Y))
		{
			result3 = result;
		}
		if (result2 <= 22f)
		{
			RemoveMe();
			return;
		}
		int ofsX = (int)(value.X / 22f);
		int ofsY = (int)(value.Y / 22f);
		TileOffsetOnMonitorToXY(ref ofsX, ref ofsY, out var x2, out var y2);
		int num8 = x + x2;
		int num9 = y + y2;
		if (num8 == item4 && num9 == item5 && item3 == item6)
		{
			RemoveMe();
			return;
		}
		base.IsPositionChanged = true;
		AngleToTarget = (float)Math.Atan2(0f - result.Y, 0f - result.X);
		if (num8 != item || num9 != item2)
		{
			SetSource(num8, num9, item3);
			Vector2 vector = new Vector2((x2 - y2) * 22, (x2 + y2) * 22 - num3 * 4);
			Offset.X = value.X - vector.X;
			Offset.Y = value.Y - vector.Y;
		}
		Offset.X += result3.X;
		Offset.Y += result3.Y;
	}

	private void RemoveMe()
	{
		CreateExplosionEffect();
		Destroy();
	}

	private static void TileOffsetOnMonitorToXY(ref int ofsX, ref int ofsY, out int x, out int y)
	{
		y = 0;
		if (ofsX == 0)
		{
			x = (y = ofsY >> 1);
			return;
		}
		if (ofsY == 0)
		{
			x = ofsX >> 1;
			y = -x;
			return;
		}
		int num = Math.Abs(ofsX);
		int num2 = Math.Abs(ofsY);
		x = ofsX;
		if (ofsY > ofsX)
		{
			if (ofsX < 0 && ofsY < 0)
			{
				y = num - num2;
			}
			else if (ofsX > 0 && ofsY > 0)
			{
				y = num2 - num;
			}
		}
		else if (ofsX > ofsY)
		{
			if (ofsX < 0 && ofsY < 0)
			{
				y = -(num2 - num);
			}
			else if (ofsX > 0 && ofsY > 0)
			{
				y = -(num - num2);
			}
		}
		if (y == 0 && ofsY != ofsX)
		{
			if (ofsY < 0)
			{
				y = -(num + num2);
			}
			else
			{
				y = num + num2;
			}
		}
		y /= 2;
		x += y;
	}
}

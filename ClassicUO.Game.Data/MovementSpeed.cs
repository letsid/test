using System;

namespace ClassicUO.Game.Data;

internal static class MovementSpeed
{
	public const int STEP_DELAY_MOUNT_RUN = 100;

	public const int STEP_DELAY_MOUNT_WALK = 200;

	public const int STEP_DELAY_RUN = 200;

	public const int STEP_DELAY_WALK = 400;

	public static int TimeToCompleteMovement(bool run, bool mounted)
	{
		if (mounted)
		{
			if (!run)
			{
				return 200;
			}
			return 100;
		}
		if (!run)
		{
			return 400;
		}
		return 200;
	}

	public static void GetPixelOffset(byte dir, ref float x, ref float y, float framesPerTile)
	{
		float num = 44f / framesPerTile;
		float num2 = 22f / framesPerTile;
		int num3 = 22;
		int num4 = 22;
		switch (dir & 7)
		{
		case 0:
			x *= num2;
			y *= 0f - num2;
			break;
		case 1:
			x *= num;
			num3 = 44;
			y = 0f;
			break;
		case 2:
			x *= num2;
			y *= num2;
			break;
		case 3:
			x = 0f;
			y *= num;
			num4 = 44;
			break;
		case 4:
			x *= 0f - num2;
			y *= num2;
			break;
		case 5:
			x *= 0f - num;
			num3 = 44;
			y = 0f;
			break;
		case 6:
			x *= 0f - num2;
			y *= 0f - num2;
			break;
		case 7:
			x = 0f;
			y *= 0f - num;
			num4 = 44;
			break;
		}
		int num5 = (int)x;
		if (Math.Abs(num5) > num3)
		{
			if (num5 < 0)
			{
				x = -num3;
			}
			else
			{
				x = num3;
			}
		}
		int num6 = (int)y;
		if (Math.Abs(num6) > num4)
		{
			if (num6 < 0)
			{
				y = -num4;
			}
			else
			{
				y = num4;
			}
		}
	}
}

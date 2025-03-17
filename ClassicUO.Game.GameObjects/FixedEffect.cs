using ClassicUO.Game.Managers;

namespace ClassicUO.Game.GameObjects;

internal sealed class FixedEffect : GameEffect
{
	public FixedEffect(EffectManager manager, ushort graphic, ushort hue, int duration, byte speed)
		: base(manager, graphic, hue, duration, speed)
	{
	}

	public FixedEffect(EffectManager manager, int sourceX, int sourceY, int sourceZ, ushort graphic, ushort hue, int duration, byte speed)
		: this(manager, graphic, hue, duration, speed)
	{
		SetSource(sourceX, sourceY, sourceZ);
	}

	public FixedEffect(EffectManager manager, uint sourceSerial, int sourceX, int sourceY, int sourceZ, ushort graphic, ushort hue, int duration, byte speed)
		: this(manager, graphic, hue, duration, speed)
	{
		Entity entity = World.Get(sourceSerial);
		if (entity != null && SerialHelper.IsValid(sourceSerial))
		{
			SetSource(entity);
		}
		else
		{
			SetSource(sourceX, sourceY, sourceZ);
		}
	}

	public override void Update(double totalTime, double frameTime)
	{
		base.Update(totalTime, frameTime);
		if (!base.IsDestroyed)
		{
			var (num, num2, num3) = GetSource();
			if (Source != null)
			{
				Offset = Source.Offset;
			}
			if (X != num || Y != num2 || Z != num3)
			{
				X = (ushort)num;
				Y = (ushort)num2;
				Z = (sbyte)num3;
				UpdateScreenPosition();
				AddToTile();
			}
		}
	}
}

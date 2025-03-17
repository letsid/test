using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Utility.Logging;

namespace ClassicUO.Game.Managers;

internal class EffectManager : LinkedObject
{
	public void Update(double totalTime, double frameTime)
	{
		GameEffect gameEffect = (GameEffect)Items;
		while (gameEffect != null)
		{
			GameEffect obj = (GameEffect)gameEffect.Next;
			gameEffect.Update(totalTime, frameTime);
			if (!gameEffect.IsDestroyed && gameEffect.Distance > World.ClientViewRange)
			{
				gameEffect.Destroy();
			}
			gameEffect = obj;
		}
	}

	public void CreateEffect(GraphicEffectType type, uint source, uint target, ushort graphic, ushort hue, ushort srcX, ushort srcY, sbyte srcZ, ushort targetX, ushort targetY, sbyte targetZ, byte speed, int duration, bool fixedDir, bool doesExplode, bool hasparticles, GraphicEffectBlendMode blendmode)
	{
		if (hasparticles)
		{
			Log.Warn("Unhandled particles in an effects packet.");
		}
		if (hue != 0)
		{
			hue++;
		}
		duration *= 50;
		GameEffect item;
		switch (type)
		{
		case GraphicEffectType.Moving:
		case GraphicEffectType.Moving70:
			duration *= 100;
			if (graphic <= 0)
			{
				return;
			}
			if (speed == 0)
			{
				speed++;
			}
			item = new MovingEffect(this, source, target, srcX, srcY, srcZ, targetX, targetY, targetZ, graphic, hue, fixedDir, duration, speed, type == GraphicEffectType.Moving70)
			{
				Blend = blendmode,
				CanCreateExplosionEffect = doesExplode
			};
			break;
		case GraphicEffectType.DragEffect:
			if (graphic <= 0)
			{
				return;
			}
			if (speed == 0)
			{
				speed++;
			}
			item = new DragEffect(this, source, target, srcX, srcY, srcZ, targetX, targetY, targetZ, graphic, hue, duration, speed)
			{
				Blend = blendmode,
				CanCreateExplosionEffect = doesExplode
			};
			break;
		case GraphicEffectType.Lightning:
			item = new LightningEffect(this, source, srcX, srcY, srcZ, hue);
			break;
		case GraphicEffectType.FixedXYZ:
			if (graphic <= 0)
			{
				return;
			}
			item = new FixedEffect(this, srcX, srcY, srcZ, graphic, hue, duration, 0)
			{
				Blend = blendmode
			};
			break;
		case GraphicEffectType.FixedFrom:
			if (graphic <= 0)
			{
				return;
			}
			item = new FixedEffect(this, source, srcX, srcY, srcZ, graphic, hue, duration, 0)
			{
				Blend = blendmode
			};
			break;
		case GraphicEffectType.ScreenFade:
			Log.Warn("Unhandled 'Screen Fade' effect.");
			return;
		default:
			Log.Warn("Unhandled effect.");
			return;
		}
		PushToBack(item);
	}

	public new void Clear()
	{
		GameEffect gameEffect = (GameEffect)Items;
		while (gameEffect != null)
		{
			LinkedObject next = gameEffect.Next;
			gameEffect.Destroy();
			gameEffect = (GameEffect)next;
		}
		Items = null;
	}
}

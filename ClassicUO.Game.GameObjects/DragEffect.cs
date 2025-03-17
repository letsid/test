using ClassicUO.Configuration;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.GameObjects;

internal class DragEffect : GameEffect
{
	private uint _lastMoveTime;

	public DragEffect(EffectManager manager, uint src, uint trg, int xSource, int ySource, int zSource, int xTarget, int yTarget, int zTarget, ushort graphic, ushort hue, int duration, byte speed)
		: base(manager, graphic, hue, duration, speed)
	{
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
		Hue = hue;
		Graphic = graphic;
	}

	public override void Update(double totalTime, double frameTime)
	{
		if (_lastMoveTime <= Time.Ticks)
		{
			Offset.X += 8f;
			Offset.Y += 8f;
			_lastMoveTime = Time.Ticks + 20;
			base.Update(totalTime, frameTime);
		}
	}

	public override bool Draw(UltimaBatcher2D batcher, int posX, int posY, float depth)
	{
		if (base.IsDestroyed)
		{
			return false;
		}
		ushort hue = (ushort)((ProfileManager.CurrentProfile.NoColorObjectsOutOfRange && base.Distance > World.ClientViewRange) ? 907 : ((!World.Player.IsDead || !ProfileManager.CurrentProfile.EnableBlackWhiteEffect) ? Hue : 910));
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(hue);
		GameObject.DrawStatic(batcher, AnimationGraphic, posX - ((int)base.CumulativeOffset.X + 22), posY - ((int)(0f - base.CumulativeOffset.Y) + 22), hueVector, depth);
		if (TileDataLoader.Instance.StaticData[Graphic].IsLight && Source != null)
		{
			Client.Game.GetScene<GameScene>().AddLight(Source, Source, posX + 22, posY + 22);
		}
		return true;
	}
}

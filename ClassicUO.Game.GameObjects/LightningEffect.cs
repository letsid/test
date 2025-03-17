using ClassicUO.Configuration;
using ClassicUO.Game.Managers;
using ClassicUO.IO;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.GameObjects;

internal sealed class LightningEffect : GameEffect
{
	public LightningEffect(EffectManager manager, uint src, int x, int y, int z, ushort hue)
		: base(manager, 20000, hue, 400, 0)
	{
		IsEnabled = true;
		AnimIndex = 0;
		Entity entity = World.Get(src);
		if (SerialHelper.IsValid(src) && entity != null)
		{
			SetSource(entity);
		}
		else
		{
			SetSource(x, y, z);
		}
	}

	public override void Update(double totalTime, double frameTime)
	{
		if (base.IsDestroyed)
		{
			return;
		}
		if (AnimIndex >= 10 || ((double)Duration < totalTime && Duration >= 0))
		{
			Destroy();
			return;
		}
		AnimationGraphic = (ushort)(Graphic + AnimIndex);
		if ((double)NextChangeFrameTime < totalTime)
		{
			AnimIndex++;
			NextChangeFrameTime = (long)totalTime + IntervalInMs;
		}
		var (num, num2, num3) = GetSource();
		if (X != num || Y != num2 || Z != num3)
		{
			X = (ushort)num;
			Y = (ushort)num2;
			Z = (sbyte)num3;
			UpdateScreenPosition();
			AddToTile();
		}
	}

	public override bool Draw(UltimaBatcher2D batcher, int posX, int posY, float depth)
	{
		ushort num = Hue;
		if (ProfileManager.CurrentProfile.NoColorObjectsOutOfRange && base.Distance > World.ClientViewRange)
		{
			num = 907;
		}
		else if (World.Player.IsDead && ProfileManager.CurrentProfile.EnableBlackWhiteEffect)
		{
			num = 910;
		}
		Vector3 hueVector;
		if (num == 0)
		{
			hueVector = ShaderHueTranslator.GetHueVector(num, partial: false, 1f);
			hueVector.Y = 0f;
		}
		else
		{
			hueVector = ShaderHueTranslator.GetHueVector(num - 1, partial: false, 1f);
			hueVector.Y = 10f;
		}
		ref UOFileIndex validRefEntry = ref GumpsLoader.Instance.GetValidRefEntry(AnimationGraphic);
		posX -= validRefEntry.Width >> 1;
		posY -= validRefEntry.Height;
		GameObject.DrawGump(batcher, AnimationGraphic, posX, posY, hueVector, depth);
		batcher.SetBlendState(null);
		return true;
	}
}

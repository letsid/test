using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.Managers;

internal class HealthLinesManager
{
	private const int BAR_WIDTH = 34;

	private const int BAR_HEIGHT = 8;

	private const int BAR_WIDTH_HALF = 17;

	private const int BAR_HEIGHT_HALF = 4;

	private const ushort BACKGROUND_GRAPHIC = 4200;

	private const ushort HP_GRAPHIC = 4201;

	public bool IsEnabled
	{
		get
		{
			if (ProfileManager.CurrentProfile != null)
			{
				return ProfileManager.CurrentProfile.ShowMobilesHP;
			}
			return false;
		}
	}

	public void Draw(UltimaBatcher2D batcher)
	{
		int x = ProfileManager.CurrentProfile.GameWindowSize.X;
		int y = ProfileManager.CurrentProfile.GameWindowSize.Y;
		if (SerialHelper.IsMobile(TargetManager.LastTargetInfo.Serial))
		{
			DrawHealthLineWithMath(batcher, TargetManager.LastTargetInfo.Serial, x, y);
			if (ProfileManager.CurrentProfile.ShowTargetFrames)
			{
				DrawTargetFrames(batcher, TargetManager.LastTargetInfo.Serial);
			}
		}
		if (SerialHelper.IsMobile(TargetManager.SelectedTarget))
		{
			DrawHealthLineWithMath(batcher, TargetManager.SelectedTarget, x, y);
		}
		if (SerialHelper.IsMobile(TargetManager.LastAttack))
		{
			DrawHealthLineWithMath(batcher, TargetManager.LastAttack, x, y);
		}
		if (!IsEnabled)
		{
			return;
		}
		int mobileHPType = ProfileManager.CurrentProfile.MobileHPType;
		if (mobileHPType < 0)
		{
			return;
		}
		int mobileHPShowWhen = ProfileManager.CurrentProfile.MobileHPShowWhen;
		foreach (Mobile value in World.Mobiles.Values)
		{
			if (value.IsDestroyed)
			{
				continue;
			}
			int hits = value.Hits;
			int hitsMax = value.HitsMax;
			if (hitsMax == 0 || (mobileHPShowWhen == 1 && hits == hitsMax))
			{
				continue;
			}
			Point realScreenPosition = value.RealScreenPosition;
			realScreenPosition.X += (int)value.CumulativeOffset.X + 22 + 5;
			realScreenPosition.Y += (int)(value.CumulativeOffset.Y - value.CumulativeOffset.Z) + 22 + 5;
			if (mobileHPType != 1 && !value.IsDead && !value.IsInvisibleAnimation() && ((mobileHPShowWhen == 2 && hits != hitsMax) || mobileHPShowWhen <= 1) && value.HitsPercentage != 0)
			{
				AnimationsLoader.Instance.GetAnimationDimensions(value.AnimIndex, value.GetGraphicForAnimation(), 0, 0, value.IsMounted, 0, out var _, out var centerY, out var _, out var height);
				Point point = realScreenPosition;
				point.Y -= height + centerY + 8 + 22;
				if (value.IsGargoyle && value.IsFlying)
				{
					point.Y -= 22;
				}
				else if (!value.IsMounted)
				{
					point.Y += 22;
				}
				point = Client.Game.Scene.Camera.WorldToScreen(point);
				point.X -= (value.HitsTexture.Width >> 1) + 5;
				point.Y -= value.HitsTexture.Height;
				if (value.ObjectHandlesStatus == ObjectHandlesStatus.DISPLAYING)
				{
					point.Y -= 23;
				}
				if (point.X >= 0 && point.X <= x - value.HitsTexture.Width && point.Y >= 0 && point.Y <= y)
				{
					value.HitsTexture.Draw(batcher, point.X, point.Y, 1f, 0);
				}
			}
			if (value.Serial != TargetManager.LastTargetInfo.Serial && value.Serial != TargetManager.SelectedTarget && value.Serial != TargetManager.LastAttack)
			{
				realScreenPosition.X -= 5;
				realScreenPosition = Client.Game.Scene.Camera.WorldToScreen(realScreenPosition);
				realScreenPosition.X -= 17;
				realScreenPosition.Y -= 4;
				if (realScreenPosition.X >= 0 && realScreenPosition.X <= x - 34 && realScreenPosition.Y >= 0 && realScreenPosition.Y <= y - 8 && mobileHPType >= 1)
				{
					DrawHealthLine(batcher, value, realScreenPosition.X, realScreenPosition.Y, value.Serial != World.Player.Serial);
				}
			}
		}
	}

	private void DrawTargetFrames(UltimaBatcher2D batcher, uint serial)
	{
		Entity entity = World.Get(serial);
		if (entity == null)
		{
			return;
		}
		Mobile mobile = entity as Mobile;
		if (!(mobile == null))
		{
			Point realScreenPosition = mobile.RealScreenPosition;
			realScreenPosition.X += (int)mobile.CumulativeOffset.X + 22 + 5;
			realScreenPosition.Y += (int)(mobile.CumulativeOffset.Y - mobile.CumulativeOffset.Z) + 22 + 5;
			realScreenPosition.X -= 5;
			Point point = new Point(realScreenPosition.X, realScreenPosition.Y);
			realScreenPosition = Client.Game.Scene.Camera.WorldToScreen(realScreenPosition);
			realScreenPosition.X -= 17;
			realScreenPosition.Y -= 4;
			int height = mobile.FrameInfo.Height;
			int width = mobile.FrameInfo.Width;
			uint g = mobile.NotorietyFlag switch
			{
				NotorietyFlag.PvPVictim => 30096u, 
				NotorietyFlag.Boss => 30071u, 
				NotorietyFlag.Staff => 30097u, 
				NotorietyFlag.Ally => 30065u, 
				NotorietyFlag.Enemy => 30071u, 
				NotorietyFlag.Murderer => 30071u, 
				NotorietyFlag.Gray => 30066u, 
				NotorietyFlag.Invulnerable => 30069u, 
				NotorietyFlag.Innocent => 30064u, 
				NotorietyFlag.Unknown => 30064u, 
				_ => 30064u, 
			};
			uint g2;
			uint g3;
			if (width >= 80)
			{
				g2 = 30061u;
				g3 = 30058u;
			}
			else if (width >= 40)
			{
				g2 = 30062u;
				g3 = 30059u;
			}
			else
			{
				g2 = 30063u;
				g3 = 30060u;
			}
			Rectangle bounds;
			Texture2D gumpTexture = GumpsLoader.Instance.GetGumpTexture(g2, out bounds);
			Rectangle bounds2;
			Texture2D gumpTexture2 = GumpsLoader.Instance.GetGumpTexture(g3, out bounds2);
			Rectangle bounds3;
			Texture2D gumpTexture3 = GumpsLoader.Instance.GetGumpTexture(g, out bounds3);
			Vector3 hueVector = ShaderHueTranslator.GetHueVector(0, partial: false, 1f);
			batcher.Draw(gumpTexture2, new Rectangle(realScreenPosition.X - 28, realScreenPosition.Y - bounds2.Height / 2 - 4, bounds2.Width, bounds2.Height), bounds2, hueVector);
			point.Y -= height + 30;
			point = Client.Game.Scene.Camera.WorldToScreen(point);
			point.X -= 17;
			point.Y -= 4;
			batcher.Draw(gumpTexture, new Rectangle(point.X - 28, point.Y, bounds.Width, bounds.Height), bounds, hueVector);
			batcher.Draw(gumpTexture3, new Rectangle(point.X - 28, point.Y, bounds.Width, bounds.Height), bounds3, hueVector);
		}
	}

	private void DrawHealthLineWithMath(UltimaBatcher2D batcher, uint serial, int screenW, int screenH)
	{
		Entity entity = World.Get(serial);
		if (!(entity == null))
		{
			Point realScreenPosition = entity.RealScreenPosition;
			realScreenPosition.X += (int)entity.CumulativeOffset.X + 22;
			realScreenPosition.Y += (int)(entity.CumulativeOffset.Y - entity.CumulativeOffset.Z) + 22 + 5;
			realScreenPosition = Client.Game.Scene.Camera.WorldToScreen(realScreenPosition);
			realScreenPosition.X -= 17;
			realScreenPosition.Y -= 4;
			if (realScreenPosition.X >= 0 && realScreenPosition.X <= screenW - 34 && realScreenPosition.Y >= 0 && realScreenPosition.Y <= screenH - 8)
			{
				DrawHealthLine(batcher, entity, realScreenPosition.X, realScreenPosition.Y, passive: false);
			}
		}
	}

	private void DrawHealthLine(UltimaBatcher2D batcher, Entity entity, int x, int y, bool passive)
	{
		if (entity == null)
		{
			return;
		}
		int num = 34 * entity.HitsPercentage / 100;
		Mobile mobile = entity as Mobile;
		float alpha = (passive ? 0.5f : 1f);
		ushort hue = ((mobile != null) ? Notoriety.GetHue(mobile.NotorietyFlag) : Notoriety.GetHue(NotorietyFlag.Gray));
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(hue, partial: false, alpha);
		if (mobile == null)
		{
			y += 22;
		}
		Texture2D gumpTexture = GumpsLoader.Instance.GetGumpTexture(4200u, out var bounds);
		batcher.Draw(gumpTexture, new Rectangle(x, y, bounds.Width, bounds.Height), bounds, hueVector);
		hueVector.X = 33f;
		if (entity.Hits != entity.HitsMax || entity.HitsMax == 0)
		{
			int num2 = 2;
			if (num >> 2 == 0)
			{
				num2 = num;
			}
			gumpTexture = GumpsLoader.Instance.GetGumpTexture(4201u, out bounds);
			batcher.DrawTiled(gumpTexture, new Rectangle(x + num - num2, y, 34 - num - num2 / 2, bounds.Height), bounds, hueVector);
		}
		hue = 90;
		if (num <= 0)
		{
			return;
		}
		if (mobile != null)
		{
			if (mobile.IsPoisoned)
			{
				hue = 63;
			}
			else if (mobile.IsYellowHits)
			{
				hue = 53;
			}
		}
		hueVector.X = (int)hue;
		gumpTexture = GumpsLoader.Instance.GetGumpTexture(4201u, out bounds);
		batcher.DrawTiled(gumpTexture, new Rectangle(x, y, num, bounds.Height), bounds, hueVector);
	}
}

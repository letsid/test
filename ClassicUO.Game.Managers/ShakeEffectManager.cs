using System;
using System.Collections.Generic;
using ClassicUO.Configuration;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Scenes;
using ClassicUO.IO;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.Managers;

internal class ShakeEffectManager
{
	public class ShakeEffect
	{
		public byte Flags { get; set; }

		public int X { get; set; }

		public int Y { get; set; }

		public int MaxDistance { get; set; }

		public int Strength { get; set; }

		public uint Expiration { get; set; }

		public uint SourceUID { get; set; }
	}

	public enum ShakeEffectFlags
	{
		ShakeEffectFlag_Vertical = 1,
		ShakeEffectFlag_Horizontal = 2,
		ShakeEffectFlag_DecreaseWithTime = 4,
		ShakeEffectFlag_DecreaseWithDistance = 8,
		ShakeEffectFlag_SourceIsCoordinate = 0x80
	}

	private static uint NextShake = 0u;

	private static Matrix currentTranslationMatrix;

	private static List<ShakeEffect> activeShakeEffects = new List<ShakeEffect>();

	public static bool HasShakes => activeShakeEffects.Count > 0;

	public static void ApplyShakeEffect(StackDataReader p)
	{
		if (ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.ActivateShakeEffects)
		{
			byte b = p.ReadUInt8();
			if ((b & 0x80) != 0)
			{
				ShakeEffect item = new ShakeEffect
				{
					X = p.ReadUInt16BE(),
					Y = p.ReadUInt16BE(),
					MaxDistance = p.ReadUInt16BE(),
					Strength = p.ReadUInt16BE(),
					Expiration = Time.Ticks + p.ReadUInt32BE(),
					Flags = b
				};
				activeShakeEffects.Add(item);
			}
			else
			{
				ShakeEffect item2 = new ShakeEffect
				{
					SourceUID = p.ReadUInt32BE(),
					MaxDistance = p.ReadUInt16BE(),
					Strength = p.ReadUInt16BE(),
					Expiration = Time.Ticks + p.ReadUInt32BE(),
					Flags = b
				};
				activeShakeEffects.Add(item2);
			}
		}
	}

	public static void ApplyShakeEffect(ref Matrix matrix)
	{
		if (Time.Ticks < NextShake)
		{
			matrix *= currentTranslationMatrix;
		}
		else
		{
			if (!(Client.Game.Scene is GameScene))
			{
				return;
			}
			activeShakeEffects.RemoveAll((ShakeEffect a) => a.Expiration < Time.Ticks);
			int num = 0;
			int num2 = 0;
			foreach (ShakeEffect activeShakeEffect in activeShakeEffects)
			{
				int num3 = activeShakeEffect.Strength;
				int num4 = activeShakeEffect.Strength;
				if ((activeShakeEffect.Flags & 8) != 0)
				{
					PlayerMobile player = World.Player;
					if (player != null)
					{
						int num5 = 0;
						if ((activeShakeEffect.Flags & 0x80) == 0)
						{
							GameObject gameObject = World.Get(activeShakeEffect.SourceUID);
							if (gameObject != null)
							{
								num5 = Math.Max(Math.Abs(player.X - gameObject.X), Math.Abs(player.Y - gameObject.Y));
							}
						}
						else
						{
							num5 = Math.Max(Math.Abs(player.X - activeShakeEffect.X), Math.Abs(player.Y - activeShakeEffect.Y));
						}
						int num6 = Math.Max(activeShakeEffect.MaxDistance - num5, 0);
						num4 = num4 * num6 / activeShakeEffect.MaxDistance;
						num3 = num3 * num6 / activeShakeEffect.MaxDistance;
					}
				}
				if ((activeShakeEffect.Flags & 2) == 0)
				{
					num4 = 0;
				}
				if ((activeShakeEffect.Flags & 1) == 0)
				{
					num3 = 0;
				}
				if (num4 > num)
				{
					num = num4;
				}
				if (num3 > num2)
				{
					num2 = num3;
				}
			}
			Random random = new Random();
			Matrix.CreateTranslation((float)(random.NextDouble() * 2.0 * (double)num - (double)num), (float)(random.NextDouble() * 2.0 * (double)num2 - (double)num2), 0f, out currentTranslationMatrix);
			matrix *= currentTranslationMatrix;
			NextShake = Time.Ticks + 10;
		}
	}
}

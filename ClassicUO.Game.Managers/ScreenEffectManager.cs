using System;
using System.Collections.Generic;
using System.Linq;
using ClassicUO.Configuration;
using ClassicUO.Renderer;
using ClassicUO.Utility.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.Managers;

internal class ScreenEffectManager
{
	internal class ScreenEffect
	{
		public uint Expiration { get; set; }

		public uint StartTime { get; set; }

		public bool Expired => Time.Ticks > Expiration;

		public Texture2D Texture { get; set; }

		public virtual void CleanUp()
		{
			Texture?.Dispose();
		}

		public virtual void Process(UltimaBatcher2D batcher)
		{
		}
	}

	internal class FadeScreenEffect : ScreenEffect
	{
		public override void Process(UltimaBatcher2D batcher)
		{
			uint num = base.Expiration - base.StartTime;
			float alpha = Math.Min(((float)Time.Ticks - (float)base.StartTime) / (float)num, 1f);
			if (ProfileManager.CurrentProfile == null)
			{
				return;
			}
			int num2 = ProfileManager.CurrentProfile.GameWindowSize.Y + 6;
			int num3 = ProfileManager.CurrentProfile.GameWindowSize.X + 6;
			if (base.Texture == null || base.Texture.IsDisposed || base.Texture.Width != num3 || base.Texture.Height != num2)
			{
				base.Texture = new Texture2D(Client.Game.GraphicsDevice, num3, num2);
				uint[] array = new uint[num3 * num2];
				for (int i = 0; i < num3; i++)
				{
					for (int j = 0; j < num2; j++)
					{
						array[i * num2 + j] = uint.MaxValue;
					}
				}
				base.Texture.SetData(array);
			}
			int x = ProfileManager.CurrentProfile.GameWindowPosition.X;
			int y = ProfileManager.CurrentProfile.GameWindowPosition.Y;
			Vector3 hueVector = ShaderHueTranslator.GetHueVector(0, partial: false, alpha);
			batcher.Draw(base.Texture, new Vector2(x, y), hueVector);
		}
	}

	internal class BlackScreenEffect : ScreenEffect
	{
		public override void Process(UltimaBatcher2D batcher)
		{
			uint num = base.Expiration - base.StartTime;
			float alpha = Math.Min(((float)Time.Ticks - (float)base.StartTime) / (float)num, 1f);
			if (ProfileManager.CurrentProfile == null)
			{
				return;
			}
			int num2 = ProfileManager.CurrentProfile.GameWindowSize.Y + 6;
			int num3 = ProfileManager.CurrentProfile.GameWindowSize.X + 6;
			if (base.Texture == null || base.Texture.IsDisposed || base.Texture.Width != num3 || base.Texture.Height != num2)
			{
				base.Texture = new Texture2D(Client.Game.GraphicsDevice, num3, num2);
				uint[] array = new uint[num3 * num2];
				for (int i = 0; i < num3; i++)
				{
					for (int j = 0; j < num2; j++)
					{
						array[i * num2 + j] = 4278190080u;
					}
				}
				base.Texture.SetData(array);
			}
			int x = ProfileManager.CurrentProfile.GameWindowPosition.X;
			int y = ProfileManager.CurrentProfile.GameWindowPosition.Y;
			Vector3 hueVector = ShaderHueTranslator.GetHueVector(0, partial: false, alpha);
			batcher.Draw(base.Texture, new Vector2(x, y), hueVector);
		}
	}

	internal class FadeAndBlackEffect : ScreenEffect
	{
		private Texture2D Texture2 { get; set; }

		public override void CleanUp()
		{
			base.CleanUp();
			Texture2?.Dispose();
		}

		public override void Process(UltimaBatcher2D batcher)
		{
			uint num = base.Expiration - 2000 - base.StartTime;
			float alpha = Math.Min(((float)Time.Ticks - (float)base.StartTime) / (float)num, 1f);
			if (ProfileManager.CurrentProfile != null)
			{
				int num2 = ProfileManager.CurrentProfile.GameWindowSize.Y + 6;
				int num3 = ProfileManager.CurrentProfile.GameWindowSize.X + 6;
				if (base.Texture == null || base.Texture.IsDisposed || base.Texture.Width != num3 || base.Texture.Height != num2)
				{
					base.Texture = new Texture2D(Client.Game.GraphicsDevice, num3, num2);
					uint[] array = new uint[num3 * num2];
					for (int i = 0; i < num3; i++)
					{
						for (int j = 0; j < num2; j++)
						{
							array[i * num2 + j] = uint.MaxValue;
						}
					}
					base.Texture.SetData(array);
				}
				int x = ProfileManager.CurrentProfile.GameWindowPosition.X;
				int y = ProfileManager.CurrentProfile.GameWindowPosition.Y;
				Vector3 hueVector = ShaderHueTranslator.GetHueVector(0, partial: false, alpha);
				batcher.Draw(base.Texture, new Vector2(x, y), hueVector);
			}
			if (base.StartTime + 2000 > Time.Ticks)
			{
				return;
			}
			uint num4 = base.StartTime + 2000;
			uint num5 = base.Expiration - num4;
			float alpha2 = Math.Min(((float)Time.Ticks - (float)num4) / (float)num5, 1f);
			if (ProfileManager.CurrentProfile == null)
			{
				return;
			}
			int num6 = ProfileManager.CurrentProfile.GameWindowSize.Y + 6;
			int num7 = ProfileManager.CurrentProfile.GameWindowSize.X + 6;
			if (Texture2 == null || Texture2.IsDisposed || Texture2.Width != num7 || Texture2.Height != num6)
			{
				Texture2 = new Texture2D(Client.Game.GraphicsDevice, num7, num6);
				uint[] array2 = new uint[num7 * num6];
				for (int k = 0; k < num7; k++)
				{
					for (int l = 0; l < num6; l++)
					{
						array2[k * num6 + l] = 4278190080u;
					}
				}
				Texture2.SetData(array2);
			}
			int x2 = ProfileManager.CurrentProfile.GameWindowPosition.X;
			int y2 = ProfileManager.CurrentProfile.GameWindowPosition.Y;
			Vector3 hueVector2 = ShaderHueTranslator.GetHueVector(0, partial: false, alpha2);
			batcher.Draw(Texture2, new Vector2(x2, y2), hueVector2);
		}
	}

	internal class FadeScreenShotEffect : ScreenEffect
	{
		private Texture2D GameScreenTexture { get; set; }

		public FadeScreenShotEffect()
		{
			try
			{
				int x = ProfileManager.CurrentProfile.GameWindowPosition.X;
				int y = ProfileManager.CurrentProfile.GameWindowPosition.Y;
				int num = Math.Abs(Math.Min(x, 0));
				int num2 = Math.Abs(Math.Min(y, 0));
				int num3 = x + num;
				int num4 = y + num2;
				int num5 = ProfileManager.CurrentProfile.GameWindowSize.Y + 6 - num;
				int num6 = ProfileManager.CurrentProfile.GameWindowSize.X + 6 - num2;
				if (Client.Game.GraphicManager.PreferredBackBufferWidth < num6)
				{
					num6 = Client.Game.GraphicManager.PreferredBackBufferWidth;
				}
				if (Client.Game.GraphicManager.PreferredBackBufferHeight < num5)
				{
					num5 = Client.Game.GraphicManager.PreferredBackBufferHeight;
				}
				if (GameScreenTexture != null)
				{
					return;
				}
				Color[] array = new Color[Client.Game.GraphicManager.PreferredBackBufferWidth * Client.Game.GraphicManager.PreferredBackBufferHeight];
				Client.Game.GraphicsDevice.GetBackBufferData(array);
				GameScreenTexture = new Texture2D(Client.Game.GraphicsDevice, num6, num5);
				uint[] array2 = new uint[num6 * num5];
				for (int i = 0; i < num5; i++)
				{
					for (int j = 0; j < num6; j++)
					{
						array2[i * num6 + j] = array[(num4 + i) * Client.Game.GraphicManager.PreferredBackBufferWidth + (num3 + j)].PackedValue;
					}
				}
				GameScreenTexture.SetData(array2);
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
			}
		}

		public override void CleanUp()
		{
			base.CleanUp();
			GameScreenTexture?.Dispose();
		}

		public override void Process(UltimaBatcher2D batcher)
		{
			int num = ProfileManager.CurrentProfile.GameWindowSize.Y + 6;
			int num2 = ProfileManager.CurrentProfile.GameWindowSize.X + 6;
			int x = ProfileManager.CurrentProfile.GameWindowPosition.X;
			int y = ProfileManager.CurrentProfile.GameWindowPosition.Y;
			int num3 = Math.Abs(Math.Min(x, 0));
			int num4 = Math.Abs(Math.Min(y, 0));
			uint num5 = base.Expiration - base.StartTime;
			float alpha = Math.Min(((float)Time.Ticks - (float)base.StartTime) / (float)num5, 1f);
			if (ProfileManager.CurrentProfile == null)
			{
				return;
			}
			if (base.Texture == null || base.Texture.IsDisposed || base.Texture.Width != num2 || base.Texture.Height != num)
			{
				base.Texture = new Texture2D(Client.Game.GraphicsDevice, num2, num);
				uint[] array = new uint[num2 * num];
				for (int i = 0; i < num2; i++)
				{
					for (int j = 0; j < num; j++)
					{
						array[i * num + j] = uint.MaxValue;
					}
				}
				base.Texture.SetData(array);
			}
			Vector3 hueVector = ShaderHueTranslator.GetHueVector(0, partial: false, alpha);
			Vector3 hueVector2 = ShaderHueTranslator.GetHueVector(0, partial: false, 1f);
			batcher.Draw(GameScreenTexture, new Vector2(x + num3, y + num4), hueVector2);
			batcher.Draw(base.Texture, new Vector2(x, y), hueVector);
		}
	}

	internal class BlackScreenShotEffect : ScreenEffect
	{
		private Texture2D GameScreenTexture { get; set; }

		public BlackScreenShotEffect()
		{
			try
			{
				int x = ProfileManager.CurrentProfile.GameWindowPosition.X;
				int y = ProfileManager.CurrentProfile.GameWindowPosition.Y;
				int num = Math.Abs(Math.Min(x, 0));
				int num2 = Math.Abs(Math.Min(y, 0));
				int num3 = x + num;
				int num4 = y + num2;
				int num5 = ProfileManager.CurrentProfile.GameWindowSize.Y + 6 - num;
				int num6 = ProfileManager.CurrentProfile.GameWindowSize.X + 6 - num2;
				if (Client.Game.GraphicManager.PreferredBackBufferWidth < num6)
				{
					num6 = Client.Game.GraphicManager.PreferredBackBufferWidth;
				}
				if (Client.Game.GraphicManager.PreferredBackBufferHeight < num5)
				{
					num5 = Client.Game.GraphicManager.PreferredBackBufferHeight;
				}
				if (GameScreenTexture != null)
				{
					return;
				}
				Color[] array = new Color[Client.Game.GraphicManager.PreferredBackBufferWidth * Client.Game.GraphicManager.PreferredBackBufferHeight];
				Client.Game.GraphicsDevice.GetBackBufferData(array);
				GameScreenTexture = new Texture2D(Client.Game.GraphicsDevice, num6, num5);
				uint[] array2 = new uint[num6 * num5];
				for (int i = 0; i < num5; i++)
				{
					for (int j = 0; j < num6; j++)
					{
						array2[i * num6 + j] = array[(num4 + i) * Client.Game.GraphicManager.PreferredBackBufferWidth + (num3 + j)].PackedValue;
					}
				}
				GameScreenTexture.SetData(array2);
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
			}
		}

		public override void CleanUp()
		{
			base.CleanUp();
			GameScreenTexture?.Dispose();
		}

		public override void Process(UltimaBatcher2D batcher)
		{
			int num = ProfileManager.CurrentProfile.GameWindowSize.Y + 6;
			int num2 = ProfileManager.CurrentProfile.GameWindowSize.X + 6;
			int x = ProfileManager.CurrentProfile.GameWindowPosition.X;
			int y = ProfileManager.CurrentProfile.GameWindowPosition.Y;
			int num3 = Math.Abs(Math.Min(x, 0));
			int num4 = Math.Abs(Math.Min(y, 0));
			uint num5 = base.Expiration - base.StartTime;
			float alpha = Math.Min(((float)Time.Ticks - (float)base.StartTime) / (float)num5, 1f);
			if (ProfileManager.CurrentProfile == null)
			{
				return;
			}
			if (base.Texture == null || base.Texture.IsDisposed || base.Texture.Width != num2 || base.Texture.Height != num)
			{
				base.Texture = new Texture2D(Client.Game.GraphicsDevice, num2, num);
				uint[] array = new uint[num2 * num];
				for (int i = 0; i < num2; i++)
				{
					for (int j = 0; j < num; j++)
					{
						array[i * num + j] = 4278190080u;
					}
				}
				base.Texture.SetData(array);
			}
			Vector3 hueVector = ShaderHueTranslator.GetHueVector(0, partial: false, alpha);
			Vector3 hueVector2 = ShaderHueTranslator.GetHueVector(0, partial: false, 1f);
			batcher.Draw(GameScreenTexture, new Vector2(x + num3, y + num4), hueVector2);
			batcher.Draw(base.Texture, new Vector2(x, y), hueVector);
		}
	}

	public static List<ScreenEffect> ActiveScreenEffects = new List<ScreenEffect>();

	public static void Process(UltimaBatcher2D batcher)
	{
		if (!ActiveScreenEffects.Any())
		{
			return;
		}
		batcher.Begin();
		ActiveScreenEffects.RemoveAll(delegate(ScreenEffect x)
		{
			if (x.Expired)
			{
				x.CleanUp();
				return true;
			}
			return false;
		});
		foreach (ScreenEffect activeScreenEffect in ActiveScreenEffects)
		{
			activeScreenEffect.Process(batcher);
		}
		batcher.End();
	}
}

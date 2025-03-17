using System;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game;

internal class Weather
{
	private struct WeatherEffect
	{
		public float SpeedX;

		public float SpeedY;

		public float X;

		public float Y;

		public float ScaleRatio;

		public float SpeedAngle;

		public float SpeedMagnitude;

		public uint ID;
	}

	private const int MAX_WEATHER_EFFECT = 150;

	private const float SIMULATION_TIME = 37f;

	private readonly WeatherEffect[] _effects = new WeatherEffect[150];

	private uint _timer;

	private uint _windTimer;

	private uint _lastTick;

	public WEATHER_TYPE? CurrentWeather { get; private set; }

	public WEATHER_TYPE Type { get; private set; }

	public byte Count { get; private set; }

	public byte CurrentCount { get; private set; }

	public byte Temperature { get; private set; }

	public sbyte Wind { get; private set; }

	private static float SinOscillate(float freq, int range, uint current_tick)
	{
		return Math.Sign(Microsoft.Xna.Framework.MathHelper.ToRadians((int)((float)current_tick / 2.7777f * freq) % 360)) * range;
	}

	public void Reset()
	{
		Type = WEATHER_TYPE.WT_RAIN;
		byte b2 = (Temperature = 0);
		byte count = (CurrentCount = b2);
		Count = count;
		Wind = 0;
		_windTimer = (_timer = 0u);
		CurrentWeather = null;
	}

	public void Generate(WEATHER_TYPE type, byte count, byte temp)
	{
		if (CurrentWeather.HasValue && CurrentWeather == type)
		{
			return;
		}
		Reset();
		Type = type;
		Count = (byte)Math.Min(150, (int)count);
		Temperature = temp;
		_timer = Time.Ticks + 360000;
		_lastTick = Time.Ticks;
		if (Type == WEATHER_TYPE.WT_INVALID_0 || Type == WEATHER_TYPE.WT_INVALID_1)
		{
			_timer = 0u;
			CurrentWeather = null;
			return;
		}
		bool flag = Count > 0;
		switch (type)
		{
		case WEATHER_TYPE.WT_RAIN:
			if (flag)
			{
				GameActions.Print(ResGeneral.ItBeginsToRain, 1154, MessageType.System, 3, unicode: false);
				CurrentWeather = type;
			}
			break;
		case WEATHER_TYPE.WT_FIERCE_STORM:
			if (flag)
			{
				GameActions.Print(ResGeneral.AFierceStormApproaches, 1154, MessageType.System, 3, unicode: false);
				Count = Math.Max((byte)150, (byte)(Count + 80));
				CurrentWeather = type;
			}
			break;
		case WEATHER_TYPE.WT_SNOW:
			if (flag)
			{
				GameActions.Print(ResGeneral.ItBeginsToSnow, 1154, MessageType.System, 3, unicode: false);
				CurrentWeather = type;
			}
			break;
		case WEATHER_TYPE.WT_STORM:
			if (flag)
			{
				GameActions.Print(ResGeneral.AStormIsBrewing, 1154, MessageType.System, 3, unicode: false);
				CurrentWeather = type;
			}
			break;
		}
		_windTimer = 0u;
		while (CurrentCount < Count)
		{
			ref WeatherEffect reference = ref _effects[CurrentCount++];
			reference.X = RandomHelper.GetValue(0, ProfileManager.CurrentProfile.GameWindowSize.X);
			reference.Y = RandomHelper.GetValue(0, ProfileManager.CurrentProfile.GameWindowSize.Y);
		}
	}

	public void Draw(UltimaBatcher2D batcher, int x, int y)
	{
		bool flag = false;
		if (_timer < Time.Ticks)
		{
			if (CurrentCount == 0)
			{
				return;
			}
			flag = true;
		}
		else if (Type == WEATHER_TYPE.WT_INVALID_0 || Type == WEATHER_TYPE.WT_INVALID_1)
		{
			return;
		}
		uint num = Time.Ticks - _lastTick;
		if (num > 7000)
		{
			_lastTick = Time.Ticks;
			num = 25u;
		}
		bool flag2 = false;
		if (_windTimer < Time.Ticks)
		{
			if (_windTimer == 0)
			{
				flag2 = true;
			}
			_windTimer = Time.Ticks + (uint)(RandomHelper.GetValue(7, 13) * 1000);
			sbyte wind = Wind;
			Wind = (sbyte)RandomHelper.GetValue(0, 4);
			if (RandomHelper.GetValue(0, 2) != 0)
			{
				Wind *= -1;
			}
			if (Wind < 0 && wind > 0)
			{
				Wind = 0;
			}
			else if (Wind > 0 && wind < 0)
			{
				Wind = 0;
			}
			if (wind != Wind)
			{
				flag2 = true;
			}
		}
		Point gameWindowSize = ProfileManager.CurrentProfile.GameWindowSize;
		Rectangle destinationRectangle = new Rectangle(0, 0, 2, 2);
		for (int i = 0; i < CurrentCount; i++)
		{
			ref WeatherEffect reference = ref _effects[i];
			if (reference.X < (float)x || reference.X > (float)(x + gameWindowSize.X) || reference.Y < (float)y || reference.Y > (float)(y + gameWindowSize.Y))
			{
				if (flag)
				{
					if (CurrentCount > 0)
					{
						CurrentCount--;
					}
					else
					{
						CurrentCount = 0;
					}
					continue;
				}
				reference.X = x + RandomHelper.GetValue(0, gameWindowSize.X);
				reference.Y = y + RandomHelper.GetValue(0, gameWindowSize.Y);
			}
			switch (Type)
			{
			case WEATHER_TYPE.WT_RAIN:
			{
				float scaleRatio = reference.ScaleRatio;
				reference.SpeedX = -5f - scaleRatio;
				reference.SpeedY = 20f + scaleRatio;
				break;
			}
			case WEATHER_TYPE.WT_FIERCE_STORM:
				reference.SpeedX = Wind;
				reference.SpeedY = 30f;
				break;
			case WEATHER_TYPE.WT_SNOW:
			case WEATHER_TYPE.WT_STORM:
			{
				if (Type == WEATHER_TYPE.WT_SNOW)
				{
					reference.SpeedX = Wind;
					reference.SpeedY = 1f;
				}
				else
				{
					reference.SpeedX = (float)Wind * 5f;
					reference.SpeedY = 5f;
				}
				if (flag2)
				{
					reference.SpeedAngle = Microsoft.Xna.Framework.MathHelper.ToDegrees((float)Math.Atan2(reference.SpeedX, reference.SpeedY));
					reference.SpeedMagnitude = (float)Math.Sqrt(Math.Pow(reference.SpeedX, 2.0) + Math.Pow(reference.SpeedY, 2.0));
				}
				float speedAngle = reference.SpeedAngle;
				float speedMagnitude = reference.SpeedMagnitude;
				speedMagnitude += reference.ScaleRatio;
				float num2 = Microsoft.Xna.Framework.MathHelper.ToRadians(speedAngle + SinOscillate(0.4f, 20, Time.Ticks + reference.ID));
				reference.SpeedX = speedMagnitude * (float)Math.Sin(num2);
				reference.SpeedY = speedMagnitude * (float)Math.Cos(num2);
				break;
			}
			}
			float num3 = (float)num / 37f;
			switch (Type)
			{
			case WEATHER_TYPE.WT_RAIN:
			case WEATHER_TYPE.WT_FIERCE_STORM:
			{
				_ = reference;
				int num4 = (int)reference.Y;
				float num5 = reference.SpeedX * num3;
				float num6 = reference.SpeedY * num3;
				reference.X += num5;
				reference.Y += num6;
				if (num5 >= 10f)
				{
					_ = reference;
				}
				else if (num5 <= -10f)
				{
					_ = reference;
				}
				if (num6 >= 10f)
				{
					num4 = (int)(reference.Y - 10f);
				}
				else if ((float)num4 <= -10f)
				{
					num4 = (int)(reference.Y + 10f);
				}
				batcher.DrawLine(start: new Vector2((float)x + reference.X, (float)y + reference.Y - 5f), end: new Vector2((float)x + reference.X, (float)y + reference.Y), texture: SolidColorTextureCache.GetTexture(Color.LightBlue), color: new Vector3(0f, 0f, 1f), stroke: 1f);
				break;
			}
			case WEATHER_TYPE.WT_SNOW:
			case WEATHER_TYPE.WT_STORM:
				reference.X += reference.SpeedX * num3;
				reference.Y += reference.SpeedY * num3;
				destinationRectangle.X = x + (int)reference.X;
				destinationRectangle.Y = y + (int)reference.Y;
				batcher.Draw(SolidColorTextureCache.GetTexture(Color.White), destinationRectangle, new Vector3(1f, 0f, 1f));
				break;
			}
		}
		_lastTick = Time.Ticks;
	}
}

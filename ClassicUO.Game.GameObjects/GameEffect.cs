using System;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.GameObjects;

internal abstract class GameEffect : GameObject
{
	private readonly EffectManager _manager;

	public bool CanCreateExplosionEffect;

	public ushort AnimationGraphic = ushort.MaxValue;

	public AnimDataFrame AnimDataFrame;

	public byte AnimIndex;

	public float AngleToTarget;

	public GraphicEffectBlendMode Blend;

	public long Duration = -1L;

	public uint IntervalInMs;

	public bool IsEnabled;

	public long NextChangeFrameTime;

	public GameObject Source;

	protected GameObject Target;

	protected int TargetX;

	protected int TargetY;

	protected int TargetZ;

	private static readonly Lazy<BlendState> _multiplyBlendState = new Lazy<BlendState>(() => new BlendState
	{
		ColorSourceBlend = Microsoft.Xna.Framework.Graphics.Blend.Zero,
		ColorDestinationBlend = Microsoft.Xna.Framework.Graphics.Blend.SourceColor
	});

	private static readonly Lazy<BlendState> _screenBlendState = new Lazy<BlendState>(() => new BlendState
	{
		ColorSourceBlend = Microsoft.Xna.Framework.Graphics.Blend.One,
		ColorDestinationBlend = Microsoft.Xna.Framework.Graphics.Blend.One
	});

	private static readonly Lazy<BlendState> _screenLessBlendState = new Lazy<BlendState>(() => new BlendState
	{
		ColorSourceBlend = Microsoft.Xna.Framework.Graphics.Blend.DestinationColor,
		ColorDestinationBlend = Microsoft.Xna.Framework.Graphics.Blend.InverseSourceAlpha
	});

	private static readonly Lazy<BlendState> _normalHalfBlendState = new Lazy<BlendState>(() => new BlendState
	{
		ColorSourceBlend = Microsoft.Xna.Framework.Graphics.Blend.DestinationColor,
		ColorDestinationBlend = Microsoft.Xna.Framework.Graphics.Blend.SourceColor
	});

	private static readonly Lazy<BlendState> _shadowBlueBlendState = new Lazy<BlendState>(() => new BlendState
	{
		ColorSourceBlend = Microsoft.Xna.Framework.Graphics.Blend.SourceColor,
		ColorDestinationBlend = Microsoft.Xna.Framework.Graphics.Blend.InverseSourceColor,
		ColorBlendFunction = BlendFunction.ReverseSubtract
	});

	public bool IsMoving
	{
		get
		{
			if (Target == null)
			{
				if (TargetX != 0)
				{
					return TargetY != 0;
				}
				return false;
			}
			return true;
		}
	}

	protected GameEffect(EffectManager manager, ushort graphic, ushort hue, int duration, byte speed)
	{
		_manager = manager;
		Graphic = graphic;
		Hue = hue;
		AllowedToDraw = GameObject.CanBeDrawn(graphic);
		AlphaHue = byte.MaxValue;
		AnimDataFrame = AnimDataLoader.Instance?.CalculateCurrentGraphic(graphic) ?? default(AnimDataFrame);
		IsEnabled = true;
		AnimIndex = 0;
		speed *= 10;
		if (speed == 0)
		{
			speed = 50;
		}
		if (AnimDataFrame.FrameInterval == 0)
		{
			IntervalInMs = speed;
		}
		else
		{
			IntervalInMs = (uint)(AnimDataFrame.FrameInterval * speed);
		}
		Duration = ((duration > 0) ? (Time.Ticks + duration) : (-1));
	}

	public unsafe override void Update(double totalTime, double frameTime)
	{
		base.Update(totalTime, frameTime);
		if (Source != null && Source.IsDestroyed)
		{
			Destroy();
		}
		else
		{
			if (base.IsDestroyed)
			{
				return;
			}
			if (IsEnabled)
			{
				if ((double)Duration < totalTime && Duration >= 0)
				{
					Destroy();
				}
				else
				{
					if (!((double)NextChangeFrameTime < totalTime))
					{
						return;
					}
					if (AnimDataFrame.FrameCount != 0)
					{
						AnimationGraphic = (ushort)(Graphic + AnimDataFrame.FrameData[AnimIndex]);
						AnimIndex++;
						if (AnimIndex >= AnimDataFrame.FrameCount)
						{
							AnimIndex = 0;
						}
					}
					else
					{
						AnimationGraphic = Graphic;
					}
					NextChangeFrameTime = (long)totalTime + IntervalInMs;
				}
			}
			else
			{
				AnimationGraphic = Graphic;
			}
		}
	}

	protected (int x, int y, int z) GetSource()
	{
		(ushort, ushort, int) tuple = ((Source == null) ? (X, Y, Z) : (Source.X, Source.Y, Source.Z + ((this is MovingEffect) ? 8 : 0)));
		return (x: tuple.Item1, y: tuple.Item2, z: tuple.Item3);
	}

	protected void CreateExplosionEffect()
	{
		if (CanCreateExplosionEffect)
		{
			(int x, int y, int z) target = GetTarget();
			int item = target.x;
			int item2 = target.y;
			int item3 = target.z;
			FixedEffect fixedEffect = new FixedEffect(_manager, 14027, Hue, 400, 0);
			fixedEffect.Blend = Blend;
			fixedEffect.SetSource(item, item2, item3);
			_manager.PushToBack(fixedEffect);
		}
	}

	public void SetSource(GameObject source)
	{
		Source = source;
		X = source.X;
		Y = source.Y;
		Z = source.Z;
		if (source is Mobile)
		{
			Z += 8;
		}
		UpdateScreenPosition();
		AddToTile();
	}

	public void SetSource(int x, int y, int z)
	{
		Source = null;
		X = (ushort)x;
		Y = (ushort)y;
		Z = (sbyte)z;
		UpdateScreenPosition();
		AddToTile();
	}

	protected (int x, int y, int z) GetTarget()
	{
		if (Target == null)
		{
			return (x: TargetX, y: TargetY, z: TargetZ);
		}
		return (x: Target.X, y: Target.Y, z: Target.Z + ((Target is Mobile) ? 8 : 0));
	}

	public void SetTarget(GameObject target)
	{
		Target = target;
	}

	public void SetTarget(int x, int y, int z)
	{
		Target = null;
		TargetX = x;
		TargetY = y;
		TargetZ = z;
	}

	public override void Destroy()
	{
		_manager?.Remove(this);
		AnimIndex = 0;
		Source = null;
		Target = null;
		base.Destroy();
	}

	public override bool Draw(UltimaBatcher2D batcher, int posX, int posY, float depth)
	{
		if (base.IsDestroyed || !AllowedToDraw)
		{
			return false;
		}
		if (AnimationGraphic == ushort.MaxValue)
		{
			return false;
		}
		ref StaticTiles reference = ref TileDataLoader.Instance.StaticData[Graphic];
		posX += (int)base.CumulativeOffset.X;
		posY += (int)(base.CumulativeOffset.Z + base.CumulativeOffset.Y);
		ushort hue = Hue;
		if (ProfileManager.CurrentProfile.NoColorObjectsOutOfRange && base.Distance > World.ClientViewRange)
		{
			hue = 907;
		}
		else if (World.Player.IsDead && ProfileManager.CurrentProfile.EnableBlackWhiteEffect)
		{
			hue = 910;
		}
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(hue, reference.IsPartialHue, reference.IsTranslucent ? 0.5f : 1f, gump: false, effect: true);
		switch (Blend)
		{
		case GraphicEffectBlendMode.Multiply:
			batcher.SetBlendState(_multiplyBlendState.Value);
			GameObject.DrawStaticRotated(batcher, AnimationGraphic, posX, posY, AngleToTarget, hueVector, depth, this is MovingEffect);
			batcher.SetBlendState(null);
			break;
		case GraphicEffectBlendMode.Screen:
		case GraphicEffectBlendMode.ScreenMore:
			batcher.SetBlendState(_screenBlendState.Value);
			GameObject.DrawStaticRotated(batcher, AnimationGraphic, posX, posY, AngleToTarget, hueVector, depth, this is MovingEffect);
			batcher.SetBlendState(null);
			break;
		case GraphicEffectBlendMode.ScreenLess:
			hueVector = ShaderHueTranslator.GetHueVector(hue, reference.IsPartialHue, 0.8f, gump: false, effect: true);
			GameObject.DrawStaticRotated(batcher, AnimationGraphic, posX, posY, AngleToTarget, hueVector, depth, this is MovingEffect);
			break;
		case GraphicEffectBlendMode.NormalHalfTransparent:
			batcher.SetBlendState(_normalHalfBlendState.Value);
			GameObject.DrawStaticRotated(batcher, AnimationGraphic, posX, posY, AngleToTarget, hueVector, depth, this is MovingEffect);
			batcher.SetBlendState(null);
			break;
		case GraphicEffectBlendMode.ShadowBlue:
			batcher.SetBlendState(_shadowBlueBlendState.Value);
			GameObject.DrawStaticRotated(batcher, AnimationGraphic, posX, posY, AngleToTarget, hueVector, depth, this is MovingEffect);
			batcher.SetBlendState(null);
			break;
		default:
			GameObject.DrawStaticRotated(batcher, AnimationGraphic, posX, posY, AngleToTarget, hueVector, depth, this is MovingEffect);
			break;
		}
		if (reference.IsLight && Source != null)
		{
			Client.Game.GetScene<GameScene>().AddLight(Source, Source, posX + 22, posY + 22);
		}
		return true;
	}

	public override bool CheckMouseSelection()
	{
		return false;
	}
}

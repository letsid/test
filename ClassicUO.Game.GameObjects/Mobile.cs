using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ClassicUO.Configuration;
using ClassicUO.Data;
using ClassicUO.Game.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using ClassicUO.Utility;
using ClassicUO.Utility.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.GameObjects;

internal class Mobile : Entity
{
	internal struct Step
	{
		public int X;

		public int Y;

		public sbyte Z;

		public byte Direction;

		public bool Run;
	}

	private static readonly QueuedPool<Mobile> _pool = new QueuedPool<Mobile>(300, delegate(Mobile mobile)
	{
		mobile.IsDestroyed = false;
		mobile.Graphic = 0;
		mobile.Steps.Clear();
		mobile.Offset = Vector3.Zero;
		mobile.SpeedMode = CharacterSpeedType.Normal;
		mobile.Race = (RaceType)0;
		mobile.Hits = 0;
		mobile.HitsMax = 0;
		mobile.Mana = 0;
		mobile.ManaMax = 0;
		mobile.Stamina = 0;
		mobile.StaminaMax = 0;
		mobile.NotorietyFlag = NotorietyFlag.Unknown;
		mobile.IsRenamable = false;
		mobile.Flags = Flags.None;
		mobile.IsFemale = false;
		mobile.InWarMode = false;
		mobile.IsRunning = false;
		mobile._animationInterval = 0;
		mobile.AnimationFrameCount = 0;
		mobile._animationRepeateMode = 1;
		mobile._animationRepeatModeCount = 1;
		mobile._animationRepeat = false;
		mobile.AnimationFromServer = false;
		mobile._isAnimationForwardDirection = false;
		mobile.LastStepSoundTime = 0L;
		mobile.StepSoundOffset = 0;
		mobile.Title = string.Empty;
		mobile._animationGroup = byte.MaxValue;
		mobile._isDead = false;
		mobile._isSA_Poisoned = false;
		mobile._lastAnimationIdleDelay = 0L;
		mobile.X = 0;
		mobile.Y = 0;
		mobile.Z = 0;
		mobile.Direction = Direction.North;
		mobile.LastAnimationChangeTime = Time.Ticks;
		mobile.TextContainer?.Clear();
		mobile.HitsPercentage = 0;
		mobile.HitsTexture?.Destroy();
		mobile.HitsTexture = null;
		mobile.IsFlipped = false;
		mobile.FrameInfo = Rectangle.Empty;
		mobile.ObjectHandlesStatus = ObjectHandlesStatus.NONE;
		mobile.AlphaHue = 0;
		mobile.AllowedToDraw = true;
		mobile.IsClicked = false;
		mobile.RemoveFromTile();
		mobile.Clear();
		mobile.Next = null;
		mobile.Previous = null;
		mobile.Name = null;
		mobile.ExecuteAnimation = true;
		mobile.HitsRequest = HitsRequestStatus.None;
		mobile.CalculateRandomIdleTime();
	});

	private static readonly byte[,] _animationIdle = new byte[3, 3]
	{
		{ 9, 10, 9 },
		{ 17, 18, 17 },
		{ 5, 6, 34 }
	};

	private bool _isDead;

	private bool _isSA_Poisoned;

	private long _lastAnimationIdleDelay;

	private bool _isAnimationForwardDirection;

	private byte _animationGroup = byte.MaxValue;

	private byte _animationInterval;

	private bool _animationRepeat;

	private ushort _animationRepeateMode = 1;

	private ushort _animationRepeatModeCount = 1;

	public byte AnimationFrameCount;

	public bool AnimationFromServer;

	public bool IsFemale;

	public bool IsRenamable;

	public bool IsRunning;

	public long LastStepSoundTime;

	public ushort Mana;

	public ushort ManaMax;

	public NotorietyFlag NotorietyFlag;

	public RaceType Race;

	public CharacterSpeedType SpeedMode;

	public ushort Stamina;

	public ushort StaminaMax;

	public int StepSoundOffset;

	public string Title = string.Empty;

	private static readonly ushort[] HANDS_BASE_ANIMID = new ushort[20]
	{
		611, 612, 613, 614, 615, 616, 617, 621, 624, 626,
		628, 634, 636, 639, 641, 646, 648, 649, 651, 0
	};

	private static readonly ushort[] HAND2_BASE_ANIMID = new ushort[10] { 576, 577, 578, 579, 580, 581, 582, 992, 993, 0 };

	private const int SIT_OFFSET_Y = 4;

	private static EquipConvData? _equipConvData;

	private static int _characterFrameStartY;

	private static int _startCharacterWaistY;

	private static int _startCharacterKneesY;

	private static int _startCharacterFeetY;

	private static int _characterFrameHeight;

	public Deque<Step> Steps { get; } = new Deque<Step>(5);

	public bool IsParalyzed => (Flags & Flags.Frozen) != 0;

	public bool IsYellowHits
	{
		get
		{
			if ((Flags & Flags.YellowBar) == 0)
			{
				return NotorietyFlag.HasFlag(NotorietyFlag.Invulnerable);
			}
			return true;
		}
	}

	public bool IsPoisoned
	{
		get
		{
			if (Client.Version < ClientVersion.CV_7000)
			{
				return (Flags & Flags.Poisoned) != 0;
			}
			return _isSA_Poisoned;
		}
	}

	public bool IgnoreCharacters => (Flags & Flags.IgnoreMobiles) != 0;

	public bool IsDead
	{
		get
		{
			if (Graphic != 402 && Graphic != 403 && (Graphic < 607 || Graphic > 608) && Graphic != 694 && Graphic != 695 && !_isDead)
			{
				if (Flags.HasFlag(Flags.Frozen) && Flags.HasFlag(Flags.Hidden))
				{
					return this is PlayerMobile;
				}
				return false;
			}
			return true;
		}
		set
		{
			_isDead = value;
		}
	}

	public bool IsFlying
	{
		get
		{
			if (Client.Version >= ClientVersion.CV_7000)
			{
				return (Flags & Flags.Poisoned) != 0;
			}
			return false;
		}
	}

	public virtual bool InWarMode
	{
		get
		{
			return (Flags & Flags.WarMode) != 0;
		}
		set
		{
		}
	}

	public bool IsHuman
	{
		get
		{
			if ((Graphic < 400 || Graphic > 403) && (Graphic < 183 || Graphic > 186) && (Graphic < 605 || Graphic > 608) && Graphic != 666 && Graphic != 667 && Graphic != 694 && Graphic != 695 && Graphic != 987 && Graphic != 991 && Graphic != 994 && Graphic != 744 && Graphic != 745)
			{
				return Graphic == 1253;
			}
			return true;
		}
	}

	public bool IsGargoyle
	{
		get
		{
			if (Client.Version < ClientVersion.CV_7000 || Graphic != 666)
			{
				return Graphic == 667;
			}
			return true;
		}
	}

	public bool IsMounted
	{
		get
		{
			Item item = FindItemByLayer(Layer.Mount);
			if (item != null && !IsDrivingBoat && item.GetGraphicForAnimation() != ushort.MaxValue)
			{
				return true;
			}
			return false;
		}
	}

	public bool IsDrivingBoat
	{
		get
		{
			Item item = FindItemByLayer(Layer.Mount);
			if (item != null)
			{
				return item.Graphic == 16022;
			}
			return false;
		}
	}

	public virtual bool IsWalking => LastStepTime > Time.Ticks - 150;

	public uint _lastTextActivity { get; set; } = uint.MaxValue;

	public uint _lastTextActivityPointCount { get; set; } = 1u;

	public uint _lastTextActivityPointCountChange { get; set; }

	public RenderedText _lastTextActivityPointRenderedText { get; set; }

	public Mobile(uint serial)
		: base(serial)
	{
		LastAnimationChangeTime = Time.Ticks;
		CalculateRandomIdleTime();
	}

	public Mobile()
		: base(0u)
	{
	}

	public static Mobile Create(uint serial)
	{
		Mobile one = _pool.GetOne();
		one.Serial = serial;
		return one;
	}

	public Item GetSecureTradeBox()
	{
		for (LinkedObject linkedObject = Items; linkedObject != null; linkedObject = linkedObject.Next)
		{
			Item item = (Item)linkedObject;
			if (item.Graphic == 7774 && item.Layer == Layer.Invalid)
			{
				return item;
			}
		}
		return null;
	}

	public void SetSAPoison(bool value)
	{
		_isSA_Poisoned = value;
	}

	private void CalculateRandomIdleTime()
	{
		_lastAnimationIdleDelay = Time.Ticks + (30000 + RandomHelper.GetValue(0, 30000));
	}

	public override void Update(double totalTime, double frameTime)
	{
		if (!base.IsDestroyed)
		{
			base.Update(totalTime, frameTime);
			if (_lastAnimationIdleDelay < Time.Ticks)
			{
				SetIdleAnimation();
			}
			ProcessAnimation(out var _, evalutate: true);
		}
	}

	public void ClearSteps()
	{
		Steps.Clear();
		Offset = Vector3.Zero;
	}

	public bool EnqueueStep(int x, int y, sbyte z, Direction direction, bool run)
	{
		if (Steps.Count >= 5)
		{
			return false;
		}
		GetEndPosition(out var x2, out var y2, out var z2, out var dir);
		if (x2 == x && y2 == y && z2 == z && dir == direction)
		{
			return true;
		}
		if (Steps.Count == 0)
		{
			if (!IsWalking)
			{
				SetAnimation(byte.MaxValue, 0, 0, 0);
			}
			LastStepTime = Time.Ticks;
		}
		Direction direction2 = DirectionHelper.CalculateDirection(x2, y2, x, y);
		Step value = default(Step);
		if (direction2 != Direction.NONE)
		{
			if (direction2 != dir)
			{
				value.X = x2;
				value.Y = y2;
				value.Z = z2;
				value.Direction = (byte)direction2;
				value.Run = run;
				Steps.AddToBack(value);
			}
			value.X = x;
			value.Y = y;
			value.Z = z;
			value.Direction = (byte)direction2;
			value.Run = run;
			Steps.AddToBack(value);
		}
		if (direction2 != direction)
		{
			value.X = x;
			value.Y = y;
			value.Z = z;
			value.Direction = (byte)direction;
			value.Run = run;
			Steps.AddToBack(value);
		}
		return true;
	}

	internal void GetEndPosition(out int x, out int y, out sbyte z, out Direction dir)
	{
		if (Steps.Count == 0)
		{
			x = X;
			y = Y;
			z = Z;
			dir = base.Direction;
		}
		else
		{
			ref Step reference = ref Steps.Back();
			x = reference.X;
			y = reference.Y;
			z = reference.Z;
			dir = (Direction)reference.Direction;
		}
	}

	public void SetAnimation(byte id, byte interval = 0, byte frameCount = 0, ushort repeatCount = 0, bool repeat = false, bool forward = false, bool fromServer = false)
	{
		_animationGroup = id;
		AnimIndex = (byte)((!forward) ? frameCount : 0);
		_animationInterval = interval;
		AnimationFrameCount = (byte)((!forward) ? frameCount : 0);
		_animationRepeateMode = repeatCount;
		_animationRepeatModeCount = repeatCount;
		_animationRepeat = repeat;
		_isAnimationForwardDirection = forward;
		AnimationFromServer = fromServer;
		LastAnimationChangeTime = Time.Ticks;
		CalculateRandomIdleTime();
	}

	public void SetIdleAnimation()
	{
		CalculateRandomIdleTime();
		if (IsMounted || InWarMode || !ExecuteAnimation)
		{
			return;
		}
		AnimIndex = 0;
		AnimationFrameCount = 0;
		_animationInterval = 1;
		_animationRepeateMode = 1;
		_animationRepeatModeCount = 1;
		_isAnimationForwardDirection = true;
		_animationRepeat = false;
		AnimationFromServer = true;
		ushort num = GetGraphicForAnimation();
		if (num >= 4096)
		{
			return;
		}
		ANIMATION_GROUPS_TYPE aNIMATION_GROUPS_TYPE = AnimationsLoader.Instance.DataIndex[num].Type;
		if ((!AnimationsLoader.Instance.DataIndex[num].IsUOP || AnimationsLoader.Instance.DataIndex[num].IsValidMUL) && !AnimationsLoader.Instance.DataIndex[num].HasBodyConversion)
		{
			ushort graphic = AnimationsLoader.Instance.DataIndex[num].Graphic;
			if (num != graphic)
			{
				num = graphic;
				ANIMATION_GROUPS_TYPE type = AnimationsLoader.Instance.DataIndex[num].Type;
				if (type != aNIMATION_GROUPS_TYPE)
				{
					aNIMATION_GROUPS_TYPE = type;
				}
			}
		}
		ANIMATION_FLAGS flags = AnimationsLoader.Instance.DataIndex[num].Flags;
		ANIMATION_GROUPS aNIMATION_GROUPS = ANIMATION_GROUPS.AG_NONE;
		bool flag = false;
		if ((flags & ANIMATION_FLAGS.AF_CALCULATE_OFFSET_LOW_GROUP_EXTENDED) != 0)
		{
			flag = true;
			aNIMATION_GROUPS_TYPE = ANIMATION_GROUPS_TYPE.MONSTER;
		}
		else if ((flags & ANIMATION_FLAGS.AF_CALCULATE_OFFSET_BY_LOW_GROUP) != 0)
		{
			aNIMATION_GROUPS_TYPE = ANIMATION_GROUPS_TYPE.ANIMAL;
		}
		switch (aNIMATION_GROUPS_TYPE)
		{
		case ANIMATION_GROUPS_TYPE.MONSTER:
		case ANIMATION_GROUPS_TYPE.SEA_MONSTER:
			aNIMATION_GROUPS = ANIMATION_GROUPS.AG_HIGHT;
			break;
		case ANIMATION_GROUPS_TYPE.ANIMAL:
			aNIMATION_GROUPS = ANIMATION_GROUPS.AG_LOW;
			break;
		case ANIMATION_GROUPS_TYPE.HUMAN:
		case ANIMATION_GROUPS_TYPE.EQUIPMENT:
			aNIMATION_GROUPS = ANIMATION_GROUPS.AG_PEOPLE;
			break;
		}
		if (aNIMATION_GROUPS == ANIMATION_GROUPS.AG_NONE)
		{
			return;
		}
		if ((flags & ANIMATION_FLAGS.AF_USE_UOP_ANIMATION) != 0)
		{
			if (aNIMATION_GROUPS != ANIMATION_GROUPS.AG_PEOPLE)
			{
				if (InWarMode)
				{
					_animationGroup = 28;
				}
				else
				{
					_animationGroup = 26;
				}
				return;
			}
			if (IsGargoyle && IsFlying)
			{
				if (RandomHelper.GetValue(0, 2) != 0)
				{
					_animationGroup = 66;
				}
				else
				{
					_animationGroup = 67;
				}
				return;
			}
		}
		int value = RandomHelper.GetValue(0, 2);
		byte animationGroup = _animationGroup;
		_animationGroup = _animationIdle[(byte)aNIMATION_GROUPS - 1, value];
		if (flag && _animationGroup == 18)
		{
			if (!AnimationsLoader.Instance.IsAnimationExists(num, 18) && AnimationsLoader.Instance.IsAnimationExists(num, 17))
			{
				_animationGroup = GetReplacedObjectAnimation(num, 17);
			}
			else
			{
				_animationGroup = 1;
			}
		}
		if (!AnimationsLoader.Instance.IsAnimationExists(num, _animationGroup))
		{
			value = ((value == 0) ? 1 : 0);
			_animationGroup = _animationIdle[(byte)aNIMATION_GROUPS - 1, value];
			if (!AnimationsLoader.Instance.IsAnimationExists(num, _animationGroup))
			{
				SetAnimation(animationGroup, 0, 0, 0);
			}
		}
	}

	protected virtual bool NoIterateAnimIndex()
	{
		if (ExecuteAnimation)
		{
			if (LastStepTime > Time.Ticks - 150)
			{
				return Steps.Count == 0;
			}
			return false;
		}
		return true;
	}

	private void ProcessFootstepsSound()
	{
		if (!ProfileManager.CurrentProfile.EnableFootstepsSound || !IsHuman || base.IsHidden || IsDead || IsFlying)
		{
			return;
		}
		long num = Time.Ticks;
		if (Steps.Count == 0 || LastStepSoundTime >= num)
		{
			return;
		}
		ref Step reference = ref Steps.Back();
		int num2 = StepSoundOffset;
		int num3 = 299;
		int num4 = 400;
		if (IsMounted)
		{
			if (reference.Run)
			{
				num3 = 297;
				num4 = 150;
			}
			else
			{
				num2 = 0;
				num4 = 350;
			}
		}
		num4 = num4 * 13 / 10;
		num3 += num2;
		StepSoundOffset = (num2 + 1) % 2;
		Client.Game.Scene.Audio.PlaySoundWithDistance(num3, reference.X, reference.Y);
		LastStepSoundTime = num + num4;
	}

	public override void ProcessAnimation(out byte dir, bool evalutate = false)
	{
		ProcessSteps(out dir, evalutate);
		ProcessFootstepsSound();
		if (LastAnimationChangeTime >= Time.Ticks || NoIterateAnimIndex())
		{
			return;
		}
		ushort graphic = GetGraphicForAnimation();
		byte group = GetGroupForAnimation(this, graphic, isParent: true);
		bool mirror = false;
		AnimationsLoader.Instance.GetAnimDirection(ref dir, ref mirror);
		int num = 80;
		AnimationDirection animDir;
		int num3;
		if (graphic < 4096 && dir < 5)
		{
			ushort hue = 0;
			animDir = AnimationsLoader.Instance.GetBodyAnimationGroup(ref graphic, ref group, ref hue, isParent: true).Direction[dir];
			if (animDir != null && (animDir.FrameCount == 0 || animDir.SpriteInfos == null))
			{
				AnimationsLoader.Instance.LoadAnimationFrames(graphic, group, dir, ref animDir);
			}
			if (animDir != null && animDir.FrameCount != 0)
			{
				int num2 = animDir.FrameCount;
				num3 = AnimIndex + ((!AnimationFromServer || _isAnimationForwardDirection) ? 1 : (-1));
				if (AnimationFromServer)
				{
					num += num * (_animationInterval + 1);
					if (AnimationFrameCount == 0)
					{
						AnimationFrameCount = (byte)num2;
					}
					else
					{
						num2 = AnimationFrameCount;
					}
					if (_isAnimationForwardDirection && num3 >= num2)
					{
						num3 = 0;
					}
					else
					{
						if (_isAnimationForwardDirection || num3 >= 0)
						{
							goto IL_01d1;
						}
						num3 = ((num2 != 0) ? ((byte)(animDir.FrameCount - 1)) : 0);
					}
					if (_animationRepeateMode != 0 && --_animationRepeateMode <= 0)
					{
						if (_animationRepeat)
						{
							_animationRepeatModeCount = _animationRepeateMode;
							_animationRepeat = false;
						}
						else
						{
							SetAnimation(byte.MaxValue, 0, 0, 0);
						}
					}
				}
				else if (num3 >= num2)
				{
					num3 = 0;
					if ((Serial & 0x80000000u) != 0)
					{
						World.CorpseManager.Remove(0u, Serial);
						World.RemoveMobile(Serial);
					}
				}
				goto IL_01d1;
			}
			if ((Serial & 0x80000000u) != 0)
			{
				World.CorpseManager.Remove(0u, Serial);
				World.RemoveMobile(Serial);
			}
		}
		else if ((Serial & 0x80000000u) != 0)
		{
			World.CorpseManager.Remove(0u, Serial);
			World.RemoveMobile(Serial);
		}
		goto IL_023e;
		IL_01d1:
		AnimIndex = (byte)(num3 % animDir.FrameCount);
		goto IL_023e;
		IL_023e:
		LastAnimationChangeTime = Time.Ticks + num;
	}

	public void ProcessSteps(out byte dir, bool evalutate = false)
	{
		dir = (byte)base.Direction;
		dir &= 7;
		if (Steps.Count == 0 || base.IsDestroyed)
		{
			return;
		}
		ref Step reference = ref Steps.Front();
		dir = reference.Direction;
		if (reference.Run)
		{
			dir &= 7;
		}
		if (!evalutate)
		{
			return;
		}
		if (AnimationFromServer)
		{
			SetAnimation(byte.MaxValue, 0, 0, 0);
		}
		int num = (int)(Time.Ticks - LastStepTime);
		bool flag = IsMounted || SpeedMode == CharacterSpeedType.FastUnmount || SpeedMode == CharacterSpeedType.FastUnmountAndCantRun || IsFlying;
		bool run = reference.Run;
		if (!flag && Serial != (uint)World.Player && Steps.Count > 1 && num > 0)
		{
			flag = num <= (run ? 100 : 200);
		}
		int num2 = MovementSpeed.TimeToCompleteMovement(run, flag) - (int)Client.Game.FrameDelay[1];
		bool flag2 = num >= num2;
		bool flag3 = false;
		if (X != reference.X || Y != reference.Y)
		{
			bool flag4 = false;
			if (base.CumulativeOffset.X == 0f && base.CumulativeOffset.Y == 0f)
			{
				int num3 = Math.Abs(X - reference.X);
				int num4 = Math.Abs(Y - reference.Y);
				flag4 = num3 > 1 || num4 > 1 || num3 + num4 == 0;
				if (!flag4)
				{
					num3 = X;
					num4 = Y;
					Pathfinder.GetNewXY((byte)(reference.Direction & 7), ref num3, ref num4);
					flag4 = num3 != reference.X || num4 != reference.Y;
				}
			}
			if (flag4)
			{
				flag2 = true;
			}
			else
			{
				float num5 = (float)num2 / 80f;
				float x = (float)num / 80f;
				float y = x;
				Offset.Z = (sbyte)((float)(reference.Z - Z) * x * (4f / num5));
				MovementSpeed.GetPixelOffset(reference.Direction, ref x, ref y, num5);
				Offset.X = (sbyte)x;
				Offset.Y = (sbyte)y;
			}
		}
		else
		{
			flag3 = true;
			flag2 = true;
		}
		if (flag2)
		{
			if (Serial == (uint)World.Player)
			{
				if (Z - reference.Z >= 22)
				{
					AddMessage(MessageType.Label, ResGeneral.Ouch, TextType.CLIENT);
				}
				if (World.Player.Walker.StepInfos[World.Player.Walker.CurrentWalkSequence].Accepted)
				{
					int num6 = World.Player.Walker.CurrentWalkSequence + 1;
					if (num6 < World.Player.Walker.StepsCount)
					{
						int num7 = World.Player.Walker.StepsCount - num6;
						for (int i = 0; i < num7; i++)
						{
							World.Player.Walker.StepInfos[num6 - 1] = World.Player.Walker.StepInfos[num6];
							num6++;
						}
					}
					World.Player.Walker.StepsCount--;
				}
				else
				{
					World.Player.Walker.CurrentWalkSequence++;
				}
			}
			X = (ushort)reference.X;
			Y = (ushort)reference.Y;
			Z = reference.Z;
			UpdateScreenPosition();
			if (World.InGame && Serial == (uint)World.Player)
			{
				World.Player.CloseRangedGumps();
			}
			base.Direction = (Direction)reference.Direction;
			IsRunning = reference.Run;
			Offset.X = 0f;
			Offset.Y = 0f;
			Offset.Z = 0f;
			Steps.RemoveFromFront();
			CalculateRandomIdleTime();
			if (flag3)
			{
				ProcessSteps(out dir, evalutate);
				return;
			}
			if (TNext != null || TPrevious != null)
			{
				AddToTile();
			}
			LastStepTime = Time.Ticks;
		}
		UpdateTextCoordsV();
	}

	public bool TryGetSittingInfo(out AnimationsLoader.SittingInfoData data)
	{
		ushort num = 0;
		if (IsHuman && !IsMounted && !IsFlying && !TestStepNoChangeDirection(this, GetGroupForAnimation(this, 0, isParent: true)))
		{
			GameObject gameObject = this;
			while (gameObject?.TPrevious != null)
			{
				gameObject = gameObject.TPrevious;
			}
			while (gameObject != null && num == 0)
			{
				Item item = gameObject as Item;
				if (((gameObject is Item && !item.IsMulti) || gameObject is Static || gameObject is Multi) && Math.Abs(Z - gameObject.Z) <= 1 && ChairTable.Table.TryGetValue(gameObject.Graphic, out data))
				{
					return true;
				}
				gameObject = gameObject.TNext;
			}
		}
		data = AnimationsLoader.SittingInfoData.Empty;
		return false;
	}

	public override void UpdateTextCoordsV()
	{
		if (base.TextContainer == null)
		{
			return;
		}
		TextObject textObject = (TextObject)base.TextContainer.Items;
		while (textObject?.Next != null)
		{
			textObject = (TextObject)textObject.Next;
		}
		if (textObject == null)
		{
			return;
		}
		int num = 0;
		bool showMobilesHP = ProfileManager.CurrentProfile.ShowMobilesHP;
		int mobileHPShowWhen = ProfileManager.CurrentProfile.MobileHPShowWhen;
		int mobileHPType = ProfileManager.CurrentProfile.MobileHPType;
		Point realScreenPosition = RealScreenPosition;
		if (IsGargoyle && IsFlying)
		{
			realScreenPosition.Y -= 22;
		}
		else if (!IsMounted)
		{
			realScreenPosition.Y += 22;
		}
		AnimationsLoader.Instance.GetAnimationDimensions(AnimIndex, GetGraphicForAnimation(), 0, 0, IsMounted, 0, out var _, out var centerY, out var _, out var height);
		realScreenPosition.X += (int)base.CumulativeOffset.X + 22;
		realScreenPosition.Y += (int)(base.CumulativeOffset.Y - base.CumulativeOffset.Z - (float)(height + centerY + 8));
		realScreenPosition = Client.Game.Scene.Camera.WorldToScreen(realScreenPosition);
		if (ObjectHandlesStatus == ObjectHandlesStatus.DISPLAYING)
		{
			realScreenPosition.Y -= 18;
		}
		if (showMobilesHP && HitsTexture != null && mobileHPType != 1 && ((mobileHPShowWhen >= 1 && Hits != HitsMax) || mobileHPShowWhen == 0))
		{
			realScreenPosition.Y -= HitsTexture.Height;
		}
		while (textObject != null)
		{
			if (textObject.RenderedText != null && !textObject.RenderedText.IsDestroyed && (num != 0 || textObject.Time >= Time.Ticks))
			{
				textObject.OffsetY = num;
				num += textObject.RenderedText.Height;
				textObject.RealScreenPosition.X = realScreenPosition.X - (textObject.RenderedText.Width >> 1);
				textObject.RealScreenPosition.Y = realScreenPosition.Y - num;
			}
			textObject = (TextObject)textObject.Previous;
		}
		FixTextCoordinatesInScreen();
	}

	public override void CheckGraphicChange(byte animIndex = 0)
	{
		switch (Graphic)
		{
		case 400:
		case 402:
		case 2566:
		case 2567:
			IsFemale = false;
			Race = RaceType.HUMAN;
			break;
		case 401:
		case 403:
		case 2568:
		case 2631:
			IsFemale = true;
			Race = RaceType.HUMAN;
			break;
		case 605:
			IsFemale = false;
			Race = RaceType.ELF;
			break;
		case 606:
			IsFemale = true;
			Race = RaceType.ELF;
			break;
		case 666:
			IsFemale = false;
			Race = RaceType.GARGOYLE;
			break;
		case 667:
			IsFemale = true;
			Race = RaceType.GARGOYLE;
			break;
		}
	}

	public override void Destroy()
	{
		uint value = Serial & 0x3FFFFFFF;
		ClearSteps();
		base.Destroy();
		if (!(this is PlayerMobile))
		{
			UIManager.GetGump<PaperDollGump>(value)?.Dispose();
			_pool.ReturnOne(this);
		}
	}

	public bool IsInvisibleAnimation()
	{
		ushort graphicForAnimation = GetGraphicForAnimation();
		byte groupForAnimation = GetGroupForAnimation(this, graphicForAnimation, isParent: true);
		if (!AnimationsLoader.Instance.IsAnimationExists(graphicForAnimation, groupForAnimation))
		{
			return true;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override ushort GetGraphicForAnimation()
	{
		ushort num = Graphic;
		switch (num)
		{
		case 402:
		case 403:
			num -= 2;
			break;
		case 694:
			num = 667;
			break;
		case 695:
			num = 666;
			break;
		}
		return num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Direction GetDirectionForAnimation()
	{
		if (Steps.Count != 0)
		{
			return (Direction)Steps.Front().Direction;
		}
		return base.Direction;
	}

	private static void CalculateHight(ushort graphic, Mobile mobile, ANIMATION_FLAGS flags, bool isrun, bool iswalking, ref byte result)
	{
		if ((flags & ANIMATION_FLAGS.AF_CALCULATE_OFFSET_BY_PEOPLE_GROUP) != 0)
		{
			if (result == byte.MaxValue)
			{
				result = 0;
			}
		}
		else if ((flags & ANIMATION_FLAGS.AF_CALCULATE_OFFSET_BY_LOW_GROUP) != 0)
		{
			if (!iswalking)
			{
				if (result == byte.MaxValue)
				{
					result = 2;
				}
			}
			else if (isrun)
			{
				result = 1;
			}
			else
			{
				result = 0;
			}
		}
		else if (mobile.IsFlying)
		{
			result = 19;
		}
		else if (!iswalking)
		{
			if (result == byte.MaxValue)
			{
				if ((flags & ANIMATION_FLAGS.AF_IDLE_AT_8_FRAME) != 0 && AnimationsLoader.Instance.IsAnimationExists(graphic, 8))
				{
					result = 8;
				}
				else if ((flags & ANIMATION_FLAGS.AF_USE_UOP_ANIMATION) != 0 && !mobile.InWarMode)
				{
					result = 25;
				}
				else
				{
					result = 1;
				}
			}
		}
		else if (isrun)
		{
			if ((flags & ANIMATION_FLAGS.AF_CAN_FLYING) != 0 && AnimationsLoader.Instance.IsAnimationExists(graphic, 19))
			{
				result = 19;
			}
			else if ((flags & ANIMATION_FLAGS.AF_USE_UOP_ANIMATION) != 0)
			{
				result = 24;
			}
			else
			{
				result = 0;
			}
		}
		else if ((flags & ANIMATION_FLAGS.AF_USE_UOP_ANIMATION) != 0 && !mobile.InWarMode)
		{
			result = 22;
		}
		else
		{
			result = 0;
		}
	}

	private static void LABEL_222(ANIMATION_FLAGS flags, ref ushort v13)
	{
		if ((flags & ANIMATION_FLAGS.AF_CALCULATE_OFFSET_LOW_GROUP_EXTENDED) != 0)
		{
			switch (v13)
			{
			case 0:
				v13 = 0;
				break;
			case 1:
				v13 = 19;
				break;
			case 5:
			case 6:
				if ((flags & ANIMATION_FLAGS.AF_IDLE_AT_8_FRAME) != 0)
				{
					v13 = 4;
				}
				else
				{
					v13 = (ushort)(6u - ((RandomHelper.GetValue() % 2 != 0) ? 1u : 0u));
				}
				break;
			case 8:
				v13 = 2;
				break;
			case 9:
				v13 = 17;
				break;
			case 10:
				v13 = 18;
				if ((flags & ANIMATION_FLAGS.AF_IDLE_AT_8_FRAME) != 0)
				{
					v13--;
				}
				break;
			case 12:
				v13 = 3;
				break;
			default:
				v13 = 1;
				break;
			}
		}
		else if ((flags & ANIMATION_FLAGS.AF_CALCULATE_OFFSET_BY_LOW_GROUP) != 0)
		{
			switch (v13)
			{
			case 0:
				v13 = 0;
				break;
			case 2:
				v13 = 8;
				break;
			case 3:
				v13 = 12;
				break;
			case 4:
			case 6:
			case 7:
			case 8:
			case 9:
			case 12:
			case 13:
			case 14:
				v13 = 5;
				break;
			case 5:
				v13 = 6;
				break;
			case 10:
			case 21:
				v13 = 7;
				break;
			case 11:
				v13 = 3;
				break;
			case 17:
				v13 = 9;
				break;
			case 18:
				v13 = 10;
				break;
			case 19:
				v13 = 1;
				break;
			default:
				v13 = 2;
				break;
			}
		}
		v13 &= 127;
	}

	private static void LABEL_190(ANIMATION_FLAGS flags, ref ushort v13)
	{
		if ((flags & ANIMATION_FLAGS.AF_UNKNOWN_80) != 0 && v13 == 4)
		{
			v13 = 5;
		}
		if ((flags & ANIMATION_FLAGS.AF_UNKNOWN_200) != 0)
		{
			if (v13 - 7 > 9)
			{
				if (v13 == 19)
				{
					v13 = 0;
				}
				else if (v13 > 19)
				{
					v13 = 1;
				}
				LABEL_222(flags, ref v13);
				return;
			}
		}
		else
		{
			if ((flags & ANIMATION_FLAGS.AF_UNKNOWN_100) != 0)
			{
				switch (v13)
				{
				case 10:
				case 15:
				case 16:
					v13 = 1;
					LABEL_222(flags, ref v13);
					break;
				case 11:
					v13 = 17;
					LABEL_222(flags, ref v13);
					break;
				default:
					LABEL_222(flags, ref v13);
					break;
				}
				return;
			}
			if ((flags & ANIMATION_FLAGS.AF_UNKNOWN_1) != 0)
			{
				if (v13 == 21)
				{
					v13 = 10;
				}
				LABEL_222(flags, ref v13);
				return;
			}
			if ((flags & ANIMATION_FLAGS.AF_CALCULATE_OFFSET_BY_PEOPLE_GROUP) == 0)
			{
				LABEL_222(flags, ref v13);
				return;
			}
			switch (v13)
			{
			case 0:
				v13 = 0;
				break;
			case 2:
				v13 = 21;
				LABEL_222(flags, ref v13);
				return;
			case 3:
				v13 = 22;
				LABEL_222(flags, ref v13);
				return;
			case 4:
			case 9:
				v13 = 9;
				LABEL_222(flags, ref v13);
				return;
			case 5:
				v13 = 11;
				LABEL_222(flags, ref v13);
				return;
			case 6:
				v13 = 13;
				LABEL_222(flags, ref v13);
				return;
			case 7:
				v13 = 18;
				LABEL_222(flags, ref v13);
				return;
			case 8:
				v13 = 19;
				LABEL_222(flags, ref v13);
				return;
			case 10:
			case 21:
				v13 = 20;
				LABEL_222(flags, ref v13);
				return;
			case 11:
				v13 = 3;
				LABEL_222(flags, ref v13);
				return;
			case 12:
			case 14:
				v13 = 16;
				LABEL_222(flags, ref v13);
				return;
			case 13:
				v13 = 17;
				LABEL_222(flags, ref v13);
				return;
			case 15:
			case 16:
				v13 = 30;
				LABEL_222(flags, ref v13);
				return;
			case 17:
				v13 = 5;
				LABEL_222(flags, ref v13);
				return;
			case 18:
				v13 = 6;
				LABEL_222(flags, ref v13);
				return;
			case 19:
				v13 = 1;
				LABEL_222(flags, ref v13);
				return;
			}
		}
		v13 = 4;
		LABEL_222(flags, ref v13);
	}

	public unsafe static byte GetGroupForAnimation(Mobile mobile, ushort checkGraphic = 0, bool isParent = false)
	{
		ushort graphic = checkGraphic;
		if (graphic == 0)
		{
			graphic = mobile.GetGraphicForAnimation();
		}
		if (graphic >= 4096)
		{
			return 0;
		}
		ANIMATION_GROUPS_TYPE aNIMATION_GROUPS_TYPE = AnimationsLoader.Instance.DataIndex[graphic].Type;
		AnimationsLoader.Instance.ConvertBodyIfNeeded(ref graphic, isParent);
		ANIMATION_GROUPS_TYPE type = AnimationsLoader.Instance.DataIndex[graphic].Type;
		ANIMATION_FLAGS flags = AnimationsLoader.Instance.DataIndex[graphic].Flags;
		bool flag = (flags & ANIMATION_FLAGS.AF_USE_UOP_ANIMATION) != 0;
		if (mobile.AnimationFromServer && mobile._animationGroup != byte.MaxValue)
		{
			ushort v = mobile._animationGroup;
			if (v == 12 && type != ANIMATION_GROUPS_TYPE.HUMAN && type != ANIMATION_GROUPS_TYPE.EQUIPMENT && (flags & ANIMATION_FLAGS.AF_UNKNOWN_1000) == 0)
			{
				switch (type)
				{
				case ANIMATION_GROUPS_TYPE.HUMAN:
				case ANIMATION_GROUPS_TYPE.EQUIPMENT:
					v = 16;
					break;
				default:
					v = 5;
					break;
				case ANIMATION_GROUPS_TYPE.MONSTER:
					v = 4;
					break;
				}
			}
			switch (type)
			{
			case ANIMATION_GROUPS_TYPE.ANIMAL:
				if (IsReplacedObjectAnimation(0, v))
				{
					aNIMATION_GROUPS_TYPE = ANIMATION_GROUPS_TYPE.UNKNOWN;
				}
				if (v <= 12)
				{
					break;
				}
				switch (v)
				{
				case 23:
					v = 0;
					break;
				case 24:
					v = 1;
					break;
				case 26:
					if (!AnimationsLoader.Instance.IsAnimationExists(graphic, 26) || (mobile.InWarMode && AnimationsLoader.Instance.IsAnimationExists(graphic, 9)))
					{
						v = 9;
					}
					break;
				case 28:
					v = (ushort)(AnimationsLoader.Instance.IsAnimationExists(graphic, 10) ? 10u : 5u);
					break;
				default:
					v = 2;
					break;
				}
				break;
			default:
				if (IsReplacedObjectAnimation(1, v))
				{
					LABEL_190(flags, ref v);
					return (byte)v;
				}
				break;
			case ANIMATION_GROUPS_TYPE.SEA_MONSTER:
				if (IsReplacedObjectAnimation(3, v))
				{
					aNIMATION_GROUPS_TYPE = ANIMATION_GROUPS_TYPE.UNKNOWN;
				}
				if (v > 8)
				{
					v = 2;
				}
				break;
			case ANIMATION_GROUPS_TYPE.MONSTER:
				if (IsReplacedObjectAnimation(2, v))
				{
					aNIMATION_GROUPS_TYPE = ANIMATION_GROUPS_TYPE.UNKNOWN;
				}
				if (!AnimationsLoader.Instance.IsAnimationExists(graphic, (byte)v))
				{
					v = 1;
				}
				if ((flags & ANIMATION_FLAGS.AF_USE_UOP_ANIMATION) == 0 && v > 21)
				{
					v = 1;
				}
				break;
			}
			if (aNIMATION_GROUPS_TYPE == ANIMATION_GROUPS_TYPE.UNKNOWN)
			{
				LABEL_190(flags, ref v);
				return (byte)v;
			}
			if (aNIMATION_GROUPS_TYPE != 0)
			{
				if (aNIMATION_GROUPS_TYPE == ANIMATION_GROUPS_TYPE.ANIMAL && type == ANIMATION_GROUPS_TYPE.MONSTER)
				{
					switch (v)
					{
					case 0:
						v = 0;
						LABEL_190(flags, ref v);
						return (byte)v;
					case 1:
						v = 19;
						LABEL_190(flags, ref v);
						return (byte)v;
					case 3:
						v = 11;
						LABEL_190(flags, ref v);
						return (byte)v;
					case 5:
						v = 4;
						LABEL_190(flags, ref v);
						return (byte)v;
					case 6:
						v = 5;
						LABEL_190(flags, ref v);
						return (byte)v;
					case 7:
					case 11:
						v = 10;
						LABEL_190(flags, ref v);
						return (byte)v;
					case 8:
						v = 2;
						LABEL_190(flags, ref v);
						return (byte)v;
					case 9:
						v = 17;
						LABEL_190(flags, ref v);
						return (byte)v;
					case 10:
						v = 18;
						LABEL_190(flags, ref v);
						return (byte)v;
					case 12:
						v = 3;
						LABEL_190(flags, ref v);
						return (byte)v;
					}
					v = 1;
				}
				LABEL_190(flags, ref v);
				return (byte)v;
			}
			switch (type)
			{
			case ANIMATION_GROUPS_TYPE.HUMAN:
				switch (v)
				{
				case 0:
					v = 0;
					break;
				case 2:
					v = 21;
					break;
				case 3:
					v = 22;
					break;
				case 4:
				case 9:
					v = 9;
					break;
				case 5:
					v = 11;
					break;
				case 6:
					v = 13;
					break;
				case 7:
					v = 18;
					break;
				case 8:
					v = 19;
					break;
				case 10:
				case 21:
					v = 20;
					break;
				case 12:
				case 14:
					v = 16;
					break;
				case 13:
					v = 17;
					break;
				case 15:
				case 16:
					v = 30;
					break;
				case 17:
					v = 5;
					LABEL_190(flags, ref v);
					return (byte)v;
				case 18:
					v = 6;
					LABEL_190(flags, ref v);
					return (byte)v;
				case 19:
					v = 1;
					LABEL_190(flags, ref v);
					return (byte)v;
				default:
					v = 4;
					break;
				}
				break;
			case ANIMATION_GROUPS_TYPE.ANIMAL:
				switch (v)
				{
				case 0:
					v = 0;
					break;
				case 2:
					v = 8;
					LABEL_190(flags, ref v);
					return (byte)v;
				case 3:
					v = 12;
					break;
				case 4:
				case 6:
				case 7:
				case 8:
				case 9:
				case 12:
				case 13:
				case 14:
					v = 5;
					LABEL_190(flags, ref v);
					return (byte)v;
				case 5:
					v = 6;
					LABEL_190(flags, ref v);
					return (byte)v;
				case 10:
				case 21:
					v = 7;
					LABEL_190(flags, ref v);
					return (byte)v;
				case 11:
					v = 3;
					LABEL_190(flags, ref v);
					return (byte)v;
				case 17:
					v = 9;
					break;
				case 18:
					v = 10;
					break;
				case 19:
					v = 1;
					LABEL_190(flags, ref v);
					return (byte)v;
				default:
					v = 2;
					LABEL_190(flags, ref v);
					return (byte)v;
				}
				break;
			case ANIMATION_GROUPS_TYPE.SEA_MONSTER:
				switch (v)
				{
				case 0:
					v = 0;
					break;
				case 2:
				case 3:
					v = 8;
					break;
				case 4:
				case 6:
				case 7:
				case 8:
				case 9:
				case 12:
				case 13:
				case 14:
					v = 5;
					break;
				case 5:
					v = 6;
					break;
				case 10:
				case 21:
					v = 7;
					break;
				case 17:
					v = 3;
					break;
				case 18:
					v = 4;
					break;
				case 19:
					LABEL_190(flags, ref v);
					return (byte)v;
				default:
					v = 2;
					LABEL_190(flags, ref v);
					return (byte)v;
				}
				break;
			}
			LABEL_190(flags, ref v);
			return (byte)v;
		}
		byte result = mobile._animationGroup;
		bool flag2 = mobile.IsWalking;
		bool flag3 = mobile.IsRunning;
		if (mobile.Steps.Count != 0)
		{
			flag2 = true;
			flag3 = mobile.Steps.Front().Run;
		}
		switch (type)
		{
		case ANIMATION_GROUPS_TYPE.ANIMAL:
			if ((flags & ANIMATION_FLAGS.AF_CALCULATE_OFFSET_LOW_GROUP_EXTENDED) != 0)
			{
				CalculateHight(graphic, mobile, flags, flag3, flag2, ref result);
			}
			else if (!flag2)
			{
				if (result == byte.MaxValue)
				{
					result = (byte)(((flags & ANIMATION_FLAGS.AF_USE_UOP_ANIMATION) == 0) ? 2 : ((mobile.InWarMode && AnimationsLoader.Instance.IsAnimationExists(graphic, 1)) ? 1 : 25));
				}
			}
			else
			{
				result = (byte)((!flag3) ? (((flags & ANIMATION_FLAGS.AF_USE_UOP_ANIMATION) != 0 && (!mobile.InWarMode || !AnimationsLoader.Instance.IsAnimationExists(graphic, 0))) ? 22 : 0) : (((flags & ANIMATION_FLAGS.AF_USE_UOP_ANIMATION) == 0) ? (AnimationsLoader.Instance.IsAnimationExists(graphic, 1) ? 1 : 2) : 24));
			}
			break;
		case ANIMATION_GROUPS_TYPE.MONSTER:
			CalculateHight(graphic, mobile, flags, flag3, flag2, ref result);
			break;
		case ANIMATION_GROUPS_TYPE.SEA_MONSTER:
			if (!flag2)
			{
				if (result == byte.MaxValue)
				{
					result = 2;
				}
			}
			else
			{
				result = (byte)(flag3 ? 1 : 0);
			}
			break;
		default:
		{
			Item item = mobile.FindItemByLayer(Layer.TwoHanded);
			if (!flag2)
			{
				if (result != byte.MaxValue)
				{
					break;
				}
				bool flag4 = item != null && item.ItemData.IsLight && item.ItemData.AnimID == graphic;
				if (mobile.IsMounted)
				{
					result = (byte)((!flag4) ? 25 : 28);
					break;
				}
				if (mobile.IsGargoyle && mobile.IsFlying)
				{
					result = (byte)((!mobile.InWarMode) ? 64 : 65);
					break;
				}
				if (!mobile.InWarMode || mobile.IsDead)
				{
					result = (byte)((!flag4) ? ((!flag || type != ANIMATION_GROUPS_TYPE.EQUIPMENT || !AnimationsLoader.Instance.IsAnimationExists(graphic, 37)) ? 4 : 37) : 0);
					break;
				}
				if (flag4)
				{
					result = 2;
					break;
				}
				ushort* ptr = stackalloc ushort[2];
				Item item2 = mobile.FindItemByLayer(Layer.OneHanded);
				if (item2 != null)
				{
					*ptr = item2.ItemData.AnimID;
				}
				if (item != null)
				{
					ptr[1] = item.ItemData.AnimID;
				}
				if (item2 == null)
				{
					if (item != null)
					{
						result = (byte)((!flag || type != ANIMATION_GROUPS_TYPE.EQUIPMENT || AnimationsLoader.Instance.IsAnimationExists(graphic, 7)) ? 7 : 8);
						for (int i = 0; i < 2; i++)
						{
							if (ptr[i] < 611 || ptr[i] > 651)
							{
								continue;
							}
							for (int j = 0; j < HANDS_BASE_ANIMID.Length; j++)
							{
								if (ptr[i] == HANDS_BASE_ANIMID[j])
								{
									result = 8;
									i = 2;
									break;
								}
							}
						}
					}
					else
					{
						result = (byte)((!mobile.IsGargoyle || !mobile.IsFlying) ? 7 : 64);
					}
				}
				else
				{
					result = 7;
				}
			}
			else if (mobile.IsMounted)
			{
				result = (byte)((!flag3) ? 23 : 24);
			}
			else if (flag3 || !mobile.InWarMode || mobile.IsDead)
			{
				if ((flags & ANIMATION_FLAGS.AF_USE_UOP_ANIMATION) == 0)
				{
					result = ((!flag3) ? ((item != null) ? ((byte)1) : ((byte)0)) : ((byte)((item != null) ? 3u : 2u)));
				}
				else if (mobile.IsGargoyle && mobile.IsFlying)
				{
					result = (byte)((!flag3) ? 62 : 63);
				}
				else if (flag3 && AnimationsLoader.Instance.IsAnimationExists(graphic, 24))
				{
					result = 24;
				}
				else if (!flag3)
				{
					result = (byte)((flag && type == ANIMATION_GROUPS_TYPE.EQUIPMENT && !AnimationsLoader.Instance.IsAnimationExists(graphic, 0)) ? 1 : 0);
				}
				else if (flag && type == ANIMATION_GROUPS_TYPE.EQUIPMENT && !AnimationsLoader.Instance.IsAnimationExists(graphic, 2))
				{
					result = 3;
				}
				else
				{
					result = 2;
					if (mobile.IsGargoyle)
					{
						item = mobile.FindItemByLayer(Layer.OneHanded);
					}
				}
				if (!(item != null))
				{
					break;
				}
				ushort animID = item.ItemData.AnimID;
				if (animID < 576 || animID > 993)
				{
					result = (byte)((mobile.IsGargoyle && mobile.IsFlying) ? ((!flag3) ? 62 : 63) : ((!flag3) ? 1 : 3));
					break;
				}
				for (int k = 0; k < HAND2_BASE_ANIMID.Length; k++)
				{
					if (HAND2_BASE_ANIMID[k] == animID)
					{
						result = (byte)((mobile.IsGargoyle && mobile.IsFlying) ? ((!flag3) ? 62 : 63) : ((!flag3) ? 1 : 3));
						break;
					}
				}
			}
			else
			{
				result = (byte)((!mobile.IsGargoyle || !mobile.IsFlying) ? 15 : 62);
			}
			break;
		}
		}
		return result;
	}

	public static bool IsReplacedObjectAnimation(byte anim, ushort v13)
	{
		if (anim < AnimationsLoader.Instance.GroupReplaces.Length)
		{
			foreach (Tuple<ushort, byte> item in AnimationsLoader.Instance.GroupReplaces[anim])
			{
				if (item.Item1 == v13)
				{
					return item.Item2 != byte.MaxValue;
				}
			}
		}
		return false;
	}

	public static byte GetReplacedObjectAnimation(ushort graphic, ushort index)
	{
		return AnimationsLoader.Instance.GetGroupIndex(graphic) switch
		{
			ANIMATION_GROUPS.AG_LOW => (byte)(getReplacedGroup(AnimationsLoader.Instance.GroupReplaces[0], index, 0) % 13), 
			ANIMATION_GROUPS.AG_PEOPLE => (byte)(getReplacedGroup(AnimationsLoader.Instance.GroupReplaces[1], index, 0) % 35), 
			_ => (byte)(index % 22), 
		};
		static ushort getReplacedGroup(List<Tuple<ushort, byte>> list, ushort idx, ushort walkIdx)
		{
			foreach (Tuple<ushort, byte> item in list)
			{
				if (item.Item1 == idx)
				{
					if (item.Item2 == byte.MaxValue)
					{
						return walkIdx;
					}
					return item.Item2;
				}
			}
			return idx;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static byte GetObjectNewAnimation(Mobile mobile, ushort type, ushort action, byte mode)
	{
		if (mobile.Graphic >= 4096)
		{
			return 0;
		}
		switch (type)
		{
		case 0:
			return GetObjectNewAnimationType_0(mobile, action, mode);
		case 1:
		case 2:
			return GetObjectNewAnimationType_1_2(mobile, action, mode);
		case 3:
			return GetObjectNewAnimationType_3(mobile, action, mode);
		case 4:
			return GetObjectNewAnimationType_4(mobile, action, mode);
		case 5:
			return GetObjectNewAnimationType_5(mobile, action, mode);
		case 6:
		case 14:
			return GetObjectNewAnimationType_6_14(mobile, action, mode);
		case 7:
			return GetObjectNewAnimationType_7(mobile, action, mode);
		case 8:
			return GetObjectNewAnimationType_8(mobile, action, mode);
		case 9:
		case 10:
			return GetObjectNewAnimationType_9_10(mobile, action, mode);
		case 11:
			return GetObjectNewAnimationType_11(mobile, action, mode);
		default:
			return 0;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool TestStepNoChangeDirection(Mobile mob, byte group)
	{
		if (((uint)group <= 3u || group == 15 || (uint)(group - 23) <= 1u) && mob.Steps.Count != 0)
		{
			ref Step reference = ref mob.Steps.Front();
			if (reference.X != mob.X || reference.Y != mob.Y)
			{
				return true;
			}
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static byte GetObjectNewAnimationType_0(Mobile mobile, ushort action, byte mode)
	{
		if (action <= 10)
		{
			IndexAnimation indexAnimation = AnimationsLoader.Instance.DataIndex[mobile.Graphic];
			ANIMATION_GROUPS_TYPE aNIMATION_GROUPS_TYPE = ANIMATION_GROUPS_TYPE.MONSTER;
			if (((uint)indexAnimation.Flags & 0x80000000u) != 0)
			{
				aNIMATION_GROUPS_TYPE = indexAnimation.Type;
			}
			switch (aNIMATION_GROUPS_TYPE)
			{
			case ANIMATION_GROUPS_TYPE.MONSTER:
				switch (mode % 4)
				{
				case 1:
					return 5;
				case 2:
					return 6;
				case 3:
					if ((indexAnimation.Flags & ANIMATION_FLAGS.AF_UNKNOWN_1) != 0)
					{
						return 12;
					}
					goto case 0;
				case 0:
					return 4;
				}
				break;
			case ANIMATION_GROUPS_TYPE.SEA_MONSTER:
				if (mode % 2 != 0)
				{
					return 6;
				}
				return 5;
			default:
				if (mobile.IsMounted)
				{
					if (action > 0)
					{
						return action switch
						{
							1 => 27, 
							2 => 28, 
							_ => 26, 
						};
					}
					return 29;
				}
				switch (action)
				{
				case 1:
					return 18;
				case 2:
					return 19;
				case 6:
					return 12;
				case 7:
					if (mobile.IsGargoyle && mobile.IsFlying && AnimationsLoader.Instance.IsAnimationExists(mobile.Graphic, 72))
					{
						return 72;
					}
					return 13;
				case 8:
					return 14;
				case 3:
					return 11;
				case 4:
					return 9;
				case 5:
					return 10;
				}
				if (mobile.IsGargoyle && mobile.IsFlying && AnimationsLoader.Instance.IsAnimationExists(mobile.Graphic, 71))
				{
					return 71;
				}
				if (AnimationsLoader.Instance.IsAnimationExists(mobile.Graphic, 31))
				{
					return 31;
				}
				break;
			case ANIMATION_GROUPS_TYPE.ANIMAL:
				break;
			}
			if ((indexAnimation.Flags & ANIMATION_FLAGS.AF_USE_2_IF_HITTED_WHILE_RUNNING) != 0)
			{
				return 2;
			}
			if (mode % 2 != 0 && AnimationsLoader.Instance.IsAnimationExists(mobile.Graphic, 6))
			{
				return 6;
			}
			return 5;
		}
		return 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static byte GetObjectNewAnimationType_1_2(Mobile mobile, ushort action, byte mode)
	{
		IndexAnimation indexAnimation = AnimationsLoader.Instance.DataIndex[mobile.Graphic];
		ANIMATION_GROUPS_TYPE aNIMATION_GROUPS_TYPE = ANIMATION_GROUPS_TYPE.MONSTER;
		if (((uint)indexAnimation.Flags & 0x80000000u) != 0)
		{
			aNIMATION_GROUPS_TYPE = indexAnimation.Type;
		}
		if (aNIMATION_GROUPS_TYPE != 0)
		{
			if (aNIMATION_GROUPS_TYPE <= ANIMATION_GROUPS_TYPE.ANIMAL || mobile.IsMounted)
			{
				return byte.MaxValue;
			}
			return 30;
		}
		if (mode % 2 != 0)
		{
			return 15;
		}
		return 16;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static byte GetObjectNewAnimationType_3(Mobile mobile, ushort action, byte mode)
	{
		IndexAnimation indexAnimation = AnimationsLoader.Instance.DataIndex[mobile.Graphic];
		ANIMATION_GROUPS_TYPE aNIMATION_GROUPS_TYPE = ANIMATION_GROUPS_TYPE.MONSTER;
		if (((uint)indexAnimation.Flags & 0x80000000u) != 0)
		{
			aNIMATION_GROUPS_TYPE = indexAnimation.Type;
		}
		switch (aNIMATION_GROUPS_TYPE)
		{
		case ANIMATION_GROUPS_TYPE.SEA_MONSTER:
			return 8;
		case ANIMATION_GROUPS_TYPE.ANIMAL:
			if (mode % 2 != 0)
			{
				return 21;
			}
			return 22;
		default:
			if (mode % 2 != 0)
			{
				return 8;
			}
			return 12;
		case ANIMATION_GROUPS_TYPE.MONSTER:
			if (mode % 2 != 0)
			{
				return 2;
			}
			return 3;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static byte GetObjectNewAnimationType_4(Mobile mobile, ushort action, byte mode)
	{
		IndexAnimation indexAnimation = AnimationsLoader.Instance.DataIndex[mobile.Graphic];
		ANIMATION_GROUPS_TYPE aNIMATION_GROUPS_TYPE = ANIMATION_GROUPS_TYPE.MONSTER;
		if (((uint)indexAnimation.Flags & 0x80000000u) != 0)
		{
			aNIMATION_GROUPS_TYPE = indexAnimation.Type;
		}
		if (aNIMATION_GROUPS_TYPE != 0)
		{
			if (aNIMATION_GROUPS_TYPE > ANIMATION_GROUPS_TYPE.ANIMAL)
			{
				if (mobile.IsGargoyle && mobile.IsFlying && AnimationsLoader.Instance.IsAnimationExists(mobile.Graphic, 77))
				{
					return 77;
				}
				if (mobile.IsMounted)
				{
					return byte.MaxValue;
				}
				return 20;
			}
			return 7;
		}
		return 10;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static byte GetObjectNewAnimationType_5(Mobile mobile, ushort action, byte mode)
	{
		IndexAnimation indexAnimation = AnimationsLoader.Instance.DataIndex[mobile.Graphic];
		ANIMATION_GROUPS_TYPE aNIMATION_GROUPS_TYPE = ANIMATION_GROUPS_TYPE.MONSTER;
		if (((uint)indexAnimation.Flags & 0x80000000u) != 0)
		{
			aNIMATION_GROUPS_TYPE = indexAnimation.Type;
		}
		if (aNIMATION_GROUPS_TYPE <= ANIMATION_GROUPS_TYPE.SEA_MONSTER)
		{
			if (mode % 2 != 0)
			{
				return 18;
			}
			return 17;
		}
		if (aNIMATION_GROUPS_TYPE != ANIMATION_GROUPS_TYPE.ANIMAL)
		{
			if (mobile.IsMounted)
			{
				return byte.MaxValue;
			}
			if (mode % 2 != 0)
			{
				return 6;
			}
			return 5;
		}
		return (mode % 3) switch
		{
			1 => 10, 
			2 => 3, 
			_ => 9, 
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static byte GetObjectNewAnimationType_6_14(Mobile mobile, ushort action, byte mode)
	{
		IndexAnimation indexAnimation = AnimationsLoader.Instance.DataIndex[mobile.Graphic];
		ANIMATION_GROUPS_TYPE aNIMATION_GROUPS_TYPE = ANIMATION_GROUPS_TYPE.MONSTER;
		if (((uint)indexAnimation.Flags & 0x80000000u) != 0)
		{
			aNIMATION_GROUPS_TYPE = indexAnimation.Type;
		}
		switch (aNIMATION_GROUPS_TYPE)
		{
		case ANIMATION_GROUPS_TYPE.ANIMAL:
			return 3;
		default:
			if (mobile.IsMounted)
			{
				return byte.MaxValue;
			}
			return 34;
		case ANIMATION_GROUPS_TYPE.SEA_MONSTER:
			return 5;
		case ANIMATION_GROUPS_TYPE.MONSTER:
			return 11;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static byte GetObjectNewAnimationType_7(Mobile mobile, ushort action, byte mode)
	{
		if (mobile.IsMounted)
		{
			return byte.MaxValue;
		}
		if (action > 0)
		{
			if (action == 1)
			{
				return 33;
			}
			return 0;
		}
		return 32;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static byte GetObjectNewAnimationType_8(Mobile mobile, ushort action, byte mode)
	{
		IndexAnimation indexAnimation = AnimationsLoader.Instance.DataIndex[mobile.Graphic];
		ANIMATION_GROUPS_TYPE aNIMATION_GROUPS_TYPE = ANIMATION_GROUPS_TYPE.MONSTER;
		if (((uint)indexAnimation.Flags & 0x80000000u) != 0)
		{
			aNIMATION_GROUPS_TYPE = indexAnimation.Type;
		}
		switch (aNIMATION_GROUPS_TYPE)
		{
		case ANIMATION_GROUPS_TYPE.ANIMAL:
			return 9;
		default:
			if (!mobile.IsMounted)
			{
				return 33;
			}
			return byte.MaxValue;
		case ANIMATION_GROUPS_TYPE.SEA_MONSTER:
			return 3;
		case ANIMATION_GROUPS_TYPE.MONSTER:
			return 11;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static byte GetObjectNewAnimationType_9_10(Mobile mobile, ushort action, byte mode)
	{
		IndexAnimation indexAnimation = AnimationsLoader.Instance.DataIndex[mobile.Graphic];
		ANIMATION_GROUPS_TYPE aNIMATION_GROUPS_TYPE = ANIMATION_GROUPS_TYPE.MONSTER;
		if (((uint)indexAnimation.Flags & 0x80000000u) != 0)
		{
			aNIMATION_GROUPS_TYPE = indexAnimation.Type;
		}
		if (aNIMATION_GROUPS_TYPE != 0)
		{
			if (mobile.IsGargoyle)
			{
				if (mobile.IsFlying)
				{
					if (action == 0)
					{
						return 60;
					}
				}
				else if (action == 0)
				{
					return 61;
				}
			}
			return byte.MaxValue;
		}
		return 20;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static byte GetObjectNewAnimationType_11(Mobile mobile, ushort action, byte mode)
	{
		IndexAnimation indexAnimation = AnimationsLoader.Instance.DataIndex[mobile.Graphic];
		ANIMATION_GROUPS_TYPE aNIMATION_GROUPS_TYPE = ANIMATION_GROUPS_TYPE.MONSTER;
		if (((uint)indexAnimation.Flags & 0x80000000u) != 0)
		{
			aNIMATION_GROUPS_TYPE = indexAnimation.Type;
		}
		if (aNIMATION_GROUPS_TYPE != 0)
		{
			if (aNIMATION_GROUPS_TYPE >= ANIMATION_GROUPS_TYPE.ANIMAL)
			{
				if (mobile.IsMounted)
				{
					return byte.MaxValue;
				}
				if ((uint)(action - 1) <= 1u)
				{
					if (mobile.IsGargoyle && mobile.IsFlying)
					{
						return 76;
					}
					return 17;
				}
				if (mobile.IsGargoyle && mobile.IsFlying)
				{
					return 75;
				}
				return 16;
			}
			return 5;
		}
		return 12;
	}

	public override bool Draw(UltimaBatcher2D batcher, int posX, int posY, float depth)
	{
		if (base.IsDestroyed || !AllowedToDraw)
		{
			return false;
		}
		bool charIsSitting = false;
		ushort num = 0;
		AnimationsLoader.SittingInfoData data = AnimationsLoader.SittingInfoData.Empty;
		_equipConvData = null;
		FrameInfo.X = 0;
		FrameInfo.Y = 0;
		FrameInfo.Width = 0;
		FrameInfo.Height = 0;
		posY -= 3;
		int num2 = posX + (int)base.CumulativeOffset.X;
		int num3 = posY + (int)(base.CumulativeOffset.Y - base.CumulativeOffset.Z);
		num2 += 22;
		num3 += 22;
		bool flag = !IsDead && !base.IsHidden && ProfileManager.CurrentProfile.ShadowsEnabled;
		if (AuraManager.IsEnabled)
		{
			AuraManager.Draw(batcher, num2, num3, (ProfileManager.CurrentProfile.PartyAura && World.Party.Contains(this)) ? ProfileManager.CurrentProfile.PartyAuraHue : Notoriety.GetHue(NotorietyFlag), depth + 9999f);
		}
		bool isHuman = IsHuman;
		bool flag2 = Client.Version >= ClientVersion.CV_7000 && (Graphic == 666 || Graphic == 667 || Graphic == 695 || Graphic == 694);
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(0, partial: false, (float)(int)AlphaHue / 255f);
		if (ProfileManager.CurrentProfile.HighlightGameObjects && SelectedObject.LastObject == this)
		{
			num = ProfileManager.CurrentProfile.HighlightGameObjectsColor;
			hueVector.Y = 1f;
		}
		else if (SelectedObject.HealthbarObject == this)
		{
			num = Notoriety.GetHue(NotorietyFlag);
		}
		else if (ProfileManager.CurrentProfile.NoColorObjectsOutOfRange && base.Distance > World.ClientViewRange)
		{
			num = 907;
			hueVector.Y = 1f;
		}
		else if (World.Player.IsDead && ProfileManager.CurrentProfile.EnableBlackWhiteEffect)
		{
			num = 910;
			hueVector.Y = 1f;
		}
		else if (base.IsHidden)
		{
			num = 910;
		}
		else
		{
			num = 0;
			if (IsDead)
			{
				if (!isHuman)
				{
					num = 902;
				}
			}
			else
			{
				if (ProfileManager.CurrentProfile.HighlightMobilesByPoisoned && IsPoisoned)
				{
					num = ProfileManager.CurrentProfile.PoisonHue;
				}
				if (ProfileManager.CurrentProfile.HighlightMobilesByParalize && IsParalyzed && NotorietyFlag != NotorietyFlag.Invulnerable && InWarMode)
				{
					num = ProfileManager.CurrentProfile.ParalyzedHue;
				}
				if (ProfileManager.CurrentProfile.HighlightMobilesByInvul && NotorietyFlag != NotorietyFlag.Invulnerable && IsYellowHits && InWarMode)
				{
					num = ProfileManager.CurrentProfile.InvulnerableHue;
				}
			}
		}
		bool flag3 = Serial == TargetManager.LastAttack;
		bool flag4 = TargetManager.IsTargeting && SelectedObject.LastObject == this;
		if (Serial != World.Player.Serial && (flag3 || flag4))
		{
			num = Notoriety.GetHue(NotorietyFlag);
		}
		ProcessSteps(out var dir);
		byte b = dir;
		AnimationsLoader.Instance.GetAnimDirection(ref dir, ref IsFlipped);
		ushort graphicForAnimation = GetGraphicForAnimation();
		byte b2 = GetGroupForAnimation(this, graphicForAnimation, isParent: true);
		byte frameIndex = AnimIndex;
		Item item = FindItemByLayer(Layer.Mount);
		sbyte b3 = 0;
		if (isHuman && item != null && item.Graphic != 16022)
		{
			ushort graphicForAnimation2 = item.GetGraphicForAnimation();
			byte b4 = 0;
			if (graphicForAnimation2 != ushort.MaxValue && graphicForAnimation2 < 4096)
			{
				b3 = AnimationsLoader.Instance.DataIndex[graphicForAnimation2].MountedHeightOffset;
				if (flag)
				{
					DrawInternal(batcher, this, null, num2, num3 + 10, hueVector, IsFlipped, frameIndex, hasShadow: true, graphicForAnimation, b2, dir, isHuman, isParent: true, isMount: false, forceUOP: false, depth, b3, num, charIsSitting);
					b4 = GetGroupForAnimation(this, graphicForAnimation2);
					DrawInternal(batcher, this, item, num2, num3, hueVector, IsFlipped, frameIndex, hasShadow: true, graphicForAnimation2, b4, dir, isHuman, isParent: true, isMount: false, forceUOP: false, depth, b3, num, charIsSitting);
				}
				else
				{
					b4 = GetGroupForAnimation(this, graphicForAnimation2);
				}
				DrawInternal(batcher, this, item, num2, num3, hueVector, IsFlipped, frameIndex, hasShadow: false, graphicForAnimation2, b4, dir, isHuman, isParent: true, isMount: true, forceUOP: false, depth, b3, num, charIsSitting);
				num3 += b3;
			}
		}
		else if (TryGetSittingInfo(out data))
		{
			b2 = 4;
			frameIndex = 0;
			ProcessSteps(out dir);
			AnimationsLoader.Instance.FixSittingDirection(ref dir, ref IsFlipped, ref num2, ref num3, ref data);
			num3 += 4;
			if (dir == 3)
			{
				if (IsGargoyle)
				{
					num3 -= 26;
					b2 = 42;
				}
				else
				{
					b2 = 25;
					depth += 1f;
				}
			}
			else if (IsGargoyle)
			{
				b2 = 42;
			}
			else
			{
				charIsSitting = true;
			}
		}
		else if (flag)
		{
			DrawInternal(batcher, this, null, num2, num3, hueVector, IsFlipped, frameIndex, hasShadow: true, graphicForAnimation, b2, dir, isHuman, isParent: true, isMount: false, forceUOP: false, depth, b3, num, charIsSitting);
		}
		DrawInternal(batcher, this, null, num2, num3, hueVector, IsFlipped, frameIndex, hasShadow: false, graphicForAnimation, b2, dir, isHuman, isParent: true, isMount: false, flag2, depth, b3, num, charIsSitting);
		if (!base.IsEmpty)
		{
			for (int i = 0; i < 23; i++)
			{
				Layer layer = LayerOrder.UsedLayers[b, i];
				Item item2 = FindItemByLayer(layer);
				if (item2 == null || (IsDead && (layer == Layer.Hair || layer == Layer.Beard)))
				{
					continue;
				}
				if (isHuman)
				{
					if (IsCovered(this, layer))
					{
						continue;
					}
					if (item2.ItemData.AnimID != 0)
					{
						graphicForAnimation = item2.ItemData.AnimID;
						if (flag2)
						{
							FixGargoyleEquipments(ref graphicForAnimation);
						}
						if (AnimationsLoader.Instance.EquipConversions.TryGetValue(Graphic, out var value) && value.TryGetValue(item2.ItemData.AnimID, out var value2))
						{
							_equipConvData = value2;
							graphicForAnimation = value2.Graphic;
						}
						DrawInternal(batcher, this, item2, num2, num3, hueVector, IsFlipped, frameIndex, hasShadow: false, graphicForAnimation, (flag2 && data.Graphic == 0) ? GetGroupForAnimation(this, graphicForAnimation, isParent: true) : b2, dir, isHuman, isParent: false, isMount: false, flag2, depth, b3, num, charIsSitting);
					}
					else if (item2.ItemData.IsLight)
					{
						Client.Game.GetScene<GameScene>().AddLight(this, item2, num2, num3);
					}
					_equipConvData = null;
				}
				else if (item2.ItemData.IsLight)
				{
					Client.Game.GetScene<GameScene>().AddLight(this, item2, num2, num3);
				}
			}
		}
		FrameInfo.X = Math.Abs(FrameInfo.X);
		FrameInfo.Y = Math.Abs(FrameInfo.Y);
		FrameInfo.Width = FrameInfo.X + FrameInfo.Width;
		FrameInfo.Height = FrameInfo.Y + FrameInfo.Height;
		return true;
	}

	private static ushort GetAnimationInfo(Mobile owner, Item item, bool isGargoyle)
	{
		if (item.ItemData.AnimID != 0)
		{
			ushort graphic = item.ItemData.AnimID;
			if (isGargoyle)
			{
				FixGargoyleEquipments(ref graphic);
			}
			if (AnimationsLoader.Instance.EquipConversions.TryGetValue(owner.Graphic, out var value) && value.TryGetValue(item.ItemData.AnimID, out var value2))
			{
				_equipConvData = value2;
				graphic = value2.Graphic;
			}
			return graphic;
		}
		return ushort.MaxValue;
	}

	private static void FixGargoyleEquipments(ref ushort graphic)
	{
		switch (graphic)
		{
		case 469:
			graphic = 342;
			break;
		case 970:
			graphic = 547;
			break;
		case 984:
			graphic = 329;
			break;
		case 882:
			graphic = 330;
			break;
		case 884:
			graphic = 328;
			break;
		case 879:
			graphic = 327;
			break;
		case 878:
			graphic = 328;
			break;
		case 1062:
			graphic = 1067;
			break;
		case 1529:
			graphic = 1531;
			break;
		case 1530:
			graphic = 1532;
			break;
		}
	}

	private static bool GetTexture(ref ushort graphic, ref byte animGroup, ref byte animIndex, byte direction, out SpriteInfo spriteInfo, out bool isUOP)
	{
		spriteInfo = default(SpriteInfo);
		isUOP = false;
		ushort hue = 0;
		AnimationDirection animationDirection = AnimationsLoader.Instance.GetBodyAnimationGroup(ref graphic, ref animGroup, ref hue, isParent: true).Direction[direction];
		if (animationDirection == null || animationDirection.Address == -1 || animationDirection.FileIndex == -1 || animationDirection.FrameCount == 0 || animationDirection.SpriteInfos == null)
		{
			return false;
		}
		int frameCount = animationDirection.FrameCount;
		if (frameCount > 0 && animIndex >= frameCount)
		{
			animIndex = (byte)(frameCount - 1);
		}
		else if (animIndex < 0)
		{
			animIndex = 0;
		}
		if (animIndex >= animationDirection.FrameCount)
		{
			return false;
		}
		spriteInfo = animationDirection.SpriteInfos[animIndex % animationDirection.FrameCount];
		if (spriteInfo.Texture == null)
		{
			return false;
		}
		isUOP = animationDirection.IsUOP;
		return true;
	}

	private static void DrawInternal(UltimaBatcher2D batcher, Mobile owner, Item entity, int x, int y, Vector3 hueVec, bool mirror, byte frameIndex, bool hasShadow, ushort id, byte animGroup, byte dir, bool isHuman, bool isParent, bool isMount, bool forceUOP, float depth, sbyte mountOffset, ushort overridedHue, bool charIsSitting)
	{
		if (id >= 4096 || owner == null)
		{
			return;
		}
		int num = y - (int)owner.CumulativeOffset.Y - (int)owner.CumulativeOffset.Z;
		_ = owner.CumulativeOffset;
		ushort hue = overridedHue;
		AnimationDirection animDir = AnimationsLoader.Instance.GetBodyAnimationGroup(ref id, ref animGroup, ref hue, isParent, forceUOP).Direction[dir];
		if (((animDir == null || animDir.Address == -1 || animDir.FileIndex == -1) && (!charIsSitting || !(entity == null) || hasShadow)) || ((animDir == null || ((animDir.FrameCount == 0 || animDir.SpriteInfos == null) && !AnimationsLoader.Instance.LoadAnimationFrames(id, animGroup, dir, ref animDir))) && (!charIsSitting || !(entity == null) || hasShadow)) || animDir == null || (entity != null && animDir.IsUOP && !AnimationsLoader.IsNormalMountAnimStub(id)))
		{
			return;
		}
		int frameCount = animDir.FrameCount;
		if (frameCount > 0 && frameIndex >= frameCount)
		{
			frameIndex = (byte)(frameCount - 1);
		}
		else if (frameIndex < 0)
		{
			frameIndex = 0;
		}
		if (frameIndex >= animDir.FrameCount)
		{
			return;
		}
		ref SpriteInfo reference = ref animDir.SpriteInfos[frameIndex % animDir.FrameCount];
		if (reference.Texture == null)
		{
			if (!charIsSitting || !(entity == null) || hasShadow)
			{
				return;
			}
		}
		else
		{
			x = ((!mirror) ? (x - reference.Center.X) : (x - (reference.UV.Width - reference.Center.X)));
			y -= reference.UV.Height + reference.Center.Y;
		}
		if (hasShadow)
		{
			batcher.DrawShadow(reference.Texture, new Vector2(x, y), reference.UV, mirror, depth);
			return;
		}
		ushort num2 = overridedHue;
		bool partial = false;
		if (num2 == 0)
		{
			num2 = entity?.Hue ?? owner.Hue;
			bool flag = TileDataLoader.Instance?.AnimPHueTiles.ContainsKey(id) ?? true;
			partial = !isMount && entity != null && entity.ItemData.IsPartialHue && flag;
			if ((num2 & 0x8000) != 0)
			{
				if (entity == null)
				{
					partial = true;
				}
				num2 &= 0x7FFF;
			}
			if (num2 == 0)
			{
				num2 = hue;
				if (num2 == 0 && _equipConvData.HasValue)
				{
					num2 = _equipConvData.Value.Color;
				}
				partial = false;
			}
		}
		hueVec = ShaderHueTranslator.GetHueVector(num2, partial, hueVec.Z);
		if (reference.Texture != null)
		{
			Vector2 position = new Vector2(x, y);
			Rectangle uV = reference.UV;
			if (charIsSitting)
			{
				Vector3 mod = CalculateSitAnimation(y, entity, isHuman, ref reference);
				batcher.DrawCharacterSitted(reference.Texture, position, uV, mod, hueVec, mirror, depth + 1f);
				owner.PutSittingAnimationIntoPicker(batcher, entity, reference, id, animGroup, mirror, dir, frameIndex);
			}
			else
			{
				int num3 = (int)Math.Min(reference.UV.Height, Math.Max(0f, (float)num - position.Y));
				int num4 = (int)Math.Min(reference.UV.Width, Math.Max(0f, 44f - position.X % 44f));
				int num5 = reference.UV.Height - num3;
				int num6 = 1;
				int num7 = 1;
				uV.Height = num3;
				if (num5 > 0)
				{
					num6 += (int)Math.Ceiling((float)num5 / 44f);
				}
				num5 = reference.UV.Width - num4;
				if (num5 > 0)
				{
					num7 += (int)Math.Ceiling((float)num5 / 44f);
				}
				int num8 = num3;
				for (int i = 0; i < num6; i++)
				{
					int num9 = i * 200;
					float num10 = 1f;
					bool flag2 = (owner.Offset.X > 0f && owner.Offset.Y > 0f) || (owner.Offset.X < 0f && owner.Offset.Y > 0f) || owner.Direction == Direction.South || owner.Direction == Direction.East;
					if (i == 0 && flag2)
					{
						num9 += 4;
					}
					batcher.Draw(reference.Texture, position, uV, hueVec, 0f, Vector2.Zero, num10, mirror ? SpriteEffects.FlipHorizontally : SpriteEffects.None, depth + (float)num9);
					position.Y += (int)((float)uV.Height * num10);
					uV.Y += uV.Height;
					uV.Height = Math.Min(44, reference.UV.Height - num8);
					num8 += uV.Height;
				}
			}
			int num11 = -reference.Center.X;
			int num12 = -(reference.UV.Height + reference.Center.Y + 3);
			if (mirror)
			{
				num11 = -(reference.UV.Width - reference.Center.X);
			}
			if (num11 < owner.FrameInfo.X)
			{
				owner.FrameInfo.X = num11;
			}
			if (num12 < owner.FrameInfo.Y)
			{
				owner.FrameInfo.Y = num12;
			}
			if (owner.FrameInfo.Width < num11 + reference.UV.Width)
			{
				owner.FrameInfo.Width = num11 + reference.UV.Width;
			}
			if (owner.FrameInfo.Height < num12 + reference.UV.Height)
			{
				owner.FrameInfo.Height = num12 + reference.UV.Height;
			}
		}
		if (entity != null && entity.ItemData.IsLight)
		{
			Client.Game.GetScene<GameScene>().AddLight(owner, entity, mirror ? (x + reference.UV.Width) : x, y);
		}
	}

	private static Vector3 CalculateSitAnimation(int y, Item entity, bool isHuman, ref SpriteInfo spriteInfo)
	{
		Vector3 result = default(Vector3);
		if (entity == null && isHuman)
		{
			int num = spriteInfo.UV.Height;
			if (num == 0)
			{
				num = 61;
			}
			_characterFrameStartY = y - ((spriteInfo.Texture == null) ? (num - 4) : 0);
			_characterFrameHeight = num;
			_startCharacterWaistY = (int)((float)num * 0.35f) + _characterFrameStartY;
			_startCharacterKneesY = (int)((float)num * 0.6f) + _characterFrameStartY;
			_startCharacterFeetY = (int)((float)num * 0.94f) + _characterFrameStartY;
			if (spriteInfo.Texture == null)
			{
				return result;
			}
		}
		result.X = 0.35f;
		result.Y = 0.6f;
		result.Z = 0.94f;
		if (entity != null)
		{
			float num2 = y + spriteInfo.UV.Height;
			if (y >= _startCharacterWaistY)
			{
				result.X = 0f;
			}
			else if (num2 <= (float)_startCharacterWaistY)
			{
				result.X = 1f;
			}
			else
			{
				float num3 = _startCharacterWaistY - y;
				result.X = num3 / (float)spriteInfo.UV.Height;
				if (result.X < 0f)
				{
					result.X = 0f;
				}
			}
			if ((float)_startCharacterWaistY >= num2 || y >= _startCharacterKneesY)
			{
				result.Y = 0f;
			}
			else if (_startCharacterWaistY <= y && num2 <= (float)_startCharacterKneesY)
			{
				result.Y = 1f;
			}
			else
			{
				float num4 = ((y >= _startCharacterWaistY) ? ((float)(_startCharacterKneesY - y)) : ((!(num2 <= (float)_startCharacterKneesY)) ? ((float)(_startCharacterKneesY - _startCharacterWaistY)) : (num2 - (float)_startCharacterWaistY)));
				result.Y = result.X + num4 / (float)spriteInfo.UV.Height;
				if (result.Y < 0f)
				{
					result.Y = 0f;
				}
			}
			if (num2 <= (float)_startCharacterKneesY)
			{
				result.Z = 0f;
			}
			else if (y >= _startCharacterKneesY)
			{
				result.Z = 1f;
			}
			else
			{
				float num5 = num2 - (float)_startCharacterKneesY;
				result.Z = result.Y + num5 / (float)spriteInfo.UV.Height;
				if (result.Z < 0f)
				{
					result.Z = 0f;
				}
			}
		}
		return result;
	}

	public override bool CheckMouseSelection()
	{
		Point realScreenPosition = RealScreenPosition;
		realScreenPosition.Y -= 3;
		realScreenPosition.X += (int)base.CumulativeOffset.X + 22;
		realScreenPosition.Y += (int)(base.CumulativeOffset.Y - base.CumulativeOffset.Z) + 22;
		Rectangle frameInfo = FrameInfo;
		frameInfo.X = realScreenPosition.X - frameInfo.X;
		frameInfo.Y = realScreenPosition.Y - frameInfo.Y;
		bool isHuman = IsHuman;
		bool isGargoyle = Client.Version >= ClientVersion.CV_7000 && (Graphic == 666 || Graphic == 667 || Graphic == 695 || Graphic == 694);
		ProcessSteps(out var dir);
		bool mirror = IsFlipped;
		AnimationsLoader.Instance.GetAnimDirection(ref dir, ref mirror);
		ushort graphic = GetGraphicForAnimation();
		byte animGroup = GetGroupForAnimation(this, graphic, isParent: true);
		byte animIndex = AnimIndex;
		byte b = animGroup;
		byte b2 = animIndex;
		int drawX = 0;
		int drawY = 0;
		bool flag = IsSitting(ref drawX, ref drawY, ref mirror, ref animGroup, ref dir);
		int num = drawX - ((mirror && flag) ? 8 : 0);
		int num2 = drawY;
		frameInfo.X += num;
		frameInfo.Y += num2;
		frameInfo.Width += (flag ? 8 : 0);
		if (!frameInfo.Contains(SelectedObject.TranslatedMousePositionByViewport))
		{
			return false;
		}
		SpriteInfo spriteInfo;
		bool isUOP;
		if (isHuman)
		{
			Item item = FindItemByLayer(Layer.Mount);
			if (item != null)
			{
				ushort graphic2 = item.GetGraphicForAnimation();
				if (graphic2 != ushort.MaxValue)
				{
					byte animGroup2 = GetGroupForAnimation(this, graphic2);
					if (GetTexture(ref graphic2, ref animGroup2, ref animIndex, dir, out spriteInfo, out isUOP))
					{
						int num3 = realScreenPosition.X - (mirror ? (spriteInfo.UV.Width - spriteInfo.Center.X) : spriteInfo.Center.X);
						int num4 = realScreenPosition.Y - (spriteInfo.UV.Height + spriteInfo.Center.Y);
						if (AnimationsLoader.Instance.PixelCheck(graphic2, animGroup2, dir, isUOP, animIndex, mirror ? (num3 + spriteInfo.UV.Width - SelectedObject.TranslatedMousePositionByViewport.X) : (SelectedObject.TranslatedMousePositionByViewport.X - num3), SelectedObject.TranslatedMousePositionByViewport.Y - num4, flag))
						{
							return true;
						}
						realScreenPosition.Y += AnimationsLoader.Instance.DataIndex[graphic2].MountedHeightOffset;
					}
				}
			}
		}
		if (GetTexture(ref graphic, ref animGroup, ref animIndex, dir, out spriteInfo, out isUOP))
		{
			int num5 = realScreenPosition.X - (mirror ? (spriteInfo.UV.Width - spriteInfo.Center.X) : spriteInfo.Center.X);
			int num6 = realScreenPosition.Y - (spriteInfo.UV.Height + spriteInfo.Center.Y);
			num5 += drawX - ((mirror && flag) ? 8 : 0);
			num6 += drawY;
			if (AnimationsLoader.Instance.PixelCheck(graphic, animGroup, dir, isUOP, animIndex, mirror ? (num5 + spriteInfo.UV.Width - SelectedObject.TranslatedMousePositionByViewport.X) : (SelectedObject.TranslatedMousePositionByViewport.X - num5), SelectedObject.TranslatedMousePositionByViewport.Y - num6, flag))
			{
				return true;
			}
		}
		if (!base.IsEmpty && isHuman)
		{
			Layer layer = Layer.OneHanded;
			while ((int)layer < 25)
			{
				Item item2 = FindItemByLayer(layer);
				if (!(item2 == null) && (!IsDead || (layer != Layer.Hair && layer != Layer.Beard)) && !IsCovered(this, layer))
				{
					graphic = GetAnimationInfo(this, item2, isGargoyle);
					if (graphic != ushort.MaxValue)
					{
						animGroup = b;
						animIndex = b2;
						if (GetTexture(ref graphic, ref animGroup, ref animIndex, dir, out spriteInfo, out isUOP))
						{
							int num7 = realScreenPosition.X - (mirror ? (spriteInfo.UV.Width - spriteInfo.Center.X) : spriteInfo.Center.X);
							int num8 = realScreenPosition.Y - (spriteInfo.UV.Height + spriteInfo.Center.Y);
							num7 += drawX - ((mirror && flag) ? 8 : 0);
							num8 += drawY;
							if (AnimationsLoader.Instance.PixelCheck(graphic, animGroup, dir, isUOP, animIndex, mirror ? (num7 + spriteInfo.UV.Width - SelectedObject.TranslatedMousePositionByViewport.X) : (SelectedObject.TranslatedMousePositionByViewport.X - num7), SelectedObject.TranslatedMousePositionByViewport.Y - num8, flag))
							{
								return true;
							}
						}
					}
				}
				layer++;
			}
		}
		return false;
	}

	internal static bool IsCovered(Mobile mobile, Layer layer)
	{
		if (mobile.IsEmpty)
		{
			return false;
		}
		switch (layer)
		{
		case Layer.Shoes:
		{
			Item item2 = mobile.FindItemByLayer(Layer.Pants);
			if (mobile.FindItemByLayer(Layer.Legs) != null)
			{
				return true;
			}
			if (item2 != null && (AnimationsLoader.Instance.FindItemFlagsInDrawDefList(item2) & 0x8000000) != 0)
			{
				return true;
			}
			break;
		}
		case Layer.Pants:
			if (mobile.FindItemByLayer(Layer.Legs) != null)
			{
				return true;
			}
			break;
		case Layer.Tunic:
		{
			Item item3 = mobile.FindItemByLayer(Layer.Robe);
			Item item4 = mobile.FindItemByLayer(Layer.Tunic);
			if (item4 != null && item4.Graphic == 568)
			{
				if (item3 != null && item3.Graphic != 39301 && item3.Graphic != 39302)
				{
					return item3.Graphic != 42002;
				}
				return false;
			}
			break;
		}
		case Layer.Torso:
		{
			Item item3 = mobile.FindItemByLayer(Layer.Robe);
			if (item3 != null && item3.Graphic != 0 && item3.Graphic != 39301 && item3.Graphic != 39302 && item3.Graphic != 42002 && item3.Graphic != 41674)
			{
				return true;
			}
			Item item5 = mobile.FindItemByLayer(Layer.Torso);
			if (item5 != null && (item5.Graphic == 30762 || item5.Graphic == 30763))
			{
				return true;
			}
			break;
		}
		case Layer.Arms:
		{
			Item item3 = mobile.FindItemByLayer(Layer.Robe);
			if (item3 != null && item3.Graphic != 0 && item3.Graphic != 39301 && item3.Graphic != 39302)
			{
				return item3.Graphic != 42002;
			}
			return false;
		}
		case Layer.Helmet:
		{
			Item item3 = mobile.FindItemByLayer(Layer.Robe);
			if (item3 != null && (AnimationsLoader.Instance.FindItemFlagsInDrawDefList(item3) & 0x80000000u) != 0)
			{
				return true;
			}
			break;
		}
		case Layer.Hair:
		{
			Item item3 = mobile.FindItemByLayer(Layer.Robe);
			if (!(item3 != null))
			{
				break;
			}
			if (item3.Graphic > 12659)
			{
				if (item3.Graphic == 19357 || item3.Graphic == 30742)
				{
					return true;
				}
			}
			else
			{
				if (item3.Graphic > 9863)
				{
					break;
				}
				if (item3.Graphic < 9859)
				{
					if (item3.Graphic >= 8270)
					{
						return item3.Graphic <= 8271;
					}
					return false;
				}
				return true;
			}
			break;
		}
		case Layer.Beard:
		{
			Item item = mobile.FindItemByLayer(Layer.Helmet);
			if (item != null && (AnimationsLoader.Instance.FindItemFlagsInDrawDefList(item) & 0x40000000) != 0)
			{
				return true;
			}
			break;
		}
		}
		return false;
	}

	public void PutSittingAnimationIntoPicker(UltimaBatcher2D batcher, Item entity, SpriteInfo spriteInfo, ushort id, byte animGroup, bool mirror, byte dir, int frame)
	{
		mirror = false;
		ushort hue = 21;
		uint num = (uint)(animGroup | (dir << 8) | 0 | 0x20000);
		ulong textureID = (uint)(id | (frame << 16)) | ((ulong)num << 32);
		if (!AnimationsLoader.Instance.Picker.Has(textureID))
		{
			PresentationParameters presentationParameters = Client.Game.GraphicsDevice.PresentationParameters;
			int num2 = 0;
			Rectangle uV = spriteInfo.UV;
			RenderTarget2D renderTarget2D = new RenderTarget2D(Client.Game.GraphicsDevice, uV.Width + 8, uV.Height + num2, mipMap: false, presentationParameters.BackBufferFormat, presentationParameters.DepthStencilFormat, presentationParameters.MultiSampleCount, RenderTargetUsage.DiscardContents);
			batcher.SetBlendState(null);
			Viewport viewport = batcher.GraphicsDevice.Viewport;
			RenderTargetBinding[] renderTargets = batcher.GraphicsDevice.GetRenderTargets();
			batcher.GraphicsDevice.SetRenderTarget(renderTarget2D);
			Vector3 mod = CalculateSitAnimation(0, entity, IsHuman, ref spriteInfo);
			Vector3 hueVector = ShaderHueTranslator.GetHueVector(hue, partial: false, 1f);
			batcher.DrawCharacterSitted(spriteInfo.Texture, new Vector2(mirror ? 8 : 0, num2), uV, mod, hueVector, mirror, 5000f);
			batcher.SetBlendState(null);
			uint[] array = new uint[(uV.Width + 8) * (uV.Height + num2)];
			renderTarget2D.GetData(array);
			AnimationsLoader.Instance.Picker.Set(textureID, uV.Width + 8, uV.Height + num2, array);
			if (renderTargets.Length != 0)
			{
				batcher.GraphicsDevice.SetRenderTarget((RenderTarget2D)renderTargets[0].RenderTarget);
			}
			else
			{
				batcher.GraphicsDevice.SetRenderTarget(null);
			}
			batcher.GraphicsDevice.Viewport = viewport;
			renderTarget2D.Dispose();
		}
	}

	public bool IsSitting(ref int drawX, ref int drawY, ref bool isFlipped, ref byte animGroup, ref byte dir)
	{
		AnimationsLoader.SittingInfoData data = AnimationsLoader.SittingInfoData.Empty;
		if (TryGetSittingInfo(out data))
		{
			animGroup = 4;
			ProcessSteps(out dir);
			AnimationsLoader.Instance.FixSittingDirection(ref dir, ref isFlipped, ref drawX, ref drawY, ref data);
			drawY += 4;
			if (dir == 3)
			{
				if (IsGargoyle)
				{
					drawY -= 26;
					animGroup = 42;
				}
				else
				{
					animGroup = 25;
				}
			}
			else
			{
				if (!IsGargoyle)
				{
					return true;
				}
				animGroup = 42;
			}
		}
		return false;
	}
}

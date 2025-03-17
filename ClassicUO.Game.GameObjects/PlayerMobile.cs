using System;
using System.Collections.Generic;
using System.Linq;
using ClassicUO.Configuration;
using ClassicUO.Data;
using ClassicUO.Game.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.IO.Audio;
using ClassicUO.IO.Resources;
using ClassicUO.Network;
using ClassicUO.Utility.Collections;
using ClassicUO.Utility.Logging;

namespace ClassicUO.Game.GameObjects;

internal class PlayerMobile : Mobile
{
	private readonly Dictionary<ushort, BuffIcon> _buffIcons = new Dictionary<ushort, BuffIcon>();

	public Ability[] Abilities = new Ability[2]
	{
		Ability.Invalid,
		Ability.Invalid
	};

	public readonly HashSet<uint> AutoOpenedCorpses = new HashSet<uint>();

	public short ColdResistance;

	public short DamageIncrease;

	public short DamageMax;

	public short DamageMin;

	public long DeathScreenTimer;

	public short DefenseChanceIncrease;

	public Lock DexLock;

	public ushort Dexterity;

	public short DexterityIncrease;

	public short EnergyResistance;

	public short EnhancePotions;

	public short FasterCasting;

	public short FasterCastRecovery;

	public short FireResistance;

	public byte Followers;

	public byte FollowersMax;

	public uint Gold;

	public short HitChanceIncrease;

	public short HitPointsIncrease;

	public short HitPointsRegeneration;

	public ushort Intelligence;

	public short IntelligenceIncrease;

	public Lock IntLock;

	public short LowerManaCost;

	public short LowerReagentCost;

	public ushort Luck;

	public short ManaIncrease;

	public short ManaRegeneration;

	public readonly HashSet<uint> ManualOpenedCorpses = new HashSet<uint>();

	public short MaxColdResistence;

	public short MaxDefenseChanceIncrease;

	public short MaxEnergyResistence;

	public short MaxFireResistence;

	public short MaxHitPointsIncrease;

	public short MaxManaIncrease;

	public short MaxPhysicResistence;

	public short MaxPoisonResistence;

	public short MaxStaminaIncrease;

	public short PhysicalResistance;

	public int KrankheitsResistance;

	public short KlingenResistance;

	public short StumpfResistance;

	public short SpitzResistance;

	public short PhysResiOverall;

	public int RPPBonusPunkte;

	public short PoisonResistance;

	public short ReflectPhysicalDamage;

	public short SpellDamageIncrease;

	public short StaminaIncrease;

	public short StaminaRegeneration;

	public short StatsCap;

	public ushort Strength;

	public short StrengthIncrease;

	public Lock StrLock;

	public short SwingSpeedIncrease;

	public uint TithingPoints;

	public ushort Weight;

	public ushort WeightMax;

	public OrderedDictionary<byte, DateTime> CastRunBlockUntil = new OrderedDictionary<byte, DateTime>();

	private uint _lastSound;

	public Skill[] Skills { get; }

	public override bool InWarMode { get; set; }

	public IReadOnlyDictionary<ushort, BuffIcon> BuffIcons => _buffIcons;

	public Ability PrimaryAbility
	{
		get
		{
			return Abilities[0];
		}
		set
		{
			Abilities[0] = value;
		}
	}

	public Ability SecondaryAbility
	{
		get
		{
			return Abilities[1];
		}
		set
		{
			Abilities[1] = value;
		}
	}

	public override bool IsWalking => LastStepTime > Time.Ticks - 150;

	internal WalkerManager Walker { get; } = new WalkerManager();

	private uint LastChatInputTime { get; set; }

	public bool ActiveCastBlock
	{
		get
		{
			foreach (byte item in CastRunBlockUntil.Keys.Where((byte k) => CastRunBlockUntil[k] < DateTime.Now))
			{
				CastRunBlockUntil.Remove(item);
			}
			return CastRunBlockUntil.Count > 0;
		}
	}

	public PlayerMobile(uint serial)
		: base(serial)
	{
		Skills = new Skill[SkillsLoader.Instance.SkillsCount];
		for (int i = 0; i < Skills.Length; i++)
		{
			SkillEntry skillEntry = SkillsLoader.Instance.Skills[i];
			Skills[i] = new Skill(skillEntry.Name, skillEntry.Index, skillEntry.HasAction);
		}
	}

	public Item FindBandage()
	{
		Item item = FindItemByLayer(Layer.Backpack);
		Item result = null;
		if (item != null)
		{
			result = item.FindItem(3617);
		}
		return result;
	}

	public Item FindItemByGraphic(ushort graphic)
	{
		Item item = FindItemByLayer(Layer.Backpack);
		if (item != null)
		{
			return FindItemInContainerRecursive(item, graphic);
		}
		return null;
	}

	private Item FindItemInContainerRecursive(Item container, ushort graphic)
	{
		Item item = null;
		if (container != null)
		{
			for (LinkedObject linkedObject = container.Items; linkedObject != null; linkedObject = linkedObject.Next)
			{
				Item item2 = (Item)linkedObject;
				if (item2.Graphic == graphic)
				{
					return item2;
				}
				if (!item2.IsEmpty)
				{
					item = FindItemInContainerRecursive(item2, graphic);
					if (item != null && item.Graphic == graphic)
					{
						return item;
					}
				}
			}
		}
		return item;
	}

	public void AddBuff(ushort type, ushort graphic, ushort hue, uint time, string text)
	{
		_buffIcons[type] = new BuffIcon(type, graphic, hue, time, text);
	}

	public bool IsBuffIconExists(ushort graphic)
	{
		return _buffIcons.ContainsKey(graphic);
	}

	public void RemoveBuff(ushort graphic)
	{
		_buffIcons.Remove(graphic);
	}

	public void UpdateAbilities()
	{
		ushort num = 0;
		Item item = FindItemByLayer(Layer.OneHanded);
		if (item != null)
		{
			num = item.Graphic;
		}
		else
		{
			item = FindItemByLayer(Layer.TwoHanded);
			if (item != null)
			{
				num = item.Graphic;
			}
		}
		Abilities[0] = Ability.Invalid;
		Abilities[1] = Ability.Invalid;
		if (num != 0)
		{
			ushort num2 = num;
			ushort num3 = 0;
			if (item != null)
			{
				ushort animID = item.ItemData.AnimID;
				int num4 = 1;
				ushort num5 = (ushort)(num - 1);
				if (TileDataLoader.Instance.StaticData[num5].AnimID == animID)
				{
					num3 = num5;
					num4 = 2;
				}
				else
				{
					num5 = (ushort)(num + 1);
					if (TileDataLoader.Instance.StaticData[num5].AnimID == animID)
					{
						num3 = num5;
						num4 = 2;
					}
				}
				for (int i = 0; i < num4; i++)
				{
					switch ((i == 0) ? num2 : num3)
					{
					case 2305:
						Abilities[0] = Ability.MovingShot;
						Abilities[1] = Ability.InfusedThrow;
						break;
					case 2306:
						Abilities[0] = Ability.InfectiousStrike;
						Abilities[1] = Ability.ShadowStrike;
						break;
					case 2309:
						Abilities[0] = Ability.DoubleStrike;
						Abilities[1] = Ability.MortalStrike;
						break;
					case 2310:
						Abilities[0] = Ability.CrushingBlow;
						Abilities[1] = Ability.Dismount;
						break;
					case 2316:
						Abilities[0] = Ability.BleedAttack;
						Abilities[1] = Ability.MortalStrike;
						break;
					case 3568:
					case 3569:
						Abilities[0] = Ability.WhirlwindAttack;
						Abilities[1] = Ability.ParalyzingBlow;
						break;
					case 3570:
					case 3571:
					case 3572:
					case 3573:
						Abilities[0] = Ability.Dismount;
						Abilities[1] = Ability.Disarm;
						break;
					case 3713:
					case 3714:
						Abilities[0] = Ability.CrushingBlow;
						Abilities[1] = Ability.Disarm;
						break;
					case 3717:
					case 3718:
						Abilities[0] = Ability.DoubleStrike;
						Abilities[1] = Ability.Disarm;
						break;
					case 3719:
					case 3720:
						Abilities[0] = Ability.BleedAttack;
						Abilities[1] = Ability.Dismount;
						break;
					case 3721:
					case 3722:
						Abilities[0] = Ability.DoubleStrike;
						Abilities[1] = Ability.ConcussionBlow;
						break;
					case 3778:
					case 3779:
						Abilities[0] = Ability.BleedAttack;
						Abilities[1] = Ability.InfectiousStrike;
						break;
					case 3780:
					case 3781:
						Abilities[0] = Ability.ShadowStrike;
						Abilities[1] = Ability.BleedAttack;
						break;
					case 3907:
					case 3908:
						Abilities[0] = Ability.ArmorIgnore;
						Abilities[1] = Ability.Disarm;
						break;
					case 3909:
					case 3910:
						Abilities[0] = Ability.BleedAttack;
						Abilities[1] = Ability.MortalStrike;
						break;
					case 3911:
					case 3912:
						Abilities[0] = Ability.BleedAttack;
						Abilities[1] = Ability.ConcussionBlow;
						break;
					case 3913:
					case 3914:
						Abilities[0] = Ability.CrushingBlow;
						Abilities[1] = Ability.Dismount;
						break;
					case 3915:
					case 3916:
						Abilities[0] = Ability.DoubleStrike;
						Abilities[1] = Ability.WhirlwindAttack;
						break;
					case 3917:
					case 3918:
						Abilities[0] = Ability.ParalyzingBlow;
						Abilities[1] = Ability.Dismount;
						break;
					case 3919:
					case 3920:
						Abilities[0] = Ability.ConcussionBlow;
						Abilities[1] = Ability.MortalStrike;
						break;
					case 3921:
					case 3922:
						Abilities[0] = Ability.InfectiousStrike;
						Abilities[1] = Ability.ShadowStrike;
						break;
					case 3932:
					case 3933:
						Abilities[0] = Ability.ConcussionBlow;
						Abilities[1] = Ability.Disarm;
						break;
					case 3934:
					case 3935:
						Abilities[0] = Ability.CrushingBlow;
						Abilities[1] = Ability.ArmorIgnore;
						break;
					case 3936:
					case 3937:
						Abilities[0] = Ability.ArmorIgnore;
						Abilities[1] = Ability.ConcussionBlow;
						break;
					case 3938:
					case 3939:
						Abilities[0] = Ability.ArmorIgnore;
						Abilities[1] = Ability.ParalyzingBlow;
						break;
					case 4021:
						Abilities[0] = Ability.CrushingBlow;
						Abilities[1] = Ability.ShadowStrike;
						break;
					case 5039:
					case 5040:
						Abilities[0] = Ability.ArmorIgnore;
						Abilities[1] = Ability.BleedAttack;
						break;
					case 5041:
					case 5042:
						Abilities[0] = Ability.ParalyzingBlow;
						Abilities[1] = Ability.MortalStrike;
						break;
					case 5043:
					case 5044:
						Abilities[0] = Ability.ShadowStrike;
						Abilities[1] = Ability.Dismount;
						break;
					case 5046:
					case 5047:
					case 5048:
						Abilities[0] = Ability.DoubleStrike;
						Abilities[1] = Ability.ParalyzingBlow;
						break;
					case 5049:
					case 5050:
						Abilities[0] = Ability.ParalyzingBlow;
						Abilities[1] = Ability.CrushingBlow;
						break;
					case 5117:
						Abilities[0] = Ability.MovingShot;
						Abilities[1] = Ability.Dismount;
						break;
					case 5091:
						Abilities[0] = Ability.CrushingBlow;
						Abilities[1] = Ability.ShadowStrike;
						break;
					case 5110:
						Abilities[0] = Ability.InfectiousStrike;
						Abilities[1] = Ability.Disarm;
						break;
					case 5112:
						Abilities[0] = Ability.ConcussionBlow;
						Abilities[1] = Ability.ForceOfNature;
						break;
					case 5115:
						Abilities[0] = Ability.WhirlwindAttack;
						Abilities[1] = Ability.BleedAttack;
						break;
					case 5119:
						Abilities[0] = Ability.DoubleStrike;
						Abilities[1] = Ability.ArmorIgnore;
						break;
					case 5121:
						Abilities[0] = Ability.ArmorIgnore;
						Abilities[1] = Ability.InfectiousStrike;
						break;
					case 5122:
					case 5123:
						Abilities[0] = Ability.ShadowStrike;
						Abilities[1] = Ability.MortalStrike;
						break;
					case 5124:
					case 5125:
						Abilities[0] = Ability.BleedAttack;
						Abilities[1] = Ability.Disarm;
						break;
					case 5126:
					case 5127:
						Abilities[0] = Ability.CrushingBlow;
						Abilities[1] = Ability.MortalStrike;
						break;
					case 5176:
					case 5177:
						Abilities[0] = Ability.WhirlwindAttack;
						Abilities[1] = Ability.CrushingBlow;
						break;
					case 5178:
					case 5179:
						Abilities[0] = Ability.CrushingBlow;
						Abilities[1] = Ability.ConcussionBlow;
						break;
					case 5180:
					case 5181:
						Abilities[0] = Ability.ArmorIgnore;
						Abilities[1] = Ability.MortalStrike;
						break;
					case 5182:
					case 5183:
						Abilities[0] = Ability.WhirlwindAttack;
						Abilities[1] = Ability.ConcussionBlow;
						break;
					case 5184:
					case 5185:
						Abilities[0] = Ability.BleedAttack;
						Abilities[1] = Ability.ShadowStrike;
						break;
					case 5186:
					case 5187:
						Abilities[0] = Ability.DoubleStrike;
						Abilities[1] = Ability.ShadowStrike;
						break;
					case 9914:
						Abilities[0] = Ability.BleedAttack;
						Abilities[1] = Ability.ParalyzingBlow;
						break;
					case 9915:
						Abilities[0] = Ability.ParalyzingBlow;
						Abilities[1] = Ability.MortalStrike;
						break;
					case 9916:
						Abilities[0] = Ability.CrushingBlow;
						Abilities[1] = Ability.MortalStrike;
						break;
					case 9917:
						Abilities[0] = Ability.ArmorIgnore;
						Abilities[1] = Ability.Dismount;
						break;
					case 9918:
						Abilities[0] = Ability.ParalyzingBlow;
						Abilities[1] = Ability.InfectiousStrike;
						break;
					case 9919:
						Abilities[0] = Ability.DoubleStrike;
						Abilities[1] = Ability.InfectiousStrike;
						break;
					case 9920:
						Abilities[0] = Ability.Dismount;
						Abilities[1] = Ability.ConcussionBlow;
						break;
					case 9921:
						Abilities[0] = Ability.DoubleStrike;
						Abilities[1] = Ability.MortalStrike;
						break;
					case 9922:
						Abilities[0] = Ability.ArmorIgnore;
						Abilities[1] = Ability.MovingShot;
						break;
					case 9923:
						Abilities[0] = Ability.DoubleStrike;
						Abilities[1] = Ability.MovingShot;
						break;
					case 9924:
						Abilities[0] = Ability.BleedAttack;
						Abilities[1] = Ability.ParalyzingBlow;
						break;
					case 9925:
						Abilities[0] = Ability.ParalyzingBlow;
						Abilities[1] = Ability.MortalStrike;
						break;
					case 9926:
						Abilities[0] = Ability.CrushingBlow;
						Abilities[1] = Ability.MortalStrike;
						break;
					case 9927:
						Abilities[0] = Ability.ArmorIgnore;
						Abilities[1] = Ability.Dismount;
						break;
					case 9928:
						Abilities[0] = Ability.ParalyzingBlow;
						Abilities[1] = Ability.InfectiousStrike;
						break;
					case 9929:
						Abilities[0] = Ability.DoubleStrike;
						Abilities[1] = Ability.InfectiousStrike;
						break;
					case 9930:
						Abilities[0] = Ability.Dismount;
						Abilities[1] = Ability.ConcussionBlow;
						break;
					case 9931:
						Abilities[0] = Ability.DoubleStrike;
						Abilities[1] = Ability.MortalStrike;
						break;
					case 9932:
						Abilities[0] = Ability.ArmorIgnore;
						Abilities[1] = Ability.MovingShot;
						break;
					case 9933:
						Abilities[0] = Ability.DoubleStrike;
						Abilities[1] = Ability.MovingShot;
						break;
					case 9934:
					case 9935:
						Abilities[0] = Ability.WhirlwindAttack;
						Abilities[1] = Ability.Disarm;
						break;
					case 10146:
						Abilities[0] = Ability.CrushingBlow;
						Abilities[1] = Ability.RidingSwipe;
						break;
					case 10147:
						Abilities[0] = Ability.Feint;
						Abilities[1] = Ability.Block;
						break;
					case 10148:
						Abilities[0] = Ability.FrenziedWhirlwind;
						Abilities[1] = Ability.DoubleStrike;
						break;
					case 10149:
						Abilities[0] = Ability.ArmorPierce;
						Abilities[1] = Ability.DoubleShot;
						break;
					case 10150:
						Abilities[0] = Ability.FrenziedWhirlwind;
						Abilities[1] = Ability.CrushingBlow;
						break;
					case 10151:
						Abilities[0] = Ability.DefenseMastery;
						Abilities[1] = Ability.FrenziedWhirlwind;
						break;
					case 10152:
						Abilities[0] = Ability.Feint;
						Abilities[1] = Ability.NerveStrike;
						break;
					case 10153:
						Abilities[0] = Ability.Feint;
						Abilities[1] = Ability.DoubleStrike;
						break;
					case 10154:
						Abilities[0] = Ability.Disarm;
						Abilities[1] = Ability.ParalyzingBlow;
						break;
					case 10155:
						Abilities[0] = Ability.DualWield;
						Abilities[1] = Ability.TalonStrike;
						break;
					case 10157:
						Abilities[0] = Ability.WhirlwindAttack;
						Abilities[1] = Ability.DefenseMastery;
						break;
					case 10158:
						Abilities[0] = Ability.Block;
						Abilities[1] = Ability.Feint;
						break;
					case 10159:
						Abilities[0] = Ability.Block;
						Abilities[1] = Ability.ArmorPierce;
						break;
					case 10221:
						Abilities[0] = Ability.CrushingBlow;
						Abilities[1] = Ability.RidingSwipe;
						break;
					case 10222:
						Abilities[0] = Ability.Feint;
						Abilities[1] = Ability.Block;
						break;
					case 10223:
						Abilities[0] = Ability.FrenziedWhirlwind;
						Abilities[1] = Ability.DoubleStrike;
						break;
					case 10224:
						Abilities[0] = Ability.ArmorPierce;
						Abilities[1] = Ability.DoubleShot;
						break;
					case 10225:
						Abilities[0] = Ability.FrenziedWhirlwind;
						Abilities[1] = Ability.CrushingBlow;
						break;
					case 10226:
						Abilities[0] = Ability.DefenseMastery;
						Abilities[1] = Ability.FrenziedWhirlwind;
						break;
					case 10227:
						Abilities[0] = Ability.Feint;
						Abilities[1] = Ability.NerveStrike;
						break;
					case 10228:
						Abilities[0] = Ability.Feint;
						Abilities[1] = Ability.DoubleStrike;
						break;
					case 10229:
						Abilities[0] = Ability.Disarm;
						Abilities[1] = Ability.ParalyzingBlow;
						break;
					case 10230:
						Abilities[0] = Ability.DualWield;
						Abilities[1] = Ability.TalonStrike;
						break;
					case 10232:
						Abilities[0] = Ability.WhirlwindAttack;
						Abilities[1] = Ability.DefenseMastery;
						break;
					case 10233:
						Abilities[0] = Ability.Block;
						Abilities[1] = Ability.Feint;
						break;
					case 10234:
						Abilities[0] = Ability.Block;
						Abilities[1] = Ability.ArmorPierce;
						break;
					case 11550:
						Abilities[0] = Ability.ForceArrow;
						Abilities[1] = Ability.SerpentArrow;
						break;
					case 11551:
						Abilities[0] = Ability.LightningArrow;
						Abilities[1] = Ability.PsychicAttack;
						break;
					case 11552:
						Abilities[0] = Ability.PsychicAttack;
						Abilities[1] = Ability.BleedAttack;
						break;
					case 11553:
						Abilities[0] = Ability.InfectiousStrike;
						Abilities[1] = Ability.ShadowStrike;
						break;
					case 11554:
						Abilities[0] = Ability.Feint;
						Abilities[1] = Ability.ArmorIgnore;
						break;
					case 11555:
						Abilities[0] = Ability.Disarm;
						Abilities[1] = Ability.Bladeweave;
						break;
					case 11556:
						Abilities[0] = Ability.ConcussionBlow;
						Abilities[1] = Ability.CrushingBlow;
						break;
					case 11557:
						Abilities[0] = Ability.Block;
						Abilities[1] = Ability.ForceOfNature;
						break;
					case 11558:
						Abilities[0] = Ability.Disarm;
						Abilities[1] = Ability.Bladeweave;
						break;
					case 11559:
						Abilities[0] = Ability.WhirlwindAttack;
						Abilities[1] = Ability.Bladeweave;
						break;
					case 11560:
						Abilities[0] = Ability.Disarm;
						Abilities[1] = Ability.CrushingBlow;
						break;
					case 11561:
						Abilities[0] = Ability.DefenseMastery;
						Abilities[1] = Ability.Bladeweave;
						break;
					case 11562:
						Abilities[0] = Ability.ForceArrow;
						Abilities[1] = Ability.SerpentArrow;
						break;
					case 11563:
						Abilities[0] = Ability.LightningArrow;
						Abilities[1] = Ability.PsychicAttack;
						break;
					case 11564:
						Abilities[0] = Ability.PsychicAttack;
						Abilities[1] = Ability.BleedAttack;
						break;
					case 11565:
						Abilities[0] = Ability.InfectiousStrike;
						Abilities[1] = Ability.ShadowStrike;
						break;
					case 11566:
						Abilities[0] = Ability.Feint;
						Abilities[1] = Ability.ArmorIgnore;
						break;
					case 11567:
						Abilities[0] = Ability.Disarm;
						Abilities[1] = Ability.Bladeweave;
						break;
					case 11568:
						Abilities[0] = Ability.ConcussionBlow;
						Abilities[1] = Ability.CrushingBlow;
						break;
					case 11569:
						Abilities[0] = Ability.Block;
						Abilities[1] = Ability.ForceOfNature;
						break;
					case 11570:
						Abilities[0] = Ability.Disarm;
						Abilities[1] = Ability.Bladeweave;
						break;
					case 11571:
						Abilities[0] = Ability.WhirlwindAttack;
						Abilities[1] = Ability.Bladeweave;
						break;
					case 11572:
						Abilities[0] = Ability.Disarm;
						Abilities[1] = Ability.CrushingBlow;
						break;
					case 11573:
						Abilities[0] = Ability.DefenseMastery;
						Abilities[1] = Ability.Bladeweave;
						break;
					case 16487:
						Abilities[0] = Ability.MysticArc;
						Abilities[1] = Ability.ConcussionBlow;
						break;
					case 2301:
					case 16488:
						Abilities[0] = Ability.DoubleStrike;
						Abilities[1] = Ability.InfectiousStrike;
						break;
					case 16491:
						Abilities[0] = Ability.ArmorIgnore;
						Abilities[1] = Ability.MortalStrike;
						break;
					case 16492:
						Abilities[0] = Ability.MovingShot;
						Abilities[1] = Ability.InfusedThrow;
						break;
					case 2308:
					case 16493:
						Abilities[0] = Ability.DoubleStrike;
						Abilities[1] = Ability.Disarm;
						break;
					case 2307:
					case 16494:
						Abilities[0] = Ability.ArmorIgnore;
						Abilities[1] = Ability.Disarm;
						break;
					case 2302:
					case 16498:
						Abilities[0] = Ability.BleedAttack;
						Abilities[1] = Ability.ParalyzingBlow;
						break;
					case 2315:
					case 16500:
						Abilities[0] = Ability.CrushingBlow;
						Abilities[1] = Ability.ConcussionBlow;
						break;
					case 2312:
					case 16501:
						Abilities[0] = Ability.WhirlwindAttack;
						Abilities[1] = Ability.Dismount;
						break;
					case 16502:
						Abilities[0] = Ability.ArmorIgnore;
						Abilities[1] = Ability.MortalStrike;
						break;
					case 18606:
						Abilities[0] = Ability.BleedAttack;
						Abilities[1] = Ability.InfectiousStrike;
						break;
					case 18608:
						Abilities[0] = Ability.BleedAttack;
						Abilities[1] = Ability.ConcussionBlow;
						break;
					case 18610:
					case 18611:
						Abilities[0] = Ability.CrushingBlow;
						Abilities[1] = Ability.Dismount;
						break;
					case 18612:
					case 18613:
						Abilities[0] = Ability.ParalyzingBlow;
						Abilities[1] = Ability.Dismount;
						break;
					case 18614:
					case 18615:
						Abilities[0] = Ability.InfectiousStrike;
						Abilities[1] = Ability.Disarm;
						break;
					case 18616:
					case 18617:
						Abilities[0] = Ability.ConcussionBlow;
						Abilities[1] = Ability.ParalyzingBlow;
						break;
					case 18618:
					case 18619:
						Abilities[0] = Ability.DoubleStrike;
						Abilities[1] = Ability.ArmorIgnore;
						break;
					case 18620:
					case 18621:
						Abilities[0] = Ability.ArmorIgnore;
						Abilities[1] = Ability.InfectiousStrike;
						break;
					case 18622:
					case 18623:
						Abilities[0] = Ability.BleedAttack;
						Abilities[1] = Ability.Disarm;
						break;
					case 18634:
					case 18635:
						Abilities[0] = Ability.Dismount;
						Abilities[1] = Ability.ConcussionBlow;
						break;
					case 1153:
					case 18624:
						Abilities[0] = Ability.WhirlwindAttack;
						Abilities[1] = Ability.CrushingBlow;
						break;
					case 18626:
					case 18627:
						Abilities[0] = Ability.DoubleStrike;
						Abilities[1] = Ability.ConcussionBlow;
						break;
					case 18628:
					case 18629:
						Abilities[0] = Ability.BleedAttack;
						Abilities[1] = Ability.ParalyzingBlow;
						break;
					case 18630:
					case 18631:
						Abilities[0] = Ability.ParalyzingBlow;
						Abilities[1] = Ability.MortalStrike;
						break;
					case 18632:
					case 18633:
						Abilities[0] = Ability.ParalyzingBlow;
						Abilities[1] = Ability.InfectiousStrike;
						break;
					case 18636:
					case 18637:
						Abilities[0] = Ability.Feint;
						Abilities[1] = Ability.Block;
						break;
					case 18638:
					case 18639:
						Abilities[0] = Ability.DualWield;
						Abilities[1] = Ability.TalonStrike;
						break;
					case 18640:
					case 18641:
						Abilities[0] = Ability.Feint;
						Abilities[1] = Ability.DoubleStrike;
						break;
					case 41609:
						Abilities[0] = Ability.ConcussionBlow;
						Abilities[1] = Ability.WhirlwindAttack;
						break;
					case 41610:
						Abilities[0] = Ability.ArmorPierce;
						Abilities[1] = Ability.WhirlwindAttack;
						break;
					case 41611:
						Abilities[0] = Ability.BleedAttack;
						Abilities[1] = Ability.WhirlwindAttack;
						break;
					case 2303:
						Abilities[0] = Ability.MysticArc;
						Abilities[1] = Ability.ConcussionBlow;
						continue;
					case 2304:
						Abilities[0] = Ability.ArmorIgnore;
						Abilities[1] = Ability.ParalyzingBlow;
						continue;
					case 2314:
						Abilities[0] = Ability.ArmorIgnore;
						Abilities[1] = Ability.MortalStrike;
						continue;
					default:
						continue;
					}
					break;
				}
			}
		}
		if (Abilities[0] == Ability.Invalid)
		{
			Abilities[0] = Ability.Disarm;
			Abilities[1] = Ability.ParalyzingBlow;
		}
		int num6 = 0;
		foreach (Gump gump in UIManager.Gumps)
		{
			if (gump is UseAbilityButtonGump useAbilityButtonGump)
			{
				useAbilityButtonGump.RequestUpdateContents();
				num6++;
			}
			if (num6 >= 2)
			{
				break;
			}
		}
	}

	protected override void OnPositionChanged()
	{
		base.OnPositionChanged();
		Plugin.UpdatePlayerPosition(X, Y, Z);
		TryOpenDoors();
		TryOpenCorpses();
	}

	public void TryOpenCorpses()
	{
		if (!ProfileManager.CurrentProfile.AutoOpenCorpses || ((ProfileManager.CurrentProfile.CorpseOpenOptions == 1 || ProfileManager.CurrentProfile.CorpseOpenOptions == 3) && TargetManager.IsTargeting) || ((ProfileManager.CurrentProfile.CorpseOpenOptions == 2 || ProfileManager.CurrentProfile.CorpseOpenOptions == 3) && base.IsHidden))
		{
			return;
		}
		foreach (Item value in World.Items.Values)
		{
			if (!value.IsDestroyed && value.IsCorpse && value.Distance <= ProfileManager.CurrentProfile.AutoOpenCorpseRange && !AutoOpenedCorpses.Contains(value.Serial))
			{
				AutoOpenedCorpses.Add(value.Serial);
				if (value.IsPlayerItem == 1)
				{
					break;
				}
				GameActions.DoubleClickQueued(value.Serial);
			}
		}
	}

	protected override void OnDirectionChanged()
	{
		base.OnDirectionChanged();
		TryOpenDoors();
	}

	private void TryOpenDoors()
	{
		if (!World.Player.IsDead && ProfileManager.CurrentProfile.AutoOpenDoors)
		{
			int x = X;
			int y = Y;
			int z = Z;
			Pathfinder.GetNewXY((byte)base.Direction, ref x, ref y);
			if (World.Items.Values.Any((Item s) => s.ItemData.IsDoor && s.X == x && s.Y == y && s.Z - 15 <= z && s.Z + 15 >= z))
			{
				GameActions.OpenDoor();
			}
		}
	}

	public override void Destroy()
	{
		if (!base.IsDestroyed)
		{
			DeathScreenTimer = 0L;
			Log.Warn("PlayerMobile disposed!");
			base.Destroy();
		}
	}

	public void CloseBank()
	{
		Item item = FindItemByLayer(Layer.Bank);
		if (!(item != null) || !item.Opened)
		{
			return;
		}
		if (!item.IsEmpty)
		{
			Item item2 = (Item)item.Items;
			while (item2 != null)
			{
				Item item3 = (Item)item2.Next;
				World.RemoveItem(item2, forceRemove: true);
				item2 = item3;
			}
			item.Items = null;
		}
		UIManager.GetGump<ContainerGump>(item.Serial)?.Dispose();
		item.Opened = false;
	}

	public void CloseRangedGumps()
	{
		foreach (Gump gump in UIManager.Gumps)
		{
			if (!(gump is PaperDollGump) && !(gump is MapGump) && !(gump is SpellbookGump))
			{
				Entity entity;
				int num;
				if (!(gump is TradingGump) && !(gump is ShopGump))
				{
					if (!(gump is ContainerGump))
					{
						continue;
					}
					num = int.MaxValue;
					entity = World.Get(gump.LocalSerial);
					if (entity != null)
					{
						if (SerialHelper.IsItem(entity.Serial))
						{
							Entity entity2 = World.Get(((Item)entity).RootContainer);
							if (entity2 != null)
							{
								num = entity2.Distance;
							}
						}
						else
						{
							num = entity.Distance;
						}
					}
					if (num > 3)
					{
						gump.Dispose();
					}
					continue;
				}
				entity = World.Get(gump.LocalSerial);
				num = int.MaxValue;
				if (entity != null)
				{
					if (SerialHelper.IsItem(entity.Serial))
					{
						Entity entity3 = World.Get(((Item)entity).RootContainer);
						if (entity3 != null)
						{
							num = entity3.Distance;
						}
					}
					else
					{
						num = entity.Distance;
					}
				}
				if (num > 5)
				{
					gump.Dispose();
				}
			}
			else if (World.Get(gump.LocalSerial) == null)
			{
				gump.Dispose();
			}
		}
	}

	public override void Update(double totalTime, double frameTime)
	{
		Heartbeat();
		base.Update(totalTime, frameTime);
	}

	public bool Walk(Direction direction, bool run)
	{
		if (Walker.WalkingFailed || Walker.LastStepRequestTime > Time.Ticks || Walker.StepsCount >= 5 || (Client.Version >= ClientVersion.CV_60142 && base.IsParalyzed))
		{
			return false;
		}
		run |= ProfileManager.CurrentProfile.AlwaysRun;
		if (SpeedMode >= CharacterSpeedType.CantRun || (Stamina <= 1 && !base.IsDead) || (base.IsHidden && ProfileManager.CurrentProfile.AlwaysRunUnlessHidden) || ActiveCastBlock || ProfileManager.CurrentProfile.AlwaysWalk)
		{
			run = false;
		}
		int num = X;
		int num2 = Y;
		sbyte b = Z;
		Direction direction2 = base.Direction;
		bool flag = base.Steps.Count == 0;
		if (!flag)
		{
			ref Step reference = ref base.Steps.Back();
			num = reference.X;
			num2 = reference.Y;
			b = reference.Z;
			direction2 = (Direction)reference.Direction;
		}
		sbyte b2 = b;
		ushort num3 = 100;
		if ((direction2 & Direction.Up) == (direction & Direction.Up))
		{
			Direction direction3 = direction;
			int x = num;
			int y = num2;
			sbyte z = b;
			if (!Pathfinder.CanWalk(ref direction3, ref x, ref y, ref z))
			{
				return false;
			}
			if ((direction & Direction.Up) != direction3)
			{
				direction = direction3;
			}
			else
			{
				direction = direction3;
				num = x;
				num2 = y;
				b = z;
				num3 = (ushort)MovementSpeed.TimeToCompleteMovement(run, base.IsMounted || SpeedMode == CharacterSpeedType.FastUnmount || SpeedMode == CharacterSpeedType.FastUnmountAndCantRun || base.IsFlying);
			}
		}
		else
		{
			Direction direction4 = direction;
			int x2 = num;
			int y2 = num2;
			sbyte z2 = b;
			if (!Pathfinder.CanWalk(ref direction4, ref x2, ref y2, ref z2) && (direction2 & Direction.Up) == direction4)
			{
				return false;
			}
			if ((direction2 & Direction.Up) == direction4)
			{
				num = x2;
				num2 = y2;
				b = z2;
				num3 = (ushort)MovementSpeed.TimeToCompleteMovement(run, base.IsMounted || SpeedMode == CharacterSpeedType.FastUnmount || SpeedMode == CharacterSpeedType.FastUnmountAndCantRun || base.IsFlying);
			}
			direction = direction4;
		}
		CloseBank();
		if (flag)
		{
			if (!IsWalking)
			{
				SetAnimation(byte.MaxValue, 0, 0, 0);
			}
			LastStepTime = Time.Ticks;
		}
		ref StepInfo reference2 = ref Walker.StepInfos[Walker.StepsCount];
		reference2.Sequence = Walker.WalkSequence;
		reference2.Accepted = false;
		reference2.Running = run;
		reference2.OldDirection = (byte)(direction2 & Direction.Up);
		reference2.Direction = (byte)direction;
		reference2.Timer = Time.Ticks;
		reference2.X = (ushort)num;
		reference2.Y = (ushort)num2;
		reference2.Z = b;
		reference2.NoRotation = (uint)reference2.OldDirection == (uint)direction && b2 - b >= 11;
		Walker.StepsCount++;
		base.Steps.AddToBack(new Step
		{
			X = num,
			Y = num2,
			Z = b,
			Direction = (byte)direction,
			Run = run
		});
		NetClient.Socket.Send_WalkRequest(direction, Walker.WalkSequence, run, Walker.FastWalkStack.GetValue());
		if (Walker.WalkSequence == byte.MaxValue)
		{
			Walker.WalkSequence = 1;
		}
		else
		{
			Walker.WalkSequence++;
		}
		Walker.UnacceptedPacketsCount++;
		AddToTile();
		int num4 = 0;
		Walker.LastStepRequestTime = Time.Ticks + num3 - num4;
		Mobile.GetGroupForAnimation(this, 0, isParent: true);
		return true;
	}

	private void Heartbeat()
	{
		if (ProfileManager.CurrentProfile == null || !ProfileManager.CurrentProfile.HeartbeatSound || ProfileManager.CurrentProfile.HeartbeatThreshold == 0 || !(World.Player != null) || ProfileManager.CurrentProfile == null || World.Player.HitsMax <= 0 || World.Player.IsDead)
		{
			return;
		}
		uint ticks = Time.Ticks;
		int num = World.Player.Hits * 100 / World.Player.HitsMax * 50 / ProfileManager.CurrentProfile.HeartbeatThreshold;
		int num2 = 0;
		if (num < 25)
		{
			num2 = 1668;
		}
		else if (num < 35)
		{
			num2 = 1667;
		}
		else if (num < 50)
		{
			num2 = 1666;
		}
		if (num2 != 0)
		{
			int num3 = 600 + num * 15;
			if (_lastSound + num3 <= ticks)
			{
				_lastSound = ticks;
				float num4 = Sound.ScaleVolume(ProfileManager.CurrentProfile.HeartbeatVolume);
				num4 *= 10f;
				SoundsLoader.Instance.GetSound(num2).Play(num4);
			}
		}
		else
		{
			_lastSound = ticks;
		}
	}

	public void UpdateLastChatInput()
	{
		if (Time.Ticks - LastChatInputTime >= 3000)
		{
			NetClient.Socket?.Send_TextActivity();
			LastChatInputTime = Time.Ticks;
		}
	}
}

using System;
using System.Collections.Generic;
using ClassicUO.Game.Managers;
using ClassicUO.Resources;
using ClassicUO.Utility;

namespace ClassicUO.Game.Data;

internal class SpellDefinition : IEquatable<SpellDefinition>
{
	public static SpellDefinition EmptySpell = new SpellDefinition("", 0, 0, "", 0, 0, TargetType.Neutral);

	internal static Dictionary<string, SpellDefinition> WordToTargettype = new Dictionary<string, SpellDefinition>();

	public readonly int GumpIconID;

	public readonly int GumpIconSmallID;

	public readonly int ID;

	public readonly int ManaCost;

	public readonly int MinSkill;

	public readonly string Name;

	public readonly string PowerWords;

	public readonly Reagents[] Regs;

	public readonly TargetType TargetType;

	public readonly int TithingCost;

	public SpellDefinition(string name, int index, int gumpIconID, int gumpSmallIconID, string powerwords, int manacost, int minskill, int tithingcost, TargetType target, params Reagents[] regs)
	{
		Name = name;
		ID = index;
		GumpIconID = gumpIconID;
		GumpIconSmallID = gumpSmallIconID;
		Regs = regs;
		ManaCost = manacost;
		MinSkill = minskill;
		PowerWords = powerwords;
		TithingCost = tithingcost;
		TargetType = target;
		AddToWatchedSpell();
	}

	public SpellDefinition(string name, int index, int gumpIconID, string powerwords, int manacost, int minskill, TargetType target, params Reagents[] regs)
	{
		Name = name;
		ID = index;
		GumpIconID = gumpIconID;
		GumpIconSmallID = gumpIconID;
		Regs = regs;
		ManaCost = manacost;
		MinSkill = minskill;
		PowerWords = powerwords;
		TithingCost = 0;
		TargetType = target;
		AddToWatchedSpell();
	}

	public SpellDefinition(string name, int index, int gumpIconID, string powerwords, TargetType target, params Reagents[] regs)
	{
		Name = name;
		ID = index;
		GumpIconID = gumpIconID;
		GumpIconSmallID = gumpIconID - 4760;
		Regs = regs;
		ManaCost = 0;
		MinSkill = 0;
		TithingCost = 0;
		PowerWords = powerwords;
		TargetType = target;
		AddToWatchedSpell();
	}

	public bool Equals(SpellDefinition other)
	{
		return ID.Equals(other.ID);
	}

	private void AddToWatchedSpell()
	{
		if (!string.IsNullOrEmpty(PowerWords))
		{
			WordToTargettype[PowerWords] = this;
		}
		else if (!string.IsNullOrEmpty(Name))
		{
			WordToTargettype[Name] = this;
		}
	}

	public string CreateReagentListString(string separator)
	{
		ValueStringBuilder valueStringBuilder = default(ValueStringBuilder);
		for (int i = 0; i < Regs.Length; i++)
		{
			switch (Regs[i])
			{
			case Reagents.BlackPearl:
				valueStringBuilder.Append(ResGeneral.BlackPearl);
				break;
			case Reagents.Bloodmoss:
				valueStringBuilder.Append(ResGeneral.Bloodmoss);
				break;
			case Reagents.Garlic:
				valueStringBuilder.Append(ResGeneral.Garlic);
				break;
			case Reagents.Ginseng:
				valueStringBuilder.Append(ResGeneral.Ginseng);
				break;
			case Reagents.MandrakeRoot:
				valueStringBuilder.Append(ResGeneral.MandrakeRoot);
				break;
			case Reagents.Nightshade:
				valueStringBuilder.Append(ResGeneral.Nightshade);
				break;
			case Reagents.SulfurousAsh:
				valueStringBuilder.Append(ResGeneral.SulfurousAsh);
				break;
			case Reagents.SpidersSilk:
				valueStringBuilder.Append(ResGeneral.SpidersSilk);
				break;
			case Reagents.BatWing:
				valueStringBuilder.Append(ResGeneral.BatWing);
				break;
			case Reagents.GraveDust:
				valueStringBuilder.Append(ResGeneral.GraveDust);
				break;
			case Reagents.DaemonBlood:
				valueStringBuilder.Append(ResGeneral.DaemonBlood);
				break;
			case Reagents.NoxCrystal:
				valueStringBuilder.Append(ResGeneral.NoxCrystal);
				break;
			case Reagents.PigIron:
				valueStringBuilder.Append(ResGeneral.PigIron);
				break;
			default:
				if (Regs[i] < Reagents.None)
				{
					valueStringBuilder.Append(StringHelper.AddSpaceBeforeCapital(Regs[i].ToString()));
				}
				break;
			}
			if (i < Regs.Length - 1)
			{
				valueStringBuilder.Append(separator);
			}
		}
		string result = valueStringBuilder.ToString();
		valueStringBuilder.Dispose();
		return result;
	}

	public static SpellDefinition FullIndexGetSpell(int fullidx)
	{
		if (fullidx < 1 || fullidx > 799)
		{
			return EmptySpell;
		}
		if (fullidx < 100)
		{
			return SpellsMagery.GetSpell(fullidx);
		}
		if (fullidx < 200)
		{
			return SpellsNecromancy.GetSpell(fullidx % 100);
		}
		if (fullidx < 300)
		{
			return SpellsChivalry.GetSpell(fullidx % 100);
		}
		if (fullidx < 500)
		{
			return SpellsBushido.GetSpell(fullidx % 100);
		}
		if (fullidx < 600)
		{
			return SpellsNinjitsu.GetSpell(fullidx % 100);
		}
		if (fullidx < 678)
		{
			return SpellsSpellweaving.GetSpell(fullidx % 100);
		}
		if (fullidx < 700)
		{
			return SpellsMysticism.GetSpell((fullidx - 77) % 100);
		}
		return SpellsMastery.GetSpell(fullidx % 100);
	}

	public static void FullIndexSetModifySpell(int fullidx, int id, int iconid, int smalliconid, int minskill, int manacost, int tithing, string name, string words, TargetType target, params Reagents[] regs)
	{
		if (fullidx < 1 || fullidx > 799)
		{
			return;
		}
		SpellDefinition spellDefinition = FullIndexGetSpell(fullidx);
		if (spellDefinition.ID == fullidx)
		{
			if (iconid == 0)
			{
				iconid = spellDefinition.GumpIconID;
			}
			if (smalliconid == 0)
			{
				smalliconid = spellDefinition.GumpIconSmallID;
			}
			if (tithing == 0)
			{
				tithing = spellDefinition.TithingCost;
			}
			if (manacost == 0)
			{
				manacost = spellDefinition.ManaCost;
			}
			if (minskill == 0)
			{
				minskill = spellDefinition.MinSkill;
			}
			if (!string.IsNullOrEmpty(spellDefinition.PowerWords) && spellDefinition.PowerWords != words)
			{
				WordToTargettype.Remove(spellDefinition.PowerWords);
			}
			if (!string.IsNullOrEmpty(spellDefinition.Name) && spellDefinition.Name != name)
			{
				WordToTargettype.Remove(spellDefinition.Name);
			}
		}
		spellDefinition = new SpellDefinition(name, fullidx, iconid, smalliconid, words, manacost, minskill, tithing, target, regs);
		if (fullidx < 100)
		{
			SpellsMagery.SetSpell(id, in spellDefinition);
		}
		else if (fullidx < 200)
		{
			SpellsNecromancy.SetSpell(id, in spellDefinition);
		}
		else if (fullidx < 300)
		{
			SpellsChivalry.SetSpell(id, in spellDefinition);
		}
		else if (fullidx < 500)
		{
			SpellsBushido.SetSpell(id, in spellDefinition);
		}
		else if (fullidx < 600)
		{
			SpellsNinjitsu.SetSpell(id, in spellDefinition);
		}
		else if (fullidx < 678)
		{
			SpellsSpellweaving.SetSpell(id, in spellDefinition);
		}
		else if (fullidx < 700)
		{
			SpellsMysticism.SetSpell(id - 77, in spellDefinition);
		}
		else
		{
			SpellsMastery.SetSpell(id, in spellDefinition);
		}
	}
}

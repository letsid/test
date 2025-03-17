using System;
using System.Collections.Generic;
using SDL2;

namespace ClassicUO.Game.Managers;

internal class HotkeysManager
{
	private readonly Dictionary<HotkeyAction, Action> _actions = new Dictionary<HotkeyAction, Action>();

	private readonly List<HotKeyCombination> _hotkeys = new List<HotKeyCombination>();

	public HotkeysManager()
	{
		Add(HotkeyAction.CastClumsy, delegate
		{
			GameActions.CastSpell(1);
		});
		Add(HotkeyAction.CastCreateFood, delegate
		{
			GameActions.CastSpell(2);
		});
		Add(HotkeyAction.CastFeeblemind, delegate
		{
			GameActions.CastSpell(3);
		});
		Add(HotkeyAction.CastHeal, delegate
		{
			GameActions.CastSpell(4);
		});
		Add(HotkeyAction.CastMagicArrow, delegate
		{
			GameActions.CastSpell(5);
		});
		Add(HotkeyAction.CastNightSight, delegate
		{
			GameActions.CastSpell(6);
		});
		Add(HotkeyAction.CastReactiveArmor, delegate
		{
			GameActions.CastSpell(7);
		});
		Add(HotkeyAction.CastWeaken, delegate
		{
			GameActions.CastSpell(8);
		});
		Add(HotkeyAction.CastAgility, delegate
		{
			GameActions.CastSpell(9);
		});
		Add(HotkeyAction.CastCunning, delegate
		{
			GameActions.CastSpell(10);
		});
		Add(HotkeyAction.CastCure, delegate
		{
			GameActions.CastSpell(11);
		});
		Add(HotkeyAction.CastHarm, delegate
		{
			GameActions.CastSpell(12);
		});
		Add(HotkeyAction.CastMagicTrap, delegate
		{
			GameActions.CastSpell(13);
		});
		Add(HotkeyAction.CastMagicUntrap, delegate
		{
			GameActions.CastSpell(14);
		});
		Add(HotkeyAction.CastProtection, delegate
		{
			GameActions.CastSpell(15);
		});
		Add(HotkeyAction.CastStrength, delegate
		{
			GameActions.CastSpell(16);
		});
		Add(HotkeyAction.CastBless, delegate
		{
			GameActions.CastSpell(17);
		});
		Add(HotkeyAction.CastFireball, delegate
		{
			GameActions.CastSpell(18);
		});
		Add(HotkeyAction.CastMagicLock, delegate
		{
			GameActions.CastSpell(19);
		});
		Add(HotkeyAction.CastPosion, delegate
		{
			GameActions.CastSpell(20);
		});
		Add(HotkeyAction.CastTelekinesis, delegate
		{
			GameActions.CastSpell(21);
		});
		Add(HotkeyAction.CastTeleport, delegate
		{
			GameActions.CastSpell(22);
		});
		Add(HotkeyAction.CastUnlock, delegate
		{
			GameActions.CastSpell(23);
		});
		Add(HotkeyAction.CastWallOfStone, delegate
		{
			GameActions.CastSpell(24);
		});
		Add(HotkeyAction.CastArchCure, delegate
		{
			GameActions.CastSpell(25);
		});
		Add(HotkeyAction.CastArchProtection, delegate
		{
			GameActions.CastSpell(26);
		});
		Add(HotkeyAction.CastCurse, delegate
		{
			GameActions.CastSpell(27);
		});
		Add(HotkeyAction.CastFireField, delegate
		{
			GameActions.CastSpell(28);
		});
		Add(HotkeyAction.CastGreaterHeal, delegate
		{
			GameActions.CastSpell(29);
		});
		Add(HotkeyAction.CastLightning, delegate
		{
			GameActions.CastSpell(30);
		});
		Add(HotkeyAction.CastManaDrain, delegate
		{
			GameActions.CastSpell(31);
		});
		Add(HotkeyAction.CastRecall, delegate
		{
			GameActions.CastSpell(32);
		});
		Add(HotkeyAction.CastBladeSpirits, delegate
		{
			GameActions.CastSpell(33);
		});
		Add(HotkeyAction.CastDispelField, delegate
		{
			GameActions.CastSpell(34);
		});
		Add(HotkeyAction.CastIncognito, delegate
		{
			GameActions.CastSpell(35);
		});
		Add(HotkeyAction.CastMagicReflection, delegate
		{
			GameActions.CastSpell(36);
		});
		Add(HotkeyAction.CastMindBlast, delegate
		{
			GameActions.CastSpell(37);
		});
		Add(HotkeyAction.CastParalyze, delegate
		{
			GameActions.CastSpell(38);
		});
		Add(HotkeyAction.CastPoisonField, delegate
		{
			GameActions.CastSpell(39);
		});
		Add(HotkeyAction.CastSummonCreature, delegate
		{
			GameActions.CastSpell(40);
		});
		Add(HotkeyAction.CastDispel, delegate
		{
			GameActions.CastSpell(41);
		});
		Add(HotkeyAction.CastEnergyBolt, delegate
		{
			GameActions.CastSpell(42);
		});
		Add(HotkeyAction.CastExplosion, delegate
		{
			GameActions.CastSpell(43);
		});
		Add(HotkeyAction.CastInvisibility, delegate
		{
			GameActions.CastSpell(44);
		});
		Add(HotkeyAction.CastMark, delegate
		{
			GameActions.CastSpell(45);
		});
		Add(HotkeyAction.CastMassCurse, delegate
		{
			GameActions.CastSpell(46);
		});
		Add(HotkeyAction.CastParalyzeField, delegate
		{
			GameActions.CastSpell(47);
		});
		Add(HotkeyAction.CastReveal, delegate
		{
			GameActions.CastSpell(48);
		});
		Add(HotkeyAction.CastChainLightning, delegate
		{
			GameActions.CastSpell(49);
		});
		Add(HotkeyAction.CastEnergyField, delegate
		{
			GameActions.CastSpell(50);
		});
		Add(HotkeyAction.CastFlamestrike, delegate
		{
			GameActions.CastSpell(51);
		});
		Add(HotkeyAction.CastGateTravel, delegate
		{
			GameActions.CastSpell(52);
		});
		Add(HotkeyAction.CastManaVampire, delegate
		{
			GameActions.CastSpell(53);
		});
		Add(HotkeyAction.CastMassDispel, delegate
		{
			GameActions.CastSpell(54);
		});
		Add(HotkeyAction.CastMeteorSwam, delegate
		{
			GameActions.CastSpell(55);
		});
		Add(HotkeyAction.CastPolymorph, delegate
		{
			GameActions.CastSpell(56);
		});
		Add(HotkeyAction.CastEarthquake, delegate
		{
			GameActions.CastSpell(57);
		});
		Add(HotkeyAction.CastEnergyVortex, delegate
		{
			GameActions.CastSpell(58);
		});
		Add(HotkeyAction.CastResurrection, delegate
		{
			GameActions.CastSpell(59);
		});
		Add(HotkeyAction.CastAirElemental, delegate
		{
			GameActions.CastSpell(60);
		});
		Add(HotkeyAction.CastSummonDaemon, delegate
		{
			GameActions.CastSpell(61);
		});
		Add(HotkeyAction.CastEarthElemental, delegate
		{
			GameActions.CastSpell(62);
		});
		Add(HotkeyAction.CastFireElemental, delegate
		{
			GameActions.CastSpell(63);
		});
		Add(HotkeyAction.CastWaterElemental, delegate
		{
			GameActions.CastSpell(64);
		});
		Add(HotkeyAction.CastAnimatedDead, delegate
		{
			GameActions.CastSpell(101);
		});
		Add(HotkeyAction.CastBloodOath, delegate
		{
			GameActions.CastSpell(102);
		});
		Add(HotkeyAction.CastCorpseSkin, delegate
		{
			GameActions.CastSpell(103);
		});
		Add(HotkeyAction.CastCurseWeapon, delegate
		{
			GameActions.CastSpell(104);
		});
		Add(HotkeyAction.CastEvilOmen, delegate
		{
			GameActions.CastSpell(105);
		});
		Add(HotkeyAction.CastHorrificBeast, delegate
		{
			GameActions.CastSpell(106);
		});
		Add(HotkeyAction.CastLichForm, delegate
		{
			GameActions.CastSpell(107);
		});
		Add(HotkeyAction.CastMindRot, delegate
		{
			GameActions.CastSpell(108);
		});
		Add(HotkeyAction.CastPainSpike, delegate
		{
			GameActions.CastSpell(109);
		});
		Add(HotkeyAction.CastPoisonStrike, delegate
		{
			GameActions.CastSpell(110);
		});
		Add(HotkeyAction.CastStrangle, delegate
		{
			GameActions.CastSpell(111);
		});
		Add(HotkeyAction.CastSummonFamiliar, delegate
		{
			GameActions.CastSpell(112);
		});
		Add(HotkeyAction.CastVampiricEmbrace, delegate
		{
			GameActions.CastSpell(113);
		});
		Add(HotkeyAction.CastVangefulSpririt, delegate
		{
			GameActions.CastSpell(114);
		});
		Add(HotkeyAction.CastWither, delegate
		{
			GameActions.CastSpell(115);
		});
		Add(HotkeyAction.CastWraithForm, delegate
		{
			GameActions.CastSpell(116);
		});
		Add(HotkeyAction.CastExorcism, delegate
		{
			GameActions.CastSpell(117);
		});
		Add(HotkeyAction.CastCleanseByFire, delegate
		{
			GameActions.CastSpell(201);
		});
		Add(HotkeyAction.CastCloseWounds, delegate
		{
			GameActions.CastSpell(202);
		});
		Add(HotkeyAction.CastConsecrateWeapon, delegate
		{
			GameActions.CastSpell(203);
		});
		Add(HotkeyAction.CastDispelEvil, delegate
		{
			GameActions.CastSpell(204);
		});
		Add(HotkeyAction.CastDivineFury, delegate
		{
			GameActions.CastSpell(205);
		});
		Add(HotkeyAction.CastEnemyOfOne, delegate
		{
			GameActions.CastSpell(206);
		});
		Add(HotkeyAction.CastHolyLight, delegate
		{
			GameActions.CastSpell(207);
		});
		Add(HotkeyAction.CastNobleSacrifice, delegate
		{
			GameActions.CastSpell(208);
		});
		Add(HotkeyAction.CastRemoveCurse, delegate
		{
			GameActions.CastSpell(209);
		});
		Add(HotkeyAction.CastSacredJourney, delegate
		{
			GameActions.CastSpell(210);
		});
		Add(HotkeyAction.CastHonorableExecution, delegate
		{
			GameActions.CastSpell(401);
		});
		Add(HotkeyAction.CastConfidence, delegate
		{
			GameActions.CastSpell(402);
		});
		Add(HotkeyAction.CastEvasion, delegate
		{
			GameActions.CastSpell(403);
		});
		Add(HotkeyAction.CastCounterAttack, delegate
		{
			GameActions.CastSpell(404);
		});
		Add(HotkeyAction.CastLightningStrike, delegate
		{
			GameActions.CastSpell(405);
		});
		Add(HotkeyAction.CastMomentumStrike, delegate
		{
			GameActions.CastSpell(406);
		});
		Add(HotkeyAction.CastFocusAttack, delegate
		{
			GameActions.CastSpell(501);
		});
		Add(HotkeyAction.CastDeathStrike, delegate
		{
			GameActions.CastSpell(502);
		});
		Add(HotkeyAction.CastAnimalForm, delegate
		{
			GameActions.CastSpell(503);
		});
		Add(HotkeyAction.CastKiAttack, delegate
		{
			GameActions.CastSpell(504);
		});
		Add(HotkeyAction.CastSurpriseAttack, delegate
		{
			GameActions.CastSpell(505);
		});
		Add(HotkeyAction.CastBackstab, delegate
		{
			GameActions.CastSpell(506);
		});
		Add(HotkeyAction.CastShadowjump, delegate
		{
			GameActions.CastSpell(507);
		});
		Add(HotkeyAction.CastMirrorImage, delegate
		{
			GameActions.CastSpell(508);
		});
		Add(HotkeyAction.CastArcaneCircle, delegate
		{
			GameActions.CastSpell(601);
		});
		Add(HotkeyAction.CastGiftOfRenewal, delegate
		{
			GameActions.CastSpell(602);
		});
		Add(HotkeyAction.CastImmolatingWeapon, delegate
		{
			GameActions.CastSpell(603);
		});
		Add(HotkeyAction.CastAttuneWeapon, delegate
		{
			GameActions.CastSpell(604);
		});
		Add(HotkeyAction.CastThinderstorm, delegate
		{
			GameActions.CastSpell(605);
		});
		Add(HotkeyAction.CastNaturesFury, delegate
		{
			GameActions.CastSpell(606);
		});
		Add(HotkeyAction.CastSummonFey, delegate
		{
			GameActions.CastSpell(607);
		});
		Add(HotkeyAction.CastSummonFiend, delegate
		{
			GameActions.CastSpell(608);
		});
		Add(HotkeyAction.CastReaperForm, delegate
		{
			GameActions.CastSpell(609);
		});
		Add(HotkeyAction.CastWildFire, delegate
		{
			GameActions.CastSpell(610);
		});
		Add(HotkeyAction.CastEssenceOfWind, delegate
		{
			GameActions.CastSpell(611);
		});
		Add(HotkeyAction.CastDryadAllure, delegate
		{
			GameActions.CastSpell(612);
		});
		Add(HotkeyAction.CastEtherealVoyage, delegate
		{
			GameActions.CastSpell(613);
		});
		Add(HotkeyAction.CastWordOfDeath, delegate
		{
			GameActions.CastSpell(614);
		});
		Add(HotkeyAction.CastGiftOfLife, delegate
		{
			GameActions.CastSpell(615);
		});
		Add(HotkeyAction.CastNetherBolt, delegate
		{
			GameActions.CastSpell(678);
		});
		Add(HotkeyAction.CastHealingStone, delegate
		{
			GameActions.CastSpell(679);
		});
		Add(HotkeyAction.CastPurgeMagic, delegate
		{
			GameActions.CastSpell(680);
		});
		Add(HotkeyAction.CastEnchant, delegate
		{
			GameActions.CastSpell(681);
		});
		Add(HotkeyAction.CastSleep, delegate
		{
			GameActions.CastSpell(682);
		});
		Add(HotkeyAction.CastEagleStrike, delegate
		{
			GameActions.CastSpell(683);
		});
		Add(HotkeyAction.CastAnimatedWeapon, delegate
		{
			GameActions.CastSpell(684);
		});
		Add(HotkeyAction.CastStoneForm, delegate
		{
			GameActions.CastSpell(685);
		});
		Add(HotkeyAction.CastSpellTrigger, delegate
		{
			GameActions.CastSpell(686);
		});
		Add(HotkeyAction.CastMassSleep, delegate
		{
			GameActions.CastSpell(687);
		});
		Add(HotkeyAction.CastCleansingWinds, delegate
		{
			GameActions.CastSpell(688);
		});
		Add(HotkeyAction.CastBombard, delegate
		{
			GameActions.CastSpell(689);
		});
		Add(HotkeyAction.CastSpellPlague, delegate
		{
			GameActions.CastSpell(690);
		});
		Add(HotkeyAction.CastHailStorm, delegate
		{
			GameActions.CastSpell(691);
		});
		Add(HotkeyAction.CastNetherCyclone, delegate
		{
			GameActions.CastSpell(692);
		});
		Add(HotkeyAction.CastRisingColossus, delegate
		{
			GameActions.CastSpell(693);
		});
		Add(HotkeyAction.CastInspire, delegate
		{
			GameActions.CastSpell(701);
		});
		Add(HotkeyAction.CastInvigorate, delegate
		{
			GameActions.CastSpell(702);
		});
		Add(HotkeyAction.CastResilience, delegate
		{
			GameActions.CastSpell(703);
		});
		Add(HotkeyAction.CastPerseverance, delegate
		{
			GameActions.CastSpell(704);
		});
		Add(HotkeyAction.CastTribulation, delegate
		{
			GameActions.CastSpell(705);
		});
		Add(HotkeyAction.CastDespair, delegate
		{
			GameActions.CastSpell(706);
		});
	}

	public bool Bind(HotkeyAction action, SDL.SDL_Keycode key, SDL.SDL_Keymod mod)
	{
		foreach (HotKeyCombination hotkey in _hotkeys)
		{
			if (hotkey.Key == key && hotkey.Mod == mod)
			{
				return false;
			}
		}
		_hotkeys.Add(new HotKeyCombination
		{
			Key = key,
			Mod = mod,
			KeyAction = action
		});
		return true;
	}

	public void UnBind(HotkeyAction action)
	{
		for (int i = 0; i < _hotkeys.Count; i++)
		{
			if (_hotkeys[i].KeyAction == action)
			{
				_hotkeys.RemoveAt(i);
				break;
			}
		}
	}

	public bool TryExecuteIfBinded(SDL.SDL_Keycode key, SDL.SDL_Keymod mod, out Action action)
	{
		for (int i = 0; i < _hotkeys.Count; i++)
		{
			HotKeyCombination hotKeyCombination = _hotkeys[i];
			if (hotKeyCombination.Key == key && hotKeyCombination.Mod == mod)
			{
				if (!_actions.TryGetValue(hotKeyCombination.KeyAction, out action))
				{
					break;
				}
				return true;
			}
		}
		action = null;
		return false;
	}

	public Dictionary<HotkeyAction, Action> GetValues()
	{
		return _actions;
	}

	private void Add(HotkeyAction action, Action handler)
	{
		_actions.Add(action, handler);
	}
}

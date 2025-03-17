using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using ClassicUO.IO.Resources;
using ClassicUO.Utility;

namespace ClassicUO.Game.Data;

internal static class StaticFilters
{
	private static readonly STATIC_TILES_FILTER_FLAGS[] _filteredTiles = new STATIC_TILES_FILTER_FLAGS[81920];

	public static readonly List<ushort> CaveTiles = new List<ushort>();

	public static readonly List<ushort> TreeTiles = new List<ushort>();

	public static void Load()
	{
		string text = Path.Combine(CUOEnviroment.ExecutablePath, "Data", "Client");
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		string path = Path.Combine(text, "cave.txt");
		string path2 = Path.Combine(text, "vegetation.txt");
		string path3 = Path.Combine(text, "tree.txt");
		if (!File.Exists(path))
		{
			using StreamWriter streamWriter = new StreamWriter(path);
			for (int i = 1339; i < 1364; i++)
			{
				if (i != 1360)
				{
					streamWriter.WriteLine(i);
				}
			}
		}
		if (!File.Exists(path2))
		{
			using StreamWriter streamWriter2 = new StreamWriter(path2);
			ushort[] array = new ushort[178]
			{
				3397, 3398, 3399, 3400, 3401, 3402, 3403, 3404, 3405, 3406,
				3407, 3408, 3409, 3410, 3411, 3412, 3420, 3421, 3422, 3423,
				3424, 3425, 3426, 3427, 3428, 3429, 3430, 3431, 3432, 3433,
				3437, 3443, 3444, 3445, 3446, 3447, 3448, 3449, 3450, 3451,
				3452, 3453, 3454, 3455, 3456, 3459, 3463, 3464, 3465, 3466,
				3467, 3468, 3469, 3470, 3471, 3472, 3473, 3475, 4790, 4791,
				4796, 4797, 4798, 4799, 4800, 4801, 4802, 4803, 4804, 4805,
				4806, 4807, 3257, 3260, 3261, 3262, 3263, 3264, 3265, 3267,
				3269, 3270, 3271, 3315, 3316, 3317, 3318, 3319, 3332, 3334,
				3335, 3336, 3337, 3338, 3339, 3340, 3341, 3342, 3343, 3344,
				3345, 3346, 3347, 3348, 3349, 3350, 3351, 3352, 3353, 3368,
				3369, 3370, 3371, 3373, 3380, 3382, 3502, 3503, 3514, 3515,
				3516, 3517, 3518, 3521, 3522, 3523, 3203, 3204, 3205, 3206,
				3207, 3208, 3209, 3210, 3211, 3212, 3213, 3214, 3219, 3220,
				3224, 3231, 3232, 3233, 3234, 3235, 3236, 3239, 3244, 3245,
				3246, 3247, 3248, 3249, 3250, 3251, 3252, 3253, 3254, 3141,
				3142, 3145, 3143, 3144, 3146, 3147, 3148, 3149, 3150, 3127,
				3128, 3258, 3375, 3378, 3379, 3391, 3392, 3305
			};
			foreach (ushort num in array)
			{
				if (!TileDataLoader.Instance.StaticData[num].IsImpassable)
				{
					streamWriter2.WriteLine(num);
				}
			}
		}
		if (!File.Exists(path3))
		{
			using StreamWriter streamWriter4 = new StreamWriter(path3);
			using StreamWriter streamWriter3 = new StreamWriter(path2, append: true);
			ushort[] array2 = new ushort[62]
			{
				3221, 3222, 3225, 3227, 3228, 3229, 3230, 3238, 3240, 3242,
				3243, 3273, 3274, 3275, 3276, 3277, 3280, 3283, 3286, 3288,
				3290, 3293, 3296, 3299, 3302, 3320, 3323, 3326, 3329, 3383,
				3384, 3393, 3394, 3395, 3396, 3415, 3416, 3417, 3418, 3419,
				3438, 3439, 3440, 3441, 3442, 3460, 3461, 3462, 3476, 3480,
				3484, 3488, 3492, 3496, 4790, 4791, 4792, 4793, 4794, 4795,
				4796, 4797
			};
			foreach (ushort num2 in array2)
			{
				byte b = 1;
				switch (num2)
				{
				case 3230:
				case 3240:
				case 3242:
				case 3243:
				case 3273:
				case 3320:
				case 3323:
				case 3326:
				case 3329:
				case 4790:
				case 4791:
				case 4792:
				case 4793:
				case 4794:
				case 4795:
					b = 0;
					break;
				}
				if (!TileDataLoader.Instance.StaticData[num2].IsImpassable)
				{
					streamWriter3.WriteLine(num2);
				}
				else
				{
					streamWriter4.WriteLine($"{num2}={b}");
				}
			}
		}
		TextFileParser textFileParser = new TextFileParser(File.ReadAllText(path), new char[3] { ' ', '\t', ',' }, new char[2] { '#', ';' }, new char[2] { '"', '"' });
		while (!textFileParser.IsEOF())
		{
			List<string> list = textFileParser.ReadTokens();
			if (list != null && list.Count != 0 && ushort.TryParse(list[0], out var result))
			{
				_filteredTiles[result] |= STATIC_TILES_FILTER_FLAGS.STFF_CAVE;
				CaveTiles.Add(result);
			}
		}
		TextFileParser textFileParser2 = new TextFileParser(File.ReadAllText(path3), new char[4] { ' ', '\t', ',', '=' }, new char[2] { '#', ';' }, new char[2] { '"', '"' });
		while (!textFileParser2.IsEOF())
		{
			List<string> list2 = textFileParser2.ReadTokens();
			if (list2 != null && list2.Count >= 2)
			{
				STATIC_TILES_FILTER_FLAGS sTATIC_TILES_FILTER_FLAGS = STATIC_TILES_FILTER_FLAGS.STFF_STUMP;
				if (byte.TryParse(list2[1], out var result2) && result2 != 0)
				{
					sTATIC_TILES_FILTER_FLAGS |= STATIC_TILES_FILTER_FLAGS.STFF_STUMP_HATCHED;
				}
				if (ushort.TryParse(list2[0], out var result3))
				{
					_filteredTiles[result3] |= sTATIC_TILES_FILTER_FLAGS;
					TreeTiles.Add(result3);
				}
			}
		}
		TextFileParser textFileParser3 = new TextFileParser(File.ReadAllText(path2), new char[3] { ' ', '\t', ',' }, new char[2] { '#', ';' }, new char[2] { '"', '"' });
		while (!textFileParser3.IsEOF())
		{
			List<string> list3 = textFileParser3.ReadTokens();
			if (list3 != null && list3.Count != 0 && ushort.TryParse(list3[0], out var result4))
			{
				_filteredTiles[result4] |= STATIC_TILES_FILTER_FLAGS.STFF_VEGETATION;
			}
		}
	}

	public static void CleanCaveTextures()
	{
	}

	public static void CleanTreeTextures()
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsTree(ushort g, out int index)
	{
		STATIC_TILES_FILTER_FLAGS sTATIC_TILES_FILTER_FLAGS = _filteredTiles[g];
		if ((sTATIC_TILES_FILTER_FLAGS & STATIC_TILES_FILTER_FLAGS.STFF_STUMP) != 0)
		{
			if ((sTATIC_TILES_FILTER_FLAGS & STATIC_TILES_FILTER_FLAGS.STFF_STUMP_HATCHED) != 0)
			{
				index = 0;
			}
			else
			{
				index = 1;
			}
			return true;
		}
		index = 0;
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsVegetation(ushort g)
	{
		return (_filteredTiles[g] & STATIC_TILES_FILTER_FLAGS.STFF_VEGETATION) != 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsCave(ushort g)
	{
		return (_filteredTiles[g] & STATIC_TILES_FILTER_FLAGS.STFF_CAVE) != 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsRock(ushort g)
	{
		if (g != 4945)
		{
			switch (g)
			{
			case 4948:
			case 4950:
			case 4953:
			case 4955:
			case 4958:
			case 4959:
			case 4960:
			case 4962:
				break;
			default:
				if (g >= 6001)
				{
					return g <= 6012;
				}
				return false;
			}
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsWalkInWater(ushort g)
	{
		switch (g)
		{
		case 2221:
		case 8460:
		case 8461:
		case 8462:
		case 8463:
		case 8464:
		case 8465:
		case 8466:
		case 8467:
		case 8468:
		case 8469:
		case 8470:
		case 8471:
		case 8488:
		case 8489:
		case 8504:
		case 8505:
		case 8506:
		case 8507:
		case 8508:
		case 8509:
		case 8510:
		case 8517:
		case 12765:
		case 12766:
		case 12767:
		case 12768:
		case 12769:
		case 12770:
		case 12771:
		case 12772:
		case 12773:
		case 12774:
		case 12775:
		case 12776:
		case 12777:
		case 12778:
		case 12779:
		case 12780:
		case 12781:
		case 12782:
		case 12783:
		case 12784:
		case 12785:
			return true;
		default:
			return false;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsWater(int g)
	{
		if (g <= 8471)
		{
			if (g <= 5465)
			{
				if ((uint)(g - 168) <= 3u || (uint)(g - 310) <= 1u || g == 5465)
				{
					goto IL_0233;
				}
			}
			else if ((uint)(g - 6038) <= 9u || (uint)(g - 6058) <= 7u || (uint)(g - 8460) <= 11u)
			{
				goto IL_0233;
			}
			goto IL_0235;
		}
		if (g <= 8517)
		{
			if ((uint)(g - 8488) > 1u && (uint)(g - 8504) > 6u && g != 8517)
			{
				goto IL_0235;
			}
		}
		else
		{
			switch (g)
			{
			case 12765:
			case 12766:
			case 12767:
			case 12768:
			case 12769:
			case 12770:
			case 12771:
			case 12772:
			case 12773:
			case 12774:
			case 12775:
			case 12776:
			case 12777:
			case 12778:
			case 12779:
			case 12780:
			case 12781:
			case 12782:
			case 12783:
			case 12784:
			case 12785:
			case 13422:
			case 13423:
			case 13424:
			case 13425:
			case 13426:
			case 13427:
			case 13428:
			case 13429:
			case 13430:
			case 13431:
			case 13432:
			case 13433:
			case 13434:
			case 13435:
			case 13436:
			case 13437:
			case 13438:
			case 13439:
			case 13440:
			case 13441:
			case 13442:
			case 13443:
			case 13444:
			case 13445:
			case 13456:
			case 13457:
			case 13458:
			case 13459:
			case 13460:
			case 13461:
			case 13462:
			case 13463:
			case 13464:
			case 13466:
			case 13467:
			case 13468:
			case 13469:
			case 13470:
			case 13471:
			case 13472:
			case 13473:
			case 13474:
			case 13475:
			case 13476:
			case 13477:
			case 13478:
			case 13479:
			case 13480:
			case 13481:
			case 13482:
			case 13483:
			case 13493:
			case 13494:
			case 13495:
			case 13496:
			case 13497:
			case 13498:
			case 13501:
			case 13502:
			case 13503:
			case 13504:
			case 13506:
			case 13507:
			case 13508:
			case 13509:
			case 13511:
			case 13512:
			case 13513:
			case 13514:
			case 96672:
			case 96673:
			case 96674:
			case 96675:
			case 96676:
			case 96677:
			case 96678:
			case 96679:
			case 96680:
			case 96681:
				break;
			default:
				goto IL_0235;
			}
		}
		goto IL_0233;
		IL_0233:
		return true;
		IL_0235:
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsField(ushort g)
	{
		if ((g < 14732 || g > 14751) && (g < 14695 || g > 14714) && (g < 14662 || g > 14692))
		{
			if (g >= 14612)
			{
				return g <= 14633;
			}
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsFireField(ushort g)
	{
		if (g >= 14732)
		{
			return g <= 14751;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsParalyzeField(ushort g)
	{
		if (g >= 14695)
		{
			return g <= 14714;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsEnergyField(ushort g)
	{
		if (g >= 14662)
		{
			return g <= 14692;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsPoisonField(ushort g)
	{
		if (g >= 14612)
		{
			return g <= 14633;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsWallOfStone(ushort g)
	{
		return g == 906;
	}
}

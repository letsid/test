namespace ClassicUO.Game.Data;

internal static class LightColors
{
	private static readonly uint[][] _lightCurveTables = new uint[5][]
	{
		new uint[32]
		{
			0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u,
			0u, 0u, 0u, 0u, 0u, 0u, 1u, 2u, 3u, 4u,
			6u, 8u, 10u, 12u, 14u, 16u, 18u, 20u, 22u, 24u,
			26u, 28u
		},
		new uint[32]
		{
			0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u,
			0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u,
			0u, 0u, 0u, 0u, 1u, 2u, 3u, 4u, 5u, 6u,
			7u, 8u
		},
		new uint[32]
		{
			0u, 1u, 2u, 4u, 6u, 8u, 11u, 14u, 17u, 20u,
			23u, 26u, 29u, 30u, 31u, 31u, 31u, 31u, 31u, 31u,
			31u, 31u, 31u, 31u, 31u, 31u, 31u, 31u, 31u, 31u,
			31u, 31u
		},
		new uint[32]
		{
			0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 1u, 1u,
			2u, 2u, 3u, 3u, 4u, 4u, 5u, 6u, 7u, 8u,
			9u, 10u, 11u, 12u, 13u, 15u, 17u, 19u, 21u, 23u,
			25u, 27u
		},
		new uint[32]
		{
			0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u, 0u,
			0u, 0u, 0u, 0u, 0u, 0u, 0u, 1u, 5u, 10u,
			15u, 20u, 25u, 30u, 30u, 18u, 18u, 18u, 18u, 18u,
			18u, 18u
		}
	};

	public static ushort GetHue(ushort id)
	{
		ushort result = 0;
		switch (id)
		{
		case 2188:
			result = 31;
			break;
		case 4012:
			result = 30;
			break;
		case 4017:
			result = 60;
			break;
		case 5703:
			result = 61;
			break;
		case 6587:
		case 7979:
			result = 40;
			break;
		case 40806:
			result = 0;
			break;
		}
		if (id < 2555 || id > 2580)
		{
			if (id < 2581 || id > 2601)
			{
				if (id < 2842 || id > 2847)
				{
					if (id < 2848 || id > 2853)
					{
						if (id < 2854 || id > 2856)
						{
							if (id < 3553 || id > 3562)
							{
								if (id < 6217 || id > 6224)
								{
									if (id < 6227 || id > 6234)
									{
										if (id < 6522 || id > 6569)
										{
											if (id < 6571 || id > 6582)
											{
												if ((id >= 7885 && id <= 7887) || (id >= 7888 && id <= 7890))
												{
													result = 1;
												}
											}
											else
											{
												result = 60;
											}
										}
										else
										{
											result = 60;
										}
									}
									else
									{
										result = 61;
									}
								}
								else
								{
									result = 61;
								}
							}
							else
							{
								result = 31;
							}
						}
						else
						{
							result = 0;
						}
					}
					else
					{
						result = 0;
					}
				}
				else
				{
					result = 0;
				}
			}
			else
			{
				result = 0;
			}
		}
		else
		{
			result = 30;
		}
		if (id == 8148 || id == 3948)
		{
			result = 2;
		}
		if (id < 3629 || id > 3632)
		{
			if (id < 3633 || id > 3635)
			{
				if (id < 3676 || id > 3690)
				{
					if (id < 4846 || id > 4941)
					{
						if (id < 12394 || id > 12955)
						{
							if (id < 13371 || id > 13420)
							{
								if (id < 13639 || id > 13644)
								{
									if (id < 14612 || id > 14633)
									{
										if (id < 14662 || id > 14692)
										{
											if (id < 14695 || id > 14714)
											{
												if (id < 14732 || id > 14751)
												{
													if (id < 15874 || id > 15883)
													{
														if (id < 15911 || id > 15930)
														{
															switch (id)
															{
															case 16638:
																result = 40;
																break;
															case 16639:
																result = 10;
																break;
															case 16640:
																result = 20;
																break;
															case 16641:
																result = 32;
																break;
															default:
																if (id < 38975 || id > 38977)
																{
																	break;
																}
																goto case 38971;
															case 38971:
															case 38972:
															case 38973:
																result = 30;
																break;
															}
														}
														else
														{
															result = 31;
														}
													}
													else
													{
														result = 1;
													}
												}
												else
												{
													result = 31;
												}
											}
											else
											{
												result = 6;
											}
										}
										else
										{
											result = 6;
										}
									}
									else
									{
										result = 1;
									}
								}
								else
								{
									result = 31;
								}
							}
							else
							{
								result = 31;
							}
						}
						else
						{
							result = 31;
						}
					}
					else
					{
						result = 31;
					}
				}
				else
				{
					result = 6;
				}
			}
			else
			{
				result = 40;
			}
		}
		else
		{
			result = 62;
		}
		return result;
	}

	internal static void CreateLookupTables(uint[] buffer)
	{
		for (uint num = 0u; num < 32; num++)
		{
			uint num2 = _lightCurveTables[0][num];
			uint num3 = _lightCurveTables[1][num];
			uint num4 = _lightCurveTables[2][num];
			uint num5 = _lightCurveTables[3][num];
			uint num6 = _lightCurveTables[4][num];
			buffer[num] = (num2 << 11) | 0xFF000000u;
			buffer[32 + num] = (num << 19) | (num >> 1 << 11) | (num >> 1 << 3) | 0xFF000000u;
			buffer[160 + num] = (num2 << 19) | (num3 << 3) | 0xFF000000u;
			buffer[288 + num] = (num << 19) | (num >> 2 << 11) | (num >> 2 << 3) | 0xFF000000u;
			buffer[608 + num] = (num << 11) | 0xFF000000u;
			buffer[928 + num] = (num4 >> 1 << 11) | (num4 << 3) | 0xFF000000u;
			buffer[960 + num] = (num2 >> 1 << 11) | (num2 << 3) | 0xFF000000u;
			buffer[992 + num] = (num << 19) | (num << 3) | 0xFF000000u;
			buffer[1248 + num] = (num << 3) | 0xFF000000u;
			buffer[1568 + num] = (num << 11) | (num << 3) | 0xFF000000u;
			buffer[1888 + num] = (num2 << 11) | (num2 << 3) | 0xFF000000u;
			buffer[1920 + num] = (num5 << 11) | (num5 << 3) | 0xFF000000u;
			buffer[1952 + num] = (num5 << 19) | (num5 << 11) | (num5 << 3) | 0xFF000000u;
			buffer[1984 + num] = (num6 << 19) | (num6 << 11) | (num6 << 3) | 0xFF000000u;
		}
	}
}

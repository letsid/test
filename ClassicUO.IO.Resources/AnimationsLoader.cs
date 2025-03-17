using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ClassicUO.Data;
using ClassicUO.Game;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using ClassicUO.Utility.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.IO.Resources;

internal class AnimationsLoader : UOFileLoader
{
	public struct SittingInfoData
	{
		public readonly ushort Graphic;

		public readonly sbyte Direction1;

		public readonly sbyte Direction2;

		public readonly sbyte Direction3;

		public readonly sbyte Direction4;

		public readonly sbyte OffsetY;

		public readonly sbyte MirrorOffsetY;

		public readonly bool DrawBack;

		public static SittingInfoData Empty;

		public SittingInfoData(ushort graphic, sbyte d1, sbyte d2, sbyte d3, sbyte d4, sbyte offsetY, sbyte mirrorOffsetY, bool drawback)
		{
			Graphic = graphic;
			Direction1 = d1;
			Direction2 = d2;
			Direction3 = d3;
			Direction4 = d4;
			OffsetY = offsetY;
			MirrorOffsetY = mirrorOffsetY;
			DrawBack = drawback;
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	private ref struct AnimIdxBlock
	{
		public readonly uint Position;

		public readonly uint Size;

		public readonly uint Unknown;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	private ref struct UOPAnimationHeader
	{
		public ushort Group;

		public ushort FrameID;

		public ushort Unk0;

		public ushort Unk1;

		public ushort Unk2;

		public ushort Unk3;

		public uint DataOffset;
	}

	private static AnimationsLoader _instance;

	private readonly Dictionary<ushort, byte> _animationSequenceReplacing = new Dictionary<ushort, byte>();

	private readonly Dictionary<ushort, Rectangle> _animDimensionCache = new Dictionary<ushort, Rectangle>();

	private readonly AnimationGroup _empty = new AnimationGroup
	{
		Direction = new AnimationDirection[5]
		{
			new AnimationDirection
			{
				FileIndex = -1,
				Address = -1L
			},
			new AnimationDirection
			{
				FileIndex = -1,
				Address = -1L
			},
			new AnimationDirection
			{
				FileIndex = -1,
				Address = -1L
			},
			new AnimationDirection
			{
				FileIndex = -1,
				Address = -1L
			},
			new AnimationDirection
			{
				FileIndex = -1,
				Address = -1L
			}
		}
	};

	private readonly Dictionary<ushort, Dictionary<ushort, EquipConvData>> _equipConv = new Dictionary<ushort, Dictionary<ushort, EquipConvData>>();

	private readonly UOFileMul[] _files = new UOFileMul[5];

	private readonly UOFileUop[] _filesUop = new UOFileUop[4];

	private readonly PixelPicker _picker = new PixelPicker();

	private TextureAtlas _atlas;

	public List<Tuple<ushort, uint>> DrawOrderDefList = new List<Tuple<ushort, uint>>();

	public PixelPicker Picker => _picker;

	public static AnimationsLoader Instance => _instance ?? (_instance = new AnimationsLoader());

	public IndexAnimation[] DataIndex { get; } = new IndexAnimation[4096];

	public IReadOnlyDictionary<ushort, Dictionary<ushort, EquipConvData>> EquipConversions => _equipConv;

	public List<Tuple<ushort, byte>>[] GroupReplaces { get; } = new List<Tuple<ushort, byte>>[2]
	{
		new List<Tuple<ushort, byte>>(),
		new List<Tuple<ushort, byte>>()
	};

	private AnimationsLoader()
	{
	}

	public void CreateAtlas(GraphicsDevice device)
	{
		_atlas = new TextureAtlas(device, 4096, 4096, SurfaceFormat.Color);
	}

	public unsafe override Task Load()
	{
		return Task.Run(delegate
		{
			bool flag = false;
			int[] array = new int[5] { 262144, 65536, 131072, 131072, 131072 };
			for (int i = 0; i < 5; i++)
			{
				string uOFilePath = UOFileManager.GetUOFilePath("anim" + ((i == 0) ? string.Empty : (i + 1).ToString()) + ".mul");
				string uOFilePath2 = UOFileManager.GetUOFilePath("anim" + ((i == 0) ? string.Empty : (i + 1).ToString()) + ".idx");
				if (File.Exists(uOFilePath) && File.Exists(uOFilePath2))
				{
					_files[i] = new UOFileMul(uOFilePath, uOFilePath2, array[i], (i == 0) ? 6 : (-1));
				}
				if (i > 0 && Client.IsUOPInstallation)
				{
					string uOFilePath3 = UOFileManager.GetUOFilePath($"AnimationFrame{i}.uop");
					if (File.Exists(uOFilePath3))
					{
						_filesUop[i - 1] = new UOFileUop(uOFilePath3, "build/animationlegacyframe/{0:D6}/{0:D2}.bin");
						if (!flag)
						{
							flag = true;
						}
					}
				}
			}
			if (flag)
			{
				LoadUop();
			}
			int num = sizeof(AnimIdxBlock);
			UOFile uOFile = _files[0]?.IdxFile;
			long? num2 = (long?)uOFile?.StartAddress + uOFile?.Length;
			UOFile obj = _files[1]?.IdxFile;
			long? num3 = (long?)obj?.StartAddress + obj?.Length;
			UOFile obj2 = _files[2]?.IdxFile;
			long? num4 = (long?)obj2?.StartAddress + obj2?.Length;
			UOFile obj3 = _files[3]?.IdxFile;
			long? num5 = (long?)obj3?.StartAddress + obj3?.Length;
			UOFile obj4 = _files[4]?.IdxFile;
			long? num6 = (long?)obj4?.StartAddress + obj4?.Length;
			if (Client.Version >= ClientVersion.CV_500A)
			{
				string uOFilePath4 = UOFileManager.GetUOFilePath("mobtypes.txt");
				if (File.Exists(uOFilePath4))
				{
					string[] array2 = new string[5] { "monster", "sea_monster", "animal", "human", "equipment" };
					using StreamReader streamReader = new StreamReader(File.OpenRead(uOFilePath4));
					string text;
					while ((text = streamReader.ReadLine()) != null)
					{
						text = text.Trim();
						if (text.Length != 0 && text[0] != '#' && char.IsNumber(text[0]))
						{
							string[] array3 = text.Split(new char[2] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
							if (array3.Length >= 3)
							{
								int num7 = int.Parse(array3[0]);
								if (num7 < 4096)
								{
									string text2 = array3[1].ToLower();
									int num8 = array3[2].IndexOf('#');
									if (num8 > 0)
									{
										array3[2] = array3[2].Substring(0, num8 - 1);
									}
									else if (num8 == 0)
									{
										continue;
									}
									uint num9 = uint.Parse(array3[2], NumberStyles.HexNumber);
									for (int j = 0; j < 5; j++)
									{
										if (text2 == array2[j])
										{
											ref IndexAnimation reference = ref DataIndex[num7];
											if (reference == null)
											{
												reference = new IndexAnimation();
											}
											reference.Type = (ANIMATION_GROUPS_TYPE)j;
											reference.Flags = (ANIMATION_FLAGS)(0x80000000u | num9);
											break;
										}
									}
								}
							}
						}
					}
				}
			}
			for (ushort num10 = 0; num10 < 4096; num10++)
			{
				if (DataIndex[num10] == null)
				{
					DataIndex[num10] = new IndexAnimation();
				}
				if (DataIndex[num10].Type == ANIMATION_GROUPS_TYPE.UNKNOWN)
				{
					DataIndex[num10].Type = CalculateTypeByGraphic(num10);
				}
				DataIndex[num10].Graphic = num10;
				DataIndex[num10].CorpseGraphic = num10;
				int groupCount;
				long num11 = DataIndex[num10].CalculateOffset(num10, DataIndex[num10].Type, out groupCount);
				if (num11 < uOFile.Length)
				{
					bool isValidMUL = false;
					long num12 = _files[0].IdxFile.StartAddress.ToInt64() + num11;
					DataIndex[num10].Groups = new AnimationGroup[100];
					int num13 = 0;
					for (byte b = 0; b < 100; b++)
					{
						DataIndex[num10].Groups[b] = new AnimationGroup
						{
							Direction = new AnimationDirection[5]
						};
						if (b < groupCount)
						{
							for (byte b2 = 0; b2 < 5; b2++)
							{
								if (DataIndex[num10].Groups[b].Direction[b2] == null)
								{
									DataIndex[num10].Groups[b].Direction[b2] = new AnimationDirection();
								}
								AnimIdxBlock* ptr = (AnimIdxBlock*)(num12 + num13 * num);
								num13++;
								if ((long?)ptr < num2 && ptr->Size != 0 && ptr->Position != uint.MaxValue && ptr->Size != uint.MaxValue)
								{
									DataIndex[num10].Groups[b].Direction[b2].Address = ptr->Position;
									DataIndex[num10].Groups[b].Direction[b2].Size = ptr->Size;
									isValidMUL = true;
								}
							}
						}
					}
					if (num10 <= 1050)
					{
						DataIndex[num10].IsValidMUL = isValidMUL;
					}
				}
			}
			string uOFilePath5 = UOFileManager.GetUOFilePath("Anim1.def");
			if (File.Exists(uOFilePath5))
			{
				using DefReader defReader = new DefReader(uOFilePath5);
				while (defReader.Next())
				{
					ushort num14 = (ushort)defReader.ReadInt();
					if (num14 != ushort.MaxValue)
					{
						int num15 = defReader.ReadGroupInt();
						GroupReplaces[0].Add(new Tuple<ushort, byte>(num14, (byte)num15));
					}
				}
			}
			uOFilePath5 = UOFileManager.GetUOFilePath("Anim2.def");
			if (File.Exists(uOFilePath5))
			{
				using DefReader defReader2 = new DefReader(uOFilePath5);
				while (defReader2.Next())
				{
					ushort num16 = (ushort)defReader2.ReadInt();
					if (num16 != ushort.MaxValue)
					{
						int num17 = defReader2.ReadGroupInt();
						GroupReplaces[1].Add(new Tuple<ushort, byte>(num16, (byte)num17));
					}
				}
			}
			if (Client.Version >= ClientVersion.CV_300)
			{
				uOFilePath5 = UOFileManager.GetUOFilePath("Equipconv.def");
				if (File.Exists(uOFilePath5))
				{
					using DefReader defReader3 = new DefReader(uOFilePath5, 5);
					while (defReader3.Next())
					{
						ushort num18 = (ushort)defReader3.ReadInt();
						if (num18 < 4096)
						{
							ushort num19 = (ushort)defReader3.ReadInt();
							if (num19 < 4096)
							{
								ushort num20 = (ushort)defReader3.ReadInt();
								if (num20 < 4096)
								{
									int num21 = defReader3.ReadInt();
									if (num21 <= 65535)
									{
										switch (num21)
										{
										case 0:
											num21 = num19;
											break;
										case -1:
										case 65535:
											num21 = num20;
											break;
										}
										ushort color = (ushort)defReader3.ReadInt();
										if (!_equipConv.TryGetValue(num18, out var value))
										{
											_equipConv.Add(num18, new Dictionary<ushort, EquipConvData>());
											if (!_equipConv.TryGetValue(num18, out value))
											{
												continue;
											}
										}
										value[num19] = new EquipConvData(num20, (ushort)num21, color);
									}
								}
							}
						}
					}
				}
				uOFilePath5 = UOFileManager.GetUOFilePath("Bodyconv.def");
				if (File.Exists(uOFilePath5))
				{
					using DefReader defReader4 = new DefReader(uOFilePath5);
					while (defReader4.Next())
					{
						ushort num22 = (ushort)defReader4.ReadInt();
						if (num22 < 4096)
						{
							int[] obj5 = new int[4] { 0, -1, -1, -1 };
							obj5[0] = defReader4.ReadInt();
							int[] array4 = obj5;
							if (defReader4.PartsCount >= 3)
							{
								array4[1] = defReader4.ReadInt();
								if (defReader4.PartsCount >= 4)
								{
									array4[2] = defReader4.ReadInt();
									if (defReader4.PartsCount >= 5)
									{
										array4[3] = defReader4.ReadInt();
									}
								}
							}
							int num23 = 0;
							ushort num24 = ushort.MaxValue;
							sbyte mountedHeightOffset = 0;
							if (array4[0] != -1 && num3.HasValue && num3 != 0)
							{
								num23 = 1;
								num24 = (ushort)array4[0];
								if (num22 == 192 || num22 == 793)
								{
									mountedHeightOffset = -9;
								}
							}
							else if (array4[1] != -1 && num4.HasValue && num4 != 0)
							{
								num23 = 2;
								num24 = (ushort)array4[1];
								if (num22 == 1401)
								{
									mountedHeightOffset = 9;
								}
							}
							else if (array4[2] != -1 && num5.HasValue && num5 != 0)
							{
								num23 = 3;
								num24 = (ushort)array4[2];
							}
							else if (array4[3] != -1 && num6.HasValue && num6 != 0)
							{
								num23 = 4;
								num24 = (ushort)array4[3];
								mountedHeightOffset = -9;
								switch (num22)
								{
								case 192:
								case 277:
									mountedHeightOffset = 0;
									break;
								case 1069:
									mountedHeightOffset = 3;
									break;
								}
							}
							if (num24 != ushort.MaxValue && num23 != 0)
							{
								UOFile idxFile = _files[num23].IdxFile;
								ANIMATION_GROUPS_TYPE type = ((Client.Version < ClientVersion.CV_500A) ? CalculateTypeByGraphic(num24) : DataIndex[num22].Type);
								long num25 = DataIndex[num22].CalculateOffset(num24, type, out var groupCount2);
								if (num25 < idxFile.Length)
								{
									DataIndex[num22].Type = type;
									if (DataIndex[num22].MountedHeightOffset == 0)
									{
										DataIndex[num22].MountedHeightOffset = mountedHeightOffset;
									}
									DataIndex[num22].GraphicConversion = (ushort)(num24 | 0x8000);
									DataIndex[num22].FileIndex = (byte)num23;
									num25 += idxFile.StartAddress.ToInt64();
									long num26 = idxFile.StartAddress.ToInt64() + idxFile.Length;
									int num27 = 0;
									DataIndex[num22].BodyConvGroups = new AnimationGroup[100];
									for (int k = 0; k < groupCount2; k++)
									{
										DataIndex[num22].BodyConvGroups[k] = new AnimationGroup();
										if (DataIndex[num22].BodyConvGroups[k].Direction == null)
										{
											DataIndex[num22].BodyConvGroups[k].Direction = new AnimationDirection[5];
										}
										for (byte b3 = 0; b3 < 5; b3++)
										{
											if (DataIndex[num22].BodyConvGroups[k].Direction[b3] == null)
											{
												DataIndex[num22].BodyConvGroups[k].Direction[b3] = new AnimationDirection();
											}
											AnimIdxBlock* ptr2 = (AnimIdxBlock*)(num25 + num27 * num);
											num27++;
											if ((long)ptr2 < num26 && ptr2->Position != uint.MaxValue && ptr2->Size != uint.MaxValue)
											{
												AnimationDirection obj6 = DataIndex[num22].BodyConvGroups[k].Direction[b3];
												obj6.Address = ptr2->Position;
												obj6.Size = Math.Max(1u, ptr2->Size);
												obj6.FileIndex = num23;
											}
										}
									}
								}
							}
						}
					}
				}
				uOFilePath5 = UOFileManager.GetUOFilePath("Body.def");
				Dictionary<int, bool> dictionary = new Dictionary<int, bool>();
				if (File.Exists(uOFilePath5))
				{
					using DefReader defReader5 = new DefReader(uOFilePath5, 1);
					while (defReader5.Next())
					{
						int num28 = defReader5.ReadInt();
						if (num28 < 4096 && !(dictionary.TryGetValue(num28, out var value2) && value2))
						{
							int[] array5 = defReader5.ReadGroup();
							if (array5 != null)
							{
								int num29 = defReader5.ReadInt();
								int num30 = ((array5.Length < 3) ? array5[0] : array5[2]);
								if (num30 < 4096)
								{
									DataIndex[num28].Graphic = (ushort)num30;
									DataIndex[num28].Color = (ushort)num29;
									DataIndex[num28].IsValidMUL = true;
									dictionary[num28] = true;
								}
							}
						}
					}
				}
				uOFilePath5 = UOFileManager.GetUOFilePath("Corpse.def");
				dictionary.Clear();
				if (File.Exists(uOFilePath5))
				{
					using DefReader defReader6 = new DefReader(uOFilePath5, 1);
					while (defReader6.Next())
					{
						int num31 = defReader6.ReadInt();
						if (num31 < 4096 && !(dictionary.TryGetValue(num31, out var value3) && value3))
						{
							int[] array6 = defReader6.ReadGroup();
							if (array6 != null)
							{
								int num32 = defReader6.ReadInt();
								int num33 = ((array6.Length < 3) ? array6[0] : array6[2]);
								if (num33 < 4096)
								{
									DataIndex[num31].CorpseGraphic = (ushort)num33;
									DataIndex[num31].CorpseColor = (ushort)num32;
									dictionary[num31] = true;
								}
							}
						}
					}
				}
				uOFilePath5 = UOFileManager.GetUOFilePath("draworder.def");
				if (File.Exists(uOFilePath5))
				{
					using (DefReader defReader7 = new DefReader(uOFilePath5))
					{
						while (defReader7.Next())
						{
							ushort item = (ushort)defReader7.ReadInt();
							uint item2 = (uint)defReader7.ReadInt();
							DrawOrderDefList.Add(new Tuple<ushort, uint>(item, item2));
						}
					}
				}
			}
		});
	}

	public uint FindItemFlagsInDrawDefList(Item item)
	{
		uint result = 0u;
		ushort num = item.ItemData.AnimID;
		if (Instance.EquipConversions.TryGetValue(num, out var value) && value.TryGetValue(item.ItemData.AnimID, out var value2))
		{
			num = value2.Graphic;
		}
		for (int i = 0; i < DrawOrderDefList.Count; i++)
		{
			if (DrawOrderDefList[i].Item1 == num)
			{
				result = DrawOrderDefList[i].Item2;
				break;
			}
		}
		return result;
	}

	public void MoveLayerYToFrontAndSwapWithX(Layer[] layers, Layer x, Layer y)
	{
		int num = -1;
		for (int i = 0; i < 23; i++)
		{
			if (layers[i] == y)
			{
				num = i;
			}
			else if (layers[i] == x)
			{
				if (num == -1)
				{
					break;
				}
				layers[num] = x;
				layers[i] = y;
			}
		}
	}

	public bool IsLayerDrawnAfter(Layer[] layers, Layer a, Layer b)
	{
		bool flag = false;
		for (int i = 0; i < 23; i++)
		{
			if (!flag && layers[i] == b)
			{
				flag = true;
			}
			else if (layers[i] == a)
			{
				if (flag)
				{
					return true;
				}
				return false;
			}
		}
		return true;
	}

	public Layer[] GetCurrentLayerOrder(Entity mobile)
	{
		Layer[] array = new Layer[23];
		for (int i = 0; i < 23; i++)
		{
			array[i] = LayerOrder.defaultClientLayerOrder[i];
		}
		Item item = mobile.FindItemByLayer(Layer.Arms);
		if (item != null && (FindItemFlagsInDrawDefList(item) & 1) != 0)
		{
			MoveLayerYToFrontAndSwapWithX(array, Layer.Face, Layer.Arms);
		}
		Item item2 = mobile.FindItemByLayer(Layer.Torso);
		if (item2 != null)
		{
			uint num = FindItemFlagsInDrawDefList(item2);
			if ((num & 2) != 0)
			{
				MoveLayerYToFrontAndSwapWithX(array, Layer.Face, Layer.Arms);
			}
			if ((num & 4) != 0)
			{
				MoveLayerYToFrontAndSwapWithX(array, Layer.Torso, Layer.Cloak);
			}
			if ((num & 8) != 0)
			{
				if (!IsLayerDrawnAfter(array, Layer.Torso, Layer.Arms))
				{
					MoveLayerYToFrontAndSwapWithX(array, Layer.Torso, Layer.Cloak);
					MoveLayerYToFrontAndSwapWithX(array, Layer.Arms, Layer.Cloak);
				}
				else
				{
					MoveLayerYToFrontAndSwapWithX(array, Layer.Torso, Layer.Cloak);
				}
			}
		}
		return array;
	}

	private unsafe void LoadUop()
	{
		if (Client.Version <= ClientVersion.CV_60144)
		{
			return;
		}
		for (ushort num = 0; num < 4096; num++)
		{
			for (byte b = 0; b < 100; b++)
			{
				ulong hash = UOFileUop.CreateHash($"build/animationlegacyframe/{num:D6}/{b:D2}.bin");
				for (int i = 0; i < _filesUop.Length; i++)
				{
					UOFileUop uOFileUop = _filesUop[i];
					if (uOFileUop == null || !uOFileUop.TryGetUOPData(hash, out var data))
					{
						continue;
					}
					if (DataIndex[num] == null)
					{
						DataIndex[num] = new IndexAnimation
						{
							UopGroups = new AnimationGroupUop[100]
						};
						DataIndex[num].InitializeUOP();
					}
					ref AnimationGroupUop reference = ref DataIndex[num].UopGroups[b];
					reference = new AnimationGroupUop
					{
						Offset = (uint)data.Offset,
						CompressedLength = (uint)data.Length,
						DecompressedLength = (uint)data.DecompressedLength,
						FileIndex = i,
						Direction = new AnimationDirection[5]
					};
					for (int j = 0; j < 5; j++)
					{
						if (reference.Direction[j] == null)
						{
							reference.Direction[j] = new AnimationDirection();
						}
						reference.Direction[j].IsUOP = true;
					}
				}
			}
		}
		for (int k = 0; k < _filesUop.Length; k++)
		{
			_filesUop[k]?.ClearHashes();
		}
		string uOFilePath = UOFileManager.GetUOFilePath("AnimationSequence.uop");
		if (!File.Exists(uOFilePath))
		{
			Log.Warn("AnimationSequence.uop not found");
			return;
		}
		UOFileUop uOFileUop2 = new UOFileUop(uOFilePath, "build/animationsequence/{0:D8}.bin");
		UOFileIndex[] entries = new UOFileIndex[Math.Max(uOFileUop2.TotalEntriesCount, 4096)];
		uOFileUop2.FillEntries(ref entries);
		Span<byte> span = stackalloc byte[1024];
		for (int l = 0; l < entries.Length; l++)
		{
			ref UOFileIndex reference2 = ref entries[l];
			if (reference2.Offset == 0L)
			{
				continue;
			}
			uOFileUop2.Seek(reference2.Offset);
			byte[] array = null;
			Span<byte> span2 = ((reference2.DecompressedLength <= 1024) ? span : ((Span<byte>)(array = ArrayPool<byte>.Shared.Rent(reference2.DecompressedLength))));
			try
			{
				fixed (byte* ptr = span2)
				{
					ZLib.Decompress(uOFileUop2.PositionAddress, reference2.Length, 0, (IntPtr)ptr, reference2.DecompressedLength);
				}
				StackDataReader stackDataReader = new StackDataReader(span2.Slice(0, reference2.DecompressedLength));
				uint num2 = stackDataReader.ReadUInt32LE();
				stackDataReader.Skip(48);
				int num3 = stackDataReader.ReadInt32LE();
				if (num3 != 48 && num3 != 68)
				{
					for (int m = 0; m < num3; m++)
					{
						int num4 = stackDataReader.ReadInt32LE();
						uint num5 = stackDataReader.ReadUInt32LE();
						int num6 = stackDataReader.ReadInt32LE();
						if (num5 == 0 && DataIndex[num2] != null)
						{
							DataIndex[num2].ReplaceUopGroup((byte)num4, (byte)num6);
						}
						stackDataReader.Skip(60);
					}
					if (DataIndex[num2] != null)
					{
						switch (num2)
						{
						case 1069u:
						case 1254u:
						case 1255u:
						case 1527u:
							DataIndex[num2].MountedHeightOffset = 18;
							break;
						case 432u:
						case 1401u:
						case 1440u:
						case 1526u:
							DataIndex[num2].MountedHeightOffset = 9;
							break;
						}
					}
				}
				stackDataReader.Release();
			}
			finally
			{
				if (array != null)
				{
					ArrayPool<byte>.Shared.Return(array);
				}
			}
		}
		uOFileUop2.Dispose();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static uint CalculatePeopleGroupOffset(ushort graphic)
	{
		return (uint)(((graphic - 400) * 175 + 35000) * sizeof(AnimIdxBlock));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static uint CalculateHighGroupOffset(ushort graphic)
	{
		return (uint)(graphic * 110 * sizeof(AnimIdxBlock));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static uint CalculateLowGroupOffset(ushort graphic)
	{
		return (uint)(((graphic - 200) * 65 + 22000) * sizeof(AnimIdxBlock));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private ANIMATION_GROUPS_TYPE CalculateTypeByGraphic(ushort graphic)
	{
		if (graphic >= 200)
		{
			if (graphic >= 400)
			{
				return ANIMATION_GROUPS_TYPE.HUMAN;
			}
			return ANIMATION_GROUPS_TYPE.ANIMAL;
		}
		return ANIMATION_GROUPS_TYPE.MONSTER;
	}

	public void ConvertBodyIfNeeded(ref ushort graphic, bool isParent = false, bool forceUOP = false)
	{
		if (graphic >= 4096)
		{
			return;
		}
		IndexAnimation indexAnimation = DataIndex[graphic];
		if ((indexAnimation.IsUOP && (isParent || !indexAnimation.IsValidMUL)) || forceUOP)
		{
			return;
		}
		ushort graphic2 = indexAnimation.Graphic;
		while ((DataIndex[graphic2].HasBodyConversion || !indexAnimation.HasBodyConversion) && (!DataIndex[graphic2].HasBodyConversion || !indexAnimation.HasBodyConversion))
		{
			if (graphic != graphic2)
			{
				graphic = graphic2;
				graphic2 = DataIndex[graphic].Graphic;
			}
			if (graphic == graphic2)
			{
				break;
			}
		}
	}

	public AnimationGroup GetBodyAnimationGroup(ref ushort graphic, ref byte group, ref ushort hue, bool isParent = false, bool forceUOP = false)
	{
		if (graphic < 4096 && group < 100)
		{
			IndexAnimation indexAnimation = DataIndex[graphic];
			if ((indexAnimation.IsUOP && (isParent || !indexAnimation.IsValidMUL)) || forceUOP)
			{
				return indexAnimation.GetUopGroup(ref group) ?? _empty;
			}
			ushort graphic2 = indexAnimation.Graphic;
			while ((DataIndex[graphic2].HasBodyConversion || !indexAnimation.HasBodyConversion) && (!DataIndex[graphic2].HasBodyConversion || !indexAnimation.HasBodyConversion))
			{
				if (graphic != graphic2)
				{
					graphic = graphic2;
					hue = indexAnimation.Color;
					graphic2 = DataIndex[graphic].Graphic;
				}
				if (graphic == graphic2)
				{
					break;
				}
			}
			if (DataIndex[graphic].HasBodyConversion && DataIndex[graphic].BodyConvGroups != null)
			{
				return DataIndex[graphic].BodyConvGroups[group] ?? _empty;
			}
			if (DataIndex[graphic].Groups != null && DataIndex[graphic].Groups[group] != null)
			{
				return DataIndex[graphic].Groups[group];
			}
		}
		return _empty;
	}

	public AnimationGroup GetCorpseAnimationGroup(ref ushort graphic, ref byte group, ref ushort hue)
	{
		if (graphic < 4096 && group < 100)
		{
			IndexAnimation indexAnimation = DataIndex[graphic];
			if (indexAnimation.IsUOP)
			{
				return indexAnimation.GetUopGroup(ref group) ?? _empty;
			}
			ushort corpseGraphic = indexAnimation.CorpseGraphic;
			while ((DataIndex[corpseGraphic].HasBodyConversion || !indexAnimation.HasBodyConversion) && (!DataIndex[corpseGraphic].HasBodyConversion || !indexAnimation.HasBodyConversion))
			{
				if (graphic != corpseGraphic)
				{
					graphic = corpseGraphic;
					hue = indexAnimation.CorpseColor;
					corpseGraphic = DataIndex[graphic].CorpseGraphic;
				}
				if (graphic == corpseGraphic)
				{
					break;
				}
			}
			if (DataIndex[graphic].HasBodyConversion)
			{
				if (DataIndex[graphic].BodyConvGroups == null)
				{
					return _empty;
				}
				return DataIndex[graphic].BodyConvGroups[group];
			}
			if (DataIndex[graphic].Groups == null)
			{
				return _empty;
			}
			return DataIndex[graphic].Groups[group] ?? _empty;
		}
		return _empty;
	}

	public bool IsReplacedByAnimationSequence(ushort graphic, out byte type)
	{
		return _animationSequenceReplacing.TryGetValue(graphic, out type);
	}

	public override void ClearResources()
	{
	}

	public bool PixelCheck(ushort animID, byte group, byte direction, bool uop, int frame, int x, int y, bool isSitting = false)
	{
		uint num = (uint)(group | (direction << 8)) | ((uop ? 1u : 0u) << 16) | ((isSitting ? 1u : 0u) << 17);
		ulong textureID = (uint)(animID | (frame << 16)) | ((ulong)num << 32);
		return _picker.Get(textureID, x, y);
	}

	public void UpdateAnimationTable(uint flags)
	{
		for (ushort num = 0; num < 4096; num++)
		{
			bool flag = DataIndex[num].FileIndex >= 3;
			if (DataIndex[num].FileIndex == 1)
			{
				flag = (World.ClientLockedFeatures.Flags & LockedFeatureFlags.LordBlackthornsRevenge) != 0;
			}
			else if (DataIndex[num].FileIndex == 2)
			{
				flag = (World.ClientLockedFeatures.Flags & LockedFeatureFlags.AgeOfShadows) != 0;
			}
			if (flag && !DataIndex[num].HasBodyConversion)
			{
				DataIndex[num].GraphicConversion = (ushort)(DataIndex[num].GraphicConversion & -32769);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void GetAnimDirection(ref byte dir, ref bool mirror)
	{
		switch (dir)
		{
		case 2:
		case 4:
			mirror = dir == 2;
			dir = 1;
			break;
		case 1:
		case 5:
			mirror = dir == 1;
			dir = 2;
			break;
		case 0:
		case 6:
			mirror = dir == 0;
			dir = 3;
			break;
		case 3:
			dir = 0;
			break;
		case 7:
			dir = 4;
			break;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void GetSittingAnimDirection(ref byte dir, ref bool mirror, ref int x, ref int y)
	{
		switch (dir)
		{
		case 0:
			mirror = true;
			dir = 3;
			break;
		case 2:
			mirror = true;
			dir = 1;
			break;
		case 4:
			mirror = false;
			dir = 1;
			break;
		case 6:
			mirror = false;
			dir = 3;
			break;
		case 1:
		case 3:
		case 5:
			break;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FixSittingDirection(ref byte direction, ref bool mirror, ref int x, ref int y, ref SittingInfoData data)
	{
		switch (direction)
		{
		case 0:
		case 7:
			if (data.Direction1 == -1)
			{
				if (direction == 7)
				{
					direction = (byte)data.Direction4;
				}
				else
				{
					direction = (byte)data.Direction2;
				}
			}
			else
			{
				direction = (byte)data.Direction1;
			}
			break;
		case 1:
		case 2:
			if (data.Direction2 == -1)
			{
				if (direction == 1)
				{
					direction = (byte)data.Direction1;
				}
				else
				{
					direction = (byte)data.Direction3;
				}
			}
			else
			{
				direction = (byte)data.Direction2;
			}
			break;
		case 3:
		case 4:
			if (data.Direction3 == -1)
			{
				if (direction == 3)
				{
					direction = (byte)data.Direction2;
				}
				else
				{
					direction = (byte)data.Direction4;
				}
			}
			else
			{
				direction = (byte)data.Direction3;
			}
			break;
		case 5:
		case 6:
			if (data.Direction4 == -1)
			{
				if (direction == 5)
				{
					direction = (byte)data.Direction3;
				}
				else
				{
					direction = (byte)data.Direction1;
				}
			}
			else
			{
				direction = (byte)data.Direction4;
			}
			break;
		}
		GetSittingAnimDirection(ref direction, ref mirror, ref x, ref y);
		int num = 8;
		if (mirror)
		{
			if (direction == 3)
			{
				y += 25 + data.MirrorOffsetY;
				x += num - 4;
			}
			else
			{
				y += data.OffsetY + 9;
			}
		}
		else if (direction == 3)
		{
			y += 23 + data.MirrorOffsetY;
			x -= 3;
		}
		else
		{
			y += 10 + data.OffsetY;
			x -= num + 1;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ANIMATION_GROUPS GetGroupIndex(ushort graphic)
	{
		if (graphic >= 4096)
		{
			return ANIMATION_GROUPS.AG_HIGHT;
		}
		switch (DataIndex[graphic].Type)
		{
		case ANIMATION_GROUPS_TYPE.ANIMAL:
			return ANIMATION_GROUPS.AG_LOW;
		case ANIMATION_GROUPS_TYPE.MONSTER:
		case ANIMATION_GROUPS_TYPE.SEA_MONSTER:
			return ANIMATION_GROUPS.AG_HIGHT;
		case ANIMATION_GROUPS_TYPE.HUMAN:
		case ANIMATION_GROUPS_TYPE.EQUIPMENT:
			return ANIMATION_GROUPS.AG_PEOPLE;
		default:
			return ANIMATION_GROUPS.AG_HIGHT;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public byte GetDieGroupIndex(ushort id, bool second, bool isRunning = false)
	{
		if (id >= 4096)
		{
			return 0;
		}
		ANIMATION_FLAGS flags = DataIndex[id].Flags;
		switch (DataIndex[id].Type)
		{
		case ANIMATION_GROUPS_TYPE.ANIMAL:
			if ((flags & ANIMATION_FLAGS.AF_USE_2_IF_HITTED_WHILE_RUNNING) != 0 || (flags & ANIMATION_FLAGS.AF_CAN_FLYING) != 0)
			{
				return 2;
			}
			if ((flags & ANIMATION_FLAGS.AF_USE_UOP_ANIMATION) != 0)
			{
				return (byte)(second ? 3u : 2u);
			}
			if ((flags & ANIMATION_FLAGS.AF_CALCULATE_OFFSET_LOW_GROUP_EXTENDED) != 0)
			{
				return (byte)(second ? 3u : 2u);
			}
			return (byte)(second ? 12u : 8u);
		case ANIMATION_GROUPS_TYPE.SEA_MONSTER:
			if (!isRunning)
			{
				return 8;
			}
			goto case ANIMATION_GROUPS_TYPE.MONSTER;
		case ANIMATION_GROUPS_TYPE.MONSTER:
			if ((flags & ANIMATION_FLAGS.AF_USE_UOP_ANIMATION) != 0)
			{
				return (byte)(second ? 3u : 2u);
			}
			if ((flags & ANIMATION_FLAGS.AF_LOW_GROUP_DIE_INDEX) != 0)
			{
				return (byte)(second ? 12u : 8u);
			}
			return (byte)(second ? 3u : 2u);
		case ANIMATION_GROUPS_TYPE.HUMAN:
		case ANIMATION_GROUPS_TYPE.EQUIPMENT:
			return (byte)(second ? 22u : 21u);
		default:
			return 0;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsAnimationExists(ushort graphic, byte group, bool isCorpse = false)
	{
		if (graphic < 4096 && group < 100)
		{
			ushort hue = 0;
			object obj;
			if (!isCorpse)
			{
				AnimationGroup bodyAnimationGroup = GetBodyAnimationGroup(ref graphic, ref group, ref hue, isParent: true);
				obj = ((bodyAnimationGroup != null) ? bodyAnimationGroup.Direction[0] : null);
			}
			else
			{
				AnimationGroup corpseAnimationGroup = GetCorpseAnimationGroup(ref graphic, ref group, ref hue);
				obj = ((corpseAnimationGroup != null) ? corpseAnimationGroup.Direction[0] : null);
			}
			AnimationDirection animationDirection = (AnimationDirection)obj;
			if (animationDirection != null)
			{
				if (animationDirection.Address == 0L || animationDirection.Size == 0)
				{
					return animationDirection.IsUOP;
				}
				return true;
			}
			return false;
		}
		return false;
	}

	public static bool IsNormalMountAnimStub(int anim)
	{
		if (anim > 432)
		{
			if (anim == 791 || (uint)(anim - 793) <= 1u || anim == 799)
			{
				return true;
			}
			return false;
		}
		switch (anim)
		{
		case 432:
			return true;
		case 114:
		case 115:
		case 116:
		case 117:
		case 118:
		case 119:
		case 120:
		case 121:
		case 122:
		case 132:
		case 144:
		case 169:
		case 170:
		case 171:
		case 177:
		case 178:
		case 179:
		case 187:
		case 188:
		case 190:
		case 191:
		case 192:
		case 193:
		case 194:
		case 195:
		case 200:
		case 204:
		case 210:
		case 213:
		case 218:
		case 219:
		case 220:
		case 226:
		case 228:
		case 243:
		case 276:
		case 277:
		case 284:
			return true;
		default:
			return false;
		}
	}

	public bool LoadAnimationFrames(ushort animID, byte animGroup, byte direction, ref AnimationDirection animDir)
	{
		if (animDir.FileIndex == -1 && animDir.Address == -1)
		{
			return false;
		}
		if (animDir.FileIndex >= _files.Length || animID >= 4096)
		{
			return false;
		}
		if (animDir.IsUOP || (animDir.Address == 0L && animDir.Size == 0))
		{
			AnimationGroupUop uopGroup = DataIndex[animID].GetUopGroup(ref animGroup);
			if (uopGroup == null || uopGroup.Offset == 0)
			{
				return false;
			}
			return ReadUOPAnimationFrame(animID, animGroup, direction, ref animDir);
		}
		if (animDir.Address == 0L && animDir.Size == 0)
		{
			return false;
		}
		ReadMULAnimationFrame(animID, animGroup, direction, ref animDir, _files[animDir.FileIndex]);
		return true;
	}

	private unsafe bool ReadUOPAnimationFrame(ushort animID, byte animGroup, byte direction, ref AnimationDirection animDirection)
	{
		AnimationGroupUop uopGroup = DataIndex[animID].GetUopGroup(ref animGroup);
		if (uopGroup.FileIndex < 0 || uopGroup.FileIndex >= _filesUop.Length)
		{
			return false;
		}
		if (uopGroup.FileIndex == 0 && uopGroup.CompressedLength == 0 && uopGroup.DecompressedLength == 0 && uopGroup.Offset == 0)
		{
			Log.Warn("uop animData is null");
			return false;
		}
		int decompressedLength = (int)uopGroup.DecompressedLength;
		UOFileUop uOFileUop = _filesUop[uopGroup.FileIndex];
		uOFileUop.Seek(uopGroup.Offset);
		byte[] array = null;
		Span<byte> span = ((decompressedLength > 1024) ? ((Span<byte>)(array = ArrayPool<byte>.Shared.Rent(decompressedLength))) : stackalloc byte[decompressedLength]);
		Span<byte> span2 = span;
		fixed (byte* ptr = span2)
		{
			ZLib.Decompress(uOFileUop.PositionAddress, (int)uopGroup.CompressedLength, 0, (IntPtr)ptr, decompressedLength);
		}
		try
		{
			StackDataReader reader = new StackDataReader(span2.Slice(0, decompressedLength));
			reader.Skip(32);
			_ = (long)reader.StartAddress;
			_ = reader.Length;
			int num = reader.ReadInt32LE();
			uint num2 = reader.ReadUInt32LE();
			reader.Seek(num2);
			ANIMATION_GROUPS_TYPE type = DataIndex[animID].Type;
			animDirection.FrameCount = (byte)((type < ANIMATION_GROUPS_TYPE.EQUIPMENT) ? Math.Round((float)num / 5f) : 10.0);
			animDirection.SpriteInfos = new SpriteInfo[animDirection.FrameCount];
			int num3 = sizeof(UOPAnimationHeader);
			int num4 = 0;
			UOPAnimationHeader* ptr2 = (UOPAnimationHeader*)(void*)reader.PositionAddress;
			ushort num5 = 0;
			ushort num6 = ptr2->FrameID;
			ushort num7 = 0;
			while (ptr2->FrameID < num)
			{
				if (ptr2->FrameID - 1 == num6 || num5 >= animDirection.FrameCount)
				{
					if (num7 != direction)
					{
						num7++;
					}
					num6 = ptr2->FrameID;
					num5 = 0;
					num2 = (uint)reader.Position;
				}
				else if (ptr2->FrameID - num6 > 1)
				{
					num5 += (ushort)(ptr2->FrameID - num6);
					num6 = ptr2->FrameID;
				}
				if (num5 == 0 && num7 == direction)
				{
					break;
				}
				reader.Skip(num3);
				ptr2 = (UOPAnimationHeader*)(void*)reader.PositionAddress;
				num5++;
				num6++;
			}
			reader.Seek(num2);
			ptr2 = (UOPAnimationHeader*)(void*)reader.PositionAddress;
			ushort num8 = ptr2->FrameID;
			while (num8 == ptr2->FrameID && num4 < animDirection.FrameCount)
			{
				long num9 = reader.Position;
				if (ptr2->Group == animGroup && num9 + ptr2->DataOffset < reader.Length)
				{
					int num10 = ptr2->FrameID % animDirection.FrameCount;
					if (animDirection.SpriteInfos[num10].Texture == null)
					{
						reader.Skip((int)ptr2->DataOffset);
						ushort* palette = (ushort*)(void*)reader.PositionAddress;
						reader.Skip(512);
						uint num11 = (uint)(animGroup | (direction << 8) | 0x10000);
						ulong key = (uint)(animID | (num10 << 16)) | ((ulong)num11 << 32);
						ReadSpriteData(ref reader, palette, key, ref animDirection.SpriteInfos[num10], alphaCheck: true);
					}
				}
				reader.Seek(num9 + num3);
				ptr2 = (UOPAnimationHeader*)(void*)reader.PositionAddress;
				num8++;
				num4++;
			}
			reader.Release();
			return true;
		}
		finally
		{
			if (array != null)
			{
				ArrayPool<byte>.Shared.Return(array);
			}
		}
	}

	private unsafe void ReadMULAnimationFrame(ushort animID, byte animGroup, byte direction, ref AnimationDirection animDir, UOFile file)
	{
		StackDataReader reader = new StackDataReader(new ReadOnlySpan<byte>(file.StartAddress.ToPointer(), (int)file.Length));
		reader.Seek(animDir.Address);
		ushort* palette = (ushort*)(void*)reader.PositionAddress;
		reader.Skip(512);
		long num = reader.Position;
		uint num2 = reader.ReadUInt32LE();
		animDir.FrameCount = (byte)num2;
		uint* ptr = (uint*)(void*)reader.PositionAddress;
		animDir.SpriteInfos = new SpriteInfo[num2];
		_ = (long)reader.StartAddress;
		_ = reader.Length;
		for (int i = 0; i < num2; i++)
		{
			if (animDir.SpriteInfos[i].Texture == null)
			{
				reader.Seek(num + ptr[i]);
				uint num3 = (uint)(animGroup | (direction << 8) | 0);
				ulong key = (uint)(animID | (i << 16)) | ((ulong)num3 << 32);
				ReadSpriteData(ref reader, palette, key, ref animDir.SpriteInfos[i], alphaCheck: false);
			}
		}
	}

	private unsafe void ReadSpriteData(ref StackDataReader reader, ushort* palette, ulong key, ref SpriteInfo spriteInfo, bool alphaCheck)
	{
		short num = reader.ReadInt16LE();
		short num2 = reader.ReadInt16LE();
		short num3 = reader.ReadInt16LE();
		short num4 = reader.ReadInt16LE();
		if (num3 == 0 || num4 == 0)
		{
			return;
		}
		long num5 = (long)reader.StartAddress + reader.Length;
		int num6 = num3 * num4;
		uint[] array = null;
		Span<uint> span = ((num6 > 1024) ? ((Span<uint>)(array = ArrayPool<uint>.Shared.Rent(num6, zero: true))) : stackalloc uint[num6]);
		Span<uint> pixels = span;
		try
		{
			uint num7 = reader.ReadUInt32LE();
			long num8 = reader.Position;
			while (num7 != 2147450879 && num8 < num5)
			{
				ushort num9 = (ushort)(num7 & 0xFFF);
				int num10 = (int)((num7 >> 22) & 0x3FF);
				if ((num10 & 0x200) > 0)
				{
					num10 |= -512;
				}
				int num11 = (int)((num7 >> 12) & 0x3FF);
				if ((num11 & 0x200) > 0)
				{
					num11 |= -512;
				}
				num10 += num;
				num11 += num2 + num4;
				int num12 = num11 * num3 + num10;
				int num13 = 0;
				while (num13 < num9)
				{
					ushort num14 = palette[(int)reader.ReadUInt8()];
					if (!alphaCheck || num14 != 0)
					{
						pixels[num12] = HuesHelper.Color16To32(num14) | 0xFF000000u;
					}
					else
					{
						pixels[num12] = 0u;
					}
					num13++;
					num12++;
				}
				num7 = reader.ReadUInt32LE();
			}
			spriteInfo.Center.X = num;
			spriteInfo.Center.Y = num2;
			_picker.Set(key, num3, num4, pixels);
			spriteInfo.Texture = _atlas.AddSprite(pixels, num3, num4, out spriteInfo.UV);
		}
		finally
		{
			if (array != null)
			{
				ArrayPool<uint>.Shared.Return(array, clearArray: true);
			}
		}
	}

	public void GetAnimationDimensions(byte animIndex, ushort graphic, byte dir, byte animGroup, bool ismounted, byte frameIndex, out int centerX, out int centerY, out int width, out int height)
	{
		dir &= 0x7F;
		bool mirror = false;
		Instance.GetAnimDirection(ref dir, ref mirror);
		if (frameIndex == byte.MaxValue)
		{
			frameIndex = animIndex;
		}
		Instance.GetAnimationDimensions(frameIndex, graphic, dir, animGroup, out centerX, out centerY, out width, out height);
		if (centerX == 0 && centerY == 0 && width == 0 && height == 0)
		{
			height = (ismounted ? 100 : 60);
		}
	}

	public unsafe void GetAnimationDimensions(byte frameIndex, ushort id, byte dir, byte animGroup, out int x, out int y, out int w, out int h)
	{
		if (id < 4096)
		{
			if (_animDimensionCache.TryGetValue(id, out var value))
			{
				x = value.X;
				y = value.Y;
				w = value.Width;
				h = value.Height;
				return;
			}
			ushort hue = 0;
			if (dir < 5)
			{
				AnimationDirection animationDirection = Instance.GetBodyAnimationGroup(ref id, ref animGroup, ref hue, isParent: true).Direction[dir];
				if (animationDirection != null)
				{
					int frameCount = animationDirection.FrameCount;
					if (frameCount > 0)
					{
						if (frameIndex >= frameCount)
						{
							frameIndex = 0;
						}
						ref SpriteInfo reference = ref animationDirection.SpriteInfos[frameIndex];
						if (reference.Texture != null)
						{
							x = reference.Center.X;
							y = reference.Center.Y;
							w = reference.UV.Width;
							h = reference.UV.Height;
							_animDimensionCache[id] = new Rectangle(x, y, w, h);
							return;
						}
					}
				}
			}
			AnimationDirection animationDirection2 = Instance.GetBodyAnimationGroup(ref id, ref animGroup, ref hue, isParent: true).Direction[0];
			if (animationDirection2 != null)
			{
				if (animationDirection2.Address != 0L && animationDirection2.Size != 0)
				{
					if (!animationDirection2.IsVerdata)
					{
						UOFileMul uOFileMul = _files[animationDirection2.FileIndex];
						uOFileMul.Seek(animationDirection2.Address);
						ReadFrameDimensionData(frameIndex, out x, out y, out w, out h, uOFileMul);
						_animDimensionCache[id] = new Rectangle(x, y, w, h);
						return;
					}
				}
				else if (animationDirection2.IsUOP && Instance.GetBodyAnimationGroup(ref id, ref animGroup, ref hue, isParent: true) is AnimationGroupUop animationGroupUop && (animationGroupUop.FileIndex != 0 || animationGroupUop.CompressedLength != 0 || animationGroupUop.DecompressedLength != 0 || animationGroupUop.Offset != 0))
				{
					int decompressedLength = (int)animationGroupUop.DecompressedLength;
					UOFileUop uOFileUop = _filesUop[animationGroupUop.FileIndex];
					uOFileUop.Seek(animationGroupUop.Offset);
					byte[] array = null;
					Span<byte> span = ((decompressedLength > 1024) ? ((Span<byte>)(array = ArrayPool<byte>.Shared.Rent(decompressedLength))) : stackalloc byte[decompressedLength]);
					Span<byte> span2 = span;
					try
					{
						fixed (byte* ptr = span2)
						{
							ZLib.Decompress(uOFileUop.PositionAddress, (int)animationGroupUop.CompressedLength, 0, (IntPtr)ptr, decompressedLength);
						}
						StackDataReader stackDataReader = new StackDataReader(span2.Slice(0, decompressedLength));
						stackDataReader.Skip(32);
						stackDataReader.ReadInt32LE();
						int num = stackDataReader.ReadInt32LE();
						stackDataReader.Seek(num);
						stackDataReader.Skip(2);
						stackDataReader.ReadInt16LE();
						stackDataReader.Skip(8);
						uint num2 = stackDataReader.ReadUInt32LE();
						stackDataReader.Seek((int)(num + num2));
						stackDataReader.Skip(512);
						x = stackDataReader.ReadInt16LE();
						y = stackDataReader.ReadInt16LE();
						w = stackDataReader.ReadInt16LE();
						h = stackDataReader.ReadInt16LE();
						_animDimensionCache[id] = new Rectangle(x, y, w, h);
						stackDataReader.Release();
						return;
					}
					finally
					{
						if (array != null)
						{
							ArrayPool<byte>.Shared.Return(array);
						}
					}
				}
			}
		}
		x = 0;
		y = 0;
		w = 0;
		h = 0;
	}

	private unsafe void ReadFrameDimensionData(byte frameIndex, out int x, out int y, out int w, out int h, UOFile reader)
	{
		reader.Skip(512);
		long position = reader.Position;
		uint num = reader.ReadUInt();
		if (num != 0 && frameIndex >= num)
		{
			frameIndex = 0;
		}
		if (frameIndex < num)
		{
			uint* ptr = (uint*)(void*)reader.PositionAddress;
			reader.Seek(position + ptr[(int)frameIndex]);
			x = reader.ReadShort();
			y = reader.ReadShort();
			w = reader.ReadShort();
			h = reader.ReadShort();
		}
		else
		{
			x = (y = (w = (h = 0)));
		}
	}
}

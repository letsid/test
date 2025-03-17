using System;
using System.Buffers;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.IO;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using ClassicUO.Utility.Logging;
using ClassicUO.Utility.Platforms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.GameObjects;

internal class Item : Entity
{
	private static readonly QueuedPool<Item> _pool = new QueuedPool<Item>(900, delegate(Item i)
	{
		i.IsDestroyed = false;
		i.Graphic = 0;
		i.Amount = 0;
		i.Container = uint.MaxValue;
		i._isMulti = false;
		i.Layer = Layer.Invalid;
		i.Price = 0u;
		i.UsedLayer = false;
		i._displayedGraphic = null;
		i.X = 0;
		i.Y = 0;
		i.Z = 0;
		i.LightID = 0;
		i.MultiDistanceBonus = 0;
		i.Flags = Flags.None;
		i.WantUpdateMulti = true;
		i.MultiInfo = null;
		i.MultiGraphic = 0;
		i.AlphaHue = 0;
		i.Name = null;
		i.Direction = Direction.North;
		i.AnimIndex = 0;
		i.Hits = 0;
		i.HitsMax = 0;
		i.LastStepTime = 0u;
		i.LastAnimationChangeTime = 0L;
		i.Clear();
		i.IsClicked = false;
		i.IsDamageable = false;
		i.Offset = Vector3.Zero;
		i.Opened = false;
		i.TextContainer?.Clear();
		i.IsFlipped = false;
		i.FrameInfo = Rectangle.Empty;
		i.ObjectHandlesStatus = ObjectHandlesStatus.NONE;
		i.AlphaHue = 0;
		i.AllowedToDraw = true;
		i.ExecuteAnimation = true;
		i.HitsRequest = HitsRequestStatus.None;
	});

	private ushort? _displayedGraphic;

	private bool _isMulti;

	public ushort Amount;

	public uint Container = uint.MaxValue;

	public bool IsDamageable;

	public Layer Layer;

	public byte LightID;

	public Rectangle? MultiInfo;

	public bool Opened;

	public uint Price;

	public bool UsedLayer;

	public bool WantUpdateMulti = true;

	private static EquipConvData? _equipConvData;

	public bool IsCoin
	{
		get
		{
			if (Graphic != 3818 && Graphic != 3821)
			{
				return Graphic == 3824;
			}
			return true;
		}
	}

	public ushort DisplayedGraphic
	{
		get
		{
			if (_displayedGraphic.HasValue)
			{
				return _displayedGraphic.Value;
			}
			if (IsCoin)
			{
				if (Amount > 5)
				{
					return (ushort)(Graphic + 2);
				}
				if (Amount > 1)
				{
					return (ushort)(Graphic + 1);
				}
			}
			else if (IsMulti)
			{
				return MultiGraphic;
			}
			return Graphic;
		}
		set
		{
			_displayedGraphic = value;
		}
	}

	public bool IsLocked
	{
		get
		{
			if ((Flags & Flags.Movable) == 0)
			{
				return ItemData.Weight > 90;
			}
			return false;
		}
	}

	public ushort MultiGraphic { get; private set; }

	public bool IsMulti
	{
		get
		{
			return _isMulti;
		}
		set
		{
			_isMulti = value;
			if (!value)
			{
				MultiDistanceBonus = 0;
				MultiInfo = null;
			}
		}
	}

	public int MultiDistanceBonus { get; private set; }

	public bool IsCorpse => Graphic == 8198;

	public bool OnGround => !SerialHelper.IsValid(Container);

	public uint RootContainer
	{
		get
		{
			Item item = this;
			while (SerialHelper.IsItem(item.Container))
			{
				item = World.Items.Get(item.Container);
				if (item == null)
				{
					return 0u;
				}
			}
			if (!SerialHelper.IsMobile(item.Container))
			{
				return item;
			}
			return item.Container;
		}
	}

	public ref StaticTiles ItemData => ref TileDataLoader.Instance.StaticData[IsMulti ? MultiGraphic : Graphic];

	public bool IsLootable
	{
		get
		{
			if (ItemData.Layer != 11 && ItemData.Layer != 16 && ItemData.Layer != 15)
			{
				return Graphic != 0;
			}
			return false;
		}
	}

	public int InnerTileIndex { get; set; }

	public Item()
		: base(0u)
	{
	}

	public static Item Create(uint serial)
	{
		Item one = _pool.GetOne();
		one.Serial = serial;
		return one;
	}

	public override void Destroy()
	{
		if (base.IsDestroyed)
		{
			return;
		}
		if (Opened)
		{
			UIManager.GetGump<ContainerGump>(Serial)?.Dispose();
			UIManager.GetGump<SpellbookGump>(Serial)?.Dispose();
			UIManager.GetGump<MapGump>(Serial)?.Dispose();
			if (IsCorpse)
			{
				UIManager.GetGump<GridLootGump>(Serial)?.Dispose();
			}
			UIManager.GetGump<BulletinBoardGump>(Serial)?.Dispose();
			UIManager.GetGump<SplitMenuGump>(Serial)?.Dispose();
			Opened = false;
		}
		base.Destroy();
		_pool.ReturnOne(this);
	}

	private unsafe void LoadMulti()
	{
		WantUpdateMulti = false;
		short num = 0;
		short num2 = 0;
		short num3 = 0;
		short num4 = 0;
		if (!World.HouseManager.TryGetHouse(Serial, out var house))
		{
			house = new House(Serial, 0u, isCustom: false);
			World.HouseManager.Add(Serial, house);
		}
		else
		{
			house.ClearComponents();
		}
		ref UOFileIndex validRefEntry = ref MultiLoader.Instance.GetValidRefEntry(Graphic);
		MultiLoader.Instance.File.SetData(validRefEntry.Address, validRefEntry.FileSize);
		bool flag = false;
		if (MultiLoader.Instance.IsUOP)
		{
			if (validRefEntry.Length > 0 && validRefEntry.DecompressedLength > 0)
			{
				MultiLoader.Instance.File.Seek(validRefEntry.Offset);
				byte[] array = null;
				Span<byte> span = ((validRefEntry.DecompressedLength > 1024) ? ((Span<byte>)(array = ArrayPool<byte>.Shared.Rent(validRefEntry.DecompressedLength, zero: true))) : stackalloc byte[validRefEntry.DecompressedLength]);
				Span<byte> span2 = span;
				try
				{
					fixed (byte* ptr = span2)
					{
						ZLib.Decompress(MultiLoader.Instance.File.PositionAddress, validRefEntry.Length, 0, (IntPtr)ptr, validRefEntry.DecompressedLength);
						StackDataReader stackDataReader = new StackDataReader(span2.Slice(0, validRefEntry.DecompressedLength));
						stackDataReader.Skip(4);
						int num5 = stackDataReader.ReadInt32LE();
						int num6 = sizeof(MultiBlockNew);
						for (int i = 0; i < num5; i++)
						{
							MultiBlockNew* ptr2 = (MultiBlockNew*)(void*)(stackDataReader.PositionAddress + i * num6);
							if (ptr2->Unknown != 0)
							{
								stackDataReader.Skip((int)(ptr2->Unknown * 4));
							}
							if (ptr2->X < num)
							{
								num = ptr2->X;
							}
							if (ptr2->X > num3)
							{
								num3 = ptr2->X;
							}
							if (ptr2->Y < num2)
							{
								num2 = ptr2->Y;
							}
							if (ptr2->Y > num4)
							{
								num4 = ptr2->Y;
							}
							if (ptr2->Flags == 0 || ptr2->Flags == 256)
							{
								Multi multi = Multi.Create(ptr2->ID);
								multi.X = (ushort)(X + ptr2->X);
								multi.Y = (ushort)(Y + ptr2->Y);
								multi.Z = (sbyte)(Z + ptr2->Z);
								multi.UpdateScreenPosition();
								multi.MultiOffsetX = ptr2->X;
								multi.MultiOffsetY = ptr2->Y;
								multi.MultiOffsetZ = ptr2->Z;
								multi.Hue = Hue;
								multi.AlphaHue = byte.MaxValue;
								multi.IsCustom = false;
								multi.State = CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_DONT_REMOVE;
								multi.IsMovable = ItemData.IsMultiMovable;
								multi.AddToTile();
								house.Components.Add(multi);
								if (multi.ItemData.IsMultiMovable)
								{
									flag = true;
								}
							}
							else if (i == 0)
							{
								MultiGraphic = ptr2->ID;
							}
						}
						stackDataReader.Release();
					}
				}
				finally
				{
					if (array != null)
					{
						ArrayPool<byte>.Shared.Return(array);
					}
				}
			}
			else
			{
				Log.Warn($"[MultiCollection.uop] invalid entry (0x{Graphic:X4})");
			}
		}
		else
		{
			int num7 = validRefEntry.Length / MultiLoader.Instance.Offset;
			MultiLoader.Instance.File.Seek(validRefEntry.Offset);
			for (int j = 0; j < num7; j++)
			{
				MultiBlock* ptr3 = (MultiBlock*)(void*)(MultiLoader.Instance.File.PositionAddress + j * MultiLoader.Instance.Offset);
				if (ptr3->X < num)
				{
					num = ptr3->X;
				}
				if (ptr3->X > num3)
				{
					num3 = ptr3->X;
				}
				if (ptr3->Y < num2)
				{
					num2 = ptr3->Y;
				}
				if (ptr3->Y > num4)
				{
					num4 = ptr3->Y;
				}
				if (ptr3->Flags != 0)
				{
					Multi multi2 = Multi.Create(ptr3->ID);
					multi2.X = (ushort)(X + ptr3->X);
					multi2.Y = (ushort)(Y + ptr3->Y);
					multi2.Z = (sbyte)(Z + ptr3->Z);
					multi2.UpdateScreenPosition();
					multi2.MultiOffsetX = ptr3->X;
					multi2.MultiOffsetY = ptr3->Y;
					multi2.MultiOffsetZ = ptr3->Z;
					if (Hue != 0)
					{
						multi2.Hue = Hue;
					}
					else
					{
						multi2.Hue = (ushort)ptr3->Hue;
					}
					multi2.AlphaHue = byte.MaxValue;
					multi2.IsCustom = false;
					multi2.State = CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_DONT_REMOVE;
					multi2.IsMovable = ItemData.IsMultiMovable;
					multi2.AddToTile();
					house.Components.Add(multi2);
					if (multi2.ItemData.IsMultiMovable)
					{
						flag = true;
					}
				}
				else if (j == 0)
				{
					MultiGraphic = ptr3->ID;
					Hue = (ushort)ptr3->Hue;
				}
			}
		}
		MultiInfo = new Rectangle
		{
			X = num,
			Y = num2,
			Width = num3,
			Height = num4
		};
		if (flag)
		{
			foreach (Multi component in house.Components)
			{
				component.IsMovable = flag;
			}
		}
		MultiDistanceBonus = Math.Max(Math.Max(Math.Abs(num), num3), Math.Max(Math.Abs(num2), num4));
		house.Bounds = MultiInfo.Value;
		UIManager.GetGump<MiniMapGump>(null)?.RequestUpdateContents();
		if (World.HouseManager.EntityIntoHouse(Serial, World.Player))
		{
			Client.Game.GetScene<GameScene>()?.UpdateMaxDrawZ(force: true);
		}
		BoatMovingManager.ClearSteps(Serial);
	}

	public override void CheckGraphicChange(byte animIndex = 0)
	{
		if (!IsMulti)
		{
			if (!IsCorpse)
			{
				AllowedToDraw = GameObject.CanBeDrawn(Graphic);
				return;
			}
			AnimIndex = 99;
			if ((base.Direction & Direction.Running) != 0)
			{
				UsedLayer = true;
				base.Direction &= ~Direction.Running;
			}
			else
			{
				UsedLayer = false;
			}
			Layer = (Layer)base.Direction;
			AllowedToDraw = true;
		}
		else if (WantUpdateMulti)
		{
			UoAssist.SignalAddMulti((ushort)(Graphic | 0x4000), X, Y);
			if (MultiDistanceBonus == 0 || World.HouseManager.IsHouseInRange(Serial, World.ClientViewRange))
			{
				LoadMulti();
				AllowedToDraw = MultiGraphic > 2;
			}
		}
	}

	public override void Update(double totalTime, double frameTime)
	{
		if (!base.IsDestroyed)
		{
			base.Update(totalTime, frameTime);
			ProcessAnimation(out var _);
		}
	}

	public override ushort GetGraphicForAnimation()
	{
		ushort num = Graphic;
		if (Layer == Layer.Mount)
		{
			switch (num)
			{
			case 16016:
				num = 276;
				break;
			case 16017:
				num = 277;
				break;
			case 16018:
				num = 284;
				break;
			case 16020:
				num = 243;
				break;
			case 16021:
				num = 169;
				break;
			case 16023:
				num = 195;
				break;
			case 16024:
				num = 194;
				break;
			case 16026:
				num = 193;
				break;
			case 16027:
			case 16029:
				return 192;
			case 16028:
				return 191;
			case 16030:
				num = 190;
				break;
			case 16032:
				num = 226;
				break;
			case 16033:
				num = 228;
				break;
			case 16034:
				num = 204;
				break;
			case 16035:
				num = 210;
				break;
			case 16036:
				num = 218;
				break;
			case 16037:
				num = 219;
				break;
			case 16038:
				num = 220;
				break;
			case 16039:
				num = 116;
				break;
			case 16040:
				num = 117;
				break;
			case 16041:
				num = 114;
				break;
			case 16042:
				num = 115;
				break;
			case 16043:
				num = 170;
				break;
			case 16044:
				num = 171;
				break;
			case 16045:
				num = 132;
				break;
			case 16047:
				num = 120;
				break;
			case 16048:
				num = 121;
				break;
			case 16049:
				num = 119;
				break;
			case 16050:
				num = 118;
				break;
			case 16051:
				num = 144;
				break;
			case 16052:
				num = 122;
				break;
			case 16053:
				num = 177;
				break;
			case 16054:
				num = 178;
				break;
			case 16055:
				num = 179;
				break;
			case 16056:
				num = 188;
				break;
			case 16058:
				num = 187;
				break;
			case 16059:
				num = 793;
				break;
			case 16060:
				num = 791;
				break;
			case 16061:
				num = 794;
				break;
			case 16062:
				num = 799;
				break;
			case 16067:
				num = 724;
				break;
			case 16078:
				num = 1434;
				break;
			case 16069:
			case 16186:
				num = 213;
				break;
			case 16070:
				num = 432;
				break;
			case 16071:
				num = 1254;
				break;
			case 16072:
				num = 1255;
				break;
			case 16073:
				num = 1069;
				break;
			case 16074:
				num = 1401;
				break;
			case 16076:
				num = 1410;
				break;
			case 16081:
				num = 1510;
				break;
			case 16075:
				num = 1407;
				break;
			case 16080:
				num = 1441;
				break;
			case 16082:
				num = 1526;
				break;
			case 16077:
				num = 1408;
				break;
			case 16079:
				num = 1440;
				break;
			case 16083:
				num = 1527;
				break;
			}
			if (ItemData.AnimID != 0)
			{
				num = ItemData.AnimID;
			}
		}
		else if (IsCorpse)
		{
			return Amount;
		}
		return num;
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
		if (OnGround)
		{
			Point realScreenPosition = RealScreenPosition;
			Rectangle realArtBounds = ArtLoader.Instance.GetRealArtBounds(Graphic);
			realScreenPosition.Y -= realArtBounds.Height >> 1;
			realScreenPosition.X += (int)base.CumulativeOffset.X + 22;
			realScreenPosition.Y += (int)(base.CumulativeOffset.Y - base.CumulativeOffset.Z) + 22;
			realScreenPosition = Client.Game.Scene.Camera.WorldToScreen(realScreenPosition);
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
			return;
		}
		while (textObject != null)
		{
			if (textObject.RenderedText != null && !textObject.RenderedText.IsDestroyed && (num != 0 || textObject.Time >= Time.Ticks))
			{
				textObject.OffsetY = num;
				num += textObject.RenderedText.Height;
				textObject.RealScreenPosition.X = textObject.X - (textObject.RenderedText.Width >> 1);
				textObject.RealScreenPosition.Y = textObject.Y - num;
			}
			textObject = (TextObject)textObject.Previous;
		}
	}

	public override void ProcessAnimation(out byte dir, bool evalutate = false)
	{
		dir = 0;
		if (!IsCorpse)
		{
			return;
		}
		dir = (byte)Layer;
		if (LastAnimationChangeTime >= Time.Ticks)
		{
			return;
		}
		byte b = (byte)((uint)AnimIndex + (ExecuteAnimation ? 1u : 0u));
		ushort graphic = GetGraphicForAnimation();
		bool mirror = false;
		AnimationsLoader.Instance.GetAnimDirection(ref dir, ref mirror);
		if (graphic < 4096 && dir < 5)
		{
			byte group = AnimationsLoader.Instance.GetDieGroupIndex(graphic, UsedLayer);
			ushort hue = 0;
			AnimationDirection animDir = AnimationsLoader.Instance.GetCorpseAnimationGroup(ref graphic, ref group, ref hue).Direction[dir];
			if (animDir.FrameCount == 0 || animDir.SpriteInfos == null)
			{
				AnimationsLoader.Instance.LoadAnimationFrames(graphic, group, dir, ref animDir);
			}
			if ((animDir.Address != 0L && animDir.Size != 0) || animDir.IsUOP)
			{
				int frameCount = animDir.FrameCount;
				if (b >= frameCount)
				{
					b = (byte)(frameCount - 1);
				}
				AnimIndex = (byte)(b % animDir.FrameCount);
			}
		}
		LastAnimationChangeTime = Time.Ticks + 80;
	}

	public override bool Draw(UltimaBatcher2D batcher, int posX, int posY, float depth)
	{
		if (!AllowedToDraw || base.IsDestroyed)
		{
			return false;
		}
		posX += (int)base.CumulativeOffset.X;
		posY += (int)(base.CumulativeOffset.Y + base.CumulativeOffset.Z);
		float alpha = (float)(int)AlphaHue / 255f;
		Vector3 hueVector;
		if (IsCorpse)
		{
			hueVector = ShaderHueTranslator.GetHueVector(0, partial: false, alpha);
			return DrawCorpse(batcher, posX, posY, hueVector, depth);
		}
		ushort hue = Hue;
		ushort num = DisplayedGraphic;
		bool partial = ItemData.IsPartialHue;
		if (OnGround)
		{
			if (ItemData.IsAnimated && ProfileManager.CurrentProfile.FieldsType == 2)
			{
				if (StaticFilters.IsFireField(Graphic))
				{
					num = 6182;
					hue = 32;
				}
				else if (StaticFilters.IsParalyzeField(Graphic))
				{
					num = 6182;
					hue = 88;
				}
				else if (StaticFilters.IsEnergyField(Graphic))
				{
					num = 6182;
					hue = 112;
				}
				else if (StaticFilters.IsPoisonField(Graphic))
				{
					num = 6182;
					hue = 68;
				}
				else if (StaticFilters.IsWallOfStone(Graphic))
				{
					num = 6182;
					hue = 906;
				}
			}
			if (ItemData.IsContainer && SelectedObject.SelectedContainer == this)
			{
				hue = 53;
				partial = false;
			}
		}
		if (ProfileManager.CurrentProfile.HighlightGameObjects && SelectedObject.LastObject == this)
		{
			hue = ProfileManager.CurrentProfile.HighlightGameObjectsColor;
			partial = false;
		}
		else if (ProfileManager.CurrentProfile.NoColorObjectsOutOfRange && base.Distance > World.ClientViewRange)
		{
			hue = 907;
		}
		else if (World.Player.IsDead && ProfileManager.CurrentProfile.EnableBlackWhiteEffect)
		{
			hue = 910;
		}
		else if (!IsLocked && !IsMulti && SelectedObject.LastObject == this)
		{
			hue = 53;
		}
		else if (base.IsHidden)
		{
			hue = 910;
		}
		hueVector = ShaderHueTranslator.GetHueVector(hue, partial, alpha);
		if (!IsMulti && !IsCoin && Amount > 1 && ItemData.IsStackable)
		{
			GameObject.DrawStaticAnimated(batcher, num, posX - 5, posY - 5, hueVector, shadow: false, depth + (float)InnerTileIndex / 100f);
		}
		if (ItemData.IsLight || (num >= 15874 && num <= 15883) || (num >= 14612 && num <= 14633))
		{
			Client.Game.GetScene<GameScene>().AddLight(this, this, posX + 22, posY + 22);
		}
		if (!SerialHelper.IsValid(Serial) && IsMulti && TargetManager.TargetingState == CursorTarget.MultiPlacement)
		{
			hueVector.Z = 0.5f;
		}
		GameObject.DrawStaticAnimated(batcher, num, posX, posY, hueVector, shadow: false, depth + (float)InnerTileIndex / 100f);
		return true;
	}

	private bool DrawCorpse(UltimaBatcher2D batcher, int posX, int posY, Vector3 hueVec, float depth)
	{
		if (base.IsDestroyed || World.CorpseManager.Exists(Serial, 0u))
		{
			return false;
		}
		posX += 22;
		posY += 22;
		byte dir = (byte)(Layer & (Layer)127 & Layer.Gloves);
		AnimationsLoader.Instance.GetAnimDirection(ref dir, ref IsFlipped);
		byte animIndex = AnimIndex;
		ushort graphic = GetGraphicForAnimation();
		AnimationsLoader.Instance.ConvertBodyIfNeeded(ref graphic);
		byte dieGroupIndex = AnimationsLoader.Instance.GetDieGroupIndex(graphic, UsedLayer);
		bool ishuman = ClassicUO.Utility.MathHelper.InRange(Amount, 400, 403) || ClassicUO.Utility.MathHelper.InRange(Amount, 183, 186) || ClassicUO.Utility.MathHelper.InRange(Amount, 605, 608) || ClassicUO.Utility.MathHelper.InRange(Amount, 666, 667) || ClassicUO.Utility.MathHelper.InRange(Amount, 694, 695) || Amount == 987 || Amount == 991 || Amount == 994 || Amount == 744 || Amount == 745;
		DrawLayer(batcher, posX, posY, this, Layer.Invalid, animIndex, ishuman, Hue, IsFlipped, hueVec.Z, dieGroupIndex, dir, hueVec, depth);
		for (int i = 1; i < 23; i++)
		{
			Layer layer = LayerOrder.UsedLayers[dir, i];
			DrawLayer(batcher, posX, posY, this, layer, animIndex, ishuman, 0, IsFlipped, hueVec.Z, dieGroupIndex, dir, hueVec, depth);
		}
		return true;
	}

	private static void DrawLayer(UltimaBatcher2D batcher, int posX, int posY, Item owner, Layer layer, byte animIndex, bool ishuman, ushort color, bool flipped, float alpha, byte animGroup, byte dir, Vector3 hueVec, float depth)
	{
		_equipConvData = null;
		bool partial = false;
		int num = posY;
		ushort graphic;
		if (layer == Layer.Invalid)
		{
			graphic = owner.GetGraphicForAnimation();
		}
		else
		{
			if (!ishuman)
			{
				return;
			}
			Item item = owner.FindItemByLayer(layer);
			if (item == null)
			{
				return;
			}
			graphic = item.ItemData.AnimID;
			partial = item.ItemData.IsPartialHue;
			if (AnimationsLoader.Instance.EquipConversions.TryGetValue(graphic, out var value) && value.TryGetValue(graphic, out var value2))
			{
				_equipConvData = value2;
				graphic = value2.Graphic;
			}
			color = item.Hue;
		}
		ushort hue = 0;
		AnimationGroup obj = ((layer == Layer.Invalid) ? AnimationsLoader.Instance.GetCorpseAnimationGroup(ref graphic, ref animGroup, ref hue) : AnimationsLoader.Instance.GetBodyAnimationGroup(ref graphic, ref animGroup, ref hue));
		if (color == 0)
		{
			color = hue;
		}
		AnimationDirection animDir = obj.Direction[dir];
		if (animDir == null || (animDir.IsUOP && !owner.IsCorpse) || ((animDir.FrameCount == 0 || animDir.SpriteInfos == null) && !AnimationsLoader.Instance.LoadAnimationFrames(graphic, animGroup, dir, ref animDir)))
		{
			return;
		}
		int frameCount = animDir.FrameCount;
		if (frameCount > 0 && animIndex >= frameCount)
		{
			animIndex = (byte)(frameCount - 1);
		}
		if (animIndex >= animDir.FrameCount)
		{
			return;
		}
		ref SpriteInfo reference = ref animDir.SpriteInfos[animIndex];
		if (reference.Texture == null)
		{
			return;
		}
		posX = ((!flipped) ? (posX - reference.Center.X) : (posX - (reference.UV.Width - reference.Center.X)));
		posY -= reference.UV.Height + reference.Center.Y;
		if (color == 0)
		{
			if ((color & 0x8000) != 0)
			{
				partial = true;
				color &= 0x7FFF;
			}
			if (color == 0 && _equipConvData.HasValue)
			{
				color = _equipConvData.Value.Color;
				partial = false;
			}
		}
		if (ProfileManager.CurrentProfile.NoColorObjectsOutOfRange && owner.Distance > World.ClientViewRange)
		{
			hueVec = ShaderHueTranslator.GetHueVector(908, partial: false, 1f);
		}
		else if (World.Player.IsDead && ProfileManager.CurrentProfile.EnableBlackWhiteEffect)
		{
			hueVec = ShaderHueTranslator.GetHueVector(911, partial: false, 1f);
		}
		else
		{
			if (ProfileManager.CurrentProfile.GridLootType > 0 && SelectedObject.CorpseObject == owner)
			{
				color = 52;
			}
			else if (ProfileManager.CurrentProfile.HighlightGameObjects && SelectedObject.LastObject == owner)
			{
				color = ProfileManager.CurrentProfile.HighlightGameObjectsColor;
			}
			hueVec = ShaderHueTranslator.GetHueVector(color, partial, alpha);
		}
		Vector2 position = new Vector2(posX, posY);
		Rectangle uV = reference.UV;
		int num2 = (int)Math.Min(reference.UV.Height, (float)num - position.Y);
		int num3 = reference.UV.Height - num2;
		int num4 = 1;
		uV.Height = num2;
		if (num3 > 0)
		{
			num4 += (int)Math.Ceiling((float)num3 / 44f);
		}
		int num5 = num2;
		for (int i = 0; i < num4; i++)
		{
			batcher.Draw(reference.Texture, position, uV, hueVec, 0f, Vector2.Zero, 1f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, depth + 1f + (float)(i * 200));
			position.Y += uV.Height;
			uV.Y += uV.Height;
			uV.Height = Math.Min(44, reference.UV.Height - num5);
			num5 += uV.Height;
		}
	}

	public override bool CheckMouseSelection()
	{
		if (!IsCorpse)
		{
			if (SelectedObject.Object == this || TargetManager.TargetingState == CursorTarget.MultiPlacement)
			{
				return false;
			}
			if (FoliageIndex != -1 && Client.Game.GetScene<GameScene>().FoliageIndex == FoliageIndex)
			{
				return false;
			}
			ushort num = DisplayedGraphic;
			if (OnGround && ItemData.IsAnimated && ProfileManager.CurrentProfile.FieldsType == 2 && (StaticFilters.IsFireField(Graphic) || StaticFilters.IsParalyzeField(Graphic) || StaticFilters.IsEnergyField(Graphic) || StaticFilters.IsPoisonField(Graphic) || StaticFilters.IsWallOfStone(Graphic)))
			{
				num = 6182;
			}
			if (ArtLoader.Instance.GetStaticTexture(num, out var _) != null)
			{
				ref UOFileIndex validRefEntry = ref ArtLoader.Instance.GetValidRefEntry(num + 16384);
				Point realScreenPosition = RealScreenPosition;
				realScreenPosition.X += (int)base.CumulativeOffset.X;
				realScreenPosition.Y += (int)(base.CumulativeOffset.Y + base.CumulativeOffset.Z);
				realScreenPosition.X -= validRefEntry.Width;
				realScreenPosition.Y -= validRefEntry.Height;
				if (ArtLoader.Instance.PixelCheck(num, SelectedObject.TranslatedMousePositionByViewport.X - realScreenPosition.X, SelectedObject.TranslatedMousePositionByViewport.Y - realScreenPosition.Y))
				{
					return true;
				}
				if (!IsMulti && !IsCoin && Amount > 1 && ItemData.IsStackable && ArtLoader.Instance.PixelCheck(num, SelectedObject.TranslatedMousePositionByViewport.X - realScreenPosition.X + 5, SelectedObject.TranslatedMousePositionByViewport.Y - realScreenPosition.Y + 5))
				{
					return true;
				}
			}
		}
		else
		{
			if (!SerialHelper.IsValid(Serial))
			{
				return false;
			}
			if (SelectedObject.Object == this)
			{
				return true;
			}
			Point realScreenPosition2 = RealScreenPosition;
			realScreenPosition2.X += 22;
			realScreenPosition2.Y += 22;
			byte dir = (byte)(Layer & (Layer)127 & Layer.Gloves);
			AnimationsLoader.Instance.GetAnimDirection(ref dir, ref IsFlipped);
			byte animIndex = AnimIndex;
			bool flag = ClassicUO.Utility.MathHelper.InRange(Amount, 400, 403) || ClassicUO.Utility.MathHelper.InRange(Amount, 183, 186) || ClassicUO.Utility.MathHelper.InRange(Amount, 605, 608) || ClassicUO.Utility.MathHelper.InRange(Amount, 666, 667) || ClassicUO.Utility.MathHelper.InRange(Amount, 694, 695) || Amount == 987 || Amount == 991 || Amount == 994 || Amount == 744 || Amount == 745;
			for (int i = -1; i < 23; i++)
			{
				Layer layer = ((i != -1) ? LayerOrder.UsedLayers[dir, i] : Layer.Invalid);
				ushort graphic;
				if (layer == Layer.Invalid)
				{
					graphic = GetGraphicForAnimation();
					AnimationsLoader.Instance.ConvertBodyIfNeeded(ref graphic);
				}
				else
				{
					if (!flag)
					{
						continue;
					}
					Item item = FindItemByLayer(layer);
					if (item == null)
					{
						continue;
					}
					graphic = item.ItemData.AnimID;
					if (AnimationsLoader.Instance.EquipConversions.TryGetValue(graphic, out var value) && value.TryGetValue(graphic, out var value2))
					{
						_equipConvData = value2;
						graphic = value2.Graphic;
					}
				}
				byte animGroup = AnimationsLoader.Instance.GetDieGroupIndex(graphic, UsedLayer);
				if (GetTexture(ref graphic, ref animGroup, ref animIndex, dir, out var spriteInfo, out var isUOP))
				{
					int num2 = realScreenPosition2.X - (IsFlipped ? (spriteInfo.UV.Width - spriteInfo.Center.X) : spriteInfo.Center.X);
					int num3 = realScreenPosition2.Y - (spriteInfo.UV.Height + spriteInfo.Center.Y);
					if (AnimationsLoader.Instance.PixelCheck(graphic, animGroup, dir, isUOP, animIndex, IsFlipped ? (num2 + spriteInfo.UV.Width - SelectedObject.TranslatedMousePositionByViewport.X) : (SelectedObject.TranslatedMousePositionByViewport.X - num2), SelectedObject.TranslatedMousePositionByViewport.Y - num3))
					{
						return true;
					}
				}
			}
		}
		return false;
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
}

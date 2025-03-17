using System.Collections.Generic;
using System.Linq;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Map;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.IO.Audio;
using ClassicUO.Utility;
using ClassicUO.Utility.Platforms;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game;

internal static class World
{
	private static readonly EffectManager _effectManager = new EffectManager();

	private static readonly List<uint> _toRemove = new List<uint>();

	private static uint _timeToDelete;

	public static Point RangeSize;

	public static PlayerMobile Player;

	public static HouseCustomizationManager CustomHouseManager;

	public static WorldMapEntityManager WMapManager = new WorldMapEntityManager();

	public static ActiveSpellIconsManager ActiveSpellIcons = new ActiveSpellIconsManager();

	public static uint LastObject;

	public static uint ObjectToRemove;

	public static ObjectPropertiesListManager OPL { get; } = new ObjectPropertiesListManager();

	public static CorpseManager CorpseManager { get; } = new CorpseManager();

	public static PartyManager Party { get; } = new PartyManager();

	public static HouseManager HouseManager { get; } = new HouseManager();

	public static Dictionary<uint, Item> Items { get; } = new Dictionary<uint, Item>();

	public static Dictionary<uint, Mobile> Mobiles { get; } = new Dictionary<uint, Mobile>();

	public static ClassicUO.Game.Map.Map Map { get; private set; }

	public static byte ClientViewRange { get; set; } = 31;

	public static bool SkillsRequested { get; set; }

	public static Season Season { get; private set; } = Season.Summer;

	public static Season OldSeason { get; set; } = Season.Summer;

	public static int OldMusicIndex { get; set; }

	public static WorldTextManager WorldTextManager { get; } = new WorldTextManager();

	public static JournalManager Journal { get; } = new JournalManager();

	public static AdditionalJournalManager AdditionalJournal { get; } = new AdditionalJournalManager();

	public static Dictionary<uint, string> AdditionalJournalFilterDict { get; } = new Dictionary<uint, string>();

	public static int MapIndex
	{
		get
		{
			return Map?.Index ?? (-1);
		}
		set
		{
			if (MapIndex == value)
			{
				return;
			}
			InternalMapChangeClear(noplayer: true);
			if (value < 0 && Map != null)
			{
				Map.Destroy();
				Map = null;
				return;
			}
			if (Map != null)
			{
				if (MapIndex >= 0)
				{
					Map.Destroy();
				}
				ushort x = Player.X;
				ushort y = Player.Y;
				sbyte z = Player.Z;
				Map = null;
				if (value >= Constants.MAPS_COUNT)
				{
					value = 0;
				}
				Map = new ClassicUO.Game.Map.Map(value);
				Player.X = x;
				Player.Y = y;
				Player.Z = z;
				Player.UpdateScreenPosition();
				Player.AddToTile();
				Player.ClearSteps();
			}
			else
			{
				Map = new ClassicUO.Game.Map.Map(value);
			}
			if (UIManager.GameCursor != null)
			{
				UIManager.GameCursor.Graphic = ushort.MaxValue;
			}
			UoAssist.SignalMapChanged(value);
		}
	}

	public static bool InGame
	{
		get
		{
			if (Player != null)
			{
				return Map != null;
			}
			return false;
		}
	}

	public static IsometricLight Light { get; } = new IsometricLight
	{
		Overall = 0,
		Personal = 0,
		RealOverall = 0,
		RealPersonal = 0
	};

	public static LockedFeatures ClientLockedFeatures { get; } = new LockedFeatures();

	public static ClientFeatures ClientFeatures { get; } = new ClientFeatures();

	public static string ServerName { get; set; }

	public static void ChangeSeason(Season season, int music)
	{
		bool num = Season != season;
		Season = season;
		if (num)
		{
			CustomSeasonManager.InitializeSeason();
		}
		foreach (Chunk usedChunk in Map.GetUsedChunks())
		{
			GameObject[,] tiles = usedChunk.Tiles;
			int upperBound = tiles.GetUpperBound(0);
			int upperBound2 = tiles.GetUpperBound(1);
			for (int i = tiles.GetLowerBound(0); i <= upperBound; i++)
			{
				for (int j = tiles.GetLowerBound(1); j <= upperBound2; j++)
				{
					for (GameObject gameObject = tiles[i, j]; gameObject != null; gameObject = gameObject.TNext)
					{
						gameObject.UpdateGraphicBySeason();
					}
				}
			}
		}
		UOMusic currentMusic = Client.Game.Scene.Audio.GetCurrentMusic();
		if (currentMusic == null || currentMusic.Index == Client.Game.Scene.Audio.LoginMusicIndex)
		{
			Client.Game.Scene.Audio.PlayMusic(music);
		}
	}

	public static void Update(double totalTime, double frameTime)
	{
		if (!(Player != null))
		{
			return;
		}
		if (SerialHelper.IsValid(ObjectToRemove))
		{
			Item item = Items.Get(ObjectToRemove);
			ObjectToRemove = 0u;
			if (item != null)
			{
				Entity entity = Get(item.Container);
				RemoveItem(item, forceRemove: true);
				if (item.Layer == Layer.OneHanded || item.Layer == Layer.TwoHanded)
				{
					Player.UpdateAbilities();
				}
				if (entity != null)
				{
					if (SerialHelper.IsMobile(entity.Serial))
					{
						UIManager.GetGump<PaperDollGump>(entity.Serial)?.RequestUpdateContents();
					}
					else if (SerialHelper.IsItem(entity.Serial))
					{
						UIManager.GetGump<ContainerGump>(entity.Serial)?.RequestUpdateContents();
						if (entity.Graphic == 8198)
						{
							UIManager.GetGump<GridLootGump>(entity)?.RequestUpdateContents();
						}
					}
				}
			}
		}
		bool flag = _timeToDelete < Time.Ticks;
		if (flag)
		{
			_timeToDelete = Time.Ticks + 50;
		}
		foreach (Mobile value in Mobiles.Values)
		{
			value.Update(totalTime, frameTime);
			if (flag && value.Distance > ClientViewRange)
			{
				RemoveMobile(value);
			}
			if (value.IsDestroyed)
			{
				_toRemove.Add(value.Serial);
			}
			else if (value.NotorietyFlag == NotorietyFlag.Ally)
			{
				WMapManager.AddOrUpdate(value.Serial, value.X, value.Y, ClassicUO.Utility.MathHelper.PercetangeOf(value.Hits, value.HitsMax), MapIndex, isguild: true, value.Name);
			}
			else if (Party.Leader != 0 && Party.Contains(value))
			{
				WMapManager.AddOrUpdate(value.Serial, value.X, value.Y, ClassicUO.Utility.MathHelper.PercetangeOf(value.Hits, value.HitsMax), MapIndex, isguild: false, value.Name);
			}
		}
		if (_toRemove.Count != 0)
		{
			for (int i = 0; i < _toRemove.Count; i++)
			{
				Mobiles.Remove(_toRemove[i]);
			}
			_toRemove.Clear();
		}
		foreach (Item value2 in Items.Values)
		{
			value2.Update(totalTime, frameTime);
			if (flag && value2.OnGround && value2.Distance > ClientViewRange)
			{
				if (value2.IsMulti)
				{
					if (HouseManager.TryToRemove(value2, ClientViewRange))
					{
						RemoveItem(value2);
					}
				}
				else
				{
					RemoveItem(value2);
				}
			}
			if (value2.IsDestroyed)
			{
				_toRemove.Add(value2.Serial);
			}
		}
		if (_toRemove.Count != 0)
		{
			for (int j = 0; j < _toRemove.Count; j++)
			{
				Items.Remove(_toRemove[j]);
			}
			_toRemove.Clear();
		}
		_effectManager.Update(totalTime, frameTime);
		WorldTextManager.Update(totalTime, frameTime);
		WMapManager.RemoveUnupdatedWEntity();
	}

	public static bool Contains(uint serial)
	{
		if (SerialHelper.IsItem(serial))
		{
			return Items.Contains(serial);
		}
		if (SerialHelper.IsMobile(serial))
		{
			return Mobiles.Contains(serial);
		}
		return false;
	}

	public static Entity Get(uint serial)
	{
		Entity entity;
		if (SerialHelper.IsMobile(serial))
		{
			entity = Mobiles.Get(serial);
			if (entity == null)
			{
				entity = Items.Get(serial);
			}
		}
		else
		{
			entity = Items.Get(serial);
			if (entity == null)
			{
				entity = Mobiles.Get(serial);
			}
		}
		if (entity != null && entity.IsDestroyed)
		{
			entity = null;
		}
		return entity;
	}

	public static Item GetOrCreateItem(uint serial)
	{
		Item item = Items.Get(serial);
		if (item != null && item.IsDestroyed)
		{
			Items.Remove(serial);
			item = null;
		}
		if (item == null)
		{
			item = Item.Create(serial);
			Items.Add(item);
		}
		return item;
	}

	public static Mobile GetOrCreateMobile(uint serial)
	{
		Mobile mobile = Mobiles.Get(serial);
		if (mobile != null && mobile.IsDestroyed)
		{
			Mobiles.Remove(serial);
			mobile = null;
		}
		if (mobile == null)
		{
			mobile = Mobile.Create(serial);
			Mobiles.Add(mobile);
		}
		return mobile;
	}

	public static void RemoveItemFromContainer(uint serial)
	{
		Item item = Items.Get(serial);
		if (item != null)
		{
			RemoveItemFromContainer(item);
		}
	}

	public static void RemoveItemFromContainer(Item obj)
	{
		uint container = obj.Container;
		if (container != uint.MaxValue)
		{
			if (SerialHelper.IsMobile(container))
			{
				UIManager.GetGump<PaperDollGump>(container)?.RequestUpdateContents();
			}
			else if (SerialHelper.IsItem(container))
			{
				UIManager.GetGump<ContainerGump>(container)?.RequestUpdateContents();
			}
			Entity entity = Get(container);
			if (entity != null)
			{
				entity.Remove(obj);
			}
			obj.Container = uint.MaxValue;
		}
		obj.Next = null;
		obj.Previous = null;
		obj.RemoveFromTile();
	}

	public static bool RemoveItem(uint serial, bool forceRemove = false)
	{
		Item item = Items.Get(serial);
		if (item == null || item.IsDestroyed)
		{
			return false;
		}
		LinkedObject linkedObject = item.Items;
		RemoveItemFromContainer(item);
		while (linkedObject != null)
		{
			LinkedObject next = linkedObject.Next;
			RemoveItem(linkedObject as Item, forceRemove);
			linkedObject = next;
		}
		OPL.Remove(serial);
		item.Destroy();
		if (forceRemove)
		{
			Items.Remove(serial);
		}
		return true;
	}

	public static bool RemoveMobile(uint serial, bool forceRemove = false)
	{
		Mobile mobile = Mobiles.Get(serial);
		if (mobile == null || mobile.IsDestroyed)
		{
			return false;
		}
		LinkedObject linkedObject = mobile.Items;
		while (linkedObject != null)
		{
			LinkedObject next = linkedObject.Next;
			RemoveItem(linkedObject as Item, forceRemove);
			linkedObject = next;
		}
		OPL.Remove(serial);
		mobile.Destroy();
		if (forceRemove)
		{
			Mobiles.Remove(serial);
		}
		return true;
	}

	public static void SpawnEffect(GraphicEffectType type, uint source, uint target, ushort graphic, ushort hue, ushort srcX, ushort srcY, sbyte srcZ, ushort targetX, ushort targetY, sbyte targetZ, byte speed, int duration, bool fixedDir, bool doesExplode, bool hasparticles, GraphicEffectBlendMode blendmode)
	{
		_effectManager.CreateEffect(type, source, target, graphic, hue, srcX, srcY, srcZ, targetX, targetY, targetZ, speed, duration, fixedDir, doesExplode, hasparticles, blendmode);
	}

	public static uint FindNearest(ScanTypeObject scanType)
	{
		int num = int.MaxValue;
		uint result = 0u;
		if (scanType == ScanTypeObject.Objects || scanType == ScanTypeObject.PlayerCorpse)
		{
			foreach (Item value in Items.Values)
			{
				if (!value.IsMulti && !value.IsDestroyed && value.OnGround && (scanType != ScanTypeObject.PlayerCorpse || (value.IsCorpse && value.IsPlayerItem != 0)) && value.Distance < num)
				{
					num = value.Distance;
					result = value.Serial;
				}
			}
		}
		else
		{
			foreach (Mobile value2 in Mobiles.Values)
			{
				if (value2.IsDestroyed || value2 == Player)
				{
					continue;
				}
				switch (scanType)
				{
				case ScanTypeObject.Party:
					if (!Party.Contains(value2))
					{
						continue;
					}
					break;
				case ScanTypeObject.Followers:
					if (!value2.IsRenamable || value2.NotorietyFlag == NotorietyFlag.Invulnerable || value2.NotorietyFlag == NotorietyFlag.Enemy)
					{
						continue;
					}
					break;
				case ScanTypeObject.Hostile:
					if (Party.Contains(value2))
					{
						continue;
					}
					break;
				case ScanTypeObject.Objects:
					continue;
				}
				if (value2.Distance < num)
				{
					num = value2.Distance;
					result = value2.Serial;
				}
			}
		}
		return result;
	}

	public static uint FindNearestToMouseCursor(ScanTypeObject scanType)
	{
		int num = int.MaxValue;
		uint result = 0u;
		if (scanType == ScanTypeObject.Objects)
		{
			foreach (Item value in Items.Values)
			{
				if (!value.IsMulti && !value.IsDestroyed && value.OnGround && value.Distance < num)
				{
					num = value.Distance;
					result = value.Serial;
				}
			}
		}
		else
		{
			foreach (Mobile value2 in Mobiles.Values)
			{
				if (value2.IsDestroyed || value2 == Player)
				{
					continue;
				}
				switch (scanType)
				{
				case ScanTypeObject.Party:
					if (!Party.Contains(value2))
					{
						continue;
					}
					break;
				case ScanTypeObject.Followers:
					if (!value2.IsRenamable || value2.NotorietyFlag == NotorietyFlag.Invulnerable || value2.NotorietyFlag == NotorietyFlag.Enemy)
					{
						continue;
					}
					break;
				case ScanTypeObject.Hostile:
					if (Party.Contains(value2))
					{
						continue;
					}
					break;
				case ScanTypeObject.Objects:
					continue;
				}
				if (value2.DistanceToMouseCursor < num)
				{
					num = value2.DistanceToMouseCursor;
					result = value2.Serial;
				}
			}
		}
		return result;
	}

	public static uint FindNext(ScanTypeObject scanType, uint lastSerial, bool reverse)
	{
		bool flag = false;
		if (scanType == ScanTypeObject.Objects || scanType == ScanTypeObject.PlayerCorpse)
		{
			IEnumerable<Item> enumerable;
			if (!reverse)
			{
				IEnumerable<Item> values = Items.Values;
				enumerable = values;
			}
			else
			{
				enumerable = Items.Values.Reverse();
			}
			foreach (Item item in enumerable)
			{
				if (!item.IsMulti && !item.IsDestroyed && item.OnGround && (scanType != ScanTypeObject.PlayerCorpse || (item.IsCorpse && item.IsPlayerItem != 0)))
				{
					if (lastSerial == 0)
					{
						return item.Serial;
					}
					if (item.Serial == lastSerial)
					{
						flag = true;
					}
					else if (flag)
					{
						return item.Serial;
					}
				}
			}
		}
		else
		{
			IEnumerable<Mobile> enumerable2;
			if (!reverse)
			{
				IEnumerable<Mobile> values2 = Mobiles.Values;
				enumerable2 = values2;
			}
			else
			{
				enumerable2 = Mobiles.Values.Reverse();
			}
			foreach (Mobile item2 in enumerable2)
			{
				if (item2.IsDestroyed || item2 == Player || (item2.IsInvisibleAnimation() && item2.IsYellowHits) || item2.IgnoreCharacters)
				{
					continue;
				}
				switch (scanType)
				{
				case ScanTypeObject.Party:
					if (!Party.Contains(item2))
					{
						continue;
					}
					break;
				case ScanTypeObject.Followers:
					if (!item2.IsRenamable || item2.NotorietyFlag == NotorietyFlag.Invulnerable || item2.NotorietyFlag == NotorietyFlag.Enemy)
					{
						continue;
					}
					break;
				case ScanTypeObject.Hostile:
					if (Party.Contains(item2))
					{
						continue;
					}
					break;
				case ScanTypeObject.Objects:
					continue;
				}
				if (lastSerial == 0)
				{
					return item2.Serial;
				}
				if (item2.Serial == lastSerial)
				{
					flag = true;
				}
				else if (flag)
				{
					return item2.Serial;
				}
			}
		}
		if (lastSerial != 0)
		{
			return FindNext(scanType, 0u, reverse);
		}
		return 0u;
	}

	public static void Clear()
	{
		foreach (Mobile value in Mobiles.Values)
		{
			RemoveMobile(value);
		}
		foreach (Item value2 in Items.Values)
		{
			RemoveItem(value2);
		}
		ObjectToRemove = 0u;
		LastObject = 0u;
		Items.Clear();
		Mobiles.Clear();
		Player?.Destroy();
		Player = null;
		Map?.Destroy();
		Map = null;
		IsometricLight light = Light;
		int overall = (Light.RealOverall = 0);
		light.Overall = overall;
		IsometricLight light2 = Light;
		overall = (Light.RealPersonal = 0);
		light2.Personal = overall;
		ClientFeatures.SetFlags((CharacterListFlags)0);
		ClientLockedFeatures.SetFlags((LockedFeatureFlags)0u);
		Party?.Clear();
		TargetManager.LastAttack = 0u;
		MessageManager.PromptData = default(PromptData);
		_effectManager.Clear();
		_toRemove.Clear();
		CorpseManager.Clear();
		OPL.Clear();
		WMapManager.Clear();
		HouseManager?.Clear();
		Season = Season.Summer;
		CustomSeasonManager.InitializeSeason();
		OldSeason = Season.Summer;
		Journal.Clear();
		WorldTextManager.Clear();
		ActiveSpellIcons.Clear();
		SkillsRequested = false;
	}

	private static void InternalMapChangeClear(bool noplayer)
	{
		if (!noplayer)
		{
			Map.Destroy();
			Map = null;
			Player.Destroy();
			Player = null;
		}
		foreach (Item value in Items.Values)
		{
			if (!noplayer || !(Player != null) || Player.IsDestroyed || value.RootContainer != (uint)Player)
			{
				if (value.OnGround && value.IsMulti)
				{
					HouseManager.Remove(value.Serial);
				}
				_toRemove.Add(value);
			}
		}
		foreach (uint item in _toRemove)
		{
			RemoveItem(item, forceRemove: true);
		}
		_toRemove.Clear();
		foreach (Mobile value2 in Mobiles.Values)
		{
			if (!noplayer || !(Player != null) || Player.IsDestroyed || !(value2 == Player))
			{
				_toRemove.Add(value2);
			}
		}
		foreach (uint item2 in _toRemove)
		{
			RemoveMobile(item2, forceRemove: true);
		}
		_toRemove.Clear();
	}
}

using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.IO.Resources;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game;

internal static class ItemHold
{
	private static bool _enabled;

	public static Point MouseOffset;

	public static bool IsFixedPosition;

	public static bool IgnoreFixedPosition;

	public static int FixedX;

	public static int FixedY;

	public static bool OnGround { get; private set; }

	public static ushort X { get; private set; }

	public static ushort Y { get; private set; }

	public static sbyte Z { get; private set; }

	public static uint Container { get; private set; }

	public static uint Serial { get; private set; }

	public static ushort Graphic { get; private set; }

	public static ushort DisplayedGraphic { get; private set; }

	public static bool IsGumpTexture { get; set; }

	public static ushort Hue { get; private set; }

	public static ushort Amount { get; private set; }

	public static ushort TotalAmount { get; private set; }

	public static bool IsStackable { get; private set; }

	public static bool IsPartialHue { get; private set; }

	public static bool IsWearable { get; private set; }

	public static bool HasAlpha { get; private set; }

	public static Layer Layer { get; private set; }

	public static Flags Flags { get; private set; }

	public static string Name { get; set; }

	public static bool Enabled
	{
		get
		{
			return _enabled;
		}
		set
		{
			_enabled = value;
			if (!value)
			{
				IsFixedPosition = false;
				FixedX = 0;
				FixedY = 0;
				IgnoreFixedPosition = false;
			}
		}
	}

	public static bool Dropped { get; set; }

	public static bool UpdatedInWorld { get; set; }

	public static ref StaticTiles ItemData => ref TileDataLoader.Instance.StaticData[Graphic];

	public static void Set(Item item, ushort amount, Point? offset = null)
	{
		Enabled = true;
		Serial = item.Serial;
		Graphic = item.Graphic;
		DisplayedGraphic = ((item.IsCoin && amount == 1) ? item.Graphic : item.DisplayedGraphic);
		X = item.X;
		Y = item.Y;
		Z = item.Z;
		OnGround = item.OnGround;
		Container = item.Container;
		Hue = item.Hue;
		Amount = amount;
		Name = item.Name;
		TotalAmount = item.Amount;
		IsStackable = item.ItemData.IsStackable;
		IsPartialHue = item.ItemData.IsPartialHue;
		HasAlpha = item.ItemData.IsTranslucent;
		IsWearable = item.ItemData.IsWearable;
		Layer = item.Layer;
		Flags = item.Flags;
		MouseOffset = offset ?? Point.Zero;
		IsFixedPosition = false;
		FixedX = 0;
		FixedY = 0;
		IgnoreFixedPosition = false;
		IsGumpTexture = false;
	}

	public static void Clear()
	{
		Serial = 0u;
		X = ushort.MaxValue;
		Y = ushort.MaxValue;
		Z = 0;
		Container = 0u;
		DisplayedGraphic = (Graphic = ushort.MaxValue);
		Hue = ushort.MaxValue;
		OnGround = false;
		Amount = 0;
		bool num2 = (HasAlpha = false);
		IsPartialHue = num2;
		IsStackable = num2;
		IsWearable = num2;
		Layer = Layer.Invalid;
		Flags = Flags.None;
		MouseOffset = Point.Zero;
		Dropped = false;
		Enabled = false;
		UpdatedInWorld = false;
		IsFixedPosition = false;
		FixedX = 0;
		FixedY = 0;
		IgnoreFixedPosition = false;
		IsGumpTexture = false;
	}
}

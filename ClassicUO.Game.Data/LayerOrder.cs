namespace ClassicUO.Game.Data;

internal static class LayerOrder
{
	public static Layer[] defaultClientLayerOrder = new Layer[23]
	{
		Layer.Cloak,
		Layer.Shirt,
		Layer.Pants,
		Layer.Shoes,
		Layer.Legs,
		Layer.Arms,
		Layer.Torso,
		Layer.Tunic,
		Layer.Ring,
		Layer.Bracelet,
		Layer.Face,
		Layer.Gloves,
		Layer.Skirt,
		Layer.Robe,
		Layer.Waist,
		Layer.Necklace,
		Layer.Hair,
		Layer.Beard,
		Layer.Earrings,
		Layer.Helmet,
		Layer.OneHanded,
		Layer.TwoHanded,
		Layer.Talisman
	};

	public static Layer[,] UsedLayers { get; } = new Layer[8, 23]
	{
		{
			Layer.Shirt,
			Layer.Pants,
			Layer.Shoes,
			Layer.Legs,
			Layer.Torso,
			Layer.Ring,
			Layer.Talisman,
			Layer.Bracelet,
			Layer.Face,
			Layer.Arms,
			Layer.Gloves,
			Layer.Skirt,
			Layer.Tunic,
			Layer.Robe,
			Layer.Necklace,
			Layer.Waist,
			Layer.Hair,
			Layer.Beard,
			Layer.Earrings,
			Layer.OneHanded,
			Layer.Helmet,
			Layer.TwoHanded,
			Layer.Cloak
		},
		{
			Layer.Shirt,
			Layer.Pants,
			Layer.Shoes,
			Layer.Legs,
			Layer.Torso,
			Layer.Ring,
			Layer.Talisman,
			Layer.Bracelet,
			Layer.Face,
			Layer.Arms,
			Layer.Gloves,
			Layer.Skirt,
			Layer.Tunic,
			Layer.Robe,
			Layer.Necklace,
			Layer.Waist,
			Layer.Hair,
			Layer.Beard,
			Layer.Earrings,
			Layer.OneHanded,
			Layer.Cloak,
			Layer.Helmet,
			Layer.TwoHanded
		},
		{
			Layer.Shirt,
			Layer.Pants,
			Layer.Shoes,
			Layer.Legs,
			Layer.Torso,
			Layer.Ring,
			Layer.Talisman,
			Layer.Bracelet,
			Layer.Face,
			Layer.Arms,
			Layer.Gloves,
			Layer.Skirt,
			Layer.Tunic,
			Layer.Robe,
			Layer.Necklace,
			Layer.Waist,
			Layer.Hair,
			Layer.Beard,
			Layer.Earrings,
			Layer.OneHanded,
			Layer.Cloak,
			Layer.Helmet,
			Layer.TwoHanded
		},
		{
			Layer.Cloak,
			Layer.Shirt,
			Layer.Pants,
			Layer.Shoes,
			Layer.Legs,
			Layer.Torso,
			Layer.Ring,
			Layer.Talisman,
			Layer.Bracelet,
			Layer.Face,
			Layer.Arms,
			Layer.Gloves,
			Layer.Skirt,
			Layer.Tunic,
			Layer.Robe,
			Layer.Waist,
			Layer.Necklace,
			Layer.Hair,
			Layer.Beard,
			Layer.Earrings,
			Layer.Helmet,
			Layer.OneHanded,
			Layer.TwoHanded
		},
		{
			Layer.Shirt,
			Layer.Pants,
			Layer.Shoes,
			Layer.Legs,
			Layer.Torso,
			Layer.Ring,
			Layer.Talisman,
			Layer.Bracelet,
			Layer.Face,
			Layer.Arms,
			Layer.Gloves,
			Layer.Skirt,
			Layer.Tunic,
			Layer.Robe,
			Layer.Necklace,
			Layer.Waist,
			Layer.Hair,
			Layer.Beard,
			Layer.Earrings,
			Layer.OneHanded,
			Layer.Cloak,
			Layer.Helmet,
			Layer.TwoHanded
		},
		{
			Layer.Shirt,
			Layer.Pants,
			Layer.Shoes,
			Layer.Legs,
			Layer.Torso,
			Layer.Ring,
			Layer.Talisman,
			Layer.Bracelet,
			Layer.Face,
			Layer.Arms,
			Layer.Gloves,
			Layer.Skirt,
			Layer.Tunic,
			Layer.Robe,
			Layer.Necklace,
			Layer.Waist,
			Layer.Hair,
			Layer.Beard,
			Layer.Earrings,
			Layer.OneHanded,
			Layer.Cloak,
			Layer.Helmet,
			Layer.TwoHanded
		},
		{
			Layer.Shirt,
			Layer.Pants,
			Layer.Shoes,
			Layer.Legs,
			Layer.Torso,
			Layer.Ring,
			Layer.Talisman,
			Layer.Bracelet,
			Layer.Face,
			Layer.Arms,
			Layer.Gloves,
			Layer.Skirt,
			Layer.Tunic,
			Layer.Robe,
			Layer.Necklace,
			Layer.Waist,
			Layer.Hair,
			Layer.Beard,
			Layer.Earrings,
			Layer.OneHanded,
			Layer.Cloak,
			Layer.Helmet,
			Layer.TwoHanded
		},
		{
			Layer.Shirt,
			Layer.Pants,
			Layer.Shoes,
			Layer.Legs,
			Layer.Torso,
			Layer.Ring,
			Layer.Talisman,
			Layer.Bracelet,
			Layer.Face,
			Layer.Arms,
			Layer.Gloves,
			Layer.Skirt,
			Layer.Tunic,
			Layer.Robe,
			Layer.Necklace,
			Layer.Waist,
			Layer.Hair,
			Layer.Beard,
			Layer.Earrings,
			Layer.OneHanded,
			Layer.Cloak,
			Layer.Helmet,
			Layer.TwoHanded
		}
	};
}

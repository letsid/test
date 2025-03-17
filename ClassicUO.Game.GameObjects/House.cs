using System;
using System.Collections.Generic;
using System.Linq;
using ClassicUO.Game.Managers;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.GameObjects;

internal sealed class House : IEquatable<uint>
{
	public Rectangle Bounds;

	public bool IsCustom;

	public uint Revision;

	public uint Serial { get; }

	public List<Multi> Components { get; } = new List<Multi>();

	public House(uint serial, uint revision, bool isCustom)
	{
		Serial = serial;
		Revision = revision;
		IsCustom = isCustom;
	}

	public bool Equals(uint other)
	{
		return Serial == other;
	}

	public IEnumerable<Multi> GetMultiAt(int x, int y)
	{
		return Components.Where((Multi s) => !s.IsDestroyed && s.X == x && s.Y == y);
	}

	public Multi Add(ushort graphic, ushort hue, ushort x, ushort y, sbyte z, bool iscustom, bool ismovable)
	{
		Multi multi = Multi.Create(graphic);
		multi.Hue = hue;
		multi.X = x;
		multi.Y = y;
		multi.Z = z;
		multi.UpdateScreenPosition();
		multi.IsCustom = iscustom;
		multi.IsMovable = ismovable;
		multi.AddToTile();
		Components.Add(multi);
		return multi;
	}

	public void ClearCustomHouseComponents(CUSTOM_HOUSE_MULTI_OBJECT_FLAGS state)
	{
		Item item = World.Items.Get(Serial);
		if (!(item != null))
		{
			return;
		}
		int num = item.Z + 7;
		for (int i = 0; i < Components.Count; i++)
		{
			Multi multi = Components[i];
			multi.State &= ~(CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_TRANSPARENT | CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_IGNORE_IN_RENDER | CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_VALIDATED_PLACE | CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_INCORRECT_PLACE);
			if (multi.IsCustom)
			{
				if (multi.Z <= item.Z && (multi.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_STAIR) == 0)
				{
					multi.State |= CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_DONT_REMOVE;
				}
				if (state == (CUSTOM_HOUSE_MULTI_OBJECT_FLAGS)0 || (multi.State & state) != 0)
				{
					multi.Destroy();
				}
			}
			else if (multi.Z <= num)
			{
				multi.State = multi.State | CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_FLOOR | CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_IGNORE_IN_RENDER;
			}
			if (multi.IsDestroyed)
			{
				Components.RemoveAt(i--);
			}
		}
	}

	public void Generate(bool recalculate = false, bool pushtotile = true, bool removePreview = false)
	{
		Item item = World.Items.Get(Serial);
		foreach (Multi component in Components)
		{
			if (item != null)
			{
				if (recalculate)
				{
					component.X = (ushort)(item.X + component.MultiOffsetX);
					component.Y = (ushort)(item.Y + component.MultiOffsetY);
					component.Z = (sbyte)(item.Z + component.MultiOffsetZ);
					component.UpdateScreenPosition();
					component.Offset = Vector3.Zero;
				}
				if (removePreview)
				{
					component.State &= ~CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_PREVIEW;
				}
				component.Hue = item.Hue;
			}
			if (pushtotile)
			{
				component.AddToTile();
			}
		}
		World.CustomHouseManager?.GenerateFloorPlace();
	}

	public void ClearComponents(bool removeCustomOnly = false)
	{
		Item item = World.Items.Get(Serial);
		if (item != null && !item.IsDestroyed)
		{
			item.WantUpdateMulti = true;
		}
		for (int i = 0; i < Components.Count; i++)
		{
			Multi multi = Components[i];
			if (!(!multi.IsCustom && removeCustomOnly))
			{
				multi.Destroy();
				Components.RemoveAt(i--);
			}
		}
	}
}

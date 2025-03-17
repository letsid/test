using System;
using System.Runtime.CompilerServices;
using ClassicUO.Game.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.IO.Resources;
using ClassicUO.Network;
using ClassicUO.Renderer;

namespace ClassicUO.Game.GameObjects;

internal abstract class Entity : GameObject, IEquatable<Entity>
{
	private Direction _direction;

	public byte AnimIndex;

	public Flags Flags;

	public ushort Hits;

	public ushort HitsMax;

	public byte HitsPercentage;

	public RenderedText HitsTexture;

	public bool IsClicked;

	public uint LastStepTime;

	public string Name;

	public uint Serial;

	public bool ExecuteAnimation = true;

	internal long LastAnimationChangeTime;

	public HitsRequestStatus HitsRequest;

	public ushort IsPlayerItem;

	public bool IsHidden => (Flags & Flags.Hidden) != 0;

	public Direction Direction
	{
		get
		{
			return _direction;
		}
		set
		{
			if (_direction != value)
			{
				_direction = value;
				OnDirectionChanged();
			}
		}
	}

	public bool Exists => World.Contains(Serial);

	protected Entity(uint serial)
	{
		Serial = serial;
	}

	public bool Equals(Entity e)
	{
		if (e != null)
		{
			return Serial == e.Serial;
		}
		return false;
	}

	public void FixHue(ushort hue)
	{
		ushort num = (ushort)(hue & 0x3FFF);
		if (num != 0)
		{
			if (num >= 3000)
			{
				num = 1;
			}
			num |= (ushort)(hue & 0xC000);
		}
		else
		{
			num = (ushort)(hue & 0x8000);
		}
		Hue = num;
	}

	public void UpdateHits(byte perc)
	{
		if (perc != HitsPercentage || HitsTexture == null || HitsTexture.IsDestroyed)
		{
			HitsPercentage = perc;
			ushort hue = 68;
			if (perc < 30)
			{
				hue = 33;
			}
			else if (perc < 50)
			{
				hue = 48;
			}
			else if (perc < 80)
			{
				hue = 88;
			}
			HitsTexture?.Destroy();
			HitsTexture = RenderedText.Create($"[{perc}%]", hue, 3, isunicode: false, FontStyle.None, TEXT_ALIGN_TYPE.TS_LEFT, 0, 30);
		}
	}

	public virtual void CheckGraphicChange(byte animIndex = 0)
	{
	}

	public override void Update(double totalTime, double frameTime)
	{
		base.Update(totalTime, frameTime);
		if (ObjectHandlesStatus == ObjectHandlesStatus.OPEN)
		{
			ObjectHandlesStatus = ObjectHandlesStatus.DISPLAYING;
			if (SerialHelper.IsMobile(Serial))
			{
				NetClient.Socket.Send_NameRequest(Serial);
			}
			UIManager.Add(new NameOverheadGump(this));
		}
		if (HitsMax > 0)
		{
			int hitsMax = HitsMax;
			hitsMax = Hits * 100 / hitsMax;
			if (hitsMax > 100)
			{
				hitsMax = 100;
			}
			else if (hitsMax < 1)
			{
				hitsMax = 0;
			}
			UpdateHits((byte)hitsMax);
		}
	}

	public override void Destroy()
	{
		base.Destroy();
		GameActions.SendCloseStatus(Serial, HitsRequest >= HitsRequestStatus.Pending);
		AnimIndex = 0;
		LastAnimationChangeTime = 0L;
		HitsTexture?.Destroy();
		HitsTexture = null;
	}

	public Item FindItem(ushort graphic, ushort hue = ushort.MaxValue)
	{
		Item result = null;
		if (hue == ushort.MaxValue)
		{
			int num = 65535;
			for (LinkedObject linkedObject = Items; linkedObject != null; linkedObject = linkedObject.Next)
			{
				Item item = (Item)linkedObject;
				if (item.Graphic == graphic && item.Hue < num)
				{
					result = item;
					num = item.Hue;
				}
				if (SerialHelper.IsValid(item.Container))
				{
					Item item2 = item.FindItem(graphic, hue);
					if (item2 != null && item2.Hue < num)
					{
						result = item2;
						num = item2.Hue;
					}
				}
			}
		}
		else
		{
			for (LinkedObject linkedObject2 = Items; linkedObject2 != null; linkedObject2 = linkedObject2.Next)
			{
				Item item3 = (Item)linkedObject2;
				short num2 = (short)(item3.Hue & 0x7FFF);
				if (item3.Graphic == graphic && num2 == hue)
				{
					result = item3;
				}
				if (SerialHelper.IsValid(item3.Container))
				{
					Item item4 = item3.FindItem(graphic, hue);
					if (item4 != null)
					{
						result = item4;
					}
				}
			}
		}
		return result;
	}

	public Item FindItem(ushort graphic, string name, ushort hue = ushort.MaxValue)
	{
		Item result = null;
		if (hue == ushort.MaxValue)
		{
			int num = 65535;
			for (LinkedObject linkedObject = Items; linkedObject != null; linkedObject = linkedObject.Next)
			{
				Item item = (Item)linkedObject;
				if (item.Graphic == graphic && item.Name.Equals(name) && item.Hue < num)
				{
					result = item;
					num = item.Hue;
				}
				if (SerialHelper.IsValid(item.Container))
				{
					Item item2 = item.FindItem(graphic, name, hue);
					if (item2 != null && item2.Hue < num)
					{
						result = item2;
						num = item2.Hue;
					}
				}
			}
		}
		else
		{
			for (LinkedObject linkedObject2 = Items; linkedObject2 != null; linkedObject2 = linkedObject2.Next)
			{
				Item item3 = (Item)linkedObject2;
				if (item3.Graphic == graphic && item3.Hue == hue && item3.Name.Equals(name))
				{
					result = item3;
				}
				if (SerialHelper.IsValid(item3.Container))
				{
					Item item4 = item3.FindItem(graphic, name, hue);
					if (item4 != null)
					{
						result = item4;
					}
				}
			}
		}
		return result;
	}

	public Item FindItem(string name)
	{
		Item result = null;
		for (LinkedObject linkedObject = Items; linkedObject != null; linkedObject = linkedObject.Next)
		{
			Item item = (Item)linkedObject;
			if (!string.IsNullOrEmpty(item.Name) && item.Name.Trim().Contains(name, StringComparison.OrdinalIgnoreCase))
			{
				result = item;
			}
			if (SerialHelper.IsValid(item.Container))
			{
				Item item2 = item.FindItem(name);
				if (item2 != null)
				{
					result = item2;
				}
			}
		}
		return result;
	}

	public Item FindItemExactName(string name)
	{
		Item result = null;
		for (LinkedObject linkedObject = Items; linkedObject != null; linkedObject = linkedObject.Next)
		{
			Item item = (Item)linkedObject;
			if (!string.IsNullOrEmpty(item.Name) && item.Name.Trim().ToLower() == name.ToLower())
			{
				result = item;
			}
			if (SerialHelper.IsValid(item.Container))
			{
				Item item2 = item.FindItemExactName(name);
				if (item2 != null)
				{
					result = item2;
				}
			}
		}
		return result;
	}

	public Item GetItemByGraphic(ushort graphic, bool deepsearch = false)
	{
		for (LinkedObject linkedObject = Items; linkedObject != null; linkedObject = linkedObject.Next)
		{
			Item item = (Item)linkedObject;
			if (item.Graphic == graphic)
			{
				return item;
			}
			if (deepsearch && !item.IsEmpty)
			{
				for (LinkedObject linkedObject2 = Items; linkedObject2 != null; linkedObject2 = linkedObject2.Next)
				{
					Item itemByGraphic = ((Item)linkedObject2).GetItemByGraphic(graphic, deepsearch);
					if (itemByGraphic != null)
					{
						return itemByGraphic;
					}
				}
			}
		}
		return null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Item FindItemByLayer(Layer layer)
	{
		for (LinkedObject linkedObject = Items; linkedObject != null; linkedObject = linkedObject.Next)
		{
			Item item = (Item)linkedObject;
			if (!item.IsDestroyed && item.Layer == layer)
			{
				return item;
			}
		}
		return null;
	}

	public void ClearUnequipped()
	{
		if (base.IsEmpty)
		{
			return;
		}
		LinkedObject linkedObject = null;
		LinkedObject linkedObject2 = Items;
		while (linkedObject2 != null)
		{
			LinkedObject next = linkedObject2.Next;
			Item item = (Item)linkedObject2;
			if (item.Layer != 0)
			{
				if (linkedObject == null)
				{
					linkedObject = linkedObject2;
				}
			}
			else
			{
				item.Container = uint.MaxValue;
				World.Items.Remove(item);
				item.Destroy();
				Remove(linkedObject2);
			}
			linkedObject2 = next;
		}
		Items = linkedObject;
	}

	public static implicit operator uint(Entity entity)
	{
		return entity.Serial;
	}

	public static bool operator ==(Entity e, Entity s)
	{
		return object.Equals(e, s);
	}

	public static bool operator !=(Entity e, Entity s)
	{
		return !object.Equals(e, s);
	}

	public override bool Equals(object obj)
	{
		if (obj is Entity e)
		{
			return Equals(e);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (int)Serial;
	}

	public abstract void ProcessAnimation(out byte dir, bool evalutate = false);

	public abstract ushort GetGraphicForAnimation();
}

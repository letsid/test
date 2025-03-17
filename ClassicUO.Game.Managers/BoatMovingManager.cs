using System;
using System.Collections.Generic;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Network;
using ClassicUO.Utility.Collections;

namespace ClassicUO.Game.Managers;

internal static class BoatMovingManager
{
	private struct BoatStep
	{
		public uint Serial;

		public int TimeDiff;

		public ushort X;

		public ushort Y;

		public sbyte Z;

		public byte Speed;

		public Direction MovingDir;

		public Direction FacingDir;
	}

	private struct ItemInside
	{
		public uint Serial;

		public int X;

		public int Y;

		public int Z;
	}

	private const int SLOW_INTERVAL = 1000;

	private const int NORMAL_INTERVAL = 500;

	private const int FAST_INTERVAL = 250;

	private static readonly Dictionary<uint, Deque<BoatStep>> _steps = new Dictionary<uint, Deque<BoatStep>>();

	private static readonly List<uint> _toRemove = new List<uint>();

	private static readonly Dictionary<uint, RawList<ItemInside>> _items = new Dictionary<uint, RawList<ItemInside>>();

	private static uint _timePacket;

	private static int GetVelocity(byte speed)
	{
		return speed switch
		{
			2 => 1000, 
			4 => 250, 
			_ => 500, 
		};
	}

	public static void MoveRequest(Direction direciton, byte speed)
	{
		NetClient.Socket.Send_MultiBoatMoveRequest(World.Player, direciton, speed);
		_timePacket = Time.Ticks;
	}

	public static void AddStep(uint serial, byte speed, Direction movingDir, Direction facingDir, ushort x, ushort y, sbyte z)
	{
		Item item = World.Items.Get(serial);
		if (!(item == null) && !item.IsDestroyed)
		{
			if (!_steps.TryGetValue(serial, out var value))
			{
				value = new Deque<BoatStep>();
				_steps[serial] = value;
			}
			bool flag = value.Count == 0;
			while (value.Count > 5)
			{
				value.RemoveFromFront();
			}
			if (flag)
			{
				item.LastStepTime = Time.Ticks;
			}
			BoatStep value2 = default(BoatStep);
			value2.Serial = serial;
			value2.TimeDiff = ((_timePacket == 0 || flag) ? GetVelocity(speed) : ((int)(Time.Ticks - _timePacket)));
			value2.Speed = speed;
			value2.X = x;
			value2.Y = y;
			value2.Z = z;
			value2.MovingDir = movingDir;
			value.AddToBack(value2);
			ClearEntities(serial);
			_timePacket = Time.Ticks;
			Console.WriteLine("CURRENT PACKET TIME: {0}", _timePacket);
		}
	}

	public static void ClearSteps(uint serial)
	{
		if (!_steps.TryGetValue(serial, out var value) || value.Count == 0)
		{
			return;
		}
		Item item = World.Items.Get(serial);
		if (item != null)
		{
			item.Offset.X = 0f;
			item.Offset.Y = 0f;
			item.Offset.Z = 0f;
		}
		if (_items.TryGetValue(serial, out var value2))
		{
			for (int i = 0; i < value2.Count; i++)
			{
				Entity entity = World.Get(value2[i].Serial);
				if (!(entity == null))
				{
					entity.ShipOffset.X = 0f;
					entity.ShipOffset.Y = 0f;
					entity.ShipOffset.Z = 0f;
				}
			}
			value2.Count = 0u;
		}
		value.Clear();
	}

	public static void ClearEntities(uint serial)
	{
		_items.Remove(serial);
	}

	public static void PushItemToList(uint serial, uint objSerial, int x, int y, int z)
	{
		if (!_items.TryGetValue(serial, out var value))
		{
			value = new RawList<ItemInside>();
			_items[serial] = value;
		}
		for (int i = 0; i < value.Count; i++)
		{
			ref ItemInside reference = ref value[i];
			if (!SerialHelper.IsValid(reference.Serial))
			{
				break;
			}
			if (reference.Serial == objSerial)
			{
				reference.X = x;
				reference.Y = y;
				reference.Z = z;
				return;
			}
		}
		value.Add(new ItemInside
		{
			Serial = objSerial,
			X = x,
			Y = y,
			Z = z
		});
	}

	public static void Update()
	{
		foreach (Deque<BoatStep> value in _steps.Values)
		{
			while (value.Count != 0)
			{
				ref BoatStep reference = ref value.Front();
				Item item = World.Items.Get(reference.Serial);
				if (item == null || item.IsDestroyed)
				{
					_toRemove.Add(reference.Serial);
					break;
				}
				_ = reference;
				_ = reference;
				int timeDiff = reference.TimeDiff;
				int num = (int)(Time.Ticks - item.LastStepTime);
				bool flag = num >= timeDiff;
				bool flag2 = false;
				if (item.X != reference.X || item.Y != reference.Y)
				{
					if (timeDiff != 0)
					{
						float num2 = (float)timeDiff / 80f;
						float x = (float)num / 80f;
						float y = x;
						item.Offset.Z = (sbyte)((float)(reference.Z - item.Z) * x * (4f / num2));
						MovementSpeed.GetPixelOffset((byte)reference.MovingDir, ref x, ref y, num2);
						item.Offset.X = (sbyte)x;
						item.Offset.Y = (sbyte)y;
					}
				}
				else
				{
					flag2 = true;
					flag = true;
				}
				World.HouseManager.TryGetHouse(item, out var house);
				if (flag)
				{
					item.X = reference.X;
					item.Y = reference.Y;
					item.Z = reference.Z;
					item.UpdateScreenPosition();
					item.Offset.X = 0f;
					item.Offset.Y = 0f;
					item.Offset.Z = 0f;
					value.RemoveFromFront();
					if (item.TNext != null || item.TPrevious != null)
					{
						item.AddToTile();
					}
					house?.Generate(recalculate: true, pushtotile: true, removePreview: true);
					UpdateEntitiesInside(item, flag, reference.X, reference.Y, reference.Z, reference.MovingDir);
					item.LastStepTime = Time.Ticks;
				}
				else
				{
					if (house != null)
					{
						foreach (Multi component in house.Components)
						{
							component.Offset = item.Offset;
						}
					}
					UpdateEntitiesInside(item, flag, item.X, item.Y, item.Z, reference.MovingDir);
				}
				if (!flag2)
				{
					break;
				}
			}
		}
		if (_toRemove.Count != 0)
		{
			for (int i = 0; i < _toRemove.Count; i++)
			{
				_steps.Remove(_toRemove[i]);
				_items.Remove(_toRemove[i]);
			}
			_toRemove.Clear();
		}
	}

	private static void UpdateEntitiesInside(uint serial, bool removeStep, int x, int y, int z, Direction direction)
	{
		if (!_items.TryGetValue(serial, out var value))
		{
			return;
		}
		Item item = World.Items.Get(serial);
		for (int i = 0; i < value.Count; i++)
		{
			ref ItemInside reference = ref value[i];
			Entity entity = World.Get(reference.Serial);
			if (entity == null || entity.IsDestroyed)
			{
				continue;
			}
			if (removeStep)
			{
				entity.X = (ushort)(x - reference.X);
				entity.Y = (ushort)(y - reference.Y);
				entity.Z = (sbyte)(z - reference.Z);
				entity.UpdateScreenPosition();
				entity.ShipOffset.X = 0f;
				entity.ShipOffset.Y = 0f;
				entity.ShipOffset.Z = 0f;
				if (entity.TPrevious != null || entity.TNext != null)
				{
					entity.AddToTile();
				}
			}
			else if (item != null)
			{
				entity.ShipOffset = item.Offset;
			}
		}
	}

	private static void GetEndPosition(Item item, Deque<BoatStep> deque, out ushort x, out ushort y, out sbyte z, out Direction dir)
	{
		if (deque.Count == 0)
		{
			x = item.X;
			y = item.Y;
			z = item.Z;
			dir = item.Direction & Direction.Up;
			dir &= Direction.Running;
		}
		else
		{
			ref BoatStep reference = ref deque.Back();
			x = reference.X;
			y = reference.Y;
			z = reference.Z;
			dir = reference.MovingDir;
		}
	}
}

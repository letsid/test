using System;
using ClassicUO.Configuration;
using ClassicUO.Data;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.Input;
using ClassicUO.IO;
using ClassicUO.IO.Resources;
using ClassicUO.Network;
using ClassicUO.Resources;

namespace ClassicUO.Game.Managers;

internal static class TargetManager
{
	private static uint _targetCursorId;

	private static readonly byte[] _lastDataBuffer = new byte[19];

	public static uint LastAttack;

	public static uint _SelectedTarget;

	public static readonly LastTargetInfo LastTargetInfo = new LastTargetInfo();

	public static uint SelectedTarget
	{
		get
		{
			return _SelectedTarget;
		}
		set
		{
			_SelectedTarget = value;
			GameActions.ShowHealthBarAndDispose(_SelectedTarget);
		}
	}

	public static MultiTargetInfo MultiTargetInfo { get; private set; }

	public static CursorTarget TargetingState { get; private set; } = CursorTarget.Invalid;

	public static bool IsTargeting { get; private set; }

	public static TargetType TargetingType { get; private set; }

	private static void ClearTargetingWithoutTargetCancelPacket()
	{
		if (TargetingState == CursorTarget.MultiPlacement)
		{
			MultiTargetInfo = null;
			TargetingState = CursorTarget.Object;
			World.HouseManager.Remove(0u);
		}
		IsTargeting = false;
	}

	public static void Reset()
	{
		ClearTargetingWithoutTargetCancelPacket();
		TargetingState = CursorTarget.Object;
		_targetCursorId = 0u;
		MultiTargetInfo = null;
		TargetingType = TargetType.Neutral;
	}

	public static void LoseTargetAndCloseHealthBar()
	{
		HealthBarGump gump = UIManager.GetGump<HealthBarGump>(null);
		HealthBarGumpCustom gump2 = UIManager.GetGump<HealthBarGumpCustom>(null);
		if (gump != null && !World.Party.Contains(gump.LocalSerial) && gump.LocalSerial != (uint)World.Player)
		{
			UIManager.GetGump<HealthBarGump>(gump.LocalSerial)?.Dispose();
		}
		else if (gump2 != null && !World.Party.Contains(gump2.LocalSerial) && gump2.LocalSerial != (uint)World.Player)
		{
			UIManager.GetGump<HealthBarGumpCustom>(gump2.LocalSerial)?.Dispose();
		}
		if (SelectedTarget != 0 || LastTargetInfo.Serial != 0)
		{
			LastAttack = 0u;
			SelectedTarget = 0u;
			LastTargetInfo.SetEntity(0u);
		}
	}

	public static void SetTargeting(CursorTarget targeting, uint cursorID, TargetType cursorType)
	{
		if (targeting != CursorTarget.Invalid)
		{
			bool isTargeting = IsTargeting;
			IsTargeting = cursorType < TargetType.Cancel;
			TargetingState = targeting;
			TargetingType = cursorType;
			if (!IsTargeting && isTargeting)
			{
				CancelTarget();
			}
			_targetCursorId = cursorID;
		}
	}

	public static void CancelTarget()
	{
		if (TargetingState == CursorTarget.MultiPlacement)
		{
			World.HouseManager.Remove(0u);
			if (World.CustomHouseManager != null)
			{
				World.CustomHouseManager.Erasing = false;
				World.CustomHouseManager.SeekTile = false;
				World.CustomHouseManager.SelectedGraphic = 0;
				World.CustomHouseManager.CombinedStair = false;
				UIManager.GetGump<HouseCustomizationGump>(null)?.Update();
			}
		}
		if (IsTargeting || TargetingType == TargetType.Cancel)
		{
			NetClient.Socket.Send_TargetCancel(TargetingState, _targetCursorId, (byte)TargetingType);
			IsTargeting = false;
		}
		Reset();
	}

	public static void SetTargetingMulti(uint deedSerial, ushort model, ushort x, ushort y, ushort z, ushort hue)
	{
		SetTargeting(CursorTarget.MultiPlacement, deedSerial, TargetType.Neutral);
		MultiTargetInfo = new MultiTargetInfo(model, x, y, z, hue);
	}

	public static void Target(uint serial)
	{
		if (!IsTargeting)
		{
			return;
		}
		Entity entity = (World.InGame ? World.Get(serial) : null);
		if (!(entity != null))
		{
			return;
		}
		switch (TargetingState)
		{
		case CursorTarget.Invalid:
			break;
		case CursorTarget.Object:
		case CursorTarget.Position:
		case CursorTarget.MultiPlacement:
		case CursorTarget.SetTargetClientSide:
		case CursorTarget.HueCommandTarget:
			if (entity != World.Player)
			{
				LastTargetInfo.SetEntity(serial);
			}
			if (SerialHelper.IsMobile(serial) && serial != (uint)World.Player && (World.Player.NotorietyFlag == NotorietyFlag.Innocent || World.Player.NotorietyFlag == NotorietyFlag.Ally))
			{
				Mobile mobile = entity as Mobile;
				if (mobile != null)
				{
					bool flag = false;
					if (TargetingType == TargetType.Harmful && ProfileManager.CurrentProfile.EnabledCriminalActionQuery && mobile.NotorietyFlag == NotorietyFlag.Innocent)
					{
						flag = true;
					}
					else if (TargetingType == TargetType.Beneficial && ProfileManager.CurrentProfile.EnabledBeneficialCriminalActionQuery && (mobile.NotorietyFlag == NotorietyFlag.Criminal || mobile.NotorietyFlag == NotorietyFlag.Murderer || mobile.NotorietyFlag == NotorietyFlag.Gray))
					{
						flag = true;
					}
					if (flag && UIManager.GetGump<QuestionGump>(null) == null)
					{
						UIManager.Add(new QuestionGump("This may flag\nyou criminal!", delegate(bool s)
						{
							if (s)
							{
								NetClient.Socket.Send_TargetObject(entity, entity.Graphic, entity.X, entity.Y, entity.Z, _targetCursorId, (byte)TargetingType);
								ClearTargetingWithoutTargetCancelPacket();
								if (LastTargetInfo.Serial != serial)
								{
									GameActions.RequestMobileStatus(serial);
								}
							}
						}));
						break;
					}
				}
			}
			if (TargetingState != CursorTarget.SetTargetClientSide)
			{
				if (World.Player != null && _targetCursorId != World.Player.Serial)
				{
					_lastDataBuffer[0] = 108;
					_lastDataBuffer[1] = 0;
					_lastDataBuffer[2] = (byte)(_targetCursorId >> 24);
					_lastDataBuffer[3] = (byte)(_targetCursorId >> 16);
					_lastDataBuffer[4] = (byte)(_targetCursorId >> 8);
					_lastDataBuffer[5] = (byte)_targetCursorId;
					_lastDataBuffer[6] = (byte)TargetingType;
					_lastDataBuffer[7] = (byte)(entity.Serial >> 24);
					_lastDataBuffer[8] = (byte)(entity.Serial >> 16);
					_lastDataBuffer[9] = (byte)(entity.Serial >> 8);
					_lastDataBuffer[10] = (byte)entity.Serial;
					_lastDataBuffer[11] = (byte)(entity.X >> 8);
					_lastDataBuffer[12] = (byte)entity.X;
					_lastDataBuffer[13] = (byte)(entity.Y >> 8);
					_lastDataBuffer[14] = (byte)entity.Y;
					_lastDataBuffer[15] = (byte)(entity.Z >> 8);
					_lastDataBuffer[16] = (byte)entity.Z;
					_lastDataBuffer[17] = (byte)(entity.Graphic >> 8);
					_lastDataBuffer[18] = (byte)entity.Graphic;
				}
				NetClient.Socket.Send_TargetObject(entity, entity.Graphic, entity.X, entity.Y, entity.Z, _targetCursorId, (byte)TargetingType);
				if (SerialHelper.IsMobile(serial) && LastTargetInfo.Serial != serial)
				{
					GameActions.RequestMobileStatus(serial);
				}
			}
			ClearTargetingWithoutTargetCancelPacket();
			Mouse.CancelDoubleClick = true;
			break;
		case CursorTarget.Grab:
			if (SerialHelper.IsItem(serial))
			{
				GameActions.GrabItem(serial, ((Item)entity).Amount);
			}
			ClearTargetingWithoutTargetCancelPacket();
			break;
		case CursorTarget.SetGrabBag:
			if (SerialHelper.IsItem(serial))
			{
				ProfileManager.CurrentProfile.GrabBagSerial = serial;
				GameActions.Print(string.Format(ResGeneral.GrabBagSet0, serial), 946, MessageType.Regular, 3);
			}
			ClearTargetingWithoutTargetCancelPacket();
			break;
		case CursorTarget.IgnorePlayerTarget:
			if (SerialHelper.IsMobile(serial))
			{
				IgnoreManager.AddIgnoredTarget(entity);
			}
			CancelTarget();
			break;
		case CursorTarget.AddAdditionalJournalTarget:
			if (SerialHelper.IsMobile(serial))
			{
				AdditionalJournalListGump.AddAdditionalJournalDictEntry(entity);
			}
			CancelTarget();
			break;
		}
	}

	public static void Target(ushort graphic, ushort x, ushort y, short z, bool wet = false)
	{
		if (!IsTargeting)
		{
			return;
		}
		if (graphic == 0)
		{
			if (TargetingState == CursorTarget.Object)
			{
				return;
			}
		}
		else
		{
			if (graphic >= TileDataLoader.Instance.StaticData.Length)
			{
				return;
			}
			ref StaticTiles reference = ref TileDataLoader.Instance.StaticData[graphic];
			if (Client.Version >= ClientVersion.CV_7090 && reference.IsSurface)
			{
				z += reference.Height;
			}
		}
		LastTargetInfo.SetStatic(graphic, x, y, (sbyte)z);
		TargetPacket(graphic, x, y, (sbyte)z);
	}

	public static void SendMultiTarget(ushort graphic, ushort x, ushort y, sbyte z, ushort hue)
	{
		if (IsTargeting && (graphic != 0 || TargetingState != 0))
		{
			StackDataWriter stackDataWriter = default(StackDataWriter);
			stackDataWriter.WriteInt8(12);
			stackDataWriter.WriteUInt16BE(7);
			stackDataWriter.WriteUInt16BE(6);
			stackDataWriter.WriteUInt16BE(hue);
			NetClient.Socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
			TargetPacket(graphic, x, y, z);
		}
	}

	public static void TargetLast()
	{
		if (IsTargeting)
		{
			_lastDataBuffer[0] = 108;
			_lastDataBuffer[1] = (byte)TargetingState;
			_lastDataBuffer[2] = (byte)(_targetCursorId >> 24);
			_lastDataBuffer[3] = (byte)(_targetCursorId >> 16);
			_lastDataBuffer[4] = (byte)(_targetCursorId >> 8);
			_lastDataBuffer[5] = (byte)_targetCursorId;
			_lastDataBuffer[6] = (byte)TargetingType;
			Span<byte> destination = stackalloc byte[19];
			_lastDataBuffer.CopyTo(destination);
			NetClient.Socket.Send(destination.ToArray(), destination.Length);
			Mouse.CancelDoubleClick = true;
			ClearTargetingWithoutTargetCancelPacket();
		}
	}

	private static void TargetPacket(ushort graphic, ushort x, ushort y, sbyte z)
	{
		if (IsTargeting)
		{
			_lastDataBuffer[0] = 108;
			_lastDataBuffer[1] = 1;
			_lastDataBuffer[2] = (byte)(_targetCursorId >> 24);
			_lastDataBuffer[3] = (byte)(_targetCursorId >> 16);
			_lastDataBuffer[4] = (byte)(_targetCursorId >> 8);
			_lastDataBuffer[5] = (byte)_targetCursorId;
			_lastDataBuffer[6] = (byte)TargetingType;
			_lastDataBuffer[7] = 0;
			_lastDataBuffer[8] = 0;
			_lastDataBuffer[9] = 0;
			_lastDataBuffer[10] = 0;
			_lastDataBuffer[11] = (byte)(x >> 8);
			_lastDataBuffer[12] = (byte)x;
			_lastDataBuffer[13] = (byte)(y >> 8);
			_lastDataBuffer[14] = (byte)y;
			_lastDataBuffer[15] = (byte)(z >> 8);
			_lastDataBuffer[16] = (byte)z;
			_lastDataBuffer[17] = (byte)(graphic >> 8);
			_lastDataBuffer[18] = (byte)graphic;
			NetClient.Socket.Send_TargetXYZ(graphic, x, y, z, _targetCursorId, (byte)TargetingType);
			Mouse.CancelDoubleClick = true;
			ClearTargetingWithoutTargetCancelPacket();
		}
	}
}

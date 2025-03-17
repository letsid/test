using System.Collections.Generic;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Network;
using ClassicUO.Network.Encryption;
using ClassicUO.Utility.Logging;

namespace ClassicUO.Game.Managers;

internal class WorldMapEntityManager
{
	private bool _ack_received;

	private bool _can_send;

	private uint _lastUpdate;

	private uint _lastPacketSend;

	private uint _lastPacketRecv;

	private readonly List<WMapEntity> _toRemove = new List<WMapEntity>();

	public readonly Dictionary<uint, WMapEntity> Entities = new Dictionary<uint, WMapEntity>();

	public bool Enabled { get; private set; }

	public void SetACKReceived()
	{
		_ack_received = true;
	}

	public void SetEnable(bool v)
	{
		if ((World.ClientFeatures.Flags & CharacterListFlags.CLF_NEW_MOVEMENT_SYSTEM) != 0 && !_ack_received)
		{
			Log.Warn("Server support new movement system. Can't use the 0xF0 packet to query guild/party position");
			v = false;
		}
		else if (EncryptionHelper.Type != 0 && !_ack_received)
		{
			Log.Warn("Server has encryption. Can't use the 0xF0 packet to query guild/party position");
			v = false;
		}
		Enabled = v;
		if (v)
		{
			RequestServerPartyGuildInfo(force: true);
		}
	}

	public void AddOrUpdate(uint serial, int x, int y, int hp, int map, bool isguild, string name = null, bool from_packet = false)
	{
		if (from_packet)
		{
			_can_send = true;
			_lastPacketRecv = Time.Ticks + 10000;
		}
		else if (_lastPacketRecv < Time.Ticks)
		{
			return;
		}
		if (!Enabled)
		{
			return;
		}
		if (string.IsNullOrEmpty(name))
		{
			Entity entity = World.Get(serial);
			if (entity != null && !string.IsNullOrEmpty(entity.Name))
			{
				name = entity.Name;
			}
		}
		if (!Entities.TryGetValue(serial, out var value) || value == null)
		{
			value = new WMapEntity(serial)
			{
				X = x,
				Y = y,
				HP = hp,
				Map = map,
				LastUpdate = Time.Ticks + 1000,
				IsGuild = isguild,
				Name = name
			};
			Entities[serial] = value;
		}
		else
		{
			value.X = x;
			value.Y = y;
			value.HP = hp;
			value.Map = map;
			value.IsGuild = isguild;
			value.LastUpdate = Time.Ticks + 1000;
			if (string.IsNullOrEmpty(value.Name) && !string.IsNullOrEmpty(name))
			{
				value.Name = name;
			}
		}
	}

	public void Remove(uint serial)
	{
		if (Entities.ContainsKey(serial))
		{
			Entities.Remove(serial);
		}
	}

	public void RemoveUnupdatedWEntity()
	{
		if (_lastUpdate > Time.Ticks)
		{
			return;
		}
		_lastUpdate = Time.Ticks + 1000;
		long num = Time.Ticks - 1000;
		foreach (WMapEntity value in Entities.Values)
		{
			if (value.LastUpdate < num)
			{
				_toRemove.Add(value);
			}
		}
		if (_toRemove.Count == 0)
		{
			return;
		}
		foreach (WMapEntity item in _toRemove)
		{
			Entities.Remove(item.Serial);
		}
		_toRemove.Clear();
	}

	public WMapEntity GetEntity(uint serial)
	{
		Entities.TryGetValue(serial, out var value);
		return value;
	}

	public void RequestServerPartyGuildInfo(bool force = false)
	{
		if ((!force && !Enabled) || !World.InGame || _lastPacketSend >= Time.Ticks)
		{
			return;
		}
		_lastPacketSend = Time.Ticks + 250;
		NetClient.Socket.Send_QueryGuildPosition();
		if (World.Party == null || World.Party.Leader == 0)
		{
			return;
		}
		PartyMember[] members = World.Party.Members;
		foreach (PartyMember partyMember in members)
		{
			if (partyMember != null && SerialHelper.IsValid(partyMember.Serial))
			{
				Mobile mobile = World.Mobiles.Get(partyMember.Serial);
				if (mobile == null || mobile.Distance > World.ClientViewRange)
				{
					NetClient.Socket.Send_QueryPartyPosition();
					break;
				}
			}
		}
	}

	public void Clear()
	{
		Entities.Clear();
		_ack_received = false;
		SetEnable(v: false);
	}
}

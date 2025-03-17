using System;
using System.Net;
using System.Net.NetworkInformation;
using ClassicUO.IO;
using ClassicUO.Utility.Logging;

namespace ClassicUO.Game.Scenes;

internal class ServerListEntry
{
	private IPAddress _ipAddress;

	private Ping _pinger = new Ping();

	private bool _sending;

	private readonly bool[] _last10Results = new bool[10];

	private int _resultIndex;

	public uint Address;

	public ushort Index;

	public string Name;

	public byte PercentFull;

	public byte Timezone;

	public int Ping = -1;

	public int PacketLoss;

	public IPStatus PingStatus;

	private static byte[] _buffData = new byte[32];

	private static PingOptions _pingOptions = new PingOptions(64, dontFragment: true);

	private ServerListEntry()
	{
	}

	public static ServerListEntry Create(ref StackDataReader p)
	{
		ServerListEntry serverListEntry = new ServerListEntry
		{
			Index = p.ReadUInt16BE(),
			Name = p.ReadASCII(32, safe: true),
			PercentFull = p.ReadUInt8(),
			Timezone = p.ReadUInt8(),
			Address = p.ReadUInt32BE()
		};
		try
		{
			serverListEntry._ipAddress = new IPAddress(new byte[4]
			{
				(byte)((serverListEntry.Address >> 24) & 0xFF),
				(byte)((serverListEntry.Address >> 16) & 0xFF),
				(byte)((serverListEntry.Address >> 8) & 0xFF),
				(byte)(serverListEntry.Address & 0xFF)
			});
		}
		catch (Exception ex)
		{
			Log.Error(ex.ToString());
		}
		serverListEntry._pinger.PingCompleted += serverListEntry.PingerOnPingCompleted;
		return serverListEntry;
	}

	public void DoPing()
	{
		if (_ipAddress != null && !_sending && _pinger != null)
		{
			if (_resultIndex >= _last10Results.Length)
			{
				_resultIndex = 0;
			}
			try
			{
				_pinger.SendAsync(_ipAddress, 1000, _buffData, _pingOptions, _resultIndex++);
				_sending = true;
			}
			catch
			{
				_ipAddress = null;
				Dispose();
			}
		}
	}

	private void PingerOnPingCompleted(object sender, PingCompletedEventArgs e)
	{
		int num = (int)e.UserState;
		if (e.Reply != null)
		{
			Ping = (int)e.Reply.RoundtripTime;
			PingStatus = e.Reply.Status;
			_last10Results[num] = e.Reply.Status == IPStatus.Success;
		}
		PacketLoss = 0;
		for (int i = 0; i < _resultIndex; i++)
		{
			if (!_last10Results[i])
			{
				PacketLoss++;
			}
		}
		PacketLoss = Math.Max(1, PacketLoss) / Math.Max(1, _resultIndex) * 100;
		_sending = false;
	}

	public void Dispose()
	{
		if (_pinger == null)
		{
			return;
		}
		_pinger.PingCompleted -= PingerOnPingCompleted;
		if (_sending)
		{
			try
			{
				_pinger.SendAsyncCancel();
			}
			catch
			{
			}
		}
		_pinger.Dispose();
		_pinger = null;
	}
}

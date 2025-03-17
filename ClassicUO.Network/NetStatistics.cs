using System;
using System.Diagnostics;

namespace ClassicUO.Network;

internal class NetStatistics
{
	private NetClient _socket;

	private uint _lastTotalBytesReceived;

	private uint _lastTotalBytesSent;

	private uint _lastTotalPacketsReceived;

	private uint _lastTotalPacketsSent;

	private byte _pingIdx;

	private readonly uint[] _pings = new uint[5];

	private readonly Stopwatch _pingStopwatch = new Stopwatch();

	public DateTime ConnectedFrom { get; set; }

	public uint TotalBytesReceived { get; set; }

	public uint TotalBytesSent { get; set; }

	public uint TotalPacketsReceived { get; set; }

	public uint TotalPacketsSent { get; set; }

	public uint DeltaBytesReceived { get; private set; }

	public uint DeltaBytesSent { get; private set; }

	public uint DeltaPacketsReceived { get; private set; }

	public uint DeltaPacketsSent { get; private set; }

	public uint Ping
	{
		get
		{
			byte b = 0;
			uint num = 0u;
			for (byte b2 = 0; b2 < 5; b2++)
			{
				if (_pings[b2] != 0)
				{
					b++;
					num += _pings[b2];
				}
			}
			if (b == 0)
			{
				return 0u;
			}
			return num / b;
		}
	}

	public NetStatistics(NetClient socket)
	{
		_socket = socket;
	}

	public void PingReceived()
	{
		_pings[_pingIdx++] = (uint)_pingStopwatch.ElapsedMilliseconds;
		if (_pingIdx >= _pings.Length)
		{
			_pingIdx = 0;
		}
		_pingStopwatch.Stop();
	}

	public void SendPing()
	{
		if (_socket.IsConnected && !_socket.IsDisposed)
		{
			_pingStopwatch.Restart();
			_socket.Send_Ping();
		}
	}

	public void Reset()
	{
		_pingStopwatch.Reset();
		ConnectedFrom = DateTime.MinValue;
		_lastTotalBytesReceived = (_lastTotalBytesSent = (_lastTotalPacketsReceived = (_lastTotalPacketsSent = 0u)));
		uint num2 = (TotalPacketsSent = 0u);
		uint num4 = (TotalPacketsReceived = num2);
		uint totalBytesReceived = (TotalBytesSent = num4);
		TotalBytesReceived = totalBytesReceived;
		num2 = (DeltaPacketsSent = 0u);
		num4 = (DeltaPacketsReceived = num2);
		totalBytesReceived = (DeltaBytesSent = num4);
		DeltaBytesReceived = totalBytesReceived;
	}

	public void Update()
	{
		DeltaBytesReceived = TotalBytesReceived - _lastTotalBytesReceived;
		DeltaBytesSent = TotalBytesSent - _lastTotalBytesSent;
		DeltaPacketsReceived = TotalPacketsReceived - _lastTotalPacketsReceived;
		DeltaPacketsSent = TotalPacketsSent - _lastTotalPacketsSent;
		_lastTotalBytesReceived = TotalBytesReceived;
		_lastTotalBytesSent = TotalBytesSent;
		_lastTotalPacketsReceived = TotalPacketsReceived;
		_lastTotalPacketsSent = TotalPacketsSent;
	}

	public override string ToString()
	{
		return $"Packets:\n >> {DeltaPacketsReceived}\n << {DeltaPacketsSent}\nBytes:\n >> {GetSizeAdaptive(DeltaBytesReceived)}\n << {GetSizeAdaptive(DeltaBytesSent)}";
	}

	public static string GetSizeAdaptive(long bytes)
	{
		decimal num = bytes;
		string arg = "B";
		if (!(num < 1024m))
		{
			arg = "KB";
			num /= 1024m;
			if (!(num < 1024m))
			{
				arg = "MB";
				num /= 1024m;
				if (!(num < 1024m))
				{
					arg = "GB";
					num /= 1024m;
				}
			}
		}
		return $"{Math.Round(num, 2):0.##} {arg}";
	}
}

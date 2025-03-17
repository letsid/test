using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ClassicUO.Network.Encryption;
using ClassicUO.Utility;
using ClassicUO.Utility.Logging;

namespace ClassicUO.Network;

internal sealed class NetClient
{
	private const int BUFF_SIZE = 65536;

	private bool _isCompressionEnabled;

	private int _sendingCount;

	private byte[] _sendingBuffer;

	private CircularBuffer _circularBuffer;

	private CircularBuffer _incompleteBuffer;

	private ConcurrentQueue<byte[]> _pluginRecvQueue = new ConcurrentQueue<byte[]>();

	private readonly bool _is_login_socket;

	private Socket _socket;

	private uint? _localIP;

	private static LogFile _logFile;

	public static NetClient LoginSocket { get; } = new NetClient(is_login_socket: true);

	public static NetClient Socket { get; } = new NetClient(is_login_socket: false);

	public bool IsConnected
	{
		get
		{
			if (_socket != null)
			{
				return _socket.Connected;
			}
			return false;
		}
	}

	public bool IsDisposed { get; private set; }

	public ClientSocketStatus Status { get; private set; }

	public uint LocalIP
	{
		get
		{
			if (!_localIP.HasValue)
			{
				try
				{
					byte[] array = (_socket?.LocalEndPoint as IPEndPoint)?.Address.MapToIPv4().GetAddressBytes();
					if (array != null && array.Length != 0)
					{
						_localIP = (uint)(array[0] | (array[1] << 8) | (array[2] << 16) | (array[3] << 24));
					}
					if (!_localIP.HasValue || _localIP == 0)
					{
						_localIP = 16777343u;
					}
				}
				catch (Exception arg)
				{
					Log.Error($"error while retriving local endpoint address: \n{arg}");
					_localIP = 16777343u;
				}
			}
			return _localIP.Value;
		}
	}

	public NetStatistics Statistics { get; }

	public event EventHandler Connected;

	public event EventHandler<SocketError> Disconnected;

	private NetClient(bool is_login_socket)
	{
		_is_login_socket = is_login_socket;
		Statistics = new NetStatistics(this);
	}

	public static void EnqueuePacketFromPlugin(byte[] data, int length)
	{
		if (LoginSocket.IsDisposed && Socket.IsConnected)
		{
			Socket._pluginRecvQueue.Enqueue(data);
			Socket.Statistics.TotalPacketsReceived++;
		}
		else if (Socket.IsDisposed && LoginSocket.IsConnected)
		{
			LoginSocket._pluginRecvQueue.Enqueue(data);
			LoginSocket.Statistics.TotalPacketsReceived++;
		}
		else
		{
			Log.Error("Attempt to write into a dead socket");
		}
	}

	public Task<bool> Connect(string ip, ushort port)
	{
		IsDisposed = false;
		IPAddress iPAddress = ResolveIP(ip);
		if (iPAddress == null)
		{
			return Task.FromResult(result: false);
		}
		return Connect(iPAddress, port);
	}

	public async Task<bool> Connect(IPAddress address, ushort port)
	{
		IsDisposed = false;
		if (Status != 0)
		{
			Log.Warn($"Socket status: {Status}");
			return false;
		}
		_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		_sendingBuffer = new byte[4096];
		_sendingCount = 0;
		_circularBuffer = new CircularBuffer(65536);
		_incompleteBuffer = new CircularBuffer(512);
		_pluginRecvQueue = new ConcurrentQueue<byte[]>();
		Statistics.Reset();
		Status = ClientSocketStatus.Connecting;
		try
		{
			CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
			cancellationTokenSource.CancelAfter(2000);
			await _socket.ConnectAsync(address, port, cancellationTokenSource.Token);
			if (_socket.Connected)
			{
				Status = ClientSocketStatus.Connected;
				this.Connected.Raise();
				Statistics.ConnectedFrom = DateTime.Now;
				return true;
			}
			Status = ClientSocketStatus.Disconnected;
			Log.Error("socket not connected");
			_circularBuffer = null;
			_incompleteBuffer = null;
			_sendingBuffer = null;
			_pluginRecvQueue = null;
			_socket?.Close();
			_socket = null;
			return false;
		}
		catch (SocketException ex)
		{
			Log.Error($"Socket error when connecting:\n{ex}");
			_logFile?.Write($"connection error: {ex}");
			Disconnect(ex.SocketErrorCode);
			return false;
		}
		catch (OperationCanceledException)
		{
			Status = ClientSocketStatus.Disconnected;
			_circularBuffer = null;
			_incompleteBuffer = null;
			_sendingBuffer = null;
			_pluginRecvQueue = null;
			_socket?.Close();
			_socket = null;
			return false;
		}
	}

	public void Disconnect()
	{
		Disconnect(SocketError.Success);
	}

	private void Disconnect(SocketError error)
	{
		_logFile?.Write($"disconnection  -  socket_error: {error}");
		if (IsDisposed)
		{
			return;
		}
		Status = ClientSocketStatus.Disconnected;
		IsDisposed = true;
		if (_socket != null)
		{
			try
			{
				_socket.Close();
			}
			catch
			{
			}
			try
			{
				_socket?.Dispose();
			}
			catch
			{
			}
			Log.Trace("Disconnected [" + (_is_login_socket ? "login socket" : "game socket") + "]");
			_isCompressionEnabled = false;
			_socket = null;
			_circularBuffer = null;
			_incompleteBuffer = null;
			_localIP = null;
			_sendingBuffer = null;
			_sendingCount = 0;
			if (error != 0)
			{
				this.Disconnected.Raise(error);
			}
			Statistics.Reset();
		}
	}

	public void EnableCompression()
	{
		_isCompressionEnabled = true;
	}

	public void Send(byte[] data, int length, bool ignorePlugin = false, bool skip_encryption = false)
	{
		if (ignorePlugin || Plugin.ProcessSendPacket(data, ref length))
		{
			Send(data, length, skip_encryption);
		}
	}

	private void Send(byte[] data, int length, bool skip_encryption)
	{
		if (_socket != null && !IsDisposed && _socket.Connected && data != null && data.Length != 0 && length > 0)
		{
			if (CUOEnviroment.PacketLog)
			{
				LogPacket(data, length, toServer: true);
			}
			if (!skip_encryption)
			{
				EncryptionHelper.Encrypt(_is_login_socket, data, data, length);
			}
			if (_sendingCount + length >= _sendingBuffer.Length)
			{
				ProcessSend();
			}
			data.AsSpan(0, length).CopyTo(_sendingBuffer.AsSpan(_sendingCount, length));
			_sendingCount += length;
			Statistics.TotalBytesSent += (uint)length;
			Statistics.TotalPacketsSent++;
		}
	}

	public void Update()
	{
		ProcessRecv();
		byte[] result;
		while (_pluginRecvQueue.TryDequeue(out result) && result != null && result.Length != 0)
		{
			short packetLength = PacketsTable.GetPacketLength(result[0]);
			int offset = 1;
			if (packetLength == -1)
			{
				if (result.Length < 3)
				{
					continue;
				}
				offset = 3;
			}
			PacketHandlers.Handlers.AnalyzePacket(result, offset, result.Length);
		}
		ProcessSend();
	}

	private void ExtractPackets()
	{
		if (!IsConnected || _circularBuffer == null || _circularBuffer.Length <= 0)
		{
			return;
		}
		lock (_circularBuffer)
		{
			int num = _circularBuffer.Length;
			byte[] array = ArrayPool<byte>.Shared.Rent(512);
			int offset;
			int length;
			while (num > 0 && IsConnected && GetPacketInfo(_circularBuffer, num, out offset, out length))
			{
				if (length > 0)
				{
					if (length > array.Length)
					{
						ArrayPool<byte>.Shared.Return(array);
						array = ArrayPool<byte>.Shared.Rent(length);
					}
					_circularBuffer.Dequeue(array, 0, length);
					if (CUOEnviroment.PacketLog)
					{
						LogPacket(array, length, toServer: false);
					}
					if (Plugin.ProcessRecvPacket(array, ref length))
					{
						PacketHandlers.Handlers.AnalyzePacket(array, offset, length);
						Statistics.TotalPacketsReceived++;
					}
				}
				num = _circularBuffer?.Length ?? 0;
			}
			ArrayPool<byte>.Shared.Return(array);
		}
	}

	private static bool GetPacketInfo(CircularBuffer buffer, int bufferLength, out int offset, out int length)
	{
		if (buffer == null || bufferLength <= 0)
		{
			length = 0;
			offset = 0;
			return false;
		}
		length = PacketsTable.GetPacketLength(buffer.GetID());
		offset = 1;
		if (length == -1)
		{
			if (bufferLength < 3)
			{
				return false;
			}
			length = buffer.GetLength();
			offset = 3;
		}
		return bufferLength >= length;
	}

	private void ProcessRecv()
	{
		if (IsDisposed || Status != ClientSocketStatus.Connected)
		{
			return;
		}
		if (!IsConnected && !IsDisposed)
		{
			Disconnect();
			return;
		}
		int num = _socket.Available;
		if (num <= 0)
		{
			return;
		}
		byte[] array = ArrayPool<byte>.Shared.Rent(16386);
		while (num > 0)
		{
			try
			{
				int size = Math.Min(4096, num);
				SocketError errorCode;
				int length = _socket.Receive(array, 0, size, SocketFlags.None, out errorCode);
				if (length > 0 && errorCode == SocketError.Success)
				{
					num -= length;
					Statistics.TotalBytesReceived += (uint)length;
					if (!_is_login_socket)
					{
						EncryptionHelper.Decrypt(array, array, length);
					}
					if (_isCompressionEnabled)
					{
						DecompressBuffer(array, ref length);
					}
					_circularBuffer.Enqueue(array.AsSpan(0, length));
					ExtractPackets();
					continue;
				}
				Log.Warn("Server sent 0 bytes. Closing connection");
				_logFile?.Write($"disconnection  -  received {length} bytes from server. ErrorCode: {errorCode}");
				Disconnect(SocketError.SocketError);
			}
			catch (SocketException ex)
			{
				Log.Error("socket error when receiving:\n" + (object)ex);
				_logFile?.Write($"disconnection  -  error while reading from socket: {ex}");
				Disconnect(ex.SocketErrorCode);
			}
			catch (Exception ex2)
			{
				if (ex2.InnerException is SocketException ex3)
				{
					Log.Error("socket error when receiving:\n" + (object)ex3);
					_logFile?.Write($"disconnection  -  error while reading from socket [1]: {ex3}");
					Disconnect(ex3.SocketErrorCode);
					break;
				}
				Log.Error("fatal error when receiving:\n" + ex2);
				_logFile?.Write($"disconnection  -  error while reading from socket [2]: {ex2}");
				Disconnect();
				throw;
			}
			break;
		}
		ArrayPool<byte>.Shared.Return(array);
	}

	private void ProcessSend()
	{
		if (IsDisposed || Status != ClientSocketStatus.Connected)
		{
			return;
		}
		if (!IsConnected && !IsDisposed)
		{
			Disconnect();
		}
		else
		{
			if (_sendingCount <= 0)
			{
				return;
			}
			try
			{
				_socket.Send(_sendingBuffer, _sendingCount, SocketFlags.None);
				_sendingCount = 0;
			}
			catch (SocketException ex)
			{
				Log.Error("socket error when sending:\n" + (object)ex);
				_logFile?.Write($"disconnection  -  error during writing to the socket buffer: {ex}");
				Disconnect(ex.SocketErrorCode);
			}
			catch (Exception ex2)
			{
				if (ex2.InnerException is SocketException ex3)
				{
					Log.Error("main exception:\n" + ex2);
					Log.Error("socket error when sending:\n" + (object)ex3);
					_logFile?.Write($"disconnection  -  error during writing to the socket buffer [2]: {ex3}");
					Disconnect(ex3.SocketErrorCode);
					return;
				}
				Log.Error("fatal error when sending:\n" + ex2);
				_logFile?.Write($"disconnection  -  error during writing to the socket buffer [3]: {ex2}");
				Disconnect();
				throw;
			}
		}
	}

	private void DecompressBuffer(Span<byte> buffer, ref int length)
	{
		int length2 = _incompleteBuffer.Length;
		int num = length2 + length;
		int num2 = num * 4 + 2;
		byte[] array = null;
		Span<byte> span = ((num2 > 1024) ? ((Span<byte>)(array = ArrayPool<byte>.Shared.Rent(num2))) : stackalloc byte[1024]);
		Span<byte> span2 = span;
		if (length2 > 0)
		{
			_incompleteBuffer.Dequeue(span2, 0, length2);
			_incompleteBuffer.Clear();
		}
		buffer.Slice(0, length).CopyTo(span2.Slice(length2));
		int num3 = 0;
		int srcOffset = 0;
		int i;
		int destLength;
		for (i = 0; Huffman.DecompressChunk(span2, ref srcOffset, num, buffer, i, out destLength); i += destLength)
		{
			num3 = srcOffset;
		}
		length = i;
		if (num3 < num)
		{
			_incompleteBuffer.Enqueue(span2, num3, num - num3);
		}
		if (array != null)
		{
			ArrayPool<byte>.Shared.Return(array);
		}
	}

	private static IPAddress ResolveIP(string addr)
	{
		IPAddress address = IPAddress.None;
		if (string.IsNullOrEmpty(addr))
		{
			return address;
		}
		if (!IPAddress.TryParse(addr, out address))
		{
			try
			{
				IPHostEntry hostEntry = Dns.GetHostEntry(addr);
				if (hostEntry.AddressList.Length != 0)
				{
					address = hostEntry.AddressList[hostEntry.AddressList.Length - 1];
				}
			}
			catch
			{
			}
		}
		return address;
	}

	private static void LogPacket(Span<byte> buffer, int length, bool toServer)
	{
		if (_logFile == null)
		{
			_logFile = new LogFile(FileSystemHelper.CreateFolderIfNotExists(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Alathair", "Fehlermeldungen"), "packets.log");
		}
		Span<char> initialBuffer = stackalloc char[256];
		ValueStringBuilder valueStringBuilder = new ValueStringBuilder(initialBuffer);
		int count = 10;
		valueStringBuilder.Append(' ', count);
		valueStringBuilder.Append(string.Format("Ticks: {0} | {1} |  ID: {2:X2}   Length: {3}\n", Time.Ticks, toServer ? "Client -> Server" : "Server -> Client", buffer[0], length));
		if (buffer[0] == 128 || buffer[0] == 145)
		{
			valueStringBuilder.Append(' ', count);
			valueStringBuilder.Append("[ACCOUNT CREDENTIALS HIDDEN]\n");
		}
		else
		{
			valueStringBuilder.Append(' ', count);
			valueStringBuilder.Append("0  1  2  3  4  5  6  7   8  9  A  B  C  D  E  F\n");
			valueStringBuilder.Append(' ', count);
			valueStringBuilder.Append("-- -- -- -- -- -- -- --  -- -- -- -- -- -- -- --\n");
			ulong num = 0uL;
			int num2 = 0;
			while (num2 < length)
			{
				valueStringBuilder.Append($"{num:X8}");
				for (int i = 0; i < 16; i++)
				{
					if (i % 8 == 0)
					{
						valueStringBuilder.Append(" ");
					}
					if (num2 + i < length)
					{
						valueStringBuilder.Append($" {buffer[num2 + i]:X2}");
					}
					else
					{
						valueStringBuilder.Append("   ");
					}
				}
				valueStringBuilder.Append("  ");
				for (int j = 0; j < 16 && num2 + j < length; j++)
				{
					byte b = buffer[num2 + j];
					if (b >= 32 && b < 128)
					{
						valueStringBuilder.Append((char)b);
					}
					else
					{
						valueStringBuilder.Append('.');
					}
				}
				valueStringBuilder.Append('\n');
				num2 += 16;
				num += 16;
			}
		}
		valueStringBuilder.Append('\n');
		valueStringBuilder.Append('\n');
		_logFile.Write(valueStringBuilder.ToString());
		valueStringBuilder.Dispose();
	}
}

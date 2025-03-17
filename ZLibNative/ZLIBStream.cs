using System;
using System.IO;
using System.IO.Compression;

namespace ZLibNative;

public sealed class ZLIBStream : Stream
{
	private readonly Adler32 _Adler32 = new Adler32();

	private bool _Closed;

	private readonly CompressionLevel _CompressionLevel = CompressionLevel.NoCompression;

	private readonly CompressionMode _CompressionMode = CompressionMode.Compress;

	private byte[] _CRC;

	private DeflateStream _DeflateStream;

	private readonly bool _LeaveOpen;

	private readonly Stream _RawStream;

	public override bool CanRead
	{
		get
		{
			if (_CompressionMode == CompressionMode.Decompress)
			{
				return !_Closed;
			}
			return false;
		}
	}

	public override bool CanWrite
	{
		get
		{
			if (_CompressionMode == CompressionMode.Compress)
			{
				return !_Closed;
			}
			return false;
		}
	}

	public override bool CanSeek => false;

	public override long Length
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override long Position
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public ZLIBStream(Stream stream, CompressionLevel compressionLevel)
		: this(stream, compressionLevel, leaveOpen: false)
	{
	}

	public ZLIBStream(Stream stream, CompressionMode compressionMode)
		: this(stream, compressionMode, leaveOpen: false)
	{
	}

	public ZLIBStream(Stream stream, CompressionLevel compressionLevel, bool leaveOpen)
	{
		_CompressionMode = CompressionMode.Compress;
		_CompressionLevel = compressionLevel;
		_LeaveOpen = leaveOpen;
		_RawStream = stream;
		InitStream();
	}

	public ZLIBStream(Stream stream, CompressionMode compressionMode, bool leaveOpen)
	{
		_CompressionMode = compressionMode;
		_CompressionLevel = CompressionLevel.Fastest;
		_LeaveOpen = leaveOpen;
		_RawStream = stream;
		InitStream();
	}

	public override int ReadByte()
	{
		if (CanRead)
		{
			int num = _DeflateStream.ReadByte();
			if (num == -1)
			{
				ReadCRC();
			}
			else
			{
				_Adler32.Update(Convert.ToByte(num));
			}
			return num;
		}
		throw new InvalidOperationException();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (CanRead)
		{
			int num = _DeflateStream.Read(buffer, offset, count);
			if (num < 1 && count > 0)
			{
				ReadCRC();
			}
			else
			{
				_Adler32.Update(buffer, offset, num);
			}
			return num;
		}
		throw new InvalidOperationException();
	}

	public override void WriteByte(byte value)
	{
		if (CanWrite)
		{
			_DeflateStream.WriteByte(value);
			_Adler32.Update(value);
			return;
		}
		throw new InvalidOperationException();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		if (CanWrite)
		{
			_DeflateStream.Write(buffer, offset, count);
			_Adler32.Update(buffer, offset, count);
			return;
		}
		throw new InvalidOperationException();
	}

	public override void Close()
	{
		if (_Closed)
		{
			return;
		}
		_Closed = true;
		if (_CompressionMode == CompressionMode.Compress)
		{
			Flush();
			_DeflateStream.Close();
			_CRC = BitConverter.GetBytes(_Adler32.GetValue());
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(_CRC);
			}
			_RawStream.Write(_CRC, 0, _CRC.Length);
		}
		else
		{
			_DeflateStream.Close();
			if (_CRC == null)
			{
				ReadCRC();
			}
		}
		if (!_LeaveOpen)
		{
			_RawStream.Close();
		}
	}

	public override void Flush()
	{
		if (_DeflateStream != null)
		{
			_DeflateStream.Flush();
		}
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotImplementedException();
	}

	public override void SetLength(long value)
	{
		throw new NotImplementedException();
	}

	public static bool IsZLibStream(Stream stream)
	{
		bool result = false;
		if (stream.Position != 0L)
		{
			throw new ArgumentOutOfRangeException("Sequence must be at position 0");
		}
		if (stream.CanRead)
		{
			int pCMF = stream.ReadByte();
			int pFlag = stream.ReadByte();
			result = ZLibHeader.DecodeHeader(pCMF, pFlag).IsSupportedZLibStream;
		}
		return result;
	}

	private void ReadCRC()
	{
		_CRC = new byte[4];
		_RawStream.Seek(-4L, SeekOrigin.End);
		if (_RawStream.Read(_CRC, 0, 4) < 4)
		{
			throw new EndOfStreamException();
		}
		if (BitConverter.IsLittleEndian)
		{
			Array.Reverse(_CRC);
		}
		uint value = _Adler32.GetValue();
		if (BitConverter.ToUInt32(_CRC, 0) != value)
		{
			throw new Exception("CRC mismatch");
		}
	}

	private void InitStream()
	{
		switch (_CompressionMode)
		{
		case CompressionMode.Compress:
			InitZLibHeader();
			_DeflateStream = new DeflateStream(_RawStream, _CompressionLevel, leaveOpen: true);
			break;
		case CompressionMode.Decompress:
			if (!IsZLibStream(_RawStream))
			{
				throw new InvalidDataException();
			}
			_DeflateStream = new DeflateStream(_RawStream, CompressionMode.Decompress, leaveOpen: true);
			break;
		}
	}

	private void InitZLibHeader()
	{
		ZLibHeader zLibHeader = new ZLibHeader
		{
			CompressionMethod = 8,
			CompressionInfo = 7,
			FDict = false
		};
		switch (_CompressionLevel)
		{
		case CompressionLevel.NoCompression:
			zLibHeader.FLevel = FLevel.Faster;
			break;
		case CompressionLevel.Fastest:
			zLibHeader.FLevel = FLevel.Default;
			break;
		case CompressionLevel.Optimal:
			zLibHeader.FLevel = FLevel.Optimal;
			break;
		}
		byte[] array = zLibHeader.EncodeZlibHeader();
		_RawStream.WriteByte(array[0]);
		_RawStream.WriteByte(array[1]);
	}
}

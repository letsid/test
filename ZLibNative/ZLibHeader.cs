using System;

namespace ZLibNative;

public sealed class ZLibHeader
{
	private byte _CompressionInfo;

	private byte _CompressionMethod;

	private byte _FCheck;

	public bool IsSupportedZLibStream { get; set; }

	public byte CompressionMethod
	{
		get
		{
			return _CompressionMethod;
		}
		set
		{
			if (value > 15)
			{
				throw new ArgumentOutOfRangeException("Argument cannot be greater than 15");
			}
			_CompressionMethod = value;
		}
	}

	public byte CompressionInfo
	{
		get
		{
			return _CompressionInfo;
		}
		set
		{
			if (value > 15)
			{
				throw new ArgumentOutOfRangeException("Argument cannot be greater than 15");
			}
			_CompressionInfo = value;
		}
	}

	public byte FCheck
	{
		get
		{
			return _FCheck;
		}
		set
		{
			if (value > 31)
			{
				throw new ArgumentOutOfRangeException("Argument cannot be greater than 31");
			}
			_FCheck = value;
		}
	}

	public bool FDict { get; set; }

	public FLevel FLevel { get; set; }

	private void RefreshFCheck()
	{
		byte b = (byte)(Convert.ToByte(FLevel) << 1);
		b |= Convert.ToByte(FDict);
		FCheck = Convert.ToByte(31 - Convert.ToByte((GetCMF() * 256 + b) % 31));
	}

	private byte GetCMF()
	{
		return (byte)((byte)(CompressionInfo << 4) | CompressionMethod);
	}

	private byte GetFLG()
	{
		return (byte)((byte)((byte)(Convert.ToByte(FLevel) << 6) | (byte)(Convert.ToByte(FDict) << 5)) | FCheck);
	}

	public byte[] EncodeZlibHeader()
	{
		byte[] array = new byte[2];
		RefreshFCheck();
		array[0] = GetCMF();
		array[1] = GetFLG();
		return array;
	}

	public static ZLibHeader DecodeHeader(int pCMF, int pFlag)
	{
		ZLibHeader zLibHeader = new ZLibHeader();
		pCMF &= 0xFF;
		pFlag &= 0xFF;
		zLibHeader.CompressionInfo = Convert.ToByte((pCMF & 0xF0) >> 4);
		zLibHeader.CompressionMethod = Convert.ToByte(pCMF & 0xF);
		zLibHeader.FCheck = Convert.ToByte(pFlag & 0x1F);
		zLibHeader.FDict = Convert.ToBoolean(Convert.ToByte((pFlag & 0x20) >> 5));
		zLibHeader.FLevel = (FLevel)Convert.ToByte((pFlag & 0xC0) >> 6);
		zLibHeader.IsSupportedZLibStream = zLibHeader.CompressionMethod == 8 && zLibHeader.CompressionInfo == 7 && (pCMF * 256 + pFlag) % 31 == 0 && !zLibHeader.FDict;
		return zLibHeader;
	}
}

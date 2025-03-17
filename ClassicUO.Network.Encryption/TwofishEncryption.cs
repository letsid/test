using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace ClassicUO.Network.Encryption;

internal class TwofishEncryption : TwofishBase, ICryptoTransform, IDisposable
{
	private byte[] _cipher_table;

	private byte[] _xor_data;

	private ushort _rect_pos;

	private byte _send_pos;

	private EncryptionDirection encryptionDirection;

	public bool CanReuseTransform { get; } = true;

	public bool CanTransformMultipleBlocks { get; }

	public int InputBlockSize => inputBlockSize;

	public int OutputBlockSize => outputBlockSize;

	public void Dispose()
	{
	}

	public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
	{
		Span<uint> x = stackalloc uint[4];
		for (int i = 0; i < 4; i++)
		{
			x[i] = (uint)((inputBuffer[i * 4 + 3 + inputOffset] << 24) | (inputBuffer[i * 4 + 2 + inputOffset] << 16) | (inputBuffer[i * 4 + 1 + inputOffset] << 8) | inputBuffer[i * 4 + inputOffset]);
		}
		if (encryptionDirection == EncryptionDirection.Encrypting)
		{
			blockEncrypt(ref x);
		}
		else
		{
			blockDecrypt(ref x);
		}
		for (int j = 0; j < 4; j++)
		{
			outputBuffer[j * 4 + outputOffset] = TwofishBase.b0(x[j]);
			outputBuffer[j * 4 + 1 + outputOffset] = TwofishBase.b1(x[j]);
			outputBuffer[j * 4 + 2 + outputOffset] = TwofishBase.b2(x[j]);
			outputBuffer[j * 4 + 3 + outputOffset] = TwofishBase.b3(x[j]);
		}
		return inputCount;
	}

	public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
	{
		byte[] array;
		if (inputCount > 0)
		{
			array = new byte[16];
			Span<uint> x = stackalloc uint[4];
			for (int i = 0; i < 4; i++)
			{
				x[i] = (uint)((inputBuffer[i * 4 + 3 + inputOffset] << 24) | (inputBuffer[i * 4 + 2 + inputOffset] << 16) | (inputBuffer[i * 4 + 1 + inputOffset] << 8) | inputBuffer[i * 4 + inputOffset]);
			}
			if (encryptionDirection == EncryptionDirection.Encrypting)
			{
				blockEncrypt(ref x);
			}
			else
			{
				blockDecrypt(ref x);
			}
			for (int j = 0; j < 4; j++)
			{
				array[j * 4] = TwofishBase.b0(x[j]);
				array[j * 4 + 1] = TwofishBase.b1(x[j]);
				array[j * 4 + 2] = TwofishBase.b2(x[j]);
				array[j * 4 + 3] = TwofishBase.b3(x[j]);
			}
		}
		else
		{
			array = new byte[0];
		}
		return array;
	}

	public void Initialize(uint seed, bool use_md5)
	{
		int keyLen = 128;
		_cipher_table = new byte[256];
		Span<byte> span = stackalloc byte[16];
		span[0] = (span[4] = (span[8] = (span[12] = (byte)((seed >> 24) & 0xFF))));
		span[1] = (span[5] = (span[9] = (span[13] = (byte)((seed >> 16) & 0xFF))));
		span[2] = (span[6] = (span[10] = (span[14] = (byte)((seed >> 8) & 0xFF))));
		span[3] = (span[7] = (span[11] = (span[15] = (byte)(seed & 0xFF))));
		byte[] array = new byte[0];
		for (int i = 0; i < span.Length / 4; i++)
		{
			Key[i] = (uint)((span[i * 4 + 3] << 24) | (span[i * 4 + 2] << 16) | (span[i * 4 + 1] << 8) | span[i * 4]);
		}
		cipherMode = CipherMode.ECB;
		if (cipherMode == CipherMode.CBC)
		{
			for (int j = 0; j < 4; j++)
			{
				IV[j] = (uint)((array[j * 4 + 3] << 24) | (array[j * 4 + 2] << 16) | (array[j * 4 + 1] << 8) | array[j * 4]);
			}
		}
		encryptionDirection = EncryptionDirection.Decrypting;
		reKey(keyLen, ref Key);
		for (int k = 0; k < 256; k++)
		{
			_cipher_table[k] = (byte)k;
		}
		_send_pos = 0;
		refreshCipherTable();
		if (use_md5)
		{
			MD5 mD = new MD5CryptoServiceProvider();
			_xor_data = mD.ComputeHash(_cipher_table, 0, 256);
			mD.Dispose();
		}
	}

	public void Encrypt(Span<byte> src, Span<byte> dst, int size)
	{
		for (int i = 0; i < size; i++)
		{
			if (_rect_pos >= 256)
			{
				refreshCipherTable();
			}
			dst[i] = (byte)(src[i] ^ _cipher_table[_rect_pos++]);
		}
	}

	public void Decrypt(Span<byte> src, Span<byte> dst, int size)
	{
		for (int i = 0; i < size; i++)
		{
			dst[i] = (byte)(src[i] ^ _xor_data[_send_pos]);
			_send_pos++;
			_send_pos &= 15;
		}
	}

	private void refreshCipherTable()
	{
		Span<uint> x = stackalloc uint[4];
		Span<byte> span = _cipher_table;
		for (int i = 0; i < 256; i += 16)
		{
			span.Slice(i, 16).CopyTo(MemoryMarshal.AsBytes(x));
			blockEncrypt(ref x);
			MemoryMarshal.AsBytes(x).CopyTo(span.Slice(i, 16));
		}
		_rect_pos = 0;
	}
}

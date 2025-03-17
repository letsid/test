using System;

namespace ClassicUO.Network.Encryption;

internal sealed class LoginCryptBehaviour
{
	private uint _k1;

	private uint _k2;

	private uint _k3;

	private readonly uint[] _key = new uint[2];

	private readonly byte[] _seed = new byte[4];

	public void Initialize(uint seed, uint k1, uint k2, uint k3)
	{
		_seed[0] = (byte)((seed >> 24) & 0xFF);
		_seed[1] = (byte)((seed >> 16) & 0xFF);
		_seed[2] = (byte)((seed >> 8) & 0xFF);
		_seed[3] = (byte)(seed & 0xFF);
		_k1 = k1;
		_k2 = k2;
		_k3 = k3;
		_key[0] = ((~seed ^ 0x1357) << 16) | ((seed ^ 0xFFFFAAAAu) & 0xFFFF);
		_key[1] = ((seed ^ 0x43210000) >> 16) | ((~seed ^ 0xABCDFFFFu) & 0xFFFF0000u);
	}

	public void Encrypt(Span<byte> src, Span<byte> dst, int size)
	{
		for (int i = 0; i < size; i++)
		{
			dst[i] = (byte)(src[i] ^ (byte)_key[0]);
			uint num = _key[0];
			uint num2 = _key[1];
			_key[1] = (((((num2 >> 1) | (num << 31)) ^ _k1) >> 1) | (num << 31)) ^ _k2;
			_key[0] = ((num >> 1) | (num2 << 31)) ^ _k3;
		}
	}

	public void Encrypt_OLD(Span<byte> src, Span<byte> dst, int size)
	{
		for (int i = 0; i < size; i++)
		{
			dst[i] = (byte)(src[i] ^ (byte)_key[0]);
			uint num = _key[0];
			uint num2 = _key[1];
			_key[0] = ((num >> 1) | (num2 << 31)) ^ _k2;
			_key[1] = ((num2 >> 1) | (num << 31)) ^ _k1;
		}
	}

	public void Encrypt_1_25_36(Span<byte> src, Span<byte> dst, int size)
	{
		for (int i = 0; i < size; i++)
		{
			dst[i] = (byte)(src[i] ^ (byte)_key[0]);
			uint num = _key[0];
			uint num2 = _key[1];
			_key[0] = ((num >> 1) | (num2 << 31)) ^ _k2;
			_key[1] = ((num2 >> 1) | (num << 31)) ^ _k1;
			_key[1] = (_k1 >> (int)(byte)((5 * num2 * num2) & 0xFF)) + num2 * _k1 + num * num * 902731137 + 128961591;
			_key[0] = (_k2 >> (int)(byte)((3 * num * num) & 0xFF)) + num * _k2 + _key[1] * _key[1] * 1278874451 + 384792639;
		}
	}
}

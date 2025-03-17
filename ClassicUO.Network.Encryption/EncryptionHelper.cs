using System;
using ClassicUO.Data;

namespace ClassicUO.Network.Encryption;

internal static class EncryptionHelper
{
	private static readonly LoginCryptBehaviour _loginCrypt = new LoginCryptBehaviour();

	private static readonly BlowfishEncryption _blowfishEncryption = new BlowfishEncryption();

	private static readonly TwofishEncryption _twoFishBehaviour = new TwofishEncryption();

	public static uint KEY_1;

	public static uint KEY_2;

	public static uint KEY_3;

	public static ENCRYPTION_TYPE Type;

	public static void CalculateEncryption(ClientVersion version)
	{
		if (version == ClientVersion.CV_200X)
		{
			KEY_1 = 756262396u;
			KEY_2 = 756262397u;
			KEY_3 = 2744996479u;
			Type = ENCRYPTION_TYPE.BLOWFISH__2_0_3;
			return;
		}
		int num = ((int)version >> 24) & 0xFF;
		int num2 = ((int)version >> 16) & 0xFF;
		int num3 = ((int)version >> 8) & 0xFF;
		KEY_2 = (uint)(((((((num << 9) | num2) << 10) | num3) ^ (num3 * num3 << 5)) << 4) ^ (num2 * num2) ^ (num2 * 184549376) ^ (num3 * 3670016) ^ 0x2C13A5FD);
		KEY_3 = (uint)((((((num << 9) | num3) << 10) | num2) * 8) ^ (num3 * num3 * 3072) ^ (num2 * num2) ^ (num2 * 109051904) ^ (num3 * 1835008) ^ 0xA31D527Fu);
		KEY_1 = KEY_2 - 1;
		if (version < (ClientVersion)18424576)
		{
			Type = ENCRYPTION_TYPE.OLD_BFISH;
		}
		else if (version == (ClientVersion)18424832)
		{
			Type = ENCRYPTION_TYPE.BLOWFISH__1_25_36;
		}
		else if (version <= ClientVersion.CV_200)
		{
			Type = ENCRYPTION_TYPE.BLOWFISH;
		}
		else if (version <= (ClientVersion)33555200)
		{
			Type = ENCRYPTION_TYPE.BLOWFISH__2_0_3;
		}
		else
		{
			Type = ENCRYPTION_TYPE.TWOFISH_MD5;
		}
	}

	public static void Initialize(bool is_login, uint seed, ENCRYPTION_TYPE encryption)
	{
		if (encryption == ENCRYPTION_TYPE.NONE)
		{
			return;
		}
		if (is_login)
		{
			_loginCrypt.Initialize(seed, KEY_1, KEY_2, KEY_3);
			return;
		}
		if (encryption >= ENCRYPTION_TYPE.OLD_BFISH && encryption < ENCRYPTION_TYPE.TWOFISH_MD5)
		{
			_blowfishEncryption.Initialize();
		}
		if (encryption == ENCRYPTION_TYPE.BLOWFISH__2_0_3 || encryption == ENCRYPTION_TYPE.TWOFISH_MD5)
		{
			_twoFishBehaviour.Initialize(seed, encryption == ENCRYPTION_TYPE.TWOFISH_MD5);
		}
	}

	public static void Encrypt(bool is_login, Span<byte> src, Span<byte> dst, int size)
	{
		if (Type == ENCRYPTION_TYPE.NONE)
		{
			return;
		}
		if (is_login)
		{
			if (Type == ENCRYPTION_TYPE.OLD_BFISH)
			{
				_loginCrypt.Encrypt_OLD(src, dst, size);
			}
			else if (Type == ENCRYPTION_TYPE.BLOWFISH__1_25_36)
			{
				_loginCrypt.Encrypt_1_25_36(src, dst, size);
			}
			else if (Type != 0)
			{
				_loginCrypt.Encrypt(src, dst, size);
			}
		}
		else if (Type == ENCRYPTION_TYPE.BLOWFISH__2_0_3)
		{
			int index_in = 0;
			int index_out = 0;
			_blowfishEncryption.Encrypt(src, dst, size, ref index_in, ref index_out);
			_twoFishBehaviour.Encrypt(dst, dst, size);
		}
		else if (Type == ENCRYPTION_TYPE.TWOFISH_MD5)
		{
			_twoFishBehaviour.Encrypt(src, dst, size);
		}
		else
		{
			int index_in2 = 0;
			int index_out2 = 0;
			_blowfishEncryption.Encrypt(src, dst, size, ref index_in2, ref index_out2);
		}
	}

	public static void Decrypt(Span<byte> src, Span<byte> dst, int size)
	{
		if (Type == ENCRYPTION_TYPE.TWOFISH_MD5)
		{
			_twoFishBehaviour.Decrypt(src, dst, size);
		}
	}
}

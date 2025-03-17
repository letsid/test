using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace ClassicUO.Network.Encryption;

internal class TwofishBase
{
	public enum EncryptionDirection
	{
		Encrypting,
		Decrypting
	}

	private int keyLength;

	private readonly int[] numRounds = new int[4] { 0, ROUNDS_128, ROUNDS_192, ROUNDS_256 };

	private int rounds;

	protected CipherMode cipherMode = CipherMode.ECB;

	protected int inputBlockSize = BLOCK_SIZE / 8;

	protected uint[] IV = new uint[4];

	protected uint[] Key = new uint[8];

	protected int outputBlockSize = BLOCK_SIZE / 8;

	protected uint[] sboxKeys = new uint[MAX_KEY_BITS / 64];

	protected uint[] subKeys = new uint[TOTAL_SUBKEYS];

	private static readonly int BLOCK_SIZE = 128;

	private static readonly int BLOCK_HALF_SIZE = BLOCK_SIZE >> 5;

	private static readonly int MAX_ROUNDS = 16;

	private static readonly int ROUNDS_128 = 16;

	private static readonly int ROUNDS_192 = 16;

	private static readonly int ROUNDS_256 = 16;

	private static readonly int MAX_KEY_BITS = 256;

	private static readonly int MIN_KEY_BITS = 128;

	private static readonly int INPUT_WHITEN = 0;

	private static readonly int OUTPUT_WHITEN = INPUT_WHITEN + BLOCK_SIZE / 32;

	private static readonly int ROUND_SUBKEYS = OUTPUT_WHITEN + BLOCK_SIZE / 32;

	private static readonly int TOTAL_SUBKEYS = ROUND_SUBKEYS + 2 * MAX_ROUNDS;

	private static readonly uint SK_STEP = 33686018u;

	private static readonly uint SK_BUMP = 16843009u;

	private static readonly int SK_ROTL = 9;

	private static readonly uint RS_GF_FDBK = 333u;

	private static readonly int MDS_GF_FDBK = 361;

	private static readonly int P_00 = 1;

	private static readonly int P_01 = 0;

	private static readonly int P_02 = 0;

	private static readonly int P_03 = P_01 ^ 1;

	private static readonly int P_04 = 1;

	private static readonly int P_10 = 0;

	private static readonly int P_11 = 0;

	private static readonly int P_12 = 1;

	private static readonly int P_13 = P_11 ^ 1;

	private static readonly int P_14 = 0;

	private static readonly int P_20 = 1;

	private static readonly int P_21 = 1;

	private static readonly int P_22 = 0;

	private static readonly int P_23 = P_21 ^ 1;

	private static readonly int P_24 = 0;

	private static readonly int P_30 = 0;

	private static readonly int P_31 = 1;

	private static readonly int P_32 = 1;

	private static readonly int P_33 = P_31 ^ 1;

	private static readonly int P_34 = 1;

	private static readonly byte[,] P8x8 = new byte[2, 256]
	{
		{
			169, 103, 179, 232, 4, 253, 163, 118, 154, 146,
			128, 120, 228, 221, 209, 56, 13, 198, 53, 152,
			24, 247, 236, 108, 67, 117, 55, 38, 250, 19,
			148, 72, 242, 208, 139, 48, 132, 84, 223, 35,
			25, 91, 61, 89, 243, 174, 162, 130, 99, 1,
			131, 46, 217, 81, 155, 124, 166, 235, 165, 190,
			22, 12, 227, 97, 192, 140, 58, 245, 115, 44,
			37, 11, 187, 78, 137, 107, 83, 106, 180, 241,
			225, 230, 189, 69, 226, 244, 182, 102, 204, 149,
			3, 86, 212, 28, 30, 215, 251, 195, 142, 181,
			233, 207, 191, 186, 234, 119, 57, 175, 51, 201,
			98, 113, 129, 121, 9, 173, 36, 205, 249, 216,
			229, 197, 185, 77, 68, 8, 134, 231, 161, 29,
			170, 237, 6, 112, 178, 210, 65, 123, 160, 17,
			49, 194, 39, 144, 32, 246, 96, 255, 150, 92,
			177, 171, 158, 156, 82, 27, 95, 147, 10, 239,
			145, 133, 73, 238, 45, 79, 143, 59, 71, 135,
			109, 70, 214, 62, 105, 100, 42, 206, 203, 47,
			252, 151, 5, 122, 172, 127, 213, 26, 75, 14,
			167, 90, 40, 20, 63, 41, 136, 60, 76, 2,
			184, 218, 176, 23, 85, 31, 138, 125, 87, 199,
			141, 116, 183, 196, 159, 114, 126, 21, 34, 18,
			88, 7, 153, 52, 110, 80, 222, 104, 101, 188,
			219, 248, 200, 168, 43, 64, 220, 254, 50, 164,
			202, 16, 33, 240, 211, 93, 15, 0, 111, 157,
			54, 66, 74, 94, 193, 224
		},
		{
			117, 243, 198, 244, 219, 123, 251, 200, 74, 211,
			230, 107, 69, 125, 232, 75, 214, 50, 216, 253,
			55, 113, 241, 225, 48, 15, 248, 27, 135, 250,
			6, 63, 94, 186, 174, 91, 138, 0, 188, 157,
			109, 193, 177, 14, 128, 93, 210, 213, 160, 132,
			7, 20, 181, 144, 44, 163, 178, 115, 76, 84,
			146, 116, 54, 81, 56, 176, 189, 90, 252, 96,
			98, 150, 108, 66, 247, 16, 124, 40, 39, 140,
			19, 149, 156, 199, 36, 70, 59, 112, 202, 227,
			133, 203, 17, 208, 147, 184, 166, 131, 32, 255,
			159, 119, 195, 204, 3, 111, 8, 191, 64, 231,
			43, 226, 121, 12, 170, 130, 65, 58, 234, 185,
			228, 154, 164, 151, 126, 218, 122, 23, 102, 148,
			161, 29, 61, 240, 222, 179, 11, 114, 167, 28,
			239, 209, 83, 62, 143, 51, 38, 95, 236, 118,
			42, 73, 129, 136, 238, 33, 196, 26, 235, 217,
			197, 57, 153, 205, 173, 49, 139, 1, 24, 35,
			221, 31, 78, 45, 249, 72, 79, 242, 101, 142,
			120, 92, 88, 25, 141, 229, 152, 87, 103, 127,
			5, 100, 175, 99, 182, 254, 245, 183, 60, 165,
			206, 233, 104, 68, 224, 77, 67, 105, 41, 46,
			172, 21, 89, 168, 10, 158, 110, 71, 223, 52,
			53, 106, 207, 220, 34, 201, 192, 155, 137, 212,
			237, 171, 18, 162, 13, 82, 187, 2, 47, 169,
			215, 97, 30, 180, 80, 4, 246, 194, 22, 37,
			134, 86, 85, 9, 190, 145
		}
	};

	private unsafe static uint f32(uint x, ref uint[] k32, int keyLen)
	{
		byte* ptr = stackalloc byte[4];
		*ptr = b0(x);
		ptr[1] = b1(x);
		ptr[2] = b2(x);
		ptr[3] = b3(x);
		switch (((keyLen + 63) / 64) & 3)
		{
		case 0:
			*ptr = (byte)(P8x8[P_04, *ptr] ^ b0(k32[3]));
			ptr[1] = (byte)(P8x8[P_14, ptr[1]] ^ b1(k32[3]));
			ptr[2] = (byte)(P8x8[P_24, ptr[2]] ^ b2(k32[3]));
			ptr[3] = (byte)(P8x8[P_34, ptr[3]] ^ b3(k32[3]));
			goto case 3;
		case 3:
			*ptr = (byte)(P8x8[P_03, *ptr] ^ b0(k32[2]));
			ptr[1] = (byte)(P8x8[P_13, ptr[1]] ^ b1(k32[2]));
			ptr[2] = (byte)(P8x8[P_23, ptr[2]] ^ b2(k32[2]));
			ptr[3] = (byte)(P8x8[P_33, ptr[3]] ^ b3(k32[2]));
			goto case 2;
		case 2:
			*ptr = P8x8[P_00, P8x8[P_01, P8x8[P_02, *ptr] ^ b0(k32[1])] ^ b0(k32[0])];
			ptr[1] = P8x8[P_10, P8x8[P_11, P8x8[P_12, ptr[1]] ^ b1(k32[1])] ^ b1(k32[0])];
			ptr[2] = P8x8[P_20, P8x8[P_21, P8x8[P_22, ptr[2]] ^ b2(k32[1])] ^ b2(k32[0])];
			ptr[3] = P8x8[P_30, P8x8[P_31, P8x8[P_32, ptr[3]] ^ b3(k32[1])] ^ b3(k32[0])];
			break;
		}
		return (uint)(M00(*ptr) ^ M01(ptr[1]) ^ M02(ptr[2]) ^ M03(ptr[3]) ^ ((M10(*ptr) ^ M11(ptr[1]) ^ M12(ptr[2]) ^ M13(ptr[3])) << 8) ^ ((M20(*ptr) ^ M21(ptr[1]) ^ M22(ptr[2]) ^ M23(ptr[3])) << 16) ^ ((M30(*ptr) ^ M31(ptr[1]) ^ M32(ptr[2]) ^ M33(ptr[3])) << 24));
	}

	protected bool reKey(int keyLen, ref uint[] key32)
	{
		keyLength = keyLen;
		rounds = numRounds[(keyLen - 1) / 64];
		int num = ROUND_SUBKEYS + 2 * rounds;
		uint[] k = new uint[MAX_KEY_BITS / 64];
		uint[] k2 = new uint[MAX_KEY_BITS / 64];
		int num2 = (keyLen + 63) / 64;
		for (int i = 0; i < num2; i++)
		{
			k[i] = key32[2 * i];
			k2[i] = key32[2 * i + 1];
			sboxKeys[num2 - 1 - i] = RS_MDS_Encode(k[i], k2[i]);
		}
		for (int i = 0; i < num / 2; i++)
		{
			uint num3 = f32((uint)(i * SK_STEP), ref k, keyLen);
			uint x = f32((uint)(i * SK_STEP + SK_BUMP), ref k2, keyLen);
			x = ROL(x, 8);
			subKeys[2 * i] = num3 + x;
			subKeys[2 * i + 1] = ROL(num3 + 2 * x, SK_ROTL);
		}
		return true;
	}

	protected void blockDecrypt(ref Span<uint> x)
	{
		Span<uint> destination = stackalloc uint[4];
		if (cipherMode == CipherMode.CBC)
		{
			x.CopyTo(destination);
		}
		for (int i = 0; i < BLOCK_HALF_SIZE; i++)
		{
			x[i] ^= subKeys[OUTPUT_WHITEN + i];
		}
		for (int num = rounds - 1; num >= 0; num--)
		{
			uint num2 = f32(x[0], ref sboxKeys, keyLength);
			uint num3 = f32(ROL(x[1], 8), ref sboxKeys, keyLength);
			x[2] = ROL(x[2], 1);
			x[2] ^= num2 + num3 + subKeys[ROUND_SUBKEYS + 2 * num];
			x[3] ^= num2 + 2 * num3 + subKeys[ROUND_SUBKEYS + 2 * num + 1];
			x[3] = ROR(x[3], 1);
			if (num > 0)
			{
				num2 = x[0];
				x[0] = x[2];
				x[2] = num2;
				num3 = x[1];
				x[1] = x[3];
				x[3] = num3;
			}
		}
		for (int j = 0; j < BLOCK_HALF_SIZE; j++)
		{
			x[j] ^= subKeys[INPUT_WHITEN + j];
			if (cipherMode == CipherMode.CBC)
			{
				x[j] ^= IV[j];
				IV[j] = destination[j];
			}
		}
	}

	public void blockEncrypt(ref Span<uint> x)
	{
		for (int i = 0; i < BLOCK_HALF_SIZE; i++)
		{
			x[i] ^= subKeys[INPUT_WHITEN + i];
			if (cipherMode == CipherMode.CBC)
			{
				x[i] ^= IV[i];
			}
		}
		for (int j = 0; j < rounds; j++)
		{
			uint num = f32(x[0], ref sboxKeys, keyLength);
			uint num2 = f32(ROL(x[1], 8), ref sboxKeys, keyLength);
			x[3] = ROL(x[3], 1);
			x[2] ^= num + num2 + subKeys[ROUND_SUBKEYS + 2 * j];
			x[3] ^= num + 2 * num2 + subKeys[ROUND_SUBKEYS + 2 * j + 1];
			x[2] = ROR(x[2], 1);
			if (j < rounds - 1)
			{
				uint num3 = x[0];
				x[0] = x[2];
				x[2] = num3;
				num3 = x[1];
				x[1] = x[3];
				x[3] = num3;
			}
		}
		for (int k = 0; k < BLOCK_HALF_SIZE; k++)
		{
			x[k] ^= subKeys[OUTPUT_WHITEN + k];
			if (cipherMode == CipherMode.CBC)
			{
				IV[k] = x[k];
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint RS_MDS_Encode(uint k0, uint k1)
	{
		uint x;
		for (uint num = (x = 0u); num < 2; num++)
		{
			x ^= ((num != 0) ? k0 : k1);
			for (uint num2 = 0u; num2 < 4; num2++)
			{
				RS_rem(ref x);
			}
		}
		return x;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void RS_rem(ref uint x)
	{
		byte b = (byte)(x >> 24);
		uint num = (uint)(((b << 1) ^ (((b & 0x80) == 128) ? RS_GF_FDBK : 0)) & 0xFF);
		uint num2 = (uint)(((b >> 1) & 0x7F) ^ (((b & 1) == 1) ? (RS_GF_FDBK >> 1) : 0) ^ num);
		x = (x << 8) ^ (num2 << 24) ^ (num << 16) ^ (num2 << 8) ^ b;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int LFSR1(int x)
	{
		return (x >> 1) ^ (((x & 1) == 1) ? (MDS_GF_FDBK / 2) : 0);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int LFSR2(int x)
	{
		return (x >> 2) ^ (((x & 2) == 2) ? (MDS_GF_FDBK / 2) : 0) ^ (((x & 1) == 1) ? (MDS_GF_FDBK / 4) : 0);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int Mx_1(int x)
	{
		return x;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int Mx_X(int x)
	{
		return x ^ LFSR2(x);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int Mx_Y(int x)
	{
		return x ^ LFSR1(x) ^ LFSR2(x);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int M00(int x)
	{
		return Mul_1(x);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int M01(int x)
	{
		return Mul_Y(x);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int M02(int x)
	{
		return Mul_X(x);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int M03(int x)
	{
		return Mul_X(x);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int M10(int x)
	{
		return Mul_X(x);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int M11(int x)
	{
		return Mul_Y(x);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int M12(int x)
	{
		return Mul_Y(x);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int M13(int x)
	{
		return Mul_1(x);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int M20(int x)
	{
		return Mul_Y(x);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int M21(int x)
	{
		return Mul_X(x);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int M22(int x)
	{
		return Mul_1(x);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int M23(int x)
	{
		return Mul_Y(x);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int M30(int x)
	{
		return Mul_Y(x);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int M31(int x)
	{
		return Mul_1(x);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int M32(int x)
	{
		return Mul_Y(x);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int M33(int x)
	{
		return Mul_X(x);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int Mul_1(int x)
	{
		return Mx_1(x);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int Mul_X(int x)
	{
		return Mx_X(x);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int Mul_Y(int x)
	{
		return Mx_Y(x);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint ROL(uint x, int n)
	{
		return (x << (n & 0x1F)) | (x >> 32 - (n & 0x1F));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint ROR(uint x, int n)
	{
		return (x >> (n & 0x1F)) | (x << 32 - (n & 0x1F));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static byte b0(uint x)
	{
		return (byte)x;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static byte b1(uint x)
	{
		return (byte)(x >> 8);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static byte b2(uint x)
	{
		return (byte)(x >> 16);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static byte b3(uint x)
	{
		return (byte)(x >> 24);
	}
}

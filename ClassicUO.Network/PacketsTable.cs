using ClassicUO.Data;

namespace ClassicUO.Network;

internal static class PacketsTable
{
	private static readonly short[] _packetsTable = new short[255]
	{
		104, 5, 7, -1, 2, 5, 5, 7, 14, 5,
		11, 266, -1, 3, -1, 61, 215, -1, -1, 10,
		6, 9, 1, -1, -1, -1, -1, 37, -1, 5,
		4, 8, 19, 8, 3, 26, 7, 20, 5, 2,
		5, 1, 5, 2, 2, 17, 15, 10, 5, 1,
		2, 2, 10, 653, -1, 8, 7, 9, -1, -1,
		-1, 2, 37, -1, 201, -1, -1, 553, 713, 5,
		-1, 11, 73, 93, 5, 9, -1, -1, 6, 2,
		-1, -1, -1, 2, 12, 1, 11, 110, 106, -1,
		-1, 4, 2, 73, -1, 49, 5, 9, 15, 13,
		1, 4, -1, 21, -1, -1, 3, 9, 19, 3,
		14, -1, 28, -1, 5, 2, -1, 35, 16, 17,
		-1, 9, -1, 2, -1, 13, 2, -1, 62, -1,
		2, 39, 69, 2, -1, -1, 70, -1, -1, -1,
		11, -1, -1, -1, 19, 65, -1, 99, -1, 9,
		-1, 2, -1, 26, -1, 258, 309, 51, -1, -1,
		3, 9, 9, 9, 149, -1, -1, 4, -1, -1,
		5, -1, -1, -1, -1, 13, -1, -1, -1, -1,
		-1, 64, 9, -1, -1, 3, 6, 9, 3, -1,
		-1, -1, 36, -1, -1, -1, 6, 203, 1, 49,
		2, 6, 6, 7, -1, 1, -1, 78, -1, 2,
		25, -1, -1, -1, -1, -1, -1, 268, -1, -1,
		9, -1, -1, -1, -1, -1, 10, -1, -1, -1,
		5, 12, 13, 75, 3, -1, -1, -1, 10, 21,
		-1, 9, 25, 26, -1, 21, -1, -1, 106, -1,
		-1, -1, -1, -1, -1
	};

	public static short GetPacketLength(int id)
	{
		return (short)((id >= 255) ? (-1) : _packetsTable[id]);
	}

	public static void AdjustPacketSizeByVersion(ClientVersion version)
	{
		if (version >= ClientVersion.CV_500A)
		{
			_packetsTable[11] = 7;
			_packetsTable[22] = -1;
			_packetsTable[49] = -1;
		}
		else
		{
			_packetsTable[11] = 266;
			_packetsTable[22] = 1;
			_packetsTable[49] = 1;
		}
		if (version >= ClientVersion.CV_5090)
		{
			_packetsTable[225] = -1;
		}
		else
		{
			_packetsTable[225] = 9;
		}
		if (version >= ClientVersion.CV_6013)
		{
			_packetsTable[227] = -1;
			_packetsTable[230] = 5;
			_packetsTable[231] = 12;
			_packetsTable[232] = 13;
			_packetsTable[233] = 75;
			_packetsTable[234] = 3;
		}
		else
		{
			_packetsTable[227] = 77;
			_packetsTable[230] = -1;
			_packetsTable[231] = -1;
			_packetsTable[232] = -1;
			_packetsTable[233] = -1;
			_packetsTable[234] = -1;
		}
		if (version >= ClientVersion.CV_6017)
		{
			_packetsTable[8] = 15;
			_packetsTable[37] = 21;
		}
		else
		{
			_packetsTable[8] = 14;
			_packetsTable[37] = 20;
		}
		if (version >= ClientVersion.CV_6060)
		{
			_packetsTable[238] = 8192;
			_packetsTable[239] = 8192;
			_packetsTable[241] = 9;
		}
		else
		{
			_packetsTable[238] = -1;
			_packetsTable[239] = 21;
			_packetsTable[241] = -1;
		}
		if (version >= ClientVersion.CV_60142)
		{
			_packetsTable[185] = 5;
		}
		else
		{
			_packetsTable[185] = 3;
		}
		if (version >= ClientVersion.CV_7000)
		{
			_packetsTable[238] = 10;
			_packetsTable[239] = 21;
		}
		else
		{
			_packetsTable[238] = -1;
			_packetsTable[239] = 21;
		}
		if (version >= ClientVersion.CV_7090)
		{
			_packetsTable[36] = 9;
			_packetsTable[153] = 30;
			_packetsTable[186] = 10;
			_packetsTable[243] = 26;
			_packetsTable[241] = 9;
			_packetsTable[242] = 25;
		}
		else
		{
			_packetsTable[36] = 7;
			_packetsTable[153] = 26;
			_packetsTable[186] = 6;
			_packetsTable[243] = 24;
			_packetsTable[241] = -1;
			_packetsTable[242] = -1;
		}
		if (version >= ClientVersion.CV_70180)
		{
			_packetsTable[0] = 106;
		}
		else
		{
			_packetsTable[0] = 104;
		}
		if (version >= ClientVersion.CV_706400)
		{
			_packetsTable[250] = 1;
			_packetsTable[251] = 2;
		}
	}
}

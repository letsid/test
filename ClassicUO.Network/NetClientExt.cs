using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ClassicUO.Configuration;
using ClassicUO.Data;
using ClassicUO.Game;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.IO;
using ClassicUO.IO.Resources;

namespace ClassicUO.Network;

internal static class NetClientExt
{
	public static void Send_ACKTalk(this NetClient socket)
	{
		int packetLength = PacketsTable.GetPacketLength(3);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(3);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt8(32);
		stackDataWriter.WriteUInt8(0);
		stackDataWriter.WriteUInt8(52);
		stackDataWriter.WriteUInt8(0);
		stackDataWriter.WriteUInt8(3);
		stackDataWriter.WriteUInt8(219);
		stackDataWriter.WriteUInt8(19);
		stackDataWriter.WriteUInt8(20);
		stackDataWriter.WriteUInt8(63);
		stackDataWriter.WriteUInt8(69);
		stackDataWriter.WriteUInt8(44);
		stackDataWriter.WriteUInt8(88);
		stackDataWriter.WriteUInt8(15);
		stackDataWriter.WriteUInt8(93);
		stackDataWriter.WriteUInt8(68);
		stackDataWriter.WriteUInt8(46);
		stackDataWriter.WriteUInt8(80);
		stackDataWriter.WriteUInt8(17);
		stackDataWriter.WriteUInt8(223);
		stackDataWriter.WriteUInt8(117);
		stackDataWriter.WriteUInt8(92);
		stackDataWriter.WriteUInt8(224);
		stackDataWriter.WriteUInt8(62);
		stackDataWriter.WriteUInt8(113);
		stackDataWriter.WriteUInt8(79);
		stackDataWriter.WriteUInt8(49);
		stackDataWriter.WriteUInt8(52);
		stackDataWriter.WriteUInt8(5);
		stackDataWriter.WriteUInt8(78);
		stackDataWriter.WriteUInt8(24);
		stackDataWriter.WriteUInt8(30);
		stackDataWriter.WriteUInt8(114);
		stackDataWriter.WriteUInt8(15);
		stackDataWriter.WriteUInt8(89);
		stackDataWriter.WriteUInt8(173);
		stackDataWriter.WriteUInt8(245);
		stackDataWriter.WriteUInt8(0);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_Ping(this NetClient socket)
	{
		int packetLength = PacketsTable.GetPacketLength(115);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(115);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt8(0);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_DoubleClick(this NetClient socket, uint serial)
	{
		int packetLength = PacketsTable.GetPacketLength(6);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(6);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(serial);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_Seed(this NetClient socket, uint v, byte major, byte minor, byte build, byte extra)
	{
		int packetLength = PacketsTable.GetPacketLength(239);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(239);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(v);
		stackDataWriter.WriteUInt32BE(major);
		stackDataWriter.WriteUInt32BE(minor);
		stackDataWriter.WriteUInt32BE(build);
		stackDataWriter.WriteUInt32BE(extra);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten, ignorePlugin: true, skip_encryption: true);
		stackDataWriter.Dispose();
	}

	public static void Send_Seed_Old(this NetClient socket, uint v)
	{
		StackDataWriter stackDataWriter = new StackDataWriter(4);
		stackDataWriter.WriteUInt32BE(v);
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten, ignorePlugin: true, skip_encryption: true);
		stackDataWriter.Dispose();
	}

	public static void Send_FirstLogin(this NetClient socket, string user, string psw)
	{
		int packetLength = PacketsTable.GetPacketLength(128);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(128);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteASCII(user, 30);
		stackDataWriter.WriteASCII(psw, 30);
		stackDataWriter.WriteUInt8(byte.MaxValue);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_SelectServer(this NetClient socket, byte index)
	{
		int packetLength = PacketsTable.GetPacketLength(160);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(160);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt8(0);
		stackDataWriter.WriteUInt8(index);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_SecondLogin(this NetClient socket, string user, string psw, uint seed)
	{
		int packetLength = PacketsTable.GetPacketLength(145);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(145);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(seed);
		stackDataWriter.WriteASCII(user, 30);
		stackDataWriter.WriteASCII(psw, 30);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_CreateCharacter(this NetClient socket, PlayerMobile character, int cityIndex, uint clientIP, int serverIndex, uint slot, byte profession)
	{
		byte b = 0;
		int num = 3;
		if (Client.Version >= ClientVersion.CV_70160)
		{
			b = 248;
			num++;
		}
		int packetLength = PacketsTable.GetPacketLength(b);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(b);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(3991793133u);
		stackDataWriter.WriteUInt32BE(uint.MaxValue);
		stackDataWriter.WriteUInt8(0);
		stackDataWriter.WriteASCII(character.Name, 30);
		stackDataWriter.WriteZero(2);
		stackDataWriter.WriteUInt32BE((uint)Client.Protocol);
		stackDataWriter.WriteUInt32BE(1u);
		stackDataWriter.WriteUInt32BE(0u);
		stackDataWriter.WriteUInt8(0);
		stackDataWriter.WriteZero(15);
		byte b2;
		if (Client.Version < ClientVersion.CV_4011D)
		{
			b2 = (character.Flags.HasFlag(Flags.Female) ? ((byte)1) : ((byte)0));
		}
		else
		{
			b2 = (byte)character.Race;
			if (Client.Version < ClientVersion.CV_7000)
			{
				b2--;
			}
			b2 = (byte)((uint)(b2 * 2) + (character.Flags.HasFlag(Flags.Female) ? 1u : 0u));
		}
		stackDataWriter.WriteUInt8(b2);
		stackDataWriter.WriteUInt8((byte)character.Strength);
		stackDataWriter.WriteUInt8((byte)character.Dexterity);
		stackDataWriter.WriteUInt8((byte)character.Intelligence);
		foreach (Skill item5 in character.Skills.OrderByDescending((Skill o) => o.Value).Take(num).ToList())
		{
			stackDataWriter.WriteUInt8((byte)item5.Index);
			stackDataWriter.WriteUInt8((byte)item5.ValueFixed);
		}
		stackDataWriter.WriteUInt16BE(character.Hue);
		Item item = character.FindItemByLayer(Layer.Hair);
		if (item != null)
		{
			stackDataWriter.WriteUInt16BE(item.Graphic);
			stackDataWriter.WriteUInt16BE(item.Hue);
		}
		else
		{
			stackDataWriter.WriteZero(4);
		}
		Item item2 = character.FindItemByLayer(Layer.Beard);
		if (item2 != null)
		{
			stackDataWriter.WriteUInt16BE(item2.Graphic);
			stackDataWriter.WriteUInt16BE(item2.Hue);
		}
		else
		{
			stackDataWriter.WriteZero(4);
		}
		stackDataWriter.WriteUInt16BE((ushort)cityIndex);
		stackDataWriter.WriteZero(2);
		stackDataWriter.WriteUInt16BE((ushort)slot);
		stackDataWriter.WriteUInt32BE(clientIP);
		Item item3 = character.FindItemByLayer(Layer.Shirt);
		if (item3 != null)
		{
			stackDataWriter.WriteUInt16BE(item3.Hue);
		}
		else
		{
			stackDataWriter.WriteZero(2);
		}
		Item item4 = character.FindItemByLayer(Layer.Pants);
		if (item4 != null)
		{
			stackDataWriter.WriteUInt16BE(item4.Hue);
		}
		else
		{
			stackDataWriter.WriteZero(2);
		}
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_DeleteCharacter(this NetClient socket, byte index, uint ipclient)
	{
		int packetLength = PacketsTable.GetPacketLength(131);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(131);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteZero(30);
		stackDataWriter.WriteUInt32BE(index);
		stackDataWriter.WriteUInt32BE(ipclient);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_SelectCharacter(this NetClient socket, uint index, string name, uint ipclient)
	{
		int packetLength = PacketsTable.GetPacketLength(93);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(93);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(3991793133u);
		stackDataWriter.WriteASCII(name, 30);
		stackDataWriter.WriteZero(2);
		stackDataWriter.WriteUInt32BE((uint)Client.Protocol);
		stackDataWriter.WriteZero(24);
		stackDataWriter.WriteUInt32BE(index);
		stackDataWriter.WriteUInt32BE(ipclient);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_PickUpRequest(this NetClient socket, uint serial, ushort count)
	{
		int packetLength = PacketsTable.GetPacketLength(7);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(7);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(serial);
		stackDataWriter.WriteUInt16BE(count);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_DropRequest_Old(this NetClient socket, uint serial, ushort x, ushort y, sbyte z, uint container)
	{
		int packetLength = PacketsTable.GetPacketLength(8);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(8);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(serial);
		stackDataWriter.WriteUInt16BE(x);
		stackDataWriter.WriteUInt16BE(y);
		stackDataWriter.WriteInt8(z);
		stackDataWriter.WriteUInt32BE(container);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_DropRequest(this NetClient socket, uint serial, ushort x, ushort y, sbyte z, byte slot, uint container)
	{
		int packetLength = PacketsTable.GetPacketLength(8);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(8);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(serial);
		stackDataWriter.WriteUInt16BE(x);
		stackDataWriter.WriteUInt16BE(y);
		stackDataWriter.WriteInt8(z);
		stackDataWriter.WriteUInt8(slot);
		stackDataWriter.WriteUInt32BE(container);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_EquipRequest(this NetClient socket, uint serial, Layer layer, uint container)
	{
		int packetLength = PacketsTable.GetPacketLength(19);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(19);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(serial);
		stackDataWriter.WriteUInt8((byte)layer);
		stackDataWriter.WriteUInt32BE(container);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_ChangeWarMode(this NetClient socket, bool state)
	{
		int packetLength = PacketsTable.GetPacketLength(114);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(114);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteBool(state);
		stackDataWriter.WriteUInt8(50);
		stackDataWriter.WriteUInt8(0);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_HelpRequest(this NetClient socket)
	{
		int packetLength = PacketsTable.GetPacketLength(155);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(155);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteZero(257);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_StatusRequest(this NetClient socket, uint serial)
	{
		int packetLength = PacketsTable.GetPacketLength(52);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(52);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(3991793133u);
		stackDataWriter.WriteUInt8(4);
		stackDataWriter.WriteUInt32BE(serial);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_SkillsRequest(this NetClient socket, uint serial)
	{
		int packetLength = PacketsTable.GetPacketLength(52);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(52);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(3991793133u);
		stackDataWriter.WriteUInt8(5);
		stackDataWriter.WriteUInt32BE(serial);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_SkillsStatusRequest(this NetClient socket, ushort skillIndex, byte lockState)
	{
		int packetLength = PacketsTable.GetPacketLength(58);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(58);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt16BE(skillIndex);
		stackDataWriter.WriteUInt8(lockState);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_ClickRequest(this NetClient socket, uint serial)
	{
		int packetLength = PacketsTable.GetPacketLength(9);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(9);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(serial);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_AttackRequest(this NetClient socket, uint serial)
	{
		int packetLength = PacketsTable.GetPacketLength(5);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(5);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(serial);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_ClientVersion(this NetClient socket, string version)
	{
		int packetLength = PacketsTable.GetPacketLength(189);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(189);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteASCII(version);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_CustomClientVersion(this NetClient socket)
	{
		Version version = Assembly.GetEntryAssembly().GetName().Version;
		StackDataWriter stackDataWriter = new StackDataWriter(21);
		stackDataWriter.WriteUInt8(12);
		stackDataWriter.WriteUInt16BE(21);
		stackDataWriter.WriteUInt16BE(4);
		stackDataWriter.WriteUInt32BE((uint)version.Major);
		stackDataWriter.WriteUInt32BE((uint)version.Minor);
		stackDataWriter.WriteUInt32BE((uint)version.Build);
		stackDataWriter.WriteUInt32BE((uint)version.Revision);
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_CustomPaperdoll(this NetClient socket, int paperdollGump)
	{
		StackDataWriter stackDataWriter = new StackDataWriter(21);
		stackDataWriter.WriteUInt8(12);
		stackDataWriter.WriteUInt16BE(9);
		stackDataWriter.WriteUInt16BE(12);
		stackDataWriter.WriteUInt32BE((uint)paperdollGump);
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_TextActivity(this NetClient socket)
	{
		StackDataWriter stackDataWriter = new StackDataWriter(5);
		stackDataWriter.WriteUInt8(12);
		stackDataWriter.WriteUInt16BE(5);
		stackDataWriter.WriteUInt16BE(11);
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_ASCIISpeechRequest(this NetClient socket, string text, MessageType type, byte font, ushort hue)
	{
		int packetLength = PacketsTable.GetPacketLength(3);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(3);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		List<SpeechEntry> keywords = SpeechesLoader.Instance.GetKeywords(text);
		if (keywords != null && keywords.Count != 0)
		{
			type |= MessageType.Encoded;
		}
		stackDataWriter.WriteUInt8((byte)type);
		stackDataWriter.WriteUInt16BE(hue);
		stackDataWriter.WriteUInt16BE(font);
		stackDataWriter.WriteASCII(text);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_UnicodeSpeechRequest(this NetClient socket, string text, MessageType type, byte font, ushort hue, string lang)
	{
		int packetLength = PacketsTable.GetPacketLength(173);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(173);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		List<SpeechEntry> keywords = SpeechesLoader.Instance.GetKeywords(text);
		int num;
		if (keywords != null)
		{
			num = ((keywords.Count != 0) ? 1 : 0);
			if (num != 0)
			{
				type |= MessageType.Encoded;
			}
		}
		else
		{
			num = 0;
		}
		stackDataWriter.WriteUInt8((byte)type);
		stackDataWriter.WriteUInt16BE(hue);
		stackDataWriter.WriteUInt16BE(font);
		stackDataWriter.WriteUTF8(lang, 4);
		if (num != 0)
		{
			List<byte> list = new List<byte>();
			byte[] bytes = Encoding.UTF8.GetBytes(text);
			int count = keywords.Count;
			list.Add((byte)(count >> 4));
			int num2 = count & 0xF;
			bool flag = false;
			int num3 = 0;
			while (num3 < count)
			{
				int keywordID = keywords[num3].KeywordID;
				if (flag)
				{
					list.Add((byte)(keywordID >> 4));
					num2 = keywordID & 0xF;
				}
				else
				{
					list.Add((byte)((num2 << 4) | ((keywordID >> 8) & 0xF)));
					list.Add((byte)keywordID);
				}
				num3++;
				flag = !flag;
			}
			if (!flag)
			{
				list.Add((byte)(num2 << 4));
			}
			for (int i = 0; i < list.Count; i++)
			{
				stackDataWriter.WriteUInt8(list[i]);
			}
			stackDataWriter.Write(bytes);
			stackDataWriter.WriteUInt8(0);
		}
		else
		{
			stackDataWriter.WriteUnicodeBE(text);
		}
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_CastSpell(this NetClient socket, int idx)
	{
		byte b = 191;
		if (Client.Version < ClientVersion.CV_60142)
		{
			b = 18;
		}
		int packetLength = PacketsTable.GetPacketLength(b);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(b);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		if (Client.Version >= ClientVersion.CV_60142)
		{
			stackDataWriter.WriteUInt16BE(28);
			stackDataWriter.WriteUInt16BE(2);
			stackDataWriter.WriteUInt16BE((ushort)idx);
		}
		else
		{
			stackDataWriter.WriteUInt8(86);
			stackDataWriter.WriteASCII(idx.ToString());
		}
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_CastSpellFromBook(this NetClient socket, int idx, uint serial)
	{
		int packetLength = PacketsTable.GetPacketLength(18);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(18);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt8(39);
		stackDataWriter.WriteASCII($"{idx} {serial}");
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_UseSkill(this NetClient socket, int idx)
	{
		int packetLength = PacketsTable.GetPacketLength(18);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(18);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt8(36);
		stackDataWriter.WriteASCII($"{idx} 0");
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_OpenDoor(this NetClient socket)
	{
		int packetLength = PacketsTable.GetPacketLength(18);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(18);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt8(88);
		stackDataWriter.WriteUInt8(0);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_OpenSpellBook(this NetClient socket, byte type)
	{
		int packetLength = PacketsTable.GetPacketLength(18);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(18);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt8(67);
		stackDataWriter.WriteUInt8(type);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_EmoteAction(this NetClient socket, string action)
	{
		int packetLength = PacketsTable.GetPacketLength(18);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(18);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt8(199);
		stackDataWriter.WriteASCII(action);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_GumpResponse(this NetClient socket, uint local, uint server, int button, uint[] switches, Tuple<ushort, string>[] entries)
	{
		int packetLength = PacketsTable.GetPacketLength(177);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(177);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(local);
		stackDataWriter.WriteUInt32BE(server);
		stackDataWriter.WriteUInt32BE((uint)button);
		stackDataWriter.WriteUInt32BE((uint)switches.Length);
		for (int i = 0; i < switches.Length; i++)
		{
			stackDataWriter.WriteUInt32BE(switches[i]);
		}
		stackDataWriter.WriteUInt32BE((uint)entries.Length);
		for (int j = 0; j < entries.Length; j++)
		{
			int num = Math.Min(4000, entries[j].Item2.Length);
			stackDataWriter.WriteUInt16BE(entries[j].Item1);
			stackDataWriter.WriteUInt16BE((ushort)num);
			stackDataWriter.WriteUnicodeBE(entries[j].Item2, num);
		}
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_VirtueGumpResponse(this NetClient socket, uint serial, uint code)
	{
		int packetLength = PacketsTable.GetPacketLength(177);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(177);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(serial);
		stackDataWriter.WriteUInt32BE(461u);
		stackDataWriter.WriteUInt32BE(code);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_MenuResponse(this NetClient socket, uint serial, ushort graphic, int code, ushort itemGraphic, ushort itemHue)
	{
		int packetLength = PacketsTable.GetPacketLength(125);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(125);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(serial);
		stackDataWriter.WriteUInt16BE(graphic);
		if (code != 0)
		{
			stackDataWriter.WriteUInt16BE((ushort)code);
			stackDataWriter.WriteUInt16BE(itemGraphic);
			stackDataWriter.WriteUInt16BE(itemHue);
		}
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_GrayMenuResponse(this NetClient socket, uint serial, ushort graphic, ushort code)
	{
		int packetLength = PacketsTable.GetPacketLength(125);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(125);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(serial);
		stackDataWriter.WriteUInt16BE(graphic);
		stackDataWriter.WriteUInt16BE(code);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_TradeResponse(this NetClient socket, uint serial, int code, bool state)
	{
		int packetLength = PacketsTable.GetPacketLength(111);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(111);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		switch (code)
		{
		case 1:
			stackDataWriter.WriteUInt8(1);
			stackDataWriter.WriteUInt32BE(serial);
			break;
		case 2:
			stackDataWriter.WriteUInt8(2);
			stackDataWriter.WriteUInt32BE(serial);
			stackDataWriter.WriteUInt32BE(state ? 1u : 0u);
			break;
		default:
			stackDataWriter.Dispose();
			return;
		}
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_TradeUpdateGold(this NetClient socket, uint serial, uint gold, uint platinum)
	{
		int packetLength = PacketsTable.GetPacketLength(111);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(111);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt8(3);
		stackDataWriter.WriteUInt32BE(serial);
		stackDataWriter.WriteUInt32BE(gold);
		stackDataWriter.WriteUInt32BE(platinum);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_LogoutNotification(this NetClient socket)
	{
		int packetLength = PacketsTable.GetPacketLength(209);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(209);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt8(0);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_TextEntryDialogResponse(this NetClient socket, uint serial, byte parentID, byte button, string text, bool code)
	{
		int packetLength = PacketsTable.GetPacketLength(172);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(172);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(serial);
		stackDataWriter.WriteUInt8(parentID);
		stackDataWriter.WriteUInt8(button);
		stackDataWriter.WriteBool(code);
		stackDataWriter.WriteUInt16BE((ushort)(text.Length + 1));
		stackDataWriter.WriteASCII(text, text.Length + 1);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_RenameRequest(this NetClient socket, uint serial, string name)
	{
		int packetLength = PacketsTable.GetPacketLength(117);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(117);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(serial);
		stackDataWriter.WriteASCII(name, 30);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_NameRequest(this NetClient socket, uint serial)
	{
		int packetLength = PacketsTable.GetPacketLength(152);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(152);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(serial);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_TipRequest(this NetClient socket, ushort id, byte flag)
	{
		int packetLength = PacketsTable.GetPacketLength(167);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(167);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt16BE(id);
		stackDataWriter.WriteUInt8(flag);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_TargetObject(this NetClient socket, uint entity, ushort graphic, ushort x, ushort y, sbyte z, uint cursorID, byte cursorType)
	{
		int packetLength = PacketsTable.GetPacketLength(108);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(108);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt8(0);
		stackDataWriter.WriteUInt32BE(cursorID);
		stackDataWriter.WriteUInt8(cursorType);
		stackDataWriter.WriteUInt32BE(entity);
		stackDataWriter.WriteUInt16BE(x);
		stackDataWriter.WriteUInt16BE(y);
		stackDataWriter.WriteUInt16BE((ushort)z);
		stackDataWriter.WriteUInt16BE(graphic);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_TargetXYZ(this NetClient socket, ushort graphic, ushort x, ushort y, sbyte z, uint cursorID, byte cursorType)
	{
		int packetLength = PacketsTable.GetPacketLength(108);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(108);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt8(1);
		stackDataWriter.WriteUInt32BE(cursorID);
		stackDataWriter.WriteUInt8(cursorType);
		stackDataWriter.WriteUInt32BE(0u);
		stackDataWriter.WriteUInt16BE(x);
		stackDataWriter.WriteUInt16BE(y);
		stackDataWriter.WriteUInt16BE((ushort)z);
		stackDataWriter.WriteUInt16BE(graphic);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_TargetCancel(this NetClient socket, CursorTarget type, uint cursorID, byte cursorType)
	{
		int packetLength = PacketsTable.GetPacketLength(108);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(108);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt8((byte)type);
		stackDataWriter.WriteUInt32BE(cursorID);
		stackDataWriter.WriteUInt8(cursorType);
		stackDataWriter.WriteUInt32BE(0u);
		stackDataWriter.WriteUInt32BE(uint.MaxValue);
		stackDataWriter.WriteUInt32BE(0u);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_ASCIIPromptResponse(this NetClient socket, string text, bool cancel)
	{
		int packetLength = PacketsTable.GetPacketLength(154);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(154);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt64BE(MessageManager.PromptData.Data);
		stackDataWriter.WriteUInt32BE((!cancel) ? 1u : 0u);
		stackDataWriter.WriteASCII(text);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_UnicodePromptResponse(this NetClient socket, string text, string lang, bool cancel)
	{
		int packetLength = PacketsTable.GetPacketLength(194);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(194);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt64BE(MessageManager.PromptData.Data);
		stackDataWriter.WriteUInt32BE((!cancel) ? 1u : 0u);
		stackDataWriter.WriteASCII(lang);
		stackDataWriter.WriteUnicodeLE(text, text.Length);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_DyeDataResponse(this NetClient socket, uint serial, ushort graphic, ushort hue)
	{
		int packetLength = PacketsTable.GetPacketLength(149);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(149);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(serial);
		stackDataWriter.WriteUInt16BE(graphic);
		stackDataWriter.WriteUInt16BE(hue);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_ProfileRequest(this NetClient socket, uint serial)
	{
		int packetLength = PacketsTable.GetPacketLength(184);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(184);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt8(0);
		stackDataWriter.WriteUInt32BE(serial);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_ProfileUpdate(this NetClient socket, uint serial, string text)
	{
		int packetLength = PacketsTable.GetPacketLength(184);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(184);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt8(1);
		stackDataWriter.WriteUInt32BE(serial);
		stackDataWriter.WriteUInt16BE(1);
		stackDataWriter.WriteUInt16BE((ushort)text.Length);
		stackDataWriter.WriteUnicodeBE(text, text.Length);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_ClickQuestArrow(this NetClient socket, bool righClick)
	{
		int packetLength = PacketsTable.GetPacketLength(191);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(191);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt16BE(7);
		stackDataWriter.WriteBool(righClick);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_CloseStatusBarGump(this NetClient socket, uint serial)
	{
		int packetLength = PacketsTable.GetPacketLength(191);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(191);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt16BE(12);
		stackDataWriter.WriteUInt32BE(serial);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_PartyInviteRequest(this NetClient socket)
	{
		int packetLength = PacketsTable.GetPacketLength(191);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(191);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt16BE(6);
		stackDataWriter.WriteUInt8(1);
		stackDataWriter.WriteUInt32BE(0u);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_PartyRemoveRequest(this NetClient socket, uint serial)
	{
		int packetLength = PacketsTable.GetPacketLength(191);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(191);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt16BE(6);
		stackDataWriter.WriteUInt8(2);
		stackDataWriter.WriteUInt32BE(serial);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_PartyChangeLootTypeRequest(this NetClient socket, bool type)
	{
		int packetLength = PacketsTable.GetPacketLength(191);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(191);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt16BE(6);
		stackDataWriter.WriteUInt8(6);
		stackDataWriter.WriteBool(type);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_PartyAccept(this NetClient socket, uint serial)
	{
		int packetLength = PacketsTable.GetPacketLength(191);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(191);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt16BE(6);
		stackDataWriter.WriteUInt8(8);
		stackDataWriter.WriteUInt32BE(serial);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_PartyDecline(this NetClient socket, uint serial)
	{
		int packetLength = PacketsTable.GetPacketLength(191);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(191);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt16BE(6);
		stackDataWriter.WriteUInt8(9);
		stackDataWriter.WriteUInt32BE(serial);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_PartyMessage(this NetClient socket, string text, uint serial)
	{
		int packetLength = PacketsTable.GetPacketLength(191);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(191);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt16BE(6);
		if (SerialHelper.IsValid(serial))
		{
			stackDataWriter.WriteUInt8(3);
			stackDataWriter.WriteUInt32BE(serial);
		}
		else
		{
			stackDataWriter.WriteUInt8(4);
		}
		stackDataWriter.WriteUnicodeBE(text);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_GameWindowSize(this NetClient socket, uint w, uint h)
	{
		int packetLength = PacketsTable.GetPacketLength(191);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(191);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt16BE(5);
		stackDataWriter.WriteUInt32BE(w);
		stackDataWriter.WriteUInt32BE(h);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_BulletinBoardRequestMessage(this NetClient socket, uint serial, uint msgSerial)
	{
		int packetLength = PacketsTable.GetPacketLength(113);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(113);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt8(3);
		stackDataWriter.WriteUInt32BE(serial);
		stackDataWriter.WriteUInt32BE(msgSerial);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_BulletinBoardRequestMessageSummary(this NetClient socket, uint serial, uint msgSerial)
	{
		int packetLength = PacketsTable.GetPacketLength(113);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(113);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt8(4);
		stackDataWriter.WriteUInt32BE(serial);
		stackDataWriter.WriteUInt32BE(msgSerial);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_BulletinBoardPostMessage(this NetClient socket, uint serial, uint msgSerial, string subject, string text)
	{
		int packetLength = PacketsTable.GetPacketLength(113);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(113);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt8(5);
		stackDataWriter.WriteUInt32BE(serial);
		stackDataWriter.WriteUInt32BE(msgSerial);
		stackDataWriter.WriteUInt8((byte)(subject.Length + 1));
		byte[] bytes = Encoding.UTF8.GetBytes(subject);
		stackDataWriter.Write(bytes);
		stackDataWriter.WriteUInt8(0);
		int position = stackDataWriter.Position;
		int num = 0;
		int i = 0;
		int num2 = 0;
		for (; i < text.Length; i++)
		{
			if (text[i] != '\n')
			{
				continue;
			}
			num++;
			int num3 = i - num2;
			if (num3 > 0)
			{
				byte[] array = ArrayPool<byte>.Shared.Rent(num3 * 2);
				try
				{
					int bytes2 = Encoding.UTF8.GetBytes(text, num2, num3, array, 0);
					stackDataWriter.WriteUInt8((byte)(bytes2 + 1));
					stackDataWriter.Write(array.AsSpan(0, bytes2));
					stackDataWriter.WriteUInt8(0);
				}
				finally
				{
					ArrayPool<byte>.Shared.Return(array);
				}
			}
			else
			{
				stackDataWriter.WriteUInt8(1);
				stackDataWriter.WriteUInt8(0);
			}
			num2 = i;
		}
		if (num == 0)
		{
			stackDataWriter.WriteUInt8((byte)(text.Length + 1));
			stackDataWriter.WriteASCII(text);
			stackDataWriter.WriteUInt8(0);
		}
		stackDataWriter.Seek(position, SeekOrigin.Begin);
		stackDataWriter.WriteUInt8((byte)num);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_BulletinBoardRemoveMessage(this NetClient socket, uint serial, uint msgSerial)
	{
		int packetLength = PacketsTable.GetPacketLength(113);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(113);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt8(6);
		stackDataWriter.WriteUInt32BE(serial);
		stackDataWriter.WriteUInt32BE(msgSerial);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_RazorACK(this NetClient socket)
	{
		int packetLength = PacketsTable.GetPacketLength(240);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(240);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt8(byte.MaxValue);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_QueryGuildPosition(this NetClient socket)
	{
		int packetLength = PacketsTable.GetPacketLength(240);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(240);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt8(1);
		stackDataWriter.WriteBool(b: true);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_QueryPartyPosition(this NetClient socket)
	{
		int packetLength = PacketsTable.GetPacketLength(240);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(240);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt8(0);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_Language(this NetClient socket, string lang)
	{
		int packetLength = PacketsTable.GetPacketLength(191);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(191);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt16BE(11);
		stackDataWriter.WriteASCII(lang, 3);
		stackDataWriter.WriteUInt8(0);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_ClientType(this NetClient socket)
	{
		int packetLength = PacketsTable.GetPacketLength(191);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(191);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt16BE(15);
		stackDataWriter.WriteUInt8(10);
		uint num = 0u;
		for (int i = 0; (long)i < (long)Client.Protocol; i++)
		{
			num |= (uint)(1 << i);
		}
		stackDataWriter.WriteUInt32BE(num);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_RequestPopupMenu(this NetClient socket, uint serial)
	{
		int packetLength = PacketsTable.GetPacketLength(191);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(191);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt16BE(19);
		stackDataWriter.WriteUInt32BE(serial);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_PopupMenuSelection(this NetClient socket, uint serial, ushort menuid)
	{
		int packetLength = PacketsTable.GetPacketLength(191);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(191);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt16BE(21);
		stackDataWriter.WriteUInt32BE(serial);
		stackDataWriter.WriteUInt16BE(menuid);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_ChatJoinCommand(this NetClient socket, string name, string password = null)
	{
		int packetLength = PacketsTable.GetPacketLength(179);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(179);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteASCII(Settings.GlobalSettings.Language, 4);
		stackDataWriter.WriteUInt16BE(98);
		stackDataWriter.WriteUInt16BE(34);
		stackDataWriter.WriteUnicodeBE(name);
		stackDataWriter.WriteUInt16BE(34);
		stackDataWriter.WriteUInt16BE(32);
		if (!string.IsNullOrEmpty(password))
		{
			stackDataWriter.WriteUnicodeBE(password);
		}
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_ChatCreateChannelCommand(this NetClient socket, string name, string password = null)
	{
		int packetLength = PacketsTable.GetPacketLength(179);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(179);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteASCII(Settings.GlobalSettings.Language, 4);
		stackDataWriter.WriteUInt16BE(99);
		stackDataWriter.WriteUnicodeBE(name);
		if (!string.IsNullOrEmpty(password))
		{
			stackDataWriter.WriteUInt16BE(123);
			stackDataWriter.WriteUnicodeBE(password);
			stackDataWriter.WriteUInt16BE(125);
		}
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_ChatLeaveChannelCommand(this NetClient socket)
	{
		int packetLength = PacketsTable.GetPacketLength(179);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(179);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteASCII(Settings.GlobalSettings.Language, 4);
		stackDataWriter.WriteUInt16BE(67);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_ChatMessageCommand(this NetClient socket, string msg)
	{
		int packetLength = PacketsTable.GetPacketLength(179);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(179);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteASCII(Settings.GlobalSettings.Language, 4);
		stackDataWriter.WriteUInt16BE(97);
		stackDataWriter.WriteUnicodeBE(msg);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_OpenChat(this NetClient socket, string name)
	{
		int packetLength = PacketsTable.GetPacketLength(181);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(181);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt8(0);
		int num = Math.Min(name.Length, 30);
		if (num > 0)
		{
			stackDataWriter.WriteUnicodeBE(name, num);
		}
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_MapMessage(this NetClient socket, uint serial, byte action, byte pin, ushort x, ushort y)
	{
		int packetLength = PacketsTable.GetPacketLength(86);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(86);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(serial);
		stackDataWriter.WriteUInt8(action);
		stackDataWriter.WriteUInt8(pin);
		stackDataWriter.WriteUInt16BE(x);
		stackDataWriter.WriteUInt16BE(y);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_GuildMenuRequest(this NetClient socket)
	{
		int packetLength = PacketsTable.GetPacketLength(215);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(215);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(World.Player.Serial);
		stackDataWriter.WriteUInt16BE(40);
		stackDataWriter.WriteUInt8(10);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_QuestMenuRequest(this NetClient socket)
	{
		int packetLength = PacketsTable.GetPacketLength(215);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(215);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(World.Player.Serial);
		stackDataWriter.WriteUInt16BE(50);
		stackDataWriter.WriteUInt8(0);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_EquipLastWeapon(this NetClient socket)
	{
		int packetLength = PacketsTable.GetPacketLength(215);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(215);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(World.Player.Serial);
		stackDataWriter.WriteUInt16BE(30);
		stackDataWriter.WriteUInt8(10);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_InvokeVirtueRequest(this NetClient socket, byte id)
	{
		int packetLength = PacketsTable.GetPacketLength(18);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(18);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt8(244);
		stackDataWriter.WriteASCII(id.ToString());
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_MegaClilocRequest_Old(this NetClient socket, uint serial)
	{
		int packetLength = PacketsTable.GetPacketLength(191);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(191);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt16BE(16);
		stackDataWriter.WriteUInt32BE(serial);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_MegaClilocRequest(this NetClient socket, ref List<uint> serials)
	{
		int packetLength = PacketsTable.GetPacketLength(214);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(214);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		int num = Math.Min(15, serials.Count);
		for (int i = 0; i < num; i++)
		{
			stackDataWriter.WriteUInt32BE(serials[i]);
		}
		serials.RemoveRange(0, num);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_StatLockStateRequest(this NetClient socket, byte stat, Lock state)
	{
		int packetLength = PacketsTable.GetPacketLength(191);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(191);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt16BE(26);
		stackDataWriter.WriteUInt8(stat);
		stackDataWriter.WriteUInt8((byte)state);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_SkillStatusChangeRequest(this NetClient socket, ushort skillindex, byte lockstate)
	{
		int packetLength = PacketsTable.GetPacketLength(58);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(58);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt16BE(skillindex);
		stackDataWriter.WriteUInt8(lockstate);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_BookHeaderChanged_Old(this NetClient socket, uint serial, string title, string author)
	{
		int packetLength = PacketsTable.GetPacketLength(147);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(147);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(serial);
		stackDataWriter.WriteUInt8(0);
		stackDataWriter.WriteUInt8(1);
		stackDataWriter.WriteUInt16BE(0);
		stackDataWriter.WriteUTF8(title, 60);
		stackDataWriter.WriteUTF8(author, 30);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_BookHeaderChanged(this NetClient socket, uint serial, string title, string author)
	{
		int packetLength = PacketsTable.GetPacketLength(212);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(212);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(serial);
		stackDataWriter.WriteUInt8(0);
		stackDataWriter.WriteUInt8(0);
		stackDataWriter.WriteUInt16BE(0);
		int byteCount = Encoding.UTF8.GetByteCount(title);
		stackDataWriter.WriteUInt16BE((ushort)(byteCount + 1));
		stackDataWriter.WriteUTF8(title, byteCount + 1);
		int byteCount2 = Encoding.UTF8.GetByteCount(author);
		stackDataWriter.WriteUInt16BE((ushort)(byteCount2 + 1));
		stackDataWriter.WriteUTF8(author, byteCount2 + 1);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_BookPageData(this NetClient socket, uint serial, string[] text, int page)
	{
		int packetLength = PacketsTable.GetPacketLength(102);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(102);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(serial);
		stackDataWriter.WriteUInt16BE(1);
		stackDataWriter.WriteUInt16BE((ushort)page);
		stackDataWriter.WriteUInt16BE((ushort)text.Length);
		for (int i = 0; i < text.Length; i++)
		{
			if (!string.IsNullOrEmpty(text[i]))
			{
				string text2 = text[i].Replace("\n", "");
				if (text2.Length > 0)
				{
					byte[] array = ArrayPool<byte>.Shared.Rent(text2.Length * 2);
					try
					{
						int bytes = Encoding.UTF8.GetBytes(text2, 0, text2.Length, array, 0);
						stackDataWriter.Write(array.AsSpan(0, bytes));
					}
					finally
					{
						ArrayPool<byte>.Shared.Return(array);
					}
				}
			}
			stackDataWriter.WriteUInt8(0);
		}
		stackDataWriter.WriteUInt8(0);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_BookPageDataRequest(this NetClient socket, uint serial, ushort page)
	{
		int packetLength = PacketsTable.GetPacketLength(102);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(102);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(serial);
		stackDataWriter.WriteUInt16BE(1);
		stackDataWriter.WriteUInt16BE(page);
		stackDataWriter.WriteUInt16BE(ushort.MaxValue);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_BuyRequest(this NetClient socket, uint serial, Tuple<uint, ushort>[] items)
	{
		int packetLength = PacketsTable.GetPacketLength(59);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(59);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(serial);
		if (items.Length != 0)
		{
			stackDataWriter.WriteUInt8(2);
			for (int i = 0; i < items.Length; i++)
			{
				stackDataWriter.WriteUInt8(26);
				stackDataWriter.WriteUInt32BE(items[i].Item1);
				stackDataWriter.WriteUInt16BE(items[i].Item2);
			}
		}
		else
		{
			stackDataWriter.WriteUInt8(0);
		}
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_SellRequest(this NetClient socket, uint serial, Tuple<uint, ushort>[] items)
	{
		int packetLength = PacketsTable.GetPacketLength(159);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(159);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(serial);
		stackDataWriter.WriteUInt16BE((ushort)items.Length);
		for (int i = 0; i < items.Length; i++)
		{
			stackDataWriter.WriteUInt32BE(items[i].Item1);
			stackDataWriter.WriteUInt16BE(items[i].Item2);
		}
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_UseCombatAbility(this NetClient socket, byte idx)
	{
		int packetLength = PacketsTable.GetPacketLength(215);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(215);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(World.Player.Serial);
		stackDataWriter.WriteUInt16BE(25);
		stackDataWriter.WriteUInt32BE(0u);
		stackDataWriter.WriteUInt8(idx);
		stackDataWriter.WriteUInt8(10);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_TargetSelectedObject(this NetClient socket, uint serial, uint targetSerial)
	{
		int packetLength = PacketsTable.GetPacketLength(191);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(191);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt16BE(44);
		stackDataWriter.WriteUInt32BE(serial);
		stackDataWriter.WriteUInt32BE(targetSerial);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_ToggleGargoyleFlying(this NetClient socket)
	{
		int packetLength = PacketsTable.GetPacketLength(191);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(191);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt16BE(50);
		stackDataWriter.WriteUInt16BE(1);
		stackDataWriter.WriteUInt32BE(0u);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_CustomHouseDataRequest(this NetClient socket, uint serial)
	{
		int packetLength = PacketsTable.GetPacketLength(191);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(191);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt16BE(30);
		stackDataWriter.WriteUInt32BE(serial);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_StunRequest(this NetClient socket)
	{
		int packetLength = PacketsTable.GetPacketLength(191);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(191);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt16BE(9);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_DisarmRequest(this NetClient socket)
	{
		int packetLength = PacketsTable.GetPacketLength(191);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(191);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt16BE(10);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_ChangeRaceRequest(this NetClient socket, ushort skinHue, ushort hairStyle, ushort hairHue, ushort beardStyle, ushort beardHue)
	{
		int packetLength = PacketsTable.GetPacketLength(191);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(191);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt16BE(42);
		stackDataWriter.WriteUInt16BE(skinHue);
		stackDataWriter.WriteUInt16BE(hairStyle);
		stackDataWriter.WriteUInt16BE(hairHue);
		stackDataWriter.WriteUInt16BE(beardStyle);
		stackDataWriter.WriteUInt16BE(beardHue);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_MultiBoatMoveRequest(this NetClient socket, uint serial, Direction dir, byte speed)
	{
		int packetLength = PacketsTable.GetPacketLength(191);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(191);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt16BE(51);
		stackDataWriter.WriteUInt32BE(serial);
		stackDataWriter.WriteUInt8((byte)dir);
		stackDataWriter.WriteUInt8((byte)dir);
		stackDataWriter.WriteUInt8(speed);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_Resync(this NetClient socket)
	{
		int packetLength = PacketsTable.GetPacketLength(34);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(34);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_WalkRequest(this NetClient socket, Direction direction, byte seq, bool run, uint fastWalk)
	{
		int packetLength = PacketsTable.GetPacketLength(2);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(2);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		if (run)
		{
			direction |= Direction.Running;
		}
		stackDataWriter.WriteUInt8((byte)direction);
		stackDataWriter.WriteUInt8(seq);
		stackDataWriter.WriteUInt32BE(fastWalk);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_CustomHouseBackup(this NetClient socket)
	{
		int packetLength = PacketsTable.GetPacketLength(215);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(215);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(World.Player.Serial);
		stackDataWriter.WriteUInt16BE(2);
		stackDataWriter.WriteUInt8(10);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_CustomHouseRestore(this NetClient socket)
	{
		int packetLength = PacketsTable.GetPacketLength(215);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(215);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(World.Player.Serial);
		stackDataWriter.WriteUInt16BE(3);
		stackDataWriter.WriteUInt8(10);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_CustomHouseCommit(this NetClient socket)
	{
		int packetLength = PacketsTable.GetPacketLength(215);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(215);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(World.Player.Serial);
		stackDataWriter.WriteUInt16BE(4);
		stackDataWriter.WriteUInt8(10);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_CustomHouseBuildingExit(this NetClient socket)
	{
		int packetLength = PacketsTable.GetPacketLength(215);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(215);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(World.Player.Serial);
		stackDataWriter.WriteUInt16BE(12);
		stackDataWriter.WriteUInt8(10);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_CustomHouseGoToFloor(this NetClient socket, byte floor)
	{
		int packetLength = PacketsTable.GetPacketLength(215);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(215);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(World.Player.Serial);
		stackDataWriter.WriteUInt16BE(18);
		stackDataWriter.WriteUInt32BE(0u);
		stackDataWriter.WriteUInt8(floor);
		stackDataWriter.WriteUInt8(10);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_CustomHouseSync(this NetClient socket)
	{
		int packetLength = PacketsTable.GetPacketLength(215);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(215);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(World.Player.Serial);
		stackDataWriter.WriteUInt16BE(14);
		stackDataWriter.WriteUInt8(10);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_CustomHouseClear(this NetClient socket)
	{
		int packetLength = PacketsTable.GetPacketLength(215);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(215);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(World.Player.Serial);
		stackDataWriter.WriteUInt16BE(16);
		stackDataWriter.WriteUInt8(10);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_CustomHouseRevert(this NetClient socket)
	{
		int packetLength = PacketsTable.GetPacketLength(215);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(215);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(World.Player.Serial);
		stackDataWriter.WriteUInt16BE(26);
		stackDataWriter.WriteUInt8(10);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_CustomHouseResponse(this NetClient socket)
	{
		int packetLength = PacketsTable.GetPacketLength(215);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(215);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(World.Player.Serial);
		stackDataWriter.WriteUInt16BE(10);
		stackDataWriter.WriteUInt8(10);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_CustomHouseAddItem(this NetClient socket, ushort graphic, int x, int y)
	{
		int packetLength = PacketsTable.GetPacketLength(215);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(215);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(World.Player.Serial);
		stackDataWriter.WriteUInt16BE(6);
		stackDataWriter.WriteUInt8(0);
		stackDataWriter.WriteUInt32BE(graphic);
		stackDataWriter.WriteUInt8(0);
		stackDataWriter.WriteUInt32BE((uint)x);
		stackDataWriter.WriteUInt8(0);
		stackDataWriter.WriteUInt32BE((uint)y);
		stackDataWriter.WriteUInt8(10);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_CustomHouseDeleteItem(this NetClient socket, ushort graphic, int x, int y, int z)
	{
		int packetLength = PacketsTable.GetPacketLength(215);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(215);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(World.Player.Serial);
		stackDataWriter.WriteUInt16BE(5);
		stackDataWriter.WriteUInt8(0);
		stackDataWriter.WriteUInt32BE(graphic);
		stackDataWriter.WriteUInt8(0);
		stackDataWriter.WriteUInt32BE((uint)x);
		stackDataWriter.WriteUInt8(0);
		stackDataWriter.WriteUInt32BE((uint)y);
		stackDataWriter.WriteUInt8(0);
		stackDataWriter.WriteUInt32BE((uint)z);
		stackDataWriter.WriteUInt8(10);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_CustomHouseAddRoof(this NetClient socket, ushort graphic, int x, int y, int z)
	{
		int packetLength = PacketsTable.GetPacketLength(215);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(215);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(World.Player.Serial);
		stackDataWriter.WriteUInt16BE(19);
		stackDataWriter.WriteUInt8(0);
		stackDataWriter.WriteUInt32BE(graphic);
		stackDataWriter.WriteUInt8(0);
		stackDataWriter.WriteUInt32BE((uint)x);
		stackDataWriter.WriteUInt8(0);
		stackDataWriter.WriteUInt32BE((uint)y);
		stackDataWriter.WriteUInt8(0);
		stackDataWriter.WriteUInt32BE((uint)z);
		stackDataWriter.WriteUInt8(10);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_CustomHouseDeleteRoof(this NetClient socket, ushort graphic, int x, int y, int z)
	{
		int packetLength = PacketsTable.GetPacketLength(215);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(215);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(World.Player.Serial);
		stackDataWriter.WriteUInt16BE(20);
		stackDataWriter.WriteUInt8(0);
		stackDataWriter.WriteUInt32BE(graphic);
		stackDataWriter.WriteUInt8(0);
		stackDataWriter.WriteUInt32BE((uint)x);
		stackDataWriter.WriteUInt8(0);
		stackDataWriter.WriteUInt32BE((uint)y);
		stackDataWriter.WriteUInt8(0);
		stackDataWriter.WriteUInt32BE((uint)z);
		stackDataWriter.WriteUInt8(10);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_CustomHouseAddStair(this NetClient socket, ushort graphic, int x, int y)
	{
		int packetLength = PacketsTable.GetPacketLength(215);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(215);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(World.Player.Serial);
		stackDataWriter.WriteUInt16BE(13);
		stackDataWriter.WriteUInt8(0);
		stackDataWriter.WriteUInt32BE(graphic);
		stackDataWriter.WriteUInt8(0);
		stackDataWriter.WriteUInt32BE((uint)x);
		stackDataWriter.WriteUInt8(0);
		stackDataWriter.WriteUInt32BE((uint)y);
		stackDataWriter.WriteUInt8(10);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_ClientViewRange(this NetClient socket, byte range)
	{
		int packetLength = PacketsTable.GetPacketLength(200);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(200);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		if (range < 5)
		{
			range = 5;
		}
		else if (range > 31)
		{
			range = 31;
		}
		stackDataWriter.WriteUInt8(range);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_OpenUOStore(this NetClient socket)
	{
		int packetLength = PacketsTable.GetPacketLength(250);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(250);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_ShowPublicHouseContent(this NetClient socket, bool show)
	{
		int packetLength = PacketsTable.GetPacketLength(251);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(251);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteBool(show);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_DeathScreen(this NetClient socket)
	{
		int packetLength = PacketsTable.GetPacketLength(44);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(44);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt8(2);
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_UOLive_HashResponse(this NetClient socket, uint block, byte mapIndex, Span<ushort> checksums)
	{
		int packetLength = PacketsTable.GetPacketLength(63);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(63);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt32BE(block);
		stackDataWriter.WriteZero(6);
		stackDataWriter.WriteUInt8(byte.MaxValue);
		stackDataWriter.WriteUInt8(mapIndex);
		for (int i = 0; i < checksums.Length; i++)
		{
			stackDataWriter.WriteUInt16BE(checksums[i]);
		}
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public static void Send_ToPlugins_AllSpells(this NetClient socket)
	{
		int packetLength = PacketsTable.GetPacketLength(191);
		StackDataWriter w2 = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		w2.WriteUInt8(191);
		if (packetLength < 0)
		{
			w2.WriteZero(2);
		}
		w2.WriteUInt16BE(48879);
		w2.WriteUInt8(0);
		writeDef(SpellsMagery.GetAllSpells, ref w2);
		writeDef(SpellsNecromancy.GetAllSpells, ref w2);
		writeDef(SpellsBushido.GetAllSpells, ref w2);
		writeDef(SpellsNinjitsu.GetAllSpells, ref w2);
		writeDef(SpellsChivalry.GetAllSpells, ref w2);
		writeDef(SpellsSpellweaving.GetAllSpells, ref w2);
		writeDef(SpellsMastery.GetAllSpells, ref w2);
		if (packetLength < 0)
		{
			w2.Seek(1, SeekOrigin.Begin);
			w2.WriteUInt16BE((ushort)w2.BytesWritten);
		}
		else
		{
			w2.WriteZero(packetLength - w2.BytesWritten);
		}
		int length = w2.BytesWritten;
		Plugin.ProcessRecvPacket(w2.AllocatedBuffer, ref length);
		w2.Dispose();
		static void writeDef(IReadOnlyDictionary<int, SpellDefinition> dict, ref StackDataWriter w)
		{
			w.WriteUInt16BE((ushort)dict.Count);
			foreach (KeyValuePair<int, SpellDefinition> item in dict)
			{
				w.WriteUInt16BE((ushort)item.Key);
				w.WriteUInt16BE((ushort)item.Value.ManaCost);
				w.WriteUInt16BE((ushort)item.Value.MinSkill);
				w.WriteUInt8((byte)item.Value.TargetType);
				w.WriteUInt16BE((ushort)item.Value.Name.Length);
				w.WriteUnicodeBE(item.Value.Name, item.Value.Name.Length);
				w.WriteUInt16BE((ushort)item.Value.PowerWords.Length);
				w.WriteUnicodeBE(item.Value.PowerWords, item.Value.PowerWords.Length);
				w.WriteUInt16BE((ushort)item.Value.Regs.Length);
				Reagents[] regs = item.Value.Regs;
				foreach (Reagents reagents in regs)
				{
					w.WriteUInt8((byte)reagents);
				}
			}
		}
	}

	public static void Send_ToPlugins_AllSkills(this NetClient socket)
	{
		int packetLength = PacketsTable.GetPacketLength(191);
		StackDataWriter stackDataWriter = new StackDataWriter((packetLength < 0) ? 64 : packetLength);
		stackDataWriter.WriteUInt8(191);
		if (packetLength < 0)
		{
			stackDataWriter.WriteZero(2);
		}
		stackDataWriter.WriteUInt16BE(48879);
		stackDataWriter.WriteUInt8(1);
		stackDataWriter.WriteUInt16BE((ushort)SkillsLoader.Instance.SortedSkills.Count);
		foreach (SkillEntry sortedSkill in SkillsLoader.Instance.SortedSkills)
		{
			stackDataWriter.WriteUInt16BE((ushort)sortedSkill.Index);
			stackDataWriter.WriteBool(sortedSkill.HasAction);
			stackDataWriter.WriteUInt16BE((ushort)sortedSkill.Name.Length);
			stackDataWriter.WriteUnicodeBE(sortedSkill.Name, sortedSkill.Name.Length);
		}
		if (packetLength < 0)
		{
			stackDataWriter.Seek(1, SeekOrigin.Begin);
			stackDataWriter.WriteUInt16BE((ushort)stackDataWriter.BytesWritten);
		}
		else
		{
			stackDataWriter.WriteZero(packetLength - stackDataWriter.BytesWritten);
		}
		int length = stackDataWriter.BytesWritten;
		Plugin.ProcessRecvPacket(stackDataWriter.AllocatedBuffer, ref length);
		stackDataWriter.Dispose();
	}
}

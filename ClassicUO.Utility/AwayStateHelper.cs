using ClassicUO.IO;
using ClassicUO.Network;

namespace ClassicUO.Utility;

public class AwayStateHelper
{
	public const int AFK_GRACE_PERIOD_DEFAULT = 5000;

	public static long _lastActivity = 0L;

	public static long _lastStateChange = 0L;

	public static uint gracePeriod = 5000u;

	public static byte AFK_CODE_SET_AFK = 1;

	public static byte AFK_CODE_SET_BACK = 2;

	public static byte AFK_CODE_REQUEST_ACTIVITY = 100;

	public static void NotifyPresent()
	{
		_lastActivity = Time.Ticks;
		if (_lastActivity - _lastStateChange > gracePeriod)
		{
			_lastStateChange = Time.Ticks;
			_lastActivity = Time.Ticks;
			SendActivityPacket();
		}
	}

	public static void HandlePacket(byte code, uint timespan)
	{
		if (code == AFK_CODE_SET_AFK)
		{
			_lastStateChange = Time.Ticks;
			gracePeriod = timespan * 1000;
		}
		else if (code == AFK_CODE_SET_BACK)
		{
			_lastStateChange = Time.Ticks;
			gracePeriod = 5000u;
		}
		else if (code == AFK_CODE_REQUEST_ACTIVITY)
		{
			long num = Time.Ticks - _lastActivity;
			if (num <= timespan * 1000)
			{
				SendActivityPacket((uint)num / 1000);
			}
		}
	}

	public static void SendActivityPacket(uint timespan = 0u)
	{
		StackDataWriter stackDataWriter = default(StackDataWriter);
		stackDataWriter.WriteUInt8(12);
		stackDataWriter.WriteUInt16BE(9);
		stackDataWriter.WriteUInt16BE(5);
		stackDataWriter.WriteUInt32BE(timespan);
		NetClient.Socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
	}
}

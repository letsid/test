using ClassicUO.Network;

namespace ClassicUO.Game.Managers;

internal class WalkerManager
{
	public ushort CurrentPlayerZ;

	public byte CurrentWalkSequence;

	public long LastStepRequestTime;

	public ushort NewPlayerZ;

	public bool ResendPacketResync;

	public StepInfo[] StepInfos = new StepInfo[5];

	public int StepsCount;

	public int UnacceptedPacketsCount;

	public bool WalkingFailed;

	public byte WalkSequence;

	public bool WantChangeCoordinates;

	public FastWalkStack FastWalkStack { get; } = new FastWalkStack();

	public void DenyWalk(byte sequence, int x, int y, sbyte z)
	{
		World.Player.ClearSteps();
		Reset();
		if (x != -1)
		{
			World.Player.X = (ushort)x;
			World.Player.Y = (ushort)y;
			World.Player.Z = z;
			World.Player.UpdateScreenPosition();
			World.RangeSize.X = x;
			World.RangeSize.Y = y;
			World.Player.AddToTile();
		}
	}

	public void ConfirmWalk(byte sequence)
	{
		if (UnacceptedPacketsCount != 0)
		{
			UnacceptedPacketsCount--;
		}
		int num = 0;
		for (int i = 0; i < StepsCount && StepInfos[i].Sequence != sequence; i++)
		{
			num++;
		}
		bool flag = num == StepsCount;
		if (!flag)
		{
			if (num >= CurrentWalkSequence)
			{
				StepInfos[num].Accepted = true;
				World.RangeSize.X = StepInfos[num].X;
				World.RangeSize.Y = StepInfos[num].Y;
			}
			else if (num == 0)
			{
				World.RangeSize.X = StepInfos[0].X;
				World.RangeSize.Y = StepInfos[0].Y;
				for (int j = 1; j < StepsCount; j++)
				{
					StepInfos[j - 1] = StepInfos[j];
				}
				StepsCount--;
				CurrentWalkSequence--;
			}
			else
			{
				flag = true;
			}
		}
		if (flag)
		{
			if (!ResendPacketResync)
			{
				NetClient.Socket.Send_Resync();
				ResendPacketResync = true;
			}
			WalkingFailed = true;
			StepsCount = 0;
			CurrentWalkSequence = 0;
		}
	}

	public void Reset()
	{
		UnacceptedPacketsCount = 0;
		StepsCount = 0;
		WalkSequence = 0;
		CurrentWalkSequence = 0;
		WalkingFailed = false;
		ResendPacketResync = false;
		LastStepRequestTime = 0L;
	}
}

using System;

namespace ClassicUO.IO.Audio;

internal class UOSound : Sound
{
	private readonly byte[] _waveBuffer;

	public int X;

	public int Y;

	public bool CalculateByDistance { get; set; }

	public UOSound(string name, int index, byte[] buffer)
		: base(name, index)
	{
		_waveBuffer = buffer;
		Delay = (uint)((float)(buffer.Length - 32) / 88.2f);
	}

	protected override void OnBufferNeeded(object sender, EventArgs e)
	{
	}

	protected override byte[] GetBuffer()
	{
		return _waveBuffer;
	}
}

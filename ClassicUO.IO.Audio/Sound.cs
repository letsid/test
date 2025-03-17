using System;
using ClassicUO.IO.Resources;
using Microsoft.Xna.Framework.Audio;

namespace ClassicUO.IO.Audio;

internal abstract class Sound : IComparable<Sound>, IDisposable
{
	public enum SoundGroup
	{
		Standard,
		Tiersound,
		Bardensound
	}

	private uint _lastPlayedTime;

	private string m_Name;

	private float m_volume = 1f;

	private float m_volumeFactor;

	protected DynamicSoundEffectInstance SoundInstance;

	protected AudioChannels Channels = AudioChannels.Mono;

	protected uint Delay = 250u;

	protected int Frequency = 22050;

	public string Name
	{
		get
		{
			return m_Name;
		}
		private set
		{
			if (!string.IsNullOrEmpty(value))
			{
				m_Name = value.Replace(".mp3", "");
			}
			else
			{
				m_Name = string.Empty;
			}
		}
	}

	public int Index { get; }

	public double DurationTime { get; private set; }

	public float Volume
	{
		get
		{
			return m_volume;
		}
		set
		{
			if (value < 0f)
			{
				value = 0f;
			}
			else if (value > 1f)
			{
				value = 1f;
			}
			m_volume = value;
			float volume = Math.Max(value - VolumeFactor, 0f);
			if (SoundInstance != null && !SoundInstance.IsDisposed)
			{
				SoundInstance.Volume = volume;
			}
		}
	}

	public float VolumeFactor
	{
		get
		{
			return m_volumeFactor;
		}
		set
		{
			m_volumeFactor = value;
			Volume = m_volume;
		}
	}

	public SoundGroup VolumeGroup { get; set; }

	public bool IsPlaying
	{
		get
		{
			if (SoundInstance != null && SoundInstance.State == SoundState.Playing)
			{
				return DurationTime > (double)Time.Ticks;
			}
			return false;
		}
	}

	protected Sound(string name, int index)
	{
		Name = name;
		Index = index;
		VolumeGroup = SoundsLoader.Instance.GetSoundGroup((uint)index);
	}

	public int CompareTo(Sound other)
	{
		if (other != null)
		{
			return Index.CompareTo(other.Index);
		}
		return -1;
	}

	public void Dispose()
	{
		if (SoundInstance != null)
		{
			SoundInstance.BufferNeeded -= OnBufferNeeded;
			if (!SoundInstance.IsDisposed)
			{
				SoundInstance.Stop();
				SoundInstance.Dispose();
			}
			SoundInstance = null;
		}
	}

	public static float ScaleVolume(float soundVolume)
	{
		soundVolume = (float)Math.Pow(10.0, (soundVolume + 30f) / 50f - 3f);
		if ((double)soundVolume < 0.0041)
		{
			return 0f;
		}
		return soundVolume;
	}

	protected abstract byte[] GetBuffer();

	protected abstract void OnBufferNeeded(object sender, EventArgs e);

	protected virtual void AfterStop()
	{
	}

	protected virtual void BeforePlay()
	{
	}

	public bool Play(float volume = 1f, float volumeFactor = 0f, bool spamCheck = false)
	{
		if (_lastPlayedTime > Time.Ticks)
		{
			return false;
		}
		BeforePlay();
		if (SoundInstance != null && !SoundInstance.IsDisposed)
		{
			SoundInstance.Stop();
		}
		else
		{
			SoundInstance = new DynamicSoundEffectInstance(Frequency, Channels);
		}
		byte[] buffer = GetBuffer();
		if (buffer != null && buffer.Length != 0)
		{
			_lastPlayedTime = Time.Ticks + Delay;
			SoundInstance.BufferNeeded += OnBufferNeeded;
			SoundInstance.SubmitBuffer(buffer, 0, buffer.Length);
			VolumeFactor = volumeFactor;
			Volume = volume;
			DurationTime = (double)Time.Ticks + SoundInstance.GetSampleDuration(buffer.Length).TotalMilliseconds;
			SoundInstance.Play();
			return true;
		}
		return false;
	}

	public void Stop()
	{
		if (SoundInstance != null)
		{
			SoundInstance.BufferNeeded -= OnBufferNeeded;
			SoundInstance.Stop();
		}
		AfterStop();
	}
}

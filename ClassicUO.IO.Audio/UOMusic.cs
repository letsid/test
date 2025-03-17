using System;
using System.IO;
using ClassicUO.Configuration;
using ClassicUO.Utility.Logging;
using Microsoft.Xna.Framework.Audio;
using MP3Sharp;

namespace ClassicUO.IO.Audio;

internal class UOMusic : Sound
{
	private const int NUMBER_OF_PCM_BYTES_TO_READ_PER_CHUNK = 32768;

	private bool m_Playing;

	private readonly bool m_Repeat;

	private MP3Stream m_Stream;

	private readonly byte[] m_WaveBuffer = new byte[32768];

	private string Path { get; }

	public UOMusic(int index, string name, bool loop, string basePath)
		: base(name, index)
	{
		m_Repeat = loop;
		m_Playing = false;
		Channels = AudioChannels.Stereo;
		Delay = 0u;
		Path = System.IO.Path.Combine(Settings.GlobalSettings.UltimaOnlineDirectory, basePath + "/" + base.Name + ".mp3");
	}

	public void Update()
	{
		OnBufferNeeded(null, null);
	}

	protected override byte[] GetBuffer()
	{
		try
		{
			if (m_Playing && SoundInstance != null)
			{
				int num = m_Stream.Read(m_WaveBuffer, 0, m_WaveBuffer.Length);
				if (num != 32768)
				{
					if (m_Repeat)
					{
						m_Stream.Position = 0L;
						m_Stream.Read(m_WaveBuffer, num, m_WaveBuffer.Length - num);
					}
					else if (num == 0)
					{
						Stop();
					}
				}
				return m_WaveBuffer;
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex.ToString());
		}
		Stop();
		return null;
	}

	protected override void OnBufferNeeded(object sender, EventArgs e)
	{
		if (!m_Playing)
		{
			return;
		}
		if (SoundInstance == null)
		{
			Stop();
			return;
		}
		while (SoundInstance.PendingBufferCount < 3)
		{
			byte[] buffer = GetBuffer();
			if (!SoundInstance.IsDisposed && buffer != null)
			{
				SoundInstance.SubmitBuffer(buffer);
				continue;
			}
			break;
		}
	}

	protected override void BeforePlay()
	{
		if (m_Playing)
		{
			Stop();
		}
		try
		{
			if (m_Stream != null)
			{
				m_Stream.Close();
				m_Stream = null;
			}
			m_Stream = new MP3Stream(Path, 32768);
			Frequency = m_Stream.Frequency;
			m_Playing = true;
		}
		catch
		{
			m_Playing = false;
		}
	}

	protected override void AfterStop()
	{
		if (m_Playing)
		{
			m_Playing = false;
			m_Stream?.Close();
			m_Stream = null;
		}
	}
}

using System;
using System.Collections.Generic;
using ClassicUO.Configuration;
using ClassicUO.Data;
using ClassicUO.IO.Audio;
using ClassicUO.IO.Resources;
using ClassicUO.Utility.Logging;
using Microsoft.Xna.Framework.Audio;

namespace ClassicUO.Game.Managers;

internal class AudioManager
{
	private bool _canReproduceAudio = true;

	private readonly LinkedList<UOSound> _currentSounds = new LinkedList<UOSound>();

	private readonly UOMusic[] _currentMusic = new UOMusic[2];

	private readonly int[] _currentMusicIndices = new int[2];

	public int LoginMusicIndex { get; private set; }

	public int DeathMusicIndex { get; } = 42;

	public void Initialize()
	{
		try
		{
			new DynamicSoundEffectInstance(0, AudioChannels.Stereo).Dispose();
		}
		catch (NoAudioHardwareException ex)
		{
			Log.Warn(ex.ToString());
			_canReproduceAudio = false;
		}
		LoginMusicIndex = ((Client.Version >= ClientVersion.CV_7000) ? 78 : ((Client.Version <= ClientVersion.CV_308Z) ? 8 : 0));
		Client.Game.Activated += OnWindowActivated;
		Client.Game.Deactivated += OnWindowDeactivated;
	}

	private void OnWindowDeactivated(object sender, EventArgs e)
	{
		if (_canReproduceAudio && ProfileManager.CurrentProfile != null && !ProfileManager.CurrentProfile.ReproduceSoundsInBackground)
		{
			SoundEffect.MasterVolume = 0f;
		}
	}

	private void OnWindowActivated(object sender, EventArgs e)
	{
		if (_canReproduceAudio && ProfileManager.CurrentProfile != null && !ProfileManager.CurrentProfile.ReproduceSoundsInBackground)
		{
			SoundEffect.MasterVolume = 1f;
		}
	}

	public void PlaySound(int index)
	{
		Profile currentProfile = ProfileManager.CurrentProfile;
		if (!_canReproduceAudio || currentProfile == null)
		{
			return;
		}
		float num = Sound.ScaleVolume(currentProfile.SoundVolume);
		if (!Client.Game.IsActive && !currentProfile.ReproduceSoundsInBackground)
		{
			num = 0f;
		}
		if (!(num < -1f) && !(num > 1f))
		{
			if (!currentProfile.EnableSound || (!Client.Game.IsActive && !currentProfile.ReproduceSoundsInBackground))
			{
				num = 0f;
			}
			UOSound uOSound = (UOSound)SoundsLoader.Instance.GetSound(index);
			if (uOSound != null && uOSound.Play(num))
			{
				uOSound.X = -1;
				uOSound.Y = -1;
				uOSound.CalculateByDistance = false;
				_currentSounds.AddLast(uOSound);
			}
		}
	}

	public void PlaySoundWithDistance(int index, int x, int y)
	{
		if (!_canReproduceAudio || !World.InGame)
		{
			return;
		}
		Profile currentProfile = ProfileManager.CurrentProfile;
		if (currentProfile == null || !currentProfile.EnableSound || (!Client.Game.IsActive && !currentProfile.ReproduceSoundsInBackground))
		{
			return;
		}
		UOSound uOSound = (UOSound)SoundsLoader.Instance.GetSound(index);
		int val = Math.Abs(x - World.Player.X);
		int val2 = Math.Abs(y - World.Player.Y);
		int num = Math.Max(val, val2);
		float soundVolume = currentProfile.SoundVolume;
		if (uOSound != null)
		{
			switch (uOSound.VolumeGroup)
			{
			case Sound.SoundGroup.Tiersound:
				soundVolume = currentProfile.AnimalSoundsVolume;
				break;
			case Sound.SoundGroup.Bardensound:
				soundVolume = currentProfile.BardSoundsVolume;
				break;
			}
		}
		float num2 = Sound.ScaleVolume(soundVolume);
		float volumeFactor = 0f;
		if (num >= 1)
		{
			volumeFactor = num2 / (float)(World.ClientViewRange + 1) * (float)num;
		}
		if (num > World.ClientViewRange)
		{
			num2 = 0f;
		}
		if (!(num2 < -1f) && !(num2 > 1f) && uOSound != null && uOSound.Play(num2, volumeFactor))
		{
			uOSound.X = x;
			uOSound.Y = y;
			uOSound.CalculateByDistance = true;
			_currentSounds.AddLast(uOSound);
		}
	}

	public void PlayMusic(int music, bool iswarmode = false, bool is_login = false)
	{
		if (!_canReproduceAudio || music >= 150)
		{
			return;
		}
		float num;
		if (is_login)
		{
			num = (Settings.GlobalSettings.LoginMusic ? Sound.ScaleVolume(Settings.GlobalSettings.LoginMusicVolume) : 0f);
		}
		else
		{
			Profile currentProfile = ProfileManager.CurrentProfile;
			num = ((currentProfile != null && currentProfile.EnableMusic) ? Sound.ScaleVolume(currentProfile.MusicVolume) : 0f);
			if (currentProfile != null && !currentProfile.EnableCombatMusic && iswarmode)
			{
				return;
			}
		}
		if (!(num < -1f) && !(num > 1f))
		{
			Sound music2 = SoundsLoader.Instance.GetMusic(music);
			if (music2 == null && _currentMusic[0] != null)
			{
				StopMusic();
			}
			else if (music2 != null && (music2 != _currentMusic[0] || iswarmode))
			{
				StopMusic();
				int num2 = (iswarmode ? 1 : 0);
				_currentMusicIndices[num2] = music;
				_currentMusic[num2] = (UOMusic)music2;
				_currentMusic[num2].Play(num);
			}
		}
	}

	public void UpdateCurrentMusicVolume(bool isLogin = false)
	{
		if (!_canReproduceAudio)
		{
			return;
		}
		for (int i = 0; i < 2; i++)
		{
			if (_currentMusic[i] != null)
			{
				float num;
				if (isLogin)
				{
					num = (Settings.GlobalSettings.LoginMusic ? ((float)Settings.GlobalSettings.LoginMusicVolume / 250f) : 0f);
				}
				else
				{
					Profile currentProfile = ProfileManager.CurrentProfile;
					num = ((currentProfile == null || !currentProfile.EnableMusic) ? 0f : ((float)currentProfile.MusicVolume / 250f));
				}
				if (num < -1f || num > 1f)
				{
					break;
				}
				_currentMusic[i].Volume = ((i == 0 && _currentMusic[1] != null) ? 0f : num);
			}
		}
	}

	public void UpdateCurrentSoundsVolume()
	{
		if (!_canReproduceAudio)
		{
			return;
		}
		Profile currentProfile = ProfileManager.CurrentProfile;
		float num = ((currentProfile == null || !currentProfile.EnableSound) ? 0f : ((float)currentProfile.SoundVolume / 250f));
		if (!(num < -1f) && !(num > 1f))
		{
			for (LinkedListNode<UOSound> linkedListNode = _currentSounds.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
			{
				linkedListNode.Value.Volume = num;
			}
		}
	}

	public void StopMusic()
	{
		for (int i = 0; i < 2; i++)
		{
			if (_currentMusic[i] != null)
			{
				_currentMusic[i].Stop();
				_currentMusic[i].Dispose();
				_currentMusic[i] = null;
			}
		}
	}

	public void StopWarMusic()
	{
		PlayMusic(_currentMusicIndices[0]);
	}

	public void StopSounds()
	{
		LinkedListNode<UOSound> linkedListNode = _currentSounds.First;
		while (linkedListNode != null)
		{
			LinkedListNode<UOSound>? next = linkedListNode.Next;
			linkedListNode.Value.Stop();
			_currentSounds.Remove(linkedListNode);
			linkedListNode = next;
		}
	}

	public void Update()
	{
		if (!_canReproduceAudio)
		{
			return;
		}
		bool flag = _currentMusic[1] != null;
		Profile currentProfile = ProfileManager.CurrentProfile;
		for (int i = 0; i < 2; i++)
		{
			if (_currentMusic[i] != null && currentProfile != null)
			{
				if (Client.Game.IsActive)
				{
					if (!currentProfile.ReproduceSoundsInBackground)
					{
						_currentMusic[i].Volume = (((i == 0 && flag) || !currentProfile.EnableMusic) ? 0f : ((float)currentProfile.MusicVolume / 250f));
					}
				}
				else if (!currentProfile.ReproduceSoundsInBackground && _currentMusic[i].Volume != 0f)
				{
					_currentMusic[i].Volume = 0f;
				}
			}
			_currentMusic[i]?.Update();
		}
		LinkedListNode<UOSound> linkedListNode = _currentSounds.First;
		while (linkedListNode != null)
		{
			LinkedListNode<UOSound>? next = linkedListNode.Next;
			if (!linkedListNode.Value.IsPlaying)
			{
				linkedListNode.Value.Stop();
				_currentSounds.Remove(linkedListNode);
			}
			linkedListNode = next;
		}
	}

	public UOMusic GetCurrentMusic()
	{
		for (int i = 0; i < 2; i++)
		{
			if (_currentMusic[i] != null && _currentMusic[i].IsPlaying)
			{
				return _currentMusic[i];
			}
		}
		return null;
	}
}

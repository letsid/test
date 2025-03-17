using System;
using ClassicUO.Game.Managers;
using ClassicUO.Input;
using ClassicUO.Renderer;
using SDL2;

namespace ClassicUO.Game.Scenes;

internal abstract class Scene : IDisposable
{
	private uint _time_cleanup = Time.Ticks + 5000;

	public readonly bool CanResize;

	public readonly bool CanBeMaximized;

	public readonly bool CanLoadAudio;

	public readonly int ID;

	public bool IsDestroyed { get; private set; }

	public bool IsLoaded { get; private set; }

	public int RenderedObjectsCount { get; protected set; }

	public AudioManager Audio { get; set; }

	public Camera Camera { get; }

	protected Scene(int sceneID, bool canresize, bool maximized, bool loadaudio)
	{
		CanResize = canresize;
		CanBeMaximized = maximized;
		CanLoadAudio = loadaudio;
		Camera = new Camera();
	}

	public virtual void Dispose()
	{
		if (!IsDestroyed)
		{
			IsDestroyed = true;
			Unload();
		}
	}

	public virtual void Update(double totalTime, double frameTime)
	{
		Audio?.Update();
		Camera.Update();
		if (_time_cleanup < Time.Ticks)
		{
			World.Map?.ClearUnusedBlocks();
			_time_cleanup = Time.Ticks + 500;
		}
	}

	public virtual void FixedUpdate(double totalTime, double frameTime)
	{
	}

	public virtual void Load()
	{
		if (CanLoadAudio)
		{
			Audio = new AudioManager();
			Audio.Initialize();
		}
		IsLoaded = true;
	}

	public virtual void Unload()
	{
		Audio?.StopMusic();
	}

	public virtual bool Draw(UltimaBatcher2D batcher)
	{
		return true;
	}

	internal virtual bool OnMouseUp(MouseButtonType button)
	{
		return false;
	}

	internal virtual bool OnMouseDown(MouseButtonType button)
	{
		return false;
	}

	internal virtual bool OnMouseDoubleClick(MouseButtonType button)
	{
		return false;
	}

	internal virtual bool OnMouseWheel(bool up)
	{
		return false;
	}

	internal virtual bool OnMouseDragging()
	{
		return false;
	}

	internal virtual void OnTextInput(string text)
	{
	}

	internal virtual void OnKeyDown(SDL.SDL_KeyboardEvent e)
	{
	}

	internal virtual void OnKeyUp(SDL.SDL_KeyboardEvent e)
	{
	}
}

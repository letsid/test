using System;
using System.Collections.Generic;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Input;
using ClassicUO.Resources;
using ClassicUO.Utility.Logging;

namespace ClassicUO.Game.Managers;

internal static class CommandManager
{
	private static readonly Dictionary<string, Action<string[]>> _commands = new Dictionary<string, Action<string[]>>();

	public static void Initialize()
	{
		Register("info", delegate
		{
			if (TargetManager.IsTargeting)
			{
				TargetManager.CancelTarget();
			}
			TargetManager.SetTargeting(CursorTarget.SetTargetClientSide, CursorType.Target, TargetType.Neutral);
		});
		Register("datetime", delegate
		{
			if (World.Player != null)
			{
				GameActions.Print(string.Format(ResGeneral.CurrentDateTimeNowIs0, DateTime.Now), 946, MessageType.Regular, 3);
			}
		});
		Register("hue", delegate
		{
			if (TargetManager.IsTargeting)
			{
				TargetManager.CancelTarget();
			}
			TargetManager.SetTargeting(CursorTarget.HueCommandTarget, CursorType.Target, TargetType.Neutral);
		});
		Register("debug", delegate
		{
			CUOEnviroment.Debug = !CUOEnviroment.Debug;
		});
	}

	public static void Register(string name, Action<string[]> callback)
	{
		name = name.ToLower();
		if (!_commands.ContainsKey(name))
		{
			_commands.Add(name, callback);
		}
		else
		{
			Log.Error("Attempted to register command: '" + name + "' twice.");
		}
	}

	public static void UnRegister(string name)
	{
		name = name.ToLower();
		if (_commands.ContainsKey(name))
		{
			_commands.Remove(name);
		}
	}

	public static void UnRegisterAll()
	{
		_commands.Clear();
	}

	public static void Execute(string name, params string[] args)
	{
		name = name.ToLower();
		if (_commands.TryGetValue(name, out var value))
		{
			value(args);
		}
		else
		{
			Log.Warn("Command: '" + name + "' not exists");
		}
	}

	public static void OnHueTarget(Entity entity)
	{
		if (entity != null)
		{
			TargetManager.Target(entity);
		}
		Mouse.LastLeftButtonClickTime = 0u;
		GameActions.Print(string.Format(ResGeneral.ItemID0Hue1, entity.Graphic, entity.Hue), 946, MessageType.Regular, 3);
	}
}

using System.Collections.Generic;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.Resources;

namespace ClassicUO.Game.UI.Controls;

internal class HotkeyControl : Control
{
	private readonly List<HotkeyBox> _hotkesBoxes = new List<HotkeyBox>();

	private readonly HotkeyAction _key;

	public HotkeyControl(string text, HotkeyAction key)
	{
		_key = key;
		CanMove = true;
		AcceptMouseInput = true;
		Add(new Label(text, isunicode: true, 0, 150, 1));
		AddNew(key);
	}

	public void AddNew(HotkeyAction action)
	{
		HotkeyBox hotkeyBox = new HotkeyBox();
		hotkeyBox.X = 150;
		HotkeyBox box = hotkeyBox;
		box.HotkeyChanged += delegate
		{
			GameScene scene = Client.Game.GetScene<GameScene>();
			if (scene != null && !scene.Hotkeys.Bind(_key, box.Key, box.Mod))
			{
				UIManager.Add(new MessageBoxGump(400, 200, ResGumps.KeyCombinationAlreadyExists, null, hasBackground: false, MessageButtonType.OK, 1));
			}
		};
		box.HotkeyCancelled += delegate
		{
			Client.Game.GetScene<GameScene>()?.Hotkeys.UnBind(_key);
		};
		if (_hotkesBoxes.Count != 0)
		{
			box.Y = _hotkesBoxes[_hotkesBoxes.Count - 1].Bounds.Bottom;
		}
		_hotkesBoxes.Add(box);
		Add(box);
	}
}

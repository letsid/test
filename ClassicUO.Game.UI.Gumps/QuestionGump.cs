using System;
using ClassicUO.Game.UI.Controls;
using ClassicUO.IO.Resources;

namespace ClassicUO.Game.UI.Gumps;

internal class QuestionGump : Gump
{
	private enum Buttons
	{
		Cancel,
		Ok
	}

	private readonly Action<bool> _result;

	public QuestionGump(string message, Action<bool> result)
		: base(0u, 0u)
	{
		base.CanCloseWithRightClick = true;
		Add(new GumpPic(0, 0, 2070, 0));
		GumpsLoader.Instance.GetGumpTexture(2070u, out var bounds);
		base.Width = bounds.Width;
		base.Height = bounds.Height;
		Label label = new Label(message, isunicode: false, 902, 165);
		label.X = 33;
		label.Y = 30;
		Add(label);
		Button button = new Button(0, 2071, 2072, 2073, "", 0);
		button.X = 37;
		button.Y = 75;
		button.ButtonAction = ButtonAction.Activate;
		Add(button);
		Button button2 = new Button(1, 2074, 2075, 2076, "", 0);
		button2.X = 100;
		button2.Y = 75;
		button2.ButtonAction = ButtonAction.Activate;
		Add(button2);
		CanMove = false;
		base.IsModal = true;
		base.X = Client.Game.Window.ClientBounds.Width - base.Width >> 1;
		base.Y = Client.Game.Window.ClientBounds.Height - base.Height >> 1;
		base.WantUpdateSize = false;
		_result = result;
	}

	public override void OnButtonClick(int buttonID)
	{
		switch (buttonID)
		{
		case 0:
			_result(obj: false);
			Dispose();
			break;
		case 1:
			_result(obj: true);
			Dispose();
			break;
		}
	}
}

using ClassicUO.Data;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.Network;
using ClassicUO.Renderer;

namespace ClassicUO.Game.UI.Gumps;

internal class BulletinBoardObject : Control
{
	public BulletinBoardObject(uint serial, string text)
	{
		base.LocalSerial = serial;
		CanMove = true;
		base.Width = 230;
		base.Height = 18;
		Add(new GumpPic(0, 0, 5411, 0));
		if (Client.Version >= ClientVersion.CV_305D)
		{
			Label label = new Label(text, isunicode: true, 0, base.Width - 23, 1, FontStyle.Fixed);
			label.X = 23;
			label.Y = 1;
			Add(label);
		}
		else
		{
			Label label2 = new Label(text, isunicode: false, 902, base.Width - 23, 9, FontStyle.Fixed);
			label2.X = 23;
			label2.Y = 1;
			Add(label2);
		}
		base.WantUpdateSize = false;
	}

	protected override bool OnMouseDoubleClick(int x, int y, MouseButtonType button)
	{
		if (button != MouseButtonType.Left)
		{
			return false;
		}
		Control rootParent = base.RootParent;
		if (rootParent != null)
		{
			NetClient.Socket.Send_BulletinBoardRequestMessage(rootParent.LocalSerial, base.LocalSerial);
		}
		return true;
	}
}

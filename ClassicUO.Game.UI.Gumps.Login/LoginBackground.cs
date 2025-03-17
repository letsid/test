using ClassicUO.Data;
using ClassicUO.Game.UI.Controls;

namespace ClassicUO.Game.UI.Gumps.Login;

internal class LoginBackground : Gump
{
	public LoginBackground()
		: base(0u, 0u)
	{
		if (Client.Version >= ClientVersion.CV_706400)
		{
			Add(new GumpPicTiled(0, 0, 640, 480, 336)
			{
				AcceptKeyboardInput = false
			});
			Add(new GumpPic(0, 4, 337, 0)
			{
				AcceptKeyboardInput = false
			});
		}
		else
		{
			Add(new GumpPicTiled(0, 0, 640, 480, 3604)
			{
				AcceptKeyboardInput = false
			});
			Add(new GumpPic(0, 0, 5500, 0)
			{
				AcceptKeyboardInput = false
			});
			Add(new GumpPic(0, 4, 5536, 0)
			{
				AcceptKeyboardInput = false
			});
			Button button = new Button(0, 5513, 5515, 5514, "", 0);
			button.X = 555;
			button.Y = 4;
			button.ButtonAction = ButtonAction.Activate;
			button.AcceptKeyboardInput = false;
			Add(button);
		}
		base.CanCloseWithEsc = false;
		base.CanCloseWithRightClick = false;
		AcceptKeyboardInput = false;
		base.LayerOrder = UILayer.Under;
	}

	public override void OnButtonClick(int buttonID)
	{
		Client.Game.Exit();
	}
}

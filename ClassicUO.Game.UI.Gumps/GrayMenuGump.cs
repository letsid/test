using System.Linq;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Network;

namespace ClassicUO.Game.UI.Gumps;

internal class GrayMenuGump : Gump
{
	private readonly ResizePic _resizePic;

	public GrayMenuGump(uint local, uint serv, string name)
		: base(local, serv)
	{
		CanMove = true;
		AcceptMouseInput = true;
		base.CanCloseWithRightClick = false;
		base.IsFromServer = true;
		ResizePic resizePic = new ResizePic(5100);
		resizePic.Width = 400;
		resizePic.Height = 111111;
		ResizePic c = resizePic;
		_resizePic = resizePic;
		Add(c);
		Label label = new Label(name, isunicode: false, 902, 370, 1);
		label.X = 20;
		label.Y = 16;
		Label label2 = label;
		Add(label);
		base.Width = _resizePic.Width;
		base.Height = label2.Height;
	}

	public void SetHeight(int h)
	{
		_resizePic.Height = h;
		base.Width = _resizePic.Width;
		base.Height = _resizePic.Height;
	}

	public int AddItem(string name, int y)
	{
		RadioButton radioButton = new RadioButton(0, 5002, 5003, name, 1, 902, isunicode: false, 330);
		radioButton.X = 25;
		radioButton.Y = y;
		RadioButton radioButton2 = radioButton;
		Add(radioButton2);
		return radioButton2.Height;
	}

	public override void OnButtonClick(int buttonID)
	{
		switch (buttonID)
		{
		case 0:
			NetClient.Socket.Send_GrayMenuResponse(base.LocalSerial, (ushort)base.ServerSerial, 0);
			Dispose();
			break;
		case 1:
		{
			ushort num = 1;
			{
				foreach (RadioButton item in base.Children.OfType<RadioButton>())
				{
					if (item.IsChecked)
					{
						NetClient.Socket.Send_GrayMenuResponse(base.LocalSerial, (ushort)base.ServerSerial, num);
						Dispose();
						break;
					}
					num++;
				}
				break;
			}
		}
		}
	}
}

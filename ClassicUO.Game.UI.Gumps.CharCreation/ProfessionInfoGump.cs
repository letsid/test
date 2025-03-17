using System;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO.Resources;

namespace ClassicUO.Game.UI.Gumps.CharCreation;

internal class ProfessionInfoGump : Control
{
	private readonly ProfessionInfo _info;

	public Action<ProfessionInfo> Selected;

	public ProfessionInfoGump(ProfessionInfo info)
	{
		_info = info;
		ClilocLoader instance = ClilocLoader.Instance;
		ResizePic resizePic = new ResizePic(3000);
		resizePic.Width = 175;
		resizePic.Height = 34;
		ResizePic resizePic2 = resizePic;
		resizePic2.SetTooltip(instance.GetString(info.Description), 250);
		Add(resizePic2);
		Label label = new Label(instance.GetString(info.Localization), isunicode: true, 0, 0, 1);
		label.X = 7;
		label.Y = 8;
		Add(label);
		Add(new GumpPic(121, -12, info.Graphic, 0));
	}

	protected override void OnMouseUp(int x, int y, MouseButtonType button)
	{
		base.OnMouseUp(x, y, button);
		if (button == MouseButtonType.Left)
		{
			Selected?.Invoke(_info);
		}
	}
}

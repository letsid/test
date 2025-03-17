using System;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Network;

namespace ClassicUO.Game.UI.Gumps;

internal class ColorPickerGump : Gump
{
	private const int SLIDER_MIN = 0;

	private const int SLIDER_MAX = 4;

	private readonly ColorPickerBox _box;

	private readonly StaticPic _dyeTybeImage;

	private readonly ushort _graphic;

	private readonly Action<ushort> _okClicked;

	public ushort Graphic => _graphic;

	public ColorPickerGump(uint serial, ushort graphic, int x, int y, Action<ushort> okClicked)
		: base(serial, 0u)
	{
		base.CanCloseWithRightClick = serial == 0;
		_graphic = graphic;
		CanMove = true;
		AcceptMouseInput = false;
		base.X = x;
		base.Y = y;
		Add(new GumpPic(0, 0, 2310, 0));
		Button button = new Button(0, 2311, 2312, 2313, "", 0);
		button.X = 208;
		button.Y = 138;
		button.ButtonAction = ButtonAction.Activate;
		Add(button);
		HSliderBar slider;
		Add(slider = new HSliderBar(39, 142, 145, 0, 4, 1, HSliderBarStyle.BlueWidgetNoBar, hasText: false, 0, 0));
		slider.ValueChanged += delegate
		{
			_box.Graduation = slider.Value;
		};
		Add(_box = new ColorPickerBox(34, 34));
		_box.ColorSelectedIndex += delegate
		{
			_dyeTybeImage.Hue = _box.SelectedHue;
		};
		StaticPic staticPic = new StaticPic(4011, 0);
		staticPic.X = 200;
		staticPic.Y = 58;
		StaticPic c = staticPic;
		_dyeTybeImage = staticPic;
		Add(c);
		_okClicked = okClicked;
		_dyeTybeImage.Hue = _box.SelectedHue;
	}

	public override void OnButtonClick(int buttonID)
	{
		if (buttonID == 0)
		{
			if (base.LocalSerial != 0)
			{
				NetClient.Socket.Send_DyeDataResponse(base.LocalSerial, _graphic, _box.SelectedHue);
			}
			_okClicked?.Invoke(_box.SelectedHue);
			Dispose();
		}
	}
}

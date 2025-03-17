using ClassicUO.Game.UI.Controls;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Gumps;

internal class CreditsGump : Gump
{
	private const ushort BACKGROUND_IMG = 1280;

	private Point _offset;

	private uint _lastUpdate;

	private const string CREDITS = "\r\nCopyright(R) ClassicUO 2021\r\n\r\nThis project does not distribute any copyrighted game assets.\r\nIn order to run this client you'll need to legally obtain a\r\ncopy of the Ultima Online Classic Client.\r\n\r\nUsing a custom client to connect to official UO servers is\r\nstrictly forbidden. \r\nWe do not assume any responsibility of the usage of this client.\r\n\r\n\r\nUltima Online(R) 2021 Electronic Arts Inc. All Rights Reserved.\r\n\r\n\r\n\r\n\r\n                [Lead Developer]\r\n                Karasho' - https://github.com/andreakarasho\r\n";

	public CreditsGump()
		: base(0u, 0u)
	{
		Client.Game.Scene.Audio.PlayMusic(8, iswarmode: false, is_login: true);
		base.LayerOrder = UILayer.Over;
		base.CanCloseWithRightClick = true;
		GumpPic gumpPic = new GumpPic(0, 0, 1280, 0);
		base.Width = gumpPic.Width;
		base.Height = gumpPic.Height;
		AlphaBlendControl alphaBlendControl = new AlphaBlendControl(1f);
		alphaBlendControl.Width = gumpPic.Width;
		alphaBlendControl.Height = gumpPic.Height;
		Add(alphaBlendControl);
		Add(gumpPic);
		Vector2 vector = Fonts.Regular.MeasureString("\r\nCopyright(R) ClassicUO 2021\r\n\r\nThis project does not distribute any copyrighted game assets.\r\nIn order to run this client you'll need to legally obtain a\r\ncopy of the Ultima Online Classic Client.\r\n\r\nUsing a custom client to connect to official UO servers is\r\nstrictly forbidden. \r\nWe do not assume any responsibility of the usage of this client.\r\n\r\n\r\nUltima Online(R) 2021 Electronic Arts Inc. All Rights Reserved.\r\n\r\n\r\n\r\n\r\n                [Lead Developer]\r\n                Karasho' - https://github.com/andreakarasho\r\n");
		_offset.X = (int)((float)base.Width / 2f - vector.X / 2f);
		_offset.Y = base.Height;
	}

	public override void Update(double totalTime, double frameTime)
	{
		base.Update(totalTime, frameTime);
		if (_lastUpdate < Time.Ticks)
		{
			_offset.Y--;
			_lastUpdate = Time.Ticks + 25;
		}
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		base.Draw(batcher, x, y);
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(0);
		batcher.DrawString(Fonts.Bold, "\r\nCopyright(R) ClassicUO 2021\r\n\r\nThis project does not distribute any copyrighted game assets.\r\nIn order to run this client you'll need to legally obtain a\r\ncopy of the Ultima Online Classic Client.\r\n\r\nUsing a custom client to connect to official UO servers is\r\nstrictly forbidden. \r\nWe do not assume any responsibility of the usage of this client.\r\n\r\n\r\nUltima Online(R) 2021 Electronic Arts Inc. All Rights Reserved.\r\n\r\n\r\n\r\n\r\n                [Lead Developer]\r\n                Karasho' - https://github.com/andreakarasho\r\n", x + _offset.X, y + _offset.Y, hueVector);
		return true;
	}
}

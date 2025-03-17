using System.Collections.Generic;
using System.Linq;
using System.Xml;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Controls;

namespace ClassicUO.Game.UI.Gumps;

internal class InfoBarGump : Gump
{
	private readonly AlphaBlendControl _background;

	private readonly List<InfoBarControl> _infobarControls = new List<InfoBarControl>();

	private long _refreshTime;

	public override GumpType GumpType => GumpType.InfoBar;

	public InfoBarGump()
		: base(0u, 0u)
	{
		CanMove = true;
		AcceptMouseInput = true;
		AcceptKeyboardInput = false;
		base.CanCloseWithRightClick = false;
		base.Height = 20;
		AlphaBlendControl alphaBlendControl = new AlphaBlendControl(0.7f);
		alphaBlendControl.Width = base.Width;
		alphaBlendControl.Height = base.Height;
		AlphaBlendControl c = alphaBlendControl;
		_background = alphaBlendControl;
		Add(c);
		ResetItems();
	}

	public void ResetItems()
	{
		foreach (InfoBarControl infobarControl in _infobarControls)
		{
			infobarControl.Dispose();
		}
		_infobarControls.Clear();
		List<InfoBarItem> infoBars = Client.Game.GetScene<GameScene>().InfoBars.GetInfoBars();
		for (int i = 0; i < infoBars.Count; i++)
		{
			InfoBarControl infoBarControl = new InfoBarControl(infoBars[i].label, infoBars[i].var, infoBars[i].hue);
			_infobarControls.Add(infoBarControl);
			Add(infoBarControl);
		}
	}

	public override void Save(XmlTextWriter writer)
	{
		base.Save(writer);
	}

	public override void Restore(XmlElement xml)
	{
		base.Restore(xml);
	}

	public override void Update(double totalTime, double frameTime)
	{
		if (base.IsDisposed)
		{
			return;
		}
		if ((double)_refreshTime < totalTime)
		{
			_refreshTime = (long)totalTime + 125;
			int num = 5;
			foreach (InfoBarControl infobarControl in _infobarControls)
			{
				infobarControl.X = num;
				num += infobarControl.Width + 5;
			}
		}
		base.Update(totalTime, frameTime);
		Control control = base.Children.LastOrDefault();
		if (control != null)
		{
			base.Width = control.Bounds.Right;
		}
		_background.Width = base.Width;
	}
}

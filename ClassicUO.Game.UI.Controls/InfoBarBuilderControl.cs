using ClassicUO.Game.Managers;
using ClassicUO.Renderer;
using ClassicUO.Resources;

namespace ClassicUO.Game.UI.Controls;

internal class InfoBarBuilderControl : Control
{
	private readonly StbTextBox infoLabel;

	private readonly ClickableColorBox labelColor;

	private readonly Combobox varStat;

	public string LabelText => infoLabel.Text;

	public InfoBarVars Var => (InfoBarVars)varStat.SelectedIndex;

	public ushort Hue => labelColor.Hue;

	public InfoBarBuilderControl(InfoBarItem item)
	{
		StbTextBox stbTextBox = new StbTextBox(byte.MaxValue, 10, 80, isunicode: true, FontStyle.None, 0);
		stbTextBox.X = 5;
		stbTextBox.Y = 0;
		stbTextBox.Width = 130;
		stbTextBox.Height = 26;
		infoLabel = stbTextBox;
		infoLabel.SetText(item.label);
		string[] vars = InfoBarManager.GetVars();
		varStat = new Combobox(200, 0, 170, vars, (int)item.var, 200, showArrow: true, "", 9);
		labelColor = new ClickableColorBox(150, 0, 13, 14, item.hue);
		NiceButton niceButton = new NiceButton(390, 0, 60, 25, ButtonAction.Activate, ResGumps.Delete)
		{
			ButtonParameter = 999
		};
		niceButton.MouseUp += delegate
		{
			Dispose();
			((DataBox)base.Parent)?.ReArrangeChildren();
		};
		ResizePic resizePic = new ResizePic(3000);
		resizePic.X = infoLabel.X - 5;
		resizePic.Y = 0;
		resizePic.Width = infoLabel.Width + 10;
		resizePic.Height = infoLabel.Height;
		Add(resizePic);
		Add(infoLabel);
		Add(varStat);
		Add(labelColor);
		Add(niceButton);
		base.Width = infoLabel.Width + 10;
		base.Height = infoLabel.Height;
	}
}

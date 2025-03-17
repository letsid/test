using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Controls;

internal class NiceButton : HitBox
{
	private readonly ButtonAction _action;

	private readonly int _groupnumber;

	private bool _isSelected;

	internal Label TextLabel { get; }

	public int ButtonParameter { get; set; }

	public bool IsSelectable { get; set; } = true;

	public bool IsSelected
	{
		get
		{
			if (_isSelected)
			{
				return IsSelectable;
			}
			return false;
		}
		set
		{
			if (!IsSelectable)
			{
				return;
			}
			_isSelected = value;
			if (!value)
			{
				return;
			}
			Control parent = base.Parent;
			if (parent == null)
			{
				return;
			}
			foreach (NiceButton item in parent.FindControls<NiceButton>())
			{
				if (item != this && item._groupnumber == _groupnumber)
				{
					item.IsSelected = false;
				}
			}
		}
	}

	public NiceButton(int x, int y, int w, int h, ButtonAction action, string text, int groupnumber = 0, TEXT_ALIGN_TYPE align = TEXT_ALIGN_TYPE.TS_CENTER, ushort hue = ushort.MaxValue, bool unicode = true, byte font = byte.MaxValue)
		: base(x, y, w, h)
	{
		_action = action;
		Add(TextLabel = new Label(text, unicode, hue, w, font, FontStyle.BlackBorder | FontStyle.Cropped, align));
		TextLabel.Y = h - TextLabel.Height >> 1;
		_groupnumber = groupnumber;
	}

	internal static NiceButton GetSelected(Control p, int group)
	{
		foreach (NiceButton item in p.FindControls<NiceButton>())
		{
			if (item._groupnumber == group && item.IsSelected)
			{
				return item;
			}
		}
		return null;
	}

	protected override void OnMouseUp(int x, int y, MouseButtonType button)
	{
		if (button == MouseButtonType.Left)
		{
			IsSelected = true;
			if (_action == ButtonAction.Default)
			{
				ChangePage(ButtonParameter);
			}
			else
			{
				OnButtonClick(ButtonParameter);
			}
		}
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		if (IsSelected)
		{
			Vector3 hueVector = ShaderHueTranslator.GetHueVector(0, partial: false, base.Alpha);
			batcher.Draw(_texture, new Vector2(x, y), new Rectangle(0, 0, base.Width, base.Height), hueVector);
		}
		return base.Draw(batcher, x, y);
	}
}

using ClassicUO.Game.Data;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Utility;

namespace ClassicUO.Game.UI.Gumps;

internal class PopupMenuGump : Gump
{
	private ushort _selectedItem;

	private readonly PopupMenuData _data;

	public PopupMenuGump(PopupMenuData data)
		: base(0u, 0u)
	{
		CanMove = false;
		base.CanCloseWithRightClick = true;
		_data = data;
		ResizePic resizePic = new ResizePic(2620)
		{
			Alpha = 0.75f
		};
		Add(resizePic);
		int num = 10;
		bool flag = false;
		int num2 = 0;
		int num3 = 20;
		for (int i = 0; i < data.Items.Length; i++)
		{
			ref PopupMenuItem reference = ref data.Items[i];
			string @string = ClilocLoader.Instance.GetString(reference.Cliloc);
			ushort hue = reference.Hue;
			if (reference.ReplacedHue != 0)
			{
				uint htmlStartColor = (HuesHelper.Color16To32(reference.ReplacedHue) << 8) | 0xFF;
				FontsLoader.Instance.SetUseHTML(value: true, htmlStartColor);
			}
			Label label = new Label(@string, isunicode: true, hue, 0, 1);
			label.X = 10;
			label.Y = num;
			Label label2 = label;
			FontsLoader.Instance.SetUseHTML(value: false);
			HitBox hitBox = new HitBox(10, num, label2.Width, label2.Height)
			{
				Tag = reference.Index
			};
			hitBox.MouseEnter += delegate(object? sender, MouseEventArgs e)
			{
				_selectedItem = (ushort)(sender as HitBox).Tag;
			};
			Add(hitBox);
			Add(label2);
			if ((reference.Flags & 2) != 0 && !flag)
			{
				flag = true;
				Button button = new Button(0, 5606, 5602, 5602, "", 0);
				button.X = 20;
				button.Y = num;
				Add(button);
				num3 += 20;
			}
			num += label2.Height;
			if (!flag)
			{
				num3 += label2.Height;
				if (num2 < label2.Width)
				{
					num2 = label2.Width;
				}
			}
		}
		num2 += 20;
		if (num3 <= 10 || num2 <= 20)
		{
			Dispose();
			return;
		}
		resizePic.Width = num2;
		resizePic.Height = num3;
		foreach (HitBox item in FindControls<HitBox>())
		{
			item.Width = num2 - 20;
		}
	}

	protected override void OnMouseUp(int x, int y, MouseButtonType button)
	{
		if (button == MouseButtonType.Left)
		{
			GameActions.ResponsePopupMenu(_data.Serial, _selectedItem);
			Dispose();
		}
	}
}

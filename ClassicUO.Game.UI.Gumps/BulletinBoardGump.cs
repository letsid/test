using System.Collections.Generic;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Resources;

namespace ClassicUO.Game.UI.Gumps;

internal class BulletinBoardGump : Gump
{
	private readonly DataBox _databox;

	public BulletinBoardGump(uint serial, int x, int y, string name)
		: base(serial, 0u)
	{
		base.X = x;
		base.Y = y;
		CanMove = true;
		base.CanCloseWithRightClick = true;
		Add(new GumpPic(0, 0, 2170, 0));
		Label label = new Label(name, isunicode: true, 1, 170, 1, FontStyle.None, TEXT_ALIGN_TYPE.TS_CENTER);
		label.X = 159;
		label.Y = 36;
		Label c = label;
		Add(c);
		HitBox hitBox = new HitBox(15, 170, 80, 80)
		{
			Alpha = 0f
		};
		hitBox.MouseUp += delegate
		{
			UIManager.GetGump<BulletinBoardItem>(base.LocalSerial)?.Dispose();
			BulletinBoardItem bulletinBoardItem = new BulletinBoardItem(base.LocalSerial, 0u, World.Player.Name, string.Empty, ResGumps.DateTime, string.Empty, 0);
			bulletinBoardItem.X = 400;
			bulletinBoardItem.Y = 335;
			UIManager.Add(bulletinBoardItem);
		};
		Add(hitBox);
		ScrollArea scrollArea = new ScrollArea(127, 159, 241, 195, normalScrollbar: false);
		Add(scrollArea);
		_databox = new DataBox(0, 0, 1, 1);
		_databox.WantUpdateSize = true;
		scrollArea.Add(_databox);
	}

	public override void Dispose()
	{
		for (LinkedListNode<Gump> linkedListNode = UIManager.Gumps.Last; linkedListNode != null; linkedListNode = linkedListNode.Previous)
		{
			if (linkedListNode.Value is BulletinBoardItem)
			{
				linkedListNode.Value.Dispose();
			}
		}
		base.Dispose();
	}

	public void RemoveBulletinObject(uint serial)
	{
		foreach (Control child in _databox.Children)
		{
			if (child.LocalSerial == serial)
			{
				child.Dispose();
				_databox.WantUpdateSize = true;
				_databox.ReArrangeChildren();
				break;
			}
		}
	}

	public void AddBulletinObject(uint serial, string msg)
	{
		foreach (Control child in _databox.Children)
		{
			if (child.LocalSerial == serial)
			{
				child.Dispose();
				break;
			}
		}
		BulletinBoardObject c = new BulletinBoardObject(serial, msg);
		_databox.Add(c);
		_databox.WantUpdateSize = true;
		_databox.ReArrangeChildren();
	}
}

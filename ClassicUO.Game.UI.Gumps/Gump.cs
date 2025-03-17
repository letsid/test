using System;
using System.Collections.Generic;
using System.Xml;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Gumps;

internal class Gump : Control
{
	public bool BlockMovement { get; set; }

	public bool CanBeSaved => GumpType != GumpType.None;

	public virtual GumpType GumpType { get; }

	public bool InvalidateContents { get; set; }

	public override bool CanMove
	{
		get
		{
			if (!BlockMovement)
			{
				return base.CanMove;
			}
			return false;
		}
		set
		{
			base.CanMove = value;
		}
	}

	public uint MasterGumpSerial { get; set; }

	public Gump(uint local, uint server)
	{
		base.LocalSerial = local;
		base.ServerSerial = server;
		AcceptMouseInput = false;
		AcceptKeyboardInput = false;
	}

	public override void Update(double totalTime, double frameTime)
	{
		if (InvalidateContents)
		{
			UpdateContents();
			InvalidateContents = false;
		}
		if (base.ActivePage == 0)
		{
			base.ActivePage = 1;
		}
		base.Update(totalTime, frameTime);
	}

	public override void Dispose()
	{
		Item item = World.Items.Get(base.LocalSerial);
		if (item != null && item.Opened)
		{
			item.Opened = false;
		}
		base.Dispose();
	}

	public virtual void Save(XmlTextWriter writer)
	{
		writer.WriteAttributeString("type", ((int)GumpType).ToString());
		writer.WriteAttributeString("x", base.X.ToString());
		writer.WriteAttributeString("y", base.Y.ToString());
		writer.WriteAttributeString("serial", base.LocalSerial.ToString());
	}

	public void SetInScreen()
	{
		Rectangle clientBounds = Client.Game.Window.ClientBounds;
		Rectangle bounds = base.Bounds;
		bounds.X += clientBounds.X;
		bounds.Y += clientBounds.Y;
		if (!clientBounds.Intersects(bounds))
		{
			base.X = 0;
			base.Y = 0;
		}
	}

	public virtual void Restore(XmlElement xml)
	{
	}

	public void RequestUpdateContents()
	{
		InvalidateContents = true;
	}

	protected virtual void UpdateContents()
	{
	}

	protected override void OnDragEnd(int x, int y)
	{
		Point location = base.Location;
		int num = base.Width - (base.Width >> 2);
		int num2 = base.Height - (base.Height >> 2);
		if (base.X < -num)
		{
			location.X = -num;
		}
		if (base.Y < -num2)
		{
			location.Y = -num2;
		}
		if (base.X > Client.Game.Window.ClientBounds.Width - (base.Width - num))
		{
			location.X = Client.Game.Window.ClientBounds.Width - (base.Width - num);
		}
		if (base.Y > Client.Game.Window.ClientBounds.Height - (base.Height - num2))
		{
			location.Y = Client.Game.Window.ClientBounds.Height - (base.Height - num2);
		}
		base.Location = location;
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		if (base.IsVisible)
		{
			return base.Draw(batcher, x, y);
		}
		return false;
	}

	public override void OnButtonClick(int buttonID)
	{
		if (base.IsDisposed || base.LocalSerial == 0)
		{
			return;
		}
		List<uint> list = new List<uint>();
		List<Tuple<ushort, string>> list2 = new List<Tuple<ushort, string>>();
		foreach (Control child in base.Children)
		{
			Control control = child;
			if (!(control is Checkbox checkbox))
			{
				if (control is StbTextBox stbTextBox)
				{
					list2.Add(new Tuple<ushort, string>((ushort)stbTextBox.LocalSerial, stbTextBox.Text));
				}
			}
			else if (checkbox.IsChecked)
			{
				list.Add(child.LocalSerial);
			}
		}
		GameActions.ReplyGump(base.LocalSerial, base.ServerSerial, buttonID, list.ToArray(), list2.ToArray());
		if (CanMove)
		{
			UIManager.SavePosition(base.ServerSerial, base.Location);
		}
		else
		{
			UIManager.RemovePosition(base.ServerSerial);
		}
		Dispose();
	}

	protected override void CloseWithRightClick()
	{
		if (base.CanCloseWithRightClick)
		{
			if (base.ServerSerial != 0)
			{
				OnButtonClick(0);
			}
			base.CloseWithRightClick();
		}
	}

	public override void ChangePage(int pageIndex)
	{
		base.ActivePage = pageIndex;
	}
}

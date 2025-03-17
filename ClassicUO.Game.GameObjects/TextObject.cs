using ClassicUO.Game.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.GameObjects;

internal class TextObject : BaseGameObject
{
	private static readonly QueuedPool<TextObject> _queue = new QueuedPool<TextObject>(1000, delegate(TextObject o)
	{
		o.IsDestroyed = false;
		o.Alpha = byte.MaxValue;
		o.Hue = 0;
		o.Time = 0L;
		o.IsTransparent = false;
		o.SecondTime = 0L;
		o.Type = MessageType.Regular;
		o.X = 0;
		o.Y = 0;
		o.RealScreenPosition = Point.Zero;
		o.OffsetY = 0;
		o.Owner = null;
		o.UnlinkD();
		o.IsTextGump = false;
		o.RenderedText?.Destroy();
		o.RenderedText = null;
		o.Clear();
	});

	public byte Alpha;

	public TextObject DLeft;

	public TextObject DRight;

	public ushort Hue;

	public bool IsDestroyed;

	public bool IsTextGump;

	public bool IsTransparent;

	public GameObject Owner;

	public RenderedText RenderedText;

	public long Time;

	public long SecondTime;

	public MessageType Type;

	public int X;

	public int Y;

	public int OffsetY;

	public static TextObject Create()
	{
		return _queue.GetOne();
	}

	public virtual void Destroy()
	{
		if (!IsDestroyed)
		{
			UnlinkD();
			RealScreenPosition = Point.Zero;
			IsDestroyed = true;
			RenderedText?.Destroy();
			RenderedText = null;
			Owner = null;
			_queue.ReturnOne(this);
		}
	}

	public void UnlinkD()
	{
		if (DRight != null)
		{
			DRight.DLeft = DLeft;
		}
		if (DLeft != null)
		{
			DLeft.DRight = DRight;
		}
		DRight = null;
		DLeft = null;
	}

	public void ToTopD()
	{
		TextObject textObject = this;
		while (textObject != null && textObject.DLeft != null)
		{
			textObject = textObject.DLeft;
		}
		((TextRenderer)textObject).MoveToTop(this);
	}
}

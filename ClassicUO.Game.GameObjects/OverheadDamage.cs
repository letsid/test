using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Utility.Collections;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.GameObjects;

internal class OverheadDamage
{
	private const int DAMAGE_Y_MOVING_TIME = 25;

	private readonly Deque<TextObject> _messages;

	private Rectangle _rectangle;

	public GameObject Parent { get; private set; }

	public bool IsDestroyed { get; private set; }

	public bool IsEmpty => _messages.Count == 0;

	public OverheadDamage(GameObject parent)
	{
		Parent = parent;
		_messages = new Deque<TextObject>();
	}

	public void SetParent(GameObject parent)
	{
		Parent = parent;
	}

	public void Add(int damage)
	{
		TextObject textObject = TextObject.Create();
		textObject.RenderedText = RenderedText.Create(damage.ToString(), (ushort)((Parent == World.Player) ? 52u : 33u), 3, isunicode: false, FontStyle.None, TEXT_ALIGN_TYPE.TS_LEFT, 0, 30);
		textObject.Time = Time.Ticks + 1500;
		_messages.AddToFront(textObject);
		if (_messages.Count > 10)
		{
			_messages.RemoveFromBack()?.Destroy();
		}
	}

	public void Update()
	{
		if (IsDestroyed)
		{
			return;
		}
		_rectangle.Width = 0;
		for (int i = 0; i < _messages.Count; i++)
		{
			TextObject textObject = _messages[i];
			float num = textObject.Time - Time.Ticks;
			if (textObject.SecondTime < Time.Ticks)
			{
				textObject.OffsetY++;
				textObject.SecondTime = Time.Ticks + 25;
			}
			if (num <= 0f)
			{
				_rectangle.Height -= textObject.RenderedText?.Height ?? 0;
				textObject.Destroy();
				_messages.RemoveAt(i--);
			}
			else if (textObject.RenderedText != null && _rectangle.Width < textObject.RenderedText.Width)
			{
				_rectangle.Width = textObject.RenderedText.Width;
			}
		}
	}

	public void Draw(UltimaBatcher2D batcher)
	{
		if (IsDestroyed || _messages.Count == 0)
		{
			return;
		}
		int num = 0;
		Point point = default(Point);
		if (Parent != null)
		{
			point.X += Parent.RealScreenPosition.X;
			point.Y += Parent.RealScreenPosition.Y;
			_rectangle.X = Parent.RealScreenPosition.X;
			_rectangle.Y = Parent.RealScreenPosition.Y;
			Rectangle bounds;
			if (Parent is Mobile mobile)
			{
				if (mobile.IsGargoyle && mobile.IsFlying)
				{
					num += 22;
				}
				else if (!mobile.IsMounted)
				{
					num = -22;
				}
				AnimationsLoader.Instance.GetAnimationDimensions(mobile.AnimIndex, mobile.GetGraphicForAnimation(), 0, 0, mobile.IsMounted, 0, out var _, out var centerY, out var _, out var height);
				point.X += (int)mobile.CumulativeOffset.X + 22;
				point.Y += (int)(mobile.CumulativeOffset.Y - mobile.CumulativeOffset.Z - (float)(height + centerY + 8));
			}
			else if (ArtLoader.Instance.GetStaticTexture(Parent.Graphic, out bounds) != null)
			{
				point.X += 22;
				int num2 = bounds.Height >> 1;
				if (Parent is Item item)
				{
					if (item.IsCorpse)
					{
						num = -22;
					}
				}
				else if (Parent is Static || Parent is Multi)
				{
					num = -44;
				}
				point.Y -= num2;
			}
		}
		point = Client.Game.Scene.Camera.WorldToScreen(point);
		foreach (TextObject message in _messages)
		{
			if (!message.IsDestroyed && message.RenderedText != null && !message.RenderedText.IsDestroyed)
			{
				message.X = point.X - (message.RenderedText.Width >> 1);
				message.Y = point.Y - num - message.RenderedText.Height - message.OffsetY;
				message.RenderedText.Draw(batcher, message.X, message.Y, (float)(int)message.Alpha / 255f, 0);
				num += message.RenderedText.Height;
			}
		}
	}

	public void Destroy()
	{
		if (IsDestroyed)
		{
			return;
		}
		IsDestroyed = true;
		foreach (TextObject message in _messages)
		{
			message.Destroy();
		}
		_messages.Clear();
	}
}

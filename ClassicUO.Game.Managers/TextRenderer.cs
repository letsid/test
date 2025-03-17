using System.Collections.Generic;
using ClassicUO.Configuration;
using ClassicUO.Game.GameObjects;
using ClassicUO.Input;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.Managers;

internal class TextRenderer : TextObject
{
	private readonly List<Rectangle> _bounds = new List<Rectangle>();

	protected TextObject FirstNode;

	protected TextObject DrawPointer;

	public TextRenderer()
	{
		FirstNode = this;
	}

	public override void Destroy()
	{
	}

	public virtual void Update(double totalTime, double frameTime)
	{
		ProcessWorldText(doit: false);
	}

	public void Select(int startX, int startY, int renderIndex, bool isGump = false)
	{
		int x = Mouse.Position.X;
		int y = Mouse.Position.Y;
		for (TextObject textObject = DrawPointer; textObject != null; textObject = textObject.DLeft)
		{
			if (textObject.RenderedText != null && !textObject.RenderedText.IsDestroyed && textObject.RenderedText.Texture != null && (textObject.Time < ClassicUO.Time.Ticks || (textObject.Owner != null && textObject.Owner.UseInRender == renderIndex)) && textObject.RenderedText.PixelCheck(x - startX - textObject.RealScreenPosition.X, y - startY - textObject.RealScreenPosition.Y))
			{
				SelectedObject.LastObject = textObject;
			}
		}
		if (!(SelectedObject.LastObject is TextObject textObject2))
		{
			return;
		}
		if (isGump)
		{
			if (textObject2.IsTextGump)
			{
				textObject2.ToTopD();
			}
		}
		else
		{
			MoveToTop(textObject2);
		}
	}

	public virtual void Draw(UltimaBatcher2D batcher, int startX, int startY, int renderIndex, bool isGump = false)
	{
		ProcessWorldText(doit: false);
		int x = Mouse.Position.X;
		int y = Mouse.Position.Y;
		BaseGameObject lastObject = SelectedObject.LastObject;
		for (TextObject textObject = DrawPointer; textObject != null; textObject = textObject.DLeft)
		{
			if (!textObject.IsDestroyed && textObject.RenderedText != null && !textObject.RenderedText.IsDestroyed && textObject.RenderedText.Texture != null && textObject.Time >= ClassicUO.Time.Ticks && (textObject.Owner.UseInRender == renderIndex || isGump))
			{
				ushort hue = 0;
				float alpha = (float)(int)textObject.Alpha / 255f;
				if (textObject.IsTransparent && textObject.Alpha == byte.MaxValue)
				{
					alpha = 0.49803922f;
				}
				int num = textObject.RealScreenPosition.X;
				int num2 = textObject.RealScreenPosition.Y;
				if (textObject.RenderedText.PixelCheck(x - num - startX, y - num2 - startY))
				{
					if (isGump)
					{
						SelectedObject.LastObject = textObject;
					}
					else
					{
						SelectedObject.Object = textObject;
					}
				}
				if (!isGump)
				{
					if (textObject.Owner is Entity && lastObject == textObject)
					{
						hue = 53;
					}
				}
				else
				{
					num += startX;
					num2 += startY;
				}
				textObject.RenderedText.Draw(batcher, num, num2, alpha, hue);
			}
		}
	}

	public void MoveToTop(TextObject obj)
	{
		if (obj != null)
		{
			obj.UnlinkD();
			TextObject dRight = FirstNode.DRight;
			FirstNode.DRight = obj;
			obj.DLeft = FirstNode;
			obj.DRight = dRight;
			if (dRight != null)
			{
				dRight.DLeft = obj;
			}
		}
	}

	public void ProcessWorldText(bool doit)
	{
		if (doit && _bounds.Count != 0)
		{
			_bounds.Clear();
		}
		DrawPointer = FirstNode;
		while (DrawPointer != null)
		{
			if (doit)
			{
				TextObject drawPointer = DrawPointer;
				if (drawPointer.Time >= ClassicUO.Time.Ticks && drawPointer.RenderedText != null && !drawPointer.RenderedText.IsDestroyed && drawPointer.Owner != null)
				{
					drawPointer.IsTransparent = Collides(drawPointer);
					CalculateAlpha(drawPointer);
				}
			}
			if (DrawPointer.DRight != null)
			{
				DrawPointer = DrawPointer.DRight;
				continue;
			}
			break;
		}
	}

	private void CalculateAlpha(TextObject msg)
	{
		if (ProfileManager.CurrentProfile == null || !ProfileManager.CurrentProfile.TextFading)
		{
			return;
		}
		int num = (int)(msg.Time - ClassicUO.Time.Ticks);
		if (num >= 0 && num <= 1000)
		{
			num /= 10;
			if (num > 100)
			{
				num = 100;
			}
			else if (num < 1)
			{
				num = 0;
			}
			num = 255 * num / 100;
			if (!msg.IsTransparent || num <= 127)
			{
				msg.Alpha = (byte)num;
			}
			msg.IsTransparent = true;
		}
	}

	private bool Collides(TextObject msg)
	{
		bool result = false;
		Rectangle rectangle = default(Rectangle);
		rectangle.X = msg.RealScreenPosition.X;
		rectangle.Y = msg.RealScreenPosition.Y;
		rectangle.Width = msg.RenderedText.Width;
		rectangle.Height = msg.RenderedText.Height;
		Rectangle rectangle2 = rectangle;
		for (int i = 0; i < _bounds.Count; i++)
		{
			if (_bounds[i].Intersects(rectangle2))
			{
				result = true;
				break;
			}
		}
		_bounds.Add(rectangle2);
		return result;
	}

	public void AddMessage(TextObject obj)
	{
		if (obj == null)
		{
			return;
		}
		obj.UnlinkD();
		TextObject firstNode = FirstNode;
		if (firstNode != null)
		{
			if (firstNode.DRight != null)
			{
				TextObject dRight = firstNode.DRight;
				firstNode.DRight = obj;
				obj.DLeft = firstNode;
				obj.DRight = dRight;
				dRight.DLeft = obj;
			}
			else
			{
				firstNode.DRight = obj;
				obj.DLeft = firstNode;
				obj.DRight = null;
			}
		}
	}

	public new virtual void Clear()
	{
		if (FirstNode != null)
		{
			TextObject textObject = FirstNode;
			while (textObject?.DLeft != null)
			{
				textObject = textObject.DLeft;
			}
			while (textObject != null)
			{
				TextObject dRight = textObject.DRight;
				textObject.Destroy();
				textObject.Clear();
				textObject = dRight;
			}
		}
		if (DrawPointer != null)
		{
			TextObject textObject2 = DrawPointer;
			while (textObject2?.DLeft != null)
			{
				textObject2 = textObject2.DLeft;
			}
			while (textObject2 != null)
			{
				TextObject dRight2 = textObject2.DRight;
				textObject2.Destroy();
				textObject2.Clear();
				textObject2 = dRight2;
			}
		}
		FirstNode = this;
		FirstNode.DLeft = null;
		FirstNode.DRight = null;
		DrawPointer = null;
	}
}

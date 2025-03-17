using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Renderer;

internal static class ScissorStack
{
	private static readonly Stack<Rectangle> _scissors = new Stack<Rectangle>();

	public static bool HasScissors => _scissors.Count - 1 > 0;

	public static bool PushScissors(GraphicsDevice device, Rectangle scissor)
	{
		if (_scissors.Count > 0)
		{
			Rectangle rectangle = _scissors.Peek();
			int num = Math.Max(rectangle.X, scissor.X);
			int num2 = Math.Min(rectangle.X + rectangle.Width, scissor.X + scissor.Width);
			if (num2 - num < 1)
			{
				return false;
			}
			int num3 = Math.Max(rectangle.Y, scissor.Y);
			int num4 = Math.Min(rectangle.Y + rectangle.Height, scissor.Y + scissor.Height);
			if (num4 - num3 < 1)
			{
				return false;
			}
			scissor.X = num;
			scissor.Y = num3;
			scissor.Width = num2 - num;
			scissor.Height = Math.Max(1, num4 - num3);
		}
		_scissors.Push(scissor);
		device.ScissorRectangle = scissor;
		return true;
	}

	public static Rectangle PopScissors(GraphicsDevice device)
	{
		Rectangle result = _scissors.Pop();
		if (_scissors.Count == 0)
		{
			device.ScissorRectangle = device.Viewport.Bounds;
			return result;
		}
		device.ScissorRectangle = _scissors.Peek();
		return result;
	}

	public static Rectangle CalculateScissors(Matrix batchTransform, int sx, int sy, int sw, int sh)
	{
		Vector2 position = new Vector2(sx, sy);
		Vector2.Transform(ref position, ref batchTransform, out position);
		Rectangle rectangle = default(Rectangle);
		rectangle.X = (int)position.X;
		rectangle.Y = (int)position.Y;
		Rectangle result = rectangle;
		position.X = sx + sw;
		position.Y = sy + sh;
		Vector2.Transform(ref position, ref batchTransform, out position);
		result.Width = (int)position.X - result.X;
		result.Height = (int)position.Y - result.Y;
		return result;
	}
}

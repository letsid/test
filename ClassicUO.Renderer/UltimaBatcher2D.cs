using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ClassicUO.Renderer.Effects;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Renderer;

internal sealed class UltimaBatcher2D : IDisposable
{
	public struct YOffsets
	{
		public int Top;

		public int Right;

		public int Left;

		public int Bottom;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	private struct PositionNormalTextureColor4 : IVertexType
	{
		public Vector3 Position0;

		public Vector3 Normal0;

		public Vector3 TextureCoordinate0;

		public Vector3 Hue0;

		public Vector3 Position1;

		public Vector3 Normal1;

		public Vector3 TextureCoordinate1;

		public Vector3 Hue1;

		public Vector3 Position2;

		public Vector3 Normal2;

		public Vector3 TextureCoordinate2;

		public Vector3 Hue2;

		public Vector3 Position3;

		public Vector3 Normal3;

		public Vector3 TextureCoordinate3;

		public Vector3 Hue3;

		private static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0), new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0), new VertexElement(24, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0), new VertexElement(36, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 1));

		public const int SIZE_IN_BYTES = 192;

		VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
	}

	private static readonly float[] _cornerOffsetX = new float[4] { 0f, 1f, 0f, 1f };

	private static readonly float[] _cornerOffsetY = new float[4] { 0f, 0f, 1f, 1f };

	private const int MAX_SPRITES = 2048;

	private const int MAX_VERTICES = 8192;

	private const int MAX_INDICES = 12288;

	private BlendState _blendState;

	private int _currentBufferPosition;

	private Effect _customEffect;

	private readonly IndexBuffer _indexBuffer;

	private int _numSprites;

	private Matrix _projectionMatrix = new Matrix(0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 1f, 0f, -1f, 1f, 0f, 1f);

	private readonly RasterizerState _rasterizerState;

	private SamplerState _sampler;

	private bool _started;

	private DepthStencilState _stencil;

	private Matrix _transformMatrix;

	private readonly DynamicVertexBuffer _vertexBuffer;

	private readonly BasicUOEffect _basicUOEffect;

	private Texture2D[] _textureInfo;

	private PositionNormalTextureColor4[] _vertexInfo;

	public int TextureSwitches;

	public int FlushesDone;

	public Matrix TransformMatrix => _transformMatrix;

	public DepthStencilState Stencil { get; } = new DepthStencilState
	{
		StencilEnable = false,
		DepthBufferEnable = false,
		StencilFunction = CompareFunction.NotEqual,
		ReferenceStencil = -1,
		StencilMask = -1,
		StencilFail = StencilOperation.Keep,
		StencilDepthBufferFail = StencilOperation.Keep,
		StencilPass = StencilOperation.Keep
	};

	public GraphicsDevice GraphicsDevice { get; }

	public UltimaBatcher2D(GraphicsDevice device)
	{
		GraphicsDevice = device;
		_textureInfo = new Texture2D[2048];
		_vertexInfo = new PositionNormalTextureColor4[2048];
		_vertexBuffer = new DynamicVertexBuffer(GraphicsDevice, typeof(PositionNormalTextureColor4), 8192, BufferUsage.WriteOnly);
		_indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, 12288, BufferUsage.WriteOnly);
		_indexBuffer.SetData(GenerateIndexArray());
		_blendState = BlendState.AlphaBlend;
		_sampler = SamplerState.PointClamp;
		_rasterizerState = new RasterizerState
		{
			CullMode = CullMode.CullCounterClockwiseFace,
			FillMode = FillMode.Solid,
			DepthBias = 0f,
			MultiSampleAntiAlias = true,
			ScissorTestEnable = true,
			SlopeScaleDepthBias = 0f
		};
		_stencil = Stencil;
		_basicUOEffect = new BasicUOEffect(device);
	}

	public void Dispose()
	{
		_vertexInfo = null;
		_basicUOEffect?.Dispose();
		_vertexBuffer.Dispose();
		_indexBuffer.Dispose();
	}

	public void SetBrightlight(float f)
	{
		_basicUOEffect.Brighlight.SetValue(f);
	}

	public void DrawString(SpriteFont spriteFont, string text, int x, int y, Vector3 color)
	{
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		EnsureSize();
		Texture2D texture = spriteFont.Texture;
		List<Rectangle> glyphData = spriteFont.GlyphData;
		List<Rectangle> croppingData = spriteFont.CroppingData;
		List<Vector3> kerning = spriteFont.Kerning;
		List<char> characterMap = spriteFont.CharacterMap;
		Vector2 zero = Vector2.Zero;
		bool flag = true;
		Vector2 zero2 = Vector2.Zero;
		float num = 1f;
		float num2 = 1f;
		foreach (char c in text)
		{
			switch (c)
			{
			case '\n':
				zero.X = 0f;
				zero.Y += spriteFont.LineSpacing;
				flag = true;
				continue;
			case '\r':
				continue;
			}
			int num3 = characterMap.IndexOf(c);
			if (num3 == -1)
			{
				num3 = (spriteFont.DefaultCharacter.HasValue ? characterMap.IndexOf(spriteFont.DefaultCharacter.Value) : characterMap.IndexOf('?'));
			}
			Vector3 vector = kerning[num3];
			if (flag)
			{
				zero.X += Math.Abs(vector.X);
				flag = false;
			}
			else
			{
				zero.X += spriteFont.Spacing + vector.X;
			}
			Rectangle rectangle = croppingData[num3];
			Rectangle value = glyphData[num3];
			float num4 = zero2.X + (zero.X + (float)rectangle.X) * num;
			float num5 = zero2.Y + (zero.Y + (float)rectangle.Y) * num2;
			Draw(texture, new Vector2(x + (int)Math.Round(num4), y + (int)Math.Round(num5)), value, color);
			zero.X += vector.Y + vector.Z;
		}
	}

	public void DrawStretchedLand(Texture2D texture, Vector2 position, Rectangle sourceRect, ref YOffsets yOffsets, ref Vector3 normalTop, ref Vector3 normalRight, ref Vector3 normalLeft, ref Vector3 normalBottom, Vector3 hue, float depth)
	{
		EnsureSize();
		ref PositionNormalTextureColor4 reference = ref _vertexInfo[_numSprites];
		float num = ((float)sourceRect.X + 0.5f) / (float)texture.Width;
		float num2 = ((float)sourceRect.Y + 0.5f) / (float)texture.Height;
		float num3 = ((float)sourceRect.Width - 1f) / (float)texture.Width;
		float num4 = ((float)sourceRect.Height - 1f) / (float)texture.Height;
		reference.TextureCoordinate0.X = _cornerOffsetX[0] * num3 + num;
		reference.TextureCoordinate0.Y = _cornerOffsetY[0] * num4 + num2;
		reference.TextureCoordinate1.X = _cornerOffsetX[1] * num3 + num;
		reference.TextureCoordinate1.Y = _cornerOffsetY[1] * num4 + num2;
		reference.TextureCoordinate2.X = _cornerOffsetX[2] * num3 + num;
		reference.TextureCoordinate2.Y = _cornerOffsetY[2] * num4 + num2;
		reference.TextureCoordinate3.X = _cornerOffsetX[3] * num3 + num;
		reference.TextureCoordinate3.Y = _cornerOffsetY[3] * num4 + num2;
		reference.TextureCoordinate0.Z = 0f;
		reference.TextureCoordinate1.Z = 0f;
		reference.TextureCoordinate2.Z = 0f;
		reference.TextureCoordinate3.Z = 0f;
		reference.Normal0 = normalTop;
		reference.Normal1 = normalRight;
		reference.Normal2 = normalLeft;
		reference.Normal3 = normalBottom;
		reference.Position0.X = position.X + 22f;
		reference.Position0.Y = position.Y - (float)yOffsets.Top;
		reference.Position1.X = position.X + 44f;
		reference.Position1.Y = position.Y + (float)(22 - yOffsets.Right);
		reference.Position2.X = position.X;
		reference.Position2.Y = position.Y + (float)(22 - yOffsets.Left);
		reference.Position3.X = position.X + 22f;
		reference.Position3.Y = position.Y + (float)(44 - yOffsets.Bottom);
		reference.Position0.Z = depth;
		reference.Position1.Z = depth;
		reference.Position2.Z = depth;
		reference.Position3.Z = depth;
		reference.Hue0 = (reference.Hue1 = (reference.Hue2 = (reference.Hue3 = hue)));
		PushSprite(texture);
	}

	public void DrawShadow(Texture2D texture, Vector2 position, Rectangle sourceRect, bool flip, float depth)
	{
		float num = sourceRect.Width;
		float num2 = (float)sourceRect.Height * 0.5f;
		float num3 = position.Y + num2 - 10f;
		float num4 = num2 / num;
		EnsureSize();
		ref PositionNormalTextureColor4 reference = ref _vertexInfo[_numSprites];
		reference.Position0.X = position.X + num * num4;
		reference.Position0.Y = num3;
		reference.Position1.X = position.X + num * (num4 + 1f);
		reference.Position1.Y = num3;
		reference.Position2.X = position.X;
		reference.Position2.Y = num3 + num2;
		reference.Position3.X = position.X + num;
		reference.Position3.Y = num3 + num2;
		reference.Position0.Z = depth;
		reference.Position1.Z = depth;
		reference.Position2.Z = depth;
		reference.Position3.Z = depth;
		float num5 = ((float)sourceRect.X + 0.5f) / (float)texture.Width;
		float num6 = ((float)sourceRect.Y + 0.5f) / (float)texture.Height;
		float num7 = ((float)sourceRect.Width - 1f) / (float)texture.Width;
		float num8 = ((float)sourceRect.Height - 1f) / (float)texture.Height;
		byte b = (byte)((flip ? 1 : 0) & 3);
		reference.TextureCoordinate0.X = _cornerOffsetX[0 ^ b] * num7 + num5;
		reference.TextureCoordinate0.Y = _cornerOffsetY[0 ^ b] * num8 + num6;
		reference.TextureCoordinate1.X = _cornerOffsetX[1 ^ b] * num7 + num5;
		reference.TextureCoordinate1.Y = _cornerOffsetY[1 ^ b] * num8 + num6;
		reference.TextureCoordinate2.X = _cornerOffsetX[2 ^ b] * num7 + num5;
		reference.TextureCoordinate2.Y = _cornerOffsetY[2 ^ b] * num8 + num6;
		reference.TextureCoordinate3.X = _cornerOffsetX[3 ^ b] * num7 + num5;
		reference.TextureCoordinate3.Y = _cornerOffsetY[3 ^ b] * num8 + num6;
		reference.TextureCoordinate0.Z = 0f;
		reference.TextureCoordinate1.Z = 0f;
		reference.TextureCoordinate2.Z = 0f;
		reference.TextureCoordinate3.Z = 0f;
		reference.Normal0.X = 0f;
		reference.Normal0.Y = 0f;
		reference.Normal0.Z = 1f;
		reference.Normal1.X = 0f;
		reference.Normal1.Y = 0f;
		reference.Normal1.Z = 1f;
		reference.Normal2.X = 0f;
		reference.Normal2.Y = 0f;
		reference.Normal2.Z = 1f;
		reference.Normal3.X = 0f;
		reference.Normal3.Y = 0f;
		reference.Normal3.Z = 1f;
		reference.Hue0.Z = (reference.Hue1.Z = (reference.Hue2.Z = (reference.Hue3.Z = (reference.Hue0.X = (reference.Hue1.X = (reference.Hue2.X = (reference.Hue3.X = 0f)))))));
		reference.Hue0.Y = (reference.Hue1.Y = (reference.Hue2.Y = (reference.Hue3.Y = 8f)));
		PushSprite(texture);
	}

	public void DrawCharacterSitted(Texture2D texture, Vector2 position, Rectangle sourceRect, Vector3 mod, Vector3 hue, bool flip, float depth)
	{
		EnsureSize();
		float num = (float)sourceRect.Height * mod.X;
		float num2 = (float)sourceRect.Height * mod.Y;
		float num3 = (float)sourceRect.Height * mod.Z;
		float num4 = (flip ? (-8f) : 8f);
		float num5 = sourceRect.Width;
		float num6 = (float)sourceRect.Width + num4;
		if (mod.X != 0f)
		{
			EnsureSize();
			ref PositionNormalTextureColor4 reference = ref _vertexInfo[_numSprites];
			reference.Position0.X = position.X + num4;
			reference.Position0.Y = position.Y;
			reference.Position1.X = position.X + num6;
			reference.Position1.Y = position.Y;
			reference.Position2.X = position.X + num4;
			reference.Position2.Y = position.Y + num;
			reference.Position3.X = position.X + num6;
			reference.Position3.Y = position.Y + num;
			reference.Position0.Z = depth;
			reference.Position1.Z = depth;
			reference.Position2.Z = depth;
			reference.Position3.Z = depth;
			float num7 = ((float)sourceRect.X + 0.5f) / (float)texture.Width;
			float num8 = ((float)sourceRect.Y + 0.5f) / (float)texture.Height;
			float num9 = ((float)sourceRect.Width - 1f) / (float)texture.Width;
			float num10 = ((float)sourceRect.Height - 1f) / (float)texture.Height;
			byte b = (byte)((flip ? 1 : 0) & 3);
			reference.TextureCoordinate0.X = _cornerOffsetX[0 ^ b] * num9 + num7;
			reference.TextureCoordinate0.Y = _cornerOffsetY[0 ^ b] * num10 + num8;
			reference.TextureCoordinate1.X = _cornerOffsetX[1 ^ b] * num9 + num7;
			reference.TextureCoordinate1.Y = _cornerOffsetY[1 ^ b] * num10 + num8;
			reference.TextureCoordinate2.X = _cornerOffsetX[2 ^ b] * num9 + num7;
			reference.TextureCoordinate2.Y = _cornerOffsetY[2 ^ b] * num10 * mod.X + num8;
			reference.TextureCoordinate3.X = _cornerOffsetX[3 ^ b] * num9 + num7;
			reference.TextureCoordinate3.Y = _cornerOffsetY[3 ^ b] * num10 * mod.X + num8;
			reference.TextureCoordinate0.Z = 0f;
			reference.TextureCoordinate1.Z = 0f;
			reference.TextureCoordinate2.Z = 0f;
			reference.TextureCoordinate3.Z = 0f;
			reference.Normal0.X = 0f;
			reference.Normal0.Y = 0f;
			reference.Normal0.Z = 1f;
			reference.Normal1.X = 0f;
			reference.Normal1.Y = 0f;
			reference.Normal1.Z = 1f;
			reference.Normal2.X = 0f;
			reference.Normal2.Y = 0f;
			reference.Normal2.Z = 1f;
			reference.Normal3.X = 0f;
			reference.Normal3.Y = 0f;
			reference.Normal3.Z = 1f;
			reference.Hue0 = (reference.Hue1 = (reference.Hue2 = (reference.Hue3 = hue)));
			PushSprite(texture);
		}
		if (mod.Y != 0f)
		{
			EnsureSize();
			ref PositionNormalTextureColor4 reference2 = ref _vertexInfo[_numSprites];
			reference2.Position0.X = position.X + num4;
			reference2.Position0.Y = position.Y + num;
			reference2.Position1.X = position.X + num6;
			reference2.Position1.Y = position.Y + num;
			reference2.Position2.X = position.X;
			reference2.Position2.Y = position.Y + num2;
			reference2.Position3.X = position.X + num5;
			reference2.Position3.Y = position.Y + num2;
			reference2.Position0.Z = depth;
			reference2.Position1.Z = depth;
			reference2.Position2.Z = depth;
			reference2.Position3.Z = depth;
			float num11 = ((float)sourceRect.X + 0.5f) / (float)texture.Width;
			float num12 = ((float)sourceRect.Y + 0.5f + num) / (float)texture.Height;
			float num13 = ((float)sourceRect.Width - 1f) / (float)texture.Width;
			float num14 = ((float)sourceRect.Height - 1f - num) / (float)texture.Height;
			byte b2 = (byte)((flip ? 1 : 0) & 3);
			reference2.TextureCoordinate0.X = _cornerOffsetX[0 ^ b2] * num13 + num11;
			reference2.TextureCoordinate0.Y = _cornerOffsetY[0 ^ b2] * num14 + num12;
			reference2.TextureCoordinate1.X = _cornerOffsetX[1 ^ b2] * num13 + num11;
			reference2.TextureCoordinate1.Y = _cornerOffsetY[1 ^ b2] * num14 + num12;
			reference2.TextureCoordinate2.X = _cornerOffsetX[2 ^ b2] * num13 + num11;
			reference2.TextureCoordinate2.Y = _cornerOffsetY[2 ^ b2] * num14 * mod.Y + num12;
			reference2.TextureCoordinate3.X = _cornerOffsetX[3 ^ b2] * num13 + num11;
			reference2.TextureCoordinate3.Y = _cornerOffsetY[3 ^ b2] * num14 * mod.Y + num12;
			reference2.TextureCoordinate0.Z = 0f;
			reference2.TextureCoordinate1.Z = 0f;
			reference2.TextureCoordinate2.Z = 0f;
			reference2.TextureCoordinate3.Z = 0f;
			reference2.Normal0.X = 0f;
			reference2.Normal0.Y = 0f;
			reference2.Normal0.Z = 1f;
			reference2.Normal1.X = 0f;
			reference2.Normal1.Y = 0f;
			reference2.Normal1.Z = 1f;
			reference2.Normal2.X = 0f;
			reference2.Normal2.Y = 0f;
			reference2.Normal2.Z = 1f;
			reference2.Normal3.X = 0f;
			reference2.Normal3.Y = 0f;
			reference2.Normal3.Z = 1f;
			reference2.Hue0 = (reference2.Hue1 = (reference2.Hue2 = (reference2.Hue3 = hue)));
			PushSprite(texture);
		}
		if (mod.Z != 0f)
		{
			EnsureSize();
			ref PositionNormalTextureColor4 reference3 = ref _vertexInfo[_numSprites];
			reference3.Position0.X = position.X;
			reference3.Position0.Y = position.Y + num2;
			reference3.Position1.X = position.X + num5;
			reference3.Position1.Y = position.Y + num2;
			reference3.Position2.X = position.X;
			reference3.Position2.Y = position.Y + num3;
			reference3.Position3.X = position.X + num5;
			reference3.Position3.Y = position.Y + num3;
			reference3.Position0.Z = depth;
			reference3.Position1.Z = depth;
			reference3.Position2.Z = depth;
			reference3.Position3.Z = depth;
			float num15 = ((float)sourceRect.X + 0.5f) / (float)texture.Width;
			float num16 = ((float)sourceRect.Y + 0.5f + num2) / (float)texture.Height;
			float num17 = ((float)sourceRect.Width - 1f) / (float)texture.Width;
			float num18 = ((float)sourceRect.Height - 1f - num2) / (float)texture.Height;
			byte b3 = (byte)((flip ? 1 : 0) & 3);
			reference3.TextureCoordinate0.X = _cornerOffsetX[0 ^ b3] * num17 + num15;
			reference3.TextureCoordinate0.Y = _cornerOffsetY[0 ^ b3] * num18 + num16;
			reference3.TextureCoordinate1.X = _cornerOffsetX[1 ^ b3] * num17 + num15;
			reference3.TextureCoordinate1.Y = _cornerOffsetY[1 ^ b3] * num18 + num16;
			reference3.TextureCoordinate2.X = _cornerOffsetX[2 ^ b3] * num17 + num15;
			reference3.TextureCoordinate2.Y = _cornerOffsetY[2 ^ b3] * num18 * mod.Z + num16;
			reference3.TextureCoordinate3.X = _cornerOffsetX[3 ^ b3] * num17 + num15;
			reference3.TextureCoordinate3.Y = _cornerOffsetY[3 ^ b3] * num18 * mod.Z + num16;
			reference3.TextureCoordinate0.Z = 0f;
			reference3.TextureCoordinate1.Z = 0f;
			reference3.TextureCoordinate2.Z = 0f;
			reference3.TextureCoordinate3.Z = 0f;
			reference3.Normal0.X = 0f;
			reference3.Normal0.Y = 0f;
			reference3.Normal0.Z = 1f;
			reference3.Normal1.X = 0f;
			reference3.Normal1.Y = 0f;
			reference3.Normal1.Z = 1f;
			reference3.Normal2.X = 0f;
			reference3.Normal2.Y = 0f;
			reference3.Normal2.Z = 1f;
			reference3.Normal3.X = 0f;
			reference3.Normal3.Y = 0f;
			reference3.Normal3.Z = 1f;
			reference3.Hue0 = (reference3.Hue1 = (reference3.Hue2 = (reference3.Hue3 = hue)));
			PushSprite(texture);
		}
	}

	public void DrawTiled(Texture2D texture, Rectangle destinationRectangle, Rectangle sourceRectangle, Vector3 hue)
	{
		int num = destinationRectangle.Height;
		Rectangle value = sourceRectangle;
		Vector2 position = new Vector2(destinationRectangle.X, destinationRectangle.Y);
		while (num > 0)
		{
			position.X = destinationRectangle.X;
			int num2 = destinationRectangle.Width;
			value.Height = Math.Min(num, sourceRectangle.Height);
			while (num2 > 0)
			{
				value.Width = Math.Min(num2, sourceRectangle.Width);
				Draw(texture, position, value, hue);
				num2 -= sourceRectangle.Width;
				position.X += sourceRectangle.Width;
			}
			num -= sourceRectangle.Height;
			position.Y += sourceRectangle.Height;
		}
	}

	public bool DrawRectangle(Texture2D texture, int x, int y, int width, int height, Vector3 hue, float depth = 0f)
	{
		Rectangle destinationRectangle = new Rectangle(x, y, width, 1);
		Draw(texture, destinationRectangle, null, hue, 0f, Vector2.Zero, SpriteEffects.None, depth);
		destinationRectangle.X += width;
		destinationRectangle.Width = 1;
		destinationRectangle.Height += height;
		Draw(texture, destinationRectangle, null, hue, 0f, Vector2.Zero, SpriteEffects.None, depth);
		destinationRectangle.X = x;
		destinationRectangle.Y = y + height;
		destinationRectangle.Width = width;
		destinationRectangle.Height = 1;
		Draw(texture, destinationRectangle, null, hue, 0f, Vector2.Zero, SpriteEffects.None, depth);
		destinationRectangle.X = x;
		destinationRectangle.Y = y;
		destinationRectangle.Width = 1;
		destinationRectangle.Height = height;
		Draw(texture, destinationRectangle, null, hue, 0f, Vector2.Zero, SpriteEffects.None, depth);
		return true;
	}

	public void DrawLine(Texture2D texture, Vector2 start, Vector2 end, Vector3 color, float stroke)
	{
		float rotation = ClassicUO.Utility.MathHelper.AngleBetweenVectors(start, end);
		Vector2.Distance(ref start, ref end, out var result);
		Draw(texture, start, texture.Bounds, color, rotation, Vector2.Zero, new Vector2(result, stroke), SpriteEffects.None, 0f);
	}

	public void Draw(Texture2D texture, Vector2 position, Vector3 color)
	{
		AddSprite(texture, 0f, 0f, 1f, 1f, position.X, position.Y, texture.Width, texture.Height, color, 0f, 0f, 0f, 1f, 0f, 0);
	}

	public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Vector3 color)
	{
		float sourceX;
		float sourceY;
		float sourceW;
		float sourceH;
		float destinationW;
		float destinationH;
		if (sourceRectangle.HasValue)
		{
			sourceX = (float)sourceRectangle.Value.X / (float)texture.Width;
			sourceY = (float)sourceRectangle.Value.Y / (float)texture.Height;
			sourceW = (float)sourceRectangle.Value.Width / (float)texture.Width;
			sourceH = (float)sourceRectangle.Value.Height / (float)texture.Height;
			destinationW = sourceRectangle.Value.Width;
			destinationH = sourceRectangle.Value.Height;
		}
		else
		{
			sourceX = 0f;
			sourceY = 0f;
			sourceW = 1f;
			sourceH = 1f;
			destinationW = texture.Width;
			destinationH = texture.Height;
		}
		AddSprite(texture, sourceX, sourceY, sourceW, sourceH, position.X, position.Y, destinationW, destinationH, color, 0f, 0f, 0f, 1f, 0f, 0);
	}

	public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Vector3 color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
	{
		float num = scale;
		float num2 = scale;
		float sourceX;
		float sourceY;
		float num3;
		float num4;
		if (sourceRectangle.HasValue)
		{
			sourceX = (float)sourceRectangle.Value.X / (float)texture.Width;
			sourceY = (float)sourceRectangle.Value.Y / (float)texture.Height;
			num3 = (float)Math.Sign(sourceRectangle.Value.Width) * Math.Max(Math.Abs(sourceRectangle.Value.Width), ClassicUO.Utility.MathHelper.MachineEpsilonFloat) / (float)texture.Width;
			num4 = (float)Math.Sign(sourceRectangle.Value.Height) * Math.Max(Math.Abs(sourceRectangle.Value.Height), ClassicUO.Utility.MathHelper.MachineEpsilonFloat) / (float)texture.Height;
			num *= (float)sourceRectangle.Value.Width;
			num2 *= (float)sourceRectangle.Value.Height;
		}
		else
		{
			sourceX = 0f;
			sourceY = 0f;
			num3 = 1f;
			num4 = 1f;
			num *= (float)texture.Width;
			num2 *= (float)texture.Height;
		}
		AddSprite(texture, sourceX, sourceY, num3, num4, position.X, position.Y, num, num2, color, origin.X / num3 / (float)texture.Width, origin.Y / num4 / (float)texture.Height, (float)Math.Sin(rotation), (float)Math.Cos(rotation), layerDepth, (byte)(effects & (SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically)));
	}

	public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Vector3 color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
	{
		float sourceX;
		float sourceY;
		float num;
		float num2;
		if (sourceRectangle.HasValue)
		{
			sourceX = (float)sourceRectangle.Value.X / (float)texture.Width;
			sourceY = (float)sourceRectangle.Value.Y / (float)texture.Height;
			num = (float)Math.Sign(sourceRectangle.Value.Width) * Math.Max(Math.Abs(sourceRectangle.Value.Width), ClassicUO.Utility.MathHelper.MachineEpsilonFloat) / (float)texture.Width;
			num2 = (float)Math.Sign(sourceRectangle.Value.Height) * Math.Max(Math.Abs(sourceRectangle.Value.Height), ClassicUO.Utility.MathHelper.MachineEpsilonFloat) / (float)texture.Height;
			scale.X *= sourceRectangle.Value.Width;
			scale.Y *= sourceRectangle.Value.Height;
		}
		else
		{
			sourceX = 0f;
			sourceY = 0f;
			num = 1f;
			num2 = 1f;
			scale.X *= texture.Width;
			scale.Y *= texture.Height;
		}
		AddSprite(texture, sourceX, sourceY, num, num2, position.X, position.Y, scale.X, scale.Y, color, origin.X / num / (float)texture.Width, origin.Y / num2 / (float)texture.Height, (float)Math.Sin(rotation), (float)Math.Cos(rotation), layerDepth, (byte)(effects & (SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically)));
	}

	public void Draw(Texture2D texture, Rectangle destinationRectangle, Vector3 color)
	{
		AddSprite(texture, 0f, 0f, 1f, 1f, destinationRectangle.X, destinationRectangle.Y, destinationRectangle.Width, destinationRectangle.Height, color, 0f, 0f, 0f, 1f, 0f, 0);
	}

	public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Vector3 color)
	{
		float sourceX;
		float sourceY;
		float sourceW;
		float sourceH;
		if (sourceRectangle.HasValue)
		{
			sourceX = (float)sourceRectangle.Value.X / (float)texture.Width;
			sourceY = (float)sourceRectangle.Value.Y / (float)texture.Height;
			sourceW = (float)sourceRectangle.Value.Width / (float)texture.Width;
			sourceH = (float)sourceRectangle.Value.Height / (float)texture.Height;
		}
		else
		{
			sourceX = 0f;
			sourceY = 0f;
			sourceW = 1f;
			sourceH = 1f;
		}
		AddSprite(texture, sourceX, sourceY, sourceW, sourceH, destinationRectangle.X, destinationRectangle.Y, destinationRectangle.Width, destinationRectangle.Height, color, 0f, 0f, 0f, 1f, 0f, 0);
	}

	public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Vector3 color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth)
	{
		float sourceX;
		float sourceY;
		float num;
		float num2;
		if (sourceRectangle.HasValue)
		{
			sourceX = (float)sourceRectangle.Value.X / (float)texture.Width;
			sourceY = (float)sourceRectangle.Value.Y / (float)texture.Height;
			num = (float)Math.Sign(sourceRectangle.Value.Width) * Math.Max(Math.Abs(sourceRectangle.Value.Width), ClassicUO.Utility.MathHelper.MachineEpsilonFloat) / (float)texture.Width;
			num2 = (float)Math.Sign(sourceRectangle.Value.Height) * Math.Max(Math.Abs(sourceRectangle.Value.Height), ClassicUO.Utility.MathHelper.MachineEpsilonFloat) / (float)texture.Height;
		}
		else
		{
			sourceX = 0f;
			sourceY = 0f;
			num = 1f;
			num2 = 1f;
		}
		AddSprite(texture, sourceX, sourceY, num, num2, destinationRectangle.X, destinationRectangle.Y, destinationRectangle.Width, destinationRectangle.Height, color, origin.X / num / (float)texture.Width, origin.Y / num2 / (float)texture.Height, (float)Math.Sin(rotation), (float)Math.Cos(rotation), layerDepth, (byte)(effects & (SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically)));
	}

	private void AddSprite(Texture2D texture, float sourceX, float sourceY, float sourceW, float sourceH, float destinationX, float destinationY, float destinationW, float destinationH, Vector3 color, float originX, float originY, float rotationSin, float rotationCos, float depth, byte effects)
	{
		EnsureSize();
		SetVertex(ref _vertexInfo[_numSprites], sourceX, sourceY, sourceW, sourceH, destinationX, destinationY, destinationW, destinationH, color, originX, originY, rotationSin, rotationCos, depth, effects);
		_textureInfo[_numSprites] = texture;
		_numSprites++;
	}

	public void Begin()
	{
		Begin(null, Matrix.Identity);
	}

	public void Begin(Effect effect)
	{
		Begin(effect, Matrix.Identity);
	}

	public void Begin(Effect customEffect, Matrix transform_matrix)
	{
		_started = true;
		TextureSwitches = 0;
		FlushesDone = 0;
		_customEffect = customEffect;
		_transformMatrix = transform_matrix;
	}

	public void End()
	{
		Flush();
		_started = false;
		_customEffect = null;
	}

	private void SetVertex(ref PositionNormalTextureColor4 sprite, float sourceX, float sourceY, float sourceW, float sourceH, float destinationX, float destinationY, float destinationW, float destinationH, Vector3 color, float originX, float originY, float rotationSin, float rotationCos, float depth, byte effects)
	{
		float num = (0f - originX) * destinationW;
		float num2 = (0f - originY) * destinationH;
		sprite.Position0.X = (0f - rotationSin) * num2 + rotationCos * num + destinationX;
		sprite.Position0.Y = rotationCos * num2 + rotationSin * num + destinationY;
		num = (1f - originX) * destinationW;
		num2 = (0f - originY) * destinationH;
		sprite.Position1.X = (0f - rotationSin) * num2 + rotationCos * num + destinationX;
		sprite.Position1.Y = rotationCos * num2 + rotationSin * num + destinationY;
		num = (0f - originX) * destinationW;
		num2 = (1f - originY) * destinationH;
		sprite.Position2.X = (0f - rotationSin) * num2 + rotationCos * num + destinationX;
		sprite.Position2.Y = rotationCos * num2 + rotationSin * num + destinationY;
		num = (1f - originX) * destinationW;
		num2 = (1f - originY) * destinationH;
		sprite.Position3.X = (0f - rotationSin) * num2 + rotationCos * num + destinationX;
		sprite.Position3.Y = rotationCos * num2 + rotationSin * num + destinationY;
		sprite.TextureCoordinate0.X = _cornerOffsetX[0 ^ effects] * sourceW + sourceX;
		sprite.TextureCoordinate0.Y = _cornerOffsetY[0 ^ effects] * sourceH + sourceY;
		sprite.TextureCoordinate1.X = _cornerOffsetX[1 ^ effects] * sourceW + sourceX;
		sprite.TextureCoordinate1.Y = _cornerOffsetY[1 ^ effects] * sourceH + sourceY;
		sprite.TextureCoordinate2.X = _cornerOffsetX[2 ^ effects] * sourceW + sourceX;
		sprite.TextureCoordinate2.Y = _cornerOffsetY[2 ^ effects] * sourceH + sourceY;
		sprite.TextureCoordinate3.X = _cornerOffsetX[3 ^ effects] * sourceW + sourceX;
		sprite.TextureCoordinate3.Y = _cornerOffsetY[3 ^ effects] * sourceH + sourceY;
		sprite.TextureCoordinate0.Z = 0f;
		sprite.TextureCoordinate1.Z = 0f;
		sprite.TextureCoordinate2.Z = 0f;
		sprite.TextureCoordinate3.Z = 0f;
		sprite.Position0.Z = depth;
		sprite.Position1.Z = depth;
		sprite.Position2.Z = depth;
		sprite.Position3.Z = depth;
		sprite.Hue0 = color;
		sprite.Hue1 = color;
		sprite.Hue2 = color;
		sprite.Hue3 = color;
		sprite.Normal0.X = 0f;
		sprite.Normal0.Y = 0f;
		sprite.Normal0.Z = 1f;
		sprite.Normal1.X = 0f;
		sprite.Normal1.Y = 0f;
		sprite.Normal1.Z = 1f;
		sprite.Normal2.X = 0f;
		sprite.Normal2.Y = 0f;
		sprite.Normal2.Z = 1f;
		sprite.Normal3.X = 0f;
		sprite.Normal3.Y = 0f;
		sprite.Normal3.Z = 1f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void EnsureSize()
	{
		if (_numSprites >= _vertexInfo.Length)
		{
			int newSize = _vertexInfo.Length + 2048;
			Array.Resize(ref _vertexInfo, newSize);
			Array.Resize(ref _textureInfo, newSize);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool PushSprite(Texture2D texture)
	{
		if (texture == null || texture.IsDisposed)
		{
			return false;
		}
		EnsureSize();
		_textureInfo[_numSprites++] = texture;
		return true;
	}

	private void ApplyStates()
	{
		GraphicsDevice.BlendState = _blendState;
		GraphicsDevice.DepthStencilState = _stencil;
		GraphicsDevice.RasterizerState = _rasterizerState;
		GraphicsDevice.SamplerStates[0] = _sampler;
		GraphicsDevice.SamplerStates[1] = SamplerState.PointClamp;
		GraphicsDevice.SamplerStates[2] = SamplerState.PointClamp;
		GraphicsDevice.SamplerStates[3] = SamplerState.PointClamp;
		GraphicsDevice.Indices = _indexBuffer;
		GraphicsDevice.SetVertexBuffer(_vertexBuffer);
		_projectionMatrix.M11 = (float)(2.0 / (double)GraphicsDevice.Viewport.Width);
		_projectionMatrix.M22 = (float)(-2.0 / (double)GraphicsDevice.Viewport.Height);
		Matrix result = _projectionMatrix;
		Matrix.CreateOrthographicOffCenter(0f, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0f, -32768f, 32767f, out result);
		Matrix.Multiply(ref _transformMatrix, ref result, out result);
		_basicUOEffect.WorldMatrix.SetValue(Matrix.Identity);
		_basicUOEffect.Viewport.SetValue(new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height));
		_basicUOEffect.MatrixTransform.SetValue(result);
		_basicUOEffect.Pass.Apply();
	}

	private void Flush()
	{
		if (_numSprites == 0)
		{
			return;
		}
		ApplyStates();
		int num = 0;
		while (true)
		{
			FlushesDone++;
			int num2 = Math.Min(_numSprites, 2048);
			int num3 = UpdateVertexBuffer(num, num2);
			int num4 = 0;
			Texture2D texture2D = _textureInfo[num];
			for (int i = 1; i < num2; i++)
			{
				Texture2D texture2D2 = _textureInfo[num + i];
				if (texture2D2 != texture2D)
				{
					TextureSwitches++;
					InternalDraw(texture2D, num3 + num4, i - num4);
					texture2D = texture2D2;
					num4 = i;
				}
			}
			InternalDraw(texture2D, num3 + num4, num2 - num4);
			if (_numSprites <= 2048)
			{
				break;
			}
			_numSprites -= 2048;
			num += 2048;
		}
		_numSprites = 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void InternalDraw(Texture texture, int baseSprite, int batchSize)
	{
		GraphicsDevice.Textures[0] = texture;
		if (_customEffect != null)
		{
			foreach (EffectPass pass in _customEffect.CurrentTechnique.Passes)
			{
				pass.Apply();
				GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, baseSprite << 2, 0, batchSize << 2, 0, batchSize << 1);
			}
			return;
		}
		GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, baseSprite << 2, 0, batchSize << 2, 0, batchSize << 1);
	}

	public bool ClipBegin(int x, int y, int width, int height)
	{
		if (width <= 0 || height <= 0)
		{
			return false;
		}
		Rectangle scissor = ScissorStack.CalculateScissors(TransformMatrix, x, y, width, height);
		Flush();
		if (ScissorStack.PushScissors(GraphicsDevice, scissor))
		{
			EnableScissorTest(enable: true);
			return true;
		}
		return false;
	}

	public void ClipEnd()
	{
		EnableScissorTest(enable: false);
		ScissorStack.PopScissors(GraphicsDevice);
		Flush();
	}

	public void EnableScissorTest(bool enable)
	{
		bool scissorTestEnable = GraphicsDevice.RasterizerState.ScissorTestEnable;
		if (ScissorStack.HasScissors)
		{
			enable = true;
		}
		if (enable != scissorTestEnable)
		{
			Flush();
			GraphicsDevice.RasterizerState.ScissorTestEnable = enable;
		}
	}

	public void SetBlendState(BlendState blend)
	{
		Flush();
		_blendState = blend ?? BlendState.AlphaBlend;
	}

	public void SetStencil(DepthStencilState stencil)
	{
		Flush();
		_stencil = stencil ?? Stencil;
	}

	public void SetSampler(SamplerState sampler)
	{
		Flush();
		_sampler = sampler ?? SamplerState.PointClamp;
	}

	private unsafe int UpdateVertexBuffer(int start, int count)
	{
		int num;
		SetDataOptions options;
		if (_currentBufferPosition + count > 2048)
		{
			num = 0;
			options = SetDataOptions.Discard;
		}
		else
		{
			num = _currentBufferPosition;
			options = SetDataOptions.NoOverwrite;
		}
		fixed (PositionNormalTextureColor4* ptr = &_vertexInfo[start])
		{
			_vertexBuffer.SetDataPointerEXT(num * 192, (IntPtr)ptr, count * 192, options);
		}
		_currentBufferPosition = num + count;
		return num;
	}

	private static short[] GenerateIndexArray()
	{
		short[] array = new short[12288];
		int num = 0;
		int num2 = 0;
		while (num < 12288)
		{
			array[num] = (short)num2;
			array[num + 1] = (short)(num2 + 1);
			array[num + 2] = (short)(num2 + 2);
			array[num + 3] = (short)(num2 + 1);
			array[num + 4] = (short)(num2 + 3);
			array[num + 5] = (short)(num2 + 2);
			num += 6;
			num2 += 4;
		}
		return array;
	}

	[Conditional("DEBUG")]
	private void EnsureStarted()
	{
		if (!_started)
		{
			throw new InvalidOperationException();
		}
	}

	[Conditional("DEBUG")]
	private void EnsureNotStarted()
	{
		if (_started)
		{
			throw new InvalidOperationException();
		}
	}
}

using System.Runtime.CompilerServices;
using ClassicUO.Game.Managers;
using ClassicUO.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Renderer;

internal class Camera
{
	private float[] _cameraZoomValues = new float[1] { 1f };

	private Matrix _projection;

	private Matrix _transform = Matrix.Identity;

	private Matrix _inverseTransform = Matrix.Identity;

	private bool _updateMatrixes = true;

	private bool _updateProjection = true;

	private int _zoomIndex;

	public Rectangle Bounds;

	public Vector2 Origin;

	public Point Position;

	public Matrix ViewTransformMatrix => TransformMatrix;

	public Matrix ProjectionMatrix
	{
		get
		{
			if (_updateProjection)
			{
				Matrix.CreateOrthographicOffCenter(0f, Bounds.Width, Bounds.Height, 0f, 0f, -1f, out _projection);
				_updateProjection = false;
			}
			return _projection;
		}
	}

	public Matrix TransformMatrix
	{
		get
		{
			UpdateMatrices();
			return _transform;
		}
	}

	public Matrix InverseTransformMatrix
	{
		get
		{
			UpdateMatrices();
			return _inverseTransform;
		}
	}

	public float Zoom
	{
		get
		{
			return _cameraZoomValues[_zoomIndex];
		}
		set
		{
			if (_cameraZoomValues[_zoomIndex] != value)
			{
				_zoomIndex = 0;
				while (_zoomIndex < _cameraZoomValues.Length && _cameraZoomValues[_zoomIndex] != value)
				{
					_zoomIndex++;
				}
				ZoomIndex = _zoomIndex;
			}
		}
	}

	public int ZoomIndex
	{
		get
		{
			return _zoomIndex;
		}
		set
		{
			_updateMatrixes = true;
			_zoomIndex = value;
			if (_zoomIndex < 0)
			{
				_zoomIndex = 0;
			}
			else if (_zoomIndex >= _cameraZoomValues.Length)
			{
				_zoomIndex = _cameraZoomValues.Length - 1;
			}
		}
	}

	public int ZoomValuesCount => _cameraZoomValues.Length;

	public void SetZoomValues(float[] values)
	{
		_cameraZoomValues = values;
	}

	public void SetGameWindowBounds(int x, int y, int width, int height)
	{
		if (Bounds.X != x || Bounds.Y != y || Bounds.Width != width || Bounds.Height != height)
		{
			Bounds.X = x;
			Bounds.Y = y;
			Bounds.Width = width;
			Bounds.Height = height;
			Origin.X = (float)width / 2f;
			Origin.Y = (float)height / 2f;
			_updateMatrixes = true;
			_updateProjection = true;
		}
	}

	public void SetPosition(int x, int y)
	{
		if (Position.X != x || Position.Y != y)
		{
			Position.X = x;
			Position.Y = y;
			_updateMatrixes = true;
		}
	}

	public void SetPositionOffset(int x, int y)
	{
		SetPosition(Position.X + x, Position.Y + y);
	}

	public Viewport GetViewport()
	{
		return new Viewport(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height);
	}

	public void Update()
	{
		UpdateMatrices();
	}

	public Point ScreenToWorld(Point point)
	{
		UpdateMatrices();
		Transform(ref point, ref _inverseTransform, out point);
		return point;
	}

	public Point WorldToScreen(Point point)
	{
		UpdateMatrices();
		Transform(ref point, ref _transform, out point);
		return point;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void Transform(ref Point position, ref Matrix matrix, out Point result)
	{
		float num = (float)position.X * matrix.M11 + (float)position.Y * matrix.M21 + matrix.M41;
		float num2 = (float)position.X * matrix.M12 + (float)position.Y * matrix.M22 + matrix.M42;
		result.X = (int)num;
		result.Y = (int)num2;
	}

	public Point MouseToWorldPosition()
	{
		Point position = Mouse.Position;
		position.X -= Bounds.X;
		position.Y -= Bounds.Y;
		return ScreenToWorld(position);
	}

	private void UpdateMatrices()
	{
		bool hasShakes = ShakeEffectManager.HasShakes;
		if (_updateMatrixes || hasShakes)
		{
			Matrix.CreateTranslation(0f - Origin.X, 0f - Origin.Y, 0f, out _transform);
			float num = 1f / Zoom;
			Matrix result;
			if (num != 1f)
			{
				Matrix.CreateScale(num, num, 1f, out result);
				Matrix.Multiply(ref _transform, ref result, out _transform);
			}
			Matrix.CreateTranslation(Origin.X, Origin.Y, 0f, out result);
			Matrix.Multiply(ref _transform, ref result, out _transform);
			if (hasShakes)
			{
				ShakeEffectManager.ApplyShakeEffect(ref _transform);
			}
			Matrix.Invert(ref _transform, out _inverseTransform);
			_updateMatrixes = false;
		}
	}
}

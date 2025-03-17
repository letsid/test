using Microsoft.Xna.Framework;

namespace ClassicUO.Game.GameObjects;

internal sealed class IsometricLight
{
	private float _height = -0.75f;

	private int _overall = 9;

	private int _realOveall = 9;

	private int _personal = 9;

	private int _realPersonal = 9;

	public int Personal
	{
		get
		{
			return _personal;
		}
		set
		{
			_personal = value;
			Recalculate();
		}
	}

	public int Overall
	{
		get
		{
			return _overall;
		}
		set
		{
			_overall = value;
			Recalculate();
		}
	}

	public float Height
	{
		get
		{
			return _height;
		}
		set
		{
			_height = value;
			Recalculate();
		}
	}

	public int RealPersonal
	{
		get
		{
			return _realPersonal;
		}
		set
		{
			_realPersonal = value;
			Recalculate();
		}
	}

	public int RealOverall
	{
		get
		{
			return _realOveall;
		}
		set
		{
			_realOveall = value;
			Recalculate();
		}
	}

	public float IsometricLevel { get; private set; }

	public Vector3 IsometricDirection { get; } = new Vector3(-1f, -1f, 0.5f);

	private void Recalculate()
	{
		int num = 32 - Overall;
		float num2 = ((Personal > num) ? Personal : num);
		IsometricLevel = num2 * (1f / 32f);
	}
}

using System;
using System.Xml;
using ClassicUO.Configuration;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Gumps;

internal class DebugGump : Gump
{
	private const string DEBUG_STRING_0 = "- FPS: {0} (Min={1}, Max={2}), Zoom: {3}, Total Objs: {4}\n";

	private const string DEBUG_STRING_1 = "- Mobiles: {0}   Items: {1}   Statics: {2}   Multi: {3}   Lands: {4}   Effects: {5}\n";

	private const string DEBUG_STRING_2 = "- CharPos: {0}\n- Mouse: {1}\n- InGamePos: {2}\n";

	private const string DEBUG_STRING_3 = "- Selected: {0}";

	private const string DEBUG_STRING_SMALL = "FPS: {0}\nZoom: {1}";

	private const string DEBUG_STRING_SMALL_NO_ZOOM = "FPS: {0}";

	private static Point _last_position = new Point(-1, -1);

	private uint _timeToUpdate;

	private readonly AlphaBlendControl _alphaBlendControl;

	private string _cacheText = string.Empty;

	public bool IsMinimized { get; set; }

	public override GumpType GumpType => GumpType.Debug;

	public DebugGump(int x, int y)
		: base(0u, 0u)
	{
		CanMove = true;
		base.CanCloseWithEsc = false;
		base.CanCloseWithRightClick = true;
		AcceptMouseInput = true;
		AcceptKeyboardInput = false;
		base.Width = 100;
		base.Height = 50;
		base.X = ((_last_position.X <= 0) ? x : _last_position.X);
		base.Y = ((_last_position.Y <= 0) ? y : _last_position.Y);
		AlphaBlendControl alphaBlendControl = new AlphaBlendControl(0.7f);
		alphaBlendControl.Width = base.Width;
		alphaBlendControl.Height = base.Height;
		AlphaBlendControl c = alphaBlendControl;
		_alphaBlendControl = alphaBlendControl;
		Add(c);
		base.LayerOrder = UILayer.Over;
		base.WantUpdateSize = true;
	}

	protected override bool OnMouseDoubleClick(int x, int y, MouseButtonType button)
	{
		if (button == MouseButtonType.Left)
		{
			IsMinimized = !IsMinimized;
			return true;
		}
		return false;
	}

	public override void Update(double totalTime, double frameTime)
	{
		base.Update(totalTime, frameTime);
		if (Time.Ticks <= _timeToUpdate)
		{
			return;
		}
		_timeToUpdate = Time.Ticks + 100;
		GameScene scene = Client.Game.GetScene<GameScene>();
		Span<char> initialBuffer = stackalloc char[256];
		ValueStringBuilder valueStringBuilder = new ValueStringBuilder(initialBuffer);
		if (IsMinimized && scene != null)
		{
			valueStringBuilder.Append($"- FPS: {CUOEnviroment.CurrentRefreshRate} (Min={0}, Max={0}), Zoom: {((!World.InGame) ? 1f : scene.Camera.Zoom)}, Total Objs: {scene.RenderedObjectsCount}\n");
			valueStringBuilder.Append($"- CUO version: {CUOEnviroment.Version}, Client version: {Settings.GlobalSettings.ClientVersion}\n");
			valueStringBuilder.Append(string.Format("- CharPos: {0}\n- Mouse: {1}\n- InGamePos: {2}\n", World.InGame ? $"{World.Player.X}, {World.Player.Y}, {World.Player.Z}" : "0xFFFF, 0xFFFF, 0", Mouse.Position, (SelectedObject.Object is GameObject gameObject) ? $"{gameObject.X}, {gameObject.Y}, {gameObject.Z}" : "0xFFFF, 0xFFFF, 0"));
			valueStringBuilder.Append($"- Selected: {ReadObject(SelectedObject.Object)}");
			if (CUOEnviroment.Profiler)
			{
				double timeInContext = Profiler.GetContext("RenderFrame").TimeInContext;
				double timeInContext2 = Profiler.GetContext("Update").TimeInContext;
				double timeInContext3 = Profiler.GetContext("FixedUpdate").TimeInContext;
				_ = Profiler.GetContext("OutOfContext").TimeInContext;
				double trackedTime = Profiler.TrackedTime;
				double averageTime = Profiler.GetContext("RenderFrame").AverageTime;
				valueStringBuilder.Append("- Profiling\n");
				valueStringBuilder.Append($"    Draw:{100.0 * (timeInContext / trackedTime):0.0}% Update:{100.0 * (timeInContext2 / trackedTime):0.0}% FixedUpd:{100.0 * (timeInContext3 / trackedTime):0.0} AvgDraw:{averageTime:0.0}ms {CUOEnviroment.CurrentRefreshRate}\n");
			}
		}
		else if (scene != null && scene.Camera.Zoom != 1f)
		{
			valueStringBuilder.Append($"FPS: {CUOEnviroment.CurrentRefreshRate}\nZoom: {((!World.InGame) ? 1f : scene.Camera.Zoom)}");
		}
		else
		{
			valueStringBuilder.Append($"FPS: {CUOEnviroment.CurrentRefreshRate}");
		}
		_cacheText = valueStringBuilder.ToString();
		valueStringBuilder.Dispose();
		Vector2 vector = Fonts.Bold.MeasureString(_cacheText);
		_alphaBlendControl.Width = (base.Width = (int)(vector.X + 20f));
		_alphaBlendControl.Height = (base.Height = (int)(vector.Y + 20f));
		base.WantUpdateSize = true;
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		if (!base.Draw(batcher, x, y))
		{
			return false;
		}
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(0);
		batcher.DrawString(Fonts.Bold, _cacheText, x + 10, y + 10, hueVector);
		return true;
	}

	private string ReadObject(BaseGameObject obj)
	{
		if (obj != null && IsMinimized)
		{
			if (obj is Mobile mobile)
			{
				return $"Mobile (0x{mobile.Serial:X8})  graphic: 0x{mobile.Graphic:X4}  flags: {mobile.Flags}  noto: {mobile.NotorietyFlag} PriorityZ: {mobile.PriorityZ}";
			}
			if (obj is Item item)
			{
				return $"Item (0x{item.Serial:X8})  graphic: 0x{item.Graphic:X4}  flags: {item.Flags}  amount: {item.Amount} itemdata: {item.ItemData.Flags} PriorityZ: {item.PriorityZ}";
			}
			if (obj is Static @static)
			{
				return $"Static (0x{@static.Graphic:X4})  height: {@static.ItemData.Height}  flags: {@static.ItemData.Flags}  Alpha: {@static.AlphaHue} PriorityZ: {@static.PriorityZ}";
			}
			if (obj is Multi multi)
			{
				return $"Multi (0x{multi.Graphic:X4})  height: {multi.ItemData.Height}  flags: {multi.ItemData.Flags}";
			}
			if (obj is GameEffect)
			{
				return "GameEffect";
			}
			if (obj is TextObject textObject)
			{
				return $"TextOverhead type: {textObject.Type}  hue: 0x{textObject.Hue:X4}";
			}
			if (obj is Land land)
			{
				return $"Land (0x{land.Graphic:X4})  flags: {land.TileData.Flags} stretched: {land.IsStretched}  avgZ: {land.AverageZ} minZ: {land.MinZ} PriorityZ: {land.PriorityZ}";
			}
		}
		return string.Empty;
	}

	public override void Save(XmlTextWriter writer)
	{
		base.Save(writer);
		writer.WriteAttributeString("minimized", IsMinimized.ToString());
	}

	public override void Restore(XmlElement xml)
	{
		base.Restore(xml);
		bool.TryParse(xml.GetAttribute("minimized"), out var result);
		IsMinimized = result;
	}

	protected override void OnDragEnd(int x, int y)
	{
		base.OnDragEnd(x, y);
		_last_position.X = base.ScreenCoordinateX;
		_last_position.Y = base.ScreenCoordinateY;
	}

	protected override void OnMove(int x, int y)
	{
		base.OnMove(x, y);
		_last_position.X = base.ScreenCoordinateX;
		_last_position.Y = base.ScreenCoordinateY;
	}
}

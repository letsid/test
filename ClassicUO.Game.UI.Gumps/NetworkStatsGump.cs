using System;
using System.Xml;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.Network;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Gumps;

internal class NetworkStatsGump : Gump
{
	private static Point _last_position = new Point(-1, -1);

	private uint _ping;

	private uint _deltaBytesReceived;

	private uint _deltaBytesSent;

	private uint _time_to_update;

	private readonly AlphaBlendControl _trans;

	private string _cacheText = string.Empty;

	public override GumpType GumpType => GumpType.NetStats;

	public bool IsMinimized { get; set; }

	public NetworkStatsGump(int x, int y)
		: base(0u, 0u)
	{
		CanMove = true;
		base.CanCloseWithEsc = false;
		base.CanCloseWithRightClick = true;
		AcceptMouseInput = true;
		AcceptKeyboardInput = false;
		_ping = (_deltaBytesReceived = (_deltaBytesSent = 0u));
		base.X = ((_last_position.X <= 0) ? x : _last_position.X);
		base.Y = ((_last_position.Y <= 0) ? y : _last_position.Y);
		base.Width = 100;
		base.Height = 30;
		AlphaBlendControl alphaBlendControl = new AlphaBlendControl(0.7f);
		alphaBlendControl.Width = base.Width;
		alphaBlendControl.Height = base.Height;
		AlphaBlendControl c = alphaBlendControl;
		_trans = alphaBlendControl;
		Add(c);
		base.LayerOrder = UILayer.Over;
		base.WantUpdateSize = false;
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
		if (Time.Ticks > _time_to_update)
		{
			_time_to_update = Time.Ticks + 100;
			if (!NetClient.Socket.IsConnected)
			{
				_ping = NetClient.LoginSocket.Statistics.Ping;
				_deltaBytesReceived = NetClient.LoginSocket.Statistics.DeltaBytesReceived;
				_deltaBytesSent = NetClient.LoginSocket.Statistics.DeltaBytesSent;
			}
			else if (!NetClient.Socket.IsDisposed)
			{
				_ping = NetClient.Socket.Statistics.Ping;
				_deltaBytesReceived = NetClient.Socket.Statistics.DeltaBytesReceived;
				_deltaBytesSent = NetClient.Socket.Statistics.DeltaBytesSent;
			}
			Span<char> initialBuffer = stackalloc char[128];
			ValueStringBuilder valueStringBuilder = new ValueStringBuilder(initialBuffer);
			if (IsMinimized)
			{
				valueStringBuilder.Append($"Ping: {_ping} ms");
			}
			else
			{
				valueStringBuilder.Append(string.Format("Ping: {0} ms\n{1} {2,-6} {3} {4,-6}", _ping, "In:", NetStatistics.GetSizeAdaptive(_deltaBytesReceived), "Out:", NetStatistics.GetSizeAdaptive(_deltaBytesSent)));
			}
			_cacheText = valueStringBuilder.ToString();
			valueStringBuilder.Dispose();
			Vector2 vector = Fonts.Bold.MeasureString(_cacheText);
			_trans.Width = (base.Width = (int)(vector.X + 20f));
			_trans.Height = (base.Height = (int)(vector.Y + 20f));
			base.WantUpdateSize = true;
		}
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		if (!base.Draw(batcher, x, y))
		{
			return false;
		}
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(0);
		if (_ping < 150)
		{
			hueVector.X = 68f;
		}
		else if (_ping < 200)
		{
			hueVector.X = 52f;
		}
		else if (_ping < 300)
		{
			hueVector.X = 49f;
		}
		else
		{
			hueVector.X = 32f;
		}
		hueVector.Y = 1f;
		batcher.DrawString(Fonts.Bold, _cacheText, x + 10, y + 10, hueVector);
		return true;
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

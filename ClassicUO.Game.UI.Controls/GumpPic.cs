using System.Collections.Generic;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Network;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Controls;

internal class GumpPic : GumpPicBase
{
	public bool IsPartialHue { get; set; }

	public bool ContainsByBounds { get; set; }

	public bool IsVirtue { get; set; }

	private void InitializeGump(int x, int y, ushort graphic, ushort hue)
	{
		base.X = x;
		base.Y = y;
		base.Graphic = graphic;
		base.Hue = hue;
		base.IsFromServer = true;
	}

	public GumpPic(int x, int y, ushort graphic, ushort hue)
	{
		InitializeGump(x, y, graphic, hue);
	}

	public GumpPic(List<string> parts)
	{
		ushort num = UInt16Converter.Parse(parts[3]);
		for (int i = 4; i < parts.Count; i++)
		{
			string text = parts[i];
			if (text.Length < "class=".Length || !(text.Substring(0, "class=".Length) == "class="))
			{
				continue;
			}
			StaticTiles staticTiles = TileDataLoader.Instance.StaticData[num];
			string text2 = text.Substring("class=".Length);
			if (text2 == "MaleTileGumpItem")
			{
				num = (ushort)(staticTiles.AnimID + 50000);
			}
			else if (text2 == "FemaleTileGumpItem")
			{
				num = (ushort)(staticTiles.AnimID + 60000);
				if (GumpsLoader.Instance.GetGumpTexture(num, out var bounds) == null || (bounds.Height == 0 && bounds.Width == 0))
				{
					num = (ushort)(staticTiles.AnimID + 50000);
				}
			}
			IsPartialHue = staticTiles.IsPartialHue;
		}
		int x = int.Parse(parts[1]);
		int y = int.Parse(parts[2]);
		InitializeGump(x, y, num, (ushort)((parts.Count > 4) ? TransformHue((ushort)(UInt16Converter.Parse(parts[4].Substring(parts[4].IndexOf('=') + 1)) + 1)) : 0));
	}

	protected override bool OnMouseDoubleClick(int x, int y, MouseButtonType button)
	{
		if (IsVirtue && button == MouseButtonType.Left)
		{
			NetClient.Socket.Send_VirtueGumpResponse(World.Player, base.Graphic);
			return true;
		}
		return base.OnMouseDoubleClick(x, y, button);
	}

	public override bool Contains(int x, int y)
	{
		if (!ContainsByBounds)
		{
			return base.Contains(x, y);
		}
		return true;
	}

	private static ushort TransformHue(ushort hue)
	{
		if (hue <= 2)
		{
			hue = 0;
		}
		return hue;
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		if (base.IsDisposed)
		{
			return false;
		}
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(base.Hue, IsPartialHue, base.Alpha, gump: true);
		Rectangle bounds;
		Texture2D gumpTexture = GumpsLoader.Instance.GetGumpTexture(base.Graphic, out bounds);
		if (gumpTexture != null)
		{
			batcher.Draw(gumpTexture, new Rectangle(x, y, base.Width, base.Height), bounds, hueVector);
		}
		return base.Draw(batcher, x, y);
	}
}

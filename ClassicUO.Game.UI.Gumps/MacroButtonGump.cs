using System;
using System.Xml;
using ClassicUO.Configuration;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Gumps;

internal class MacroButtonGump : AnchorableGump
{
	private Texture2D backgroundTexture;

	private Label label;

	public Macro _macro;

	public override GumpType GumpType => GumpType.MacroButton;

	public MacroButtonGump(Macro macro, int x, int y)
		: this()
	{
		base.X = x;
		base.Y = y;
		_macro = macro;
		BuildGump();
	}

	public MacroButtonGump()
		: base(0u, 0u)
	{
		CanMove = true;
		AcceptMouseInput = true;
		base.CanCloseWithRightClick = true;
		base.WantUpdateSize = false;
		base.WidthMultiplier = 2;
		base.HeightMultiplier = 1;
		GroupMatrixWidth = 44;
		GroupMatrixHeight = 44;
		base.AnchorType = ANCHOR_TYPE.SPELL;
	}

	private void BuildGump()
	{
		base.Width = 88;
		base.Height = 44;
		Label obj = new Label(_macro.Name, isunicode: true, 946, base.Width, byte.MaxValue, FontStyle.BlackBorder, TEXT_ALIGN_TYPE.TS_CENTER);
		obj.X = 0;
		obj.Width = base.Width - 10;
		label = obj;
		label.Y = (base.Height >> 1) - (label.Height >> 1);
		Add(label);
		backgroundTexture = SolidColorTextureCache.GetTexture(new Color(30, 30, 30));
	}

	protected override void OnMouseEnter(int x, int y)
	{
		label.Hue = 53;
		backgroundTexture = SolidColorTextureCache.GetTexture(Color.DimGray);
		base.OnMouseEnter(x, y);
	}

	protected override void OnMouseExit(int x, int y)
	{
		label.Hue = 946;
		backgroundTexture = SolidColorTextureCache.GetTexture(new Color(30, 30, 30));
		base.OnMouseExit(x, y);
	}

	protected override void OnMouseUp(int x, int y, MouseButtonType button)
	{
		base.OnMouseUp(x, y, MouseButtonType.Left);
		Point lDragOffset = Mouse.LDragOffset;
		if (ProfileManager.CurrentProfile.CastSpellsByOneClick && button == MouseButtonType.Left && !Keyboard.Alt && Math.Abs(lDragOffset.X) < 5 && Math.Abs(lDragOffset.Y) < 5)
		{
			RunMacro();
		}
	}

	protected override bool OnMouseDoubleClick(int x, int y, MouseButtonType button)
	{
		if (ProfileManager.CurrentProfile.CastSpellsByOneClick || button != MouseButtonType.Left)
		{
			return false;
		}
		RunMacro();
		return true;
	}

	private void RunMacro()
	{
		if (_macro != null)
		{
			GameScene scene = Client.Game.GetScene<GameScene>();
			scene.Macros.SetMacroToExecute(_macro.Items as MacroObject);
			scene.Macros.WaitForTargetTimer = 0L;
			scene.Macros.Update();
		}
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(0);
		batcher.Draw(backgroundTexture, new Rectangle(x, y, base.Width, base.Height), hueVector);
		batcher.DrawRectangle(SolidColorTextureCache.GetTexture(Color.Gray), x, y, base.Width, base.Height, hueVector);
		base.Draw(batcher, x, y);
		return true;
	}

	public override void Save(XmlTextWriter writer)
	{
		if (_macro != null)
		{
			int num = Client.Game.GetScene<GameScene>().Macros.GetAllMacros().IndexOf(_macro);
			base.LocalSerial = (uint)(num + 1000);
			base.Save(writer);
			writer.WriteAttributeString("name", _macro.Name);
		}
	}

	public override void Restore(XmlElement xml)
	{
		base.Restore(xml);
		Macro macro = Client.Game.GetScene<GameScene>().Macros.FindMacro(xml.GetAttribute("name"));
		if (macro != null)
		{
			_macro = macro;
			BuildGump();
		}
	}
}

using System;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Resources;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Gumps;

internal class NameOverHeadHandlerGump : Gump
{
	public static Point? LastPosition;

	public override GumpType GumpType => GumpType.NameOverHeadHandler;

	public NameOverHeadHandlerGump()
		: base(0u, 0u)
	{
		CanMove = true;
		AcceptMouseInput = true;
		base.CanCloseWithRightClick = true;
		if (!LastPosition.HasValue)
		{
			base.X = 100;
			base.Y = 100;
		}
		else
		{
			base.X = LastPosition.Value.X;
			base.Y = LastPosition.Value.Y;
		}
		base.WantUpdateSize = false;
		base.LayerOrder = UILayer.Over;
		AlphaBlendControl alphaBlendControl;
		Add(alphaBlendControl = new AlphaBlendControl(0.7f)
		{
			Hue = 34
		});
		RadioButton all;
		Add(all = new RadioButton(0, 208, 209, ResGumps.All, 0, ushort.MaxValue)
		{
			IsChecked = (NameOverHeadManager.TypeAllowed == NameOverheadTypeAllowed.All)
		});
		RadioButton radioButton = new RadioButton(0, 208, 209, ResGumps.MobilesOnly, 0, ushort.MaxValue);
		radioButton.Y = all.Y + all.Height;
		radioButton.IsChecked = NameOverHeadManager.TypeAllowed == NameOverheadTypeAllowed.Mobiles;
		RadioButton c = radioButton;
		RadioButton mobiles = radioButton;
		Add(c);
		RadioButton radioButton2 = new RadioButton(0, 208, 209, ResGumps.ItemsOnly, 0, ushort.MaxValue);
		radioButton2.Y = mobiles.Y + mobiles.Height;
		radioButton2.IsChecked = NameOverHeadManager.TypeAllowed == NameOverheadTypeAllowed.Items;
		c = radioButton2;
		RadioButton items = radioButton2;
		Add(c);
		RadioButton radioButton3 = new RadioButton(0, 208, 209, ResGumps.MobilesAndCorpsesOnly, 0, ushort.MaxValue);
		radioButton3.Y = items.Y + items.Height;
		radioButton3.IsChecked = NameOverHeadManager.TypeAllowed == NameOverheadTypeAllowed.Corpses;
		c = radioButton3;
		RadioButton mobilesCorpses = radioButton3;
		Add(c);
		alphaBlendControl.Width = Math.Max(mobilesCorpses.Width, Math.Max(items.Width, Math.Max(all.Width, mobiles.Width)));
		alphaBlendControl.Height = all.Height + mobiles.Height + items.Height + mobilesCorpses.Height;
		base.Width = alphaBlendControl.Width;
		base.Height = alphaBlendControl.Height;
		all.ValueChanged += delegate
		{
			if (all.IsChecked)
			{
				NameOverHeadManager.TypeAllowed = NameOverheadTypeAllowed.All;
			}
		};
		mobiles.ValueChanged += delegate
		{
			if (mobiles.IsChecked)
			{
				NameOverHeadManager.TypeAllowed = NameOverheadTypeAllowed.Mobiles;
			}
		};
		items.ValueChanged += delegate
		{
			if (items.IsChecked)
			{
				NameOverHeadManager.TypeAllowed = NameOverheadTypeAllowed.Items;
			}
		};
		mobilesCorpses.ValueChanged += delegate
		{
			if (mobilesCorpses.IsChecked)
			{
				NameOverHeadManager.TypeAllowed = NameOverheadTypeAllowed.Corpses;
			}
		};
	}

	protected override void OnDragEnd(int x, int y)
	{
		LastPosition = new Point(base.ScreenCoordinateX, base.ScreenCoordinateY);
		SetInScreen();
		base.OnDragEnd(x, y);
	}
}

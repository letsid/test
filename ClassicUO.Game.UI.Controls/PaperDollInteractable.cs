using ClassicUO.Configuration;
using ClassicUO.Data;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Utility.Logging;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Controls;

internal class PaperDollInteractable : Control
{
	private class GumpPicEquipment : GumpPic
	{
		private readonly Layer _layer;

		public bool CanLift { get; set; }

		public GumpPicEquipment(uint serial, int x, int y, ushort graphic, ushort hue, Layer layer)
			: base(x, y, graphic, hue)
		{
			base.LocalSerial = serial;
			CanMove = false;
			_layer = layer;
			if (SerialHelper.IsValid(serial) && World.InGame)
			{
				SetTooltip(serial);
			}
		}

		protected override bool OnMouseDoubleClick(int x, int y, MouseButtonType button)
		{
			if (button != MouseButtonType.Left)
			{
				return false;
			}
			if (World.InGame)
			{
				GameActions.DoubleClick(base.LocalSerial);
			}
			return true;
		}

		protected override void OnMouseUp(int x, int y, MouseButtonType button)
		{
			SelectedObject.Object = World.Get(base.LocalSerial);
			base.OnMouseUp(x, y, button);
		}

		public override void Update(double totalTime, double frameTime)
		{
			base.Update(totalTime, frameTime);
			if (!World.InGame)
			{
				return;
			}
			if (CanLift && !ItemHold.Enabled && Mouse.LButtonPressed && UIManager.LastControlMouseDown(MouseButtonType.Left) == this && ((Mouse.LastLeftButtonClickTime != uint.MaxValue && Mouse.LastLeftButtonClickTime != 0 && Mouse.LastLeftButtonClickTime + 350 < Time.Ticks) || Mouse.LDragOffset != Point.Zero))
			{
				GameActions.PickUp(base.LocalSerial, 0, 0, -1, null);
				if (_layer == Layer.OneHanded || _layer == Layer.TwoHanded)
				{
					World.Player.UpdateAbilities();
				}
			}
			else if (base.MouseIsOver)
			{
				SelectedObject.Object = World.Get(base.LocalSerial);
			}
		}

		protected override void OnMouseOver(int x, int y)
		{
			SelectedObject.Object = World.Get(base.LocalSerial);
		}
	}

	private static readonly Layer[] _layerOrder = new Layer[23]
	{
		Layer.Cloak,
		Layer.Shirt,
		Layer.Pants,
		Layer.Shoes,
		Layer.Legs,
		Layer.Arms,
		Layer.Torso,
		Layer.Tunic,
		Layer.Ring,
		Layer.Bracelet,
		Layer.Face,
		Layer.Gloves,
		Layer.Skirt,
		Layer.Robe,
		Layer.Waist,
		Layer.Necklace,
		Layer.Hair,
		Layer.Beard,
		Layer.Earrings,
		Layer.Helmet,
		Layer.OneHanded,
		Layer.TwoHanded,
		Layer.Talisman
	};

	private static readonly Layer[] _layerOrder_quiver_fix = new Layer[23]
	{
		Layer.Shirt,
		Layer.Pants,
		Layer.Shoes,
		Layer.Legs,
		Layer.Arms,
		Layer.Torso,
		Layer.Tunic,
		Layer.Ring,
		Layer.Bracelet,
		Layer.Face,
		Layer.Gloves,
		Layer.Skirt,
		Layer.Robe,
		Layer.Cloak,
		Layer.Waist,
		Layer.Necklace,
		Layer.Hair,
		Layer.Beard,
		Layer.Earrings,
		Layer.Helmet,
		Layer.OneHanded,
		Layer.TwoHanded,
		Layer.Talisman
	};

	public static Layer[] defaultClientLayerOrder = new Layer[23]
	{
		Layer.Cloak,
		Layer.Shirt,
		Layer.Pants,
		Layer.Shoes,
		Layer.Legs,
		Layer.Arms,
		Layer.Torso,
		Layer.Tunic,
		Layer.Ring,
		Layer.Bracelet,
		Layer.Face,
		Layer.Gloves,
		Layer.Skirt,
		Layer.Robe,
		Layer.Waist,
		Layer.Necklace,
		Layer.Hair,
		Layer.Beard,
		Layer.Earrings,
		Layer.Helmet,
		Layer.OneHanded,
		Layer.TwoHanded,
		Layer.Talisman
	};

	private readonly PaperDollGump _paperDollGump;

	private bool _updateUI;

	public bool HasFakeItem { get; private set; }

	public PaperDollInteractable(int x, int y, uint serial, PaperDollGump paperDollGump)
	{
		base.X = x;
		base.Y = y;
		_paperDollGump = paperDollGump;
		AcceptMouseInput = false;
		base.LocalSerial = serial;
		_updateUI = true;
	}

	public override void Update(double totalTime, double frameTime)
	{
		base.Update(totalTime, frameTime);
		if (_updateUI)
		{
			UpdateUI();
			_updateUI = false;
		}
	}

	public void SetFakeItem(bool value)
	{
		_updateUI = (HasFakeItem && !value) || (!HasFakeItem && value);
		HasFakeItem = value;
	}

	private void UpdateUI()
	{
		if (base.IsDisposed)
		{
			return;
		}
		Mobile mobile = World.Mobiles.Get(base.LocalSerial);
		if (mobile == null || mobile.IsDestroyed)
		{
			Dispose();
			return;
		}
		Clear();
		ushort hue = mobile.Hue;
		ushort graphic;
		if (mobile.Graphic == 401 || mobile.Graphic == 403)
		{
			graphic = 13;
		}
		else if (mobile.Graphic == 605)
		{
			graphic = 14;
		}
		else if (mobile.Graphic == 606)
		{
			graphic = 15;
		}
		else if (mobile.Graphic == 666 || mobile.Graphic == 694)
		{
			graphic = 666;
		}
		else if (mobile.Graphic == 667 || mobile.Graphic == 695)
		{
			graphic = 665;
		}
		else if (mobile.Graphic == 1253)
		{
			graphic = 51253;
		}
		else if (mobile.Graphic != 987)
		{
			graphic = (ushort)((!mobile.IsFemale) ? 12 : 13);
		}
		else
		{
			graphic = 12;
			hue = 1002;
		}
		Add(new GumpPic(0, 0, graphic, hue)
		{
			IsPartialHue = true
		});
		if (mobile.Graphic == 987)
		{
			Add(new GumpPic(0, 0, 50987, mobile.Hue)
			{
				AcceptMouseInput = true,
				IsPartialHue = true
			});
		}
		Item item = mobile.FindItemByLayer(Layer.Cloak);
		Item item2 = mobile.FindItemByLayer(Layer.Arms);
		if (item2 != null)
		{
			if (item2.Graphic == 5136)
			{
				_ = 1;
			}
			else
				_ = item2.Graphic == 5143;
		}
		else if (HasFakeItem && ItemHold.Enabled && !ItemHold.IsFixedPosition && 19 == ItemHold.ItemData.Layer)
		{
			if (ItemHold.Graphic == 5136)
			{
				_ = 1;
			}
			else
				_ = ItemHold.Graphic == 5143;
		}
		Layer[] currentLayerOrder = AnimationsLoader.Instance.GetCurrentLayerOrder(mobile);
		foreach (Layer layer in currentLayerOrder)
		{
			item = mobile.FindItemByLayer(layer);
			if (item != null)
			{
				if (!Mobile.IsCovered(mobile, layer))
				{
					ushort animID = GetAnimID(mobile.Graphic, item.ItemData.AnimID, mobile.IsFemale);
					Add(new GumpPicEquipment(item.Serial, 0, 0, animID, (ushort)(item.Hue & 0xBFFF), layer)
					{
						AcceptMouseInput = true,
						IsPartialHue = item.ItemData.IsPartialHue,
						CanLift = (World.InGame && !World.Player.IsDead && layer != Layer.Beard && layer != Layer.Hair && (_paperDollGump.CanLift || base.LocalSerial == (uint)World.Player)),
						Alpha = (item.ItemData.IsTransparent ? 0.7f : 1f)
					});
				}
			}
			else if (HasFakeItem && ItemHold.Enabled && !ItemHold.IsFixedPosition && (uint)layer == ItemHold.ItemData.Layer && ItemHold.ItemData.AnimID != 0)
			{
				ushort animID2 = GetAnimID(mobile.Graphic, ItemHold.ItemData.AnimID, mobile.IsFemale);
				Add(new GumpPicEquipment(0u, 0, 0, animID2, (ushort)(ItemHold.Hue & 0x3FFF), ItemHold.Layer)
				{
					AcceptMouseInput = true,
					IsPartialHue = ItemHold.IsPartialHue,
					Alpha = 0.5f
				});
			}
		}
		item = mobile.FindItemByLayer(Layer.Backpack);
		if (!(item != null) || item.ItemData.AnimID == 0)
		{
			return;
		}
		ushort graphic2 = (ushort)(item.ItemData.AnimID + 50000);
		if (mobile.Serial == World.Player.Serial)
		{
			GumpsLoader instance = GumpsLoader.Instance;
			Rectangle bounds;
			switch (ProfileManager.CurrentProfile.BackpackStyle)
			{
			case 1:
				if (instance.GetGumpTexture(30587u, out bounds) != null)
				{
					graphic2 = 30587;
				}
				break;
			case 2:
				if (instance.GetGumpTexture(30588u, out bounds) != null)
				{
					graphic2 = 30588;
				}
				break;
			case 3:
				if (instance.GetGumpTexture(30589u, out bounds) != null)
				{
					graphic2 = 30589;
				}
				break;
			default:
				if (instance.GetGumpTexture(50422u, out bounds) != null)
				{
					graphic2 = 50422;
				}
				break;
			}
		}
		int num = 0;
		if (World.ClientFeatures.PaperdollBooks)
		{
			num = 6;
		}
		Add(new GumpPicEquipment(item.Serial, -num, 0, graphic2, (ushort)(item.Hue & 0x3FFF), Layer.Backpack)
		{
			AcceptMouseInput = true
		});
	}

	public void Update()
	{
		_updateUI = true;
	}

	private static ushort GetAnimID(ushort graphic, ushort animID, bool isfemale)
	{
		int num = (isfemale ? 60000 : 50000);
		if (Client.Version >= ClientVersion.CV_7000 && animID == 970 && (graphic == 695 || graphic == 694))
		{
			animID = 547;
		}
		AnimationsLoader.Instance.ConvertBodyIfNeeded(ref graphic);
		if (AnimationsLoader.Instance.EquipConversions.TryGetValue(graphic, out var value) && value.TryGetValue(animID, out var value2))
		{
			animID = ((value2.Gump <= 50000) ? value2.Gump : ((ushort)((value2.Gump >= 60000) ? (value2.Gump - 60000) : (value2.Gump - 50000))));
		}
		if (GumpsLoader.Instance.GetGumpTexture((ushort)(animID + num), out var bounds) == null)
		{
			num = (isfemale ? 50000 : 60000);
		}
		if (GumpsLoader.Instance.GetGumpTexture((ushort)(animID + num), out bounds) == null)
		{
			Log.Error($"Texture not found in paperdoll: gump_graphic: {(ushort)(animID + num)}");
		}
		return (ushort)(animID + num);
	}
}

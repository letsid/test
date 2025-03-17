using System;
using System.Collections.Generic;
using ClassicUO.Configuration;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDL2;

namespace ClassicUO.Game;

internal sealed class GameCursor
{
	private readonly struct CursorInfo
	{
		public readonly int Width;

		public readonly int Height;

		public readonly IntPtr CursorPtr;

		public CursorInfo(IntPtr ptr, int w, int h)
		{
			CursorPtr = ptr;
			Width = w;
			Height = h;
		}
	}

	private static readonly ushort[,] _cursorData = new ushort[3, 16]
	{
		{
			8298, 8299, 8300, 8301, 8302, 8303, 8304, 8305, 8306, 8307,
			8308, 8309, 8310, 8311, 8312, 8313
		},
		{
			8275, 8276, 8277, 8278, 8279, 8280, 8281, 8282, 8283, 8284,
			8285, 8286, 8287, 8288, 8289, 8290
		},
		{
			8298, 8299, 8300, 8301, 8302, 8303, 8304, 8305, 8306, 8307,
			8308, 8309, 8310, 8311, 8312, 8313
		}
	};

	private readonly Aura _aura = new Aura(30);

	private readonly CustomBuildObject[] _componentsList = new CustomBuildObject[10];

	private readonly int[,] _cursorOffset = new int[2, 16];

	private readonly IntPtr[,] _cursors_ptr = new IntPtr[3, 16];

	private ushort _graphic = 8307;

	private bool _needGraphicUpdate = true;

	private readonly List<Multi> _temp = new List<Multi>();

	private readonly Tooltip _tooltip;

	public ushort Graphic
	{
		get
		{
			return _graphic;
		}
		set
		{
			if (_graphic != value)
			{
				_graphic = value;
				_needGraphicUpdate = true;
			}
		}
	}

	public bool IsLoading { get; set; }

	public bool IsDraggingCursorForced { get; set; }

	public bool AllowDrawSDLCursor { get; set; } = true;

	public GameCursor()
	{
		_tooltip = new Tooltip();
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				ushort index = _cursorData[i, j];
				int hotX;
				int hotY;
				IntPtr intPtr = ArtLoader.Instance.CreateCursorSurfacePtr(index, (ushort)((i == 2) ? 51u : 0u), out hotX, out hotY);
				if (intPtr != IntPtr.Zero)
				{
					if (hotX != 0 || hotY != 0)
					{
						_cursorOffset[0, j] = hotX;
						_cursorOffset[1, j] = hotY;
					}
					_cursors_ptr[i, j] = SDL.SDL_CreateColorCursor(intPtr, hotX, hotY);
				}
			}
		}
	}

	private ushort GetDraggingItemGraphic()
	{
		if (ItemHold.Enabled)
		{
			if (ItemHold.IsGumpTexture)
			{
				return (ushort)(ItemHold.DisplayedGraphic - 11369);
			}
			return ItemHold.DisplayedGraphic;
		}
		return ushort.MaxValue;
	}

	private Point GetDraggingItemOffset()
	{
		ushort draggingItemGraphic = GetDraggingItemGraphic();
		if (draggingItemGraphic != ushort.MaxValue)
		{
			ArtLoader.Instance.GetStaticTexture(draggingItemGraphic, out var bounds);
			float num = 1f;
			if (ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.ScaleItemsInsideContainers)
			{
				num = UIManager.ContainerScale;
			}
			return new Point((int)((float)(bounds.Width >> 1) * num) - ItemHold.MouseOffset.X, (int)((float)(bounds.Height >> 1) * num) - ItemHold.MouseOffset.Y);
		}
		return Point.Zero;
	}

	public void Update(double totalTime, double frameTime)
	{
		Graphic = AssignGraphicByState();
		if (_needGraphicUpdate)
		{
			_needGraphicUpdate = false;
			if (AllowDrawSDLCursor && Settings.GlobalSettings.RunMouseInASeparateThread)
			{
				ushort graphic = Graphic;
				graphic = ((graphic >= 8298) ? ((ushort)(graphic - 8298)) : ((ushort)(graphic - 8275)));
				int num = ((World.InGame && World.Player.InWarMode) ? 1 : ((World.InGame && World.MapIndex != 0) ? 2 : 0));
				ref IntPtr reference = ref _cursors_ptr[num, graphic];
				if (reference != IntPtr.Zero)
				{
					SDL.SDL_SetCursor(reference);
				}
			}
		}
		if (!ItemHold.Enabled)
		{
			return;
		}
		ushort draggingItemGraphic = GetDraggingItemGraphic();
		if (draggingItemGraphic == ushort.MaxValue || !ItemHold.IsFixedPosition || UIManager.IsDragging)
		{
			return;
		}
		ArtLoader.Instance.GetStaticTexture(draggingItemGraphic, out var bounds);
		Point draggingItemOffset = GetDraggingItemOffset();
		int num2 = ItemHold.FixedX - draggingItemOffset.X;
		int num3 = ItemHold.FixedY - draggingItemOffset.Y;
		if (Mouse.Position.X >= num2 && Mouse.Position.X < num2 + bounds.Width && Mouse.Position.Y >= num3 && Mouse.Position.Y < num3 + bounds.Height)
		{
			if (!ItemHold.IgnoreFixedPosition)
			{
				ItemHold.IsFixedPosition = false;
				ItemHold.FixedX = 0;
				ItemHold.FixedY = 0;
			}
		}
		else if (ItemHold.IgnoreFixedPosition)
		{
			ItemHold.IgnoreFixedPosition = false;
		}
	}

	public void Draw(UltimaBatcher2D sb)
	{
		if (World.InGame && TargetManager.IsTargeting && ProfileManager.CurrentProfile != null)
		{
			if (TargetManager.TargetingState == CursorTarget.MultiPlacement)
			{
				if (World.CustomHouseManager != null && World.CustomHouseManager.SelectedGraphic != 0)
				{
					ushort hue = 0;
					Array.Clear(_componentsList, 0, 10);
					if (!World.CustomHouseManager.CanBuildHere(_componentsList, out var type))
					{
						hue = 33;
					}
					_temp.ForEach(delegate(Multi s)
					{
						s.Destroy();
					});
					_temp.Clear();
					for (int i = 0; i < _componentsList.Length && _componentsList[i].Graphic != 0; i++)
					{
						Multi multi = Multi.Create(_componentsList[i].Graphic);
						multi.AlphaHue = byte.MaxValue;
						multi.Hue = hue;
						multi.State = CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_PREVIEW;
						_temp.Add(multi);
					}
					if (_componentsList.Length != 0 && SelectedObject.LastObject is GameObject gameObject)
					{
						int num = 0;
						if (gameObject.Z < World.CustomHouseManager.MinHouseZ && gameObject.X >= World.CustomHouseManager.StartPos.X && gameObject.X <= World.CustomHouseManager.EndPos.X - 1 && gameObject.Y >= World.CustomHouseManager.StartPos.Y && gameObject.Y <= World.CustomHouseManager.EndPos.Y - 1 && type != CUSTOM_HOUSE_BUILD_TYPE.CHBT_STAIR)
						{
							num += 7;
						}
						GameScene scene = Client.Game.GetScene<GameScene>();
						for (int j = 0; j < _componentsList.Length; j++)
						{
							ref CustomBuildObject reference = ref _componentsList[j];
							if (reference.Graphic == 0)
							{
								break;
							}
							_temp[j].X = (ushort)(gameObject.X + reference.X);
							_temp[j].Y = (ushort)(gameObject.Y + reference.Y);
							_temp[j].Z = (sbyte)(gameObject.Z + num + reference.Z);
							_temp[j].UpdateRealScreenPosition(scene.ScreenOffset.X, scene.ScreenOffset.Y);
							_temp[j].UpdateScreenPosition();
							_temp[j].AddToTile();
						}
					}
				}
				else if (_temp.Count != 0)
				{
					_temp.ForEach(delegate(Multi s)
					{
						s.Destroy();
					});
					_temp.Clear();
				}
			}
			else if (_temp.Count != 0)
			{
				_temp.ForEach(delegate(Multi s)
				{
					s.Destroy();
				});
				_temp.Clear();
			}
			if (ProfileManager.CurrentProfile.AuraOnMouse)
			{
				ushort graphic = Graphic;
				if (graphic < 8298)
				{
					graphic -= 8275;
				}
				else
				{
					graphic -= 8298;
				}
				ushort hue2 = 0;
				switch (TargetManager.TargetingType)
				{
				case TargetType.Neutral:
					hue2 = 946;
					break;
				case TargetType.Harmful:
					hue2 = 35;
					break;
				case TargetType.Beneficial:
					hue2 = 90;
					break;
				}
				_aura.Draw(sb, Mouse.Position.X, Mouse.Position.Y, hue2, 0f);
			}
			if (ProfileManager.CurrentProfile.ShowTargetRangeIndicator && UIManager.IsMouseOverWorld && SelectedObject.Object is GameObject { Distance: var distance })
			{
				string text = distance.ToString();
				Vector3 color = new Vector3(0f, 1f, 1f);
				sb.DrawString(Fonts.Bold, text, Mouse.Position.X - 26, Mouse.Position.Y - 21, color);
				color.Y = 0f;
				sb.DrawString(Fonts.Bold, text, Mouse.Position.X - 25, Mouse.Position.Y - 20, color);
			}
		}
		else if (_temp.Count != 0)
		{
			_temp.ForEach(delegate(Multi s)
			{
				s.Destroy();
			});
			_temp.Clear();
		}
		if (ItemHold.Enabled && !ItemHold.Dropped)
		{
			float num2 = 1f;
			if (ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.ScaleItemsInsideContainers)
			{
				num2 = UIManager.ContainerScale;
			}
			ushort draggingItemGraphic = GetDraggingItemGraphic();
			Rectangle bounds;
			Texture2D staticTexture = ArtLoader.Instance.GetStaticTexture(draggingItemGraphic, out bounds);
			if (staticTexture != null)
			{
				Point draggingItemOffset = GetDraggingItemOffset();
				int x = (ItemHold.IsFixedPosition ? ItemHold.FixedX : Mouse.Position.X) - draggingItemOffset.X;
				int y = (ItemHold.IsFixedPosition ? ItemHold.FixedY : Mouse.Position.Y) - draggingItemOffset.Y;
				Vector3 hueVector = ShaderHueTranslator.GetHueVector(ItemHold.Hue, ItemHold.IsPartialHue, ItemHold.HasAlpha ? 0.5f : 1f);
				Rectangle destinationRectangle = new Rectangle(x, y, (int)((float)bounds.Width * num2), (int)((float)bounds.Height * num2));
				sb.Draw(staticTexture, destinationRectangle, bounds, hueVector);
				if (ItemHold.Amount > 1 && ItemHold.DisplayedGraphic == ItemHold.Graphic && ItemHold.IsStackable)
				{
					destinationRectangle.X += 5;
					destinationRectangle.Y += 5;
					sb.Draw(staticTexture, destinationRectangle, bounds, hueVector);
				}
			}
		}
		DrawToolTip(sb, Mouse.Position);
		if (!Settings.GlobalSettings.RunMouseInASeparateThread)
		{
			Graphic = AssignGraphicByState();
			ushort graphic2 = Graphic;
			graphic2 = ((graphic2 >= 8298) ? ((ushort)(graphic2 - 8298)) : ((ushort)(graphic2 - 8275)));
			int num3 = _cursorOffset[0, graphic2];
			int num4 = _cursorOffset[1, graphic2];
			Vector3 color2 = ((!World.InGame || World.MapIndex == 0 || World.Player.InWarMode) ? ShaderHueTranslator.GetHueVector(0) : ShaderHueTranslator.GetHueVector(51));
			Rectangle bounds2;
			Texture2D staticTexture2 = ArtLoader.Instance.GetStaticTexture(Graphic, out bounds2);
			sb.Draw(staticTexture2, new Vector2(Mouse.Position.X - num3, Mouse.Position.Y - num4), bounds2, color2);
		}
	}

	private void DrawToolTip(UltimaBatcher2D batcher, Point position)
	{
		if (Client.Game.Scene is GameScene && ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.UseTooltip)
		{
			if (!World.ClientFeatures.TooltipsEnabled || (SelectedObject.Object is Item { IsLocked: not false } item && item.ItemData.Weight == byte.MaxValue && !item.ItemData.IsContainer && !World.OPL.Contains(item)) || (ItemHold.Enabled && !ItemHold.IsFixedPosition))
			{
				if (!_tooltip.IsEmpty && (UIManager.MouseOverControl == null || UIManager.IsMouseOverWorld))
				{
					_tooltip.Clear();
				}
			}
			else
			{
				if (UIManager.IsMouseOverWorld && SelectedObject.Object is Entity entity && World.OPL.Contains(entity))
				{
					if (_tooltip.IsEmpty || (uint)entity != _tooltip.Serial)
					{
						_tooltip.SetGameObject(entity);
					}
					_tooltip.Draw(batcher, position.X, position.Y + 24);
					return;
				}
				if (UIManager.MouseOverControl != null && UIManager.MouseOverControl.Tooltip is uint num && SerialHelper.IsValid(num) && World.OPL.Contains(num))
				{
					if (_tooltip.IsEmpty || num != _tooltip.Serial)
					{
						_tooltip.SetGameObject(num);
					}
					_tooltip.Draw(batcher, position.X, position.Y + 24);
					return;
				}
			}
		}
		if (UIManager.MouseOverControl != null && UIManager.MouseOverControl.HasTooltip && !Mouse.IsDragging)
		{
			if (UIManager.MouseOverControl.Tooltip is string text)
			{
				if (_tooltip.IsEmpty || _tooltip.Text != text)
				{
					_tooltip.SetText(text, UIManager.MouseOverControl.TooltipMaxLength);
				}
				_tooltip.Draw(batcher, position.X, position.Y + 24);
			}
		}
		else if (!_tooltip.IsEmpty)
		{
			_tooltip.Clear();
		}
	}

	private ushort AssignGraphicByState()
	{
		int num = ((World.InGame && World.Player.InWarMode) ? 1 : 0);
		if (TargetManager.IsTargeting)
		{
			return _cursorData[num, 12];
		}
		if (UIManager.IsDragging || IsDraggingCursorForced)
		{
			return _cursorData[num, 8];
		}
		if (IsLoading)
		{
			return _cursorData[num, 13];
		}
		if (UIManager.MouseOverControl != null && UIManager.MouseOverControl.AcceptKeyboardInput && UIManager.MouseOverControl.IsEditable)
		{
			return _cursorData[num, 14];
		}
		ushort result = _cursorData[num, 9];
		if (!UIManager.IsMouseOverWorld)
		{
			return result;
		}
		if (ProfileManager.CurrentProfile == null)
		{
			return result;
		}
		int x = ProfileManager.CurrentProfile.GameWindowPosition.X + (ProfileManager.CurrentProfile.GameWindowSize.X >> 1);
		int y = ProfileManager.CurrentProfile.GameWindowPosition.Y + (ProfileManager.CurrentProfile.GameWindowSize.Y >> 1);
		return _cursorData[num, GetMouseDirection(x, y, Mouse.Position.X, Mouse.Position.Y, 1)];
	}

	public static int GetMouseDirection(int x1, int y1, int to_x, int to_y, int current_facing)
	{
		int num = to_x - x1;
		int num2 = to_y - y1;
		int num3 = 100 * (Sgn(num) + 2) + 10 * (Sgn(num2) + 2);
		if (num != 0 && num2 != 0)
		{
			num = Math.Abs(num);
			num2 = Math.Abs(num2);
			num3 = ((num2 * 5 <= num * 2) ? (num3 + 1) : ((num2 * 2 < num * 5) ? (num3 + 2) : (num3 + 3)));
		}
		else if (num == 0 && num2 == 0)
		{
			return current_facing;
		}
		return num3 switch
		{
			111 => 6, 
			112 => 7, 
			113 => 0, 
			120 => 6, 
			131 => 6, 
			132 => 5, 
			133 => 4, 
			210 => 0, 
			230 => 4, 
			311 => 2, 
			312 => 1, 
			313 => 0, 
			320 => 2, 
			331 => 2, 
			332 => 3, 
			333 => 4, 
			_ => current_facing, 
		};
	}

	private static int Sgn(int val)
	{
		bool num = 0 < val;
		int num2 = ((val < 0) ? 1 : 0);
		return (num ? 1 : 0) - num2;
	}
}

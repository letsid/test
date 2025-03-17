using System.Collections.Generic;
using ClassicUO.Configuration;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.Input;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.Managers;

internal static class UIManager
{
	private static readonly Dictionary<uint, Point> _gumpPositionCache = new Dictionary<uint, Point>();

	private static readonly Control[] _mouseDownControls = new Control[255];

	private static Point _dragOrigin;

	private static bool _isDraggingControl;

	private static Control _keyboardFocusControl;

	private static Control _lastFocus;

	private static bool _needSort;

	public static float ContainerScale { get; set; } = 1f;

	public static AnchorManager AnchorManager { get; } = new AnchorManager();

	public static LinkedList<Gump> Gumps { get; } = new LinkedList<Gump>();

	public static Control MouseOverControl { get; private set; }

	public static bool IsModalOpen { get; private set; }

	public static bool IsMouseOverWorld
	{
		get
		{
			Point position = Mouse.Position;
			Profile currentProfile = ProfileManager.CurrentProfile;
			if (currentProfile != null && GameCursor.AllowDrawSDLCursor && DraggingControl == null && MouseOverControl == null && !IsModalOpen && position.X >= currentProfile.GameWindowPosition.X + 5 && position.X < currentProfile.GameWindowPosition.X + 5 + currentProfile.GameWindowSize.X && position.Y >= currentProfile.GameWindowPosition.Y + 5)
			{
				return position.Y < currentProfile.GameWindowPosition.Y + 5 + currentProfile.GameWindowSize.Y;
			}
			return false;
		}
	}

	public static Control DraggingControl { get; private set; }

	public static GameCursor GameCursor { get; private set; }

	public static SystemChatControl SystemChat { get; set; }

	public static PopupMenuGump PopupMenu { get; private set; }

	public static Control KeyboardFocusControl
	{
		get
		{
			return _keyboardFocusControl;
		}
		set
		{
			if (_keyboardFocusControl != value)
			{
				_keyboardFocusControl?.OnFocusLost();
				_keyboardFocusControl = value;
				if (value != null && value.AcceptKeyboardInput && !value.IsFocused)
				{
					value.OnFocusEnter();
				}
			}
		}
	}

	public static bool IsDragging
	{
		get
		{
			if (_isDraggingControl)
			{
				return DraggingControl != null;
			}
			return false;
		}
	}

	public static ContextMenuShowMenu ContextMenu { get; private set; }

	public static void ShowGamePopup(PopupMenuGump popup)
	{
		PopupMenu?.Dispose();
		PopupMenu = popup;
		if (popup != null && !popup.IsDisposed)
		{
			Add(PopupMenu);
		}
	}

	public static bool IsModalControlOpen()
	{
		foreach (Gump gump in Gumps)
		{
			if (gump.IsModal)
			{
				return true;
			}
		}
		return false;
	}

	public static void OnMouseDragging()
	{
		HandleMouseInput();
		if (_mouseDownControls[1] != null && (ProfileManager.CurrentProfile == null || !ProfileManager.CurrentProfile.HoldAltToMoveGumps || Keyboard.Alt))
		{
			AttemptDragControl(_mouseDownControls[1], attemptAlwaysSuccessful: true);
		}
		if (_isDraggingControl)
		{
			DoDragControl();
		}
	}

	public static void OnMouseButtonDown(MouseButtonType button)
	{
		HandleMouseInput();
		if (MouseOverControl != null)
		{
			if (MouseOverControl.IsEnabled && MouseOverControl.IsVisible && _lastFocus != MouseOverControl)
			{
				_lastFocus?.OnFocusLost();
				MouseOverControl.OnFocusEnter();
				_lastFocus = MouseOverControl;
			}
			MakeTopMostGump(MouseOverControl);
			MouseOverControl.InvokeMouseDown(Mouse.Position, button);
			if (MouseOverControl.AcceptKeyboardInput)
			{
				_keyboardFocusControl = MouseOverControl;
			}
			_mouseDownControls[(int)button] = MouseOverControl;
		}
		else
		{
			foreach (Gump gump in Gumps)
			{
				if (gump.IsModal && gump.ModalClickOutsideAreaClosesThisControl)
				{
					gump.Dispose();
					Mouse.CancelDoubleClick = true;
				}
			}
		}
		if (PopupMenu != null && !PopupMenu.Bounds.Contains(Mouse.Position.X, Mouse.Position.Y))
		{
			ShowGamePopup(null);
		}
	}

	public static void OnMouseButtonUp(MouseButtonType button)
	{
		EndDragControl(Mouse.Position);
		HandleMouseInput();
		if (MouseOverControl != null)
		{
			if ((_mouseDownControls[(int)button] != null && MouseOverControl == _mouseDownControls[(int)button]) || ItemHold.Enabled)
			{
				MouseOverControl.InvokeMouseUp(Mouse.Position, button);
			}
			else if (_mouseDownControls[(int)button] != null && MouseOverControl != _mouseDownControls[(int)button])
			{
				_mouseDownControls[(int)button].InvokeMouseUp(Mouse.Position, button);
			}
		}
		else
		{
			_mouseDownControls[(int)button]?.InvokeMouseUp(Mouse.Position, button);
		}
		if (button == MouseButtonType.Right)
		{
			Control control = _mouseDownControls[(int)button];
			if (control != null && MouseOverControl == control)
			{
				control.InvokeMouseCloseGumpWithRClick();
			}
		}
		_mouseDownControls[(int)button] = null;
	}

	public static bool OnMouseDoubleClick(MouseButtonType button)
	{
		HandleMouseInput();
		if (MouseOverControl != null && MouseOverControl.InvokeMouseDoubleClick(Mouse.Position, button))
		{
			if (button == MouseButtonType.Left)
			{
				DelayedObjectClickManager.Clear();
			}
			return true;
		}
		return false;
	}

	public static void OnMouseWheel(bool isup)
	{
		if (MouseOverControl != null && MouseOverControl.AcceptMouseInput)
		{
			MouseOverControl.InvokeMouseWheel(isup ? MouseEventType.WheelScrollUp : MouseEventType.WheelScrollDown);
		}
	}

	public static Control LastControlMouseDown(MouseButtonType button)
	{
		return _mouseDownControls[(int)button];
	}

	public static void InitializeGameCursor()
	{
		GameCursor = new GameCursor();
	}

	public static void SavePosition(uint serverSerial, Point point)
	{
		_gumpPositionCache[serverSerial] = point;
	}

	public static bool RemovePosition(uint serverSerial)
	{
		return _gumpPositionCache.Remove(serverSerial);
	}

	public static bool GetGumpCachePosition(uint id, out Point pos)
	{
		return _gumpPositionCache.TryGetValue(id, out pos);
	}

	public static void ShowContextMenu(ContextMenuShowMenu menu)
	{
		ContextMenu?.Dispose();
		ContextMenu = menu;
		if (ContextMenu != null && !menu.IsDisposed)
		{
			Add(ContextMenu);
		}
	}

	public static T GetGump<T>(uint? serial = null) where T : Control
	{
		if (serial.HasValue)
		{
			for (LinkedListNode<Gump> linkedListNode = Gumps.Last; linkedListNode != null; linkedListNode = linkedListNode.Previous)
			{
				Control value = linkedListNode.Value;
				if (!value.IsDisposed && value.LocalSerial == serial.Value && value is T result)
				{
					return result;
				}
			}
		}
		else
		{
			for (LinkedListNode<Gump> linkedListNode2 = Gumps.First; linkedListNode2 != null; linkedListNode2 = linkedListNode2.Next)
			{
				Control value2 = linkedListNode2.Value;
				if (!value2.IsDisposed && value2 is T result2)
				{
					return result2;
				}
			}
		}
		return null;
	}

	public static Gump GetGump(uint serial)
	{
		for (LinkedListNode<Gump> linkedListNode = Gumps.Last; linkedListNode != null; linkedListNode = linkedListNode.Previous)
		{
			Control value = linkedListNode.Value;
			if (!value.IsDisposed && value.LocalSerial == serial)
			{
				return value as Gump;
			}
		}
		return null;
	}

	public static TradingGump GetTradingGump(uint serial)
	{
		for (LinkedListNode<Gump> linkedListNode = Gumps.Last; linkedListNode != null; linkedListNode = linkedListNode.Previous)
		{
			if (linkedListNode.Value != null && !linkedListNode.Value.IsDisposed && linkedListNode.Value is TradingGump tradingGump && (tradingGump.ID1 == serial || tradingGump.ID2 == serial || tradingGump.LocalSerial == serial))
			{
				return tradingGump;
			}
		}
		return null;
	}

	public static void Update(double totalTime, double frameTime)
	{
		SortControlsByInfo();
		LinkedListNode<Gump> linkedListNode = Gumps.First;
		while (linkedListNode != null)
		{
			LinkedListNode<Gump>? next = linkedListNode.Next;
			Gump value = linkedListNode.Value;
			value.Update(totalTime, frameTime);
			if (value.IsDisposed)
			{
				Gumps.Remove(linkedListNode);
			}
			linkedListNode = next;
		}
		GameCursor?.Update(totalTime, frameTime);
		HandleKeyboardInput();
		HandleMouseInput();
	}

	public static void Draw(UltimaBatcher2D batcher)
	{
		SortControlsByInfo();
		batcher.Begin();
		for (LinkedListNode<Gump> linkedListNode = Gumps.Last; linkedListNode != null; linkedListNode = linkedListNode.Previous)
		{
			Control value = linkedListNode.Value;
			value.Draw(batcher, value.X, value.Y);
		}
		GameCursor?.Draw(batcher);
		batcher.End();
	}

	public static void Add(Gump gump, bool front = true)
	{
		if (!gump.IsDisposed)
		{
			if (front)
			{
				Gumps.AddFirst(gump);
			}
			else
			{
				Gumps.AddLast(gump);
			}
			_needSort = Gumps.Count > 1;
		}
	}

	public static void Clear()
	{
		foreach (Gump gump in Gumps)
		{
			gump.Dispose();
		}
	}

	private static void HandleKeyboardInput()
	{
		if (_keyboardFocusControl != null && _keyboardFocusControl.IsDisposed)
		{
			_keyboardFocusControl = null;
		}
		if (_keyboardFocusControl != null)
		{
			return;
		}
		if (SystemChat != null && !SystemChat.IsDisposed)
		{
			_keyboardFocusControl = SystemChat.TextBoxControl;
			_keyboardFocusControl.OnFocusEnter();
			return;
		}
		for (LinkedListNode<Gump> linkedListNode = Gumps.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			Control value = linkedListNode.Value;
			if (!value.IsDisposed && value.IsVisible && value.IsEnabled)
			{
				_keyboardFocusControl = value.GetFirstControlAcceptKeyboardInput();
				if (_keyboardFocusControl != null)
				{
					_keyboardFocusControl.OnFocusEnter();
					break;
				}
			}
		}
	}

	private static void HandleMouseInput()
	{
		Control mouseOverControl = GetMouseOverControl(Mouse.Position);
		if (MouseOverControl != null && mouseOverControl != MouseOverControl)
		{
			MouseOverControl.InvokeMouseExit(Mouse.Position);
			if (MouseOverControl.RootParent != null && (mouseOverControl == null || mouseOverControl.RootParent != MouseOverControl.RootParent))
			{
				MouseOverControl.RootParent.InvokeMouseExit(Mouse.Position);
			}
		}
		if (mouseOverControl != null)
		{
			if (mouseOverControl != MouseOverControl)
			{
				mouseOverControl.InvokeMouseEnter(Mouse.Position);
				if (mouseOverControl.RootParent != null && (MouseOverControl == null || mouseOverControl.RootParent != MouseOverControl.RootParent))
				{
					mouseOverControl.RootParent.InvokeMouseEnter(Mouse.Position);
				}
			}
			mouseOverControl.InvokeMouseOver(Mouse.Position);
		}
		MouseOverControl = mouseOverControl;
		for (int i = 0; i < 6; i++)
		{
			if (_mouseDownControls[i] != null && _mouseDownControls[i] != mouseOverControl)
			{
				_mouseDownControls[i].InvokeMouseOver(Mouse.Position);
			}
		}
	}

	private static Control GetMouseOverControl(Point position)
	{
		if (_isDraggingControl)
		{
			return DraggingControl;
		}
		Control res = null;
		IsModalOpen = IsModalControlOpen();
		for (LinkedListNode<Gump> linkedListNode = Gumps.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			Control value = linkedListNode.Value;
			if ((!IsModalOpen || value.IsModal) && value.IsVisible && value.IsEnabled)
			{
				value.HitTest(position, ref res);
				if (res != null)
				{
					return res;
				}
			}
		}
		return null;
	}

	public static void MakeTopMostGump(Control control)
	{
		Gump gump = control as Gump;
		if (gump == null && control?.RootParent is Gump)
		{
			gump = control.RootParent as Gump;
		}
		if (gump == null)
		{
			return;
		}
		for (LinkedListNode<Gump> linkedListNode = Gumps.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			if (linkedListNode.Value == gump)
			{
				if (gump.LayerOrder == UILayer.Under)
				{
					if (linkedListNode != Gumps.Last)
					{
						Gumps.Remove(gump);
						Gumps.AddBefore(Gumps.Last, linkedListNode);
					}
				}
				else
				{
					Gumps.Remove(gump);
					Gumps.AddFirst(linkedListNode);
				}
				break;
			}
		}
		_needSort = Gumps.Count > 1;
	}

	private static void SortControlsByInfo()
	{
		if (!_needSort)
		{
			return;
		}
		for (LinkedListNode<Gump> linkedListNode = Gumps.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			Gump value = linkedListNode.Value;
			if (value.LayerOrder != UILayer.Default)
			{
				if (value.LayerOrder == UILayer.Under)
				{
					for (LinkedListNode<Gump> linkedListNode2 = Gumps.First; linkedListNode2 != null; linkedListNode2 = linkedListNode2.Next)
					{
						if (linkedListNode2.Value == value && value != Gumps.Last.Value)
						{
							Gumps.Remove(linkedListNode2);
							Gumps.AddBefore(Gumps.Last, linkedListNode2);
						}
					}
				}
				else if (value.LayerOrder == UILayer.Over)
				{
					for (LinkedListNode<Gump> linkedListNode3 = Gumps.First; linkedListNode3 != null; linkedListNode3 = linkedListNode3.Next)
					{
						if (linkedListNode3.Value == value)
						{
							Gumps.Remove(linkedListNode3);
							Gumps.AddFirst(value);
						}
					}
				}
			}
		}
		_needSort = false;
	}

	public static void AttemptDragControl(Control control, bool attemptAlwaysSuccessful = false)
	{
		if (_isDraggingControl || (ItemHold.Enabled && !ItemHold.IsFixedPosition))
		{
			return;
		}
		Control control2 = control;
		if (!control2.CanMove)
		{
			return;
		}
		while (control2.Parent != null)
		{
			control2 = control2.Parent;
		}
		if (!control2.CanMove)
		{
			return;
		}
		if (attemptAlwaysSuccessful || !_isDraggingControl)
		{
			DraggingControl = control2;
			_dragOrigin = Mouse.LClickPosition;
			for (int i = 0; i < 6; i++)
			{
				_mouseDownControls[i] = null;
			}
		}
		Point point = Mouse.Position - _dragOrigin;
		if (attemptAlwaysSuccessful || point != Point.Zero)
		{
			_isDraggingControl = true;
			control2.InvokeDragBegin(point);
		}
	}

	private static void DoDragControl()
	{
		if (DraggingControl != null)
		{
			Point point = Mouse.Position - _dragOrigin;
			DraggingControl.X += point.X;
			DraggingControl.Y += point.Y;
			DraggingControl.InvokeMove(point.X, point.Y);
			_dragOrigin = Mouse.Position;
		}
	}

	private static void EndDragControl(Point mousePosition)
	{
		if (_isDraggingControl)
		{
			DoDragControl();
		}
		DraggingControl?.InvokeDragEnd(mousePosition);
		DraggingControl = null;
		_isDraggingControl = false;
	}
}

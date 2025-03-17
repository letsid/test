using System;
using System.Collections.Generic;
using System.Linq;
using ClassicUO.Game.Managers;
using ClassicUO.Input;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SDL2;

namespace ClassicUO.Game.UI.Controls;

internal abstract class Control
{
	internal static int _StepsDone = 1;

	internal static int _StepChanger = 1;

	private bool _acceptKeyboardInput;

	private bool _acceptMouseInput;

	private bool _mouseIsDown;

	private int _activePage;

	private bool _attempToDrag;

	private Rectangle _bounds;

	private bool _handlesKeyboardFocus;

	private Point _offset;

	private Control _parent;

	public virtual ClickPriority Priority { get; set; } = ClickPriority.Default;

	public uint ServerSerial { get; set; }

	public uint LocalSerial { get; set; }

	public bool IsFromServer { get; set; }

	public int Page { get; set; }

	public Point Location
	{
		get
		{
			return _bounds.Location;
		}
		set
		{
			X = value.X;
			Y = value.Y;
			_bounds.Location = value;
		}
	}

	public ref Rectangle Bounds => ref _bounds;

	public Point Offset => _offset;

	public bool IsDisposed { get; private set; }

	public bool IsVisible { get; set; } = true;

	public bool IsEnabled { get; set; }

	public bool HasKeyboardFocus => UIManager.KeyboardFocusControl == this;

	public bool MouseIsOver => UIManager.MouseOverControl == this;

	public virtual bool CanMove { get; set; }

	public bool CanCloseWithRightClick { get; set; } = true;

	public bool CanCloseWithEsc { get; set; }

	public bool IsEditable { get; set; }

	public bool IsFocused { get; set; }

	public float Alpha { get; set; } = 1f;

	public List<Control> Children { get; }

	public object Tag { get; set; }

	public object Tooltip { get; private set; }

	public bool HasTooltip => Tooltip != null;

	public virtual bool AcceptKeyboardInput
	{
		get
		{
			if (IsEnabled && !IsDisposed && IsVisible)
			{
				return _acceptKeyboardInput;
			}
			return false;
		}
		set
		{
			_acceptKeyboardInput = value;
		}
	}

	public virtual bool AcceptMouseInput
	{
		get
		{
			if (IsEnabled && !IsDisposed && _acceptMouseInput)
			{
				return IsVisible;
			}
			return false;
		}
		set
		{
			_acceptMouseInput = value;
		}
	}

	public ref int X => ref _bounds.X;

	public ref int Y => ref _bounds.Y;

	public ref int Width => ref _bounds.Width;

	public ref int Height => ref _bounds.Height;

	public int ParentX
	{
		get
		{
			if (Parent == null)
			{
				return 0;
			}
			return Parent.X + Parent.ParentX;
		}
	}

	public int ParentY
	{
		get
		{
			if (Parent == null)
			{
				return 0;
			}
			return Parent.Y + Parent.ParentY;
		}
	}

	public int ScreenCoordinateX => ParentX + X;

	public int ScreenCoordinateY => ParentY + Y;

	public ContextMenuControl ContextMenu { get; set; }

	public Control Parent
	{
		get
		{
			return _parent;
		}
		internal set
		{
			if (value == null)
			{
				_parent?.Children.Remove(this);
			}
			else
			{
				_parent?.Children.Remove(this);
				value.Children.Add(this);
			}
			_parent = value;
		}
	}

	public Control RootParent
	{
		get
		{
			if (Parent == null)
			{
				return null;
			}
			Control parent = Parent;
			while (parent.Parent != null)
			{
				parent = parent.Parent;
			}
			return parent;
		}
	}

	public UILayer LayerOrder { get; set; } = UILayer.Default;

	public bool IsModal { get; set; }

	public bool ModalClickOutsideAreaClosesThisControl { get; set; }

	public virtual bool HandlesKeyboardFocus
	{
		get
		{
			if (!IsEnabled || IsDisposed || !IsVisible)
			{
				return false;
			}
			if (_handlesKeyboardFocus)
			{
				return true;
			}
			if (Children == null)
			{
				return false;
			}
			foreach (Control child in Children)
			{
				if (child.HandlesKeyboardFocus)
				{
					return true;
				}
			}
			return false;
		}
		set
		{
			_handlesKeyboardFocus = value;
		}
	}

	public int ActivePage
	{
		get
		{
			return _activePage;
		}
		set
		{
			_activePage = value;
			OnPageChanged();
		}
	}

	public bool WantUpdateSize { get; set; } = true;

	public bool AllowedToDraw { get; set; }

	public int TooltipMaxLength { get; private set; }

	internal event EventHandler<MouseEventArgs> MouseDown;

	internal event EventHandler<MouseEventArgs> MouseUp;

	internal event EventHandler<MouseEventArgs> MouseOver;

	internal event EventHandler<MouseEventArgs> MouseEnter;

	internal event EventHandler<MouseEventArgs> MouseExit;

	internal event EventHandler<MouseEventArgs> DragBegin;

	internal event EventHandler<MouseEventArgs> DragEnd;

	internal event EventHandler<MouseWheelEventArgs> MouseWheel;

	internal event EventHandler<MouseDoubleClickEventArgs> MouseDoubleClick;

	internal event EventHandler FocusEnter;

	internal event EventHandler FocusLost;

	internal event EventHandler<KeyboardEventArgs> KeyDown;

	internal event EventHandler<KeyboardEventArgs> KeyUp;

	protected Control(Control parent = null)
	{
		Parent = parent;
		Children = new List<Control>();
		AllowedToDraw = true;
		AcceptMouseInput = true;
		Page = 0;
		IsDisposed = false;
		IsEnabled = true;
	}

	public void UpdateOffset(int x, int y)
	{
		if (_offset.X == x && _offset.Y == y)
		{
			return;
		}
		_offset.X = x;
		_offset.Y = y;
		foreach (Control child in Children)
		{
			child.UpdateOffset(x, y);
		}
	}

	public virtual bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		if (IsDisposed)
		{
			return false;
		}
		foreach (Control child in Children)
		{
			if ((child.Page == 0 || child.Page == ActivePage) && child.IsVisible)
			{
				child.Draw(batcher, child.X + x, child.Y + y);
			}
		}
		DrawDebug(batcher, x, y);
		return true;
	}

	public virtual void Update(double totalTime, double frameTime)
	{
		if (IsDisposed || Children.Count == 0)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < Children.Count; i++)
		{
			Control control = Children[i];
			if (control.IsDisposed)
			{
				OnChildRemoved();
				Children.RemoveAt(i--);
				continue;
			}
			control.Update(totalTime, frameTime);
			if (WantUpdateSize && (control.Page == 0 || control.Page == ActivePage) && control.IsVisible)
			{
				if (num < control.Bounds.Right)
				{
					num = control.Bounds.Right;
				}
				if (num2 < control.Bounds.Bottom)
				{
					num2 = control.Bounds.Bottom;
				}
			}
		}
		if (WantUpdateSize && IsVisible)
		{
			if (num != Width)
			{
				Width = num;
			}
			if (num2 != Height)
			{
				Height = num2;
			}
			WantUpdateSize = false;
		}
	}

	public virtual void OnPageChanged()
	{
		if (ServerSerial != 0)
		{
			WantUpdateSize = true;
		}
	}

	private void DrawDebug(UltimaBatcher2D batcher, int x, int y)
	{
		if (IsVisible && CUOEnviroment.Debug)
		{
			Vector3 hueVector = ShaderHueTranslator.GetHueVector(0);
			batcher.DrawRectangle(SolidColorTextureCache.GetTexture(Color.Green), x, y, Width, Height, hueVector);
		}
	}

	public void BringOnTop()
	{
		UIManager.MakeTopMostGump(this);
	}

	public void SetTooltip(string text, int maxWidth = 0)
	{
		ClearTooltip();
		if (!string.IsNullOrEmpty(text))
		{
			Tooltip = text;
			TooltipMaxLength = maxWidth;
		}
	}

	public void SetTooltip(uint entity)
	{
		ClearTooltip();
		Tooltip = entity;
	}

	public void ClearTooltip()
	{
		Tooltip = null;
	}

	public void SetKeyboardFocus()
	{
		if (AcceptKeyboardInput && !HasKeyboardFocus)
		{
			UIManager.KeyboardFocusControl = this;
		}
	}

	public void HitTest(int x, int y, ref Control res)
	{
		if (!IsVisible || !IsEnabled || IsDisposed)
		{
			return;
		}
		int parentX = ParentX;
		int parentY = ParentY;
		if (!Bounds.Contains(x - parentX - _offset.X, y - parentY - _offset.Y) || !Contains(x - X - parentX, y - Y - parentY))
		{
			return;
		}
		if (AcceptMouseInput && (res == null || res.Priority >= Priority))
		{
			res = this;
			OnHitTestSuccess(x, y, ref res);
		}
		for (int i = 0; i < Children.Count; i++)
		{
			Control control = Children[i];
			if (control.Page == 0 || control.Page == ActivePage)
			{
				control.HitTest(x, y, ref res);
			}
		}
	}

	public void HitTest(Point position, ref Control res)
	{
		HitTest(position.X, position.Y, ref res);
	}

	public virtual void OnHitTestSuccess(int x, int y, ref Control res)
	{
	}

	public Control GetFirstControlAcceptKeyboardInput()
	{
		if (_acceptKeyboardInput)
		{
			return this;
		}
		if (Children == null || Children.Count == 0)
		{
			return null;
		}
		foreach (Control child in Children)
		{
			Control firstControlAcceptKeyboardInput = child.GetFirstControlAcceptKeyboardInput();
			if (firstControlAcceptKeyboardInput != null)
			{
				return firstControlAcceptKeyboardInput;
			}
		}
		return null;
	}

	public virtual void Add(Control c, int page = 0)
	{
		c.Page = page;
		c.Parent = this;
		OnChildAdded();
	}

	public void Insert(int index, Control c, int page = 0)
	{
		c.Page = 0;
		c._parent?.Children.Remove(c);
		c._parent = this;
		Children.Insert(index, c);
		OnChildAdded();
	}

	public virtual void Remove(Control c)
	{
		if (c != null)
		{
			c.Parent = null;
			Children.Remove(c);
			OnChildRemoved();
		}
	}

	public virtual void Clear()
	{
		foreach (Control child in Children)
		{
			child.Dispose();
		}
	}

	public T[] GetControls<T>() where T : Control
	{
		return (from s in Children.OfType<T>()
			where !s.IsDisposed
			select s).ToArray();
	}

	public IEnumerable<T> FindControls<T>() where T : Control
	{
		return from s in Children.OfType<T>()
			where !s.IsDisposed
			select s;
	}

	public void InvokeMouseDown(Point position, MouseButtonType button)
	{
		int x = position.X - X - ParentX;
		int y = position.Y - Y - ParentY;
		OnMouseDown(x, y, button);
		this.MouseDown.Raise(new MouseEventArgs(x, y, button, ButtonState.Pressed), this);
	}

	public void InvokeMouseUp(Point position, MouseButtonType button)
	{
		int x = position.X - X - ParentX;
		int y = position.Y - Y - ParentY;
		OnMouseUp(x, y, button);
		this.MouseUp.Raise(new MouseEventArgs(x, y, button), this);
	}

	public void InvokeMouseCloseGumpWithRClick()
	{
		if (CanCloseWithRightClick)
		{
			CloseWithRightClick();
		}
	}

	public void InvokeMouseOver(Point position)
	{
		int x = position.X - X - ParentX;
		int y = position.Y - Y - ParentY;
		OnMouseOver(x, y);
		this.MouseOver.Raise(new MouseEventArgs(x, y), this);
	}

	public void InvokeMouseEnter(Point position)
	{
		int x = position.X - X - ParentX;
		int y = position.Y - Y - ParentY;
		OnMouseEnter(x, y);
		this.MouseEnter.Raise(new MouseEventArgs(x, y), this);
	}

	public void InvokeMouseExit(Point position)
	{
		int x = position.X - X - ParentX;
		int y = position.Y - Y - ParentY;
		OnMouseExit(x, y);
		this.MouseExit.Raise(new MouseEventArgs(x, y), this);
	}

	public bool InvokeMouseDoubleClick(Point position, MouseButtonType button)
	{
		int x = position.X - X - ParentX;
		int y = position.Y - Y - ParentY;
		bool num = OnMouseDoubleClick(x, y, button);
		MouseDoubleClickEventArgs mouseDoubleClickEventArgs = new MouseDoubleClickEventArgs(x, y, button);
		this.MouseDoubleClick.Raise(mouseDoubleClickEventArgs, this);
		return num | mouseDoubleClickEventArgs.Result;
	}

	public void InvokeTextInput(string c)
	{
		OnTextInput(c);
	}

	public void InvokeKeyDown(SDL.SDL_Keycode key, SDL.SDL_Keymod mod)
	{
		OnKeyDown(key, mod);
		KeyboardEventArgs e = new KeyboardEventArgs(key, mod, KeyboardEventType.Down);
		this.KeyDown?.Raise(e);
	}

	public void InvokeKeyUp(SDL.SDL_Keycode key, SDL.SDL_Keymod mod)
	{
		OnKeyUp(key, mod);
		KeyboardEventArgs e = new KeyboardEventArgs(key, mod, KeyboardEventType.Up);
		this.KeyUp?.Raise(e);
	}

	public void InvokeMouseWheel(MouseEventType delta)
	{
		OnMouseWheel(delta);
		this.MouseWheel.Raise(new MouseWheelEventArgs(delta), this);
	}

	public void InvokeDragBegin(Point position)
	{
		int x = position.X - X - ParentX;
		int y = position.Y - Y - ParentY;
		OnDragBegin(x, y);
		this.DragBegin.Raise(new MouseEventArgs(x, y, MouseButtonType.Left, ButtonState.Pressed), this);
	}

	public void InvokeDragEnd(Point position)
	{
		int x = position.X - X - ParentX;
		int y = position.Y - Y - ParentY;
		OnDragEnd(x, y);
		this.DragEnd.Raise(new MouseEventArgs(x, y, MouseButtonType.Left), this);
	}

	public void InvokeMove(int x, int y)
	{
		x = x - X - ParentX;
		y = y - Y - ParentY;
		OnMove(x, y);
	}

	protected virtual void OnMouseDown(int x, int y, MouseButtonType button)
	{
		_mouseIsDown = true;
		Parent?.OnMouseDown(X + x, Y + y, button);
	}

	protected virtual void OnMouseUp(int x, int y, MouseButtonType button)
	{
		_mouseIsDown = false;
		if (_attempToDrag)
		{
			_attempToDrag = false;
			InvokeDragEnd(new Point(x, y));
		}
		Parent?.OnMouseUp(X + x, Y + y, button);
		if (button == MouseButtonType.Right && !IsDisposed && !CanCloseWithRightClick && !ClassicUO.Input.Keyboard.Alt && !ClassicUO.Input.Keyboard.Shift && !ClassicUO.Input.Keyboard.Ctrl)
		{
			ContextMenu?.Show();
		}
	}

	protected virtual void OnMouseWheel(MouseEventType delta)
	{
		Parent?.OnMouseWheel(delta);
	}

	protected virtual void OnMouseOver(int x, int y)
	{
		if (_mouseIsDown && !_attempToDrag)
		{
			Point point = (ClassicUO.Input.Mouse.LButtonPressed ? ClassicUO.Input.Mouse.LDragOffset : (ClassicUO.Input.Mouse.MButtonPressed ? ClassicUO.Input.Mouse.MDragOffset : Point.Zero));
			if (Math.Abs(point.X) > 0 || Math.Abs(point.Y) > 0)
			{
				InvokeDragBegin(new Point(x, y));
				_attempToDrag = true;
			}
		}
		else
		{
			Parent?.OnMouseOver(X + x, Y + y);
		}
	}

	protected virtual void OnMouseEnter(int x, int y)
	{
	}

	protected virtual void OnMouseExit(int x, int y)
	{
		_attempToDrag = false;
	}

	protected virtual bool OnMouseDoubleClick(int x, int y, MouseButtonType button)
	{
		return Parent?.OnMouseDoubleClick(X + x, Y + y, button) ?? false;
	}

	protected virtual void OnDragBegin(int x, int y)
	{
	}

	protected virtual void OnDragEnd(int x, int y)
	{
		_mouseIsDown = false;
	}

	protected virtual void OnTextInput(string c)
	{
	}

	protected virtual void OnKeyDown(SDL.SDL_Keycode key, SDL.SDL_Keymod mod)
	{
		Parent?.OnKeyDown(key, mod);
	}

	protected virtual void OnKeyUp(SDL.SDL_Keycode key, SDL.SDL_Keymod mod)
	{
		Parent?.OnKeyUp(key, mod);
	}

	public virtual bool Contains(int x, int y)
	{
		return !IsDisposed;
	}

	protected virtual void OnMove(int x, int y)
	{
	}

	internal virtual void OnFocusEnter()
	{
		if (!IsFocused)
		{
			IsFocused = true;
			this.FocusEnter.Raise(this);
		}
	}

	internal virtual void OnFocusLost()
	{
		if (IsFocused)
		{
			IsFocused = false;
			this.FocusLost.Raise(this);
		}
	}

	protected virtual void OnChildAdded()
	{
	}

	protected virtual void OnChildRemoved()
	{
	}

	protected virtual void CloseWithRightClick()
	{
		if (!CanCloseWithRightClick)
		{
			return;
		}
		for (Control parent = Parent; parent != null; parent = parent.Parent)
		{
			if (!parent.CanCloseWithRightClick)
			{
				return;
			}
		}
		if (Parent == null)
		{
			Dispose();
		}
		else
		{
			Parent.CloseWithRightClick();
		}
	}

	public void KeyboardTabToNextFocus(Control c)
	{
		int num = Children.IndexOf(c);
		for (int i = num + 1; i < Children.Count; i++)
		{
			if (Children[i].AcceptKeyboardInput)
			{
				Children[i].SetKeyboardFocus();
				return;
			}
		}
		for (int j = 0; j < num; j++)
		{
			if (Children[j].AcceptKeyboardInput)
			{
				Children[j].SetKeyboardFocus();
				break;
			}
		}
	}

	public virtual void OnButtonClick(int buttonID)
	{
		Parent?.OnButtonClick(buttonID);
	}

	public virtual void OnKeyboardReturn(int textID, string text)
	{
		Parent?.OnKeyboardReturn(textID, text);
	}

	public virtual void ChangePage(int pageIndex)
	{
		Parent?.ChangePage(pageIndex);
	}

	public virtual void Dispose()
	{
		if (IsDisposed)
		{
			return;
		}
		foreach (Control child in Children)
		{
			child.Dispose();
		}
		Children.Clear();
		IsDisposed = true;
	}
}

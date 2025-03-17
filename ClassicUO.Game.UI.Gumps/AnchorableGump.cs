using ClassicUO.Configuration;
using ClassicUO.Game.Managers;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Gumps;

internal abstract class AnchorableGump : Gump
{
	private AnchorableGump _anchorCandidate;

	private int _prevX;

	private int _prevY;

	private const ushort LOCK_GRAPHIC = 2092;

	public ANCHOR_TYPE AnchorType { get; protected set; }

	public virtual int GroupMatrixWidth { get; protected set; }

	public virtual int GroupMatrixHeight { get; protected set; }

	public int WidthMultiplier { get; protected set; } = 1;

	public int HeightMultiplier { get; protected set; } = 1;

	public bool ShowLock
	{
		get
		{
			if (Keyboard.Alt)
			{
				return UIManager.AnchorManager[this] != null;
			}
			return false;
		}
	}

	protected AnchorableGump(uint local, uint server)
		: base(local, server)
	{
	}

	protected override void OnMove(int x, int y)
	{
		if (Keyboard.Alt && !ProfileManager.CurrentProfile.HoldAltToMoveGumps)
		{
			UIManager.AnchorManager.DetachControl(this);
		}
		else
		{
			UIManager.AnchorManager[this]?.UpdateLocation(this, base.X - _prevX, base.Y - _prevY);
		}
		_prevX = base.X;
		_prevY = base.Y;
		base.OnMove(x, y);
	}

	protected override void OnMouseDown(int x, int y, MouseButtonType button)
	{
		UIManager.AnchorManager[this]?.MakeTopMost();
		_prevX = base.X;
		_prevY = base.Y;
		base.OnMouseDown(x, y, button);
	}

	protected override void OnMouseOver(int x, int y)
	{
		if (!base.IsDisposed && UIManager.IsDragging && UIManager.DraggingControl == this)
		{
			_anchorCandidate = UIManager.AnchorManager.GetAnchorableControlUnder(this);
		}
		base.OnMouseOver(x, y);
	}

	protected override void OnDragEnd(int x, int y)
	{
		Attache();
		base.OnDragEnd(x, y);
	}

	public void TryAttacheToExist()
	{
		_anchorCandidate = UIManager.AnchorManager.GetAnchorableControlUnder(this);
		Attache();
	}

	private void Attache()
	{
		if (_anchorCandidate != null)
		{
			base.Location = UIManager.AnchorManager.GetCandidateDropLocation(this, _anchorCandidate);
			UIManager.AnchorManager.DropControl(this, _anchorCandidate);
			_anchorCandidate = null;
		}
	}

	protected override void OnMouseUp(int x, int y, MouseButtonType button)
	{
		if (button == MouseButtonType.Left && ShowLock && GumpsLoader.Instance.GetGumpTexture(2092u, out var bounds) != null && x >= base.Width - bounds.Width && x < base.Width && y >= 0 && y <= bounds.Height)
		{
			UIManager.AnchorManager.DetachControl(this);
		}
		base.OnMouseUp(x, y, button);
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		base.Draw(batcher, x, y);
		Vector3 hueVector;
		if (ShowLock)
		{
			hueVector = ShaderHueTranslator.GetHueVector(0);
			Rectangle bounds;
			Texture2D gumpTexture = GumpsLoader.Instance.GetGumpTexture(2092u, out bounds);
			if (gumpTexture != null)
			{
				if (UIManager.MouseOverControl != null && (UIManager.MouseOverControl == this || UIManager.MouseOverControl.RootParent == this))
				{
					hueVector.X = 34f;
					hueVector.Y = 1f;
				}
				batcher.Draw(gumpTexture, new Vector2(x + (base.Width - bounds.Width), y), bounds, hueVector);
			}
		}
		hueVector = ShaderHueTranslator.GetHueVector(0);
		if (_anchorCandidate != null)
		{
			Point candidateDropLocation = UIManager.AnchorManager.GetCandidateDropLocation(this, _anchorCandidate);
			if (candidateDropLocation != base.Location)
			{
				Texture2D texture = SolidColorTextureCache.GetTexture(Color.Silver);
				hueVector = ShaderHueTranslator.GetHueVector(0, partial: false, 0.5f);
				batcher.Draw(texture, new Rectangle(candidateDropLocation.X, candidateDropLocation.Y, base.Width, base.Height), hueVector);
				hueVector.Z = 0f;
				batcher.DrawRectangle(texture, candidateDropLocation.X, candidateDropLocation.Y, base.Width, base.Height, hueVector);
				batcher.DrawRectangle(texture, candidateDropLocation.X + 1, candidateDropLocation.Y + 1, base.Width - 2, base.Height - 2, hueVector);
			}
		}
		return true;
	}

	protected override void CloseWithRightClick()
	{
		if (UIManager.AnchorManager[this] == null || Keyboard.Alt || !ProfileManager.CurrentProfile.HoldDownKeyAltToCloseAnchored)
		{
			if (ProfileManager.CurrentProfile.CloseAllAnchoredGumpsInGroupWithRightClick)
			{
				UIManager.AnchorManager.DisposeAllControls(this);
			}
			base.CloseWithRightClick();
		}
	}

	public override void Dispose()
	{
		UIManager.AnchorManager.DetachControl(this);
		base.Dispose();
	}
}

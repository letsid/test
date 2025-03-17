using System;
using System.Runtime.CompilerServices;
using ClassicUO.Configuration;
using ClassicUO.Data;
using ClassicUO.Game.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.IO;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.GameObjects;

internal abstract class GameObject : BaseGameObject
{
	private Point _screenPosition;

	public int CurrentRenderIndex;

	public sbyte FoliageIndex = -1;

	public ushort Graphic;

	public ushort Hue;

	public Vector3 Offset;

	public Vector3 ShipOffset;

	public short PriorityZ;

	public GameObject TNext;

	public GameObject TPrevious;

	public byte UseInRender;

	public ushort X;

	public ushort Y;

	public sbyte Z;

	public GameObject RenderListNext;

	public byte AlphaHue;

	public bool AllowedToDraw = true;

	public ObjectHandlesStatus ObjectHandlesStatus;

	public Rectangle FrameInfo;

	protected bool IsFlipped;

	public bool IsDestroyed { get; protected set; }

	public bool IsPositionChanged { get; protected set; }

	public TextContainer TextContainer { get; private set; }

	public int Distance
	{
		get
		{
			if (World.Player == null)
			{
				return 65535;
			}
			if (this == World.Player)
			{
				return 0;
			}
			int x = X;
			int y = Y;
			if (this is Mobile mobile && mobile.Steps.Count != 0)
			{
				ref Mobile.Step reference = ref mobile.Steps.Back();
				x = reference.X;
				y = reference.Y;
			}
			int x2 = World.RangeSize.X;
			int y2 = World.RangeSize.Y;
			return Math.Max(Math.Abs(x - x2), Math.Abs(y - y2));
		}
	}

	public int DistanceToMouseCursor
	{
		get
		{
			if (World.Player == null)
			{
				return 65535;
			}
			if (this == World.Player)
			{
				return 0;
			}
			if (!(Client.Game.Scene is GameScene))
			{
				return 65535;
			}
			int num = 15;
			int num2 = 11;
			int x = SelectedObject.TranslatedMousePositionByViewport.X;
			int y = SelectedObject.TranslatedMousePositionByViewport.Y;
			int num3 = RealScreenPosition.X - x + num;
			int num4 = RealScreenPosition.Y - y - num2;
			return (int)Math.Sqrt(num3 * num3 + num4 * num4);
		}
	}

	public Vector3 CumulativeOffset => ShipOffset + Offset;

	public virtual void Update(double totalTime, double frameTime)
	{
	}

	public abstract bool CheckMouseSelection();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector2 GetScreenPosition()
	{
		return new Vector2((float)RealScreenPosition.X + CumulativeOffset.X, (float)RealScreenPosition.Y + (CumulativeOffset.Y - CumulativeOffset.Z));
	}

	public void AddToTile(int x, int y)
	{
		if (World.Map != null)
		{
			RemoveFromTile();
			if (!IsDestroyed)
			{
				World.Map.GetChunk(x, y)?.AddGameObject(this, x % 8, y % 8);
			}
		}
	}

	public void AddToTile()
	{
		AddToTile(X, Y);
	}

	public void RemoveFromTile()
	{
		if (TPrevious != null)
		{
			TPrevious.TNext = TNext;
		}
		if (TNext != null)
		{
			TNext.TPrevious = TPrevious;
		}
		TNext = null;
		TPrevious = null;
	}

	public virtual void UpdateGraphicBySeason()
	{
	}

	public void UpdateScreenPosition()
	{
		_screenPosition.X = (X - Y) * 22;
		_screenPosition.Y = (X + Y) * 22 - (Z << 2);
		IsPositionChanged = true;
		OnPositionChanged();
	}

	public void UpdateRealScreenPosition(int offsetX, int offsetY)
	{
		RealScreenPosition.X = _screenPosition.X - offsetX - 22;
		RealScreenPosition.Y = _screenPosition.Y - offsetY - 22;
		IsPositionChanged = false;
		UpdateTextCoordsV();
	}

	public void AddMessage(MessageType type, string message, TextType text_type)
	{
		AddMessage(type, message, ProfileManager.CurrentProfile.ChatFont, ProfileManager.CurrentProfile.SpeechHue, isunicode: true, text_type);
	}

	public virtual void UpdateTextCoordsV()
	{
		if (TextContainer == null)
		{
			return;
		}
		TextObject textObject = (TextObject)TextContainer.Items;
		while (textObject?.Next != null)
		{
			textObject = (TextObject)textObject.Next;
		}
		if (textObject == null)
		{
			return;
		}
		int num = 0;
		Point realScreenPosition = RealScreenPosition;
		Rectangle realArtBounds = ArtLoader.Instance.GetRealArtBounds(Graphic);
		realScreenPosition.Y -= realArtBounds.Height >> 1;
		realScreenPosition.X += (int)CumulativeOffset.X + 22;
		realScreenPosition.Y += (int)(CumulativeOffset.Y - CumulativeOffset.Z) + 44;
		realScreenPosition = Client.Game.Scene.Camera.WorldToScreen(realScreenPosition);
		while (textObject != null)
		{
			if (textObject.RenderedText != null && !textObject.RenderedText.IsDestroyed && (num != 0 || textObject.Time >= Time.Ticks))
			{
				textObject.OffsetY = num;
				num += textObject.RenderedText.Height;
				textObject.RealScreenPosition.X = realScreenPosition.X - (textObject.RenderedText.Width >> 1);
				textObject.RealScreenPosition.Y = realScreenPosition.Y - num;
			}
			textObject = (TextObject)textObject.Previous;
		}
		FixTextCoordinatesInScreen();
	}

	protected void FixTextCoordinatesInScreen()
	{
		if (this is Item item && SerialHelper.IsValid(item.Container))
		{
			return;
		}
		int num = 0;
		int num2 = 6;
		int num3 = num2 + ProfileManager.CurrentProfile.GameWindowSize.X - 6;
		int num4 = 0;
		for (TextObject textObject = (TextObject)TextContainer.Items; textObject != null; textObject = (TextObject)textObject.Next)
		{
			if (textObject.RenderedText != null && !textObject.RenderedText.IsDestroyed && textObject.RenderedText.Texture != null && textObject.Time >= Time.Ticks)
			{
				int x = textObject.RealScreenPosition.X;
				int num5 = x + textObject.RenderedText.Width;
				if (x < num2)
				{
					textObject.RealScreenPosition.X += num2 - x;
				}
				if (num5 > num3)
				{
					textObject.RealScreenPosition.X -= num5 - num3;
				}
				int y = textObject.RealScreenPosition.Y;
				if (y < num4 && num == 0)
				{
					num = num4 - y;
				}
				if (num != 0)
				{
					textObject.RealScreenPosition.Y += num;
				}
			}
		}
	}

	public void AddMessage(MessageType type, string text, byte font, ushort hue, bool isunicode, TextType text_type)
	{
		if (!string.IsNullOrEmpty(text))
		{
			TextObject msg = MessageManager.CreateMessage(text, hue, font, isunicode, type, text_type);
			AddMessage(msg);
		}
	}

	public void AddMessage(TextObject msg)
	{
		if (TextContainer == null)
		{
			TextContainer = new TextContainer();
		}
		msg.Owner = this;
		TextContainer.Add(msg);
		if (this is Item item && SerialHelper.IsValid(item.Container))
		{
			UpdateTextCoordsV();
			return;
		}
		IsPositionChanged = true;
		World.WorldTextManager.AddMessage(msg);
	}

	protected virtual void OnPositionChanged()
	{
	}

	protected virtual void OnDirectionChanged()
	{
	}

	public virtual void Destroy()
	{
		if (!IsDestroyed)
		{
			Next = null;
			Previous = null;
			RenderListNext = null;
			Clear();
			RemoveFromTile();
			TextContainer?.Clear();
			IsDestroyed = true;
			PriorityZ = 0;
			IsPositionChanged = false;
			Hue = 0;
			Offset = Vector3.Zero;
			ShipOffset = Vector3.Zero;
			CurrentRenderIndex = 0;
			UseInRender = 0;
			RealScreenPosition = Point.Zero;
			_screenPosition = Point.Zero;
			IsFlipped = false;
			Graphic = 0;
			ObjectHandlesStatus = ObjectHandlesStatus.NONE;
			FrameInfo = Rectangle.Empty;
		}
	}

	public static bool CanBeDrawn(ushort g)
	{
		switch (g)
		{
		case 1:
		case 8636:
			return false;
		case 40524:
		case 40548:
		case 40549:
		case 40573:
		{
			ref StaticTiles reference2 = ref TileDataLoader.Instance.StaticData[g];
			if (!reference2.IsBackground)
			{
				return !reference2.IsSurface;
			}
			return false;
		}
		default:
		{
			TileDataLoader instance = TileDataLoader.Instance;
			if (!instance.ShowNoDrawTiles && instance.NoDrawTiles.ContainsKey(g))
			{
				return false;
			}
			switch (g)
			{
			case 8600:
			case 8601:
			case 8602:
			case 8603:
			case 8604:
			case 8605:
			case 8606:
			case 8607:
			case 8608:
			case 8609:
			case 8610:
			case 8611:
			case 8612:
				return false;
			default:
			{
				if (g == 3941 && Client.Version < ClientVersion.CV_60144)
				{
					return true;
				}
				TileDataLoader instance2 = TileDataLoader.Instance;
				if (g < ((instance2 == null) ? ((int?)null) : instance2.StaticData?.Length))
				{
					ref StaticTiles reference = ref TileDataLoader.Instance.StaticData[g];
					if (!reference.IsNoDiagonal || (reference.IsAnimated && World.Player != null && World.Player.Race == RaceType.GARGOYLE))
					{
						return true;
					}
				}
				break;
			}
			case 25555:
				break;
			}
			return false;
		}
		}
	}

	public abstract bool Draw(UltimaBatcher2D batcher, int posX, int posY, float depth);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float CalculateDepthZ()
	{
		ushort x = X;
		int y = Y;
		int num = PriorityZ;
		if ((!(Offset.X > 0f) || !(Offset.Y < 0f)) && (!(Offset.X > 0f) || Offset.Y != 0f))
		{
			if (Offset.X > 0f && Offset.Y > 0f)
			{
				num += Math.Max(0, (int)Offset.Z);
			}
			else if (Offset.X != 0f || !(Offset.Y > 0f))
			{
				if (Offset.X < 0f && Offset.Y > 0f)
				{
					num += Math.Max(0, (int)Offset.Z);
				}
				else if ((!(Offset.X < 0f) || Offset.Y != 0f) && (!(Offset.X < 0f) || !(Offset.Y > 0f)) && Offset.X == 0f)
				{
					_ = Offset.Y;
					_ = 0f;
				}
			}
		}
		int num2 = ((Client.Game.Scene is GameScene gameScene) ? (gameScene.MinTile.X + gameScene.MinTile.Y) : 0);
		return (x + y - num2) * 100 + (127 + num);
	}

	public Rectangle GetOnScreenRectangle()
	{
		Rectangle empty = Rectangle.Empty;
		empty.X = (int)((float)(RealScreenPosition.X - FrameInfo.X + 22) + CumulativeOffset.X);
		empty.Y = (int)((float)(RealScreenPosition.Y - FrameInfo.Y + 22) + (CumulativeOffset.Y - CumulativeOffset.Z));
		empty.Width = FrameInfo.Width;
		empty.Height = FrameInfo.Height;
		return empty;
	}

	public virtual bool TransparentTest(int z)
	{
		return false;
	}

	protected static void DrawLand(UltimaBatcher2D batcher, ushort graphic, int x, int y, Vector3 hue, float depth)
	{
		Rectangle bounds;
		Texture2D landTexture = ArtLoader.Instance.GetLandTexture(graphic, out bounds);
		if (landTexture != null)
		{
			batcher.Draw(landTexture, new Vector2(x, y), bounds, hue, 0f, Vector2.Zero, 1f, SpriteEffects.None, depth + 0.5f);
		}
	}

	protected static void DrawLand(UltimaBatcher2D batcher, ushort graphic, int x, int y, ref UltimaBatcher2D.YOffsets yOffsets, ref Vector3 nTop, ref Vector3 nRight, ref Vector3 nLeft, ref Vector3 nBottom, Vector3 hue, float depth)
	{
		Rectangle bounds;
		Texture2D landTexture = TexmapsLoader.Instance.GetLandTexture(TileDataLoader.Instance.LandData[graphic].TexID, out bounds);
		if (landTexture != null)
		{
			batcher.DrawStretchedLand(landTexture, new Vector2(x, y), bounds, ref yOffsets, ref nTop, ref nRight, ref nLeft, ref nBottom, hue, depth + 0.5f);
		}
		else
		{
			DrawStatic(batcher, graphic, x, y, hue, depth);
		}
	}

	protected static void DrawStatic(UltimaBatcher2D batcher, ushort graphic, int x, int y, Vector3 hue, float depth)
	{
		Rectangle bounds;
		Texture2D staticTexture = ArtLoader.Instance.GetStaticTexture(graphic, out bounds);
		if (staticTexture != null)
		{
			ref UOFileIndex validRefEntry = ref ArtLoader.Instance.GetValidRefEntry(graphic + 16384);
			x -= validRefEntry.Width;
			y -= validRefEntry.Height;
			batcher.Draw(staticTexture, new Vector2(x, y), bounds, hue, 0f, Vector2.Zero, 1f, SpriteEffects.None, depth + 0.5f);
		}
	}

	protected static void DrawGump(UltimaBatcher2D batcher, ushort graphic, int x, int y, Vector3 hue, float depth)
	{
		Rectangle bounds;
		Texture2D gumpTexture = GumpsLoader.Instance.GetGumpTexture(graphic, out bounds);
		if (gumpTexture != null)
		{
			batcher.Draw(gumpTexture, new Vector2(x, y), bounds, hue, 0f, Vector2.Zero, 1f, SpriteEffects.None, depth + 0.5f);
		}
	}

	protected static void DrawStaticRotated(UltimaBatcher2D batcher, ushort graphic, int x, int y, float angle, Vector3 hue, float depth, bool centeredrotation = false)
	{
		Rectangle bounds;
		Texture2D staticTexture = ArtLoader.Instance.GetStaticTexture(graphic, out bounds);
		if (staticTexture != null)
		{
			ref UOFileIndex validRefEntry = ref ArtLoader.Instance.GetValidRefEntry(graphic + 16384);
			if (centeredrotation)
			{
				batcher.Draw(staticTexture, new Rectangle(x + (bounds.Width - validRefEntry.Width) / 2, y + (bounds.Height - validRefEntry.Height) / 2, bounds.Width, bounds.Height), bounds, hue, angle, new Vector2((float)(bounds.Width - validRefEntry.Width) / 2f, (float)(bounds.Height - validRefEntry.Height) / 2f), SpriteEffects.None, depth + 0.5f);
			}
			else
			{
				batcher.Draw(staticTexture, new Rectangle(x - validRefEntry.Width, y - validRefEntry.Height, bounds.Width, bounds.Height), bounds, hue, angle, Vector2.Zero, SpriteEffects.None, depth + 0.5f);
			}
		}
	}

	protected static void DrawStaticAnimated(UltimaBatcher2D batcher, ushort graphic, int x, int y, Vector3 hue, bool shadow, float depth)
	{
		graphic = (ushort)(graphic + ArtLoader.Instance.GetValidRefEntry(graphic + 16384).AnimOffset);
		Rectangle bounds;
		Texture2D staticTexture = ArtLoader.Instance.GetStaticTexture(graphic, out bounds);
		if (staticTexture != null)
		{
			ref UOFileIndex validRefEntry = ref ArtLoader.Instance.GetValidRefEntry(graphic + 16384);
			x -= validRefEntry.Width;
			y -= validRefEntry.Height;
			Vector2 position = new Vector2(x, y);
			if (shadow)
			{
				batcher.DrawShadow(staticTexture, position, bounds, flip: false, depth + 0.25f);
			}
			batcher.Draw(staticTexture, position, bounds, hue, 0f, Vector2.Zero, 1f, SpriteEffects.None, depth + 0.5f);
		}
	}
}

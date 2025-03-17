using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.IO;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.GameObjects;

internal sealed class Static : GameObject
{
	private static readonly QueuedPool<Static> _pool = new QueuedPool<Static>(38400, delegate(Static s)
	{
		s.IsDestroyed = false;
		s.AlphaHue = 0;
		s.FoliageIndex = 0;
	});

	public bool IsVegetation;

	public int Index;

	private int _canBeTransparent;

	public string Name => ItemData.Name;

	public ushort OriginalGraphic { get; private set; }

	public ushort OriginalColor { get; private set; }

	public ref StaticTiles ItemData => ref TileDataLoader.Instance.StaticData[Graphic];

	public int InnerTileIndex { get; set; }

	public static Static Create(ushort graphic, ushort hue, int index)
	{
		Static one = _pool.GetOne();
		ushort graphic2 = (one.OriginalGraphic = graphic);
		one.Graphic = graphic2;
		graphic2 = (one.OriginalColor = hue);
		one.Hue = graphic2;
		one.Index = index;
		if (one.ItemData.Height > 5 || one.ItemData.Height == 0)
		{
			one._canBeTransparent = 1;
		}
		else if (one.ItemData.IsRoof || (one.ItemData.IsSurface && one.ItemData.IsBackground) || one.ItemData.IsWall)
		{
			one._canBeTransparent = 1;
		}
		else if (one.ItemData.Height == 5 && one.ItemData.IsSurface && !one.ItemData.IsBackground)
		{
			one._canBeTransparent = 1;
		}
		else
		{
			one._canBeTransparent = 0;
		}
		return one;
	}

	public void SetGraphic(ushort g)
	{
		Graphic = g;
	}

	public void RestoreOriginalGraphic()
	{
		Graphic = OriginalGraphic;
		Hue = OriginalColor;
	}

	public override void UpdateGraphicBySeason()
	{
		SetGraphic(CustomSeasonManager.RemapStaticWorldObject(this, OriginalGraphic));
		Hue = CustomSeasonManager.RemapColor(this, OriginalGraphic, OriginalColor);
		AllowedToDraw = GameObject.CanBeDrawn(Graphic);
		IsVegetation = StaticFilters.IsVegetation(Graphic);
	}

	public override void Destroy()
	{
		if (!base.IsDestroyed)
		{
			base.Destroy();
			_pool.ReturnOne(this);
		}
	}

	public override bool TransparentTest(int z)
	{
		bool result = true;
		if (Z <= z - ItemData.Height)
		{
			result = false;
		}
		else if (z < Z && (_canBeTransparent & 0xFF) == 0)
		{
			result = false;
		}
		return result;
	}

	public override bool Draw(UltimaBatcher2D batcher, int posX, int posY, float depth)
	{
		if (!AllowedToDraw || base.IsDestroyed)
		{
			return false;
		}
		ushort num = Graphic;
		ushort hue = Hue;
		bool partial = ItemData.IsPartialHue;
		if (ProfileManager.CurrentProfile.HighlightGameObjects && SelectedObject.LastObject == this)
		{
			hue = ProfileManager.CurrentProfile.HighlightGameObjectsColor;
			partial = false;
		}
		else if (ProfileManager.CurrentProfile.NoColorObjectsOutOfRange && base.Distance > World.ClientViewRange)
		{
			hue = 907;
			partial = false;
		}
		else if (World.Player.IsDead && ProfileManager.CurrentProfile.EnableBlackWhiteEffect)
		{
			hue = 910;
			partial = false;
		}
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(hue, partial, (float)(int)AlphaHue / 255f);
		int index;
		bool flag = StaticFilters.IsTree(num, out index);
		if (flag && ProfileManager.CurrentProfile.TreeToStumps)
		{
			num = 3673;
		}
		GameObject.DrawStaticAnimated(batcher, num, posX, posY, hueVector, ProfileManager.CurrentProfile.ShadowsEnabled && ProfileManager.CurrentProfile.ShadowsStatics && (flag || ItemData.IsFoliage || StaticFilters.IsRock(num)), depth + (float)InnerTileIndex / 100f);
		if (ItemData.IsLight)
		{
			Client.Game.GetScene<GameScene>().AddLight(this, this, posX + 22, posY + 22);
		}
		return true;
	}

	public override bool CheckMouseSelection()
	{
		if (SelectedObject.Object != this && (FoliageIndex == -1 || Client.Game.GetScene<GameScene>().FoliageIndex != FoliageIndex))
		{
			ushort num = Graphic;
			if (StaticFilters.IsTree(num, out var _) && ProfileManager.CurrentProfile.TreeToStumps)
			{
				num = 3673;
			}
			ref UOFileIndex validRefEntry = ref ArtLoader.Instance.GetValidRefEntry(num + 16384);
			Point realScreenPosition = RealScreenPosition;
			realScreenPosition.X -= validRefEntry.Width;
			realScreenPosition.Y -= validRefEntry.Height;
			return ArtLoader.Instance.PixelCheck(num, SelectedObject.TranslatedMousePositionByViewport.X - realScreenPosition.X, SelectedObject.TranslatedMousePositionByViewport.Y - realScreenPosition.Y);
		}
		return false;
	}
}

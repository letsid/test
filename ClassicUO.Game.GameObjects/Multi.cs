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

internal sealed class Multi : GameObject
{
	private static readonly QueuedPool<Multi> _pool = new QueuedPool<Multi>(76800, delegate(Multi m)
	{
		m.IsDestroyed = false;
		m.AlphaHue = 0;
		m.FoliageIndex = 0;
		m.IsHousePreview = false;
		m.MultiOffsetX = (m.MultiOffsetY = (m.MultiOffsetZ = 0));
		m.IsCustom = false;
		m.State = (CUSTOM_HOUSE_MULTI_OBJECT_FLAGS)0;
		m.IsMovable = false;
		m.Offset = Vector3.Zero;
	});

	private ushort _originalGraphic;

	public bool IsCustom;

	public bool IsVegetation;

	public int MultiOffsetX;

	public int MultiOffsetY;

	public int MultiOffsetZ;

	public bool IsMovable;

	public CUSTOM_HOUSE_MULTI_OBJECT_FLAGS State;

	private int _canBeTransparent;

	public bool IsHousePreview;

	public string Name => ItemData.Name;

	public ref StaticTiles ItemData => ref TileDataLoader.Instance.StaticData[Graphic];

	public static Multi Create(ushort graphic)
	{
		Multi one = _pool.GetOne();
		one.Graphic = (one._originalGraphic = graphic);
		one.UpdateGraphicBySeason();
		one.AllowedToDraw = GameObject.CanBeDrawn(one.Graphic);
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

	public override void UpdateGraphicBySeason()
	{
		Graphic = CustomSeasonManager.RemapStaticWorldObject(this, _originalGraphic);
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
		ushort hue = Hue;
		if (State != 0)
		{
			if ((State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_IGNORE_IN_RENDER) != 0)
			{
				return false;
			}
			if ((State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_INCORRECT_PLACE) != 0)
			{
				hue = 43;
			}
			if ((State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_TRANSPARENT) != 0)
			{
				AlphaHue = 192;
			}
		}
		ushort graphic = Graphic;
		bool partial = ItemData.IsPartialHue;
		Profile currentProfile = ProfileManager.CurrentProfile;
		if (currentProfile.HighlightGameObjects && SelectedObject.LastObject == this)
		{
			hue = ProfileManager.CurrentProfile.HighlightGameObjectsColor;
			partial = false;
		}
		else if (currentProfile.NoColorObjectsOutOfRange && base.Distance > World.ClientViewRange)
		{
			hue = 907;
			partial = false;
		}
		else if (World.Player.IsDead && currentProfile.EnableBlackWhiteEffect)
		{
			hue = 910;
			partial = false;
		}
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(hue, partial, (float)(int)AlphaHue / 255f);
		if (IsHousePreview)
		{
			hueVector.Z *= 0.5f;
		}
		posX += (int)base.CumulativeOffset.X;
		posY += (int)(base.CumulativeOffset.Y + base.CumulativeOffset.Z);
		GameObject.DrawStaticAnimated(batcher, graphic, posX, posY, hueVector, shadow: false, depth);
		if (ItemData.IsLight)
		{
			Client.Game.GetScene<GameScene>().AddLight(this, this, posX + 22, posY + 22);
		}
		return true;
	}

	public override bool CheckMouseSelection()
	{
		if (SelectedObject.Object != this && !IsHousePreview && (FoliageIndex == -1 || Client.Game.GetScene<GameScene>().FoliageIndex != FoliageIndex))
		{
			if (State != 0 && (State & (CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_IGNORE_IN_RENDER | CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_PREVIEW)) != 0)
			{
				return false;
			}
			ref UOFileIndex validRefEntry = ref ArtLoader.Instance.GetValidRefEntry(Graphic + 16384);
			Point realScreenPosition = RealScreenPosition;
			realScreenPosition.X -= validRefEntry.Width;
			realScreenPosition.Y -= validRefEntry.Height;
			return ArtLoader.Instance.PixelCheck(Graphic, SelectedObject.TranslatedMousePositionByViewport.X - realScreenPosition.X, SelectedObject.TranslatedMousePositionByViewport.Y - realScreenPosition.Y);
		}
		return false;
	}
}

using System;
using System.Collections.Generic;
using ClassicUO.Configuration;
using ClassicUO.Game.GameObjects;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.Managers;

internal class WorldTextManager : TextRenderer
{
	private readonly Dictionary<uint, OverheadDamage> _damages = new Dictionary<uint, OverheadDamage>();

	private readonly List<Tuple<uint, uint>> _subst = new List<Tuple<uint, uint>>();

	private readonly List<uint> _toRemoveDamages = new List<uint>();

	public override void Update(double totalTime, double frameTime)
	{
		base.Update(totalTime, frameTime);
		UpdateDamageOverhead(totalTime, frameTime);
		if (_toRemoveDamages.Count <= 0)
		{
			return;
		}
		foreach (uint toRemoveDamage in _toRemoveDamages)
		{
			_damages.Remove(toRemoveDamage);
		}
		_toRemoveDamages.Clear();
	}

	public override void Draw(UltimaBatcher2D batcher, int startX, int startY, int renderIndex, bool isGump = false)
	{
		base.Draw(batcher, startX, startY, renderIndex, isGump);
		foreach (KeyValuePair<uint, OverheadDamage> damage in _damages)
		{
			Entity entity = World.Get(damage.Key);
			if (entity == null || entity.IsDestroyed)
			{
				uint num = damage.Key | 0x80000000u;
				if (!World.CorpseManager.Exists(0u, num))
				{
					continue;
				}
				Item corpseObject = World.CorpseManager.GetCorpseObject(num);
				if (corpseObject != null && corpseObject != damage.Value.Parent)
				{
					_subst.Add(Tuple.Create(damage.Key, corpseObject.Serial));
					damage.Value.SetParent(corpseObject);
				}
			}
			damage.Value.Draw(batcher);
		}
		foreach (Mobile value in World.Mobiles.Values)
		{
			if (value._lastTextActivity != uint.MaxValue && ClassicUO.Time.Ticks - value._lastTextActivity <= 4000)
			{
				Point realScreenPosition = value.RealScreenPosition;
				if (value.IsGargoyle && value.IsFlying)
				{
					realScreenPosition.Y -= 22;
				}
				else if (!value.IsMounted)
				{
					realScreenPosition.Y += 22;
				}
				AnimationsLoader.Instance.GetAnimationDimensions(value.AnimIndex, value.GetGraphicForAnimation(), 0, 0, value.IsMounted, 0, out var _, out var centerY, out var width, out var height);
				realScreenPosition.X += (int)value.CumulativeOffset.X + 22;
				width = ProfileManager.CurrentProfile.ChatActivitiySize;
				int num2;
				string text;
				switch (width)
				{
				case 0:
					num2 = 8;
					text = ".";
					break;
				case 1:
					num2 = 2;
					text = "•";
					break;
				default:
					num2 = 2;
					text = "•";
					break;
				}
				realScreenPosition.Y += (int)(value.CumulativeOffset.Y - value.CumulativeOffset.Z - (float)(height + centerY + num2));
				realScreenPosition = Client.Game.Scene.Camera.WorldToScreen(realScreenPosition);
				bool flag = false;
				if (ClassicUO.Time.Ticks - value._lastTextActivityPointCountChange > 500)
				{
					flag = true;
					value._lastTextActivityPointCountChange = ClassicUO.Time.Ticks;
					value._lastTextActivityPointCount = value._lastTextActivityPointCount % 3 + 1;
				}
				if (flag || value._lastTextActivityPointRenderedText == null)
				{
					int lastTextActivityPointCount = (int)value._lastTextActivityPointCount;
					string text2 = "";
					for (int i = 0; i < lastTextActivityPointCount; i++)
					{
						text2 += text;
					}
					value._lastTextActivityPointRenderedText?.Destroy();
					value._lastTextActivityPointRenderedText = RenderedText.Create(text2, ProfileManager.CurrentProfile.SpeechHue, byte.MaxValue, isunicode: true, FontStyle.None, TEXT_ALIGN_TYPE.TS_LEFT, 0, 30);
				}
				Vector3 hueVector = ShaderHueTranslator.GetHueVector(0, partial: false, 1f);
				if (value._lastTextActivityPointRenderedText != null && value._lastTextActivityPointRenderedText.Texture != null)
				{
					batcher.Draw(value._lastTextActivityPointRenderedText.Texture, new Vector2(realScreenPosition.X - 5, realScreenPosition.Y - 15), hueVector);
				}
			}
			else
			{
				value._lastTextActivityPointRenderedText?.Destroy();
				value._lastTextActivityPointCount = 1u;
				value._lastTextActivityPointRenderedText = null;
			}
		}
	}

	private void UpdateDamageOverhead(double totalTime, double frameTime)
	{
		if (_subst.Count != 0)
		{
			foreach (Tuple<uint, uint> item in _subst)
			{
				if (_damages.TryGetValue(item.Item1, out var value))
				{
					_damages.Remove(item.Item1);
					_damages[item.Item2] = value;
				}
			}
			_subst.Clear();
		}
		foreach (KeyValuePair<uint, OverheadDamage> damage in _damages)
		{
			damage.Value.Update();
			if (damage.Value.IsEmpty)
			{
				_toRemoveDamages.Add(damage.Key);
			}
		}
	}

	internal void AddDamage(uint obj, int dmg)
	{
		if (!_damages.TryGetValue(obj, out var value) || value == null)
		{
			value = new OverheadDamage(World.Get(obj));
			_damages[obj] = value;
		}
		value.Add(dmg);
	}

	public override void Clear()
	{
		if (_toRemoveDamages.Count > 0)
		{
			foreach (uint toRemoveDamage in _toRemoveDamages)
			{
				_damages.Remove(toRemoveDamage);
			}
			_toRemoveDamages.Clear();
		}
		_subst.Clear();
		base.Clear();
	}
}

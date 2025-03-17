using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI;

internal class Tooltip
{
	private uint _hash;

	private uint _lastHoverTime;

	private int _maxWidth;

	private RenderedText _renderedText;

	private string _textHTML;

	public string Text { get; protected set; }

	public bool IsEmpty => Text == null;

	public uint Serial { get; private set; }

	public bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		if (SerialHelper.IsValid(Serial) && World.OPL.TryGetRevision(Serial, out var revision) && _hash != revision)
		{
			_hash = revision;
			Text = ReadProperties(Serial, out _textHTML);
		}
		if (string.IsNullOrEmpty(Text))
		{
			return false;
		}
		if (_lastHoverTime > Time.Ticks)
		{
			return false;
		}
		byte b = 1;
		float num = 0.7f;
		ushort hue = ushort.MaxValue;
		float num2 = 1f;
		if (ProfileManager.CurrentProfile != null)
		{
			b = ProfileManager.CurrentProfile.TooltipFont;
			num = (float)ProfileManager.CurrentProfile.TooltipBackgroundOpacity / 100f;
			if (float.IsNaN(num))
			{
				num = 0f;
			}
			num2 = (float)ProfileManager.CurrentProfile.TooltipDisplayZoom / 100f;
		}
		FontsLoader.Instance.SetUseHTML(value: true);
		FontsLoader.Instance.RecalculateWidthByInfo = true;
		if (_renderedText == null)
		{
			byte font = b;
			_renderedText = RenderedText.Create(null, hue, font, isunicode: true, FontStyle.BlackBorder, TEXT_ALIGN_TYPE.TS_CENTER, 0, 5, isHTML: true, recalculateWidthByInfo: true);
		}
		if (_renderedText.Text != Text)
		{
			if (_maxWidth == 0)
			{
				int num3 = FontsLoader.Instance.GetWidthUnicode(b, Text);
				if (num3 > 600)
				{
					num3 = 600;
				}
				num3 = FontsLoader.Instance.GetWidthExUnicode(b, Text, num3, TEXT_ALIGN_TYPE.TS_CENTER, 8);
				if (num3 > 600)
				{
					num3 = 600;
				}
				_renderedText.MaxWidth = num3;
			}
			else
			{
				_renderedText.MaxWidth = _maxWidth;
			}
			_renderedText.Font = b;
			_renderedText.Hue = hue;
			_renderedText.Text = _textHTML;
		}
		FontsLoader.Instance.RecalculateWidthByInfo = false;
		FontsLoader.Instance.SetUseHTML(value: false);
		if (_renderedText.Texture == null || _renderedText.Texture.IsDisposed)
		{
			return false;
		}
		int num4 = _renderedText.Width + 8;
		int num5 = _renderedText.Height + 8;
		if (x < 0)
		{
			x = 0;
		}
		else if (x > Client.Game.Window.ClientBounds.Width - num4)
		{
			x = Client.Game.Window.ClientBounds.Width - num4;
		}
		if (y < 0)
		{
			y = 0;
		}
		else if (y > Client.Game.Window.ClientBounds.Height - num5)
		{
			y = Client.Game.Window.ClientBounds.Height - num5;
		}
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(0, partial: false, num);
		batcher.Draw(SolidColorTextureCache.GetTexture(Color.Black), new Rectangle(x - 4, y - 2, (int)((float)num4 * num2), (int)((float)num5 * num2)), hueVector);
		batcher.DrawRectangle(SolidColorTextureCache.GetTexture(Color.Gray), x - 4, y - 2, (int)((float)num4 * num2), (int)((float)num5 * num2), hueVector);
		batcher.Draw(_renderedText.Texture, new Rectangle(x + 3, y + 3, (int)((float)_renderedText.Texture.Width * num2), (int)((float)_renderedText.Texture.Height * num2)), null, Vector3.UnitZ);
		return true;
	}

	public void Clear()
	{
		Serial = 0u;
		_hash = 0u;
		string textHTML = (Text = null);
		_textHTML = textHTML;
		_maxWidth = 0;
	}

	public void SetGameObject(uint serial)
	{
		if (Serial == 0 || serial != Serial)
		{
			uint revision = 0u;
			if (Serial == 0 || Serial != serial || (World.OPL.TryGetRevision(Serial, out var revision2) && World.OPL.TryGetRevision(serial, out revision) && revision2 != revision))
			{
				_maxWidth = 0;
				Serial = serial;
				_hash = revision;
				Text = ReadProperties(serial, out _textHTML);
				_lastHoverTime = (uint)(Time.Ticks + ((ProfileManager.CurrentProfile != null) ? ProfileManager.CurrentProfile.TooltipDelayBeforeDisplay : 250));
			}
		}
	}

	private string ReadProperties(uint serial, out string htmltext)
	{
		bool flag = false;
		string text = null;
		htmltext = string.Empty;
		if (SerialHelper.IsValid(serial) && World.OPL.TryGetNameAndData(serial, out var name, out var data))
		{
			ValueStringBuilder valueStringBuilder = default(ValueStringBuilder);
			ValueStringBuilder valueStringBuilder2 = default(ValueStringBuilder);
			if (!string.IsNullOrEmpty(name))
			{
				if (SerialHelper.IsItem(serial))
				{
					valueStringBuilder.Append("<basefont color=\"yellow\">");
					flag = true;
				}
				else
				{
					Mobile mobile = World.Mobiles.Get(serial);
					if (mobile != null)
					{
						valueStringBuilder.Append(Notoriety.GetHTMLHue(mobile.NotorietyFlag));
						flag = true;
					}
				}
				valueStringBuilder2.Append(name);
				valueStringBuilder.Append(name);
				if (flag)
				{
					valueStringBuilder.Append("<basefont color=\"#FFFFFFFF\">");
				}
			}
			if (!string.IsNullOrEmpty(data))
			{
				valueStringBuilder2.Append('\n');
				valueStringBuilder2.Append(data);
				valueStringBuilder.Append('\n');
				valueStringBuilder.Append(data);
			}
			htmltext = valueStringBuilder.ToString();
			text = valueStringBuilder2.ToString();
			valueStringBuilder2.Dispose();
			valueStringBuilder.Dispose();
		}
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		return null;
	}

	public void SetText(string text, int maxWidth = 0)
	{
		if (ProfileManager.CurrentProfile == null || ProfileManager.CurrentProfile.UseTooltip)
		{
			_maxWidth = maxWidth;
			Serial = 0u;
			Text = (_textHTML = text);
			_lastHoverTime = (uint)(Time.Ticks + ((ProfileManager.CurrentProfile != null) ? ProfileManager.CurrentProfile.TooltipDelayBeforeDisplay : 250));
		}
	}
}

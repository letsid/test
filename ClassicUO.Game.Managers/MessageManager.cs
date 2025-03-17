using System;
using System.Collections.Generic;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Utility;

namespace ClassicUO.Game.Managers;

internal static class MessageManager
{
	public static PromptData PromptData { get; set; }

	public static event EventHandler<MessageEventArgs> MessageReceived;

	public static event EventHandler<MessageEventArgs> LocalizedMessageReceived;

	public static void HandleMessage(Entity parent, string text, string name, ushort hue, MessageType type, byte font, TextType textType, bool unicode = false, string lang = null)
	{
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		Profile currentProfile = ProfileManager.CurrentProfile;
		switch (type)
		{
		case MessageType.Guild:
			if (currentProfile.IgnoreGuildMessages)
			{
				return;
			}
			break;
		case MessageType.Alliance:
			if (currentProfile.IgnoreAllianceMessages)
			{
				return;
			}
			break;
		case MessageType.Spell:
		{
			if (!string.IsNullOrEmpty(text) && SpellDefinition.WordToTargettype.TryGetValue(text, out var value))
			{
				if (currentProfile != null && currentProfile.EnabledSpellFormat && !string.IsNullOrWhiteSpace(currentProfile.SpellDisplayFormat))
				{
					ValueStringBuilder valueStringBuilder = new ValueStringBuilder(currentProfile.SpellDisplayFormat.AsSpan());
					valueStringBuilder.Replace("{power}".AsSpan(), value.PowerWords.AsSpan());
					valueStringBuilder.Replace("{spell}".AsSpan(), value.Name.AsSpan());
					text = valueStringBuilder.ToString().Trim();
					valueStringBuilder.Dispose();
				}
				if (currentProfile != null && currentProfile.EnabledSpellHue)
				{
					hue = ((value.TargetType == TargetType.Beneficial) ? currentProfile.BeneficHue : ((value.TargetType != TargetType.Harmful) ? currentProfile.NeutralHue : currentProfile.HarmfulHue));
				}
			}
			goto default;
		}
		default:
		{
			if (parent == null || (IgnoreManager.IgnoredCharsList.Contains(parent.Name) && type != MessageType.Spell))
			{
				break;
			}
			TextObject textObject = CreateMessage(text, hue, font, unicode, type, textType);
			textObject.Owner = parent;
			if (parent is Item { OnGround: false } item)
			{
				textObject.X = DelayedObjectClickManager.X;
				textObject.Y = DelayedObjectClickManager.Y;
				textObject.IsTextGump = true;
				bool flag = false;
				for (LinkedListNode<Gump> linkedListNode = UIManager.Gumps.Last; linkedListNode != null; linkedListNode = linkedListNode.Previous)
				{
					Control value2 = linkedListNode.Value;
					if (!value2.IsDisposed)
					{
						Control control = value2;
						if (!(control is PaperDollGump paperDollGump))
						{
							if (!(control is ContainerGump containerGump))
							{
								if (control is TradingGump tradingGump && (tradingGump.ID1 == item.Container || tradingGump.ID2 == item.Container))
								{
									tradingGump.AddText(textObject);
									flag = true;
								}
							}
							else if (value2.LocalSerial == item.Container)
							{
								containerGump.AddText(textObject);
								flag = true;
							}
						}
						else if (value2.LocalSerial == item.Container)
						{
							paperDollGump.AddText(textObject);
							flag = true;
						}
					}
					if (flag)
					{
						break;
					}
				}
			}
			parent.AddMessage(textObject);
			break;
		}
		case MessageType.System:
		case MessageType.Command:
		case MessageType.Encoded:
		case MessageType.Party:
			break;
		}
		MessageManager.MessageReceived.Raise(new MessageEventArgs(parent, text, name, hue, type, font, textType, unicode, lang), parent);
	}

	public static void OnLocalizedMessage(Entity entity, MessageEventArgs args)
	{
		MessageManager.LocalizedMessageReceived.Raise(args, entity);
	}

	public static TextObject CreateMessage(string msg, ushort hue, byte font, bool isunicode, MessageType type, TextType textType)
	{
		bool flag = false;
		if (ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.OverrideAllFonts && font != 8)
		{
			font = ProfileManager.CurrentProfile.ChatFont;
			flag = !isunicode;
			isunicode = true;
		}
		int num = (isunicode ? FontsLoader.Instance.GetWidthUnicode(font, msg) : FontsLoader.Instance.GetWidthASCII(font, msg));
		num = ((num > 200) ? (isunicode ? FontsLoader.Instance.GetWidthExUnicode(font, msg, 190, TEXT_ALIGN_TYPE.TS_LEFT, 8) : FontsLoader.Instance.GetWidthExASCII(font, msg, 200, TEXT_ALIGN_TYPE.TS_LEFT, 8)) : 0);
		ushort num2 = (ushort)(hue & 0x3FFF);
		if (num2 != 0)
		{
			if (num2 >= 3000)
			{
				num2 = 1;
			}
			num2 |= (ushort)(hue & 0xC000);
		}
		else
		{
			num2 = (ushort)(hue & 0x8000);
		}
		TextObject textObject = TextObject.Create();
		textObject.Alpha = byte.MaxValue;
		textObject.Type = type;
		textObject.Hue = num2;
		if (!isunicode && textType == TextType.OBJECT)
		{
			num2 = 32767;
		}
		textObject.RenderedText = RenderedText.Create(msg, num2, font, isunicode, (FontStyle)(8 | (flag ? 1024 : 0)), TEXT_ALIGN_TYPE.TS_LEFT, num, 30, isHTML: false, recalculateWidthByInfo: false, textType == TextType.OBJECT);
		textObject.Time = CalculateTimeToLive(textObject.RenderedText);
		textObject.RenderedText.Hue = textObject.Hue;
		return textObject;
	}

	private static long CalculateTimeToLive(RenderedText rtext)
	{
		Profile currentProfile = ProfileManager.CurrentProfile;
		if (currentProfile == null)
		{
			return 0L;
		}
		long num2;
		if (currentProfile.ScaleSpeechDelay)
		{
			int num = currentProfile.SpeechDelay;
			if (num < 10)
			{
				num = 10;
			}
			num2 = (long)((float)(4000 * rtext.LinesCount * num) / 100f);
		}
		else
		{
			long num3 = 5497558140000L * currentProfile.SpeechDelay >> 32 >> 5;
			num2 = (num3 >> 31) + num3;
		}
		return num2 + Time.Ticks;
	}
}

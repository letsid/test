using System;
using System.Collections.Generic;
using System.Linq;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using ClassicUO.Utility.Logging;
using Microsoft.Xna.Framework;
using SDL2;

namespace ClassicUO.Game.UI.Gumps;

internal class InspectorGump : Gump
{
	private const int WIDTH = 500;

	private const int HEIGHT = 400;

	private readonly GameObject _obj;

	public InspectorGump(GameObject obj)
		: base(0u, 0u)
	{
		base.X = 200;
		base.Y = 100;
		_obj = obj;
		CanMove = true;
		AcceptMouseInput = false;
		base.CanCloseWithRightClick = true;
		Add(new BorderControl(0, 0, 500, 400, 4));
		Add(new GumpPicTiled(4, 4, 492, 392, 2624)
		{
			Alpha = 0.5f
		});
		Add(new GumpPicTiled(4, 4, 492, 392, 2624)
		{
			Alpha = 0.5f
		});
		Label label = new Label(ResGumps.ObjectInformation, isunicode: true, 1153, 0, 3);
		label.X = 20;
		label.Y = 10;
		Add(label);
		Add(new Line(20, 30, 450, 1, uint.MaxValue));
		Add(new NiceButton(385, 5, 100, 25, ButtonAction.Activate, ResGumps.Dump)
		{
			ButtonParameter = 0
		});
		ScrollArea scrollArea = new ScrollArea(20, 35, 460, 355, normalScrollbar: true)
		{
			AcceptMouseInput = true
		};
		Add(scrollArea);
		DataBox dataBox = new DataBox(0, 0, 1, 1)
		{
			WantUpdateSize = true
		};
		scrollArea.Add(dataBox);
		Dictionary<string, string> gameObjectProperties = GetGameObjectProperties(obj);
		if (gameObjectProperties == null)
		{
			return;
		}
		int num = 5;
		int num2 = 5;
		foreach (KeyValuePair<string, string> item in gameObjectProperties.OrderBy((KeyValuePair<string, string> s) => s.Key))
		{
			Label label2 = new Label(item.Key + ":", isunicode: true, 33, 0, 1, FontStyle.BlackBorder);
			label2.X = num;
			label2.Y = num2;
			Label label3 = label2;
			dataBox.Add(label3);
			int height = label3.Height;
			Label label4 = new Label(item.Value, isunicode: true, 1153, 235, 1, FontStyle.BlackBorder);
			label4.X = num + 200;
			label4.Y = num2;
			label4.AcceptMouseInput = true;
			label3 = label4;
			label3.MouseUp += OnLabelClick;
			if (label3.Height > 0)
			{
				height = label3.Height;
			}
			dataBox.Add(label3);
			dataBox.Add(new Line(num, num2 + height + 2, 435, 1, Color.Gray.PackedValue));
			num2 += height + 4;
		}
	}

	public override void OnButtonClick(int buttonID)
	{
		if (buttonID != 0)
		{
			return;
		}
		Dictionary<string, string> gameObjectProperties = GetGameObjectProperties(_obj);
		if (gameObjectProperties == null)
		{
			return;
		}
		using LogFile logFile = new LogFile(CUOEnviroment.ExecutablePath, "dump_gameobject.txt");
		logFile.Write("###################################################");
		logFile.Write($"CUO version: {CUOEnviroment.Version}");
		logFile.Write($"OBJECT TYPE: {_obj.GetType()}");
		foreach (KeyValuePair<string, string> item in gameObjectProperties.OrderBy((KeyValuePair<string, string> s) => s.Key))
		{
			logFile.Write(item.Key + " = " + item.Value);
		}
		logFile.Write("###################################################");
		logFile.Write("");
	}

	private void OnLabelClick(object sender, EventArgs e)
	{
		Label label = (Label)sender;
		if (label != null)
		{
			SDL.SDL_SetClipboardText(label.Text);
		}
	}

	private Dictionary<string, string> GetGameObjectProperties(GameObject obj)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary["Graphics"] = $"0x{obj.Graphic:X4}";
		dictionary["Hue"] = $"0x{obj.Hue:X4}";
		dictionary["Position"] = $"X={obj.X}, Y={obj.Y}, Z={obj.Z}";
		dictionary["PriorityZ"] = obj.PriorityZ.ToString();
		dictionary["Distance"] = obj.Distance.ToString();
		dictionary["AllowedToDraw"] = obj.AllowedToDraw.ToString();
		dictionary["AlphaHue"] = obj.AlphaHue.ToString();
		if (!(obj is Mobile mobile))
		{
			if (!(obj is Item item))
			{
				if (!(obj is Static @static))
				{
					if (!(obj is Multi multi))
					{
						if (obj is Land land)
						{
							dictionary["IsFlat"] = (!land.IsStretched).ToString();
							dictionary["NormalLeft"] = land.NormalLeft.ToString();
							dictionary["NormalRight"] = land.NormalRight.ToString();
							dictionary["NormalTop"] = land.NormalTop.ToString();
							dictionary["NormalBottom"] = land.NormalBottom.ToString();
							dictionary["MinZ"] = land.MinZ.ToString();
							dictionary["AvgZ"] = land.AverageZ.ToString();
							dictionary["YOffsets"] = land.YOffsets.ToString();
						}
					}
					else
					{
						dictionary["State"] = multi.State.ToString();
						dictionary["IsMovable"] = multi.IsMovable.ToString();
					}
				}
				else
				{
					dictionary["IsVegetation"] = @static.IsVegetation.ToString();
				}
			}
			else
			{
				dictionary["Serial"] = $"0x{item.Serial:X8}";
				dictionary["Flags"] = item.Flags.ToString();
				dictionary["HP"] = $"{item.Hits}/{item.HitsMax}";
				dictionary["IsCoins"] = item.IsCoin.ToString();
				dictionary["Amount"] = item.Amount.ToString();
				dictionary["Container"] = item.Container.ToString();
				dictionary["Layer"] = item.Layer.ToString();
				dictionary["Price"] = item.Price.ToString();
				dictionary["Direction"] = item.Direction.ToString();
				dictionary["IsMulti"] = item.IsMulti.ToString();
				dictionary["MultiGraphic"] = $"0x{item.MultiGraphic:X4}";
			}
		}
		else
		{
			dictionary["Serial"] = $"0x{mobile.Serial:X8}";
			dictionary["Flags"] = mobile.Flags.ToString();
			dictionary["Notoriety"] = mobile.NotorietyFlag.ToString();
			dictionary["Title"] = mobile.Title ?? string.Empty;
			dictionary["Name"] = mobile.Name ?? string.Empty;
			dictionary["HP"] = $"{mobile.Hits}/{mobile.HitsMax}";
			dictionary["Mana"] = $"{mobile.Mana}/{mobile.ManaMax}";
			dictionary["Stamina"] = $"{mobile.Stamina}/{mobile.StaminaMax}";
			dictionary["SpeedMode"] = mobile.SpeedMode.ToString();
			dictionary["Race"] = mobile.Race.ToString();
			dictionary["IsRenamable"] = mobile.IsRenamable.ToString();
			dictionary["Direction"] = mobile.Direction.ToString();
			dictionary["IsDead"] = mobile.IsDead.ToString();
			dictionary["IsDrivingABoat"] = mobile.IsDrivingBoat.ToString();
			dictionary["IsMounted"] = mobile.IsMounted.ToString();
		}
		return dictionary;
	}
}

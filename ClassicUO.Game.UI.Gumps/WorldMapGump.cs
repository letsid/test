using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using ClassicUO.Utility;
using ClassicUO.Utility.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDL2;

namespace ClassicUO.Game.UI.Gumps;

internal class WorldMapGump : ResizableGump
{
	internal class WMapMarker
	{
		public string Name { get; set; }

		public int X { get; set; }

		public int Y { get; set; }

		public int MapId { get; set; }

		public Color Color { get; set; }

		public Texture2D MarkerIcon { get; set; }

		public string MarkerIconName { get; set; }

		public int ZoomIndex { get; set; }

		public string ColorName { get; set; }
	}

	internal class WMapMarkerFile
	{
		public string Name { get; set; }

		public string FullPath { get; set; }

		public List<WMapMarker> Markers { get; set; }

		public bool Hidden { get; set; }

		public bool IsEditable { get; set; }
	}

	private class CurLoader
	{
		public unsafe static Texture2D CreateTextureFromICO_Cur(Stream stream)
		{
			byte[] array = ArrayPool<byte>.Shared.Rent((int)stream.Length);
			try
			{
				stream.Read(array, 0, array.Length);
				StackDataReader stackDataReader = new StackDataReader(array.AsSpan(0, (int)stream.Length));
				int num = 0;
				uint num2 = 0u;
				uint* ptr = stackalloc uint[256];
				stackDataReader.ReadUInt16LE();
				stackDataReader.ReadUInt16LE();
				ushort num3 = stackDataReader.ReadUInt16LE();
				for (int i = 0; i < num3; i++)
				{
					stackDataReader.ReadUInt8();
					int num4 = stackDataReader.ReadUInt8();
					int num5 = stackDataReader.ReadUInt8();
					stackDataReader.ReadUInt8();
					stackDataReader.ReadUInt16LE();
					stackDataReader.ReadUInt16LE();
					stackDataReader.ReadUInt32LE();
					uint num6 = stackDataReader.ReadUInt32LE();
					if (num4 == 0)
					{
						num4 = 256;
					}
					if (num5 == 0)
					{
						num5 = 256;
					}
					if (num5 > num)
					{
						num = num5;
						num2 = num6;
					}
				}
				stackDataReader.Seek(num2);
				if (stackDataReader.ReadUInt32LE() == 40)
				{
					uint num7 = stackDataReader.ReadUInt32LE();
					uint num8 = stackDataReader.ReadUInt32LE();
					stackDataReader.ReadUInt16LE();
					ushort num9 = stackDataReader.ReadUInt16LE();
					uint num10 = stackDataReader.ReadUInt32LE();
					stackDataReader.ReadUInt32LE();
					stackDataReader.ReadUInt32LE();
					stackDataReader.ReadUInt32LE();
					uint num11 = stackDataReader.ReadUInt32LE();
					stackDataReader.ReadUInt32LE();
					if (num10 == 0)
					{
						int num12;
						switch (num9)
						{
						case 1:
						case 4:
							num12 = num9;
							num9 = 8;
							break;
						case 8:
							num12 = 8;
							break;
						case 32:
							num12 = 0;
							break;
						default:
							return null;
						}
						num8 >>= 1;
						SDL.SDL_Surface* ptr2 = (SDL.SDL_Surface*)(void*)SDL.SDL_CreateRGBSurface(0u, (int)num7, (int)num8, 32, 16711680u, 65280u, 255u, 4278190080u);
						int i;
						if (num9 <= 8)
						{
							if (num11 == 0)
							{
								num11 = (uint)(1 << (int)num9);
							}
							for (i = 0; i < num11; i++)
							{
								ptr[i] = stackDataReader.ReadUInt32LE();
							}
						}
						byte* ptr3 = (byte*)(void*)(ptr2->pixels + ptr2->h * ptr2->pitch);
						int num13;
						int num14;
						switch (num12)
						{
						case 1:
							num13 = (int)(num7 + 7) >> 3;
							num14 = ((num13 % 4 != 0) ? (4 - num13 % 4) : 0);
							break;
						case 4:
							num13 = (int)(num7 + 1) >> 1;
							num14 = ((num13 % 4 != 0) ? (4 - num13 % 4) : 0);
							break;
						case 8:
							num13 = (int)num7;
							num14 = ((num13 % 4 != 0) ? (4 - num13 % 4) : 0);
							break;
						default:
							num13 = (int)(num7 * 4);
							num14 = 0;
							break;
						}
						while (ptr3 > (void*)ptr2->pixels)
						{
							ptr3 -= ptr2->pitch;
							if (num12 == 1 || num12 == 4 || num12 == 8)
							{
								byte b = 0;
								int num15 = 8 - num12;
								for (i = 0; i < ptr2->w; i++)
								{
									if (i % (8 / num12) == 0)
									{
										b = stackDataReader.ReadUInt8();
									}
									*(uint*)(ptr3 + (nint)i * (nint)4) = ptr[b >> num15];
									b = (byte)(b << num12);
								}
							}
							else
							{
								for (int j = 0; j < ptr2->pitch; j++)
								{
									ptr3[j] = stackDataReader.ReadUInt8();
								}
							}
							if (num14 != 0)
							{
								for (i = 0; i < num14; i++)
								{
									stackDataReader.ReadUInt8();
								}
							}
						}
						ptr3 = (byte*)(void*)(ptr2->pixels + ptr2->h * ptr2->pitch);
						num12 = 1;
						num13 = (int)(num7 + 7) >> 3;
						num14 = ((num13 % 4 != 0) ? (4 - num13 % 4) : 0);
						while (ptr3 > (void*)ptr2->pixels)
						{
							byte b2 = 0;
							int num16 = 8 - num12;
							ptr3 -= ptr2->pitch;
							for (i = 0; i < ptr2->w; i++)
							{
								if (i % (8 / num12) == 0)
								{
									b2 = stackDataReader.ReadUInt8();
								}
								*(int*)(ptr3 + (nint)i * (nint)4) |= ((b2 >> num16 == 0) ? (-16777216) : 0);
								b2 = (byte)(b2 << num12);
							}
							if (num14 != 0)
							{
								for (i = 0; i < num14; i++)
								{
									stackDataReader.ReadUInt8();
								}
							}
						}
						ptr2 = (SDL.SDL_Surface*)(void*)INTERNAL_convertSurfaceFormat((IntPtr)ptr2);
						int num17 = ptr2->w * ptr2->h * 4;
						byte* ptr4 = (byte*)(void*)ptr2->pixels;
						i = 0;
						while (i < num17)
						{
							if (ptr4[3] == 0)
							{
								*ptr4 = 0;
								ptr4[1] = 0;
								ptr4[2] = 0;
							}
							i += 4;
							ptr4 += 4;
						}
						Texture2D texture2D = new Texture2D(Client.Game.GraphicsDevice, ptr2->w, ptr2->h);
						texture2D.SetDataPointerEXT(0, new Rectangle(0, 0, ptr2->w, ptr2->h), ptr2->pixels, num17);
						SDL.SDL_FreeSurface((IntPtr)ptr2);
						stackDataReader.Release();
						return texture2D;
					}
					return null;
				}
				return null;
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(array);
			}
		}

		private unsafe static IntPtr INTERNAL_convertSurfaceFormat(IntPtr surface)
		{
			IntPtr result = surface;
			SDL.SDL_Surface* ptr = (SDL.SDL_Surface*)(void*)surface;
			SDL.SDL_PixelFormat* ptr2 = (SDL.SDL_PixelFormat*)(void*)ptr->format;
			if (ptr2->format != SDL.SDL_PIXELFORMAT_ABGR8888)
			{
				result = SDL.SDL_ConvertSurfaceFormat(surface, SDL.SDL_PIXELFORMAT_ABGR8888, 0u);
				SDL.SDL_FreeSurface(surface);
			}
			return result;
		}
	}

	private static Point _last_position = new Point(100, 100);

	private Point _center;

	private Point _lastScroll;

	private Point _mouseCenter;

	private bool _flipMap = true;

	private const bool _freeView = false;

	private List<string> _hiddenMarkerFiles;

	private bool _isScrolling;

	private bool _isTopMost;

	private readonly string _mapFilesPath = Path.Combine(CUOEnviroment.ExecutablePath, "Data", "Client");

	private readonly string _mapIconsPath = Path.Combine(CUOEnviroment.ExecutablePath, "Data", "Client", "MapIcons");

	public const string USER_MARKERS_FILE = "userMarkers";

	public static readonly string UserMarkersFilePath = Path.Combine(CUOEnviroment.ExecutablePath, "Data", "Client", "userMarkers.usr");

	private int _mapIndex;

	private bool _mapMarkersLoaded;

	private static Texture2D _mapTexture;

	private static uint[] _pixelBuffer;

	private static sbyte[] _zBuffer;

	public static readonly List<WMapMarkerFile> _markerFiles = new List<WMapMarkerFile>();

	private ClassicUO.Renderer.SpriteFont _markerFont = Fonts.Map1;

	private int _markerFontIndex = 1;

	public static readonly Dictionary<string, Texture2D> _markerIcons = new Dictionary<string, Texture2D>();

	private readonly Dictionary<string, ContextMenuItemEntry> _options = new Dictionary<string, ContextMenuItemEntry>();

	private bool _showCoordinates;

	private bool _showGroupBar = true;

	private bool _showGroupName = true;

	private bool _showMarkerIcons = true;

	private bool _showMarkerNames = true;

	private bool _showMarkers = true;

	private bool _showMobiles = true;

	private bool _showMultis = true;

	private bool _showPartyMembers = true;

	private bool _showPlayerBar = true;

	private bool _showPlayerName = true;

	private int _zoomIndex = 4;

	private WMapMarker _gotoMarker;

	private readonly float[] _zooms = new float[10] { 0.125f, 0.25f, 0.5f, 0.75f, 1f, 1.5f, 2f, 4f, 6f, 8f };

	private static readonly Dictionary<string, Color> _colorMap = new Dictionary<string, Color>
	{
		{
			"red",
			Color.Red
		},
		{
			"green",
			Color.Green
		},
		{
			"blue",
			Color.Blue
		},
		{
			"purple",
			Color.Purple
		},
		{
			"black",
			Color.Black
		},
		{
			"yellow",
			Color.Yellow
		},
		{
			"white",
			Color.White
		},
		{
			"none",
			Color.Transparent
		}
	};

	public override GumpType GumpType => GumpType.WorldMap;

	public float Zoom => _zooms[_zoomIndex];

	public bool TopMost
	{
		get
		{
			return _isTopMost;
		}
		set
		{
			_isTopMost = value;
			base.ShowBorder = !_isTopMost;
			base.LayerOrder = ((!_isTopMost) ? UILayer.Under : UILayer.Over);
		}
	}

	public bool FreeView => false;

	public WorldMapGump()
		: base(400, 400, 100, 100, 0u, 0u, 0)
	{
		CanMove = true;
		AcceptMouseInput = true;
		base.CanCloseWithRightClick = false;
		base.X = _last_position.X;
		base.Y = _last_position.Y;
		LoadSettings();
		GameActions.Print(ResGumps.WorldMapLoading, 53, MessageType.Regular, 3);
		Load();
		OnResize();
		LoadMarkers();
		World.WMapManager.SetEnable(v: true);
		BuildGump();
	}

	public override void Restore(XmlElement xml)
	{
		base.Restore(xml);
		BuildGump();
	}

	private void LoadSettings()
	{
		base.Width = ProfileManager.CurrentProfile.WorldMapWidth;
		base.Height = ProfileManager.CurrentProfile.WorldMapHeight;
		SetFont(ProfileManager.CurrentProfile.WorldMapFont);
		ResizeWindow(new Point(base.Width, base.Height));
		_flipMap = ProfileManager.CurrentProfile.WorldMapFlipMap;
		TopMost = ProfileManager.CurrentProfile.WorldMapTopMost;
		_showPartyMembers = ProfileManager.CurrentProfile.WorldMapShowParty;
		World.WMapManager.SetEnable(_showPartyMembers);
		_zoomIndex = ProfileManager.CurrentProfile.WorldMapZoomIndex;
		_showCoordinates = ProfileManager.CurrentProfile.WorldMapShowCoordinates;
		_showMobiles = ProfileManager.CurrentProfile.WorldMapShowMobiles;
		_showPlayerName = ProfileManager.CurrentProfile.WorldMapShowPlayerName;
		_showPlayerBar = ProfileManager.CurrentProfile.WorldMapShowPlayerBar;
		_showGroupName = ProfileManager.CurrentProfile.WorldMapShowGroupName;
		_showGroupBar = ProfileManager.CurrentProfile.WorldMapShowGroupBar;
		_showMarkers = ProfileManager.CurrentProfile.WorldMapShowMarkers;
		_showMultis = ProfileManager.CurrentProfile.WorldMapShowMultis;
		_showMarkerNames = ProfileManager.CurrentProfile.WorldMapShowMarkersNames;
		_hiddenMarkerFiles = (string.IsNullOrEmpty(ProfileManager.CurrentProfile.WorldMapHiddenMarkerFiles) ? new List<string>() : ProfileManager.CurrentProfile.WorldMapHiddenMarkerFiles.Split(',').ToList());
	}

	public void SaveSettings()
	{
		if (ProfileManager.CurrentProfile != null)
		{
			ProfileManager.CurrentProfile.WorldMapWidth = base.Width;
			ProfileManager.CurrentProfile.WorldMapHeight = base.Height;
			ProfileManager.CurrentProfile.WorldMapFlipMap = _flipMap;
			ProfileManager.CurrentProfile.WorldMapTopMost = TopMost;
			ProfileManager.CurrentProfile.WorldMapFreeView = FreeView;
			ProfileManager.CurrentProfile.WorldMapShowParty = _showPartyMembers;
			ProfileManager.CurrentProfile.WorldMapZoomIndex = _zoomIndex;
			ProfileManager.CurrentProfile.WorldMapShowCoordinates = _showCoordinates;
			ProfileManager.CurrentProfile.WorldMapShowMobiles = _showMobiles;
			ProfileManager.CurrentProfile.WorldMapShowPlayerName = _showPlayerName;
			ProfileManager.CurrentProfile.WorldMapShowPlayerBar = _showPlayerBar;
			ProfileManager.CurrentProfile.WorldMapShowGroupName = _showGroupName;
			ProfileManager.CurrentProfile.WorldMapShowGroupBar = _showGroupBar;
			ProfileManager.CurrentProfile.WorldMapShowMarkers = _showMarkers;
			ProfileManager.CurrentProfile.WorldMapShowMultis = _showMultis;
			ProfileManager.CurrentProfile.WorldMapShowMarkersNames = _showMarkerNames;
			ProfileManager.CurrentProfile.WorldMapHiddenMarkerFiles = string.Join(",", _hiddenMarkerFiles);
		}
	}

	private bool ParseBool(string boolStr)
	{
		bool result;
		return bool.TryParse(boolStr, out result) && result;
	}

	private void BuildGump()
	{
		BuildContextMenu();
	}

	private void BuildOptionDictionary()
	{
		_options.Clear();
		_options["show_all_markers"] = new ContextMenuItemEntry(ResGumps.ShowAllMarkers, delegate
		{
			_showMarkers = !_showMarkers;
		}, canBeSelected: true, _showMarkers);
		_options["show_marker_names"] = new ContextMenuItemEntry(ResGumps.ShowMarkerNames, delegate
		{
			_showMarkerNames = !_showMarkerNames;
		}, canBeSelected: true, _showMarkerNames);
		_options["show_marker_icons"] = new ContextMenuItemEntry(ResGumps.ShowMarkerIcons, delegate
		{
			_showMarkerIcons = !_showMarkerIcons;
		}, canBeSelected: true, _showMarkerIcons);
		_options["flip_map"] = new ContextMenuItemEntry(ResGumps.FlipMap, delegate
		{
			_flipMap = !_flipMap;
		}, canBeSelected: true, _flipMap);
		_options["goto_location"] = new ContextMenuItemEntry(ResGumps.GotoLocation, delegate
		{
			UIManager.Add(new EntryDialog(250, 150, ResGumps.EnterLocation, delegate(string name)
			{
				_gotoMarker = null;
				if (string.IsNullOrWhiteSpace(name))
				{
					GameActions.Print(ResGumps.InvalidLocation, 53, MessageType.Regular, 3);
				}
				else
				{
					int result = -1;
					int result2 = -1;
					string[] array = name.Split(' ');
					if (array.Length < 2)
					{
						try
						{
							ConvertCoords(name, ref result, ref result2);
							return;
						}
						catch
						{
							GameActions.Print(ResGumps.InvalidLocation, 53, MessageType.Regular, 3);
							return;
						}
					}
					if (!int.TryParse(array[0], out result))
					{
						GameActions.Print(ResGumps.InvalidLocation, 53, MessageType.Regular, 3);
					}
					if (!int.TryParse(array[1], out result2))
					{
						GameActions.Print(ResGumps.InvalidLocation, 53, MessageType.Regular, 3);
					}
				}
			})
			{
				CanCloseWithRightClick = true
			});
		});
		_options["top_most"] = new ContextMenuItemEntry(ResGumps.TopMost, delegate
		{
			TopMost = !TopMost;
		}, canBeSelected: true, _isTopMost);
		_options["show_party_members"] = new ContextMenuItemEntry(ResGumps.ShowPartyMembers, delegate
		{
			_showPartyMembers = !_showPartyMembers;
			World.WMapManager.SetEnable(_showPartyMembers);
		}, canBeSelected: true, _showPartyMembers);
		_options["show_mobiles"] = new ContextMenuItemEntry(ResGumps.ShowMobiles, delegate
		{
			_showMobiles = !_showMobiles;
		}, canBeSelected: true, _showMobiles);
		_options["show_multis"] = new ContextMenuItemEntry(ResGumps.ShowHousesBoats, delegate
		{
			_showMultis = !_showMultis;
		}, canBeSelected: true, _showMultis);
		_options["show_your_name"] = new ContextMenuItemEntry(ResGumps.ShowYourName, delegate
		{
			_showPlayerName = !_showPlayerName;
		}, canBeSelected: true, _showPlayerName);
		_options["show_your_healthbar"] = new ContextMenuItemEntry(ResGumps.ShowYourHealthbar, delegate
		{
			_showPlayerBar = !_showPlayerBar;
		}, canBeSelected: true, _showPlayerBar);
		_options["show_party_name"] = new ContextMenuItemEntry(ResGumps.ShowGroupName, delegate
		{
			_showGroupName = !_showGroupName;
		}, canBeSelected: true, _showGroupName);
		_options["show_party_healthbar"] = new ContextMenuItemEntry(ResGumps.ShowGroupHealthbar, delegate
		{
			_showGroupBar = !_showGroupBar;
		}, canBeSelected: true, _showGroupBar);
		_options["show_coordinates"] = new ContextMenuItemEntry(ResGumps.ShowYourCoordinates, delegate
		{
			_showCoordinates = !_showCoordinates;
		}, canBeSelected: true, _showCoordinates);
		_options["add_marker_on_player"] = new ContextMenuItemEntry(ResGumps.AddMarkerOnPlayer, delegate
		{
			AddMarkerOnPlayer();
		});
		_options["saveclose"] = new ContextMenuItemEntry(ResGumps.SaveClose, Dispose);
	}

	private void BuildContextMenu()
	{
		BuildOptionDictionary();
		base.ContextMenu?.Dispose();
		base.ContextMenu = new ContextMenuControl();
		ContextMenuItemEntry contextMenuItemEntry = new ContextMenuItemEntry(ResGumps.FontStyle);
		contextMenuItemEntry.Add(new ContextMenuItemEntry(string.Format(ResGumps.Style0, 1), delegate
		{
			SetFont(1);
		}));
		contextMenuItemEntry.Add(new ContextMenuItemEntry(string.Format(ResGumps.Style0, 2), delegate
		{
			SetFont(2);
		}));
		contextMenuItemEntry.Add(new ContextMenuItemEntry(string.Format(ResGumps.Style0, 3), delegate
		{
			SetFont(3);
		}));
		contextMenuItemEntry.Add(new ContextMenuItemEntry(string.Format(ResGumps.Style0, 4), delegate
		{
			SetFont(4);
		}));
		contextMenuItemEntry.Add(new ContextMenuItemEntry(string.Format(ResGumps.Style0, 5), delegate
		{
			SetFont(5);
		}));
		contextMenuItemEntry.Add(new ContextMenuItemEntry(string.Format(ResGumps.Style0, 6), delegate
		{
			SetFont(6);
		}));
		ContextMenuItemEntry contextMenuItemEntry2 = new ContextMenuItemEntry(ResGumps.MapMarkerOptions);
		contextMenuItemEntry2.Add(new ContextMenuItemEntry(ResGumps.ReloadMarkers, LoadMarkers));
		contextMenuItemEntry2.Add(contextMenuItemEntry);
		contextMenuItemEntry2.Add(_options["show_all_markers"]);
		contextMenuItemEntry2.Add(new ContextMenuItemEntry(""));
		contextMenuItemEntry2.Add(_options["show_marker_names"]);
		contextMenuItemEntry2.Add(_options["show_marker_icons"]);
		contextMenuItemEntry2.Add(new ContextMenuItemEntry(""));
		if (_markerFiles.Count > 0)
		{
			foreach (WMapMarkerFile markerFile in _markerFiles)
			{
				ContextMenuItemEntry contextMenuItemEntry3 = new ContextMenuItemEntry(string.Format(ResGumps.ShowHide0, markerFile.Name), delegate
				{
					markerFile.Hidden = !markerFile.Hidden;
					if (!markerFile.Hidden)
					{
						string text = _hiddenMarkerFiles.SingleOrDefault((string x) => x.Equals(markerFile.Name));
						if (!string.IsNullOrEmpty(text))
						{
							_hiddenMarkerFiles.Remove(text);
						}
					}
					else
					{
						_hiddenMarkerFiles.Add(markerFile.Name);
					}
				}, canBeSelected: true, !markerFile.Hidden);
				_options["show_marker_" + markerFile.Name] = contextMenuItemEntry3;
				contextMenuItemEntry2.Add(contextMenuItemEntry3);
			}
		}
		else
		{
			contextMenuItemEntry2.Add(new ContextMenuItemEntry(ResGumps.NoMapFiles));
		}
		base.ContextMenu.Add(contextMenuItemEntry2);
		ContextMenuItemEntry contextMenuItemEntry4 = new ContextMenuItemEntry(ResGumps.NamesHealthbars);
		contextMenuItemEntry4.Add(_options["show_your_name"]);
		contextMenuItemEntry4.Add(_options["show_your_healthbar"]);
		contextMenuItemEntry4.Add(_options["show_party_name"]);
		contextMenuItemEntry4.Add(_options["show_party_healthbar"]);
		base.ContextMenu.Add(contextMenuItemEntry4);
		base.ContextMenu.Add("", (List<ContextMenuItemEntry>)null);
		base.ContextMenu.Add(_options["flip_map"]);
		base.ContextMenu.Add(_options["top_most"]);
		base.ContextMenu.Add("", (List<ContextMenuItemEntry>)null);
		base.ContextMenu.Add(_options["show_party_members"]);
		base.ContextMenu.Add(_options["show_mobiles"]);
		base.ContextMenu.Add(_options["show_multis"]);
		base.ContextMenu.Add(_options["show_coordinates"]);
		base.ContextMenu.Add("", (List<ContextMenuItemEntry>)null);
		base.ContextMenu.Add(_options["add_marker_on_player"]);
		base.ContextMenu.Add("", (List<ContextMenuItemEntry>)null);
		base.ContextMenu.Add(_options["saveclose"]);
	}

	public override void Update(double totalTime, double frameTime)
	{
		base.Update(totalTime, frameTime);
		if (!base.IsDisposed && _mapIndex != World.MapIndex)
		{
			Load();
		}
	}

	private Point RotatePoint(int x, int y, float zoom, int dist, float angle = 45f)
	{
		x = (int)((float)x * zoom);
		y = (int)((float)y * zoom);
		if (angle == 0f)
		{
			return new Point(x, y);
		}
		double num = Math.Cos((double)dist * Math.PI / 4.0);
		double num2 = Math.Sin((double)dist * Math.PI / 4.0);
		return new Point((int)Math.Round(num * (double)x - num2 * (double)y), (int)Math.Round(num2 * (double)x + num * (double)y));
	}

	private void AdjustPosition(int x, int y, int centerX, int centerY, out int newX, out int newY)
	{
		int offset = GetOffset(x, y, centerX, centerY);
		int num = x;
		int num2 = y;
		while (offset != 0)
		{
			if ((offset & 1) != 0)
			{
				num2 = centerY;
				num = x * num2 / y;
			}
			else if ((offset & 2) != 0)
			{
				num2 = -centerY;
				num = x * num2 / y;
			}
			else if ((offset & 4) != 0)
			{
				num = centerX;
				num2 = y * num / x;
			}
			else if ((offset & 8) != 0)
			{
				num = -centerX;
				num2 = y * num / x;
			}
			x = num;
			y = num2;
			offset = GetOffset(x, y, centerX, centerY);
		}
		newX = x;
		newY = y;
	}

	private int GetOffset(int x, int y, int centerX, int centerY)
	{
		if (y > centerY)
		{
			return 1;
		}
		if (y < -centerY)
		{
			return 2;
		}
		if (x > centerX)
		{
			return 4;
		}
		if (x >= -centerX)
		{
			return 0;
		}
		return 8;
	}

	public override void Dispose()
	{
		SaveSettings();
		World.WMapManager.SetEnable(v: false);
		UIManager.GameCursor.IsDraggingCursorForced = false;
		base.Dispose();
	}

	private void SetFont(int fontIndex)
	{
		_markerFontIndex = fontIndex;
		switch (fontIndex)
		{
		case 1:
			_markerFont = Fonts.Map1;
			break;
		case 2:
			_markerFont = Fonts.Map2;
			break;
		case 3:
			_markerFont = Fonts.Map3;
			break;
		case 4:
			_markerFont = Fonts.Map4;
			break;
		case 5:
			_markerFont = Fonts.Map5;
			break;
		case 6:
			_markerFont = Fonts.Map6;
			break;
		default:
			_markerFontIndex = 1;
			_markerFont = Fonts.Map1;
			break;
		}
	}

	private bool GetOptionValue(string key)
	{
		_options.TryGetValue(key, out var value);
		return value?.IsSelected ?? false;
	}

	public void SetOptionValue(string key, bool v)
	{
		if (_options.TryGetValue(key, out var value) && value != null)
		{
			value.IsSelected = v;
		}
	}

	private unsafe void LoadMapChunk(Span<uint> buffer, Span<sbyte> allZ, int chunkX, int chunkY)
	{
		if (World.Map == null)
		{
			return;
		}
		HuesLoader instance = HuesLoader.Instance;
		ref IndexMap index = ref World.Map.GetIndex(chunkX, chunkY);
		if (index.MapAddress == 0L)
		{
			return;
		}
		int num = 0;
		MapBlock* ptr = (MapBlock*)index.MapAddress;
		MapCells* ptr2 = (MapCells*)(&ptr->Cells);
		for (int i = 0; i < 8; i++)
		{
			int num2 = i << 3;
			int num3 = 0;
			while (num3 < 8)
			{
				ushort c = (ushort)(0x8000 | instance.GetRadarColorData(ptr2[num2].TileID & 0x3FFF));
				buffer[num] = HuesHelper.Color16To32(c) | 0xFF000000u;
				allZ[num] = ptr2[num2].Z;
				num3++;
				num2++;
				num++;
			}
		}
		StaticsBlock* ptr3 = (StaticsBlock*)index.StaticAddress;
		if (ptr3 == null)
		{
			return;
		}
		int staticCount = (int)index.StaticCount;
		int num4 = 0;
		while (num4 < staticCount)
		{
			if (ptr3->Color != 0 && ptr3->Color != ushort.MaxValue && GameObject.CanBeDrawn(ptr3->Color))
			{
				int index2 = ptr3->Y * 8 + ptr3->X;
				if (ptr3->Z >= allZ[index2])
				{
					ushort c2 = (ushort)(0x8000 | ((ptr3->Hue != 0) ? instance.GetColor16(16384, ptr3->Hue) : instance.GetRadarColorData(ptr3->Color + 16384)));
					buffer[index2] = HuesHelper.Color16To32(c2) | 0xFF000000u;
					allZ[index2] = ptr3->Z;
				}
			}
			num4++;
			ptr3++;
		}
	}

	private void LoadMapDetails(Span<uint> buffer, Span<sbyte> allZ)
	{
		for (int i = 0; i < 8; i++)
		{
			int num = i * 8;
			int num2 = (i + 1) * 8;
			int num3 = 0;
			while (num3 < 8)
			{
				sbyte b = allZ[num];
				sbyte b2 = allZ[((num2 >= allZ.Length) ? num : num2) % allZ.Length];
				ref uint reference = ref buffer[num];
				if (b != b2 && reference != 0)
				{
					byte b3 = (byte)(reference & 0xFF);
					byte b4 = (byte)((reference >> 8) & 0xFF);
					byte b5 = (byte)((reference >> 16) & 0xFF);
					byte b6 = (byte)((reference >> 24) & 0xFF);
					if (b < b2)
					{
						b3 = (byte)Math.Min(255f, (float)(int)b3 * 0.8f);
						b4 = (byte)Math.Min(255f, (float)(int)b4 * 0.8f);
						b5 = (byte)Math.Min(255f, (float)(int)b5 * 0.8f);
					}
					else
					{
						b3 = (byte)Math.Min(255f, (float)(int)b3 * 1.25f);
						b4 = (byte)Math.Min(255f, (float)(int)b4 * 1.25f);
						b5 = (byte)Math.Min(255f, (float)(int)b5 * 1.25f);
					}
					reference = (uint)(b3 | (b4 << 8) | (b5 << 16) | (b6 << 24));
				}
				num3++;
				num++;
				num2++;
			}
		}
	}

	private unsafe Task Load()
	{
		_mapIndex = World.MapIndex;
		if (_mapIndex < 0 || _mapIndex > Constants.MAPS_COUNT)
		{
			return Task.CompletedTask;
		}
		return Task.Run(delegate
		{
			if (World.InGame)
			{
				if (_mapTexture == null || _mapTexture.IsDisposed)
				{
					int num = -1;
					int num2 = -1;
					for (int i = 0; i < MapLoader.Instance.MapsDefaultSize.GetLength(0); i++)
					{
						if (num < MapLoader.Instance.MapsDefaultSize[i, 0])
						{
							num = MapLoader.Instance.MapsDefaultSize[i, 0];
						}
						if (num2 < MapLoader.Instance.MapsDefaultSize[i, 1])
						{
							num2 = MapLoader.Instance.MapsDefaultSize[i, 1];
						}
					}
					num += 2;
					num2 += 2;
					_mapTexture = new Texture2D(Client.Game.GraphicsDevice, num, num2, mipMap: false, SurfaceFormat.Color);
					_pixelBuffer = new uint[num * num2];
					_zBuffer = new sbyte[num * num2];
				}
				try
				{
					int num3 = MapLoader.Instance.MapsDefaultSize[World.MapIndex, 0];
					int num4 = MapLoader.Instance.MapsDefaultSize[World.MapIndex, 1];
					int num5 = MapLoader.Instance.MapBlocksSize[World.MapIndex, 0];
					int num6 = MapLoader.Instance.MapBlocksSize[World.MapIndex, 1];
					sbyte[] zBuffer = _zBuffer;
					uint[] pixelBuffer = _pixelBuffer;
					pixelBuffer.AsSpan().Fill(0u);
					fixed (uint* ptr = &pixelBuffer[0])
					{
						_mapTexture.SetDataPointerEXT(0, null, (IntPtr)ptr, 4 * _mapTexture.Width * _mapTexture.Height);
					}
					HuesLoader instance = HuesLoader.Instance;
					int num7 = 0;
					int num8 = 0;
					for (int j = 0; j < num5; j++)
					{
						num7 = j << 3;
						for (int k = 0; k < num6; k++)
						{
							ref IndexMap index = ref World.Map.GetIndex(j, k);
							if (index.MapAddress != 0L)
							{
								MapBlock* ptr2 = (MapBlock*)index.MapAddress;
								MapCells* ptr3 = (MapCells*)(&ptr2->Cells);
								num8 = k << 3;
								for (int l = 0; l < 8; l++)
								{
									int num9 = (num8 + l + 1) * (num3 + 2) + num7 + 1;
									int num10 = l << 3;
									int num11 = 0;
									while (num11 < 8)
									{
										ushort c = (ushort)(0x8000 | instance.GetRadarColorData(ptr3[num10].TileID & 0x3FFF));
										pixelBuffer[num9] = HuesHelper.Color16To32(c) | 0xFF000000u;
										zBuffer[num9] = ptr3[num10].Z;
										num11++;
										num10++;
										num9++;
									}
								}
								StaticsBlock* ptr4 = (StaticsBlock*)index.StaticAddress;
								if (ptr4 != null)
								{
									int staticCount = (int)index.StaticCount;
									int num12 = 0;
									while (num12 < staticCount)
									{
										if (ptr4->Color != 0 && ptr4->Color != ushort.MaxValue && GameObject.CanBeDrawn(ptr4->Color))
										{
											int num13 = (num8 + ptr4->Y + 1) * (num3 + 2) + num7 + ptr4->X + 1;
											if (ptr4->Z >= zBuffer[num13])
											{
												ushort c2 = (ushort)(0x8000 | ((ptr4->Hue != 0) ? instance.GetColor16(16384, ptr4->Hue) : instance.GetRadarColorData(ptr4->Color + 16384)));
												pixelBuffer[num13] = HuesHelper.Color16To32(c2) | 0xFF000000u;
												zBuffer[num13] = ptr4->Z;
											}
										}
										num12++;
										ptr4++;
									}
								}
							}
						}
					}
					int num14 = num3 - 1;
					int num15 = num4 - 1;
					for (num8 = 1; num8 < num15; num8++)
					{
						int num16 = (num8 + 1) * (num3 + 2) + 1;
						int num17 = (num8 + 1 + 1) * (num3 + 2) + 1;
						for (num7 = 1; num7 < num14; num7++)
						{
							sbyte b = zBuffer[++num16];
							sbyte b2 = zBuffer[num17++];
							if (b != b2)
							{
								ref uint reference = ref pixelBuffer[num16];
								if (reference != 0)
								{
									byte b3 = (byte)(reference & 0xFF);
									byte b4 = (byte)((reference >> 8) & 0xFF);
									byte b5 = (byte)((reference >> 16) & 0xFF);
									byte b6 = (byte)((reference >> 24) & 0xFF);
									if (b3 != 0 || b4 != 0 || b5 != 0)
									{
										if (b < b2)
										{
											b3 = (byte)Math.Min(255f, (float)(int)b3 * 0.8f);
											b4 = (byte)Math.Min(255f, (float)(int)b4 * 0.8f);
											b5 = (byte)Math.Min(255f, (float)(int)b5 * 0.8f);
										}
										else
										{
											b3 = (byte)Math.Min(255f, (float)(int)b3 * 1.25f);
											b4 = (byte)Math.Min(255f, (float)(int)b4 * 1.25f);
											b5 = (byte)Math.Min(255f, (float)(int)b5 * 1.25f);
										}
										reference = (uint)(b3 | (b4 << 8) | (b5 << 16) | (b6 << 24));
									}
								}
							}
						}
					}
					num3 += 2;
					num4 += 2;
					fixed (uint* ptr5 = &pixelBuffer[0])
					{
						_mapTexture.SetDataPointerEXT(0, new Rectangle(0, 0, num3, num4), (IntPtr)ptr5, 4 * num3 * num4);
					}
				}
				catch (Exception arg)
				{
					Log.Error($"error loading worldmap: {arg}");
				}
				GameActions.Print(ResGumps.WorldMapLoaded, 72, MessageType.Regular, 3);
			}
		});
	}

	private void LoadMarkers()
	{
		if (!World.InGame)
		{
			return;
		}
		_mapMarkersLoaded = false;
		GameActions.Print(ResGumps.LoadingWorldMapMarkers, 42, MessageType.Regular, 3);
		foreach (Texture2D value6 in _markerIcons.Values)
		{
			if (value6 != null && !value6.IsDisposed)
			{
				value6.Dispose();
			}
		}
		if (!File.Exists(UserMarkersFilePath))
		{
			using (File.Create(UserMarkersFilePath))
			{
			}
		}
		_markerIcons.Clear();
		if (!Directory.Exists(_mapIconsPath))
		{
			Directory.CreateDirectory(_mapIconsPath);
		}
		foreach (string item in Directory.GetFiles(_mapIconsPath, "*.cur").Union(Directory.GetFiles(_mapIconsPath, "*.ico")))
		{
			FileStream fileStream2 = new FileStream(item, FileMode.Open, FileAccess.Read);
			MemoryStream memoryStream = new MemoryStream();
			fileStream2.CopyTo(memoryStream);
			memoryStream.Seek(0L, SeekOrigin.Begin);
			try
			{
				Texture2D value = CurLoader.CreateTextureFromICO_Cur(memoryStream);
				_markerIcons.Add(Path.GetFileNameWithoutExtension(item).ToLower(), value);
			}
			catch (Exception arg)
			{
				Log.Error($"{arg}");
			}
			finally
			{
				memoryStream.Dispose();
				fileStream2.Dispose();
			}
		}
		foreach (string item2 in Directory.GetFiles(_mapIconsPath, "*.png").Union(Directory.GetFiles(_mapIconsPath, "*.jpg")))
		{
			FileStream fileStream3 = new FileStream(item2, FileMode.Open, FileAccess.Read);
			MemoryStream memoryStream2 = new MemoryStream();
			fileStream3.CopyTo(memoryStream2);
			memoryStream2.Seek(0L, SeekOrigin.Begin);
			try
			{
				Texture2D value2 = Texture2D.FromStream(Client.Game.GraphicsDevice, memoryStream2);
				_markerIcons.Add(Path.GetFileNameWithoutExtension(item2).ToLower(), value2);
			}
			catch (Exception arg2)
			{
				Log.Error($"{arg2}");
			}
			finally
			{
				memoryStream2.Dispose();
				fileStream3.Dispose();
			}
		}
		List<string> list = new List<string>();
		list.Add(UserMarkersFilePath);
		list.AddRange(Directory.GetFiles(_mapFilesPath, "*.map").Union(Directory.GetFiles(_mapFilesPath, "*.csv")).Union(Directory.GetFiles(_mapFilesPath, "*.xml")));
		_markerFiles.Clear();
		foreach (string item3 in list)
		{
			if (!File.Exists(item3))
			{
				continue;
			}
			WMapMarkerFile markerFile = new WMapMarkerFile
			{
				Hidden = false,
				Name = Path.GetFileNameWithoutExtension(item3),
				FullPath = item3,
				Markers = new List<WMapMarker>(),
				IsEditable = false
			};
			if (!string.IsNullOrEmpty(_hiddenMarkerFiles.FirstOrDefault((string x) => x.Contains(markerFile.Name))))
			{
				markerFile.Hidden = true;
			}
			if (item3 != null && Path.GetExtension(item3).ToLower().Equals(".xml"))
			{
				using XmlTextReader xmlTextReader = new XmlTextReader(File.Open(item3, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
				while (xmlTextReader.Read())
				{
					if (xmlTextReader.Name.Equals("Marker"))
					{
						WMapMarker wMapMarker = new WMapMarker
						{
							X = int.Parse(xmlTextReader.GetAttribute("X")),
							Y = int.Parse(xmlTextReader.GetAttribute("Y")),
							Name = xmlTextReader.GetAttribute("Name"),
							MapId = int.Parse(xmlTextReader.GetAttribute("Facet")),
							Color = Color.White,
							ZoomIndex = 3
						};
						if (_markerIcons.TryGetValue(xmlTextReader.GetAttribute("Icon").ToLower(), out var value3))
						{
							wMapMarker.MarkerIcon = value3;
							wMapMarker.MarkerIconName = xmlTextReader.GetAttribute("Icon").ToLower();
						}
						markerFile.Markers.Add(wMapMarker);
					}
				}
			}
			else if (item3 != null && Path.GetExtension(item3).ToLower().Equals(".map"))
			{
				using StreamReader streamReader = new StreamReader(File.Open(item3, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
				while (!streamReader.EndOfStream)
				{
					string text = streamReader.ReadLine();
					if (string.IsNullOrEmpty(text) || text.Equals("3") || (!text.Substring(0, 1).Equals("+") && !text.Substring(0, 1).Equals("-")))
					{
						continue;
					}
					string text2 = text.Substring(1, text.IndexOf(':') - 1);
					text = text.Substring(text.IndexOf(':') + 2);
					string[] array = text.Split(' ');
					if (array.Length > 1)
					{
						WMapMarker wMapMarker2 = new WMapMarker
						{
							X = int.Parse(array[0]),
							Y = int.Parse(array[1]),
							MapId = int.Parse(array[2]),
							Name = string.Join(" ", array, 3, array.Length - 3),
							Color = Color.White,
							ZoomIndex = 3
						};
						string[] array2 = text2.Split(' ');
						wMapMarker2.MarkerIconName = array2[0].ToLower();
						if (_markerIcons.TryGetValue(array2[0].ToLower(), out var value4))
						{
							wMapMarker2.MarkerIcon = value4;
						}
						markerFile.Markers.Add(wMapMarker2);
					}
				}
			}
			else if (item3 != null && Path.GetExtension(item3).ToLower().Equals(".usr"))
			{
				markerFile.Markers = LoadUserMarkers();
				markerFile.IsEditable = true;
			}
			else if (item3 != null)
			{
				using StreamReader streamReader2 = new StreamReader(File.Open(item3, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
				while (!streamReader2.EndOfStream)
				{
					string text3 = streamReader2.ReadLine();
					if (string.IsNullOrEmpty(text3))
					{
						return;
					}
					string[] array3 = text3.Split(',');
					if (array3.Length > 1)
					{
						WMapMarker wMapMarker3 = new WMapMarker
						{
							X = int.Parse(array3[0]),
							Y = int.Parse(array3[1]),
							MapId = int.Parse(array3[2]),
							Name = array3[3],
							MarkerIconName = array3[4].ToLower(),
							Color = GetColor(array3[5]),
							ZoomIndex = ((array3.Length == 7) ? int.Parse(array3[6]) : 3)
						};
						if (_markerIcons.TryGetValue(array3[4].ToLower(), out var value5))
						{
							wMapMarker3.MarkerIcon = value5;
						}
						markerFile.Markers.Add(wMapMarker3);
					}
				}
			}
			if (markerFile.Markers.Count > 0)
			{
				GameActions.Print($"..{Path.GetFileName(item3)} ({markerFile.Markers.Count})", 43, MessageType.Regular, 3);
			}
			_markerFiles.Add(markerFile);
		}
		BuildContextMenu();
		int num = 0;
		foreach (WMapMarkerFile markerFile2 in _markerFiles)
		{
			num += markerFile2.Markers.Count;
		}
		_mapMarkersLoaded = true;
		GameActions.Print(string.Format(ResGumps.WorldMapMarkersLoaded0, num), 42, MessageType.Regular, 3);
	}

	private void AddMarkerOnPlayer()
	{
		if (World.InGame)
		{
			UIManager.Add(new EntryDialog(250, 150, ResGumps.EnterMarkerName, SaveMakerOnPlayer)
			{
				CanCloseWithRightClick = true
			});
		}
	}

	private void SaveMakerOnPlayer(string markerName)
	{
		if (!World.InGame)
		{
			return;
		}
		if (string.IsNullOrWhiteSpace(markerName))
		{
			GameActions.Print(ResGumps.InvalidMarkerName, 42, MessageType.Regular, 3);
		}
		string text = "blue";
		string text2 = "";
		int num = 3;
		string value = $"{World.Player.X},{World.Player.Y},{World.Map.Index},{markerName},{text2},{text},{num}";
		using (FileStream stream = File.Open(UserMarkersFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write))
		{
			using StreamWriter streamWriter = new StreamWriter(stream);
			streamWriter.BaseStream.Seek(0L, SeekOrigin.End);
			streamWriter.WriteLine(value);
		}
		WMapMarker wMapMarker = new WMapMarker
		{
			X = World.Player.X,
			Y = World.Player.Y,
			Color = GetColor(text),
			ColorName = text,
			MapId = World.Map.Index,
			MarkerIconName = text2,
			Name = markerName,
			ZoomIndex = num
		};
		if (!string.IsNullOrWhiteSpace(wMapMarker.MarkerIconName) && _markerIcons.TryGetValue(wMapMarker.MarkerIconName, out var value2))
		{
			wMapMarker.MarkerIcon = value2;
		}
		_markerFiles.FirstOrDefault((WMapMarkerFile x) => x.FullPath == UserMarkersFilePath)?.Markers.Add(wMapMarker);
	}

	internal static void ReloadUserMarkers()
	{
		WMapMarkerFile wMapMarkerFile = _markerFiles.FirstOrDefault((WMapMarkerFile f) => f.Name == "userMarkers");
		if (wMapMarkerFile != null)
		{
			wMapMarkerFile.Markers = LoadUserMarkers();
		}
	}

	internal static List<WMapMarker> LoadUserMarkers()
	{
		List<WMapMarker> list = new List<WMapMarker>();
		using StreamReader streamReader = new StreamReader(UserMarkersFilePath);
		while (!streamReader.EndOfStream)
		{
			string text = streamReader.ReadLine();
			if (!string.IsNullOrEmpty(text))
			{
				string[] array = text.Split(',');
				if (array.Length > 1)
				{
					list.Add(ParseMarker(array));
				}
			}
		}
		return list;
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		if (base.IsDisposed || !World.InGame)
		{
			return false;
		}
		if (!_isScrolling)
		{
			_center.X = World.Player.X;
			_center.Y = World.Player.Y;
		}
		int num = x + 4;
		int num2 = y + 4;
		int num3 = base.Width - 8;
		int num4 = base.Height - 8;
		int num5 = _center.X + 1;
		int num6 = _center.Y + 1;
		int num7 = (int)Math.Max((float)num3 * 1.75f, (float)num4 * 1.75f);
		int num8 = (int)((float)num7 / Zoom);
		int num9 = num8 >> 1;
		int num10 = num3 >> 1;
		int num11 = num4 >> 1;
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(0);
		batcher.Draw(SolidColorTextureCache.GetTexture(Color.Black), new Rectangle(num, num2, num3, num4), hueVector);
		if (_mapTexture != null && batcher.ClipBegin(num, num2, num3, num4))
		{
			Rectangle destinationRectangle = new Rectangle(num + num10, num2 + num11, num7, num7);
			Rectangle value = new Rectangle(num5 - num9, num6 - num9, num8, num8);
			batcher.Draw(origin: new Vector2((float)value.Width / 2f, (float)value.Height / 2f), texture: _mapTexture, destinationRectangle: destinationRectangle, sourceRectangle: value, color: hueVector, rotation: _flipMap ? Microsoft.Xna.Framework.MathHelper.ToRadians(45f) : 0f, effects: SpriteEffects.None, layerDepth: 0f);
			DrawAll(batcher, num, num2, num10, num11);
			batcher.ClipEnd();
		}
		return base.Draw(batcher, x, y);
	}

	private void DrawAll(UltimaBatcher2D batcher, int gX, int gY, int halfWidth, int halfHeight)
	{
		if (_showMultis)
		{
			foreach (House house in World.HouseManager.Houses)
			{
				Item item = World.Items.Get(house.Serial);
				if (item != null)
				{
					DrawMulti(batcher, house, item.X, item.Y, gX, gY, halfWidth, halfHeight, Zoom);
				}
			}
		}
		if (_showMarkers && _mapMarkersLoaded)
		{
			WMapMarker wMapMarker = null;
			foreach (WMapMarkerFile markerFile in _markerFiles)
			{
				if (markerFile.Hidden)
				{
					continue;
				}
				foreach (WMapMarker marker in markerFile.Markers)
				{
					if (DrawMarker(batcher, marker, gX, gY, halfWidth, halfHeight, Zoom))
					{
						wMapMarker = marker;
					}
				}
			}
			if (wMapMarker != null)
			{
				DrawMarkerString(batcher, wMapMarker, gX, gY, halfWidth, halfHeight);
			}
		}
		if (_gotoMarker != null)
		{
			DrawMarker(batcher, _gotoMarker, gX, gY, halfWidth, halfHeight, Zoom);
		}
		if (_showMobiles)
		{
			foreach (Mobile value in World.Mobiles.Values)
			{
				if (value == World.Player)
				{
					continue;
				}
				if (value.NotorietyFlag != NotorietyFlag.Ally)
				{
					DrawMobile(batcher, value, gX, gY, halfWidth, halfHeight, Zoom, Color.Red);
				}
				else if (value != null && value.Distance <= World.ClientViewRange)
				{
					WMapEntity entity = World.WMapManager.GetEntity(value);
					if (entity != null)
					{
						if (string.IsNullOrEmpty(entity.Name) && !string.IsNullOrEmpty(value.Name))
						{
							entity.Name = value.Name;
						}
					}
					else
					{
						DrawMobile(batcher, value, gX, gY, halfWidth, halfHeight, Zoom, Color.Lime, drawName: true, isparty: true, _showGroupBar);
					}
				}
				else
				{
					WMapEntity entity2 = World.WMapManager.GetEntity(value.Serial);
					if (entity2 != null && entity2.IsGuild)
					{
						DrawWMEntity(batcher, entity2, gX, gY, halfWidth, halfHeight, Zoom);
					}
				}
			}
		}
		foreach (WMapEntity value2 in World.WMapManager.Entities.Values)
		{
			if (value2.IsGuild && !World.Party.Contains(value2.Serial))
			{
				DrawWMEntity(batcher, value2, gX, gY, halfWidth, halfHeight, Zoom);
			}
		}
		if (_showPartyMembers)
		{
			for (int i = 0; i < 10; i++)
			{
				PartyMember partyMember = World.Party.Members[i];
				if (partyMember == null || !SerialHelper.IsValid(partyMember.Serial))
				{
					continue;
				}
				Mobile mobile = World.Mobiles.Get(partyMember.Serial);
				if (mobile != null && mobile.Distance <= World.ClientViewRange)
				{
					WMapEntity entity3 = World.WMapManager.GetEntity(mobile);
					if (entity3 != null && string.IsNullOrEmpty(entity3.Name) && !string.IsNullOrEmpty(partyMember.Name))
					{
						entity3.Name = partyMember.Name;
					}
					DrawMobile(batcher, mobile, gX, gY, halfWidth, halfHeight, Zoom, Color.Yellow, _showGroupName, isparty: true, _showGroupBar);
				}
				else
				{
					WMapEntity entity4 = World.WMapManager.GetEntity(partyMember.Serial);
					if (entity4 != null && !entity4.IsGuild)
					{
						DrawWMEntity(batcher, entity4, gX, gY, halfWidth, halfHeight, Zoom);
					}
				}
			}
		}
		DrawMobile(batcher, World.Player, gX, gY, halfWidth, halfHeight, Zoom, Color.White, _showPlayerName, isparty: false, _showPlayerBar);
		if (_showCoordinates)
		{
			batcher.DrawString(color: new Vector3(0f, 1f, 1f), spriteFont: Fonts.Bold, text: $"{World.Player.X}, {World.Player.Y} ({World.Player.Z}) [{_zoomIndex}]", x: gX + 6, y: gY + 6);
			Vector3 hueVector = ShaderHueTranslator.GetHueVector(0);
			batcher.DrawString(Fonts.Bold, $"{World.Player.X}, {World.Player.Y} ({World.Player.Z}) [{_zoomIndex}]", gX + 5, gY + 5, hueVector);
		}
	}

	private void DrawMobile(UltimaBatcher2D batcher, Mobile mobile, int x, int y, int width, int height, float zoom, Color color, bool drawName = false, bool isparty = false, bool drawHpBar = false)
	{
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(0);
		int x2 = mobile.X - _center.X;
		int y2 = mobile.Y - _center.Y;
		Point point = RotatePoint(x2, y2, zoom, 1, _flipMap ? 45f : 0f);
		AdjustPosition(point.X, point.Y, width - 4, height - 4, out point.X, out point.Y);
		point.X += x + width;
		point.Y += y + height;
		if (point.X < x)
		{
			point.X = x;
		}
		if (point.X > x + base.Width - 8 - 4)
		{
			point.X = x + base.Width - 8 - 4;
		}
		if (point.Y < y)
		{
			point.Y = y;
		}
		if (point.Y > y + base.Height - 8 - 4)
		{
			point.Y = y + base.Height - 8 - 4;
		}
		batcher.Draw(SolidColorTextureCache.GetTexture(color), new Rectangle(point.X - 2, point.Y - 2, 4, 4), hueVector);
		if (drawName && !string.IsNullOrEmpty(mobile.Name))
		{
			Vector2 vector = Fonts.Regular.MeasureString(mobile.Name);
			if ((float)point.X + vector.X / 2f > (float)(x + base.Width - 8))
			{
				point.X = x + base.Width - 8 - (int)(vector.X / 2f);
			}
			else if ((float)point.X - vector.X / 2f < (float)x)
			{
				point.X = x + (int)(vector.X / 2f);
			}
			if ((float)point.Y + vector.Y > (float)(y + base.Height))
			{
				point.Y = y + base.Height - (int)vector.Y;
			}
			else if ((float)point.Y - vector.Y < (float)y)
			{
				point.Y = y + (int)vector.Y;
			}
			int num = (int)((float)point.X - vector.X / 2f);
			int num2 = (int)((float)point.Y - vector.Y);
			hueVector.X = 0f;
			hueVector.Y = 1f;
			batcher.DrawString(Fonts.Regular, mobile.Name, num + 1, num2 + 1, hueVector);
			hueVector.X = (isparty ? 52 : Notoriety.GetHue(mobile.NotorietyFlag));
			hueVector.Y = 1f;
			hueVector.Z = 1f;
			batcher.DrawString(Fonts.Regular, mobile.Name, num, num2, hueVector);
		}
		if (!drawHpBar)
		{
			return;
		}
		int num3 = mobile.HitsMax;
		if (num3 > 0)
		{
			num3 = mobile.Hits * 100 / num3;
			if (num3 > 100)
			{
				num3 = 100;
			}
			else if (num3 < 1)
			{
				num3 = 0;
			}
		}
		point.Y += 5;
		DrawHpBar(batcher, point.X, point.Y, num3);
	}

	private bool DrawMarker(UltimaBatcher2D batcher, WMapMarker marker, int x, int y, int width, int height, float zoom)
	{
		if (marker.MapId != World.MapIndex)
		{
			return false;
		}
		if (_zoomIndex < marker.ZoomIndex && marker.Color == Color.Transparent)
		{
			return false;
		}
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(0);
		int x2 = marker.X - _center.X;
		int y2 = marker.Y - _center.Y;
		Point point = RotatePoint(x2, y2, zoom, 1, _flipMap ? 45f : 0f);
		point.X += x + width;
		point.Y += y + height;
		if (point.X < x || point.X > x + base.Width - 8 - 4 || point.Y < y || point.Y > y + base.Height - 8 - 4)
		{
			return false;
		}
		bool flag = _showMarkerNames && !string.IsNullOrEmpty(marker.Name) && _zoomIndex > 5;
		bool result = false;
		if (_zoomIndex < marker.ZoomIndex || !_showMarkerIcons || marker.MarkerIcon == null)
		{
			batcher.Draw(SolidColorTextureCache.GetTexture(marker.Color), new Rectangle(point.X - 2, point.Y - 2, 4, 4), hueVector);
			if (Mouse.Position.X >= point.X - 4 && Mouse.Position.X <= point.X + 2 && Mouse.Position.Y >= point.Y - 4 && Mouse.Position.Y <= point.Y + 2)
			{
				result = true;
			}
		}
		else
		{
			batcher.Draw(marker.MarkerIcon, new Vector2(point.X - (marker.MarkerIcon.Width >> 1), point.Y - (marker.MarkerIcon.Height >> 1)), hueVector);
			if (!flag && Mouse.Position.X >= point.X - (marker.MarkerIcon.Width >> 1) && Mouse.Position.X <= point.X + (marker.MarkerIcon.Width >> 1) && Mouse.Position.Y >= point.Y - (marker.MarkerIcon.Height >> 1) && Mouse.Position.Y <= point.Y + (marker.MarkerIcon.Height >> 1))
			{
				result = true;
			}
		}
		if (flag)
		{
			DrawMarkerString(batcher, marker, x, y, width, height);
			result = false;
		}
		return result;
	}

	private void DrawMarkerString(UltimaBatcher2D batcher, WMapMarker marker, int x, int y, int width, int height)
	{
		int x2 = marker.X - _center.X;
		int y2 = marker.Y - _center.Y;
		Point point = RotatePoint(x2, y2, Zoom, 1, _flipMap ? 45f : 0f);
		point.X += x + width;
		point.Y += y + height;
		Vector2 vector = _markerFont.MeasureString(marker.Name);
		if ((float)point.X + vector.X / 2f > (float)(x + base.Width - 8))
		{
			point.X = x + base.Width - 8 - (int)(vector.X / 2f);
		}
		else if ((float)point.X - vector.X / 2f < (float)x)
		{
			point.X = x + (int)(vector.X / 2f);
		}
		if ((float)point.Y + vector.Y > (float)(y + base.Height))
		{
			point.Y = y + base.Height - (int)vector.Y;
		}
		else if ((float)point.Y - vector.Y < (float)y)
		{
			point.Y = y + (int)vector.Y;
		}
		int num = (int)((float)point.X - vector.X / 2f);
		int num2 = (int)((float)point.Y - vector.Y - 5f);
		batcher.Draw(color: new Vector3(0f, 1f, 0.5f), texture: SolidColorTextureCache.GetTexture(Color.Black), destinationRectangle: new Rectangle(num - 2, num2 - 2, (int)(vector.X + 4f), (int)(vector.Y + 4f)));
		batcher.DrawString(color: new Vector3(0f, 1f, 1f), spriteFont: _markerFont, text: marker.Name, x: num + 1, y: num2 + 1);
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(0);
		batcher.DrawString(_markerFont, marker.Name, num, num2, hueVector);
	}

	private void DrawMulti(UltimaBatcher2D batcher, House house, int multiX, int multiY, int x, int y, int width, int height, float zoom)
	{
		int x2 = multiX - _center.X;
		int y2 = multiY - _center.Y;
		int num = Math.Abs(house.Bounds.Width - house.Bounds.X);
		int num2 = Math.Abs(house.Bounds.Height - house.Bounds.Y);
		Point point = RotatePoint(x2, y2, zoom, 1, _flipMap ? 45f : 0f);
		point.X += x + width;
		point.Y += y + height;
		if (point.X >= x && point.X <= x + base.Width - 8 - 4 && point.Y >= y && point.Y <= y + base.Height - 8 - 4)
		{
			Vector3 hueVector = ShaderHueTranslator.GetHueVector(0);
			Texture2D texture = SolidColorTextureCache.GetTexture(Color.DarkGray);
			batcher.Draw(texture, new Rectangle(point.X - (int)((float)num / 2f), point.Y - (int)((float)num2 / 2f), (int)((float)num * zoom), (int)((float)num2 * zoom)), null, hueVector, _flipMap ? Microsoft.Xna.Framework.MathHelper.ToRadians(45f) : 0f, new Vector2(0.5f, 0.5f), SpriteEffects.None, 0f);
		}
	}

	private void DrawWMEntity(UltimaBatcher2D batcher, WMapEntity entity, int x, int y, int width, int height, float zoom)
	{
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(0);
		ushort num;
		Color color;
		if (entity.IsGuild)
		{
			num = 68;
			color = Color.LimeGreen;
		}
		else
		{
			num = 52;
			color = Color.Yellow;
		}
		if (entity.Map != World.MapIndex)
		{
			num = 992;
			color = Color.DarkGray;
		}
		int x2 = entity.X - _center.X;
		int y2 = entity.Y - _center.Y;
		Point point = RotatePoint(x2, y2, zoom, 1, _flipMap ? 45f : 0f);
		AdjustPosition(point.X, point.Y, width - 4, height - 4, out point.X, out point.Y);
		point.X += x + width;
		point.Y += y + height;
		if (point.X < x)
		{
			point.X = x;
		}
		if (point.X > x + base.Width - 8 - 4)
		{
			point.X = x + base.Width - 8 - 4;
		}
		if (point.Y < y)
		{
			point.Y = y;
		}
		if (point.Y > y + base.Height - 8 - 4)
		{
			point.Y = y + base.Height - 8 - 4;
		}
		batcher.Draw(SolidColorTextureCache.GetTexture(color), new Rectangle(point.X - 2, point.Y - 2, 4, 4), hueVector);
		if (_showGroupName)
		{
			string text = entity.Name ?? ResGumps.OutOfRange;
			Vector2 vector = Fonts.Regular.MeasureString(entity.Name ?? text);
			if ((float)point.X + vector.X / 2f > (float)(x + base.Width - 8))
			{
				point.X = x + base.Width - 8 - (int)(vector.X / 2f);
			}
			else if ((float)point.X - vector.X / 2f < (float)x)
			{
				point.X = x + (int)(vector.X / 2f);
			}
			if ((float)point.Y + vector.Y > (float)(y + base.Height))
			{
				point.Y = y + base.Height - (int)vector.Y;
			}
			else if ((float)point.Y - vector.Y < (float)y)
			{
				point.Y = y + (int)vector.Y;
			}
			int num2 = (int)((float)point.X - vector.X / 2f);
			int num3 = (int)((float)point.Y - vector.Y);
			hueVector.X = 0f;
			hueVector.Y = 1f;
			batcher.DrawString(Fonts.Regular, text, num2 + 1, num3 + 1, hueVector);
			batcher.DrawString(color: new Vector3((int)num, 1f, 1f), spriteFont: Fonts.Regular, text: text, x: num2, y: num3);
		}
		if (_showGroupBar)
		{
			point.Y += 5;
			DrawHpBar(batcher, point.X, point.Y, entity.HP);
		}
	}

	private void DrawHpBar(UltimaBatcher2D batcher, int x, int y, int hp)
	{
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(0);
		batcher.Draw(SolidColorTextureCache.GetTexture(Color.Black), new Rectangle(x - 12 - 1, y - 1 - 1, 27, 5), hueVector);
		batcher.Draw(SolidColorTextureCache.GetTexture(Color.Red), new Rectangle(x - 12, y - 1, 25, 3), hueVector);
		int num = 100;
		if (num > 0)
		{
			num = hp * 100 / num;
			if (num > 100)
			{
				num = 100;
			}
			if (num > 1)
			{
				num = 25 * num / 100;
			}
		}
		batcher.Draw(SolidColorTextureCache.GetTexture(Color.CornflowerBlue), new Rectangle(x - 12, y - 1, num, 3), hueVector);
	}

	protected override void OnMouseUp(int x, int y, MouseButtonType button)
	{
		if (button == MouseButtonType.Left && !Keyboard.Alt)
		{
			_isScrolling = false;
			CanMove = true;
		}
		UIManager.GameCursor.IsDraggingCursorForced = false;
		base.OnMouseUp(x, y, button);
	}

	protected override void OnMouseDown(int x, int y, MouseButtonType button)
	{
		if (!ItemHold.Enabled)
		{
			if (((button == MouseButtonType.Left && Keyboard.Alt) || button == MouseButtonType.Middle) && x > 4 && x < base.Width - 8 && y > 4 && y < base.Height - 8)
			{
				_lastScroll.X = x;
				_lastScroll.Y = y;
				_isScrolling = true;
				CanMove = false;
				UIManager.GameCursor.IsDraggingCursorForced = true;
			}
			if (button == MouseButtonType.Left && Keyboard.Ctrl)
			{
				float num = (float)base.Width / Zoom;
				float num2 = (float)base.Height / Zoom;
				float num3 = (float)x / Zoom;
				float num4 = (float)y / Zoom;
				if (_flipMap)
				{
					float num5 = (num + num2) / 1.41f;
					float num6 = (num2 - num) / 1.41f;
					num = (int)num5;
					num2 = (int)num6;
					float num7 = (num3 + num4) / 1.41f;
					float num8 = (num4 - num3) / 1.41f;
					num3 = (int)num7;
					num4 = (int)num8;
				}
				_mouseCenter.X = _center.X - (int)(num / 2f) + (int)num3;
				_mouseCenter.Y = _center.Y - (int)(num2 / 2f) + (int)num4;
				WMapMarkerFile wMapMarkerFile = _markerFiles.Where((WMapMarkerFile f) => f.Name == "userMarkers").FirstOrDefault();
				if (wMapMarkerFile == null)
				{
					return;
				}
				UIManager.GetGump<UserMarkersGump>(null)?.Dispose();
				UIManager.Add(new UserMarkersGump(_mouseCenter.X, _mouseCenter.Y, wMapMarkerFile.Markers));
			}
		}
		base.OnMouseDown(x, y, button);
	}

	protected override void OnMouseOver(int x, int y)
	{
		Point point = (Mouse.LButtonPressed ? Mouse.LDragOffset : (Mouse.MButtonPressed ? Mouse.MDragOffset : Point.Zero));
		if (_isScrolling && point != Point.Zero)
		{
			Point lastScroll = _lastScroll;
			lastScroll.X -= x;
			lastScroll.Y -= y;
			if (!(lastScroll == Point.Zero))
			{
				lastScroll = RotatePoint(lastScroll.X, lastScroll.Y, 1f / Zoom, -1, _flipMap ? 45f : 0f);
				_center.X += lastScroll.X;
				_center.Y += lastScroll.Y;
				if (_center.X < 0)
				{
					_center.X = 0;
				}
				if (_center.Y < 0)
				{
					_center.Y = 0;
				}
				if (_center.X > MapLoader.Instance.MapsDefaultSize[World.MapIndex, 0])
				{
					_center.X = MapLoader.Instance.MapsDefaultSize[World.MapIndex, 0];
				}
				if (_center.Y > MapLoader.Instance.MapsDefaultSize[World.MapIndex, 1])
				{
					_center.Y = MapLoader.Instance.MapsDefaultSize[World.MapIndex, 1];
				}
				_lastScroll.X = x;
				_lastScroll.Y = y;
			}
		}
		else
		{
			base.OnMouseOver(x, y);
		}
	}

	protected override void OnMouseWheel(MouseEventType delta)
	{
		if (delta == MouseEventType.WheelScrollUp)
		{
			_zoomIndex++;
			if (_zoomIndex >= _zooms.Length)
			{
				_zoomIndex = _zooms.Length - 1;
			}
		}
		else
		{
			_zoomIndex--;
			if (_zoomIndex < 0)
			{
				_zoomIndex = 0;
			}
		}
		base.OnMouseWheel(delta);
	}

	protected override bool OnMouseDoubleClick(int x, int y, MouseButtonType button)
	{
		if (button != MouseButtonType.Left || _isScrolling || Keyboard.Alt)
		{
			return base.OnMouseDoubleClick(x, y, button);
		}
		TopMost = !TopMost;
		return true;
	}

	protected override void OnMove(int x, int y)
	{
		base.OnMove(x, y);
		_last_position.X = base.ScreenCoordinateX;
		_last_position.Y = base.ScreenCoordinateY;
	}

	internal static WMapMarker ParseMarker(string[] splits)
	{
		WMapMarker wMapMarker = new WMapMarker
		{
			X = int.Parse(Truncate(splits[0], 4)),
			Y = int.Parse(Truncate(splits[1], 4)),
			MapId = int.Parse(splits[2]),
			Name = Truncate(splits[3], 25),
			MarkerIconName = splits[4].ToLower(),
			Color = GetColor(Truncate(splits[5], 10)),
			ColorName = Truncate(splits[5], 10),
			ZoomIndex = ((splits.Length == 7) ? int.Parse(splits[6]) : 3)
		};
		if (_markerIcons.TryGetValue(splits[4].ToLower(), out var value))
		{
			wMapMarker.MarkerIcon = value;
		}
		return wMapMarker;
	}

	private static string Truncate(string s, int maxLen)
	{
		if (s.Length <= maxLen)
		{
			return s;
		}
		return s.Remove(maxLen);
	}

	public static Color GetColor(string name)
	{
		if (!_colorMap.TryGetValue(name, out var value))
		{
			return Color.White;
		}
		return value;
	}

	private static void ConvertCoords(string coords, ref int xAxis, ref int yAxis)
	{
		string[] array = coords.Split(',');
		string text = array[0];
		string text2 = array[1];
		string[] array2 = text.Split('°', 'o');
		double num = Convert.ToDouble(array2[0]);
		double num2 = Convert.ToDouble(array2[1].Substring(0, array2[1].IndexOf("'", StringComparison.Ordinal)));
		if (text.Substring(text.Length - 1).Equals("N"))
		{
			yAxis = (int)(1624.0 - num2 / 60.0 * 11.377777777777778 - num * 11.377777777777778);
		}
		else
		{
			yAxis = (int)(1624.0 + num2 / 60.0 * 11.377777777777778 + num * 11.377777777777778);
		}
		string[] array3 = text2.Split('°', 'o');
		double num3 = Convert.ToDouble(array3[0]);
		double num4 = Convert.ToDouble(array3[1].Substring(0, array3[1].IndexOf("'", StringComparison.Ordinal)));
		if (text2.Substring(text2.Length - 1).Equals("W"))
		{
			xAxis = (int)(1323.0 - num4 / 60.0 * 14.222222222222221 - num3 * 14.222222222222221);
		}
		else
		{
			xAxis = (int)(1323.0 + num4 / 60.0 * 14.222222222222221 + num3 * 14.222222222222221);
		}
		if (xAxis < 0)
		{
			xAxis += 5120;
		}
		else if (xAxis > 5120)
		{
			xAxis -= 5120;
		}
		if (yAxis < 0)
		{
			yAxis += 4096;
		}
		else if (yAxis > 4096)
		{
			yAxis -= 4096;
		}
	}
}

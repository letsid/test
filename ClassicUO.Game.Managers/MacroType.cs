using System.ComponentModel;

namespace ClassicUO.Game.Managers;

internal enum MacroType
{
	[Description("Nichts")]
	None,
	[Description("Sagen")]
	Say,
	[Description("Emote")]
	Emote,
	[Description("Flüstern")]
	Whisper,
	[Description("Schreien")]
	Yell,
	[Description("Gehen")]
	Walk,
	[Description("Kriegsmodus an und aus schalten")]
	WarPeace,
	[Description("Einfügen")]
	Paste,
	[Description("Öffnen")]
	Open,
	[Description("Schliessen")]
	Close,
	[Description("Minimieren")]
	Minimize,
	[Description("Maximieren")]
	Maximize,
	[Description("Tür öffnen")]
	OpenDoor,
	[Description("Skill benutzen")]
	UseSkill,
	[Description("Letzter Skill")]
	LastSkill,
	[Description("Zaubere Zauber")]
	CastSpell,
	[Description("Letzter Zauber")]
	LastSpell,
	[Description("Letztes Objekt")]
	LastObject,
	[Description("Verbeugen")]
	Bow,
	[Description("Salutieren")]
	Salute,
	[Description("Spiel verlassen")]
	QuitGame,
	[Description("AllNames")]
	AllNames,
	[Description("Letztes Ziel")]
	LastTarget,
	[Description("Ziel Selbst")]
	TargetSelf,
	[Description("Anlegen Ablegen")]
	ArmDisarm,
	[Description("Warte auf Ziel")]
	WaitForTarget,
	[Description("Nächstes Ziel")]
	TargetNext,
	[Description("Greife letztes Ziel an")]
	AttackLast,
	[Description("Zeitverzögerung in ms")]
	Delay,
	[Description("Unsichtbarkeitsradius")]
	CircleTrans,
	[Description("Schließe Gump")]
	CloseGump,
	[Description("AlwaysRun")]
	AlwaysRun,
	[Description("Speichere Einstellungen")]
	SaveDesktop,
	[Description("Keine Funktion")]
	KillGumpOpen,
	[Description("Keine Funktion")]
	PrimaryAbility,
	[Description("Keine Funktion")]
	SecondaryAbility,
	[Description("Letzte Waffe ausrüsten")]
	EquipLastWeapon,
	[Description("Keine Funktion")]
	SetUpdateRange,
	[Description("Keine Funktion")]
	ModifyUpdateRange,
	[Description("Keine Funktion")]
	IncreaseUpdateRange,
	[Description("Keine Funktion")]
	DecreaseUpdateRange,
	[Description("Keine Funktion")]
	MaxUpdateRange,
	[Description("Keine Funktion")]
	MinUpdateRange,
	[Description("Keine Funktion")]
	DefaultUpdateRange,
	[Description("Farbe für Ziele außer Reichweite an")]
	EnableRangeColor,
	[Description("Farbe für Ziele außer Reichweite aus")]
	DisableRangeColor,
	[Description("Farbe für Ziele außer Reichweite schalten")]
	ToggleRangeColor,
	[Description("Keine Funktion")]
	InvokeVirtue,
	[Description("Wähle nächstes")]
	SelectNext,
	[Description("Wähle vorheriges")]
	SelectPrevious,
	[Description("Wähle nahesten")]
	SelectNearest,
	[Description("Greife ausgewähltes Ziel an")]
	AttackSelectedTarget,
	[Description("Nutze ausgewähltes Ziel")]
	UseSelectedTarget,
	[Description("Aktuelles Ziel")]
	CurrentTarget,
	[Description("Keine Funktion")]
	TargetSystemOnOff,
	[Description("Buffleiste An und Aus schalten")]
	ToggleBuffIconGump,
	[Description("Bandagiere selbst")]
	BandageSelf,
	[Description("Bandagiere Ziel")]
	BandageTarget,
	[Description("Keine Funktion")]
	ToggleGargoyleFly,
	[Description("Zoom")]
	Zoom,
	[Description("Chatsichtbarkeit schalten")]
	ToggleChatVisibility,
	[Description("Keine Funktion")]
	INVALID,
	[Description("Aura")]
	Aura,
	[Description("Aura An und Aus schalten")]
	AuraOnOff,
	[Description("Gegenstand aufheben")]
	Grab,
	[Description("Sammelbeutel auswählen")]
	SetGrabBag,
	[Description("Namensplaketten anzeigen")]
	NamesOnOff,
	[Description("Nutze ausgerüsteten Gegenstand")]
	UseItemInHand,
	[Description("Keine Funktion")]
	UsePotion,
	[Description("Schließe alle Lebensleisten")]
	CloseAllHealthBars,
	[Description("Keine Funktion")]
	ToggleDrawRoofs,
	[Description("Keine Funktion")]
	ToggleTreeStumps,
	[Description("Keine Funktion")]
	ToggleVegetation,
	[Description("Keine Funktion")]
	ToggleCaveTiles,
	[Description("Sagen in Partychat")]
	PartySay,
	[Description("Doppelklick ausführen")]
	Doppelklick,
	[Description("Wähle am nahesten zum Mauszeiger")]
	SelectNearestToCursor,
	[Description("Zusatzjournal an/aus")]
	ZusatzjournalAnAus,
	[Description("Bandagieren selbst wenn unter %-Leben")]
	BandageSelfKonditionell,
	[Description("AlwaysWalk")]
	AlwaysWalk,
	[Description("Keine Funktion")]
	Usetype,
	[Description("Keine Funktion")]
	Usename,
	[Description("Benutze Gegenstand")]
	Useitem
}

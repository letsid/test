namespace ClassicUO.Game.Data;

internal class LockedFeatures
{
	public LockedFeatureFlags Flags { get; private set; }

	public bool T2A => Flags.HasFlag(LockedFeatureFlags.TheSecondAge);

	public bool UOR => Flags.HasFlag(LockedFeatureFlags.Renaissance);

	public bool ThirdDawn => Flags.HasFlag(LockedFeatureFlags.ThirdDawn);

	public bool LBR => Flags.HasFlag(LockedFeatureFlags.LordBlackthornsRevenge);

	public bool AOS => Flags.HasFlag(LockedFeatureFlags.AgeOfShadows);

	public bool CharSlots6 => Flags.HasFlag(LockedFeatureFlags.CharacterSlot6);

	public bool SE => Flags.HasFlag(LockedFeatureFlags.SamuraiEmpire);

	public bool ML => Flags.HasFlag(LockedFeatureFlags.MondainsLegacy);

	public bool Splash8th => Flags.HasFlag(LockedFeatureFlags.Splash8);

	public bool Splash9th => Flags.HasFlag(LockedFeatureFlags.Splash9);

	public bool TenthAge => Flags.HasFlag(LockedFeatureFlags.TenthAge);

	public bool MoreStorage => Flags.HasFlag(LockedFeatureFlags.MoreStorage);

	public bool CharSlots7 => Flags.HasFlag(LockedFeatureFlags.CharacterSlot7);

	public bool TenthAgeFaces => Flags.HasFlag(LockedFeatureFlags.TenthAgeFaces);

	public bool TrialAccount => Flags.HasFlag(LockedFeatureFlags.TrialAccount);

	public bool EleventhAge => Flags.HasFlag(LockedFeatureFlags.EleventhAge);

	public bool SA => Flags.HasFlag(LockedFeatureFlags.StygianAbyss);

	public void SetFlags(LockedFeatureFlags flags)
	{
		Flags = flags;
	}
}

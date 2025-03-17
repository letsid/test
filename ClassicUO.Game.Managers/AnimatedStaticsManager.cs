using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.IO;
using ClassicUO.IO.Resources;
using ClassicUO.Utility.Collections;

namespace ClassicUO.Game.Managers;

internal static class AnimatedStaticsManager
{
	private struct static_animation_info
	{
		public uint time;

		public ushort index;

		public byte anim_index;

		public bool is_field;
	}

	private static RawList<static_animation_info> _static_infos;

	public static uint ProcessTime;

	public unsafe static void Initialize()
	{
		if (_static_infos != null)
		{
			return;
		}
		_static_infos = new RawList<static_animation_info>();
		UOFile animDataFile = AnimDataLoader.Instance.AnimDataFile;
		if (animDataFile == null)
		{
			return;
		}
		long num = animDataFile.StartAddress.ToInt64();
		uint num2 = (uint)(num + animDataFile.Length - sizeof(AnimDataFrame));
		for (int i = 0; i < TileDataLoader.Instance.StaticData.Length; i++)
		{
			if (TileDataLoader.Instance.StaticData[i].IsAnimated)
			{
				uint num3 = (uint)(i * 68 + 4 * (i / 8 + 1));
				if ((uint)(num + num3) <= num2)
				{
					_static_infos.Add(new static_animation_info
					{
						index = (ushort)i,
						is_field = StaticFilters.IsField((ushort)i)
					});
				}
			}
		}
	}

	public unsafe static void Process()
	{
		if (_static_infos == null || _static_infos.Count == 0 || ProcessTime >= Time.Ticks)
		{
			return;
		}
		UOFile animDataFile = AnimDataLoader.Instance.AnimDataFile;
		if (animDataFile == null)
		{
			return;
		}
		uint num = 100u;
		uint num2 = Time.Ticks + 250;
		bool flag = ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.FieldsType != 0;
		long num3 = animDataFile.StartAddress.ToInt64();
		UOFileIndex[] entries = ArtLoader.Instance.Entries;
		for (int i = 0; i < _static_infos.Count; i++)
		{
			ref static_animation_info reference = ref _static_infos[i];
			if (flag && reference.is_field)
			{
				reference.anim_index = 0;
				continue;
			}
			if (reference.time < Time.Ticks)
			{
				uint num4 = (uint)(reference.index * 68 + 4 * (reference.index / 8 + 1));
				AnimDataFrame* ptr = (AnimDataFrame*)(num3 + num4);
				byte b = reference.anim_index;
				if (ptr->FrameInterval > 0)
				{
					reference.time = Time.Ticks + ptr->FrameInterval * num + 1;
				}
				else
				{
					reference.time = Time.Ticks + num;
				}
				if (b < ptr->FrameCount && reference.index + 16384 < entries.Length)
				{
					entries[reference.index + 16384].AnimOffset = ptr->FrameData[(int)b++];
				}
				if (b >= ptr->FrameCount)
				{
					b = 0;
				}
				reference.anim_index = b;
			}
			if (reference.time < num2)
			{
				num2 = reference.time;
			}
		}
		ProcessTime = num2;
	}
}

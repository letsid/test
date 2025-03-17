using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ClassicUO.Utility;

namespace ClassicUO.IO.Resources;

internal class SkillsLoader : UOFileLoader
{
	private static SkillsLoader _instance;

	private UOFileMul _file;

	public readonly List<SkillEntry> Skills = new List<SkillEntry>();

	public readonly List<SkillEntry> SortedSkills = new List<SkillEntry>();

	public static SkillsLoader Instance => _instance ?? (_instance = new SkillsLoader());

	public int SkillsCount => Skills.Count;

	private SkillsLoader()
	{
	}

	public override Task Load()
	{
		return Task.Run(delegate
		{
			if (SkillsCount <= 0)
			{
				string uOFilePath = UOFileManager.GetUOFilePath("skills.mul");
				string uOFilePath2 = UOFileManager.GetUOFilePath("Skills.idx");
				FileSystemHelper.EnsureFileExists(uOFilePath);
				FileSystemHelper.EnsureFileExists(uOFilePath2);
				_file = new UOFileMul(uOFilePath, uOFilePath2, 0, 16);
				_file.FillEntries(ref Entries);
				int i = 0;
				int num = 0;
				for (; i < Entries.Length; i++)
				{
					ref UOFileIndex validRefEntry = ref GetValidRefEntry(i);
					if (validRefEntry.Length > 0)
					{
						_file.SetData(validRefEntry.Address, validRefEntry.FileSize);
						_file.Seek(validRefEntry.Offset);
						bool hasAction = _file.ReadBool();
						string name = Encoding.UTF8.GetString(_file.ReadArray<byte>(validRefEntry.Length - 1)).TrimEnd('\0');
						SkillEntry item = new SkillEntry(num++, name, hasAction);
						Skills.Add(item);
					}
				}
				SortedSkills.AddRange(Skills);
				SortedSkills.Sort((SkillEntry a, SkillEntry b) => string.Compare(a.Name, b.Name, StringComparison.InvariantCulture));
			}
		});
	}

	public int GetSortedIndex(int index)
	{
		if (index < SkillsCount)
		{
			return SortedSkills[index].Index;
		}
		return -1;
	}
}

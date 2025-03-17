using System.Xml;
using ClassicUO.IO.Resources;
using ClassicUO.Resources;

namespace ClassicUO.Game.Managers;

internal sealed class SkillsGroup
{
	private readonly byte[] _list = new byte[60];

	public int Count;

	public bool IsMaximized;

	public string Name = ResGeneral.NoName;

	public SkillsGroup Left { get; set; }

	public SkillsGroup Right { get; set; }

	public SkillsGroup()
	{
		for (int i = 0; i < _list.Length; i++)
		{
			_list[i] = byte.MaxValue;
		}
	}

	public byte GetSkill(int index)
	{
		if (index < 0 || index >= Count)
		{
			return byte.MaxValue;
		}
		return _list[index];
	}

	public void Add(byte item)
	{
		if (!Contains(item))
		{
			_list[Count++] = item;
		}
	}

	public void Remove(byte item)
	{
		bool flag = false;
		for (int i = 0; i < Count; i++)
		{
			if (_list[i] == item)
			{
				flag = true;
				for (; i < Count - 1; i++)
				{
					_list[i] = _list[i + 1];
				}
				break;
			}
		}
		if (flag)
		{
			Count--;
			if (Count < 0)
			{
				Count = 0;
			}
			_list[Count] = byte.MaxValue;
		}
	}

	public bool Contains(byte item)
	{
		for (int i = 0; i < Count; i++)
		{
			if (_list[i] == item)
			{
				return true;
			}
		}
		return false;
	}

	public unsafe void Sort()
	{
		byte* ptr = stackalloc byte[60];
		int num = 0;
		int skillsCount = SkillsLoader.Instance.SkillsCount;
		for (int i = 0; i < skillsCount; i++)
		{
			for (int j = 0; j < Count; j++)
			{
				if (SkillsLoader.Instance.GetSortedIndex(i) == _list[j])
				{
					ptr[num++] = _list[j];
					break;
				}
			}
		}
		for (int k = 0; k < Count; k++)
		{
			_list[k] = ptr[k];
		}
	}

	public void TransferTo(SkillsGroup group)
	{
		for (int i = 0; i < Count; i++)
		{
			group.Add(_list[i]);
		}
		group.Sort();
	}

	public void Save(XmlTextWriter xml)
	{
		xml.WriteStartElement("group");
		xml.WriteAttributeString("name", Name);
		xml.WriteStartElement("skillids");
		for (int i = 0; i < Count; i++)
		{
			byte skill = GetSkill(i);
			if (skill != byte.MaxValue)
			{
				xml.WriteStartElement("skill");
				xml.WriteAttributeString("id", skill.ToString());
				xml.WriteEndElement();
			}
		}
		xml.WriteEndElement();
		xml.WriteEndElement();
	}
}

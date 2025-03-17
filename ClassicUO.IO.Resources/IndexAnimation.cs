namespace ClassicUO.IO.Resources;

internal class IndexAnimation
{
	private byte[] _uopReplaceGroupIndex;

	public AnimationGroup[] BodyConvGroups;

	public ushort Color;

	public ushort CorpseColor;

	public ushort CorpseGraphic;

	public byte FileIndex;

	public ANIMATION_FLAGS Flags;

	public ushort Graphic;

	public ushort GraphicConversion = 32768;

	public AnimationGroup[] Groups;

	public bool IsValidMUL;

	public sbyte MountedHeightOffset;

	public ANIMATION_GROUPS_TYPE Type = ANIMATION_GROUPS_TYPE.UNKNOWN;

	public AnimationGroupUop[] UopGroups;

	public bool IsUOP => (Flags & ANIMATION_FLAGS.AF_USE_UOP_ANIMATION) != 0;

	public bool HasBodyConversion
	{
		get
		{
			if ((GraphicConversion & 0x8000) == 0)
			{
				return BodyConvGroups != null;
			}
			return false;
		}
	}

	public AnimationGroupUop GetUopGroup(ref byte group)
	{
		if (group < 100 && UopGroups != null)
		{
			group = _uopReplaceGroupIndex[group];
			return UopGroups[group];
		}
		return null;
	}

	public void InitializeUOP()
	{
		if (_uopReplaceGroupIndex == null)
		{
			_uopReplaceGroupIndex = new byte[100];
			for (byte b = 0; b < 100; b++)
			{
				_uopReplaceGroupIndex[b] = b;
			}
		}
	}

	public void ReplaceUopGroup(byte old, byte newG)
	{
		_uopReplaceGroupIndex[old] = newG;
	}

	public long CalculateOffset(ushort graphic, ANIMATION_GROUPS_TYPE type, out int groupCount)
	{
		long result = 0L;
		groupCount = 0;
		ANIMATION_GROUPS aNIMATION_GROUPS = ANIMATION_GROUPS.AG_NONE;
		switch (type)
		{
		case ANIMATION_GROUPS_TYPE.MONSTER:
			aNIMATION_GROUPS = (((Flags & ANIMATION_FLAGS.AF_CALCULATE_OFFSET_BY_PEOPLE_GROUP) == 0) ? (((Flags & ANIMATION_FLAGS.AF_CALCULATE_OFFSET_BY_LOW_GROUP) != 0) ? ANIMATION_GROUPS.AG_LOW : ANIMATION_GROUPS.AG_HIGHT) : ANIMATION_GROUPS.AG_PEOPLE);
			break;
		case ANIMATION_GROUPS_TYPE.SEA_MONSTER:
			result = AnimationsLoader.CalculateHighGroupOffset(graphic);
			groupCount = 13;
			break;
		case ANIMATION_GROUPS_TYPE.ANIMAL:
			aNIMATION_GROUPS = (((Flags & ANIMATION_FLAGS.AF_CALCULATE_OFFSET_LOW_GROUP_EXTENDED) == 0) ? ANIMATION_GROUPS.AG_LOW : (((Flags & ANIMATION_FLAGS.AF_CALCULATE_OFFSET_BY_PEOPLE_GROUP) == 0) ? (((Flags & ANIMATION_FLAGS.AF_CALCULATE_OFFSET_BY_LOW_GROUP) != 0) ? ANIMATION_GROUPS.AG_LOW : ANIMATION_GROUPS.AG_HIGHT) : ANIMATION_GROUPS.AG_PEOPLE));
			break;
		default:
			aNIMATION_GROUPS = ANIMATION_GROUPS.AG_PEOPLE;
			break;
		}
		switch (aNIMATION_GROUPS)
		{
		case ANIMATION_GROUPS.AG_LOW:
			result = AnimationsLoader.CalculateLowGroupOffset(graphic);
			groupCount = 13;
			break;
		case ANIMATION_GROUPS.AG_HIGHT:
			result = AnimationsLoader.CalculateHighGroupOffset(graphic);
			groupCount = 22;
			break;
		case ANIMATION_GROUPS.AG_PEOPLE:
			result = AnimationsLoader.CalculatePeopleGroupOffset(graphic);
			groupCount = 35;
			break;
		}
		return result;
	}
}

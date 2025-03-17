using System;
using System.ComponentModel;
using System.Reflection;

namespace ClassicUO.Utility;

public static class EnumHelper
{
	public static string GetDescription(this Enum value)
	{
		Type type = value.GetType();
		string name = Enum.GetName(type, value);
		if (name != null)
		{
			FieldInfo field = type.GetField(name);
			if (field != null && Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute descriptionAttribute)
			{
				return descriptionAttribute.Description;
			}
		}
		return null;
	}
}

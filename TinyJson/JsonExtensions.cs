using System;

namespace TinyJson;

public static class JsonExtensions
{
	public static bool IsNullable(this Type type)
	{
		if (!(Nullable.GetUnderlyingType(type) != null))
		{
			return !type.IsPrimitive;
		}
		return true;
	}

	public static bool IsNumeric(this Type type)
	{
		if (type.IsEnum)
		{
			return false;
		}
		switch (Type.GetTypeCode(type))
		{
		case TypeCode.SByte:
		case TypeCode.Byte:
		case TypeCode.Int16:
		case TypeCode.UInt16:
		case TypeCode.Int32:
		case TypeCode.UInt32:
		case TypeCode.Int64:
		case TypeCode.UInt64:
		case TypeCode.Single:
		case TypeCode.Double:
		case TypeCode.Decimal:
			return true;
		case TypeCode.Object:
		{
			Type underlyingType = Nullable.GetUnderlyingType(type);
			if (underlyingType != null)
			{
				return underlyingType.IsNumeric();
			}
			return false;
		}
		default:
			return false;
		}
	}

	public static bool IsFloatingPoint(this Type type)
	{
		if (type.IsEnum)
		{
			return false;
		}
		switch (Type.GetTypeCode(type))
		{
		case TypeCode.Single:
		case TypeCode.Double:
		case TypeCode.Decimal:
			return true;
		case TypeCode.Object:
		{
			Type underlyingType = Nullable.GetUnderlyingType(type);
			if (underlyingType != null)
			{
				return underlyingType.IsFloatingPoint();
			}
			return false;
		}
		default:
			return false;
		}
	}
}

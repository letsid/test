using System;
using System.Linq;
using System.Reflection;

namespace TinyJson;

public static class TypeExtensions
{
	public static bool IsInstanceOfGenericType(this Type type, Type genericType)
	{
		while (type != null)
		{
			if (type.IsGenericType && type.GetGenericTypeDefinition() == genericType)
			{
				return true;
			}
			type = type.BaseType;
		}
		return false;
	}

	public static bool HasGenericInterface(this Type type, Type genericInterface)
	{
		if (genericInterface == null)
		{
			throw new ArgumentNullException();
		}
		Predicate<Type> interfaceTest = (Type i) => i.IsGenericType && i.GetGenericTypeDefinition().IsAssignableFrom(genericInterface);
		if (!interfaceTest(type))
		{
			return type.GetInterfaces().Any((Type i) => interfaceTest(i));
		}
		return true;
	}

	private static string UnwrapFieldName(string name)
	{
		if (!string.IsNullOrEmpty(name) && name[0] == '<')
		{
			for (int i = 1; i < name.Length; i++)
			{
				if (name[i] == '>')
				{
					return name.Substring(1, i - 1);
				}
			}
		}
		return name;
	}

	public static string UnwrappedPropertyName(this PropertyInfo property)
	{
		JsonPropertyAttribute customAttribute = property.GetCustomAttribute<JsonPropertyAttribute>(inherit: true);
		if (customAttribute != null)
		{
			return customAttribute.Name;
		}
		return property.Name;
	}

	public static string UnwrappedFieldName(this FieldInfo field)
	{
		JsonPropertyAttribute customAttribute = field.GetCustomAttribute<JsonPropertyAttribute>(inherit: true);
		if (customAttribute != null)
		{
			return customAttribute.Name;
		}
		return UnwrapFieldName(field.Name);
	}
}

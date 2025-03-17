using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace TinyJson;

public static class JsonMapper
{
	internal static Action<object, JsonBuilder> genericEncoder;

	internal static Func<Type, object, object> genericDecoder;

	internal static IDictionary<Type, Action<object, JsonBuilder>> encoders;

	internal static IDictionary<Type, Func<Type, object, object>> decoders;

	static JsonMapper()
	{
		encoders = new Dictionary<Type, Action<object, JsonBuilder>>();
		decoders = new Dictionary<Type, Func<Type, object, object>>();
		RegisterEncoder(typeof(object), DefaultEncoder.GenericEncoder());
		RegisterEncoder(typeof(IDictionary), DefaultEncoder.DictionaryEncoder());
		RegisterEncoder(typeof(IEnumerable), DefaultEncoder.EnumerableEncoder());
		RegisterEncoder(typeof(DateTime), DefaultEncoder.ZuluDateEncoder());
		RegisterDecoder(typeof(object), DefaultDecoder.GenericDecoder());
		RegisterDecoder(typeof(IDictionary), DefaultDecoder.DictionaryDecoder());
		RegisterDecoder(typeof(Array), DefaultDecoder.ArrayDecoder());
		RegisterDecoder(typeof(IList), DefaultDecoder.ListDecoder());
		RegisterDecoder(typeof(ICollection), DefaultDecoder.CollectionDecoder());
		RegisterDecoder(typeof(IEnumerable), DefaultDecoder.EnumerableDecoder());
	}

	public static void RegisterDecoder(Type type, Func<Type, object, object> decoder)
	{
		if (type == typeof(object))
		{
			genericDecoder = decoder;
		}
		else
		{
			decoders.Add(type, decoder);
		}
	}

	public static void RegisterEncoder(Type type, Action<object, JsonBuilder> encoder)
	{
		if (type == typeof(object))
		{
			genericEncoder = encoder;
		}
		else
		{
			encoders.Add(type, encoder);
		}
	}

	public static Func<Type, object, object> GetDecoder(Type type)
	{
		if (decoders.ContainsKey(type))
		{
			return decoders[type];
		}
		foreach (KeyValuePair<Type, Func<Type, object, object>> decoder in decoders)
		{
			Type key = decoder.Key;
			if (key.IsAssignableFrom(type))
			{
				return decoder.Value;
			}
			if (key.HasGenericInterface(type))
			{
				return decoder.Value;
			}
		}
		return genericDecoder;
	}

	public static Action<object, JsonBuilder> GetEncoder(Type type)
	{
		if (encoders.ContainsKey(type))
		{
			return encoders[type];
		}
		foreach (KeyValuePair<Type, Action<object, JsonBuilder>> encoder in encoders)
		{
			Type key = encoder.Key;
			if (key.IsAssignableFrom(type))
			{
				return encoder.Value;
			}
			if (key.HasGenericInterface(type))
			{
				return encoder.Value;
			}
		}
		return genericEncoder;
	}

	public static T DecodeJsonObject<T>(object jsonObj)
	{
		return (T)GetDecoder(typeof(T))(typeof(T), jsonObj);
	}

	public static void EncodeValue(object value, JsonBuilder builder)
	{
		if (JsonBuilder.IsSupported(value))
		{
			builder.AppendValue(value);
			return;
		}
		Action<object, JsonBuilder> encoder = GetEncoder(value.GetType());
		if (encoder != null)
		{
			encoder(value, builder);
		}
		else
		{
			Console.WriteLine("Encoder for " + value.GetType()?.ToString() + " not found");
		}
	}

	public static void EncodeNameValue(string name, object value, JsonBuilder builder)
	{
		builder.AppendName(name);
		EncodeValue(value, builder);
	}

	private static object ConvertValue(object value, Type type)
	{
		if (value != null)
		{
			Type type2 = Nullable.GetUnderlyingType(type) ?? type;
			if (!type.IsEnum)
			{
				TypeConverter converter = TypeDescriptor.GetConverter(type2);
				if (converter.CanConvertFrom(value.GetType()))
				{
					return converter.ConvertFrom(value);
				}
				return Convert.ChangeType(value, type2);
			}
			if (value is string)
			{
				return Enum.Parse(type, (string)value);
			}
			return Enum.ToObject(type, value);
		}
		return value;
	}

	public static object DecodeValue(object value, Type targetType)
	{
		if (value == null)
		{
			return null;
		}
		if (JsonBuilder.IsSupported(value))
		{
			value = ConvertValue(value, targetType);
		}
		if (value != null && !targetType.IsInstanceOfType(value))
		{
			value = GetDecoder(targetType)(targetType, value);
		}
		if (value != null && targetType.IsInstanceOfType(value))
		{
			return value;
		}
		Console.WriteLine("Couldn't decode: " + targetType);
		return null;
	}

	public static bool DecodeValue(object target, string name, object value, PropertyInfo[] properties, bool matchSnakeCase)
	{
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (propertyInfo.GetCustomAttribute<JsonIgnore>(inherit: true) != null)
			{
				continue;
			}
			string text = propertyInfo.UnwrappedPropertyName();
			if (matchSnakeCase)
			{
				text = text.SnakeCaseToCamelCase();
				name = name.SnakeCaseToCamelCase();
			}
			if (text.Equals(name, StringComparison.CurrentCultureIgnoreCase))
			{
				Type type = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
				if (value == null)
				{
					propertyInfo.SetValue(target, null);
					return true;
				}
				object obj = DecodeValue(value, type);
				if (obj != null && type.IsInstanceOfType(obj))
				{
					propertyInfo.SetValue(target, obj);
					return true;
				}
				return false;
			}
		}
		return false;
	}

	public static bool DecodeValue(object target, string name, object value, FieldInfo[] fields, bool matchSnakeCase)
	{
		foreach (FieldInfo fieldInfo in fields)
		{
			if (fieldInfo.GetCustomAttribute<JsonIgnore>(inherit: true) != null)
			{
				continue;
			}
			string text = fieldInfo.UnwrappedFieldName();
			if (matchSnakeCase)
			{
				text = text.SnakeCaseToCamelCase();
				name = name.SnakeCaseToCamelCase();
			}
			if (text.Equals(name, StringComparison.CurrentCultureIgnoreCase))
			{
				Type type = Nullable.GetUnderlyingType(fieldInfo.FieldType) ?? fieldInfo.FieldType;
				if (value == null)
				{
					fieldInfo.SetValue(target, null);
					return true;
				}
				object obj = DecodeValue(value, type);
				if (obj != null && type.IsInstanceOfType(obj))
				{
					fieldInfo.SetValue(target, obj);
					return true;
				}
				return false;
			}
		}
		return false;
	}
}

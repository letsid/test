using System;
using System.Collections;
using System.Reflection;

namespace TinyJson;

public static class DefaultEncoder
{
	public static Action<object, JsonBuilder> GenericEncoder()
	{
		return delegate(object obj, JsonBuilder builder)
		{
			builder.AppendBeginObject();
			Type type = obj.GetType();
			bool flag = type.GetCustomAttribute<MatchSnakeCaseAttribute>(inherit: true) != null;
			bool flag2 = true;
			PropertyInfo[] properties = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
			if (properties.Length == 0)
			{
				FieldInfo[] fields = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
				foreach (FieldInfo fieldInfo in fields)
				{
					if (fieldInfo.GetCustomAttribute<JsonIgnore>(inherit: true) == null)
					{
						if (flag2)
						{
							flag2 = false;
						}
						else
						{
							builder.AppendSeperator();
						}
						string text = fieldInfo.UnwrappedFieldName();
						if (flag)
						{
							text = text.CamelCaseToSnakeCase();
						}
						JsonMapper.EncodeNameValue(text, fieldInfo.GetValue(obj), builder);
					}
				}
			}
			else
			{
				PropertyInfo[] array = properties;
				foreach (PropertyInfo propertyInfo in array)
				{
					if (propertyInfo.GetCustomAttribute<JsonIgnore>(inherit: true) == null)
					{
						if (flag2)
						{
							flag2 = false;
						}
						else
						{
							builder.AppendSeperator();
						}
						string text2 = propertyInfo.UnwrappedPropertyName();
						if (flag)
						{
							text2 = text2.CamelCaseToSnakeCase();
						}
						JsonMapper.EncodeNameValue(text2, propertyInfo.GetValue(obj), builder);
					}
				}
			}
			builder.AppendEndObject();
		};
	}

	public static Action<object, JsonBuilder> DictionaryEncoder()
	{
		return delegate(object obj, JsonBuilder builder)
		{
			builder.AppendBeginObject();
			bool flag = true;
			IDictionary dictionary = (IDictionary)obj;
			foreach (object key in dictionary.Keys)
			{
				if (flag)
				{
					flag = false;
				}
				else
				{
					builder.AppendSeperator();
				}
				JsonMapper.EncodeNameValue(key.ToString(), dictionary[key], builder);
			}
			builder.AppendEndObject();
		};
	}

	public static Action<object, JsonBuilder> EnumerableEncoder()
	{
		return delegate(object obj, JsonBuilder builder)
		{
			builder.AppendBeginArray();
			bool flag = true;
			foreach (object item in (IEnumerable)obj)
			{
				if (flag)
				{
					flag = false;
				}
				else
				{
					builder.AppendSeperator();
				}
				JsonMapper.EncodeValue(item, builder);
			}
			builder.AppendEndArray();
		};
	}

	public static Action<object, JsonBuilder> ZuluDateEncoder()
	{
		return delegate(object obj, JsonBuilder builder)
		{
			string str = ((DateTime)obj).ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
			builder.AppendString(str);
		};
	}
}

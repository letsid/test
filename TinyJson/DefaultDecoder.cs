using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace TinyJson;

public static class DefaultDecoder
{
	public static Func<Type, object, object> GenericDecoder()
	{
		return delegate(Type type, object jsonObj)
		{
			object obj = Activator.CreateInstance(type, nonPublic: true);
			if (jsonObj is IDictionary)
			{
				PropertyInfo[] properties = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
				bool matchSnakeCase = type.GetCustomAttribute<MatchSnakeCaseAttribute>() != null;
				if (properties.Length == 0)
				{
					FieldInfo[] fields = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
					foreach (DictionaryEntry item in (IDictionary)jsonObj)
					{
						string text = (string)item.Key;
						object value = item.Value;
						if (!JsonMapper.DecodeValue(obj, text, value, fields, matchSnakeCase))
						{
							Console.WriteLine("Couldn't decode field \"" + text + "\" of " + type);
						}
					}
				}
				else
				{
					foreach (DictionaryEntry item2 in (IDictionary)jsonObj)
					{
						string text2 = (string)item2.Key;
						object value2 = item2.Value;
						if (!JsonMapper.DecodeValue(obj, text2, value2, properties, matchSnakeCase))
						{
							Console.WriteLine("Couldn't decode field \"" + text2 + "\" of " + type);
						}
					}
				}
			}
			else
			{
				Console.WriteLine("Unsupported json type: " + ((jsonObj != null) ? jsonObj.GetType().ToString() : "null"));
			}
			return obj;
		};
	}

	public static Func<Type, object, object> DictionaryDecoder()
	{
		return delegate(Type type, object jsonObj)
		{
			Console.WriteLine("Decode Dictionary");
			if (jsonObj is IDictionary<string, object>)
			{
				Dictionary<string, object> dictionary = (Dictionary<string, object>)jsonObj;
				if (type.GetGenericArguments().Length == 2)
				{
					IDictionary dictionary2 = null;
					Type type2 = type.GetGenericArguments()[0];
					Type type3 = type.GetGenericArguments()[1];
					bool flag = type3.IsNullable();
					if (type != typeof(IDictionary) && typeof(IDictionary).IsAssignableFrom(type))
					{
						Console.WriteLine("Create Dictionary Instance");
						dictionary2 = Activator.CreateInstance(type, nonPublic: true) as IDictionary;
					}
					else
					{
						Console.WriteLine("Create Dictionary Instance for IDictionary interface");
						dictionary2 = Activator.CreateInstance(typeof(Dictionary<, >).MakeGenericType(type2, type3)) as IDictionary;
					}
					{
						foreach (KeyValuePair<string, object> item in dictionary)
						{
							Console.WriteLine(item.Key + " = " + JsonMapper.DecodeValue(item.Value, type3));
							object obj = JsonMapper.DecodeValue(item.Value, type3);
							object key = item.Key;
							if (type2 == typeof(int))
							{
								key = int.Parse(item.Key);
							}
							if (obj != null || flag)
							{
								dictionary2.Add(key, obj);
							}
						}
						return dictionary2;
					}
				}
				Console.WriteLine("Unexpected type arguemtns");
			}
			if (jsonObj is IDictionary<int, object>)
			{
				Dictionary<string, object> dictionary3 = new Dictionary<string, object>();
				foreach (KeyValuePair<int, object> item2 in (Dictionary<int, object>)jsonObj)
				{
					dictionary3.Add(item2.Key.ToString(), item2.Value);
				}
				if (type.GetGenericArguments().Length == 2)
				{
					IDictionary dictionary4 = null;
					Type type4 = type.GetGenericArguments()[0];
					Type type5 = type.GetGenericArguments()[1];
					bool flag2 = type5.IsNullable();
					dictionary4 = ((!(type != typeof(IDictionary)) || !typeof(IDictionary).IsAssignableFrom(type)) ? (Activator.CreateInstance(typeof(Dictionary<, >).MakeGenericType(type4, type5)) as IDictionary) : (Activator.CreateInstance(type, nonPublic: true) as IDictionary));
					{
						foreach (KeyValuePair<string, object> item3 in dictionary3)
						{
							Console.WriteLine(item3.Key + " = " + JsonMapper.DecodeValue(item3.Value, type5));
							object obj2 = JsonMapper.DecodeValue(item3.Value, type5);
							if (obj2 != null || flag2)
							{
								dictionary4.Add(Convert.ToInt32(item3.Key), obj2);
							}
						}
						return dictionary4;
					}
				}
				Console.WriteLine("Unexpected type arguemtns");
			}
			Console.WriteLine("Couldn't decode Dictionary: " + type);
			return (object)null;
		};
	}

	public static Func<Type, object, object> ArrayDecoder()
	{
		return delegate(Type type, object jsonObj)
		{
			if (typeof(IEnumerable).IsAssignableFrom(type) && jsonObj is IList)
			{
				IList list = (IList)jsonObj;
				if (type.IsArray)
				{
					Type elementType = type.GetElementType();
					bool flag = elementType.IsNullable();
					Array array = Array.CreateInstance(elementType, list.Count);
					for (int i = 0; i < list.Count; i++)
					{
						object obj = JsonMapper.DecodeValue(list[i], elementType);
						if (obj != null || flag)
						{
							array.SetValue(obj, i);
						}
					}
					return array;
				}
			}
			Console.WriteLine("Couldn't decode Array: " + type);
			return (object)null;
		};
	}

	public static Func<Type, object, object> ListDecoder()
	{
		return delegate(Type type, object jsonObj)
		{
			if (type.HasGenericInterface(typeof(IList<>)) && type.GetGenericArguments().Length == 1)
			{
				Type type2 = type.GetGenericArguments()[0];
				if (jsonObj is IList)
				{
					IList list = (IList)jsonObj;
					IList list2 = null;
					bool flag = type2.IsNullable();
					list2 = ((!(type != typeof(IList)) || !typeof(IList).IsAssignableFrom(type)) ? (Activator.CreateInstance(typeof(List<>).MakeGenericType(type2)) as IList) : (Activator.CreateInstance(type, nonPublic: true) as IList));
					{
						foreach (object item in list)
						{
							object obj = JsonMapper.DecodeValue(item, type2);
							if (obj != null || flag)
							{
								list2.Add(obj);
							}
						}
						return list2;
					}
				}
			}
			Console.WriteLine("Couldn't decode List: " + type);
			return (object)null;
		};
	}

	public static Func<Type, object, object> CollectionDecoder()
	{
		return delegate(Type type, object jsonObj)
		{
			if (type.HasGenericInterface(typeof(ICollection<>)))
			{
				Type type2 = type.GetGenericArguments()[0];
				if (jsonObj is IList)
				{
					IList list = (IList)jsonObj;
					object obj = Activator.CreateInstance((type.IsInstanceOfGenericType(typeof(HashSet<>)) ? typeof(HashSet<>) : typeof(List<>)).MakeGenericType(type2), nonPublic: true);
					bool flag = type2.IsNullable();
					MethodInfo method = type.GetMethod("Add");
					if (method != null)
					{
						foreach (object item in list)
						{
							object obj2 = JsonMapper.DecodeValue(item, type2);
							if (obj2 != null || flag)
							{
								method.Invoke(obj, new object[1] { obj2 });
							}
						}
						return obj;
					}
				}
			}
			Console.WriteLine("Couldn't decode Collection: " + type);
			return (object)null;
		};
	}

	public static Func<Type, object, object> EnumerableDecoder()
	{
		return delegate(Type type, object jsonObj)
		{
			if (typeof(IEnumerable).IsAssignableFrom(type))
			{
				if (jsonObj is IDictionary)
				{
					return DictionaryDecoder()(type, jsonObj);
				}
				if (jsonObj is IList)
				{
					if (type.IsArray)
					{
						return ArrayDecoder()(type, jsonObj);
					}
					if (type.HasGenericInterface(typeof(IList<>)))
					{
						return ListDecoder()(type, jsonObj);
					}
					if (type.HasGenericInterface(typeof(ICollection<>)))
					{
						return CollectionDecoder()(type, jsonObj);
					}
				}
			}
			Console.WriteLine("Couldn't decode Enumerable: " + type);
			return (object)null;
		};
	}
}

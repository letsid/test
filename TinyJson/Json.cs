namespace TinyJson;

public static class Json
{
	public static T Decode<T>(this string json)
	{
		if (string.IsNullOrEmpty(json))
		{
			return default(T);
		}
		object obj = JsonParser.ParseValue(json);
		if (obj == null)
		{
			return default(T);
		}
		return JsonMapper.DecodeJsonObject<T>(obj);
	}

	public static string Encode(this object value, bool pretty = false)
	{
		JsonBuilder jsonBuilder = new JsonBuilder(pretty);
		JsonMapper.EncodeValue(value, jsonBuilder);
		return jsonBuilder.ToString();
	}
}

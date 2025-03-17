using System;

namespace TinyJson;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class JsonPropertyAttribute : Attribute
{
	public string Name { get; }

	public JsonPropertyAttribute(string name)
	{
		Name = name;
	}
}

using System;

namespace TinyJson;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class JsonIgnore : Attribute
{
}

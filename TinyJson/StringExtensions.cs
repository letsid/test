using ClassicUO.Utility;

namespace TinyJson;

public static class StringExtensions
{
	public static string SnakeCaseToCamelCase(this string snakeCaseName)
	{
		bool flag = true;
		ValueStringBuilder valueStringBuilder = new ValueStringBuilder(snakeCaseName.Length);
		for (int i = 0; i < snakeCaseName.Length; i++)
		{
			if (snakeCaseName[i] == '_')
			{
				flag = true;
			}
			else if (flag)
			{
				valueStringBuilder.Append(char.ToUpperInvariant(snakeCaseName[i]));
				flag = false;
			}
			else
			{
				valueStringBuilder.Append(snakeCaseName[i]);
			}
		}
		string result = valueStringBuilder.ToString();
		valueStringBuilder.Dispose();
		return result;
	}

	public static string CamelCaseToSnakeCase(this string camelCaseName)
	{
		ValueStringBuilder valueStringBuilder = new ValueStringBuilder(camelCaseName.Length * 2);
		if (char.IsUpper(camelCaseName[0]))
		{
			valueStringBuilder.Append(char.ToLowerInvariant(camelCaseName[0]));
		}
		for (int i = 1; i < camelCaseName.Length; i++)
		{
			if (char.IsUpper(camelCaseName[i]))
			{
				valueStringBuilder.Append("_");
				valueStringBuilder.Append(char.ToLowerInvariant(camelCaseName[i]));
			}
			else
			{
				valueStringBuilder.Append(camelCaseName[i]);
			}
		}
		string result = valueStringBuilder.ToString();
		valueStringBuilder.Dispose();
		return result;
	}
}

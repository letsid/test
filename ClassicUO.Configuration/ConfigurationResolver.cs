using System.IO;
using System.Text.RegularExpressions;
using ClassicUO.Utility.Logging;
using TinyJson;

namespace ClassicUO.Configuration;

internal static class ConfigurationResolver
{
	public static T Load<T>(string file) where T : class
	{
		if (!File.Exists(file))
		{
			Log.Warn(file + " not found.");
			return null;
		}
		return Regex.Replace(File.ReadAllText(file), "(?<!\\\\)  # lookbehind: Check that previous character isn't a \\\r\n                                                \\\\         # match a \\\r\n                                                (?!\\\\)     # lookahead: Check that the following character isn't a \\", "\\\\", RegexOptions.IgnorePatternWhitespace).Decode<T>();
	}

	public static void Save<T>(T obj, string file) where T : class
	{
		try
		{
			FileInfo fileInfo = new FileInfo(file);
			if (fileInfo.Directory != null && !fileInfo.Directory.Exists)
			{
				fileInfo.Directory.Create();
			}
			File.WriteAllText(file, obj.Encode(pretty: true));
		}
		catch (IOException ex)
		{
			Log.Error(ex.ToString());
		}
	}
}

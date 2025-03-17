using System;
using System.IO;
using System.Text;

namespace ClassicUO.Utility;

internal static class FileSystemHelper
{
	public static string CreateFolderIfNotExists(string path, params string[] parts)
	{
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
		for (int i = 0; i < parts.Length; i++)
		{
			for (int j = 0; j < invalidFileNameChars.Length; j++)
			{
				parts[i] = parts[i].Replace(invalidFileNameChars[j].ToString(), "");
			}
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string path2 in parts)
		{
			stringBuilder.Append(Path.Combine(path, path2));
			string text = stringBuilder.ToString();
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			path = text;
			stringBuilder.Clear();
		}
		return path;
	}

	public static void EnsureFileExists(string path)
	{
		if (!File.Exists(path))
		{
			throw new FileNotFoundException(path);
		}
	}

	public static void CopyAllTo(this DirectoryInfo source, DirectoryInfo target)
	{
		Directory.CreateDirectory(target.FullName);
		FileInfo[] files = source.GetFiles();
		foreach (FileInfo fileInfo in files)
		{
			Console.WriteLine("Copying {0}\\{1}", target.FullName, fileInfo.Name);
			fileInfo.CopyTo(Path.Combine(target.FullName, fileInfo.Name), overwrite: true);
		}
		DirectoryInfo[] directories = source.GetDirectories();
		foreach (DirectoryInfo directoryInfo in directories)
		{
			DirectoryInfo target2 = target.CreateSubdirectory(directoryInfo.Name);
			directoryInfo.CopyAllTo(target2);
		}
	}
}

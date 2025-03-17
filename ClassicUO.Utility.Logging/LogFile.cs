using System;
using System.Buffers;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ClassicUO.Utility.Logging;

internal sealed class LogFile : IDisposable
{
	private readonly FileStream logStream;

	public LogFile(string directory, string file)
	{
		logStream = new FileStream($"{directory}/{DateTime.Now:yyyy-MM-dd_hh-mm-ss}_{file}", FileMode.Append, FileAccess.Write, FileShare.ReadWrite, 4096, useAsync: true);
	}

	public void Dispose()
	{
		logStream.Close();
	}

	public void Write(string message)
	{
		byte[] array = ArrayPool<byte>.Shared.Rent(message.Length);
		try
		{
			Encoding.UTF8.GetBytes(message, 0, message.Length, array, 0);
			logStream.Write(array, 0, message.Length);
			logStream.WriteByte(10);
			logStream.Flush();
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(array);
		}
	}

	public async Task WriteAsync(string message)
	{
		byte[] buffer = ArrayPool<byte>.Shared.Rent(message.Length);
		try
		{
			Encoding.UTF8.GetBytes(message, 0, message.Length, buffer, 0);
			await logStream.WriteAsync(buffer, 0, message.Length);
			logStream.WriteByte(10);
			await logStream.FlushAsync();
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buffer);
		}
	}

	public override string ToString()
	{
		return logStream.Name;
	}
}

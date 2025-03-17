using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using ClassicUO.IO;
using ClassicUO.Network;

namespace ClassicUO.Utility;

public class CheckSumCalculator
{
	private static List<CheckSumJob> _jobs;

	private static List<byte[]> _tosend;

	private static Thread _thread;

	private static bool stopRequest;

	private static DateTime nextSend;

	static CheckSumCalculator()
	{
		_jobs = new List<CheckSumJob>();
		_tosend = new List<byte[]>();
		_thread = null;
		stopRequest = false;
		nextSend = DateTime.Now;
		if (_thread == null)
		{
			_thread = new Thread(Work);
			_thread.Start();
		}
	}

	public static void AddCheckSumJob(CheckSumJob j)
	{
		lock (_jobs)
		{
			_jobs.Add(j);
		}
	}

	public static void RequestStop()
	{
		stopRequest = true;
	}

	private static void Work()
	{
		while (!stopRequest)
		{
			CheckSumJob checkSumJob = new CheckSumJob("");
			bool flag = false;
			lock (_jobs)
			{
				if (_jobs.Count > 0)
				{
					checkSumJob = _jobs[0];
					_jobs.RemoveAt(0);
					flag = true;
				}
			}
			if (flag && checkSumJob.filename != null)
			{
				string uOFilePath = UOFileManager.GetUOFilePath(checkSumJob.filename);
				if (File.Exists(uOFilePath))
				{
					byte[] array = CalculateSHA1(uOFilePath, checkSumJob.inputstream);
					string fileName = Path.GetFileName(uOFilePath);
					StackDataWriter stackDataWriter = default(StackDataWriter);
					stackDataWriter.WriteUInt8(12);
					stackDataWriter.WriteInt16BE((short)(26 + fileName.Length + 1));
					stackDataWriter.WriteInt16BE(1);
					stackDataWriter.WriteUInt8(1);
					stackDataWriter.Write(array);
					stackDataWriter.WriteASCII(fileName);
					lock (_tosend)
					{
						_tosend.Add(new ReadOnlySpan<byte>(stackDataWriter.AllocatedBuffer, 0, stackDataWriter.BytesWritten).ToArray());
					}
				}
			}
			Thread.Sleep(250);
		}
	}

	public static void HandlePending()
	{
		if (nextSend > DateTime.Now || NetClient.Socket == null)
		{
			return;
		}
		nextSend = DateTime.Now + TimeSpan.FromSeconds(2.0);
		lock (_tosend)
		{
			for (int i = 0; i < _tosend.Count; i++)
			{
				NetClient.Socket.Send(_tosend[i], _tosend[i].Length);
			}
			_tosend.Clear();
		}
	}

	private static byte[] CalculateSHA1(string filename, Stream input)
	{
		Stream stream = input;
		if (stream == null)
		{
			stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
		}
		using BufferedStream inputStream = new BufferedStream(stream);
		using SHA1Managed sHA1Managed = new SHA1Managed();
		byte[] result = sHA1Managed.ComputeHash(inputStream);
		stream.Close();
		return result;
	}
}

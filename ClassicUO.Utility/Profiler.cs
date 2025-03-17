using System;
using System.Collections.Generic;
using System.Diagnostics;
using ClassicUO.Utility.Logging;

namespace ClassicUO.Utility;

internal static class Profiler
{
	internal class ProfileData
	{
		public static ProfileData Empty = new ProfileData(null, 0.0);

		private uint m_LastIndex;

		private readonly double[] m_LastTimes = new double[60];

		public string[] Context;

		public double LastTime => m_LastTimes[m_LastIndex % 60];

		public double TimeInContext
		{
			get
			{
				double num = 0.0;
				for (int i = 0; i < 60; i++)
				{
					num += m_LastTimes[i];
				}
				return num;
			}
		}

		public double AverageTime => TimeInContext / 60.0;

		public ProfileData(string[] context, double time)
		{
			Context = context;
			m_LastIndex = 0u;
			AddNewHitLength(time);
		}

		public bool MatchesContext(string[] context)
		{
			if (Context.Length != context.Length)
			{
				return false;
			}
			for (int i = 0; i < Context.Length; i++)
			{
				if (Context[i] != context[i])
				{
					return false;
				}
			}
			return true;
		}

		public void AddNewHitLength(double time)
		{
			m_LastTimes[m_LastIndex % 60] = time;
			m_LastIndex++;
		}

		public override string ToString()
		{
			string text = string.Empty;
			for (int i = 0; i < Context.Length; i++)
			{
				if (text != string.Empty)
				{
					text += ":";
				}
				text += Context[i];
			}
			return $"{text} - {TimeInContext:0.0}ms";
		}
	}

	private readonly struct ContextAndTick
	{
		public readonly string Name;

		public readonly long Tick;

		public ContextAndTick(string name, long tick)
		{
			Name = name;
			Tick = tick;
		}

		public override string ToString()
		{
			return $"{Name} [{Tick}]";
		}
	}

	public const int ProfileTimeCount = 60;

	private static readonly List<ContextAndTick> m_Context;

	private static readonly List<Tuple<string[], double>> m_ThisFrameData;

	private static readonly List<ProfileData> m_AllFrameData;

	private static readonly ProfileData m_TotalTimeData;

	private static readonly Stopwatch _timer;

	private static long m_BeginFrameTicks;

	public static double LastFrameTimeMS { get; private set; }

	public static double TrackedTime => m_TotalTimeData.TimeInContext;

	static Profiler()
	{
		m_Context = new List<ContextAndTick>();
		m_ThisFrameData = new List<Tuple<string[], double>>();
		m_AllFrameData = new List<ProfileData>();
		m_TotalTimeData = new ProfileData(null, 0.0);
		_timer = Stopwatch.StartNew();
	}

	public static void BeginFrame()
	{
		if (!CUOEnviroment.Profiler)
		{
			return;
		}
		if (m_ThisFrameData.Count > 0)
		{
			foreach (Tuple<string[], double> thisFrameDatum in m_ThisFrameData)
			{
				bool flag = false;
				foreach (ProfileData allFrameDatum in m_AllFrameData)
				{
					if (allFrameDatum.MatchesContext(thisFrameDatum.Item1))
					{
						allFrameDatum.AddNewHitLength(thisFrameDatum.Item2);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					m_AllFrameData.Add(new ProfileData(thisFrameDatum.Item1, thisFrameDatum.Item2));
				}
			}
			m_ThisFrameData.Clear();
		}
		m_BeginFrameTicks = _timer.ElapsedTicks;
	}

	public static void EndFrame()
	{
		if (CUOEnviroment.Profiler)
		{
			LastFrameTimeMS = (double)(_timer.ElapsedTicks - m_BeginFrameTicks) * 1000.0 / (double)Stopwatch.Frequency;
			m_TotalTimeData.AddNewHitLength(LastFrameTimeMS);
		}
	}

	public static void EnterContext(string context_name)
	{
		if (CUOEnviroment.Profiler)
		{
			m_Context.Add(new ContextAndTick(context_name, _timer.ElapsedTicks));
		}
	}

	public static void ExitContext(string context_name)
	{
		if (CUOEnviroment.Profiler)
		{
			if (m_Context[m_Context.Count - 1].Name != context_name)
			{
				Log.Error("Profiler.ExitProfiledContext: context_name does not match current context.");
			}
			string[] array = new string[m_Context.Count];
			for (int i = 0; i < m_Context.Count; i++)
			{
				array[i] = m_Context[i].Name;
			}
			double item = (double)(_timer.ElapsedTicks - m_Context[m_Context.Count - 1].Tick) * 1000.0 / (double)Stopwatch.Frequency;
			m_ThisFrameData.Add(new Tuple<string[], double>(array, item));
			m_Context.RemoveAt(m_Context.Count - 1);
		}
	}

	public static bool InContext(string context_name)
	{
		if (!CUOEnviroment.Profiler)
		{
			return false;
		}
		if (m_Context.Count == 0)
		{
			return false;
		}
		return m_Context[m_Context.Count - 1].Name == context_name;
	}

	public static ProfileData GetContext(string context_name)
	{
		if (!CUOEnviroment.Profiler)
		{
			return ProfileData.Empty;
		}
		for (int i = 0; i < m_AllFrameData.Count; i++)
		{
			if (m_AllFrameData[i].Context[m_AllFrameData[i].Context.Length - 1] == context_name)
			{
				return m_AllFrameData[i];
			}
		}
		return ProfileData.Empty;
	}
}

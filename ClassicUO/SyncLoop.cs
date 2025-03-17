using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ClassicUO;

public class SyncLoop : SynchronizationContext
{
	private ConcurrentQueue<Action> _queue = new ConcurrentQueue<Action>();

	private Thread _mainThread { get; set; }

	public SyncLoop()
	{
		_queue = new ConcurrentQueue<Action>();
		_mainThread = Thread.CurrentThread;
	}

	public override void Post(SendOrPostCallback d, object state)
	{
		_queue.Enqueue(delegate
		{
			d(state);
		});
	}

	public override SynchronizationContext CreateCopy()
	{
		return new SyncLoop();
	}

	public override void Send(SendOrPostCallback d, object state)
	{
		if (Thread.CurrentThread == _mainThread)
		{
			d(state);
			return;
		}
		AutoResetEvent autoResetEvent = new AutoResetEvent(initialState: false);
		_queue.Enqueue(delegate
		{
			d(state);
			autoResetEvent.Set();
		});
		autoResetEvent.WaitOne();
	}

	public void ExecuteTask()
	{
		if (Thread.CurrentThread != _mainThread)
		{
			throw new Exception("This is not the main thread!");
		}
		Action result;
		while (!_queue.IsEmpty && _queue.TryDequeue(out result))
		{
			result();
		}
	}
}

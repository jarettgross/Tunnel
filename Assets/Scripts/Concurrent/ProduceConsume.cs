using UnityEngine;
using System.Collections.Generic;
using System.Threading;

public class ProduceConsume<T> {

	readonly object listLock = new object ();
	Queue<T> queue = new Queue<T> ();

	public void Produce(T o)
	{
		lock (listLock)
		{
			queue.Enqueue(o);

			// We always need to pulse, even if the queue wasn't
			// empty before. Otherwise, if we add several items
			// in quick succession, we may only pulse once, waking
			// a single thread up, even if there are multiple threads
			// waiting for items.            
			Monitor.Pulse(listLock);
		}
	}

	public T Consume()
	{
		lock (listLock)
		{
			// If the queue is empty, wait for an item to be added
			// Note that this is a while loop, as we may be pulsed
			// but not wake up before another thread has come in and
			// consumed the newly added object. In that case, we'll
			// have to wait for another pulse.
			while (queue.Count==0)
			{
				// This releases listLock, only reacquiring it
				// after being woken up by a call to Pulse
				Monitor.Wait(listLock);
			}
			return queue.Dequeue();
		}
	}

	public T ConsumeNonBlock() {
		lock (listLock) {
	
			if (queue.Count == 0)
				return default(T);

			return queue.Dequeue ();
		}
	}

}

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace CCBA.Integrations.Base.Helpers
{
    /// <summary>
    /// Developer: Johan Nieuwenhuis
    /// </summary>
    public class ThreadManager
    {
        public readonly ConcurrentDictionary<Guid, Thread> Threads = new ConcurrentDictionary<Guid, Thread>();

        public void ThreadedAction(Action action)
        {
            var guid = Guid.NewGuid();
            var t = new Thread(() =>
            {
                action.Invoke();
                while (Threads.TryRemove(guid, out _))
                {
                    Thread.Sleep(1000);
                }
            })
            { IsBackground = true, Priority = ThreadPriority.Lowest };
            while (!Threads.TryAdd(guid, t))
            {
                Thread.Sleep(1);
            }
            t.Start();
        }
    }
}
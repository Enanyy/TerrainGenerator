using System.Collections.Generic;
using System;
using System.Threading;

public static class ThreadQueue  {

	static Queue<IThread> threadQueue = new Queue<IThread>();
   
	public static void DoFunc(Func<object> func, Action<object> callback) {
        ThreadStart thread = delegate() {

            object data = func();
            lock (threadQueue)
            {
                threadQueue.Enqueue(new ThreadFunc(callback, data));
            }
        };

        //ThreadPool.QueueUserWorkItem(thread);

		new Thread (thread).Start ();
	}

    public static void DoAction(Action func, Action callback)
    {
        ThreadStart thread = delegate () {

            if (func != null)
            {
                func();
            }
            lock (threadQueue)
            {
                threadQueue.Enqueue(new ThreadAction(callback));
            }
        };
        //ThreadPool.QueueUserWorkItem(thread);
        new Thread(thread).Start();

    }

    public static void Update()
    {
        if (threadQueue.Count > 0)
        {
            for (int i = 0; i < threadQueue.Count; i++)
            {
                IThread thread = threadQueue.Dequeue();
                thread.Invoke();
            }
        }
    }

    interface IThread
    {
        void Invoke();
    }
    class ThreadAction : IThread
    {
        public readonly Action callback;
        public ThreadAction(Action callback)
        {
            this.callback = callback;
        }

        public void Invoke()
        {
            if(callback!=null)
            {
                callback();
            }
        }

    }
   
    class ThreadFunc:IThread
    {
		public readonly Action<object> callback;
		public readonly object data;

		public ThreadFunc (Action<object> callback, object data)
		{
			this.callback = callback;
			this.data = data;
		}

        public void Invoke()
        {
            if(callback!= null)
            {
                callback(data);
            }
        }
	}

}

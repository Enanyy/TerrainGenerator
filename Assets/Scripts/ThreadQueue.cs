using System.Collections.Generic;
using System;
using System.Threading;

public static class ThreadQueue  {

	static Queue<IThread> threadQueue = new Queue<IThread>();
   
	public static void DoFunc(Func<object> func, Action<object> callback)
    {
        ThreadStart thread = delegate() {
            object data = null;
            Exception exception = null;
            try
            {
                if (func != null)
                {
                    data = func();
                }
            }
            catch (Exception e)
            {
                exception = e;
            }
            lock (threadQueue)
            {
                threadQueue.Enqueue(new ThreadFunc(callback, data, exception));
            }
        };

		new Thread (thread).Start ();
	}

    public static void DoAction(Action func, Action callback)
    {
        ThreadStart thread = delegate () {
            Exception exception = null;
            try
            {
                if (func != null)
                {
                    func();
                }
            }
            catch(Exception e)
            {
                exception = e;
            }
            
            lock (threadQueue)
            {
                threadQueue.Enqueue(new ThreadAction(callback, exception));
            }
        };

        new Thread(thread).Start();

    }

    public static void Update()
    {
        while (threadQueue.Count > 0)
        {
            var thread = threadQueue.Dequeue();
            if (thread != null)
            {
                if (thread.exception == null)
                {
                    thread.OnCompleted();
                }
                else
                {
                    UnityEngine.Debug.LogError(thread.exception);
                }
            }
        }
    }

    interface IThread
    {
        Exception exception { get; }
        void OnCompleted();

    }
    class ThreadAction : IThread
    {
        public Exception exception { get; private set; }
        public readonly Action callback;
        public ThreadAction(Action callback,Exception exception)
        {
            this.callback = callback;
            this.exception = exception;
        }

        public void OnCompleted()
        {
            if(callback!=null)
            {
                callback();
            }
        }

    }
   
    class ThreadFunc:IThread
    {
        public Exception exception { get; private set; }

        public readonly Action<object> callback;
		public readonly object data;

		public ThreadFunc (Action<object> callback, object data, Exception exception)
		{
			this.callback = callback;
			this.data = data;
            this.exception = exception;
		}

        public void OnCompleted()
        {
            if(callback!= null)
            {
                callback(data);
            }
        }
	}

}

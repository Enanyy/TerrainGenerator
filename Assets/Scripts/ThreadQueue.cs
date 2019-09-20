using System.Collections.Generic;
using System;
using System.Threading;

public static class ThreadQueue  {

	static List<IThread> threads = new List<IThread>();
    public static int maxThreads = 8;
    static int numThreads;
    public static void DoFunc<T>(Func<T> func, Action<T> callback)
    {      
        threads.Add(new ThreadFunc<T>(func,callback));
	}

    public static void DoAction(Action func, Action callback, float delay = 0)
    {
        if (delay <= 0)
        {
            threads.Add(new ThreadAction(func, callback));
        }
        else
        {
            threads.Add(new ThreadDelayAction(func, callback, delay));
        }
    }

    

    public static void Update()
    {
        for(int i = 0; i < threads.Count;)
        {
            var t = threads[i];
            if(t == null || t.isCompleted)
            {
                if (t.exception == null)
                {
                    t.OnCompleted();
                }
                else
                {
                    UnityEngine.Debug.LogError(t.exception);
                }

                threads.RemoveAt(i);

                continue;  
            }
            else if (t.isExecuted == false)
            {
                t.Execute();
            }


            i++;
        }
    }

    public static bool RunAsync(Action action)
    {
        if(numThreads>= maxThreads)
        {
            return false;
        }
        else
        {
            Interlocked.Increment(ref numThreads);

            new Thread(delegate () { RunThread(action); }).Start();

            return true;
        }

    }
    private static void RunThread(Action action)
    {
        try
        {
            if(action!= null)
            {
                action();
            }
        }
        catch
        {

        }
        finally
        {
            Interlocked.Decrement(ref numThreads);
        }
    }


    interface IThread
    {
        Exception exception { get; }
        bool isExecuted { get; }
        bool isCompleted { get; }
        void Execute();
        void OnCompleted();

    }
    class ThreadAction : IThread
    {
        public Exception exception { get; private set; }
        public readonly Action callback;
        public readonly Action func;
        public bool isExecuted { get; private set; }
        public bool isCompleted { get; private set; }
        public ThreadAction(Action func, Action callback)
        {
            this.callback = callback;
            this.func = func;
            isCompleted = false;
            isExecuted = false;
        }

        public void Execute()
        {
           isExecuted = RunAsync(() => {
                try
                {
                    if(func!= null)
                    {
                        func();
                    }
                   isCompleted = true;
                }
                catch(Exception e)
                {
                    exception = e;
                }
            });
        }


        public void OnCompleted()
        {
            if(callback!=null)
            {
                callback();
            }
        }

    }
   
    class ThreadFunc<T>:IThread
    {
        public Exception exception { get; private set; }

        public readonly Action<T> callback;
		public T data { get; private set; }
        public readonly Func<T> func;

        public bool isExecuted { get; private set; }
        public bool isCompleted { get; private set; }
        public ThreadFunc (Func<T> func, Action<T> callback)
		{
			this.callback = callback;
            this.func = func;
		}
        public void Execute()
        {
            isExecuted = RunAsync(() =>
            {
                try
                {
                    if (func != null)
                    {
                       data = func();
                    }
                    isCompleted = true;
                }
                catch (Exception e)
                {
                    exception = e;
                }
            });
        }

        public void OnCompleted()
        {
            if(callback!= null)
            {
                callback(data);
            }
        }
	}

    class ThreadDelayAction:IThread
    {
        public Exception exception { get; private set; }
        public readonly Action callback;
        public readonly Action func;
        public bool isExecuted { get; private set; }
        public bool isCompleted { get; private set; }

        private float mDelay;
        public ThreadDelayAction(Action func, Action callback, float delay)
        {
            mDelay = delay;
            this.callback = callback;
            this.func = func;
            isCompleted = false;
            isExecuted = false;
        }

        public void Execute()
        {
            if (mDelay > 0)
            {
                mDelay -= UnityEngine.Time.deltaTime;
            }
            else
            {
                isExecuted = RunAsync(() =>
                {
                    try
                    {
                        if (func != null)
                        {
                            func();
                        }
                        isCompleted = true;
                    }
                    catch (Exception e)
                    {
                        exception = e;
                    }
                });
            }
        }


        public void OnCompleted()
        {
            if (callback != null)
            {
                callback();
            }
        }
    }

}

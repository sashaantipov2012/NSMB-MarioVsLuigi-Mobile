using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sych.ShareAssets.Runtime.Tools
{
    internal sealed class UnityMainThreadDispatcher : MonoBehaviour
    {
        private static readonly Queue<Action> _executionQueue = new();
        private static UnityMainThreadDispatcher _instance;

        public static UnityMainThreadDispatcher Instance()
        {
            if (_instance)
                return _instance;

            var obj = new GameObject("UnityMainThreadDispatcher");
            _instance = obj.AddComponent<UnityMainThreadDispatcher>();
            DontDestroyOnLoad(obj);
            return _instance;
        }
        
        public void Update()
        {
            lock (_executionQueue)
                while (_executionQueue.Count > 0)
                    _executionQueue.Dequeue().Invoke();
        }

        public void Enqueue(Action action)
        {
            lock (_executionQueue)
                _executionQueue.Enqueue(action);
        }
    }
    
    public static class UnityMainThreadDispatcherExtensions
    {
        public static void InvokeInUnityThread(this Action action) => UnityMainThreadDispatcher.Instance().Enqueue(action);

        public static void InvokeInUnityThread<T>(this Action<T> action, T arg) => UnityMainThreadDispatcher.Instance().Enqueue(() => action(arg));
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Sych.ShareAssets.Runtime.Tools;
using UnityEngine;
using UnityEngine.Scripting;

namespace Sych.ShareAssets.Runtime.Android
{
    [Preserve]
    public sealed class ShareNativeBridge : IShareBridge
    {
        private const string JavaClass = "com.sych.share.ShareUtils";

        [Preserve]
        private class ShareCallback : AndroidJavaProxy
        {
            private readonly Action<bool> _callback;

            public ShareCallback(Action<bool> callback) : base("com.sych.share.ShareUtils$ShareResultCallback") => _callback = callback;

            public void onShareResult(bool success) => _callback?.InvokeInUnityThread(success);
        }

        public void ShareItems(List<string> items, Action<bool> completeCallback)
        {
            if (items == null || items.Count == 0)
                throw new ArgumentException("Items is null or empty");
            
            if (items.Any(string.IsNullOrEmpty))
                throw new ArgumentException("Items contains null or empty elements");

            using var pluginClass = new AndroidJavaClass(JavaClass);
            var javaArray = items.ToArray();
            var callback = new ShareCallback(completeCallback);
            pluginClass.CallStatic("share", javaArray, callback);
        }
    }
}

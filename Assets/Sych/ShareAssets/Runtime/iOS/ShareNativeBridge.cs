using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Sych.ShareAssets.Runtime.Tools;

namespace Sych.ShareAssets.Runtime.iOS
{
    public sealed class ShareNativeBridge : IShareBridge
    {
        private const string DllName = "__Internal";

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void CompleteDelegate(bool success);

        private static Action<bool> _callback;

        [DllImport(DllName)]
        private static extern void share(string items, CompleteDelegate callback);
        
        public void ShareItems(List<string> items, Action<bool> completeCallback)
        {
            if (items == null || items.Count == 0)
                throw new ArgumentException("Items is null or empty");
            
            if(items.Any(string.IsNullOrEmpty))
                throw new ArgumentException("Items contains null or empty elements");
            
            _callback = completeCallback;
            var itemsArray = string.Join("|", items);
            share(itemsArray, OnCompleteCallback);
        }

        [AOT.MonoPInvokeCallback(typeof(CompleteDelegate))]
        private static void OnCompleteCallback(bool success) => _callback?.InvokeInUnityThread(success);
    }
}
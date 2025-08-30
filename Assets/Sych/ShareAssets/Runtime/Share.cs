using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sych.ShareAssets.Runtime.Tools;
using UnityEngine;
using static Sych.ShareAssets.Runtime.Constants;

namespace Sych.ShareAssets.Runtime
{
    public static class Share
    {
        private static readonly IShareBridge Bridge;

        /// <summary>
        /// Returns true if the current platform supports sharing.
        /// </summary>
        public static bool IsPlatformSupported { get; }

        static Share()
        {
            switch (Application.isEditor)
            {
                case false when Application.platform == RuntimePlatform.IPhonePlayer && ReflectionUtils.TryToCreateInstance<IShareBridge>(IOSBridgeType, IOSAssembly, out var iosBridge):
                    IsPlatformSupported = true;
                    Bridge = iosBridge;
                    break;
                case false when Application.platform == RuntimePlatform.Android && ReflectionUtils.TryToCreateInstance<IShareBridge>(AndroidBridgeType, AndroidAssembly, out var androidBridge):
                    IsPlatformSupported = true;
                    Bridge = androidBridge;
                    break;
                default:
                    IsPlatformSupported = false;
                    Bridge = new NullShareBridge();
                    break;
            }
        }

        /// <summary>
        /// Asynchronous version of Item(), returns a bool indicating whether the share action was completed.
        /// </summary>
        /// <param name="item">An item to share. Item can be plain text, URLs, images, videos, audio files, or any file path.</param>
        public static async Task<bool> ItemAsync(string item)
        {
            var tcs = new TaskCompletionSource<bool>();
            Item(item, success => tcs.SetResult(success));
            return await tcs.Task;
        }

        /// <summary>
        /// Share the provided item.
        /// The callback returns true if the share window was successfully opened, or false if an error occurred.
        /// </summary>
        /// <param name="item">An item to share. Item can be plain text, URLs, images, videos, audio files, or any file path.</param>
        /// <param name="completeCallback">Callback invoked with a bool indicating the completion status of the share operation.</param>
        public static void Item(string item, Action<bool> completeCallback) => Bridge.ShareItems(new List<string> { item }, completeCallback);

        [Obsolete("Sharing multiple content types at once (e.g., text + file) may not be supported by many target apps. Use ShareAsync(string item) for reliable behavior.")]
        public static async Task<bool> ItemsAsync(List<string> items)
        {
            var tcs = new TaskCompletionSource<bool>();
            Items(items, success => tcs.SetResult(success));
            return await tcs.Task;
        }

        [Obsolete("Sharing multiple content types at once (e.g., text + file) may not be supported by many target apps. Use ShareAsync(string item) for reliable behavior.")]
        public static void Items(List<string> items, Action<bool> completeCallback) => Bridge.ShareItems(items, completeCallback);
    }
}
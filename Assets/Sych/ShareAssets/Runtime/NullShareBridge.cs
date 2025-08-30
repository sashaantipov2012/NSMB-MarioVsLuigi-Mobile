using System;
using System.Collections.Generic;

namespace Sych.ShareAssets.Runtime
{
    internal sealed class NullShareBridge : IShareBridge
    {
        public void ShareItems(List<string> items, Action<bool> completeCallback) => completeCallback?.Invoke(false);
    }
}
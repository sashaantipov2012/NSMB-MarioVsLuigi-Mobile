using System;
using System.Collections.Generic;

namespace Sych.ShareAssets.Runtime
{
    public interface IShareBridge
    {
        void ShareItems(List<string> items, Action<bool> completeCallback);
    }
}

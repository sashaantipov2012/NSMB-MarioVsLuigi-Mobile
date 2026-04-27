using System;

namespace NSMB.UI.Translation {
    public interface ITranslationSource : IComparable, IEquatable<ITranslationSource> {
        public int Priority { get; }
        public bool IsRTL { get; }
        public bool TryGetTranslation(string key, out string result);
        public void Reload();
    }
}
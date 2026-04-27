using System;
using System.Collections.Generic;

namespace NSMB.UI.Translation {
    public abstract class JsonTranslationSource : ITranslationSource, IComparable {

        //---Properties
        public int Priority { get; set; }
        public bool IsRTL => (loadedTranslations["rtl"] ?? "").Equals("true", StringComparison.InvariantCultureIgnoreCase);

        //---Protected Variables
        protected Dictionary<string, string> loadedTranslations;

        bool ITranslationSource.TryGetTranslation(string key, out string result) {
            if (loadedTranslations == null || key == null) {
                result = null;
                return false;
            }

            return loadedTranslations.TryGetValue(key, out result);
        }

        public abstract void Reload();

        public abstract bool Equals(ITranslationSource other);

        int IComparable.CompareTo(object other) {
            if (other is not ITranslationSource otherTranslationSource) {
                return 0;
            }
            return Priority.CompareTo(otherTranslationSource.Priority);
        }

    }
}
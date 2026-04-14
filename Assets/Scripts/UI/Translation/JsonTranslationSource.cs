using System.Collections.Generic;

namespace NSMB.UI.Translation {
    public abstract class JsonTranslationSource : ITranslationSource {

        //---Properties
        public int Priority { get; set; }

        //---Protected Variables
        protected Dictionary<string, string> loadedTranslations;

        public bool TryGetTranslation(string key, out string result) {
            if (loadedTranslations == null || key == null) {
                result = null;
                return false;
            }

            return loadedTranslations.TryGetValue(key, out result);
        }

        public abstract void Reload();

        public int CompareTo(object other) {
            if (other is not ITranslationSource otherTranslationSource) {
                return 0;
            }
            return Priority.CompareTo(otherTranslationSource.Priority);
        }

        public abstract bool Equals(ITranslationSource other);
    }
}
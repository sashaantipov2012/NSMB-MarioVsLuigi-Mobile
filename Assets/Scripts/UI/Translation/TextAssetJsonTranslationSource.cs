using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace NSMB.UI.Translation {
    public class TextAssetJsonTranslationSource : JsonTranslationSource {

        //---Private Variables
        private readonly TextAsset textAsset;

        public TextAssetJsonTranslationSource(TextAsset asset) {
            textAsset = asset;
            Reload();
        }

        public override void Reload() {
            loadedTranslations = JsonConvert.DeserializeObject<Dictionary<string, string>>(textAsset.text);
        }

        public override bool Equals(ITranslationSource other) {
            if (other is not TextAssetJsonTranslationSource otherTextAssetSource) {
                return false;
            }
            return textAsset == otherTextAssetSource.textAsset;
        }
    }
}
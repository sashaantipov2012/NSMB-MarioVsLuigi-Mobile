using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace NSMB.UI.Translation {
    public class FileJsonTranslationSource : JsonTranslationSource {

        //---Private Variables
        private readonly string filePath;

        public FileJsonTranslationSource(string file) {
            filePath = file;
            Reload();
        }

        public override void Reload() {
            loadedTranslations = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(filePath));
        }

        public override bool Equals(ITranslationSource other) {
            if (other is not FileJsonTranslationSource otherFileSource) {
                return false;
            }
            return filePath == otherFileSource.filePath;
        }
    }
}
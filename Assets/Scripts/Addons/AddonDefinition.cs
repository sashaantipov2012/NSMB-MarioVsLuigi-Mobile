using Newtonsoft.Json;
using System;
using UnityEngine;

namespace NSMB.Addons {
    [Serializable]
    public class AddonDefinition : IEquatable<AddonDefinition>, IDisposable {
        public Guid ReleaseGuid { get; set; }
        public string DisplayName { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
#if UNITY_EDITOR
        public string IconAssetPath { get; set; }
#endif
        public string[] SupportedPlatforms { get; set; }
        [JsonIgnore]
        public Texture2D IconTexture { get; set; }

        [JsonIgnore]
        public string FullName => $"{DisplayName} ({Version})";

        ~AddonDefinition() {
            if (IconTexture) {
                Debug.LogError($"Memory Leak! AddonDefinition ({DisplayName}) IconTexture was not disposed!");
                Dispose();
            }
        }

        public bool Equals(AddonDefinition other) {
            return ReleaseGuid == other.ReleaseGuid;
        }

        public void Dispose() {
            if (IconTexture) {
                UnityEngine.Object.Destroy(IconTexture);
            }
        }
    }
}
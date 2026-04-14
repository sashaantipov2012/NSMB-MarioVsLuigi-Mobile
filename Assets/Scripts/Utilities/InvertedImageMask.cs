using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace NSMB.Utilities {
    public class InvertedImageMask : Image {

        private Material newMaterial;

        public override Material materialForRendering {
            get {
                if (!newMaterial) {
                    newMaterial = new(base.materialForRendering);
                    newMaterial.SetInt("_StencilComp", (int) CompareFunction.NotEqual);
                }
                return newMaterial;
            }
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            if (newMaterial) {
                Destroy(newMaterial);
            }
        }

        ~InvertedImageMask() {
            try {
                if (newMaterial) {
                    Debug.LogError("InvertedImageMask destructor called without freeing newMaterial. This should never happen?");
                    Destroy(newMaterial);
                }
            } catch { /* Fixes exiting play mode in editor while debugging */ }
        }
    }
}
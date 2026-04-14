using NSMB.Sound;
using Quantum;
using System;

namespace NSMB.Quantum {
    public class StageContext : QuantumMonoBehaviour, IQuantumViewContext {

        public QuantumMapData MapData;
        [NonSerialized] public VersusStageData Stage;

        public void Awake() {
            Stage = (VersusStageData) QuantumUnityDB.GetGlobalAsset(MapData.GetAsset(false).UserAsset);
            SoundEffectResolver.Instance.GlobalProviders.Add(Stage);
        }

        public void OnDestroy() {
            if (SoundEffectResolver.Instance) {
                SoundEffectResolver.Instance.GlobalProviders.Remove(Stage);
            }
        }
    }
}

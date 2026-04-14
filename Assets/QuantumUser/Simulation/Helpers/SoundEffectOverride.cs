using System;
using UnityEngine;

[Serializable]
public class SoundEffectOverride {
    public SoundEffect SoundEffect;
    public OverrideMode Mode = OverrideMode.Single;
    public ushort Priority;
    public bool PlayGlobally = false;
#if QUANTUM_UNITY
    public AudioClip[] AudioClips;
#endif

    public enum OverrideMode {
        Single = 0,
        Random = 1,
        Layered = 2,
    }
}

public interface ISoundOverrideProvider : IAssetOverrideProvider<SoundEffect, SoundEffectOverride> {

}
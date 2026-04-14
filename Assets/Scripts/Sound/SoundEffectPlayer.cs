using NSMB.Utilities.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NSMB.Sound {
    public class SoundEffectPlayer : MonoBehaviour, ISoundOverrideProvider {

        //---Serialized Variables
        [SerializeField] private SoundEffectOverride[] sfxOverrides;
        [SerializeField] private AudioSource source;

        public void OnValidate() {
            this.SetIfNull(ref source);
        }

        public void Play(SoundEffect sfx, IList<ISoundOverrideProvider> extraProviders = null, int? variant = null) {
            List<ISoundOverrideProvider> providers = new() { this };
            if (extraProviders != null) {
                providers.AddRange(extraProviders);
            }
            var @override =  SoundEffectResolver.Instance.GetOneOverride(sfx, providers, variant);
            AudioClip clip = null;
            if (@override != null && @override.AudioClips != null && @override.AudioClips.Length > 0) {
                clip = @override.AudioClips[0];
            }
            source.clip = clip;
            source.Play();
        }

        public void Stop() {
            source.Stop();
        }

        public void PlayOneShot(SoundEffect sfx, IList<ISoundOverrideProvider> extraProviders = null, int? variant = null, float volume = 1) {
            List<ISoundOverrideProvider> providers = new() { this };
            if (extraProviders != null) {
                providers.AddRange(extraProviders);
            }
            source.PlayOneShot(sfx, providers, variant, volume);
        }

        [NonSerialized] private Dictionary<SoundEffect, SoundEffectOverride> overridesDict;
        public SoundEffectOverride GetOverride(SoundEffect sfx) {
            if (overridesDict == null) {
                overridesDict = new();
                foreach (var @override in sfxOverrides) {
                    overridesDict[@override.SoundEffect] = @override;
                }
            }
            overridesDict.TryGetValue(sfx, out var result);
            return result;
        }
    }
}
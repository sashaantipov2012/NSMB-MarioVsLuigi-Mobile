using JimmysUnityUtilities;
using NSMB.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NSMB.Sound {
    public class SoundEffectResolver : Singleton<SoundEffectResolver> {

        //---Properties
        public List<ISoundOverrideProvider> GlobalProviders { get; private set; } = new();

        //---Serialized Variables
        [SerializeField] private GlobalSoundEffectOverrides defaultProvider;
        [SerializeField] private AudioSource globalSfxSource;

        public void OnValidate() {
            this.SetIfNull(ref globalSfxSource);
        }

        public void Awake() {
            Set(this);
        }

        public SoundEffectOverride GetOneOverride(SoundEffect sfx, IList<ISoundOverrideProvider> extraProviders, int? variant = null) {
            List<ISoundOverrideProvider> allProviders = new();
            if (extraProviders != null) {
                allProviders.AddRange(extraProviders);
            }
            allProviders.AddRange(GlobalProviders);

            var sortedOverrides = allProviders
                .Select(op => op.GetOverride(sfx))
                .Where(o => o != null)
                .OrderByDescending(o => o.Priority)
                .ThenByDescending(o => o.Mode)
                .ToList();

            // Add default always LAST!
            var defaultOverride = defaultProvider.GetOverride(sfx);
            if (defaultOverride != null) {
                sortedOverrides.Add(defaultOverride);
            }

            if (sortedOverrides.Count == 0) {
                // Invalid sound effect, maybe?
                return null;
            }

            var mode = sortedOverrides[0].Mode;
            switch (mode) {
            case SoundEffectOverride.OverrideMode.Single:
            case SoundEffectOverride.OverrideMode.Layered:
                // Pick first only
                return sortedOverrides[0];
            case SoundEffectOverride.OverrideMode.Random:
                // Pick one random from all potential sounds.
                return sortedOverrides
                    .Where(o => o.Mode == mode)
                    .GetRandomElement();
            }

            return null;
        }

        public IList<AudioClip> PlayOneShot(AudioSource localSfxSource, SoundEffect sfx, IList<ISoundOverrideProvider> extraProviders, int? variant = null, float volume = 1) {
            List<ISoundOverrideProvider> allProviders = new();
            if (extraProviders != null) {
                allProviders.AddRange(extraProviders);
            }
            allProviders.AddRange(GlobalProviders);

            var sortedOverrides = allProviders
                .Select(op => op.GetOverride(sfx))
                .Where(o => o != null)
                .OrderByDescending(o => o.Priority)
                .ThenByDescending(o => o.Mode)
                .ToList();

            // Add default always LAST!
            var defaultOverride = defaultProvider.GetOverride(sfx);
            if (defaultOverride != null) {
                sortedOverrides.Add(defaultOverride);
            }

            if (sortedOverrides.Count == 0) {
                // Invalid sound effect, maybe?
                return Array.Empty<AudioClip>();
            }

            List<AudioClip> clips = new();
            AudioClip clip;
            var mode = sortedOverrides[0].Mode;
            switch (mode) {
            case SoundEffectOverride.OverrideMode.Single:
                // Pick first only
                clip = PlayOverride(localSfxSource, sortedOverrides[0], variant, volume);
                if (clip) {
                    clips.Add(clip);
                }
                break;
            case SoundEffectOverride.OverrideMode.Random:
                // Pick one random from all potential sounds.
                var randomOverride = sortedOverrides
                    .Where(o => o.Mode == mode)
                    .GetRandomElement();

                clip = PlayOverride(localSfxSource, randomOverride, variant, volume);
                if (clip) {
                    clips.Add(clip);
                }
                break;
            case SoundEffectOverride.OverrideMode.Layered:
                // Play all with "layered" at once.
                foreach (var sfxOverride in sortedOverrides.Where(o => o.Mode == mode)) {
                    clip = PlayOverride(localSfxSource, sfxOverride, variant, volume);
                    if (clip) {
                        clips.Add(clip);
                    }
                }
                return clips;
            }
            return clips;
        }

        private AudioClip PlayOverride(AudioSource localSfxSource, SoundEffectOverride sfxOverride, int? variant, float volume) {
            var clips = sfxOverride.AudioClips;
            if (clips == null || clips.Length == 0) {
                return null;
            }

            var source = sfxOverride.PlayGlobally ? globalSfxSource : localSfxSource;
            if (!source || !source.isActiveAndEnabled) {
                return null;
            }

            variant ??= UnityEngine.Random.Range(0, clips.Length);
            var randomClip = clips[QuantumUtils.Modulo(variant.Value, clips.Length)];
            if (randomClip) {
                source.PlayOneShot(randomClip, volume);
                return randomClip;
            } else {
                return null;
            }
        }
    }
}
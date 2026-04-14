using NSMB.Utilities.Extensions;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace NSMB.Sound {
    public class AudioMixerManager : Singleton<AudioMixerManager> {

        //---Static
        public static readonly string KeyMaster = "MasterVolume", KeyMusic = "MusicVolume", KeySfx = "SoundVolume", KeyOverride = "OverrideVolume";
        public static event Action<string, float> OnAudioMixerValueChanged;

        //---Serialized Variables
        [SerializeField] private AudioMixer mainMixer;

        //---Private variables
        private Coroutine fadeMusicRoutine, fadeSfxRoutine;

        public void Awake() {
            Set(this);
        }

        public void Start() {
            if (!Application.isFocused) {
                if (Settings.Instance.audioMuteMusicOnUnfocus) {
                    SetFloat("MusicVolume", -80f);
                }

                if (Settings.Instance.audioMuteSFXOnUnfocus) {
                    SetFloat("SoundVolume", -80f);
                }
            }
        }

        public void OnApplicationFocus(bool focus) {
            if (focus) {
                // Restore
                this.StopCoroutineNullable(ref fadeMusicRoutine);
                this.StopCoroutineNullable(ref fadeSfxRoutine);

                Settings.Instance.ApplyVolumeSettings();
            } else {
                // Fade out
                if (Settings.Instance.audioMuteMusicOnUnfocus) {
                    fadeMusicRoutine ??= StartCoroutine(FadeOut("MusicVolume"));
                }
                if (Settings.Instance.audioMuteSFXOnUnfocus) {
                    fadeSfxRoutine ??= StartCoroutine(FadeOut("SoundVolume"));
                }
            }
        }

        public void SetFloat(string key, float value) {
            mainMixer.SetFloat(key, value);
            OnAudioMixerValueChanged?.Invoke(key, value);
        }

        public bool GetFloat(string key, out float value) {
            return mainMixer.GetFloat(key, out value);
        }

        public IEnumerator FadeOut(string key, float fadeTime = 0.5f) {
            GetFloat(key, out float currentVolume);
            currentVolume = ToLinearScale(currentVolume);
            float fadeRate = currentVolume / fadeTime;

            while (currentVolume > 0f) {
                currentVolume -= fadeRate * Time.fixedDeltaTime;
                SetFloat(key, ToLogScale(currentVolume));
                yield return null;
            }
            SetFloat(key, -80f);
        }

        public static float ToLinearScale(float x) {
            return Mathf.Pow(10, x / 20);
        }

        public static float ToLogScale(float x) {
            return 20 * Mathf.Log10(x);
        }
    }
}

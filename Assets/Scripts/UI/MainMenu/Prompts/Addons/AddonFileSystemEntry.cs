using NSMB.Addons;
using NSMB.Utilities;
using NSMB.Utilities.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NSMB.UI.MainMenu.Submenus.Prompts.Addons {
    public class AddonFileSystemEntry : MonoBehaviour, ISelectHandler {

        //---Static Variables
        private static readonly Dictionary<AddonsSubmenu.ScannedPath.AddonType, string> SpriteNames = new() {
            { AddonsSubmenu.ScannedPath.AddonType.Folder,       "<sprite name=folder>" },
            { AddonsSubmenu.ScannedPath.AddonType.AddonFile,    "<sprite name=addon>" },
            { AddonsSubmenu.ScannedPath.AddonType.NonAddonFile, "<sprite name=unknown_file>" },
        };
        private static readonly Dictionary<AddonLoadResultEnum, string> LoadResultTranslationKeys = new() {
            { AddonLoadResultEnum.Success, "ui.addons.manage.status.enabled" },
            { AddonLoadResultEnum.AlreadyLoaded, "ui.addons.manage.status.enabled" },
            { AddonLoadResultEnum.ReadFailure, "ui.addons.manage.status.readerror" },
            { AddonLoadResultEnum.IncompatibleGameVersion, "ui.addons.manage.status.incompatible.game" },
            { AddonLoadResultEnum.IncompatibleWithOtherAddon, "ui.addons.manage.status.incompatible.otheraddon" },
            { AddonLoadResultEnum.IncompatbilePlatform, "ui.addons.manage.status.incompatible.platform" },
        };

        //---Properties
        public AddonsSubmenu.ScannedPath Path => scannedPath;

        //---Public Variables
        public Button button;

        //---Serialized Variables
        [SerializeField] private TMP_Text text, stateText;
        //[SerializeField] private GameObject enabledText;
        [SerializeField] private SelectablePromptLabel promptLabel;

        //---Private Variables
        private AddonsSubmenu parent;
        private AddonsSubmenu.ScannedPath scannedPath;
        private Coroutine blankStateTextCoroutine;

        public void Initialize(AddonsSubmenu parent, AddonsSubmenu.ScannedPath scannedPath) {
            this.parent = parent;
            this.scannedPath = scannedPath;

            text.text = scannedPath.Name;
            if (SpriteNames.TryGetValue(scannedPath.Type, out string sprite)) {
                if (sprite == SpriteNames[AddonsSubmenu.ScannedPath.AddonType.Folder] && scannedPath.Name == "..") {
                    // Manual override for "up"
                    sprite = "<sprite name=up>";
                }
                text.text = sprite + text.text;
            }

            promptLabel.translationKey = text.text;
            UpdateEnabledState();
            gameObject.SetActive(true);
        }

        public void OnDisable() {
            UpdateEnabledState();
        }

        public void OnDestroy() {
            scannedPath.Addon?.Dispose();
        }

        public async void OnClicked() {
            if (parent.loadingGraphic.activeSelf) {
                return;
            }
            switch (scannedPath.Type) {
            case AddonsSubmenu.ScannedPath.AddonType.Folder:
                if (scannedPath.Name == "..") {
                    GlobalController.Instance.PlaySound(SoundEffect.UI_Back);
                } else {
                    GlobalController.Instance.PlaySound(SoundEffect.UI_Decide);
                }
                _ = parent.OpenFolder(scannedPath.Name);
                break;
            case AddonsSubmenu.ScannedPath.AddonType.AddonFile:
                parent.loadingGraphic.SetActive(true);
                var addonManager = GlobalController.Instance.addonManager;
                if (addonManager.IsAddonLoaded(scannedPath.Addon.ReleaseGuid)) {
                    await addonManager.UnloadAddon(scannedPath.Addon.ReleaseGuid);
                    await Awaitable.MainThreadAsync();
                    parent.loadingGraphic.SetActive(false);
                    GlobalController.Instance.PlaySound(SoundEffect.Player_Sound_Powerdown);
                    UpdateEnabledState();
                } else {
                    using FileStream fs = new(scannedPath.FullPath, FileMode.Open);
                    var loadAddonResult = await addonManager.LoadAddonStream(fs);
                    await Awaitable.MainThreadAsync();
                    if (loadAddonResult.Success) {
                        GlobalController.Instance.PlaySound(SoundEffect.Player_Sound_PowerupCollect);
                        UpdateEnabledState();
                    } else {
                        // Show error message
                        GlobalController.Instance.PlaySound(SoundEffect.UI_Error);
                        if (LoadResultTranslationKeys.TryGetValue(loadAddonResult.Result, out var translationKey)) {
                            var text = GlobalController.Instance.translationManager.GetTranslationWithReplacements(translationKey,
                                "version", GameVersion.Current.ToStringIgnoreHotfix() + ".X",
                                "addon", loadAddonResult.IncompatibleWith?.Definition?.FullName);
                            stateText.text = text;
                            if (blankStateTextCoroutine != null) {
                                StopCoroutine(blankStateTextCoroutine);
                            }
                            blankStateTextCoroutine = StartCoroutine(BlankStateText());
                        } else {
                            UpdateEnabledState();
                        }
                    }
                    parent.loadingGraphic.SetActive(false);
                }
                break;
            case AddonsSubmenu.ScannedPath.AddonType.NonAddonFile:
                GlobalController.Instance.PlaySound(SoundEffect.UI_Error);
                break;
            }
        }

        private IEnumerator BlankStateText() {
            yield return new WaitForSecondsRealtime(3f);
            stateText.text = "";
            blankStateTextCoroutine = null;
        }

        public void UpdateEnabledState() {
            if (!this) {
                return;
            }
            if (scannedPath.Addon != null && GlobalController.Instance.addonManager.IsAddonLoaded(scannedPath.Addon.ReleaseGuid)) {
                stateText.text = GlobalController.Instance.translationManager.GetTranslation("ui.addons.manage.status.enabled");
            } else {
                stateText.text = "";
            }
            if (blankStateTextCoroutine != null) {
                StopCoroutine(blankStateTextCoroutine);
            }
        }

        public void OnSelect(BaseEventData eventData) {
            parent.scrollRect.verticalNormalizedPosition = parent.scrollRect.ScrollToCenter((RectTransform) transform, false);
            parent.UpdateSelectedAddonText(this);
        }
    }
}
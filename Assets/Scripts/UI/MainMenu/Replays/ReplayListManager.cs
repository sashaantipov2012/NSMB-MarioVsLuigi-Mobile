using NSMB.Replay;
using NSMB.UI.Elements;
using NSMB.UI.MainMenu.Submenus.Prompts;
using NSMB.UI.Translation;
using NSMB.Utilities;
using NSMB.Utilities.Extensions;
using Quantum;
using SFB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NSMB.UI.MainMenu.Submenus.Replays {
    public class ReplayListManager : Selectable {

#if UNITY_WEBGL && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        public static extern void UploadFile(string gameObjectName, string methodName, string filter, bool multiple);
#endif

        //---Static Variables
        public static ReplayListManager Instance { get; private set; }
        public static string ReplayDirectory { get; private set; } 
        public static string TempDirectory { get; private set; }
        public static string SavedDirectory { get; private set; }
        public static string FavoriteDirectory { get; private set; }

        //---Properties
        public ReplayListEntry Selected { get; set; }
        public List<ReplayListEntry> Replays => replays;

        //---Serialized Variables
        [SerializeField] private MainMenuCanvas canvas;
        [SerializeField] private ReplayDeletePromptSubmenu deletePrompt;
        [SerializeField] private ReplayRenamePromptSubmenu renamePrompt;
        [SerializeField] private ReplayListEntry replayTemplate;
        [SerializeField] private TMP_Text noReplaysText, hiddenReplaysText, headerTemplate;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] internal VerticalLayoutGroup layout;
        [SerializeField] private TMP_Dropdown sortDropdown;
        [SerializeField] private SpriteChangingToggle ascendingToggle;
        [SerializeField] private TMP_InputField searchField;
        [SerializeField] private TMP_Text replayInformation;
        [SerializeField] private GameObject importButton, loadingIcon;

        //---Private Variables
        private readonly List<TMP_Text> headers = new();
        private readonly List<ReplayListEntry> replays = new();
        private readonly SortedSet<ReplayListEntry> temporaryReplays = new(new ReplayDateComparer());
        private bool sortAscending;
        private int sortIndex;
        private bool languageChangedSinceLastOpen;


        [RuntimeInitializeOnLoadMethod]
        public static void CreateDirectories() {
            ReplayDirectory = Path.Combine(Application.persistentDataPath, "replays");
            TempDirectory = Path.Combine(ReplayDirectory, "temp");
            SavedDirectory = Path.Combine(ReplayDirectory, "saved");
            FavoriteDirectory = Path.Combine(ReplayDirectory, "favorite");

            Directory.CreateDirectory(TempDirectory);
            Directory.CreateDirectory(FavoriteDirectory);
            Directory.CreateDirectory(SavedDirectory);
        }

#if UNITY_EDITOR
        protected override void OnValidate() {
            base.OnValidate();
            this.SetIfNull(ref canvas, UnityExtensions.GetComponentType.Parent);
        }
#endif

        protected override void OnEnable() {
            base.OnEnable();
#if UNITY_EDITOR
            // #if fixes an error in the editor.
            if (GlobalController.Instance && GlobalController.Instance.translationManager) {
                OnLanguageChanged(GlobalController.Instance.translationManager);
            }
#else
            OnLanguageChanged(GlobalController.Instance.translationManager);
#endif
        }

        public void Initialize() {
            Instance = this;
            TranslationManager.OnLanguageChanged += OnLanguageChanged;

            // _ = FindReplays();
        }

        public void OnDestroyCustom() {
            TranslationManager.OnLanguageChanged -= OnLanguageChanged;
        }

        public void Show() {
            sortDropdown.value = 0;
            ascendingToggle.isOn = false;
            searchField.SetTextWithoutNotify("");
            replayTemplate.gameObject.SetActive(false);
            scrollRect.verticalNormalizedPosition = 1;
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform) layout.transform);
            Canvas.ForceUpdateCanvases();

            _ = FindReplays();

            //SortReplays();
            OnScrollRectScrolled(default);
            OnLanguageChanged(GlobalController.Instance.translationManager);
        }

        private IEnumerator SelectAtEndOfFrame() {
            yield return new WaitForEndOfFrame();
            if (!canvas.EventSystem.alreadySelecting) {
                if (Selected) {
                    canvas.EventSystem.SetSelectedGameObject(Selected.IsOpen ? Selected.defaultSelection : Selected.button.gameObject);
                } else if (GetFirstReplayEntry() != null) {
                    canvas.EventSystem.SetSelectedGameObject(GetFirstReplayEntry().button.gameObject);
                } else {
                    canvas.EventSystem.SetSelectedGameObject(importButton);
                }
            }
        }

        public override void OnSelect(BaseEventData eventData) {
            StartCoroutine(SelectAtEndOfFrame());
        }

        public override void OnPointerDown(PointerEventData eventData) {
            // Do nothing.
        }

        public void StartRename(ReplayListEntry replay) {
            renamePrompt.Open(replay);
        }

        public void StartDeletion(ReplayListEntry replay) {
            deletePrompt.Open(replay);
        }

        public void Select(ReplayListEntry replay, bool open) {
            foreach (var otherReplay in replays) {
                if (otherReplay != replay) {
                    otherReplay.HideButtons();
                }
            }
            Selected = replay;
            if (Selected) {
                Selected.OnSelect(open);
            }
            LayoutRebuilder.MarkLayoutForRebuild((RectTransform) headerTemplate.transform.parent);
            UpdateInformation(replay);
        }

        public void UpdateInformation(ReplayListEntry replay) {
            TranslationManager tm = GlobalController.Instance.translationManager;
            if (replay == null) {
                replayInformation.text = tm.GetTranslation("ui.extras.replays.information.none");
                replayInformation.horizontalAlignment = HorizontalAlignmentOptions.Center;
                return;
            }
            if (!replay.ReplayFile.Header.IsCompatible) {
                replayInformation.text = tm.GetTranslationWithReplacements("ui.extras.replays.incompatible", "version", replay.ReplayFile.Header.Version.ToStringIgnoreHotfix() + ".X");
                replayInformation.horizontalAlignment = HorizontalAlignmentOptions.Center;
                return;
            }

            BinaryReplayHeader header = replay.ReplayFile.Header;
            ref var rules = ref header.Rules;
            string gamemodeName;
            if (QuantumUnityDB.TryGetGlobalAsset(rules.Gamemode, out var gamemode)) {
                gamemodeName = gamemode.NamePrefix + tm.GetTranslation(gamemode.TranslationKey);
            } else {
                gamemodeName = "<sprite name=room_customlevel> ???";
            }

            // Playerlist
            StringBuilder builder = new();
            foreach (int i in Enumerable.Range(0, header.PlayerInformation.Length).OrderByDescending(idx => header.PlayerInformation[idx].FinalObjectiveCount)) {
                ref ReplayPlayerInformation info = ref header.PlayerInformation[i];

                // Color and width
                builder.Append("<width=85%>");
                if (header.Rules.TeamsEnabled) {
                    var allTeams = AssetRepository<TeamAsset>.AllAssets;
                    TeamAsset team = allTeams[info.Team % allTeams.Count];
                    builder.Append("<nobr>");
                    builder.Append("<color=#").Append(Utils.ColorToHex(team.color, false)).Append(">").Append(Settings.Instance.GraphicsColorblind ? team.textSpriteColorblind : team.textSpriteNormal);
                } else {
                    builder.Append("<color=white>");
                    builder.Append("<nobr>- ");
                }

                // Username
                builder.Append(string.IsNullOrWhiteSpace(info.Nickname) ? "noname" : info.Nickname);
                builder.Append("</nobr>");

                // Stars
                builder.Append("<width=100%><line-height=0><align=right><br>");
                builder.Append(gamemode ? Utils.GetSymbolString(gamemode.ObjectiveSymbolPrefix) : "");
                builder.Append(info.Team == header.WinningTeam ? "<color=yellow>" : "<color=white>");
                builder.Append(Mathf.Max(0, info.FinalObjectiveCount));

                // Fix formatting
                builder.AppendLine("<align=left><line-height=100%>");
            }
            builder.AppendLine();

            // Add rules
            string off = tm.GetTranslation("ui.generic.off");
            string on = tm.GetTranslation("ui.generic.on");

            builder.Append("<align=center><color=white>");
            builder.AppendLine(gamemodeName);

            if (gamemode is CoinRunnersGamemode) {
                builder.Append("<sprite name=room_timer> ").Append(Utils.SecondsToMinuteSeconds(rules.TimerMinutes * 60)).Append("    ");
                builder.Append("<sprite name=room_coins> ").Append(rules.CoinsForPowerup).Append("    ");
                builder.Append("<sprite name=room_lives> ").Append(rules.Lives > 0 ? rules.Lives : off).Append("    ");
                builder.Append("<sprite name=room_powerups>").Append(rules.CustomPowerupsEnabled ? on : off).Append("    ");
                builder.Append("<sprite name=room_teams>").AppendLine(rules.TeamsEnabled ? on : off);
            } else {
                // Default to star chasers
                builder.Append("<sprite name=room_stars> ").Append(rules.StarsToWin).Append("    ");
                builder.Append("<sprite name=room_coins> ").Append(rules.CoinsForPowerup).Append("    ");
                builder.Append("<sprite name=room_lives> ").Append(rules.Lives > 0 ? rules.Lives : off).Append("    ");
                builder.Append("<sprite name=room_timer> ").Append(rules.TimerMinutes > 0 ? Utils.SecondsToMinuteSeconds(rules.TimerMinutes * 60) : off).Append("    ");
                builder.Append("<sprite name=room_powerups>").Append(rules.CustomPowerupsEnabled ? on : off).Append("    ");
                builder.Append("<sprite name=room_teams>").AppendLine(rules.TeamsEnabled ? on : off);
            } 
            builder.Append("<color=#aaa>").Append(DateTimeToLocalizedString(DateTime.UnixEpoch.AddSeconds(header.UnixTimestamp), false, false)).Append(" - ");
            builder.Append(Utils.SecondsToMinuteSeconds(header.ReplayLengthInFrames / 60)).Append(" - ").Append(Utils.BytesToString(replay.ReplayFile.FileSize));

            replayInformation.text = builder.ToString();
            replayInformation.horizontalAlignment = HorizontalAlignmentOptions.Left;
        }

        public static string DateTimeToLocalizedString(DateTime dt, bool shortDisplay, bool dateOnly) {
            TranslationManager tm = GlobalController.Instance.translationManager;
            dt = dt.ToLocalTime();
            try {
                CultureInfo culture = new(tm.CurrentLocale);
                if (dateOnly) {
                    if (shortDisplay) {
                        return dt.ToString(culture.DateTimeFormat.ShortDatePattern);
                    } else {
                        return dt.ToString(culture.DateTimeFormat.LongDatePattern);
                    }
                } else {
                    return dt.ToString(culture.DateTimeFormat);
                }
            } catch {
                if (dateOnly) {
                    if (shortDisplay) {
                        return dt.ToLocalTime().ToShortDateString();
                    } else {
                        return dt.ToLocalTime().ToLongDateString();
                    }
                } else {
                    return dt.ToLocalTime().ToString();
                }
            }
        }

        public void RemoveReplay(ReplayListEntry replay) {
            if (!replay) {
                return;
            }
            
            replays.Remove(replay);
            bool wasTemporary = temporaryReplays.Remove(replay);
            
            Destroy(replay.gameObject);
            if (replays.Count == 0) {
                noReplaysText.text = GlobalController.Instance.translationManager.GetTranslation(Settings.Instance.GeneralReplaysEnabled ? "ui.extras.replays.none" : "ui.extras.replays.disabled");
            }
            int? index = isActiveAndEnabled ? replays.IndexOf(replay) : null;
            SortReplays(index);
            
            if (wasTemporary && Settings.Instance.generalMaxTempReplays != 0) {
                foreach (var tempReplay in temporaryReplays.Skip(Settings.Instance.generalMaxTempReplays - 5).Take(5)) {
                    tempReplay.UpdateText();
                }
            }
        }

        public void RemoveReplayByPath(string path) {
            RemoveReplay(replays.FirstOrDefault(rle => rle.ReplayFile.FilePath == path));
        }

        private async Awaitable FindReplays() {
            try {
                await Awaitable.MainThreadAsync();
                noReplaysText.text = "";
                loadingIcon.SetActive(true);
                hiddenReplaysText.gameObject.SetActive(false);

                if (languageChangedSinceLastOpen) {
                    // Update to fix a bug where the names are invalid after a language switch.
                    foreach (var createdReplays in replays) {
                        createdReplays.UpdateText();
                    }
                    languageChangedSinceLastOpen = false;
                }

                //await Awaitable.BackgroundThreadAsync();

                List<BinaryReplayFile> newReplays = new();
                foreach (var filepath in Directory.EnumerateFiles(ReplayDirectory, "*.mvlreplay", SearchOption.AllDirectories)) {
                    if (newReplays.Any(brf => brf.FilePath == filepath)
                        || replays.Any(brf => brf.ReplayFile.FilePath == filepath)) {
                        continue;
                    }

                    var parseResult = BinaryReplayFile.TryLoadNewFromFile(filepath, false, out BinaryReplayFile replay);
                    if (parseResult == ReplayParseResult.Success) {
                        newReplays.Add(replay);
                    } else {
                        Debug.Log($"[Replay] Parsing '{filepath}' failed with reason '{parseResult}'");
                    }
                }

                await Awaitable.MainThreadAsync();
                foreach (var replay in newReplays) {
                    ReplayListEntry newReplayEntry = Instantiate(replayTemplate, replayTemplate.transform.parent);
                    newReplayEntry.Initialize(this, replay);
                    newReplayEntry.UpdateText();
                    newReplayEntry.name = newReplayEntry.ReplayFile.Header.GetDisplayName();
                    newReplayEntry.gameObject.SetActive(true);
                    replays.Add(newReplayEntry);

                    if (newReplayEntry.IsTemporary) {
                        temporaryReplays.Add(newReplayEntry);
                    }

                    if (isActiveAndEnabled) {
                        newReplayEntry.gameObject.SetActive(true);
                        canvas.EventSystem.SetSelectedGameObject(newReplayEntry.gameObject);
                        scrollRect.verticalNormalizedPosition = 1;
                    }
                }

                // Update a second time to fix the "Temporary" label.
                foreach (var createdReplays in replays) {
                    createdReplays.UpdateText();
                }

                await FilterReplays();
                SortReplays();
                if (replays.Count == 0) {
                    noReplaysText.text = GlobalController.Instance.translationManager.GetTranslation(Settings.Instance.GeneralReplaysEnabled ? "ui.extras.replays.none" : "ui.extras.replays.disabled");
                }
                loadingIcon.SetActive(false);
            } catch (Exception e) {
                Debug.Log(e);
                throw;
            }
        }

        public void OnSortDropdownChanged() {
            sortIndex = sortDropdown.value;
            SortReplays();
        }

        public void OnAscendingSortToggleChanged() {
            sortAscending = ascendingToggle.isOn;
            SortReplays();
        }

        public void OnSearchChanged() {
            _ = FilterReplays();
        }

        public void OnImportClicked() {
            TranslationManager tm = GlobalController.Instance.translationManager;

#if UNITY_WEBGL && !UNITY_EDITOR
            UploadFile(name, nameof(ImportFile), ".mvlreplay", false);
#else
            string[] selected = StandaloneFileBrowser.OpenFilePanel(tm.GetTranslation("ui.extras.replays.actions.import"), "", "mvlreplay", false);
            if (selected != null && selected.Length > 0) {
                StartCoroutine(ImportFile(selected[0], true));
            }
#endif
        }

        private IEnumerator ImportFile(string filepath, bool makeCopy) {
#if UNITY_WEBGL
            using UnityEngine.Networking.UnityWebRequest downloadRequest = new(filepath, "GET");
            downloadRequest.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
            yield return downloadRequest.SendWebRequest();
            while (!downloadRequest.downloadHandler.isDone) {
                yield return null;
            }
            byte[] replay = ((UnityEngine.Networking.DownloadHandlerBuffer) downloadRequest.downloadHandler).data;
            using MemoryStream memStream = new MemoryStream(replay);

            ReplayParseResult parseResult = BinaryReplayFile.TryLoadNewFromStream(memStream, true, out BinaryReplayFile parsedReplay);
#else
            ReplayParseResult parseResult = BinaryReplayFile.TryLoadNewFromFile(filepath, true, out BinaryReplayFile parsedReplay);
#endif

            if (parseResult == ReplayParseResult.Success) {
                // Change to today
                parsedReplay.Header.UnixTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();

                if (makeCopy) {
                    // Write into the replays folder (not copy, since we changed the timestamp in the header...)
                    string newPath = Path.Combine(ReplayDirectory, "saved", parsedReplay.Header.UnixTimestamp + ".mvlreplay");
                    using (FileStream fs = new FileStream(newPath, FileMode.Create)) {
                        parsedReplay.WriteToStream(fs);
                    }
                    parsedReplay.FilePath = newPath;
                }

                ReplayListEntry newReplayEntry = Instantiate(replayTemplate, replayTemplate.transform.parent);
                newReplayEntry.Initialize(this, parsedReplay);
                newReplayEntry.name = newReplayEntry.ReplayFile.Header.GetDisplayName();

                replays.Add(newReplayEntry);
                newReplayEntry.UpdateText();
                newReplayEntry.gameObject.SetActive(true);
                SortReplays();

                canvas.EventSystem.SetSelectedGameObject(newReplayEntry.gameObject);

                noReplaysText.text = "";
            } else {
                GlobalController.Instance.sfx.PlayOneShot(SoundEffect.UI_Error);
                Debug.LogWarning($"[Replay] Failed to parse {filepath} as a replay: {parseResult}");
            }
            yield return null;
        }

        public void UpdateReplayNavigation(int? selectIndex = null) {
            ReplayListEntry previous = null;
            foreach (var replay in replays) {
                if (!replay.gameObject.activeInHierarchy) {
                    continue;
                }

                replay.UpdateNavigation(previous);
                previous = replay;
            }
            if (selectIndex.HasValue && replays.Count > 0) {
                Select(replays[Mathf.Clamp(selectIndex.Value, 0, replays.Count - 1)], true);
            } else {
                Select(null, false);
                scrollRect.verticalNormalizedPosition = 1;
            }
        }

        public void SortReplays(int? selectedIndex = null) {
            replays.Sort(sortIndex switch {
                1 => SortByName,
                2 => SortByStage,
                _ => SortByDate,
            });
            foreach (var header in headers) {
                Destroy(header.gameObject);
            }
            headers.Clear();
            string previousHeader = null;
            foreach (var replay in replays) {
                string currentHeader = GetHeader(replay);
                if (previousHeader != currentHeader && currentHeader != null) {
                    TMP_Text newHeader = Instantiate(headerTemplate, headerTemplate.transform.parent);
                    newHeader.gameObject.SetActive(true);
                    newHeader.text = $"- {currentHeader} -";

                    headers.Add(newHeader);
                }
                previousHeader = currentHeader;
                replay.transform.SetAsLastSibling();
            }
            hiddenReplaysText.transform.SetAsLastSibling();

            UpdateReplayNavigation(selectedIndex);
        }

        public async Awaitable FilterReplays() {
            // Reset state
            await Awaitable.MainThreadAsync();
            foreach (var replay in replays) {
                replay.gameObject.SetActive(true);
            }
            foreach (var header in headers) {
                header.gameObject.SetActive(true);
            }

            await Awaitable.BackgroundThreadAsync();
            int hidden = 0;
            TranslationManager tm = GlobalController.Instance.translationManager;
            if (!string.IsNullOrWhiteSpace(searchField.text)) {
                List<GameObject> toDisableGameObjects = new();
                List<string> enabledHeaders = new();
                foreach (var replay in replays) {
                    // Check display name
                    if (replay.ReplayFile.Header.GetDisplayName().Contains(searchField.text, StringComparison.InvariantCultureIgnoreCase)) {
                        enabledHeaders.Add(GetHeader(replay));
                        continue;
                    }

                    // Check date
                    if (DateTimeToLocalizedString(DateTime.UnixEpoch.AddSeconds(replay.ReplayFile.Header.UnixTimestamp), false, false).Contains(searchField.text, StringComparison.InvariantCultureIgnoreCase)) {
                        enabledHeaders.Add(GetHeader(replay));
                        continue;
                    }

                    // Check stage name
                    if (QuantumUnityDB.TryGetGlobalAsset(replay.ReplayFile.Header.Rules.Stage, out Map map)
                        && QuantumUnityDB.TryGetGlobalAsset(map.UserAsset, out VersusStageData stage)) {

                        if (tm.GetTranslation(stage.TranslationKey).Contains(searchField.text, StringComparison.InvariantCultureIgnoreCase)) {
                            enabledHeaders.Add(GetHeader(replay));
                            continue;
                        }
                    }

                    // Check player usernames
                    bool found = false;
                    foreach (var playerInfo in replay.ReplayFile.Header.PlayerInformation) {
                        if (playerInfo.Nickname.Contains(searchField.text, StringComparison.InvariantCultureIgnoreCase)) {
                            found = true;
                            break;
                        }
                    }
                    if (found) {
                        enabledHeaders.Add(GetHeader(replay));
                        continue;
                    }

                    // Check status
                    if (replay.warningText.text.Contains(searchField.text, StringComparison.InvariantCultureIgnoreCase)) {
                        enabledHeaders.Add(GetHeader(replay));
                        continue;
                    }

                    // Did not match
                    toDisableGameObjects.Add(replay.gameObject);
                    hidden++;
                }

                await Awaitable.MainThreadAsync();

                foreach (var go in toDisableGameObjects) {
                    go.SetActive(false);
                }

                // Activate headers w/ replays only
                foreach (var header in headers) {
                    header.gameObject.SetActive(enabledHeaders.Contains(header.text[2..^2]));
                }
            }

            await Awaitable.MainThreadAsync();

            hiddenReplaysText.gameObject.SetActive(hidden > 0);
            hiddenReplaysText.text = tm.GetTranslationWithReplacements("ui.extras.replays.search.hidden", "replays", hidden.ToString());
            UpdateReplayNavigation();
        }

        public ReplayListEntry GetFirstReplayEntry() {
            if (replays.Count <= 0) {
                return null;
            }

            return replays[0];
        }

        private string GetHeader(ReplayListEntry rle) {
            if (sortIndex == 1) {
                // Name
                return null;
            } else if (sortIndex == 2) {
                // Stage
                if (QuantumUnityDB.TryGetGlobalAsset(rle.ReplayFile.Header.Rules.Stage, out Map map)) {
                    if (QuantumUnityDB.TryGetGlobalAsset(map.UserAsset, out VersusStageData stage)) {
                        return GlobalController.Instance.translationManager.GetTranslation(stage.TranslationKey);
                    }
                }
                return "???";
            } else {
                // Date
                return DateTimeToLocalizedString(DateTime.UnixEpoch.AddSeconds(rle.ReplayFile.Header.UnixTimestamp), true, true);
            }
        }

        public int SortByDate(ReplayListEntry a, ReplayListEntry b) {
            if (sortAscending) {
                return a.ReplayFile.Header.UnixTimestamp.CompareTo(b.ReplayFile.Header.UnixTimestamp);
            } else {
                return b.ReplayFile.Header.UnixTimestamp.CompareTo(a.ReplayFile.Header.UnixTimestamp);
            }
        }

        public int SortByName(ReplayListEntry a, ReplayListEntry b) {
            if (sortAscending) {
                return a.ReplayFile.Header.GetDisplayName().CompareTo(b.ReplayFile.Header.GetDisplayName());
            } else {
                return b.ReplayFile.Header.GetDisplayName().CompareTo(a.ReplayFile.Header.GetDisplayName());
            }
        }

        public int SortByStage(ReplayListEntry a, ReplayListEntry b) {
            var allMaps = AssetRepository<Map>.AllAssetRefs;

            if (sortAscending) {
                return
                    allMaps.IndexOf(map => map == a.ReplayFile.Header.Rules.Stage)
                    - allMaps.IndexOf(map => map == b.ReplayFile.Header.Rules.Stage);
            } else {
                return
                    allMaps.IndexOf(map => map == b.ReplayFile.Header.Rules.Stage)
                    - allMaps.IndexOf(map => map == a.ReplayFile.Header.Rules.Stage);
            }
        }

        public void OnScrollRectScrolled(Vector2 pos) {
            /*
            RectTransform parent = (RectTransform) canvas.transform;
            parent.GetWorldCorners(corners);
            Rect parentRect = new((Vector2) corners[0], parent.rect.size);
            foreach (var replay in replays) {
                replay.UpdateVisibility(parentRect);
            }
            */
        }

        private void OnLanguageChanged(TranslationManager tm) {
            if (!gameObject.activeInHierarchy) {
                languageChangedSinceLastOpen = true;
                return;
            }

            int index = sortDropdown.value;

            sortDropdown.ClearOptions();
            string prefix = tm.RightToLeft ? "<align=right>" : "";
            sortDropdown.options.Add(new TMP_Dropdown.OptionData { text = prefix + tm.GetTranslation("ui.extras.replays.sort.date") });
            sortDropdown.options.Add(new TMP_Dropdown.OptionData { text = prefix + tm.GetTranslation("ui.extras.replays.sort.alphabetical") });
            sortDropdown.options.Add(new TMP_Dropdown.OptionData { text = prefix + tm.GetTranslation("ui.extras.replays.sort.stage") });
            sortDropdown.SetValueWithoutNotify(index);
            sortDropdown.RefreshShownValue();

            UpdateInformation(Selected);
        }

        /*
        public int? GetReplaysUntilDeletion(ReplayListEntry replay) {
            int max = Settings.Instance.generalMaxTempReplays;
            int index = temporaryReplays.IndexOf(r => r == replay);
            if (max <= 0 || !replay.IsTemporary || index == -1) {
                return null;
            }

            return Mathf.Max(1, max - index);
        }
        */

        public IList<string> GetTemporaryReplaysToDelete() {
            if (Settings.Instance.generalMaxTempReplays <= 0) {
                return Array.Empty<string>();
            }

            var x = Directory.EnumerateFiles(TempDirectory, "*.mvlreplay")
                .OrderByDescending(path => {
                    try {
                        return File.GetLastWriteTime(path);
                    } catch {
                        return default;
                    }
                })
                .Skip(Settings.Instance.generalMaxTempReplays)
                .ToList();

            return x;
        }

        public class ReplayDateComparer : IComparer<ReplayListEntry> {
            public int Compare(ReplayListEntry x, ReplayListEntry y) {
                try {
                    return File.GetLastWriteTime(x.ReplayFile.FilePath).CompareTo(y.ReplayFile.FilePath);
                } catch {
                    return 0;
                }
            }
        }
    }
}

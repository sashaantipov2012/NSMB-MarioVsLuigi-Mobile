using NSMB.UI.MainMenu.Submenus.InRoom;
using NSMB.Utilities;
using Quantum;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Scripting;
using Button = UnityEngine.UI.Button;

namespace NSMB.UI.Elements {
    public class ContextMenuTeamSelector : Button, IScrollHandler {

        //---Serialized Variables
        [SerializeField] private TMP_Text label;
        [SerializeField] private string teamTranslationKey, clearTranslationKey;
        [SerializeField] private GameObject[] showOnSelect;
        [SerializeField] private PlayerListEntry parent;

        //---Properties
        private int _teamIndex;
        public int TeamIndex {
            get => _teamIndex;
            set {
                int entries = AssetRepository<TeamAsset>.AllAssetRefs.Count;
                if (isTeamLocked) {
                    entries++;
                }

                _teamIndex = Mathf.Clamp(value, 0, entries - 1);
                UpdateLabel();
            }
        }

        //---Private Variables
        private bool isTeamLocked;

        protected override unsafe void OnEnable() {
            base.OnEnable();

            if (parent.player.IsValid) {
                var game = QuantumRunner.DefaultGame;
                Frame f = game.Frames.Predicted;
                var playerData = QuantumUtils.GetPlayerData(f, parent.player);
                TeamIndex = playerData->RequestedTeam;
                isTeamLocked = playerData->IsTeamLocked;
            } else {
                TeamIndex = 0;
            }
        }

        protected override void OnDisable() {
            base.OnDisable();
            foreach (var go in showOnSelect) {
                go.SetActive(false);
            }
        }

        public override void OnMove(AxisEventData eventData) {
            if (eventData.moveDir == MoveDirection.Left) {
                TeamIndex--;
                eventData.Use();
            } else if (eventData.moveDir == MoveDirection.Right) {
                TeamIndex++;
                eventData.Use();
            } else {
                base.OnMove(eventData);
            }
        }

        public override void OnSelect(BaseEventData eventData) {
            base.OnSelect(eventData);
            foreach (var go in showOnSelect) {
                go.SetActive(true);
            }
        }

        public override void OnDeselect(BaseEventData eventData) {
            base.OnDeselect(eventData);
            foreach (var go in showOnSelect) {
                go.SetActive(false);
            }
        }

        public void OnScroll(PointerEventData eventData) {
            if (eventData.scrollDelta.y > 0) {
                TeamIndex--;
                eventData.Use();
            } else if (eventData.scrollDelta.y < 0) {
                TeamIndex++;
                eventData.Use();
            }
        }

        [Preserve]
        public void AddIndex(int value) {
            TeamIndex += value;
        }

        public void UpdateLabel() {
#if UNITY_EDITOR
            if (!this || !Application.IsPlaying(this)) {
                return;
            }
#endif
            var tm = GlobalController.Instance.translationManager;
            string text;

            var teams = AssetRepository<TeamAsset>.AllAssets;
            if (TeamIndex < teams.Count) {
                var team = teams[TeamIndex];
                string teamName = tm.GetTranslation(team.nameTranslationKey);
                text = tm.GetTranslationWithReplacements("ui.inroom.player.changeteam",
                    "team", (Settings.Instance.GraphicsColorblind ? team.textSpriteColorblind : team.textSpriteNormal) + teamName);
            } else {
                text = tm.GetTranslation("ui.inroom.player.changeteam.unlock");
            }
            
            label.text = text;
        }
    }
}

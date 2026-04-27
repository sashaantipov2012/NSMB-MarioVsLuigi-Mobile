using NSMB.Networking;
using NSMB.UI.Translation;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NSMB.UI.MainMenu.Submenus.Prompts {
    public class JoinRoomPromptSubmenu : PromptSubmenu {

        //---Serialized Variables
        [SerializeField] private TMP_InputField idInputField;
        [SerializeField] private TMP_Text invalidText;
        [SerializeField] private Color validIdColor, invalidIdColor;

        //---Private Variables
        private bool success;

        public override void Show(bool first) {
            base.Show(first);

            if (first) {
                // Default values
                idInputField.text = "";
                idInputField.characterLimit = NetworkHandler.RoomIdLength;
                ((TMP_Text) idInputField.placeholder).text = new string('#', NetworkHandler.RoomIdLength);
                IDTextChanged();
            }
            success = false;

            Settings.Controls.UI.Submit.performed += OnSubmit;
        }

        public override void Hide(SubmenuHideReason hideReason) {
            base.Hide(hideReason);
            Settings.Controls.UI.Submit.performed -= OnSubmit;
        }

        public override bool TryGoBack(out bool playSound) {
            if (success) {
                playSound = false;
                return true;
            }

            return base.TryGoBack(out playSound);
        }

        public void ConfirmClicked() {
            if (!NetworkHandler.IsValidRoomId(idInputField.text, out _)) {
                Canvas.PlaySound(SoundEffect.UI_Error);
                return;
            }

            success = true;
            Canvas.PlayConfirmSound();
            _ = NetworkHandler.JoinRoom(new Photon.Realtime.EnterRoomArgs {
                RoomName = idInputField.text
            });
            Canvas.GoBack();
        }

        public void IDTextChanged() {
            bool valid = NetworkHandler.IsValidRoomId(idInputField.text, out int regionIndex);
            TranslationManager tm = GlobalController.Instance.translationManager;
            if (valid) {
                invalidText.text = tm.GetTranslationWithReplacements("ui.rooms.joinprivate.valid", "region", $"region.{NetworkHandler.Regions.ElementAt(regionIndex).Code}");
                invalidText.color = validIdColor;
            } else {
                invalidText.text = tm.GetTranslation("ui.rooms.joinprivate.invalid");
                invalidText.color = invalidIdColor;
            }
        }

        private void OnSubmit(InputAction.CallbackContext context) {
            if (Canvas.EventSystem.currentSelectedGameObject == idInputField.gameObject
                && context.control.device.name == "Keyboard"
                && context.control.name.Length != 1) {
                // Allow submit to confirm, but only when not using gamepad
                ConfirmClicked();
                idInputField.ActivateInputField();
            }
        }
    }
}
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace NSMB.UI.MainMenu {
    public class TMP_InputFieldFixer : MonoBehaviour {

        //---Private Variables
        private int heldDirection;

        public void OnEnable() {
            Settings.Controls.UI.Navigate.performed += OnNavigate;
            Settings.Controls.UI.Navigate.canceled += OnNavigate;
        }

        public void OnDisable() {
            Settings.Controls.UI.Navigate.performed -= OnNavigate;
            Settings.Controls.UI.Navigate.canceled -= OnNavigate;
        }

        public void OnNavigate(InputAction.CallbackContext context) {
            float y = context.ReadValue<Vector2>().y;

            // No need for a deadzone because the input action processor handles it
            int currentDirection;
            if (y > 0) {
                currentDirection = 1;
            } else if (y < 0) {
                currentDirection = -1;
            } else {
                currentDirection = 0;
            }

            if (heldDirection == currentDirection) {
                return;
            }

            var osk = OnScreenKeyboard.Instance;
            if (osk && osk.IsOpen) {
                heldDirection = currentDirection;
                return;
            }

            EventSystem eventSystem = EventSystem.current;
            GameObject selectedObject = eventSystem.currentSelectedGameObject;
            if (!selectedObject) {
                heldDirection = currentDirection;
                eventSystem.sendNavigationEvents = true;
                return;
            }

            if (!selectedObject.TryGetComponent(out TMP_InputField selectedText)) {
                heldDirection = currentDirection;
                eventSystem.sendNavigationEvents = true;
                return;
            }

            // We are selecting a text object.

            // "context.control.name.Length != 1" is bullshit... i don't trust this.
            // (for context (heh), 1-length names are to make movement directions on keyboard
            // for typing characters (like W/S) not navigate while typing)
            if (currentDirection != 0 && context.control.name.Length != 1) {
                Selectable next = currentDirection == 1 ? selectedText.FindSelectableOnUp() : selectedText.FindSelectableOnDown();
                if (next) {
                    eventSystem.SetSelectedGameObject(next.gameObject);
                    if (next.TryGetComponent(out TMP_InputField nextInputField)) {
                        nextInputField.OnPointerClick(new PointerEventData(eventSystem));
                    }
                }
                eventSystem.sendNavigationEvents = false;
            }

            heldDirection = currentDirection;
        }
    }
}

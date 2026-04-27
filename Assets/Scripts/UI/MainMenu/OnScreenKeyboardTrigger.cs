using NSMB.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace NSMB.UI.MainMenu {
    [RequireComponent(typeof(TMP_InputField))]
    public class OnScreenKeyboardTrigger : MonoBehaviour, ISubmitHandler {

        //---Serialized Variables
        [SerializeField, FormerlySerializedAs("InputField")] private TMP_InputField inputField;
        [SerializeField, FormerlySerializedAs("DisabledCharacters")] private string disabledCharacters;

        public void OnValidate() {
            this.SetIfNull(ref inputField);
        }

        public void Start() {
            inputField.shouldActivateOnSelect = false;
        }

        public void OnSubmit(BaseEventData eventData) {
            OnScreenKeyboard kb = FindFirstObjectByType<OnScreenKeyboard>();
            kb.OpenIfNeeded(inputField, new string[] { "QWERTYUIOP\b", "ASDFGHJKL", "ZXCVBNM" }, disabledCharacters);
        }
    }
}
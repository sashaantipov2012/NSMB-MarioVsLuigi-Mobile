using UnityEngine;

namespace NSMB.UI.MainMenu.Submenus.Prompts {
    public class PromptSubmenu : MainMenuSubmenu {

        //---Properites
        public virtual GameObject BackButton => backButton;

        //---Serialized Variables
        [SerializeField] protected GameObject backButton;

        public override bool TryGoBack(out bool playSound) {
            if (BackButton && Canvas.EventSystem.currentSelectedGameObject != BackButton) {
                Canvas.EventSystem.SetSelectedGameObject(BackButton);
                playSound = false;
                return false;
            }

            return base.TryGoBack(out playSound);
        }
    }
}
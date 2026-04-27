using NSMB.Networking;
using UnityEngine;

namespace NSMB.UI.Options {
    public class RefreshNicknameColorButton : MonoBehaviour {
        public void Click() {
            if (AuthenticationHandler.TryUpdateNicknameColor()) {
                GlobalController.Instance.PlaySound(SoundEffect.UI_Decide);
            } else {
                GlobalController.Instance.PlaySound(SoundEffect.UI_Error);
            }
        }
    }
}
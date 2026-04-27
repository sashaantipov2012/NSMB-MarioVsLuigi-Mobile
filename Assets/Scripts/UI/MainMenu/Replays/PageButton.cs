using TMPro;
using UnityEngine;
using UnityEngine.Scripting;

namespace NSMB.UI.MainMenu.Submenus.Replays {
    public class PageButton : MonoBehaviour {

        //---Serialized Variables
        [SerializeField] private ReplayListManager replayList;
        [SerializeField] private TMP_Text text;

        [Preserve]
        public void OnClick() {
            if (!int.TryParse(text.text, out int page)
                || replayList.CurrentPage == page - 1) {
                return;
            }

            // page is 0-indexed, so -1
            replayList.canvas.PlayCursorSound();
            _ = replayList.CreateReplayListEntries(default, page - 1);
        }

        [Preserve]
        public void NextPage() {
            if (replayList.CurrentPage + 1 == replayList.PageCount) {
                return;
            }
            replayList.canvas.PlayCursorSound();
            _ = replayList.CreateReplayListEntries(default, replayList.CurrentPage + 1);
        }

        [Preserve]
        public void PreviousPage() {
            if (replayList.CurrentPage - 1 == 0) {
                return;
            }
            replayList.canvas.PlayCursorSound();
            _ = replayList.CreateReplayListEntries(default, replayList.CurrentPage - 1);
        }
    }
}
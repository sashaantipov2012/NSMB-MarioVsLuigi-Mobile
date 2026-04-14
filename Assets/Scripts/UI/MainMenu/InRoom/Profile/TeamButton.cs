using Quantum;
using UnityEngine;
using UnityEngine.UI;

namespace NSMB.UI.MainMenu.Submenus.InRoom {
    public class TeamButton : MonoBehaviour {

        //---Serialized Variables
        [SerializeField] private Sprite overlayUnpressed, overlayPressed;
        [SerializeField] private Image flag;
        [SerializeField] public int index;

        public unsafe void OnEnable() {
            Settings.OnColorblindModeChanged += OnColorblindModeChanged;

            var game = QuantumRunner.DefaultGame;
            Frame f = game.Frames.Predicted;
            var playerData = QuantumUtils.GetPlayerData(f, game.GetLocalPlayers()[0]);

            var teams = f.Context.GetAllAssets<TeamAsset>();
            TeamAsset team = teams[index % teams.Count];
            flag.sprite = Settings.Instance.GraphicsColorblind ? team.spriteColorblind : team.spriteNormal;
        }

        public void OnDisable() {
            Settings.OnColorblindModeChanged -= OnColorblindModeChanged;
        }

        private unsafe void OnColorblindModeChanged() {
            Frame f = QuantumRunner.DefaultGame.Frames.Predicted;
            var teams = f.Context.GetAllAssets<TeamAsset>();
            TeamAsset team = teams[index % teams.Count];
            flag.sprite = Settings.Instance.GraphicsColorblind ? team.spriteColorblind : team.spriteNormal;
        }
    }
}

using UnityEngine;

namespace NSMB.UI.Game {
    [CreateAssetMenu(menuName = "ScriptableObjects/PlayerSlotInfo")]
    public class PlayerSlotInfo : ScriptableObject {
        public Color Color;
        public Sprite Sprite;
        public string Icon;

        public Color GetModifiedColor(float s, float v) {
            Color.RGBToHSV(Color, out float hue, out float saturation, out float value);
            return Color.HSVToRGB(hue, saturation * s, value * v);
        }
    }
}

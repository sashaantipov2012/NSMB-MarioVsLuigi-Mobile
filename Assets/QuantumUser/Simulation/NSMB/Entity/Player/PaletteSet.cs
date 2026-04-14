using Quantum;
using System;
using UnityEngine.Serialization;

public class PaletteSet : AssetObject, IOrderedAsset {

    int IOrderedAsset.Order => Order;

    [FormerlySerializedAs("colors")] public CharacterSpecificPalette[] Colors = { new() };
    [FormerlySerializedAs("translationKey")] public string TranslationKey;
    [FormerlySerializedAs("order")] public int Order;
    public bool IsLegacy;

    public CharacterSpecificPalette GetPaletteForCharacter(AssetRef<CharacterAsset> player) {
        CharacterSpecificPalette nullPlayer = null;
        foreach (CharacterSpecificPalette color in Colors) {
            if (player.Equals(color.Character)) {
                return color;
            }

            if (color.Character == null) {
                nullPlayer = color;
            }
        }
        return nullPlayer ?? Colors[0];
    }
}

[Serializable]
public class CharacterSpecificPalette {

    [FormerlySerializedAs("character")] public AssetRef<CharacterAsset> Character;
    public ColorRGBA ShirtColor;
    public ColorRGBA OverallsColor;
    [FormerlySerializedAs("hatUsesOverallsColor")] public bool HatUsesOverallsColor;

}
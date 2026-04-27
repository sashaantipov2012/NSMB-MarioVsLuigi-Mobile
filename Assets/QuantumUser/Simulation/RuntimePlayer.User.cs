namespace Quantum {
    public partial class RuntimePlayer {

        public string UserId;
        public string IpAddressHash;

        public bool UseColoredNickname;
        public string NicknameColor;

        public AssetRef<CharacterAsset> Character;
        public AssetRef<PaletteSet> Palette;

        public bool IsGloballyMuted;

    }
}
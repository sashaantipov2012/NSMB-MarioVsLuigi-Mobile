namespace Quantum {
    public partial struct BannedPlayerInfo {
        public BannedPlayerInfo(RuntimePlayer player) {
            Nickname = player.PlayerNickname;
            UserId = player.UserId;
            IpAddressHash = player.IpAddressHash;
        }

        public readonly bool MatchesPlayer(RuntimePlayer player) {
            return player.UserId == UserId || player.IpAddressHash == IpAddressHash;
        }
    }
}
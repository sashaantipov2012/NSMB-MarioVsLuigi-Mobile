namespace Quantum {
    public static class KillReasonExtensions {
    
        public static bool ShouldSpawnCoin(this EnemyKillReason reason) {
            return reason == EnemyKillReason.Special || reason == EnemyKillReason.Groundpounded;
        }
    }
}
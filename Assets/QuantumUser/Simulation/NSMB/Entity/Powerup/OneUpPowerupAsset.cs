using Quantum;

public unsafe class OneUpPowerupAsset : PowerupAsset {

    public override int CountPlayersWithReserve(Frame f) {
        return 0;
    }

    public override int CountPlayersWithState(Frame f) {
        return 0;
    }

    public override PowerupReserveResult Collect(Frame f, EntityRef entity) {
        if (f.Global->Rules.IsLivesEnabled && f.Unsafe.TryGetPointer(entity, out MarioPlayer* mario)) {
            if (mario->Lives < byte.MaxValue) {
                mario->Lives++;
            }
        }

        return PowerupReserveResult.CollectNewIgnoreOld;
    }
}

using Photon.Deterministic;
using Quantum;

public unsafe class StarmanPowerupAsset : PowerupAsset {

    public FP StarmanDuration = 10;

    public override int CountPlayersWithState(Frame f) {
        int count = 0;
        foreach ((_, var marioPlayer) in f.Unsafe.GetComponentBlockIterator<MarioPlayer>()) {
            if (marioPlayer->IsStarmanInvincible) {
                count++;
            }
        }
        return count;
    }

    public override unsafe PowerupReserveResult Collect(Frame f, EntityRef marioEntity) {
        var mario = f.Unsafe.GetPointer<MarioPlayer>(marioEntity);
        mario->InvincibilityFrames = (ushort) (StarmanDuration * f.UpdateRate);

        f.Signals.OnMarioPlayerBecameInvincible(marioEntity);
        return PowerupReserveResult.CollectNewIgnoreOld;
    }
}
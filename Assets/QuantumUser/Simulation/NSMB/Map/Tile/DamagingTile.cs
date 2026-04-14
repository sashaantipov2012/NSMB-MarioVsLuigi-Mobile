using Photon.Deterministic;
using Quantum;

public unsafe class DamagingTile : StageTile, IInteractableTile {

    public InteractionDirection DamageDirections = InteractionDirection.Up | InteractionDirection.Down | InteractionDirection.Left | InteractionDirection.Right;
    public bool InstantKill = false;

    public bool Interact(Frame f, EntityRef entity, InteractionDirection direction, IntVector2 tilePosition, StageTileInstance tileInstance, out bool playBumpSound) {
        playBumpSound = false;

        if (!DamageDirections.HasFlag(direction)) {
            return false;
        }

        if (!f.Unsafe.TryGetPointer(entity, out MarioPlayer* mario)) {
            return false;
        }

        if (InstantKill) {
            mario->Death(f, entity, false, true, EntityRef.None);
        } else {
            mario->Powerdown(f, entity, false, EntityRef.None);
        }
        return true;
    }
}
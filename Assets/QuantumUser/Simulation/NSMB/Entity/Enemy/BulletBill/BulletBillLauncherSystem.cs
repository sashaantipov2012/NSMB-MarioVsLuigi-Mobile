using Photon.Deterministic;

namespace Quantum {
    public unsafe class BulletBillLauncherSystem : SystemMainThreadEntityFilter<BulletBillLauncher, BulletBillLauncherSystem.Filter>, ISignalOnComponentRemoved<BulletBill> {
        public struct Filter {
            public EntityRef Entity;
            public BulletBillLauncher* Launcher;
            public BreakableObject* Breakable;
            public PhysicsCollider2D* Collider;
            public Transform2D* Transform;
        }

        private static readonly FPVector2 SpawnOffset = new FPVector2(0, FP.FromString("-0.45"));

        public override void Update(Frame f, ref Filter filter, VersusStageData stage) {
            if (filter.Breakable->IsBroken) {
                return;
            }
            var launcher = filter.Launcher;
            if (launcher->BulletBillCount >= 3) {
                return;
            }

            var transform = filter.Transform;
            var collider = filter.Collider;
            FPVector2 spawnpoint = transform->Position + FPVector2.Up * (collider->Shape.Box.Extents.Y * 2) + SpawnOffset;

            var allPlayers = f.Filter<MarioPlayer, Transform2D>();
            FP smallestDistance = FP.UseableMax;
            bool tooClose = false;
            while (allPlayers.NextUnsafe(out _, out _, out Transform2D* marioTransform)) {
                QuantumUtils.WrappedDistance(stage, spawnpoint, marioTransform->Position, out FP distance);
                FP abs = FPMath.Abs(distance);

                // Player is too close
                if (abs < launcher->MinimumShootRadius) {
                    smallestDistance = FP.UseableMax;
                    tooClose = true;
                    break;
                }

                if (abs < FPMath.Abs(smallestDistance)) {
                    smallestDistance = distance;
                }
            }

            if (FPMath.Abs(smallestDistance) > launcher->MaximumShootRadius) {
                if (!tooClose) {
                    launcher->TimeToShootFrames = launcher->TimeToShoot;
                }
                return;
            }

            if (QuantumUtils.Decrement(ref launcher->TimeToShootFrames)) {
                // Attempt a shot
                var entity = filter.Entity;
                bool right = smallestDistance < 0;

                EntityRef newBillEntity = f.Create(launcher->BulletBillPrototype);
                var newBill = f.Unsafe.GetPointer<BulletBill>(newBillEntity);
                var newBillTransform = f.Unsafe.GetPointer<Transform2D>(newBillEntity);
                newBill->Initialize(f, newBillEntity, entity, right);
                newBillTransform->Position = spawnpoint;

                launcher->BulletBillCount++;
                launcher->TimeToShootFrames = launcher->TimeToShoot;

                f.Events.BulletBillLauncherShoot(entity, newBillEntity, right);
            }
        }

        #region Signals
        public void OnRemoved(Frame f, EntityRef entity, BulletBill* component) {
            if (f.Unsafe.TryGetPointer(component->Owner, out BulletBillLauncher* launcher)) {
                launcher->BulletBillCount--;
            }
        }

        #endregion
    }
}

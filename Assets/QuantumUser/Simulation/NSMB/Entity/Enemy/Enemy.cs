using Photon.Deterministic;

namespace Quantum {
    public unsafe partial struct Enemy {
        public readonly bool IsAlive => !IsDead && IsActive;

        public readonly EntityRef FindClosestPlayerToSpawnpoint(Frame f, EntityRef entity, VersusStageData stage = null) {
            var allPlayers = f.Filter<MarioPlayer, Transform2D>();
            allPlayers.UseCulling = false;

            if (stage == null) {
                stage = f.FindAsset<VersusStageData>(f.Map.UserAsset);
            }
            FP closestDistance = FP.MaxValue;
            EntityRef closestPlayer = EntityRef.None;
            while (allPlayers.NextUnsafe(out EntityRef marioEntity, out MarioPlayer* mario, out Transform2D* marioTransform)) {
                if (mario->IsDead) {
                    continue;
                }

                FP newDistance = QuantumUtils.WrappedDistance(stage, Spawnpoint, marioTransform->Position);

                if (newDistance <= closestDistance) {
                    closestPlayer = marioEntity;
                    closestDistance = newDistance;
                }
            }
            return closestPlayer;
        }

        /**
         * <summary>
         * Sets the respawn data for the enemy
         * </summary>
         * <param name="waitTime">How long to wait until the enemy respawns in frames.</param>
         * <param name="sparklesTime">When the sparkles will spawn (based off time remaining) also in frames.</param>
         */
        public void SetDelayedRespawn(int waitTime = 420, int sparklesTime = 80) {
            RespawnTimer = waitTime;
            RespawnSparklesTimer = sparklesTime;
        }

        public void Respawn(Frame f, EntityRef entity) {
            var transform = f.Unsafe.GetPointer<Transform2D>(entity);

            IsActive = true;
            IsDead = false;
            IgnoreOffscreen = false;
            SetDelayedRespawn(0, 0);
            transform->Teleport(f, Spawnpoint);

            if (f.Unsafe.TryGetPointer(entity, out PhysicsObject* physicsObject)) {
                physicsObject->IsFrozen = false;
                physicsObject->Velocity = FPVector2.Zero;
                physicsObject->DisableCollision = false;
            }

            // face left by default
            var shouldFaceRight = false;
            var closestMario = FindClosestPlayerToSpawnpoint(f, entity);

            // use closest player and face them
            if (f.Unsafe.TryGetPointer(closestMario, out Transform2D* marioTransform)) {
                QuantumUtils.WrappedDistance(f, Spawnpoint, marioTransform->Position, out FP xDiff);
                shouldFaceRight = xDiff < 0;
            }

            FacingRight = shouldFaceRight;
        }

        public void ChangeFacingRight(Frame f, EntityRef entity, bool newFacingRight) {
            if (FacingRight != newFacingRight) {
                FacingRight = newFacingRight;
                f.Signals.OnEnemyTurnaround(entity);
            }
        }
    }
}
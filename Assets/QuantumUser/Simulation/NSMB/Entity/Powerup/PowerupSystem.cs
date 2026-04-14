using Photon.Deterministic;

namespace Quantum {
    public unsafe class PowerupSystem : SystemMainThreadEntityFilter<Powerup, PowerupSystem.Filter>, ISignalOnEntityBumped, ISignalOnEntityCrushed {

        public struct Filter {
            public EntityRef Entity;
            public Transform2D* Transform;
            public Powerup* Powerup;
            public CoinItem* CoinItem;
            public PhysicsObject* PhysicsObject;
            public PhysicsCollider2D* Collider;
            public Interactable* Interactable;
        }

        public override void OnInit(Frame f) {
            f.Context.Interactions.Register<Powerup, MarioPlayer>(f, OnPowerupMarioInteraction);
        }

        public override void Update(Frame f, ref Filter filter, VersusStageData stage) {
            var coinItem = filter.CoinItem;
            if (f.FindAsset(coinItem->Scriptable) is not PowerupAsset asset) {
                Log.Warn($"Powerup contains non-PowerupAsset scriptable! ({coinItem->Scriptable})");
                return;
            }

            var powerup = filter.Powerup;
            var physicsObject = filter.PhysicsObject;
            var transform = filter.Transform;

            if (asset.AvoidPlayers && physicsObject->IsTouchingGround) {
                FPVector2? closestMarioPosition = null;
                FP? closestDistance = null;
                var allPlayers = f.Filter<MarioPlayer, Transform2D>();
                while (allPlayers.NextUnsafe(out _, out _, out Transform2D* marioTransform)) {
                    FP distance = QuantumUtils.WrappedDistance(stage, marioTransform->Position, transform->Position);
                    if (closestDistance == null || distance < closestDistance) {
                        closestMarioPosition = marioTransform->Position;
                        closestDistance = distance;
                    }
                }

                if (closestMarioPosition.HasValue) {
                    powerup->FacingRight = QuantumUtils.WrappedDirectionSign(stage, closestMarioPosition.Value, transform->Position) == -1;
                }
            }

            HandleCollision(f, ref filter, asset);

            if (powerup->AnimationCurveTimer > 0) {
                transform->Position = powerup->AnimationCurveOrigin + new FPVector2(
                    asset.AnimationCurveX.Evaluate(FPMath.Clamp(powerup->AnimationCurveTimer, 0, asset.AnimationCurveX.EndTime - FP._0_10)),
                    asset.AnimationCurveY.Evaluate(FPMath.Clamp(powerup->AnimationCurveTimer, 0, asset.AnimationCurveY.EndTime - FP._0_10))
                );
                powerup->AnimationCurveTimer += f.DeltaTime;
            }
        }

        public void HandleCollision(Frame f, ref Filter filter, PowerupAsset asset) {
            var powerup = filter.Powerup;
            var physicsObject = filter.PhysicsObject;

            if (powerup->AnimationCurveTimer > 0) {
                return;
            }

            if (physicsObject->IsTouchingLeftWall || physicsObject->IsTouchingRightWall) {
                powerup->FacingRight = physicsObject->IsTouchingLeftWall;
                physicsObject->Velocity.X = asset.Speed * (powerup->FacingRight ? 1 : -1);
            }

            if (physicsObject->IsTouchingGround) {
                if (asset.FollowAnimationCurve) {
                    physicsObject->IsFrozen = true;
                    powerup->AnimationCurveOrigin = filter.Transform->Position;
                    powerup->AnimationCurveTimer += FP._0_01;
                } else {
                    physicsObject->Velocity.X = asset.Speed * (powerup->FacingRight ? 1 : -1);
                    if (asset.BounceStrength > 0) {
                        physicsObject->Velocity.Y = FPMath.Max(physicsObject->Velocity.Y, asset.BounceStrength);
                        physicsObject->IsTouchingGround = false;
                    }
                }

                /*
                if (data.HitRoof || (data.HitLeft && data.HitRight)) {
                    DespawnEntity();
                    return;
                }
                */
            }
        }

        public void OnPowerupMarioInteraction(Frame f, EntityRef powerupEntity, EntityRef marioEntity) {
            if (!f.Exists(powerupEntity) || f.DestroyPending(powerupEntity)) {
                // Already collected
                return;
            }

            var coinItem = f.Unsafe.GetPointer<CoinItem>(powerupEntity);

            // Don't collect if we're ignoring players (usually, after blue shell spawns from a blue koopa,
            // so we dont collect it instantly)
            if (coinItem->IgnorePlayerFrames > 0) {
                return;
            }

            if (f.FindAsset(coinItem->Scriptable) is PowerupAsset asset) {
                // Change the player's powerup state
                PowerupReserveResult result = asset.Collect(f, marioEntity);

                f.Signals.OnMarioPlayerCollectedPowerup(marioEntity, powerupEntity);
                f.Events.MarioPlayerCollectedPowerup(marioEntity, result, asset);
            }

            f.Events.CollectableDespawned(powerupEntity, f.Unsafe.GetPointer<Transform2D>(powerupEntity)->Position, true);
            f.Destroy(powerupEntity);
        }

        public void OnEntityBumped(Frame f, EntityRef entity, FPVector2 position, EntityRef bumpOwner, QBoolean fromBelow) {
            if (!f.Unsafe.TryGetPointer(entity, out Transform2D* transform)
                || !f.Unsafe.TryGetPointer(entity, out Powerup* powerup)
                || !f.Unsafe.TryGetPointer(entity, out CoinItem* coinItem)
                || !f.Unsafe.TryGetPointer(entity, out PhysicsObject* physicsObject)
                || coinItem->SpawnAnimationFrames > 0
                || f.FindAsset(coinItem->Scriptable) is not PowerupAsset asset) {

                return;
            }

            QuantumUtils.UnwrapWorldLocations(f, transform->Position, position, out FPVector2 ourPos, out FPVector2 theirPos);
            physicsObject->Velocity = new FPVector2(
                asset.Speed * (ourPos.X > theirPos.X ? 1 : -1),
                asset.BumpedFromBelowVelocity
            );
            physicsObject->IsTouchingGround = false;
            powerup->FacingRight = ourPos.X > theirPos.X;
        }

        public void OnEntityCrushed(Frame f, EntityRef entity) {
            if (f.Has<Powerup>(entity)
                && f.Unsafe.TryGetPointer(entity, out CoinItem* coinItem)
                && coinItem->SpawnAnimationFrames <= 0) {

                f.Events.CollectableDespawned(entity, f.Unsafe.GetPointer<Transform2D>(entity)->Position, false);
                f.Destroy(entity);
            }
        }
    }
}
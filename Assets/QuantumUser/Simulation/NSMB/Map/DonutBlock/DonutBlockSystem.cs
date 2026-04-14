using Photon.Deterministic;

namespace Quantum {
    public unsafe class DonutBlockSystem : SystemMainThreadEntityFilter<DonutBlock, DonutBlockSystem.Filter> {

        public struct Filter {
            public EntityRef EntityRef;
            public DonutBlock* DonutBlock;
            public Transform2D* Transform;
        }

        public override void OnInit(Frame f) {
            f.Context.Interactions.Register<MarioPlayer, DonutBlock>(f, OnMarioPlayerDonutBlockInteraction);
        }

        public override void Update(Frame f, ref Filter filter, VersusStageData stage) {
            var donutBlock = filter.DonutBlock;

            if (donutBlock->IsFalling) {
                // Falling logic
                var transform = filter.Transform;
                if (QuantumUtils.Decrement(ref donutBlock->Timer)) {
                    // Respawn back at origin.
                    transform->Teleport(f, donutBlock->Origin);
                    donutBlock->IsFalling = false;
                    donutBlock->Timer = donutBlock->FramesUntilFall;
                } else {
                    transform->Position += FPVector2.Down * (donutBlock->FallSpeed * f.DeltaTime);
                }
            } else {
                if (donutBlock->HasMario) {
                    if (QuantumUtils.Decrement(ref donutBlock->Timer)) {
                        donutBlock->IsFalling = true;
                        donutBlock->Timer = donutBlock->FramesUntilRespawn;
                    }
                } else {
                    donutBlock->Timer = donutBlock->FramesUntilFall;
                }
            }

            donutBlock->HasMario = false;
        }

        public bool OnMarioPlayerDonutBlockInteraction(Frame f, EntityRef marioEntity, EntityRef donutBlockEntity, PhysicsContact contact) {
            var mario = f.Unsafe.GetPointer<MarioPlayer>(marioEntity);
            if (mario->CurrentPowerupState == PowerupState.MiniMushroom) {
                // Not affected by Mini Mario.
                return true;
            }

            var donutBlock = f.Unsafe.GetPointer<DonutBlock>(donutBlockEntity);
            donutBlock->HasMario = true;
            return true;
        }
    }
}
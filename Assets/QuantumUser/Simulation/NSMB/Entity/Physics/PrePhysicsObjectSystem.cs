//#define MULTITHREADED

namespace Quantum {
#if MULTITHREADED
    public unsafe class PrePhysicsObjectSystem : SystemArrayComponent<PhysicsObject> {
        public override unsafe void Update(FrameThreadSafe f, EntityRef entity, PhysicsObject* component) {
            component->WasBeingCrushed = component->IsBeingCrushed;
            component->IsBeingCrushed = false;
        }
    }
#else
    public unsafe class PrePhysicsObjectSystem : SystemMainThreadEntity<PhysicsObject> {
        public override void Update(Frame f) {
            foreach ((_, var component) in f.Unsafe.GetComponentBlockIterator<PhysicsObject>()) {
                component->WasBeingCrushed = component->IsBeingCrushed;
                component->IsBeingCrushed = false;
            }
        }
    }
#endif
}
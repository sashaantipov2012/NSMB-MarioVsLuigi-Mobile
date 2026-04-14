using Unity.Collections.LowLevel.Unsafe;

namespace Quantum {
    public unsafe partial class Frame {

        public StageTileInstance* StageTiles;
        public int StageTilesLength;

        partial void FreeUser() {
            if (StageTiles != null) {
#if QUANTUM_3_1
                QuantumUnsafe.Free(StageTiles);
#else
                UnsafeUtility.Free(StageTiles, Unity.Collections.Allocator.Persistent);
#endif
                StageTiles = null;
            }
        }

        partial void SerializeUser(FrameSerializer serializer) {
            var stream = serializer.Stream;

            // Tilemap
            if (stream.Writing) {
                stream.WriteInt(StageTilesLength);
            } else {
                int newLength = stream.ReadInt();
                ReallocStageTiles(newLength);
            }
            for (int i = 0; i < StageTilesLength; i++) {
                StageTileInstance.Serialize(StageTiles + i, serializer);
            }
        }

        partial void CopyFromUser(Frame frame) {
            ReallocStageTiles(frame.StageTilesLength);
#if QUANTUM_3_1
            QuantumUnsafe.Copy(StageTiles, frame.StageTiles, StageTileInstance.SIZE * frame.StageTilesLength);
#else
            UnsafeUtility.MemCpy(StageTiles, frame.StageTiles, StageTileInstance.SIZE * frame.StageTilesLength);
#endif
        }

        public void ReallocStageTiles(int newSize) {
            if (StageTilesLength == newSize) {
                return;
            }

            if (StageTiles != null) {
#if QUANTUM_3_1
                QuantumUnsafe.Free(StageTiles);
#else
                UnsafeUtility.Free(StageTiles, Unity.Collections.Allocator.Persistent);
#endif
                StageTiles = null;
            }
            
            if (newSize > 0) {
#if QUANTUM_3_1
                StageTiles = (StageTileInstance*) QuantumUnsafe.Alloc(StageTileInstance.SIZE * newSize, StageTileInstance.ALIGNMENT);
#else
                StageTiles = (StageTileInstance*) UnsafeUtility.Malloc(StageTileInstance.SIZE * newSize, StageTileInstance.ALIGNMENT, Unity.Collections.Allocator.Persistent);
#endif
            }

            StageTilesLength = newSize;
        }

        public bool PlayerIsConnected(PlayerRef player) {
            return ResolveDictionary(Global->PlayerDatas).ContainsKey(player);
        }
    }
}
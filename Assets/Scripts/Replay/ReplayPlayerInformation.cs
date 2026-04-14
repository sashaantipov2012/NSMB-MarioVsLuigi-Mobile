using NSMB.Utilities;
using Quantum;
using System.IO;

namespace NSMB.Replay {
    public struct ReplayPlayerInformation {
        public string Nickname;
        public int FinalObjectiveCount;
        public byte Team;
        public AssetRef<CharacterAsset> Character;
        public PlayerRef PlayerRef;

        public readonly void Serialize(BinaryWriter writer) {
            writer.Write(Nickname);
            writer.Write(FinalObjectiveCount);
            writer.Write(Team);
            writer.Write(Character.Id.Value);
            writer.Write(PlayerRef);
        }

        public static ReplayPlayerInformation Deserialize(BinaryReader reader, GameVersion version) {
            if (version >= new GameVersion(2, 0, 5)) {
                return new ReplayPlayerInformation {
                    Nickname = reader.ReadString(),
                    FinalObjectiveCount = reader.ReadInt32(),
                    Team = reader.ReadByte(),
                    Character = new AssetRef<CharacterAsset>(new AssetGuid(reader.ReadInt64())),
                    PlayerRef = reader.ReadInt32(),
                };
            } else {
                return new ReplayPlayerInformation {
                    Nickname = reader.ReadString(),
                    FinalObjectiveCount = reader.ReadInt32(),
                    Team = reader.ReadByte(),
                    Character = AssetRepository<CharacterAsset>.AllAssetRefs[reader.ReadByte()],
                    PlayerRef = reader.ReadInt32(),
                };
            }
        }
    }
}

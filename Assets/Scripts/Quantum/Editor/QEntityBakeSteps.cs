using Quantum;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NSMB.Quantum {
    public class BigStarBakeStep : IQuantumBakeStep {
        int IQuantumBakeStep.Order => -1;
        void IQuantumBakeStep.OnBake(QuantumMapData data, VersusStageData stage) {
            GameObject[] starSpawns = GameObject.FindGameObjectsWithTag("StarSpawn");
            if (starSpawns.Length <= 0) {
                throw new QuantumBakeException($"No star spawns are defined! This might cause issues.");
            } else if (starSpawns.Length > Constants.MaxStarSpawns) {
                throw new QuantumBakeException($"The stage data has a limit of {Constants.MaxStarSpawns} star spawns! (Found {starSpawns.Length}). To change this, modify '#define MaxStarSpawns' in BigStar.qtn");
            }

            stage.BigStarSpawnpoints =
                starSpawns
                    .Select(go => go.transform.position.ToFPVector2())
                    .Take(Constants.MaxStarSpawns)
                    .ToArray();

            Debug.Log($"Baked {stage.BigStarSpawnpoints.Length} Big Star spawns");
        }
    }

    public class EnemyBakeStep : IQuantumBakeStep {
        void IQuantumBakeStep.OnBake(QuantumMapData data, VersusStageData stage) {
            var enemies = GameObject.FindObjectsByType<QPrototypeEnemy>(FindObjectsSortMode.None);
            foreach (var enemy in enemies) {
                enemy.Prototype.Spawnpoint = enemy.transform.position.ToFPVector2();
                EditorUtility.SetDirty(enemy);
            }
            if (enemies.Length > 0) {
                Debug.Log($"Baked {enemies.Length} enemy spawnpoints.");
            }
        }
    }

    public class DonutBlockBakeStep : IQuantumBakeStep {
        void IQuantumBakeStep.OnBake(QuantumMapData data, VersusStageData stage) {
            var donutBlocks = GameObject.FindObjectsByType<QPrototypeDonutBlock>(FindObjectsSortMode.None);
            foreach (var donutBlock in donutBlocks) {
                donutBlock.Prototype.Origin = donutBlock.transform.position.ToFPVector2();
                EditorUtility.SetDirty(donutBlock);
            }
            if (donutBlocks.Length > 0) {
                Debug.Log($"Baked {donutBlocks.Length} Donut Blocks.");
            }
        }
    }
}

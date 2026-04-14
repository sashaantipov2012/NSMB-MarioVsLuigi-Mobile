using UnityEngine;

namespace NSMB.Utilities {
    [CreateAssetMenu(menuName = "Build Identifier")]
    public class BuildIdentifier : ScriptableObject {
        public string Identifier;
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(BuildIdentifier))]
    public class BuildIdentifierEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            UnityEditor.EditorGUILayout.LabelField("The value below separates the game's servers into groups.\n" +
                "Players can only see rooms and join others that have executables built with the same ID.\n" +
                "Change this if you're making a mod and don't want it to connect to the main game!",
                UnityEditor.EditorStyles.wordWrappedLabel);

            UnityEditor.EditorGUILayout.Space();

            ((BuildIdentifier) target).Identifier = UnityEditor.EditorGUILayout.TextField("Identifier", ((BuildIdentifier) target).Identifier);

            UnityEditor.EditorUtility.SetDirty(target);
        }
    }
#endif
}

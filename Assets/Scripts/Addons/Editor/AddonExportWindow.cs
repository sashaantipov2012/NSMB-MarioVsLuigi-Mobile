using JimmysUnityUtilities;
using Newtonsoft.Json;
using NSMB.Addons;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NSMB.Editor {

    public class AddonBuildWindow : EditorWindow {

        private static readonly Dictionary<BuildTarget, string> BuildTargets = new() {
            [BuildTarget.StandaloneWindows64] = "Win64",
            //[BuildTarget.StandaloneWindows] = "Win32",
            //[BuildTarget.StandaloneOSX] = "MacOS",
            //[BuildTarget.StandaloneLinux64] = "Linux",
            //[BuildTarget.Android] = "Android",
            //[BuildTarget.iOS] = "iOS",
            //[BuildTarget.WebGL] = "WebGL",
        };

        public class BuildableAddon {
            public string FolderPath;
            public AddonDefinition AddonDef;

            public string FolderName => new DirectoryInfo(FolderPath).Name;
        }

        private List<BuildableAddon> availableAddonFolders;
        private int selectedAddonFolder;
        private Vector2 addonFolderSelectScroll;


        [MenuItem("Tools/MvLO/Addons/Build", secondaryPriority = 2)]
        public static void BuildAddons() {
            GetWindow<AddonBuildWindow>();
        }

        public void OnEnable() {
            availableAddonFolders = new();
            try {
                foreach (var folderPath in Directory.GetDirectories("Assets/Addons")) {
                    if (new DirectoryInfo(folderPath).Name.StartsWith('.')) {
                        continue;
                    }

                    BuildableAddon buildableAddon = new() {
                        FolderPath = folderPath
                    };
                    string addonDefPath = folderPath + "/addon.json";
                    try {
                        string addonDefJson = File.ReadAllText(addonDefPath);
                        buildableAddon.AddonDef = JsonConvert.DeserializeObject<AddonDefinition>(addonDefJson);
                        try {
                            buildableAddon.AddonDef.IconTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(buildableAddon.AddonDef.IconAssetPath);
                        } catch { }
                    } catch (Exception e) {
                        Debug.LogWarning($"Failed to find/parse addon definition of addon folder {folderPath} (path: {addonDefPath})");
                        Debug.LogError(e);
                    }
                    availableAddonFolders.Add(buildableAddon);
                }
            } catch { }
        }

        public void OnDisable() {
            foreach (var addon in availableAddonFolders) {
                string addonDefPath = addon.FolderPath + "/addon.json";
                File.WriteAllText(addonDefPath, JsonConvert.SerializeObject(addon.AddonDef));
            }
        }

        public void OnGUI() {
            if (availableAddonFolders.Count <= 0) {
                EditorGUILayout.LabelField("No addon folders exist!", EditorStyles.boldLabel);

                if (GUILayout.Button("Open Addon Create Window")) {
                    AddonCreateWindow.CreateAddon();
                    Close();
                }
                return;
            }

            EditorGUILayout.LabelField("Select a folder to build into an addon", EditorStyles.boldLabel);
            addonFolderSelectScroll = EditorGUILayout.BeginScrollView(addonFolderSelectScroll);
            selectedAddonFolder = GUILayout.SelectionGrid(selectedAddonFolder, availableAddonFolders.Select(ba => " " + ba.FolderName).ToArray(), 1, EditorStyles.radioButton);
            EditorGUILayout.EndScrollView();

            // Separator line
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            BuildableAddon selectedAddon = availableAddonFolders[selectedAddonFolder];
            selectedAddon.AddonDef.DisplayName = EditorGUILayout.TextField("Name", selectedAddon.AddonDef.DisplayName);
            selectedAddon.AddonDef.Author = EditorGUILayout.TextField("Author", selectedAddon.AddonDef.Author);
            selectedAddon.AddonDef.Version = EditorGUILayout.TextField("Version", selectedAddon.AddonDef.Version);
            selectedAddon.AddonDef.Description = EditorGUILayout.TextField("Description", selectedAddon.AddonDef.Description);
            selectedAddon.AddonDef.IconTexture = (Texture2D) EditorGUILayout.ObjectField("Icon", selectedAddon.AddonDef.IconTexture, typeof(Texture2D), false);
            if (selectedAddon.AddonDef.IconTexture) {
                selectedAddon.AddonDef.IconAssetPath = AssetDatabase.GetAssetPath(selectedAddon.AddonDef.IconTexture);
            }
            if (GUILayout.Button("Build")) {
                string savePath = $"ExportedAddons/{selectedAddon.FolderName}-{selectedAddon.AddonDef.Version}";

                if (Directory.Exists(savePath)) {
                    if (!EditorUtility.DisplayDialog("Addon exists", $"The addon build path {savePath} already exists.\nCreating an addon with the same name + version as another might be confusing.\n\nWould you like to overwrite the existing files and continue the build anyway?", "Yes", "No")) {
                        return;
                    }
                    Directory.Delete(savePath, true);
                }
                Directory.CreateDirectory(savePath);

                // Clean old addon folder
                string buildPath = "ExportedAddons/temp";
                try {
                    Directory.Delete(buildPath, true);
                } catch { }
                Directory.CreateDirectory(buildPath);

                // Update the addon.json
                selectedAddon.AddonDef.ReleaseGuid = Guid.NewGuid();

                // Write it to the Unity asset
                File.WriteAllText(
                    selectedAddon.FolderPath + "/addon.json",
                    JsonConvert.SerializeObject(selectedAddon.AddonDef, Formatting.Indented)
                );

                // Copy it to the export folder
                File.Copy(selectedAddon.FolderPath + "/addon.json", buildPath + "/addon.json");

                // Add icon
                if (selectedAddon.AddonDef.IconTexture) {
                    RenderTexture outputTexture = RenderTexture.GetTemporary(selectedAddon.AddonDef.IconTexture.width, selectedAddon.AddonDef.IconTexture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
                    Graphics.Blit(selectedAddon.AddonDef.IconTexture, outputTexture);
                    Texture2D writeableIcon = outputTexture.ToTexture2D();
                    File.WriteAllBytes(buildPath + "/icon.png", writeableIcon.EncodeToPNG());
                    RenderTexture.ReleaseTemporary(outputTexture);
                    DestroyImmediate(writeableIcon);
                }

                // Generate list of asset bundles.
                List<AssetBundleBuild> buildMap = new() {
                    new() {
                        assetBundleName = "basegame-assets",
                        assetNames = AssetDatabase.GetAssetPathsFromAssetBundle("basegame-assets"),
                    },
                    new() {
                        assetBundleName = "basegame-scenes",
                        assetNames = AssetDatabase.GetAssetPathsFromAssetBundle("basegame-scenes"),
                    }
                };

                string[] sceneAssets =
                    AssetDatabase.FindAssets("t:Scene", new[] { selectedAddon.FolderPath })
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .ToArray();
                if (sceneAssets.Length > 0) {
                    buildMap.Add(new() {
                        assetBundleName = selectedAddon.AddonDef.ReleaseGuid + "-scenes",
                        assetNames = sceneAssets,
                    });
                }

                string[] nonSceneAssets =
                    AssetDatabase.FindAssets("", new[] { selectedAddon.FolderPath })
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Except(sceneAssets)
                    .ToArray();
                if (nonSceneAssets.Length > 0) {
                    buildMap.Add(new() {
                        assetBundleName = selectedAddon.AddonDef.ReleaseGuid + "-assets",
                        assetNames = nonSceneAssets,
                    });
                }

                // Build asset bundles
                int steps = BuildTargets.Count + 1;
                int counter = 0;

                var oldBuildTarget = EditorUserBuildSettings.activeBuildTarget;
                var oldBuildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
                var buildMapArray = buildMap.ToArray();
                List<BuildTarget> failedBuilds = new();
                foreach ((var buildTarget, _) in BuildTargets) {
                    try {
                        EditorUtility.DisplayProgressBar("Building addon", $"Building for {buildTarget} build target...", (float) ++counter / steps);
                        string platformBuildPath = buildPath + "/" + buildTarget;
                        Directory.CreateDirectory(platformBuildPath);
                        BuildPipeline.BuildAssetBundles(platformBuildPath, 
                            buildMapArray, 
                            BuildAssetBundleOptions.AppendHashToAssetBundleName | BuildAssetBundleOptions.AssetBundleStripUnityVersion, 
                            buildTarget);

                        // Delete {buildTarget} bundle- it only contains a manifest and causes name collisions.
                        File.Delete(platformBuildPath + "/" + buildTarget);

                        // Delete .manifests
                        foreach (var manifest in Directory.EnumerateFiles(platformBuildPath, "*.manifest", new EnumerationOptions { RecurseSubdirectories = true })) {
                            File.Delete(manifest);
                        }

                        // Delete base game assets / scenes
                        foreach (var basegameBundle in Directory.EnumerateFiles(platformBuildPath, "", new EnumerationOptions { RecurseSubdirectories = true }).Where(filename => filename.Contains("basegame"))) {
                            File.Delete(basegameBundle);
                        }

                        Debug.Log($"Successfully built addon for platform {buildTarget}");
                    } catch (Exception e) {
                        Debug.LogError($"Failed to export addon for platform {buildTarget}");
                        Debug.LogError(e);
                        failedBuilds.Add(buildTarget);
                    }
                }

                //EditorUserBuildSettings.SwitchActiveBuildTarget(oldBuildTargetGroup, oldBuildTarget);

                /*
                // Create standalone addons
                foreach ((var buildTarget, var buildTargetPretty) in BuildTargets) {
                    if (failedBuilds.Contains(buildTarget)) {
                        continue;
                    }

                    string platformZipPath = $"{savePath}/{selectedAddon.FolderName}-{selectedAddon.AddonDef.Version}-{buildTargetPretty}{AddonManager.AddonExtension}";
                    ZipFile.CreateFromDirectory(buildPath, platformZipPath);

                    // Open and remove unrelated entries
                    using var platformZip = ZipFile.Open(platformZipPath, ZipArchiveMode.Update);
                    var entriesToRemove = platformZip.Entries
                        .Where(en => en.FullName.Contains('/') && !en.FullName.StartsWith($"{buildTarget}/"))
                        .ToList();

                    foreach (var entry in entriesToRemove) {
                        entry.Delete();
                    }
                }
                */

                // Create universal addon
                string universalZipPath = $"{savePath}/{selectedAddon.FolderName}-{selectedAddon.AddonDef.Version}-Universal{AddonManager.AddonExtension}";
                EditorUtility.DisplayProgressBar("Building addon", "Compressing into .mvladdon...", (float) ++counter / steps);
                ZipFile.CreateFromDirectory(buildPath, universalZipPath);

                // Clean
                try {
                    Directory.Delete(buildPath, true);
                } catch { }
                EditorUtility.ClearProgressBar();

                if (failedBuilds.Count > 0) {
                    Debug.LogError($"Addon build error: The following builds failed:\n* {string.Join("\n* ", failedBuilds)}\n\nThe addon was saved to {universalZipPath}");
                    EditorUtility.DisplayDialog("Build(s) Failed", $"The following builds failed:\n* {string.Join("\n* ", failedBuilds)}\n\nThe addon was saved to {universalZipPath}", "OK");
                } else {
                    Debug.Log($"Addon build successful: The addon was saved to {universalZipPath}");
                    EditorUtility.DisplayDialog("Build Successful", $"The addon was saved to {universalZipPath}", "OK");
                }

                Close();
            }
        }

        public class AddonCreateWindow : EditorWindow {

            private string displayName = "New Addon", author = "Unknown", version = "v1.0", description = "A brand-new MvLO addon.";

            [MenuItem("Tools/MvLO/Addons/Create", secondaryPriority = 1)]
            public static void CreateAddon() {
                GetWindow<AddonCreateWindow>();
            }

            public void OnGUI() {
                EditorGUILayout.LabelField("Addon information", EditorStyles.boldLabel);

                displayName = EditorGUILayout.TextField("Name", displayName);
                author = EditorGUILayout.TextField("Author", author);
                version = EditorGUILayout.TextField("Version", version);
                description = EditorGUILayout.TextField("Description", description);

                if (GUILayout.Button("Create")) {
                    string folderName = string.Concat(displayName.Split(Path.GetInvalidFileNameChars()));

                    // Create folder
                    string folder = $"Assets/Addons/{folderName}";
                    if (Directory.Exists(folder)) {
                        if (!EditorUtility.DisplayDialog("Addon already exists!", $"An addon already exists with this name.\nDo you want to overwrite it's addon definition?\n(Assets will be preserved.)", "Yes", "Cancel")) {
                            return;
                        }
                    } else {
                        Directory.CreateDirectory(folder);
                    }

                    // Write addon definition
                    File.WriteAllText(
                        Path.Combine(folder, "addon.json"),
                        JsonUtility.ToJson(new AddonDefinition {
                            DisplayName = displayName,
                            Author = author,
                            Version = version,
                            Description = description,
                        }, true));

                    EditorUtility.DisplayDialog("Addon created!", $"Successfully created a new addon \"{displayName}\".\n\nFolder: {folder}\nAny assets/stages placed inside this folder will be included in the addon when built.", "Ok");

                    Close();
                }
            }
        }
    }
}
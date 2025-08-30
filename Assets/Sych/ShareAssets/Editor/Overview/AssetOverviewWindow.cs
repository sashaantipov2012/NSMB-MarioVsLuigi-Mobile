using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Sych.ShareAssets.Editor.Overview
{
    internal sealed class AssetOverviewWindow : EditorWindow
    {
        private const float VerticalPadding = 8f;
        private const float HorizontalPadding = 10f;
        private const float MaxButtonWight = 220f;
        private static readonly Color32 BackgroundColor = new(33, 33, 33, 255);
        private static readonly Color32 ButtonsNormalColor = new(115, 66, 230,  255);
        private static readonly Color32 ButtonsPressedColor = new(255, 151, 0, 255);

        private static AssetOverviewSettings _settings;
        private static bool _isDontShownAgainEnabled;
        private static bool _isiOSSupport;
        private static bool _isAndroidSupport;
        private static string _assetVersion;

        private static GUIStyle _textStyleGreen;
        private static GUIStyle _textStyleRed;
        private static GUIStyle _textStyleYellow;
        private static GUIStyle _headerLabelStyle;
        private static GUIStyle _labelStyle;
        private static GUIStyle _textStyle;
        
        private static AssetOverviewSettings Settings => _settings ??= AssetOverviewSettingsProvider.GetSettings();

        [MenuItem("Tools/Share/Overview", false, 5000)]
        public static void ShowWindow()
        {
            Initialize();
            var window = GetWindow<AssetOverviewWindow>(Settings.PluginName);
            window.minSize = new Vector2(450, 415);
            window.maxSize = new Vector2(450, 415);
            window.Show();
        }

        private void OnDisable() => SetDontShownAgain(_isDontShownAgainEnabled);

        private void OnGUI()
        {
            try
            {
                var backgroundTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                backgroundTexture.SetPixel(0, 0, BackgroundColor);
                backgroundTexture.Apply();
                GUI.DrawTexture(new Rect(0, 0, maxSize.x, maxSize.y), backgroundTexture, ScaleMode.StretchToFill);

                GUILayout.Space(VerticalPadding);
                GUILayout.Label($"Welcome to {Settings.PluginName}!", _headerLabelStyle);
                GUILayout.Space(VerticalPadding);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(HorizontalPadding);
                GUILayout.Label(Settings.PluginDescription, _textStyle);
                GUILayout.Space(HorizontalPadding);
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(VerticalPadding);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(HorizontalPadding);
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField($"IOS bundle: {(_isiOSSupport ? "Included" : "Excluded")}",
                    _isiOSSupport ? _textStyleGreen : _textStyleRed);
                GUILayout.Space(2f);
                EditorGUILayout.LabelField($"Android bundle: {(_isAndroidSupport ? "Included" : "Excluded")}",
                    _isAndroidSupport ? _textStyleGreen : _textStyleRed);
                GUILayout.Space(2f);
                EditorGUILayout.TextArea($"Support: {Settings.SupportEmail}", _labelStyle);
                GUILayout.Space(8f);
                EditorGUILayout.TextArea($"License: {Settings.License}", _textStyleYellow);
                GUILayout.Space(8f);
                EditorGUILayout.TextArea($"Version: {_assetVersion}", _labelStyle);
                EditorGUILayout.EndVertical();
                GUILayout.Space(HorizontalPadding);
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(20f);
                EditorGUILayout.LabelField("Thank you for using this asset ❤️", _headerLabelStyle);
                GUILayout.Space(20f);

                DrawButton("Rate this asset ❤️ (+1 to karma)", () => Application.OpenURL(Settings.CurrentAssetStoreLink));
                DrawButton("Other assets", () => Application.OpenURL(Settings.AllAssetStoreLink));
                DrawButton("Documentation", () =>
                {
                    var readmePath = DocumentationProvider.GetFilePath();
                    if (File.Exists(readmePath))
                        EditorUtility.OpenWithDefaultApp(readmePath);
                });
                DrawButton("Example scene", () =>
                {
                    var scenePath = ExampleSceneProvider.GetScenePath();
                    if (File.Exists(scenePath))
                        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                });
                DrawButton("Get started using asset", Close);

                GUILayout.Space(5f);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                _isDontShownAgainEnabled = GUILayout.Toggle(_isDontShownAgainEnabled, "Do not show this again");
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
            catch (Exception)
            {
                Close();
            }
        }

        private static void Initialize()
        {
            _headerLabelStyle = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white },
                fontSize = 14
            };
            _textStyleGreen = new GUIStyle
            {
                fontStyle = FontStyle.Bold,
                fontSize = 11,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.green }
            };
            _textStyleRed = new GUIStyle
            {
                fontStyle = FontStyle.Bold,
                fontSize = 11,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.red }
            };
            _textStyleYellow = new GUIStyle
            {
                fontStyle = FontStyle.Bold,
                fontSize = 11,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.yellow }
            };
            _labelStyle = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white },
                fontSize = 11,
                wordWrap = true
            };
            _textStyle = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Normal,
                normal = { textColor = Color.white },
                fontSize = 12,
                wordWrap = true
            };

            _isDontShownAgainEnabled = !Settings.IsIntroductionEnabled;
            _isAndroidSupport = IsTypeExists(Settings.AndroidBundleMainType, Settings.AndroidBundleMainAssembly);
            _isiOSSupport = IsTypeExists(Settings.IosBundleMainType, Settings.IosBundleMainAssembly);
            _assetVersion = VersionProvider.GetVersion();
        }

        private static Texture2D MakeTexture(int width, int height, Color color)
        {
            var pix = new Color[width * height];
            for (var i = 0; i < pix.Length; i++)
                pix[i] = color;
            var result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private static void DrawButton(string text, Action action)
        {
            GUILayout.Space(1f);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var buttonStyle = new GUIStyle
            {
                normal =
                {
                    textColor = Color.white,
                    background = MakeTexture(1,1, ButtonsNormalColor)
                },
                active = {
                    textColor = Color.white,
                    background = MakeTexture(1,1, ButtonsPressedColor)
                },
                hover = {
                    textColor = Color.white,
                    background = MakeTexture(1,1, ButtonsPressedColor)
                },
                focused = {
                    textColor = Color.white,
                    background = MakeTexture(1,1, ButtonsPressedColor)
                },
                margin = new RectOffset(4,4,2,2),
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            
            if (GUILayout.Button(text, buttonStyle, GUILayout.Height(25), GUILayout.MaxWidth(MaxButtonWight)))
                action.Invoke();
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
        
        private static void SetDontShownAgain(bool enable)
        {
            if(Settings == null)
                return;
            
            Settings.IsIntroductionEnabled = !enable;
            AssetOverviewSettingsProvider.SatSettings(Settings);
        }

        private static bool IsTypeExists(string typeName, string assemblyName) => Type.GetType($"{assemblyName}.{typeName}, {assemblyName}") != null;

        [InitializeOnLoad]
        private class Initializer
        {
            static Initializer()
            {
                var sessionKey = $"is_vs_window_shown_{typeof(AssetOverviewWindow).Namespace}";
                if (SessionState.GetBool(sessionKey, false))
                    return;

                SessionState.SetBool(sessionKey, true);

                if(Settings is not { IsIntroductionEnabled: true })
                    return;
                
                EditorApplication.delayCall += ShowWindow;
            }
        }
    }
}
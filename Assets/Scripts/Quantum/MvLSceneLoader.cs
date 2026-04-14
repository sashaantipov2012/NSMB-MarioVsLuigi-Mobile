using Quantum;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NSMB.Quantum {
    public class MvLSceneLoader : MonoBehaviour {

        //---Static
        public static MvLSceneLoader Instance;

        //---Properties
        public Map CurrentLoadedMap => currentMap;

        //---Private Variables
        private Coroutine loadingCoroutine;
        private Map currentMap;
        private Scene currentScene;

        public void Start() {
            Instance = this;
            QuantumCallback.Subscribe<CallbackUpdateView>(this, OnUpdateView);
            QuantumCallback.Subscribe<CallbackGameDestroyed>(this, OnGameDestroyed);
        }

        public void OnUpdateView(CallbackUpdateView e) {
            if (loadingCoroutine != null) {
                return;
            }

            QuantumGame game = e.Game;
            Frame f = game.Frames.Predicted;
            if ((game.Session.GameMode == Photon.Deterministic.DeterministicGameMode.Replay || game.GetLocalPlayers().Count > 0)
                && f.Map != currentMap) {
                loadingCoroutine = StartCoroutine(SceneChangeCoroutine(game, currentMap, f.Map));
            }
        }

        public void OnGameDestroyed(CallbackGameDestroyed e) {
            if (loadingCoroutine != null) {
                StopCoroutine(loadingCoroutine);
            }
            loadingCoroutine = StartCoroutine(SceneChangeCoroutine(e.Game, currentMap, null));
        }

        private IEnumerator SceneChangeCoroutine(QuantumGame game, Map oldMap, Map newMap) {
            if (oldMap == newMap) {
                loadingCoroutine = null;
                yield break;
            }

            Scene oldScene = currentScene;

            // Load new map
            string newSceneName = newMap ? newMap.Scene : null;
            QuantumCallback.Dispatcher.Publish(new CallbackUnitySceneLoadBegin(game) { SceneName = newSceneName });
            yield return LoadMap(newMap);
            QuantumCallback.Dispatcher.Publish(new CallbackUnitySceneLoadDone(game) { SceneName = newSceneName });
            currentMap = newMap;

            // Unload previous map (if available)
            if (oldScene.IsValid()) {
                string oldSceneName = oldMap ? oldMap.Scene : null;
                QuantumCallback.Dispatcher.Publish(new CallbackUnitySceneUnloadBegin(game) { SceneName = oldSceneName });
                yield return SceneManager.UnloadSceneAsync(oldScene);
                QuantumCallback.Dispatcher.Publish(new CallbackUnitySceneUnloadDone(game) { SceneName = oldSceneName });
            }

            loadingCoroutine = null;
        }

        private IEnumerator LoadMap(Map map) {
            // Handle special MainMenu case.
            if (map == null) {
                Scene loadedMainMenuScene = SceneManager.GetSceneByName("MainMenu");
                if (loadedMainMenuScene.IsValid()) {
                    SceneManager.SetActiveScene(loadedMainMenuScene);
                } else {
                    AsyncOperation mainMenuOp = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
                    mainMenuOp.allowSceneActivation = true;
                    while (!mainMenuOp.isDone) {
                        yield return null;
                    }
                }
                currentScene = default;
                yield break;
            }
            
            // Check if the scene already is loaded
            Scene loadedScene = SceneManager.GetSceneByPath(map.ScenePath);
            if (!loadedScene.IsValid()) {
#if UNITY_EDITOR
                var loadOp = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(map.ScenePath, new LoadSceneParameters { loadSceneMode = LoadSceneMode.Additive });
#else
                var loadOp = SceneManager.LoadSceneAsync(map.ScenePath, LoadSceneMode.Additive);
#endif
                loadOp.allowSceneActivation = true;
                yield return loadOp;
                loadedScene = SceneManager.GetSceneByPath(map.ScenePath);
            }

            currentScene = loadedScene;
            SceneManager.SetActiveScene(currentScene);
        }
    }
}

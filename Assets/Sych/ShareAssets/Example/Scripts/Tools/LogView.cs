using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Sych.ShareAssets.Example.Tools
{
    public class LogView : MonoBehaviour
    {
        private const int MaxLogsCount = 50;
        [SerializeField] private RectTransform _logsRoot;
        [SerializeField] private ScrollRect _logsScroll;
        [SerializeField] private InputField _logMessageReference;
        [SerializeField] private InputField _logErrorReference;
        [SerializeField] private InputField _logWarningReference;

        public void LogMessage(string log) => Log(log, LogType.Log, _logMessageReference);

        public void LogWarning(string log) => Log(log, LogType.Warning, _logWarningReference);

        public void LogError(string log) => Log(log, LogType.Error, _logErrorReference);

        private void Log(string log, LogType logType, InputField logReference)
        {
            var logText = Instantiate(logReference, _logsRoot);
            logText.gameObject.SetActive(true);
            logText.text = $"[{DateTime.Now:HH:mm:ss}] [{logType.ToString()}]: {log}";
            UpdateLogsCount();
            logText.gameObject.SetActive(false);
            logText.gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_logsRoot);
            StartCoroutine(UpdateScroll());
        }

        private void UpdateLogsCount()
        {
            if (_logsRoot.childCount > MaxLogsCount)
                Destroy(_logsRoot.GetChild(0).gameObject);
        }

        private IEnumerator UpdateScroll()
        {
            _logsScroll.verticalNormalizedPosition = 0f;
            yield return new WaitForEndOfFrame();
            _logsScroll.verticalNormalizedPosition = 0f;
        }
    }
}
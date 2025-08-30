using System;
using System.Collections.Generic;
using System.IO;
using Sych.ShareAssets.Example.Tools;
using Sych.ShareAssets.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace Sych.ShareAssets.Example
{
    public class ExampleController : MonoBehaviour
    {
        [SerializeField] private LogView _logView;
        [SerializeField] private Text _title;
        [SerializeField] private Button _share;
        [SerializeField] private InputField _text;
        [SerializeField] private InputField _fileName;
        [SerializeField] private InputField _url;

        private void Awake()
        {
            _share.onClick.AddListener(ShareClicked);
            _text.text = "Hello, world!";
            _fileName.text = string.Empty;
            _url.text = string.Empty;
            
            _logView.LogMessage($"{_title.text} started.");
        }

        private void OnDestroy() => _share.onClick.RemoveAllListeners();

        private void ShareClicked()
        {
            if (!Share.IsPlatformSupported)
            {
                _logView.LogError("Share: platform not supported");
                return;
            }
            
            var items = new List<string>();
            if (!string.IsNullOrEmpty(_text.text))
                items.Add(_text.text);
            if (!string.IsNullOrEmpty(_fileName.text))
            {
                var fileName = $"{_fileName.text}.txt";
                var filePath = CreateSampleAttachment(fileName);
                items.Add(filePath); // or for example $"{Application.streamingAssetsPath}/your_file.extension";
            }
            if (!string.IsNullOrEmpty(_url.text))
                items.Add(_url.text);

            _logView.LogMessage("Share: requested");
            Share.Items(items, success =>
            {
                _logView.LogMessage($"Share: {(success ? "success" : "failed")}");
            });
        }

        private string CreateSampleAttachment(string fileName)
        {
            var filePath = Path.Combine(Application.persistentDataPath, fileName);
            if (File.Exists(filePath))
                return filePath;

            var content = $"This is a test attachment generated on {DateTime.Now}\n" +
                          $"From device: {SystemInfo.deviceModel}\n" +
                          $"Platform: {Application.platform}\n" +
                          $"Unity version: {Application.unityVersion}";

            File.WriteAllText(filePath, content);
            return filePath;
        }
    }
}

using Quantum.Editor;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class AutoRefreshWhileMinimized {
    private static FileSystemWatcher watcher;
    private static bool queuedReimport;

    static AutoRefreshWhileMinimized() {
        watcher = new FileSystemWatcher(Application.dataPath) {
            NotifyFilter = NotifyFilters.CreationTime
                         | NotifyFilters.DirectoryName
                         | NotifyFilters.FileName
                         | NotifyFilters.LastWrite
                         | NotifyFilters.Size,
            Filter = "*.qtn",
            IncludeSubdirectories = true,
            EnableRaisingEvents = true
        };

        watcher.Changed += (_, _) => queuedReimport = true;
        watcher.Created += (_, _) => queuedReimport = true;
        watcher.Deleted += (_, _) => queuedReimport = true;
        watcher.Renamed += (_, _) => queuedReimport = true;

        EditorApplication.update += () => {
            if (queuedReimport) {
                QuantumCodeGenQtn.Run();
                queuedReimport = false;
            }
        };

        AppDomain.CurrentDomain.DomainUnload += (_, _) => watcher?.Dispose();
    }
}

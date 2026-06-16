using System.IO;
using UnityEditor;
using UnityEngine;

namespace HierarchyPlus.Editor
{
    /// <summary>
    /// AsmdefInstaller — One-shot editor utility that writes the two Assembly
    /// Definition files required.
    ///
    /// Run once via:  HierarchyPlus → Install Assembly Definitions
    ///
    /// After execution this script can be deleted — it has no runtime purpose.
    /// </summary>
    public static class AsmdefInstaller
    {
        private const string RuntimeAsmdefPath = "Assets/HierarchyPlus/Runtime/HierarchyPlus.Runtime.asmdef";
        private const string EditorAsmdefPath  = "Assets/HierarchyPlus/Editor/HierarchyPlus.Editor.asmdef";

        private const string RuntimeAsmdefContent = @"{
            ""name"": ""HierarchyPlus.Runtime"",
            ""rootNamespace"": ""HierarchyPlus"",
            ""references"": [],
            ""includePlatforms"": [],
            ""excludePlatforms"": [],
            ""allowUnsafeCode"": false,
            ""overrideReferences"": false,
            ""precompiledReferences"": [],
            ""autoReferenced"": true,
            ""defineConstraints"": [],
            ""versionDefines"": [],
            ""noEngineReferences"": false
        }";

        private const string EditorAsmdefContent = @"{
            ""name"": ""HierarchyPlus.Editor"",
            ""rootNamespace"": ""HierarchyPlus.Editor"",
            ""references"": [
                ""HierarchyPlus.Runtime""
            ],
            ""includePlatforms"": [
                ""Editor""
            ],
            ""excludePlatforms"": [],
            ""allowUnsafeCode"": false,
            ""overrideReferences"": false,
            ""precompiledReferences"": [],
            ""autoReferenced"": false,
            ""defineConstraints"": [],
            ""versionDefines"": [],
            ""noEngineReferences"": false
        }";

        [MenuItem("HierarchyPlus/Install Assembly Definitions")]
        public static void Install()
        {
            WriteIfAbsent(RuntimeAsmdefPath, RuntimeAsmdefContent, "Runtime");
            WriteIfAbsent(EditorAsmdefPath,  EditorAsmdefContent,  "Editor");
            AssetDatabase.Refresh();
            Debug.Log("[HierarchyPlus] Assembly Definition files written. Unity will recompile.");
        }

        private static void WriteIfAbsent(string path, string content, string label)
        {
            if (File.Exists(path))
            {
                Debug.Log($"[HierarchyPlus] {label} asmdef already exists — skipped.");
                return;
            }

            File.WriteAllText(path, content);
            Debug.Log($"[HierarchyPlus] Created {label} asmdef at {path}");
        }
    }
}

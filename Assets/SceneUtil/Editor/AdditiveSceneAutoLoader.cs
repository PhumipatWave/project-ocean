#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Standalone [InitializeOnLoad] class — fires on every domain reload.
/// Listens for scene opens and auto-loads configured additive scenes.
/// Config asset lives at Assets/SceneUtil/AdditiveSceneConfig.asset (outside Editor folder).
/// </summary>
[InitializeOnLoad]
public static class AdditiveSceneAutoLoader
{
    private const string CONFIG_PATH = "Assets/SceneUtil/AdditiveSceneConfig.asset";

    static AdditiveSceneAutoLoader()
    {
        EditorSceneManager.sceneOpened -= OnSceneOpened;
        EditorSceneManager.sceneOpened += OnSceneOpened;
        Debug.Log("[AdditiveSceneSetup] AutoLoader hooked into sceneOpened.");
    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        if (mode == OpenSceneMode.Additive) return;

        Debug.Log($"[AdditiveSceneSetup] Scene opened: '{scene.path}' mode={mode}");

        var config = AssetDatabase.LoadAssetAtPath<AdditiveSceneConfig>(CONFIG_PATH);
        if (config == null)
        {
            Debug.LogWarning($"[AdditiveSceneSetup] Config asset not found at: {CONFIG_PATH}");
            return;
        }

        if (!config.autoLoadEnabled)
        {
            Debug.Log("[AdditiveSceneSetup] Auto-load is disabled in config.");
            return;
        }

        var rule = config.GetRuleForScene(scene.path);
        if (rule == null)
        {
            Debug.Log($"[AdditiveSceneSetup] No rule found for scene path: '{scene.path}' " +
                      $"(total rules: {config.rules.Count})");
            // Log all stored paths to help diagnose mismatches
            foreach (var r in config.rules)
                Debug.Log($"  [AdditiveSceneSetup] Known rule path: '{r.mainScenePath}'");
            return;
        }

        if (rule.additiveScenes.Count == 0)
        {
            Debug.Log("[AdditiveSceneSetup] Rule found but no additive scenes configured.");
            return;
        }

        var scenesToLoad = new System.Collections.Generic.List<string>();
        foreach (var entry in rule.additiveScenes)
        {
            if (!entry.enabled || string.IsNullOrEmpty(entry.scenePath)) continue;
            scenesToLoad.Add(entry.scenePath);
        }

        if (scenesToLoad.Count == 0)
        {
            Debug.Log("[AdditiveSceneSetup] All additive entries are disabled or empty.");
            return;
        }

        Debug.Log($"[AdditiveSceneSetup] Scheduling {scenesToLoad.Count} additive scene(s) to load...");

        EditorApplication.delayCall += () =>
        {
            foreach (var scenePath in scenesToLoad)
            {
                bool alreadyOpen = false;
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    if (SceneManager.GetSceneAt(i).path == scenePath)
                    { alreadyOpen = true; break; }
                }

                if (!alreadyOpen)
                {
                    Debug.Log($"[AdditiveSceneSetup] Loading additive: '{scenePath}'");
                    EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                }
                else
                {
                    Debug.Log($"[AdditiveSceneSetup] Already open, skipping: '{scenePath}'");
                }
            }
        };
    }
}
#endif

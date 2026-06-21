using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Runtime equivalent of AdditiveSceneAutoLoader (editor-only).
/// Place this on a persistent GameObject in your first scene (e.g. MainMenuScene).
/// It survives scene loads and auto-loads additive scenes defined in AdditiveSceneConfig.
/// </summary>
public class AdditiveSceneLoader : MonoBehaviour
{
    [SerializeField] private AdditiveSceneConfig config;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Additive) 
            return;

        if (config == null || !config.autoLoadEnabled) 
            return;

        var rule = config.GetRuleForScene(scene.path);
        if (rule == null)
            return;

        foreach (var entry in rule.additiveScenes)
        {
            if (!entry.enabled || string.IsNullOrEmpty(entry.scenePath))
                continue;

            if (IsLoaded(entry.scenePath)) 
                continue;
            SceneManager.LoadSceneAsync(entry.scenePath, LoadSceneMode.Additive);
        }
    }

    private static bool IsLoaded(string scenePath)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).path == scenePath) 
                return true;
        return false;
    }
}

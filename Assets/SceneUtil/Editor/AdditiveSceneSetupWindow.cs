#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor window for configuring additive scene rules.
/// Config ScriptableObject lives at Assets/SceneUtil/AdditiveSceneConfig.asset (outside Editor folder).
/// Menu: Tools > Additive Scene Setup > Open Window
/// </summary>
public class AdditiveSceneSetupWindow : EditorWindow
{
    private const string CONFIG_PATH = "Assets/SceneUtil/AdditiveSceneConfig.asset";

    // ── Config ────────────────────────────────────────────────────────────────
    private static AdditiveSceneConfig LoadConfig()
        => AssetDatabase.LoadAssetAtPath<AdditiveSceneConfig>(CONFIG_PATH);

    private static AdditiveSceneConfig LoadOrCreateConfig()
    {
        var cfg = LoadConfig();
        if (cfg != null) return cfg;
        cfg = ScriptableObject.CreateInstance<AdditiveSceneConfig>();
        AssetDatabase.CreateAsset(cfg, CONFIG_PATH);
        AssetDatabase.SaveAssets();
        Debug.Log($"[AdditiveSceneSetup] Created config at {CONFIG_PATH}");
        return cfg;
    }

    // ── Menu ──────────────────────────────────────────────────────────────────
    [MenuItem("Tools/Additive Scene Setup/Open Window")]
    public static void OpenWindow()
    {
        var win = GetWindow<AdditiveSceneSetupWindow>("Additive Scene Setup");
        win.minSize = new Vector2(500, 400);
        win.Show();
    }

    [MenuItem("Tools/Additive Scene Setup/Toggle Auto-Load on Open")]
    public static void ToggleAutoLoad()
    {
        var config = LoadOrCreateConfig();
        config.autoLoadEnabled = !config.autoLoadEnabled;
        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
        Debug.Log($"[AdditiveSceneSetup] Auto-Load: {(config.autoLoadEnabled ? "ENABLED" : "DISABLED")}");
    }

    [MenuItem("Tools/Additive Scene Setup/Toggle Auto-Load on Open", true)]
    public static bool ToggleAutoLoadValidate()
    {
        var config = LoadConfig();
        Menu.SetChecked("Tools/Additive Scene Setup/Toggle Auto-Load on Open",
            config != null && config.autoLoadEnabled);
        return true;
    }

    // ── Window state ──────────────────────────────────────────────────────────
    private AdditiveSceneConfig _config;
    private Vector2 _scroll;
    private int _selectedRuleIndex = -1;
    private List<string> _buildScenes = new List<string>();

    private void OnEnable()
    {
        _config = LoadOrCreateConfig();
        RefreshBuildScenes();
    }

    private void RefreshBuildScenes()
    {
        _buildScenes = EditorBuildSettings.scenes
            .Select(s => s.path)
            .Where(p => !string.IsNullOrEmpty(p))
            .ToList();
    }

    // ── GUI ───────────────────────────────────────────────────────────────────
    private void OnGUI()
    {
        if (_config == null)
        {
            EditorGUILayout.HelpBox("Config asset not found.", MessageType.Warning);
            if (GUILayout.Button("Reload Config")) _config = LoadOrCreateConfig();
            return;
        }

        DrawToolbar();
        EditorGUILayout.Space(4);

        using (var scroll = new EditorGUILayout.ScrollViewScope(_scroll))
        {
            _scroll = scroll.scrollPosition;
            DrawRuleList();
            EditorGUILayout.Space(8);
            if (_selectedRuleIndex >= 0 && _selectedRuleIndex < _config.rules.Count)
                DrawRuleDetail(_config.rules[_selectedRuleIndex]);
        }

        if (GUI.changed)
            EditorUtility.SetDirty(_config);
    }

    // ── Toolbar ───────────────────────────────────────────────────────────────
    private void DrawToolbar()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            GUILayout.Label("Additive Scene Setup", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            GUI.backgroundColor = _config.autoLoadEnabled
                ? new Color(0.4f, 1f, 0.4f)
                : new Color(1f, 0.5f, 0.5f);
            bool newVal = GUILayout.Toggle(_config.autoLoadEnabled,
                _config.autoLoadEnabled ? "✔ Auto-Load ON" : "✘ Auto-Load OFF",
                EditorStyles.toolbarButton, GUILayout.Width(120));
            GUI.backgroundColor = Color.white;

            if (newVal != _config.autoLoadEnabled)
            {
                _config.autoLoadEnabled = newVal;
                EditorUtility.SetDirty(_config);
                AssetDatabase.SaveAssets();
            }

            if (GUILayout.Button("Save", EditorStyles.toolbarButton))
            {
                EditorUtility.SetDirty(_config);
                AssetDatabase.SaveAssets();
                Debug.Log("[AdditiveSceneSetup] Config saved.");
            }

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
                RefreshBuildScenes();
        }
    }

    // ── Rule list ─────────────────────────────────────────────────────────────
    private void DrawRuleList()
    {
        EditorGUILayout.LabelField("Main Scenes", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        for (int i = 0; i < _config.rules.Count; i++)
        {
            var rule = _config.rules[i];
            bool isSelected = (_selectedRuleIndex == i);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.backgroundColor = isSelected ? Color.cyan : Color.white;
                if (GUILayout.Button(System.IO.Path.GetFileNameWithoutExtension(rule.mainScenePath),
                    GUILayout.MinWidth(200)))
                {
                    _selectedRuleIndex = i;
                    GUI.FocusControl(null);
                }
                GUI.backgroundColor = Color.white;

                int enabledCount = rule.additiveScenes.Count(e => e.enabled);
                GUILayout.Label($"{enabledCount}/{rule.additiveScenes.Count} additive", GUILayout.Width(110));

                GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
                if (GUILayout.Button("✕", GUILayout.Width(24)))
                {
                    if (EditorUtility.DisplayDialog("Remove Rule",
                        $"Remove config for '{System.IO.Path.GetFileNameWithoutExtension(rule.mainScenePath)}'?",
                        "Remove", "Cancel"))
                    {
                        _config.rules.RemoveAt(i);
                        if (_selectedRuleIndex >= _config.rules.Count)
                            _selectedRuleIndex = _config.rules.Count - 1;
                        EditorUtility.SetDirty(_config);
                        break;
                    }
                }
                GUI.backgroundColor = Color.white;
            }
        }

        EditorGUILayout.EndVertical();

        // Add from build settings
        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Add rule for a Main Scene:", EditorStyles.miniBoldLabel);

        foreach (var scenePath in _buildScenes)
        {
            if (_config.rules.Any(r => r.mainScenePath == scenePath)) continue;

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(
                    System.IO.Path.GetFileNameWithoutExtension(scenePath), GUILayout.MinWidth(200));

                GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);
                if (GUILayout.Button("+ Add Rule", GUILayout.Width(90)))
                {
                    _config.GetOrCreateRule(scenePath);
                    _selectedRuleIndex = _config.rules.Count - 1;
                    EditorUtility.SetDirty(_config);
                }
                GUI.backgroundColor = Color.white;
            }
        }

        // Pick any scene not in build settings
        EditorGUILayout.Space(4);
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("Or pick any scene:", GUILayout.Width(110));
            var pickedObj = EditorGUILayout.ObjectField(null, typeof(SceneAsset), false) as SceneAsset;
            if (pickedObj != null)
            {
                string pickedPath = AssetDatabase.GetAssetPath(pickedObj);
                if (!_config.rules.Any(r => r.mainScenePath == pickedPath))
                {
                    _config.GetOrCreateRule(pickedPath);
                    _selectedRuleIndex = _config.rules.Count - 1;
                    EditorUtility.SetDirty(_config);
                }
            }
        }
    }

    // ── Rule detail ───────────────────────────────────────────────────────────
    private void DrawRuleDetail(AdditiveSceneConfig.SceneRule rule)
    {
        EditorGUILayout.LabelField(
            $"Additive Scenes for:  {System.IO.Path.GetFileNameWithoutExtension(rule.mainScenePath)}",
            EditorStyles.boldLabel);
        EditorGUILayout.LabelField(rule.mainScenePath, EditorStyles.miniLabel);
        EditorGUILayout.Space(4);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        if (rule.additiveScenes.Count == 0)
        {
            EditorGUILayout.LabelField("No additive scenes configured yet.", EditorStyles.centeredGreyMiniLabel);
        }
        else
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Load?", GUILayout.Width(44));
                GUILayout.Label("Scene",  GUILayout.MinWidth(200));
                GUILayout.Label("",       GUILayout.Width(28));
            }

            for (int i = 0; i < rule.additiveScenes.Count; i++)
            {
                var entry = rule.additiveScenes[i];
                using (new EditorGUILayout.HorizontalScope())
                {
                    entry.enabled = EditorGUILayout.Toggle(entry.enabled, GUILayout.Width(20));

                    var currentAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(entry.scenePath);
                    var newAsset = EditorGUILayout.ObjectField(
                        currentAsset, typeof(SceneAsset), false, GUILayout.MinWidth(200)) as SceneAsset;
                    if (newAsset != currentAsset)
                        entry.scenePath = newAsset != null ? AssetDatabase.GetAssetPath(newAsset) : "";

                    GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
                    if (GUILayout.Button("✕", GUILayout.Width(24)))
                    {
                        rule.additiveScenes.RemoveAt(i);
                        EditorUtility.SetDirty(_config);
                        break;
                    }
                    GUI.backgroundColor = Color.white;
                }
            }
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Add additive scene:", EditorStyles.miniBoldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            var picked = EditorGUILayout.ObjectField(null, typeof(SceneAsset), false) as SceneAsset;
            if (picked != null)
            {
                string path = AssetDatabase.GetAssetPath(picked);
                if (!rule.additiveScenes.Any(e => e.scenePath == path))
                {
                    rule.additiveScenes.Add(new AdditiveSceneConfig.AdditiveSceneEntry
                        { scenePath = path, enabled = true });
                    EditorUtility.SetDirty(_config);
                }
            }
        }

        EditorGUILayout.Space(6);
        GUI.backgroundColor = new Color(0.4f, 0.8f, 1f);
        if (GUILayout.Button("▶  Load Additive Scenes Now", GUILayout.Height(28)))
            LoadAdditiveScenesNow(rule);
        GUI.backgroundColor = Color.white;
    }

    private void LoadAdditiveScenesNow(AdditiveSceneConfig.SceneRule rule)
    {
        int loaded = 0;
        foreach (var entry in rule.additiveScenes)
        {
            if (!entry.enabled || string.IsNullOrEmpty(entry.scenePath)) continue;

            bool alreadyOpen = false;
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).path == entry.scenePath) { alreadyOpen = true; break; }

            if (!alreadyOpen)
            {
                EditorSceneManager.OpenScene(entry.scenePath, OpenSceneMode.Additive);
                loaded++;
                Debug.Log($"[AdditiveSceneSetup] Loaded additive: {entry.scenePath}");
            }
        }

        Debug.Log(loaded == 0
            ? "[AdditiveSceneSetup] All additive scenes already open."
            : $"[AdditiveSceneSetup] Loaded {loaded} additive scene(s).");
    }
}
#endif

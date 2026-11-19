using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject storing per-main-scene additive scene rules.
/// Lives outside the Editor folder so the asset can be loaded by editor scripts reliably.
/// </summary>
[CreateAssetMenu(fileName = "AdditiveSceneConfig", menuName = "Project Ocean/Additive Scene Config")]
public class AdditiveSceneConfig : ScriptableObject
{
    [Serializable]
    public class AdditiveSceneEntry
    {
        public string scenePath;
        public bool enabled = true;
    }

    [Serializable]
    public class SceneRule
    {
        public string mainScenePath;
        public List<AdditiveSceneEntry> additiveScenes = new List<AdditiveSceneEntry>();
    }

    public List<SceneRule> rules = new List<SceneRule>();
    public bool autoLoadEnabled = true;

    public SceneRule GetRuleForScene(string mainScenePath)
    {
        return rules.Find(r => r.mainScenePath == mainScenePath);
    }

    public SceneRule GetOrCreateRule(string mainScenePath)
    {
        var rule = GetRuleForScene(mainScenePath);
        if (rule == null)
        {
            rule = new SceneRule { mainScenePath = mainScenePath };
            rules.Add(rule);
        }
        return rule;
    }
}

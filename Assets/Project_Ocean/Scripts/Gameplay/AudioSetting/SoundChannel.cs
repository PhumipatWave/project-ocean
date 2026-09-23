using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// One channel of the AudioManager (Master / BGM / SFX). Every sound in the channel is a CHILD object
/// with its own AudioSource + clip; the child's NAME is the key the game plays it by.
/// To add a sound: add a child object with an AudioSource (clip, volume, loop, mixer Output = this
/// channel's group) and name it. To swap a sound: change the clip on that child. Nothing else references
/// the clip file directly.
/// </summary>
public class SoundChannel : MonoBehaviour
{
    private Dictionary<string, AudioSource> lookup;
    private readonly HashSet<string> warned = new HashSet<string>();

    public bool TryGet(string key, out AudioSource source)
    {
        if (lookup == null) Build();

        if (!string.IsNullOrEmpty(key) && lookup.TryGetValue(key, out source) && source.clip != null)
            return true;

        // Once per key — a missing sound must be visible, but not spam every call.
        if (warned.Add(key ?? "<null>"))
            Debug.LogWarning($"[SoundChannel] '{name}' has no sound named '{key}'.", this);

        source = null;
        return false;
    }

    private void Build()
    {
        lookup = new Dictionary<string, AudioSource>();

        foreach (var s in GetComponentsInChildren<AudioSource>(true))
        {
            if (s.transform == transform) continue; // the channel object itself is only a container
            if (!lookup.ContainsKey(s.name)) lookup.Add(s.name, s);
            else Debug.LogWarning($"[SoundChannel] '{name}' has two sounds named '{s.name}' — first one wins.", s);
        }
    }
}

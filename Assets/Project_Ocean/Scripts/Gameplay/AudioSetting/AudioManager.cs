using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public enum AudioChannel
{
    Master,
    BGM,
    SFX
}

/// <summary>What one scene sounds like. Keys are sound-object names (see SoundKeys). An EMPTY key stops
/// that layer, so a scene with all keys empty (the main menu) is silent.</summary>
[System.Serializable]
public class SceneSound
{
    public string scene;        // scene name, e.g. "Land_Map"
    public string bgm;          // music track (BGM channel)
    public string ambience;     // looping / long ambience layered under the music (BGM channel)
    public string enterSfx;     // optional short one-shot when the scene starts (SFX channel)
}

/// <summary>
/// The one audio owner. Lives on the persistent (GameManager) root, NOT inside a settings
/// panel — a copy inside an inactive panel never ran Awake, so Instance stayed null.
///
/// Layout:  AudioManager (this script)
///          ├─ Master  (SoundChannel → mixer group Master: UI / system sounds)
///          │   └─ one child object per sound: AudioSource + clip, named by its key
///          ├─ BGM     (SoundChannel → mixer group BGM: music + looping ambience)
///          └─ SFX     (SoundChannel → mixer group SFX: effects)
/// Play by key (see SoundKeys). Each sound keeps its own volume / pitch / loop on its AudioSource.
/// The Master GROUP scales everything; the Master CHANNEL is for sounds that belong to no other
/// channel (UI clicks, stingers), so only the master slider affects them.
///
/// Music follows the scene by itself: on every scene load the manager looks the scene up in its
/// "Scenes" table and switches music / ambience / enter sound. The manager persists, so without this
/// the previous scene's music would keep playing. A scene that is not in the table is left alone.
///
/// Volumes (0..1) are stored in PlayerPrefs and pushed to the mixer's exposed parameters.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioMixer mixer;
    [SerializeField] private SoundChannel master;
    [SerializeField] private SoundChannel bgm;
    [SerializeField] private SoundChannel sfx;

    [Tooltip("What each scene sounds like. All keys empty = silence (main menu). Scenes not listed are left alone.")]
    [SerializeField] private SceneSound[] scenes;

    // Exposed parameter names on the mixer (dB). Must match the mixer asset.
    private const string MasterParam = "MasterVolume";
    private const string BgmParam    = "BGMVolume";
    private const string SfxParam    = "SFXVolume";

    private const float MinDb = -80f;

    // The music track and the ambience loop that are currently playing (both live in the BGM channel).
    private AudioSource music;
    private AudioSource ambience;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // Persistence is handled by the parent GameManager root's DontDestroyOnLoad.
    }

    private void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    // AudioMixer.SetFloat is unreliable during Awake, so apply the saved volumes here.
    private void Start()
    {
        foreach (AudioChannel channel in System.Enum.GetValues(typeof(AudioChannel)))
            Apply(channel, GetVolume(channel));
    }

    // -------------------- Scene-driven sound -------------------- //

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Instance != this || scenes == null) return; // a duplicate being destroyed must not react

        foreach (var s in scenes)
        {
            if (s == null || s.scene != scene.name) continue;

            if (!string.IsNullOrEmpty(s.bgm)) PlayBGM(s.bgm); else StopBGM();
            if (!string.IsNullOrEmpty(s.ambience)) PlayAmbience(s.ambience); else StopAmbience();
            if (!string.IsNullOrEmpty(s.enterSfx)) PlaySFX(s.enterSfx);
            return;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // -------------------- Music & ambience (BGM channel) -------------------- //

    /// <summary>Plays a music track. Replaces the previous track; the same track already playing is
    /// left alone, so calling it on every scene load doesn't restart it.</summary>
    public void PlayBGM(string key) => PlayLoop(bgm, key, ref music);

    public void StopBGM()
    {
        if (music != null) music.Stop();
        music = null;
    }

    /// <summary>Plays a looping ambience layer on top of the music (its own slot).</summary>
    public void PlayAmbience(string key) => PlayLoop(bgm, key, ref ambience);

    public void StopAmbience()
    {
        if (ambience != null) ambience.Stop();
        ambience = null;
    }

    private static void PlayLoop(SoundChannel channel, string key, ref AudioSource slot)
    {
        if (channel == null || !channel.TryGet(key, out AudioSource src)) return;
        if (slot == src && src.isPlaying) return;

        if (slot != null && slot != src) slot.Stop();
        slot = src;
        src.Play();
    }

    // -------------------- One-shots (SFX / Master channels) -------------------- //

    /// <summary>Plays an effect. Overlapping plays of the same sound don't cut each other off.
    /// A pitch range (e.g. 1.1–1.3) picks a random pitch per play; leave it out to keep the pitch
    /// set on the sound's AudioSource.</summary>
    public void PlaySFX(string key, float pitchMin = 1f, float pitchMax = 1f)
    {
        if (sfx == null || !sfx.TryGet(key, out AudioSource src)) return;

        if (pitchMin != 1f || pitchMax != 1f)
            src.pitch = Random.Range(pitchMin, pitchMax);

        src.PlayOneShot(src.clip);
    }

    /// <summary>Cuts an effect off early (e.g. a search sound that ends when the search does).</summary>
    public void StopSFX(string key)
    {
        if (sfx != null && sfx.TryGet(key, out AudioSource src))
            src.Stop();
    }

    /// <summary>Plays a UI / system sound. Only the Master slider scales it.</summary>
    public void PlayMaster(string key)
    {
        if (master != null && master.TryGet(key, out AudioSource src))
            src.PlayOneShot(src.clip);
    }

    // -------------------- Slider preview -------------------- //

    private const float PreviewInterval = 0.2f; // min seconds between preview sounds while dragging
    private const float PreviewBgmHold  = 1.5f; // BGM preview stops this long after the last slider move

    private readonly float[] nextPreviewAt = new float[3]; // per channel, unscaled time
    private AudioSource previewBgm;
    private float previewBgmStopAt;

    /// <summary>Plays the sound that lets you hear a channel's level while you drag its slider:
    /// Master → OpenInventory, SFX → Hurt, BGM → LandTheme. One-shots are throttled so dragging
    /// doesn't machine-gun; the BGM preview keeps playing while you move and stops shortly after.
    /// Uses unscaled time, so it also works in the pause menu.</summary>
    public void PlayPreview(AudioChannel channel)
    {
        switch (channel)
        {
            case AudioChannel.BGM:
                PreviewBgm();
                break;
            case AudioChannel.SFX:
                if (PreviewReady(channel)) PlaySFX(SoundKeys.Hurt);
                break;
            default:
                if (PreviewReady(channel)) PlayMaster(SoundKeys.OpenInventory);
                break;
        }
    }

    private bool PreviewReady(AudioChannel channel)
    {
        if (Time.unscaledTime < nextPreviewAt[(int)channel]) return false;
        nextPreviewAt[(int)channel] = Time.unscaledTime + PreviewInterval;
        return true;
    }

    private void PreviewBgm()
    {
        previewBgmStopAt = Time.unscaledTime + PreviewBgmHold;
        if (previewBgm != null) return; // already previewing — the hold time was just extended

        if (bgm == null || !bgm.TryGet(SoundKeys.LandTheme, out AudioSource src)) return;
        if (src.isPlaying) return;      // it is already the scene's real music (Land) — leave it alone

        previewBgm = src;
        src.Play();
    }

    private void Update()
    {
        if (previewBgm == null || Time.unscaledTime < previewBgmStopAt) return;

        // If a scene load made it the real music in the meantime, keep it playing.
        if (previewBgm != music) previewBgm.Stop();
        previewBgm = null;
    }

    // -------------------- Volume -------------------- //

    /// <summary>Saved volume for a channel, 0..1 (1 = full).</summary>
    public float GetVolume(AudioChannel channel) => PlayerPrefs.GetFloat(PrefKey(channel), 1f);

    public void SetVolume(AudioChannel channel, float value)
    {
        value = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(PrefKey(channel), value); // flushed by Unity on quit
        Apply(channel, value);
    }

    private void Apply(AudioChannel channel, float value)
    {
        if (mixer == null) return;

        // Linear slider -> decibels. 0 must map to silence, not log10(0) = -infinity.
        float db = value > 0.0001f ? Mathf.Max(MinDb, Mathf.Log10(value) * 20f) : MinDb;
        mixer.SetFloat(ParamName(channel), db);
    }

    private static string PrefKey(AudioChannel channel) => "audio." + channel;

    private static string ParamName(AudioChannel channel)
    {
        switch (channel)
        {
            case AudioChannel.BGM: return BgmParam;
            case AudioChannel.SFX: return SfxParam;
            default:               return MasterParam;
        }
    }
}

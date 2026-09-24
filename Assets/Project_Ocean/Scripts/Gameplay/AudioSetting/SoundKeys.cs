/// <summary>
/// Keys for the sounds the game plays from code. A key is the NAME of a sound object under the
/// matching channel of the AudioManager (Master / BGM / SFX). Use these instead of raw strings so a
/// typo is a compile error, not a silent missing sound.
/// </summary>
public static class SoundKeys
{
    // ---- SFX ----
    public const string Hurt       = "Hurt";
    public const string RopeMove   = "RopeMove";
    public const string Shoot      = "Shoot";
    public const string Collect    = "Collect";   // picking up an item / enemy loot
    public const string DoorOpen   = "DoorOpen";
    public const string TrashClick = "TrashClick";

    // ---- Master (UI / system) ----
    public const string OpenInventory = "OpenInventory";

    // ---- BGM channel: music ----
    public const string LandTheme  = "LandTheme";
    public const string OceanTheme = "OceanTheme";

    // ---- BGM channel: ambience layer (its own slot, replaced when the scene changes) ----
    public const string LandAmbience  = "LandAmbience";
    public const string OceanAmbience = "OceanAmbience";

    // ---- scene "enter" one-shot (SFX) — short sounds only; long ones belong in the ambience layer ----
    public const string EnterOcean = "EnterOcean";
}

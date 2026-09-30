# Gameplay/AudioSetting

7 types · 226 LOC (non-blank) · depends on: [Core](Core.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class AudioChannel {
    <<enum>>
    Master
    BGM
    SFX
  }
  class SceneSound {
    +string scene
    +string bgm
    +string ambience
    +string enterSfx
  }
  class AudioManager {
    <<MB>>
    +PlayBGM(string) void
    +StopBGM() void
    +PlayAmbience(string) void
    +StopAmbience() void
    +PlaySFX(string, float, float) void
    +StopSFX(string) void
    +PlayMaster(string) void
    +PlayPreview(AudioChannel) void
    +GetVolume(AudioChannel) float
    +SetVolume(AudioChannel, float) void
  }
  class AudioSliderBinder {
    <<MB>>
  }
  class SoundChannel {
    <<MB>>
    +TryGet(string, out AudioSource) bool
  }
  class SoundKeys {
    <<static>>
    +string Hurt$
    +string RopeMove$
    +string Shoot$
    +string Collect$
    +string DoorOpen$
    +string TrashClick$
    +string OpenInventory$
    +string LandTheme$
    +string OceanTheme$
    +string LandAmbience$
    ... +2 more fields
  }
  class VolumeSliderUI {
    <<MB>>
  }
  class Singleton {
    <<Core>>
  }
  AudioManager --> SceneSound : scenes
  AudioManager --> SoundChannel : master
  AudioSliderBinder --> AudioChannel : channel
  Singleton <|-- AudioManager
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| AudioChannel | enum | `AudioManager.cs:8` | 6 |
| AudioManager | MB | `AudioManager.cs:28` | 135 |
| AudioSliderBinder | MB | `AudioSliderBinder.cs:9` | 19 |
| SceneSound | class | `AudioManager.cs:18` | 7 |
| SoundChannel | MB | `SoundChannel.cs:8` | 25 |
| SoundKeys | static | `SoundKeys.cs:6` | 15 |
| VolumeSliderUI | MB | `VolumeSliderUI.cs:8` | 19 |

# Core/Datas/Upgrades

3 types · 31 LOC (non-blank) · depends on: [Gameplay/Projectiles](Gameplay_Projectiles.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class UpgradeType {
    <<enum>>
    Inventory
    Rope
    NormalBullet
  }
  class UpgradeUnlock {
    +string unlockName
    +int unlockLevel
  }
  class UpgradeData {
    <<SO>>
    +string upgradeName
    +string description
    +UpgradeType type
    +int maxLevel
    +int prices
    +float value
    +List~UpgradeUnlock~ unlocks
  }
  UpgradeData --> UpgradeType : type
  UpgradeData --> UpgradeUnlock : unlocks
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| UpgradeData | SO | `UpgradeData.cs:25` | 18 |
| UpgradeType | enum | `UpgradeData.cs:7` | 6 |
| UpgradeUnlock | class | `UpgradeData.cs:15` | 7 |

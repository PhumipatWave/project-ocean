# Core/Data

3 types · 19 LOC (non-blank) · depends on: none

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class AmmoType {
    <<enum>>
    Normal
    Tranquilizer
    GrapplingHook
    Granade
  }
  class BulletData {
    <<SO>>
    +float attackDamage
    +float qualityMultiplier
    +float bulletSpeed
    +float bulletMass
  }
  class RopeStructureData {
    <<SO>>
    +float maxRopeLength
    +int ropeSegment
  }
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| AmmoType | enum | `BulletData.cs:5` | 7 |
| BulletData | SO | `BulletData.cs:14` | 7 |
| RopeStructureData | SO | `RopeStructureData.cs:6` | 5 |

# Core/Pool

2 types · 71 LOC (non-blank) · depends on: [Core](Core.md) ×1

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class IPoolable {
    <<interface>>
    +OnGetFromPool()* void
    +OnReturnToPool()* void
  }
  class ObjectPoolManager {
    <<MB>>
    #bool DestroyGameObjectOnDuplicate
    +Prewarm(GameObject, int) void
    +Get(GameObject, Vector3, Quaternion) GameObject
    +Return(GameObject) void
  }
  class Singleton {
    <<Core>>
  }
  Singleton <|-- ObjectPoolManager
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| IPoolable | interface | `IPoolable.cs:5` | 5 |
| ObjectPoolManager | MB | `ObjectPoolManager.cs:7` | 66 |

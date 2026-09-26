# Core/Pool

2 types · 76 LOC (non-blank) · depends on: none

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
    +ObjectPoolManager Instance$
    +Prewarm(GameObject, int) void
    +Get(GameObject, Vector3, Quaternion) GameObject
    +Return(GameObject) void
  }
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| IPoolable | interface | `IPoolable.cs:5` | 5 |
| ObjectPoolManager | MB | `ObjectPoolManager.cs:7` | 71 |

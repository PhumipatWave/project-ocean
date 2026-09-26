# Core/Datas/ScriptableDatas/Characters

2 types · 27 LOC (non-blank) · depends on: none

[← index](../README.md)

```mermaid
classDiagram
  direction LR
  class CharacterData {
    <<SO>>
    +string id
    +string displayName
    +Sprite portrait
  }
  class CharacterDatabase {
    <<SO>>
    +Get(string) CharacterData
  }
  CharacterDatabase --> CharacterData : characters
```

## Files

| Type | Kind | File | LOC |
|---|---|---|---|
| CharacterData | SO | `CharacterData.cs:11` | 8 |
| CharacterDatabase | SO | `CharacterDatabase.cs:9` | 19 |

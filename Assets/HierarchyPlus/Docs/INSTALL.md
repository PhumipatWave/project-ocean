# HierarchyPlus — Installation Guide

**Compatibility:** Unity 2022 LTS · Unity 2023 LTS · Unity 6
**Type:** Editor-only extension (no runtime overhead in builds)

---

## 1. Copy the Package

Drop the `HierarchyPlus` folder into your project's `Assets` directory:

```
Assets/
└── HierarchyPlus/
    ├── Editor/
    └── Runtime/
```

Unity will detect the scripts and compile automatically.

---

## 2. Install Assembly Definitions

Open the menu:

```
HierarchyPlus → Install Assembly Definitions
```

This writes two `.asmdef` files that isolate HierarchyPlus from the rest of your
project. Unity will recompile once. The `AsmdefInstaller.cs` helper can be
deleted afterward.

---

## 3. Verify

Open the Hierarchy window. The Console should show:

```
[HierarchyPlus] Initialized (Phase 11 — Performance Optimization).
```

---

## 4. Tag Your GameObjects

### Category Tag
Attach **HierarchyPlus/Category Tag** to assign a category.

| Category    | Color  | Icon |
|-------------|--------|------|
| Player      | Blue   | 🎮   |
| Enemy       | Red    | 👾   |
| UI          | White  | 🖼   |
| Manager     | Gray   | ⚙    |
| Environment | Green  | 🌿   |
| Audio       | Orange | 🔊   |
| Entity      | Yellow | 🧩   |

### Development Status Tag
Attach **HierarchyPlus/Development Status Tag** to track workflow.

| Field               | Description                              |
|---------------------|------------------------------------------|
| Status              | Current state — drives badge color       |
| Assigned Developer  | Name shown in hover tooltip              |
| Notes               | Free-text shown in hover tooltip         |

| Status         | Badge | Color  |
|----------------|-------|--------|
| New            | NEW   | Blue   |
| WorkInProgress | WIP   | Yellow |
| Complete       | DONE  | Green  |
| Problem        | BUG   | Red    |
| Review         | REV   | Purple |
| Deprecated     | DEP   | Gray   |

---

## 5. Component Minimap

Icons appear automatically on the right side of each row.

| Action            | Result                                        |
|-------------------|-----------------------------------------------|
| Hover icon        | Opens a mini Inspector popup                  |
| Alt + Click icon  | Opens the script in your external IDE         |

---

## 6. Unity Version Notes

| Version         | Status                                                    |
|-----------------|-----------------------------------------------------------|
| Unity 2022 LTS  | Fully supported                                           |
| Unity 2023 LTS  | Fully supported                                           |
| Unity 6         | Supported — CS0618 pragma suppresses deprecation warning  |

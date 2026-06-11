using System.IO;
using UnityEditor;
using UnityEngine;

namespace HierarchyPlus.Editor
{
    /// <summary>
    /// DocsInstaller — One-shot utility that writes INSTALL.md and
    /// FOLDER_STRUCTURE.md into Assets/HierarchyPlus/Docs/.
    ///
    /// Run once via:  HierarchyPlus → Install Documentation
    ///
    /// This script can be deleted after use.
    /// </summary>
    public static class DocsInstaller
    {
        [MenuItem("HierarchyPlus/Install Documentation")]
        public static void Install()
        {
            string docsDir = Path.Combine(Application.dataPath, "HierarchyPlus", "Docs");
            Directory.CreateDirectory(docsDir);

            WriteIfAbsent(Path.Combine(docsDir, "INSTALL.md"),           InstallMd);
            WriteIfAbsent(Path.Combine(docsDir, "FOLDER_STRUCTURE.md"),  FolderStructureMd);

            AssetDatabase.Refresh();
            Debug.Log("[HierarchyPlus] Documentation files written to Assets/HierarchyPlus/Docs/");
        }

        private static void WriteIfAbsent(string path, string content)
        {
            if (File.Exists(path)) { Debug.Log($"[HierarchyPlus] Already exists — skipped: {path}"); return; }
            File.WriteAllText(path, content);
            Debug.Log($"[HierarchyPlus] Created: {path}");
        }

        // ── INSTALL.md ────────────────────────────────────────────────────────

        private const string InstallMd = @"# HierarchyPlus — Installation Guide

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
";

        // ── FOLDER_STRUCTURE.md ──────────────────────────────────────────────

        private const string FolderStructureMd = @"# HierarchyPlus — Folder Structure

```
Assets/HierarchyPlus/
│
├── Docs/
│   ├── INSTALL.md                      Installation guide
│   └── FOLDER_STRUCTURE.md             This file
│
├── Runtime/                            ── HierarchyPlus.Runtime assembly ──
│   ├── HierarchyPlus.Runtime.asmdef    Assembly Definition
│   ├── HierarchyCategory.cs            Enum: category values
│   ├── HierarchyCategoryTag.cs         MonoBehaviour data component
│   ├── DevelopmentStatus.cs            Enum: workflow status values
│   └── DevelopmentStatusTag.cs         MonoBehaviour data component
│
└── Editor/                             ── HierarchyPlus.Editor assembly ──
    ├── HierarchyPlus.Editor.asmdef     Assembly Definition (Editor-only)
    ├── HierarchyPlusCore.cs            [InitializeOnLoad] entry point
    ├── HierarchyPlusDrawer.cs          Per-row GUI orchestrator
    ├── HierarchyDataCache.cs           Tag component lookup cache (Phase 11)
    ├── CategoryColorMap.cs             Category → color + emoji mapping
    ├── CategoryIconMap.cs              Category → Texture2D icon mapping
    ├── StatusColorMap.cs               Status → color + label + tooltip
    ├── ComponentMinimap.cs             Right-side component icon strip
    ├── ComponentInspectorPopup.cs      Hover mini-Inspector popup
    ├── ComponentTooltipWindow.cs       Hover name/namespace/assembly tooltip
    ├── MissingScriptDetector.cs        Missing-script ⚠ warning icon
    ├── ScriptNavigator.cs              Alt+Click → open script in IDE
    ├── AsmdefInstaller.cs              One-shot asmdef writer (deletable)
    └── DocsInstaller.cs                One-shot docs writer (deletable)
```

---

## Architecture

```
[InitializeOnLoad]
HierarchyPlusCore
    ├── hierarchyWindowItemOnGUI → HierarchyPlusDrawer
    │                                 ├── HierarchyDataCache
    │                                 ├── CategoryColorMap / CategoryIconMap
    │                                 ├── StatusColorMap
    │                                 ├── MissingScriptDetector
    │                                 └── ComponentMinimap
    │                                       ├── ComponentInspectorPopup
    │                                       └── ScriptNavigator
    ├── hierarchyWindowItemOnGUI → FlushHoverState
    └── hierarchyChanged → InvalidateCache × 3
```
";
    }
}

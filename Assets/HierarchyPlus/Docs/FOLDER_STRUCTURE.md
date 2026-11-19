# HierarchyPlus — Folder Structure

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

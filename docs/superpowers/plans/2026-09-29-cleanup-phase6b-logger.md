# Cleanup Phase 6b: Editor/Dev-only Info Logging — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Info logs (`Debug.Log`) cost nothing in release builds: no call, and no string building (most are `$"..."` interpolations, some per-frame). Warnings and errors stay as they are.

**Survey (2026-09-29):** 144 `Debug.Log*` calls in `Assets/Project_Ocean/Scripts`: 56 `Debug.Log`, 56 `Debug.LogWarning`, 32 `Debug.LogError`. The roadmap's "137" counted all three kinds.

**Decision: only `Debug.Log` changes.** `LogWarning`/`LogError` stay in release on purpose: they land in the player's `Player.log`, which is how a save failure or a missing item id on a Steam player's machine gets diagnosed. Stripping them would hide exactly the problems that matter.

**Architecture:** one static class in `Game.Core` (every layer references it):
```csharp
// Assets/Project_Ocean/Scripts/Core/GameLog.cs
using System.Diagnostics;

namespace Game.Core
{
    /// <summary>Info logging compiled out of release builds (editor and development builds only).</summary>
    public static class GameLog
    {
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Info(object message, UnityEngine.Object context = null) =>
            UnityEngine.Debug.Log(message, context);
    }
}
```
`[Conditional]` removes the call *and* the evaluation of its arguments at every call site, so an interpolated string isn't built either. Then `Debug.Log(` → `GameLog.Info(` in the live scripts. `Deprecated/` is skipped: it's editor-only and unused.

## Global Constraints

- No behaviour change in the editor or in development builds (same messages, same context objects).
- Mechanical replace only on `Debug.Log(` (not `LogWarning`/`LogError`); files keep their encoding, BOM and line endings.
- Claude does not run `git commit`, `git push`, `git add`, `git stash`, `git restore`, `git checkout` or `git mv`.
- Unity MCP: `set_active_instance` = `project-ocean@7db8e111`.

## Risks

- **A log with side effects in its argument** (e.g. `Debug.Log(x++)`) would stop running in release. Check every replaced call's arguments for `++`, `--`, `=` or method calls with side effects before replacing; list any, and leave those as `Debug.Log`.
- **Name clash:** a file with its own `GameLog` or a `using` alias. Checked by compile.
- **Files without `using Game.Core;`** (Core files already sit in `namespace Game.Core`; Gameplay/UI files got the `using` in Phase 4). Checked by compile.

---

### Task 1: Logger and test

- [ ] **Step 1:** Test first, `Tests/EditMode/GameLogTests.cs`:
  - `Info_LogsInEditor`: `LogAssert.Expect(LogType.Log, "hello")`, then `GameLog.Info("hello")`.
  - `Info_IsConditionalOnEditorAndDevBuild`: reflection finds both `ConditionalAttribute`s (`UNITY_EDITOR`, `DEVELOPMENT_BUILD`) on `GameLog.Info`.
  Red: `CS0103`.
- [ ] **Step 2:** Create `Core/GameLog.cs` as above. Compile, tests green.

### Task 2: Replace

- [ ] **Step 1:** Side-effect check on all 56 call arguments (see Risks).
- [ ] **Step 2:** Replace `Debug.Log(` → `GameLog.Info(` in every live file (not `Deprecated/`), preserving encoding/BOM/EOL. Add `using Game.Core;` only where the compile says it's missing.
- [ ] **Step 3:** Compile 0 errors; tests all pass; `grep -rn "Debug\.Log(" --include=*.cs Assets/Project_Ocean/Scripts | grep -v Deprecated/` → no output.

### Task 3: Prove the stripping

- [ ] **Step 1:** Player-script compile, release (`ScriptCompilationOptions.None`), and development (`ScriptCompilationOptions.DevelopmentBuild`). Pick a unique info-log string (e.g. `"[QuestManager] Initialized"`). Expected: present in the development `Game.Gameplay.dll`, absent from the release one. A `LogError` string (e.g. `"[SaveBridge] SaveGame failed"`) is present in both.
- [ ] **Step 2:** Play smoke in the editor: load slot 0, and the console still shows the usual info lines (e.g. `[QuestManager] Initialized`, `[SaveBridge] Game saved`). Restore saves.
- [ ] **Step 3:** `findings.md` Phase 6: mark the logging item done with the corrected counts. UML regen + tests, `graphify update .`. Hand-off, suggested commit "chore: route info logs through editor/dev-only GameLog". Do not commit.

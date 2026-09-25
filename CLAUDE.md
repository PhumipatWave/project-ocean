# UNITY PROJECT AI GUIDE

## Project Overview

Game:

* Single-player Fishing RPG
* Underwater exploration
* Harpoon combat
* Inventory system
* Shop system
* Upgrade system

Engine:

* Unity 6 LTS

Platform:

* PC (Steam)

---

## Scene Structure

Scenes live in Assets/Project_Ocean/Levels/Scenes/.

Bootstrap

* Startup scene
* Its manager hierarchy is an unused duplicate — do not edit managers here

AS_PlayerHUD

* Persistent managers that actually run (GameManager root, DontDestroyOnLoad)
* Player HUD, NotificationTextUI, ConfirmationDialog

MainMenuScene

* Main menu, save slot UI (SaveMenuUI)
* Settings

IntroScene

* Intro

Land_Map

* NPCs
* Shops
* Quests
* Upgrade stations

Ocean_Map

* Fishing gameplay
* Fish AI
* Combat
* Loot

Use additive scene loading.

---

## Existing Managers

Global (persistent):

* GameManager
* SaveBridge + SaveSystem (save/load)
* AudioManager
* DayManager
* QuestManager
* CutsceneManager
* LoadingManager
* InventoryManager
* UIManager
* KeyBindManager
* ObjectPoolManager
* CameraShakeManager
* NotificationManager (static event bus)

Land:

* ShopManager
* UpgradeManager
* NPCManager

Avoid creating duplicate managers.

---

## Architecture Rules

Prefer:

* Composition
* Events
* Interfaces
* ScriptableObjects

Avoid:

* God classes
* Hard-coded references
* FindObjectOfType

Keep classes focused on a single responsibility.

---

## Comment Rules

* Write short comments: 1 line, 2 max.
* Comment WHY (non-obvious reason, bug it prevents), never WHAT the code does.
* No comment on code whose name already explains it.
* No history in comments ("used to", "earlier version", "Bug 1:") — git has that.
* XML <summary> only on public API, 1 sentence.

---

## Save System Rules

Use SaveBridge (SaveSystem handles the files).

Do not:

* Save scene references
* Save MonoBehaviours

Save only runtime data.

---

## UI Rules

Use prefab-based UI.

Separate:

* UI View
* UI Logic

Avoid gameplay logic inside UI scripts.

---

## Git Rules

Do not modify unrelated files.

Show affected files before making changes.

Keep changes small and focused.

---

## AI Workflow

Phase 1: Architect

* Analyze
* Create implementation plan
* Identify risks

Phase 2: Programmer

* Implement approved plan only

Phase 3: QA

* Review bugs
* Review edge cases

Phase 4: Refactor

* Improve maintainability
* Keep behavior unchanged

Never skip phases.

Always provide a plan before coding.

## graphify

This project has a knowledge graph at graphify-out/ with god nodes, community structure, and cross-file relationships.

Rules:
- For codebase questions, first run `graphify query "<question>"` when graphify-out/graph.json exists. Use `graphify path "<A>" "<B>"` for relationships and `graphify explain "<concept>"` for focused concepts. These return a scoped subgraph, usually much smaller than GRAPH_REPORT.md or raw grep output.
- If graphify-out/wiki/index.md exists, use it for broad navigation instead of raw source browsing.
- Read graphify-out/GRAPH_REPORT.md only for broad architecture review or when query/path/explain do not surface enough context.
- After modifying code, run `graphify update .` to keep the graph current (AST-only, no API cost).

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

Bootstrap

* Persistent systems
* SaveSystem
* AudioSystem
* SceneLoader

Land

* NPCs
* Shops
* Quests
* Upgrade stations

Ocean

* Fishing gameplay
* Fish AI
* Combat
* Loot

Menu

* Main menu
* Settings

Use additive scene loading.

---

## Existing Managers

Global:

* GameManager
* SaveManager
* AudioManager
* DayManager

Land:

* ShopManager
* UpgradeManager
* QuestManager

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

## Save System Rules

Use SaveManager.

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

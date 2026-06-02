# UNITY PROJECT AI DEVELOPMENT GUIDE

## Project Overview

Project Type:

* Single Player Casual Fishing Action Game
* Underwater exploration
* Harpoon combat
* Fish AI behavior
* Inventory and economy systems

Engine:

* Unity 6 LTS

Programming Style:

* Modular OOP architecture
* Composition over inheritance
* Data-driven systems
* Scalable for future multiplayer support

Target Platform:

* PC
* Steam

---

# Core Architecture Rules

## Always Prefer Composition

DO:

* Use components for behavior separation
* Create reusable systems
* Use interfaces for interaction systems

AVOID:

* Deep inheritance chains
* Giant manager classes
* Monolithic player scripts

Bad:
PlayerController : Character : Entity : MonoBehaviour

Good:
PlayerMovement
PlayerCombat
PlayerInventory
PlayerInteraction

---

# SOLID Principles

All generated code must follow:

* Single Responsibility Principle
* Open Closed Principle
* Dependency Inversion where applicable

Each class should have ONE clear responsibility.

---

# Folder Structure

Assets/Project_Ocean/Art/Scripts/Core/
Assets/Project_Ocean/Art/Scripts/Gameplay/
Assets/Project_Ocean/Art/Scripts/Input/
Assets/Project_Ocean/Art/Scripts/UI/

Never place scripts outside Scripts folder.

---

# Namespace Rules

Use:
ProjectName.System.Feature

Examples:
FishingGame.AI
FishingGame.Inventory
FishingGame.UI

Avoid global namespace.

---

# MonoBehaviour Rules

MonoBehaviours should:

* Handle Unity lifecycle only
* Connect systems together
* Avoid business logic when possible

Business logic should exist in plain C# classes.

---

# Update Loop Rules

Avoid Update() unless necessary.

Prefer:

* Events
* Coroutines
* Timers
* State machines

If Update is required:

* Cache references
* Avoid allocations
* Avoid GetComponent calls

---

# GetComponent Rules

NEVER repeatedly call GetComponent in Update.

Cache references in Awake.

Bad:
void Update()
{
GetComponent().velocity = move;
}

Good:
private Rigidbody rb;

void Awake()
{
rb = GetComponent();
}

---

# ScriptableObject Rules

Use ScriptableObjects for:

* Item data
* Fish data
* Dialogue data
* Enemy stats
* Audio collections
* Balance tuning

Never store runtime mutable state in ScriptableObjects.

---

# Inventory System Rules

Inventory architecture:

* Runtime inventory data separate from item definitions
* Item definitions use ScriptableObject
* Runtime inventory uses serializable data models

Preferred:
ItemDataSO
InventoryItemRuntime
InventoryManager

Avoid:
Putting runtime quantity directly inside ScriptableObject.

---

# Save System Rules

Save system must:

* Use JSON serialization
* Save runtime data only
* Never save scene object references
* Use unique IDs for persistent world objects

Save architecture:
SaveData
PlayerData
InventorySaveData
WorldStateData

---

# Scene Management Rules

Use additive scene loading where possible.

Persistent systems:

* Audio
* SaveSystem
* SceneLoader

Gameplay systems should be scene-local unless persistence is required.

Avoid large DontDestroyOnLoad usage.

---

# Singleton Rules

Singletons allowed ONLY for:

* GameBootstrap
* SaveSystem
* AudioSystem
* SceneLoader

Gameplay systems should avoid singleton dependency.

Use dependency injection or serialized references where possible.

---

# AI Architecture Rules

Enemy AI uses:

* Finite State Machine
* Behavior Trees for complex logic

Each AI must separate:

* Detection
* Movement
* Combat
* Decision Making

Preferred Structure:
FishBrain
FishStateMachine
FishSensor
FishMovement
FishCombat

Avoid giant EnemyController scripts.

---

# Event System Rules

Prefer event-driven architecture.

Use:

* C# events
* UnityEvent only for designer workflow

Avoid:
FindObjectOfType
Hard object references between systems

---

# UI Rules

UI must:

* Use prefab-based workflow
* Separate logic from visuals
* Avoid direct gameplay logic in UI scripts

Use:
View + Presenter pattern where applicable

---

# Performance Rules

Target:

* 60 FPS minimum

Avoid:

* LINQ in gameplay loops
* Runtime Instantiate spam
* GC allocations during combat

Use:

* Object pooling
* Cached references
* Addressables

---

# Addressables Rules

Use Addressables for:

* Large assets
* Audio
* VFX
* UI popups
* Fish prefabs

Avoid Resources folder unless prototype stage.

---

# Physics Rules

Use:

* FixedUpdate only for physics movement
* Rigidbody movement APIs

Avoid:

* Transform movement for physics objects

---

# Animation Rules

Animation logic should use:

* Animator parameters
* Animation events sparingly

Avoid gameplay-critical logic inside animation events.

---

# Git Workflow

Branches:

* main
* develop
* feature/*

Never commit:

* Library/
* Temp/
* Logs/

Use:

* Git LFS for large binary assets

---

# Code Style Rules

Private fields:
[SerializeField] private

Naming:

* PascalCase for public members
* camelCase for private fields
* Interfaces start with I

Examples:
IInteractable
playerInventory
MoveSpeed

---

# Logging Rules

Use conditional logging tools.

Avoid excessive Debug.Log in release builds.

Create:
ProjectLogger utility.

---

# Testing Checklist

Before completing any feature:

* Null reference safety
* Scene reload safety
* Save/load validation
* Performance check
* Mobile/low-end validation if applicable

---

# Forbidden Patterns

NEVER:

* Use FindObjectOfType in gameplay loops
* Use static mutable gameplay state carelessly
* Create god managers
* Create 2000+ line MonoBehaviours
* Hardcode item IDs in logic
* Use magic numbers

---

# Preferred Patterns

PREFER:

* Interfaces
* Events
* Data-driven systems
* Modular prefabs
* State machines
* Composition
* ScriptableObjects

---

# AI Assistant Behavior Rules

When generating code:

* Prioritize scalability
* Explain architecture decisions
* Avoid quick hacks
* Generate production-ready code
* Follow existing project structure
* Avoid unnecessary dependencies
* Keep scripts focused and modular

When unsure:

* Ask before introducing architectural changes

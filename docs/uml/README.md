# Project Ocean — UML

Source: `Assets/Project_Ocean/Scripts` · 213 files · 262 types · 19724 non-blank lines · 127 MonoBehaviours · 13 ScriptableObjects · 15 interfaces · 30 enums

Regenerate after any script change: `python tools/uml/gen_uml.py` (self-test: `python tools/uml/test_gen_uml.py`).

## Pages

- [Overview — layers & modules](00-overview.md)
- [Inheritance & interfaces](01-inheritance.md)
- [Event flow](02-events.md)
- [Metrics](metrics.md)
- [Findings & refactor analysis](findings.md) (hand-written)

## Modules

**Core**: [Core](modules/Core.md) · [Core/Datas](modules/Core_Datas.md) · [Core/Datas/Items](modules/Core_Datas_Items.md) · [Core/Datas/ScriptableDatas/Characters](modules/Core_Datas_ScriptableDatas_Characters.md) · [Core/Datas/Upgrades](modules/Core_Datas_Upgrades.md) · [Core/Events](modules/Core_Events.md) · [Core/Inputs](modules/Core_Inputs.md) · [Core/Inputs/KeyBind](modules/Core_Inputs_KeyBind.md) · [Core/Inputs/KeyBind/MoveDirection](modules/Core_Inputs_KeyBind_MoveDirection.md) · [Core/Inputs/KeyBind/SaveKeyBind](modules/Core_Inputs_KeyBind_SaveKeyBind.md) · [Core/Inputs/KeyBind/UIKeyBind](modules/Core_Inputs_KeyBind_UIKeyBind.md) · [Core/Pool](modules/Core_Pool.md) · [Core/Save](modules/Core_Save.md) · [Core/Save/DayReset](modules/Core_Save_DayReset.md) · [Core/Scenes](modules/Core_Scenes.md) · [Core/VideoManager](modules/Core_VideoManager.md)

**Gameplay**: [Gameplay/AudioSetting](modules/Gameplay_AudioSetting.md) · [Gameplay/Contracts](modules/Gameplay_Contracts.md) · [Gameplay/Cutscenes](modules/Gameplay_Cutscenes.md) · [Gameplay/Entities](modules/Gameplay_Entities.md) · [Gameplay/Entities/Enemies](modules/Gameplay_Entities_Enemies.md) · [Gameplay/Entities/Enemies/Behaviours](modules/Gameplay_Entities_Enemies_Behaviours.md) · [Gameplay/Entities/Enemies/Brain](modules/Gameplay_Entities_Enemies_Brain.md) · [Gameplay/Entities/Enemies/Brain/FSM](modules/Gameplay_Entities_Enemies_Brain_FSM.md) · [Gameplay/Entities/Enemies/Brain/FSM/States](modules/Gameplay_Entities_Enemies_Brain_FSM_States.md) · [Gameplay/Entities/Enemies/Brain/Interfaces](modules/Gameplay_Entities_Enemies_Brain_Interfaces.md) · [Gameplay/Entities/Enemies/Brain/LinearPatrol](modules/Gameplay_Entities_Enemies_Brain_LinearPatrol.md) · [Gameplay/Entities/Enemies/NavAgents](modules/Gameplay_Entities_Enemies_NavAgents.md) · [Gameplay/Entities/Enemies/NavAgents/Core](modules/Gameplay_Entities_Enemies_NavAgents_Core.md) · [Gameplay/Entities/Enemies/PartBreak](modules/Gameplay_Entities_Enemies_PartBreak.md) · [Gameplay/Entities/NPCs](modules/Gameplay_Entities_NPCs.md) · [Gameplay/Entities/Players](modules/Gameplay_Entities_Players.md) · [Gameplay/Entities/Players/OxygenTube](modules/Gameplay_Entities_Players_OxygenTube.md) · [Gameplay/Environments](modules/Gameplay_Environments.md) · [Gameplay/Game](modules/Gameplay_Game.md) · [Gameplay/InteractObjects](modules/Gameplay_InteractObjects.md) · [Gameplay/Interaction](modules/Gameplay_Interaction.md) · [Gameplay/Inventory](modules/Gameplay_Inventory.md) · [Gameplay/MainCameras](modules/Gameplay_MainCameras.md) · [Gameplay/Projectiles](modules/Gameplay_Projectiles.md) · [Gameplay/QuestSystems/Quest](modules/Gameplay_QuestSystems_Quest.md) · [Gameplay/RopeSystems](modules/Gameplay_RopeSystems.md) · [Gameplay/Save](modules/Gameplay_Save.md) · [Gameplay/Services](modules/Gameplay_Services.md) · [Gameplay/Spawning](modules/Gameplay_Spawning.md)

**UI**: [UI](modules/UI.md) · [UI/Dialogue](modules/UI_Dialogue.md) · [UI/Interactions](modules/UI_Interactions.md) · [UI/Inventory](modules/UI_Inventory.md) · [UI/Inventory/InventoryBin](modules/UI_Inventory_InventoryBin.md) · [UI/Inventory/InventoryBuy](modules/UI_Inventory_InventoryBuy.md) · [UI/Inventory/InventoryPlayer](modules/UI_Inventory_InventoryPlayer.md) · [UI/Loading](modules/UI_Loading.md) · [UI/Loading/LoadingTextAnimation](modules/UI_Loading_LoadingTextAnimation.md) · [UI/Loading/LoadingVisualController](modules/UI_Loading_LoadingVisualController.md) · [UI/Money](modules/UI_Money.md) · [UI/Notifications](modules/UI_Notifications.md) · [UI/Ocean](modules/UI_Ocean.md) · [UI/Quest](modules/UI_Quest.md) · [UI/QuestCompleteUI](modules/UI_QuestCompleteUI.md) · [UI/Save](modules/UI_Save.md) · [UI/Shop](modules/UI_Shop.md) · [UI/UIManager](modules/UI_UIManager.md) · [UI/UIMenuSetting](modules/UI_UIMenuSetting.md) · [UI/Upgrades](modules/UI_Upgrades.md) · [UI/VFX](modules/UI_VFX.md)

**Editor**: [Editor](modules/Editor.md)

## Legend

- `<|--` inherits · `<|..` implements · `-->` holds a field/property of that type (label = member)
- `+` public · `#` protected · `$` static · `*` abstract · `~T~` = generic `<T>` · `Array` = `[]`
- Stereotypes: `MB` MonoBehaviour · `SO` ScriptableObject · `static` · `abstract` · `interface` · `enum`
- Class boxes show public/protected API only, capped at 10 fields and 12 methods. External types show their module as stereotype.

## Limits

Static text parser, not the compiler. Links made only via `GetComponent<T>`, inspector wiring, prefabs, `SendMessage` or strings are invisible; identical names can overcount. `InputSystem_Actions.cs` (generated) and `Scripts/Deprecated/` are excluded.

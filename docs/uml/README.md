# Project Ocean — UML

Source: `Assets/Project_Ocean/Scripts` · 204 files · 257 types · 19448 non-blank lines · 130 MonoBehaviours · 13 ScriptableObjects · 9 interfaces · 30 enums

Regenerate after any script change: `python tools/uml/gen_uml.py` (self-test: `python tools/uml/test_gen_uml.py`).

## Pages

- [Overview — layers & modules](00-overview.md)
- [Inheritance & interfaces](01-inheritance.md)
- [Event flow](02-events.md)
- [Metrics](metrics.md)
- [Findings & refactor analysis](findings.md) (hand-written)

## Modules

**Core**: [Core](modules/Core.md) · [Core/Datas](modules/Core_Datas.md) · [Core/Datas/ScriptableDatas/Characters](modules/Core_Datas_ScriptableDatas_Characters.md) · [Core/Datas/ScriptableDatas/Quests/QuestBoard](modules/Core_Datas_ScriptableDatas_Quests_QuestBoard.md) · [Core/Inputs](modules/Core_Inputs.md) · [Core/Inputs/KeyBind](modules/Core_Inputs_KeyBind.md) · [Core/Inputs/KeyBind/MovePlayerDirsction](modules/Core_Inputs_KeyBind_MovePlayerDirsction.md) · [Core/Inputs/KeyBind/SaveKeyBind](modules/Core_Inputs_KeyBind_SaveKeyBind.md) · [Core/Inputs/KeyBind/UIKeyBind](modules/Core_Inputs_KeyBind_UIKeyBind.md) · [Core/Interfaces](modules/Core_Interfaces.md) · [Core/Pool](modules/Core_Pool.md) · [Core/Save Game](modules/Core_Save_Game.md) · [Core/Save Game/DayAndResetEnemys](modules/Core_Save_Game_DayAndResetEnemys.md) · [Core/Services](modules/Core_Services.md) · [Core/VideoManager](modules/Core_VideoManager.md)

**Gameplay**: [Gameplay/AudioSetting](modules/Gameplay_AudioSetting.md) · [Gameplay/Cutscenes](modules/Gameplay_Cutscenes.md) · [Gameplay/Entitys](modules/Gameplay_Entitys.md) · [Gameplay/Entitys/Enemys](modules/Gameplay_Entitys_Enemys.md) · [Gameplay/Entitys/Enemys/Beharviours](modules/Gameplay_Entitys_Enemys_Beharviours.md) · [Gameplay/Entitys/Enemys/Brain](modules/Gameplay_Entitys_Enemys_Brain.md) · [Gameplay/Entitys/Enemys/Brain/FSM](modules/Gameplay_Entitys_Enemys_Brain_FSM.md) · [Gameplay/Entitys/Enemys/Brain/FSM/States](modules/Gameplay_Entitys_Enemys_Brain_FSM_States.md) · [Gameplay/Entitys/Enemys/Brain/LinearPatrol](modules/Gameplay_Entitys_Enemys_Brain_LinearPatrol.md) · [Gameplay/Entitys/Enemys/NavAgents](modules/Gameplay_Entitys_Enemys_NavAgents.md) · [Gameplay/Entitys/Enemys/NavAgents/Core](modules/Gameplay_Entitys_Enemys_NavAgents_Core.md) · [Gameplay/Entitys/Enemys/Partbraked](modules/Gameplay_Entitys_Enemys_Partbraked.md) · [Gameplay/Entitys/NPCs](modules/Gameplay_Entitys_NPCs.md) · [Gameplay/Entitys/Players](modules/Gameplay_Entitys_Players.md) · [Gameplay/Entitys/Players/OxygenTube](modules/Gameplay_Entitys_Players_OxygenTube.md) · [Gameplay/Environments](modules/Gameplay_Environments.md) · [Gameplay/InteractObjects](modules/Gameplay_InteractObjects.md) · [Gameplay/MainCameras](modules/Gameplay_MainCameras.md) · [Gameplay/Projectiles](modules/Gameplay_Projectiles.md) · [Gameplay/QuestSystems/Quest](modules/Gameplay_QuestSystems_Quest.md) · [Gameplay/RopeSystems](modules/Gameplay_RopeSystems.md)

**UI**: [UI](modules/UI.md) · [UI/Interactions](modules/UI_Interactions.md) · [UI/Inventory](modules/UI_Inventory.md) · [UI/Inventory/InventoryBin](modules/UI_Inventory_InventoryBin.md) · [UI/Inventory/InventoryBuy](modules/UI_Inventory_InventoryBuy.md) · [UI/Inventory/InventoryPlayer](modules/UI_Inventory_InventoryPlayer.md) · [UI/Items](modules/UI_Items.md) · [UI/LoadingSecne](modules/UI_LoadingSecne.md) · [UI/LoadingSecne/LoadingTextAnimation](modules/UI_LoadingSecne_LoadingTextAnimation.md) · [UI/LoadingSecne/LoadingVisualController](modules/UI_LoadingSecne_LoadingVisualController.md) · [UI/Money](modules/UI_Money.md) · [UI/Notifications](modules/UI_Notifications.md) · [UI/Ocean](modules/UI_Ocean.md) · [UI/QuestCompleteUI](modules/UI_QuestCompleteUI.md) · [UI/Shop](modules/UI_Shop.md) · [UI/UIManager](modules/UI_UIManager.md) · [UI/UIMenuSetting](modules/UI_UIMenuSetting.md) · [UI/Upgrads](modules/UI_Upgrads.md) · [UI/VFX](modules/UI_VFX.md)

**Editor**: [Editor](modules/Editor.md)

## Legend

- `<|--` inherits · `<|..` implements · `-->` holds a field/property of that type (label = member)
- `+` public · `#` protected · `$` static · `*` abstract · `~T~` = generic `<T>` · `Array` = `[]`
- Stereotypes: `MB` MonoBehaviour · `SO` ScriptableObject · `static` · `abstract` · `interface` · `enum`
- Class boxes show public/protected API only, capped at 10 fields and 12 methods. External types show their module as stereotype.

## Limits

Static text parser, not the compiler. Links made only via `GetComponent<T>`, inspector wiring, prefabs, `SendMessage` or strings are invisible; identical names can overcount. `InputSystem_Actions.cs` (generated) and `Scripts/Deprecated/` are excluded.

# All scripts

[← index](README.md)

Every script in `Assets/Project_Ocean/Scripts` on one page: 242 files, 291 types. Click a module for its class diagram. The draw.io file has the matching **All Scripts** page.

## Core (51 files)

| Script | Module | Types | Extends / implements | LOC |
|---|---|---|---|---|
| DialogueState.cs | [Core](modules/Core.md) | `DialogueState` static | — | 61 |
| GameFeature.cs | [Core](modules/Core.md) | `GameFeature` enum | — | 38 |
| GameLog.cs | [Core](modules/Core.md) | `GameLog` static | — | 13 |
| GameSession.cs | [Core](modules/Core.md) | `GameSession` static | — | 9 |
| GameState.cs | [Core](modules/Core.md) | `GameState` enum | — | 9 |
| PlayerUIState.cs | [Core](modules/Core.md) | `PlayerUIState` static | — | 59 |
| SceneNames.cs | [Core](modules/Core.md) | `SceneNames` static | — | 14 |
| Singleton.cs | [Core](modules/Core.md) | `Singleton` MB/abstract | — | 34 |
| Tags.cs | [Core](modules/Core.md) | `Tags` static | — | 9 |
| BulletData.cs | [Core/Datas](modules/Core_Datas.md) | `AmmoType` enum<br>`BulletData` SO | —<br>— | 19 |
| RopeStructureData.cs | [Core/Datas](modules/Core_Datas.md) | `RopeStructureData` SO | — | 10 |
| InventoryItemDatabase.cs | [Core/Datas/Items](modules/Core_Datas_Items.md) | `InventoryItemDatabase` | — | 21 |
| Item.cs | [Core/Datas/Items](modules/Core_Datas_Items.md) | `Item` SO<br>`ItemType` enum | —<br>— | 49 |
| CharacterData.cs | [Core/Datas/ScriptableDatas/Characters](modules/Core_Datas_ScriptableDatas_Characters.md) | `CharacterData` SO | — | 18 |
| CharacterDatabase.cs | [Core/Datas/ScriptableDatas/Characters](modules/Core_Datas_ScriptableDatas_Characters.md) | `CharacterDatabase` SO | — | 28 |
| UpgradeData.cs | [Core/Datas/Upgrades](modules/Core_Datas_Upgrades.md) | `UpgradeType` enum<br>`UpgradeUnlock`<br>`UpgradeData` SO | —<br>—<br>— | 39 |
| CutsceneEvents.cs | [Core/Events](modules/Core_Events.md) | `CutsceneEvents` static | — | 13 |
| EnemyEvents.cs | [Core/Events](modules/Core_Events.md) | `EnemyEvents` static | — | 12 |
| ExploreEvents.cs | [Core/Events](modules/Core_Events.md) | `ExploreEvents` static | — | 13 |
| GameFlowEvents.cs | [Core/Events](modules/Core_Events.md) | `GameFlowEvents` static | — | 9 |
| NotificationChannel.cs | [Core/Events](modules/Core_Events.md) | `NotificationChannel` enum | — | 12 |
| NotificationManager.cs | [Core/Events](modules/Core_Events.md) | `NotificationManager` static | — | 19 |
| NotificationMessage.cs | [Core/Events](modules/Core_Events.md) | `NotificationMessage` | — | 20 |
| NpcEvents.cs | [Core/Events](modules/Core_Events.md) | `NpcEvents` static | — | 12 |
| RewardEvents.cs | [Core/Events](modules/Core_Events.md) | `RewardEvents` static | — | 9 |
| TutorialEvents.cs | [Core/Events](modules/Core_Events.md) | `TutorialActionId` enum<br>`TutorialEvents` static | —<br>— | 46 |
| TutorialGate.cs | [Core/Events](modules/Core_Events.md) | `TutorialGate` static | — | 48 |
| InputReader.cs | [Core/Inputs](modules/Core_Inputs.md) | `InputReader` SO | — | 154 |
| KeyAction.cs | [Core/Inputs/KeyBind](modules/Core_Inputs_KeyBind.md) | `KeyAction` enum | — | 15 |
| KeyBindManager.cs | [Core/Inputs/KeyBind](modules/Core_Inputs_KeyBind.md) | `KeyBindManager` MB | Singleton | 346 |
| MoveDirection.cs | [Core/Inputs/KeyBind/MoveDirection](modules/Core_Inputs_KeyBind_MoveDirection.md) | `MoveDirection` enum | — | 10 |
| KeyBindResetAllButton.cs | [Core/Inputs/KeyBind/SaveKeyBind](modules/Core_Inputs_KeyBind_SaveKeyBind.md) | `KeyBindResetAllButton` MB | — | 24 |
| KeyBindSettings.cs | [Core/Inputs/KeyBind/SaveKeyBind](modules/Core_Inputs_KeyBind_SaveKeyBind.md) | `KeyBindSettings`<br>`KeyBindingData` | —<br>— | 16 |
| KeyBindErrorUI.cs | [Core/Inputs/KeyBind/UIKeyBind](modules/Core_Inputs_KeyBind_UIKeyBind.md) | `KeyBindErrorUI` MB | — | 32 |
| KeyBindUI.cs | [Core/Inputs/KeyBind/UIKeyBind](modules/Core_Inputs_KeyBind_UIKeyBind.md) | `KeyBindUI` MB | RebindButtonBase | 19 |
| MoveKeyBindUI.cs | [Core/Inputs/KeyBind/UIKeyBind](modules/Core_Inputs_KeyBind_UIKeyBind.md) | `MoveKeyBindUI` MB | RebindButtonBase | 14 |
| RebindButtonBase.cs | [Core/Inputs/KeyBind/UIKeyBind](modules/Core_Inputs_KeyBind_UIKeyBind.md) | `RebindButtonBase` MB/abstract | — | 57 |
| IPoolable.cs | [Core/Pool](modules/Core_Pool.md) | `IPoolable` interface | — | 12 |
| ObjectPoolManager.cs | [Core/Pool](modules/Core_Pool.md) | `ObjectPoolManager` MB | Singleton | 82 |
| EncryptionHelper.cs | [Core/Save](modules/Core_Save.md) | `EncryptionHelper` static | — | 54 |
| ISaveParticipant.cs | [Core/Save](modules/Core_Save.md) | `ISaveParticipant` interface | — | 16 |
| SaveData.cs | [Core/Save](modules/Core_Save.md) | `SaveData`<br>`ItemSaveData`<br>`QuestSaveData` | —<br>—<br>— | 61 |
| SaveSystem.cs | [Core/Save](modules/Core_Save.md) | `SaveSlotMeta`<br>`SaveSystem` static | —<br>— | 201 |
| DayManager.cs | [Core/Save/DayReset](modules/Core_Save_DayReset.md) | `DayManager` MB | Singleton, ISaveParticipant | 79 |
| EnemyKillTracker.cs | [Core/Save/DayReset](modules/Core_Save_DayReset.md) | `EnemyKillTracker` MB | Singleton, ISaveParticipant | 34 |
| OneTimePickupTracker.cs | [Core/Save/DayReset](modules/Core_Save_DayReset.md) | `OneTimePickupTracker` MB | Singleton, ISaveParticipant | 26 |
| ILoadingVisual.cs | [Core/Scenes](modules/Core_Scenes.md) | `ILoadingVisual` interface | — | 8 |
| LoadingManager.cs | [Core/Scenes](modules/Core_Scenes.md) | `LoadingManager` MB | Singleton | 182 |
| LoadingType.cs | [Core/Scenes](modules/Core_Scenes.md) | `LoadingType` enum | — | 9 |
| SceneLoadContext.cs | [Core/Scenes](modules/Core_Scenes.md) | `SceneLoadContext` static | — | 34 |
| IntroPlayer.cs | [Core/VideoManager](modules/Core_VideoManager.md) | `IntroPlayer` MB | — | 89 |

## Gameplay (131 files)

| Script | Module | Types | Extends / implements | LOC |
|---|---|---|---|---|
| AudioManager.cs | [Gameplay/AudioSetting](modules/Gameplay_AudioSetting.md) | `AudioChannel` enum<br>`SceneSound`<br>`AudioManager` MB | —<br>—<br>Singleton | 200 |
| AudioSliderBinder.cs | [Gameplay/AudioSetting](modules/Gameplay_AudioSetting.md) | `AudioSliderBinder` MB | — | 34 |
| SoundChannel.cs | [Gameplay/AudioSetting](modules/Gameplay_AudioSetting.md) | `SoundChannel` MB | — | 39 |
| SoundKeys.cs | [Gameplay/AudioSetting](modules/Gameplay_AudioSetting.md) | `SoundKeys` static | — | 29 |
| VolumeSliderUI.cs | [Gameplay/AudioSetting](modules/Gameplay_AudioSetting.md) | `VolumeSliderUI` MB | — | 26 |
| IDialogueView.cs | [Gameplay/Contracts](modules/Gameplay_Contracts.md) | `IDialogueView` interface | — | 14 |
| IInteractPopupView.cs | [Gameplay/Contracts](modules/Gameplay_Contracts.md) | `IInteractPopupView` interface | — | 11 |
| IModalPrompt.cs | [Gameplay/Contracts](modules/Gameplay_Contracts.md) | `IModalPrompt` interface<br>`ModalPrompt` static | —<br>— | 22 |
| IQuestOfferView.cs | [Gameplay/Contracts](modules/Gameplay_Contracts.md) | `IQuestOfferView` interface | — | 10 |
| Cutscene.cs | [Gameplay/Cutscenes](modules/Gameplay_Cutscenes.md) | `CutsceneTrackBinding`<br>`Cutscene` MB | —<br>— | 208 |
| CutsceneGate.cs | [Gameplay/Cutscenes](modules/Gameplay_Cutscenes.md) | `CutsceneGate` static | — | 26 |
| CutsceneManager.cs | [Gameplay/Cutscenes](modules/Gameplay_Cutscenes.md) | `CutsceneManager` MB | Singleton | 205 |
| CutsceneSkipHint.cs | [Gameplay/Cutscenes](modules/Gameplay_Cutscenes.md) | `CutsceneSkipHint` MB | — | 21 |
| AnimHash.cs | [Gameplay/Entities](modules/Gameplay_Entities.md) | `AnimHash` static | — | 24 |
| Entity.cs | [Gameplay/Entities](modules/Gameplay_Entities.md) | `Entity` MB/abstract | — | 84 |
| EntityAnimation.cs | [Gameplay/Entities](modules/Gameplay_Entities.md) | `EntityAnimation` MB | — | 50 |
| EntityDeathHandler.cs | [Gameplay/Entities](modules/Gameplay_Entities.md) | `EntityDeathHandler` MB | IPoolable | 63 |
| EntityHealth.cs | [Gameplay/Entities](modules/Gameplay_Entities.md) | `EntityHealth` MB | IEntityHealth, IPoolable | 86 |
| IDamagePopup.cs | [Gameplay/Entities](modules/Gameplay_Entities.md) | `IDamagePopup` interface | — | 10 |
| IEntityHealth.cs | [Gameplay/Entities](modules/Gameplay_Entities.md) | `IEntityHealth` interface | — | 16 |
| Enemy.cs | [Gameplay/Entities/Enemies](modules/Gameplay_Entities_Enemies.md) | `Enemy` MB | Entity | 116 |
| EnemyLoot.cs | [Gameplay/Entities/Enemies](modules/Gameplay_Entities_Enemies.md) | `EnemyLoot` MB | CollectableItemBase | 48 |
| Boid.cs | [Gameplay/Entities/Enemies/Behaviours](modules/Gameplay_Entities_Enemies_Behaviours.md) | `Boid` MB | IPoolable | 115 |
| AggressiveEnemyProfile.cs | [Gameplay/Entities/Enemies/Brain](modules/Gameplay_Entities_Enemies_Brain.md) | `AggressiveEnemyProfile` SO | EnemyBaseProfile | 54 |
| EnemyBaseProfile.cs | [Gameplay/Entities/Enemies/Brain](modules/Gameplay_Entities_Enemies_Brain.md) | `EnemyBaseProfile` SO/abstract | — | 73 |
| EnemyBrain.cs | [Gameplay/Entities/Enemies/Brain](modules/Gameplay_Entities_Enemies_Brain.md) | `EnemyBrain` | EnemyBrainBase, IBrainVision, IBrainAttack, IBrainHitReaction | 147 |
| EnemyBrainBase.cs | [Gameplay/Entities/Enemies/Brain](modules/Gameplay_Entities_Enemies_Brain.md) | `EnemyBrainBase` abstract | IEnemyBrain | 47 |
| EnemyBrainContext.cs | [Gameplay/Entities/Enemies/Brain](modules/Gameplay_Entities_Enemies_Brain.md) | `EnemyBrainContext` | — | 22 |
| EnemyVision.cs | [Gameplay/Entities/Enemies/Brain](modules/Gameplay_Entities_Enemies_Brain.md) | `EnemyVision` | — | 59 |
| FriendlyEnemyProfile.cs | [Gameplay/Entities/Enemies/Brain](modules/Gameplay_Entities_Enemies_Brain.md) | `FriendlyEnemyProfile` SO | EnemyBaseProfile | 16 |
| ScaredEnemyProfile.cs | [Gameplay/Entities/Enemies/Brain](modules/Gameplay_Entities_Enemies_Brain.md) | `ScaredEnemyProfile` SO | EnemyBaseProfile | 22 |
| EnemyFSM.cs | [Gameplay/Entities/Enemies/Brain/FSM](modules/Gameplay_Entities_Enemies_Brain_FSM.md) | `EnemyFSM` | — | 56 |
| EnemyStateBase.cs | [Gameplay/Entities/Enemies/Brain/FSM](modules/Gameplay_Entities_Enemies_Brain_FSM.md) | `EnemyStateBase` abstract | IEnemyState | 96 |
| EnemyStateTag.cs | [Gameplay/Entities/Enemies/Brain/FSM](modules/Gameplay_Entities_Enemies_Brain_FSM.md) | `EnemyStateTag` enum | — | 20 |
| EnemyAttackState.cs | [Gameplay/Entities/Enemies/Brain/FSM/States](modules/Gameplay_Entities_Enemies_Brain_FSM_States.md) | `EnemyAttackState` | EnemyStateBase | 149 |
| EnemyFleeState.cs | [Gameplay/Entities/Enemies/Brain/FSM/States](modules/Gameplay_Entities_Enemies_Brain_FSM_States.md) | `EnemyFleeState` | EnemyStateBase | 121 |
| EnemyFollowState.cs | [Gameplay/Entities/Enemies/Brain/FSM/States](modules/Gameplay_Entities_Enemies_Brain_FSM_States.md) | `EnemyFollowState` | EnemyStateBase | 245 |
| EnemyPrepareAttackState.cs | [Gameplay/Entities/Enemies/Brain/FSM/States](modules/Gameplay_Entities_Enemies_Brain_FSM_States.md) | `EnemyPrepareAttackState` | EnemyStateBase | 47 |
| EnemySearchState.cs | [Gameplay/Entities/Enemies/Brain/FSM/States](modules/Gameplay_Entities_Enemies_Brain_FSM_States.md) | `EnemySearchState` | EnemyStateBase | 71 |
| EnemyWanderState.cs | [Gameplay/Entities/Enemies/Brain/FSM/States](modules/Gameplay_Entities_Enemies_Brain_FSM_States.md) | `EnemyWanderState` | EnemyStateBase | 170 |
| IBrainAttack.cs | [Gameplay/Entities/Enemies/Brain/Interfaces](modules/Gameplay_Entities_Enemies_Brain_Interfaces.md) | `IBrainAttack` interface | — | 8 |
| IBrainHitReaction.cs | [Gameplay/Entities/Enemies/Brain/Interfaces](modules/Gameplay_Entities_Enemies_Brain_Interfaces.md) | `IBrainHitReaction` interface | — | 10 |
| IBrainVision.cs | [Gameplay/Entities/Enemies/Brain/Interfaces](modules/Gameplay_Entities_Enemies_Brain_Interfaces.md) | `IBrainVision` interface | — | 19 |
| IEnemyBrain.cs | [Gameplay/Entities/Enemies/Brain/Interfaces](modules/Gameplay_Entities_Enemies_Brain_Interfaces.md) | `IEnemyBrain` interface | — | 13 |
| IEnemyState.cs | [Gameplay/Entities/Enemies/Brain/Interfaces](modules/Gameplay_Entities_Enemies_Brain_Interfaces.md) | `IEnemyState` interface | — | 18 |
| LinearPatrolBrain.cs | [Gameplay/Entities/Enemies/Brain/LinearPatrol](modules/Gameplay_Entities_Enemies_Brain_LinearPatrol.md) | `LinearPatrolBrain`<br>`State` enum | IEnemyBrain, IBrainHitReaction<br>— | 199 |
| LinearPatrolProfile.cs | [Gameplay/Entities/Enemies/Brain/LinearPatrol](modules/Gameplay_Entities_Enemies_Brain_LinearPatrol.md) | `PatrolAxis` enum<br>`PatrolMovementMode` enum<br>`LinearPatrolProfile` SO | —<br>—<br>EnemyBaseProfile | 45 |
| CharacterFacing2D.cs | [Gameplay/Entities/Enemies/NavAgents](modules/Gameplay_Entities_Enemies_NavAgents.md) | `CharacterFacing2D` static | — | 41 |
| EnemyAgent.cs | [Gameplay/Entities/Enemies/NavAgents](modules/Gameplay_Entities_Enemies_NavAgents.md) | `EnemyAgent` MB | EnemyNavAgentBase | 33 |
| FishAlertUI.cs | [Gameplay/Entities/Enemies/NavAgents](modules/Gameplay_Entities_Enemies_NavAgents.md) | `FishAlertUI` MB | — | 107 |
| EnemyFacingController.cs | [Gameplay/Entities/Enemies/NavAgents/Core](modules/Gameplay_Entities_Enemies_NavAgents_Core.md) | `EnemyFacingController`<br>`FacingTurnMode` enum | —<br>— | 127 |
| EnemyNavAgentBase.cs | [Gameplay/Entities/Enemies/NavAgents/Core](modules/Gameplay_Entities_Enemies_NavAgents_Core.md) | `EnemyNavAgentBase` MB/abstract | IPoolable | 256 |
| EnemyNavPathfinder.cs | [Gameplay/Entities/Enemies/NavAgents/Core](modules/Gameplay_Entities_Enemies_NavAgents_Core.md) | `EnemyNavPathfinder` | — | 115 |
| EnemyVisionHelper.cs | [Gameplay/Entities/Enemies/NavAgents/Core](modules/Gameplay_Entities_Enemies_NavAgents_Core.md) | `EnemyVisionHelper` static | — | 26 |
| EnemyAttackHitbox.cs | [Gameplay/Entities/Enemies/PartBreak](modules/Gameplay_Entities_Enemies_PartBreak.md) | `EnemyAttackHitbox` MB | — | 24 |
| NPC.cs | [Gameplay/Entities/NPCs](modules/Gameplay_Entities_NPCs.md) | `NPC` MB | — | 161 |
| NpcQuestConversation.cs | [Gameplay/Entities/NPCs](modules/Gameplay_Entities_NPCs.md) | `NpcQuestConversation` | — | 94 |
| NpcQuestMarker.cs | [Gameplay/Entities/NPCs](modules/Gameplay_Entities_NPCs.md) | `NpcQuestMarker` | — | 36 |
| DevPanel.cs | [Gameplay/Entities/Players](modules/Gameplay_Entities_Players.md) | `DevPanel` MB<br>`DevItemButton` struct | —<br>— | 185 |
| Player.cs | [Gameplay/Entities/Players](modules/Gameplay_Entities_Players.md) | `Player` MB | Entity | 71 |
| PlayerGunHandler.cs | [Gameplay/Entities/Players](modules/Gameplay_Entities_Players.md) | `PlayerGunHandler` MB | — | 278 |
| PlayerHealth.cs | [Gameplay/Entities/Players](modules/Gameplay_Entities_Players.md) | `PlayerHealth` MB | EntityHealth, ISaveParticipant | 104 |
| PlayerMoney.cs | [Gameplay/Entities/Players](modules/Gameplay_Entities_Players.md) | `PlayerMoney` MB | ISaveParticipant | 50 |
| PlayerMovement.cs | [Gameplay/Entities/Players](modules/Gameplay_Entities_Players.md) | `PlayerMovement` MB | — | 111 |
| PlayerRegistry.cs | [Gameplay/Entities/Players](modules/Gameplay_Entities_Players.md) | `PlayerRegistry` static | — | 26 |
| PlayerSceneMode.cs | [Gameplay/Entities/Players](modules/Gameplay_Entities_Players.md) | `PlayerSceneMode` MB | — | 61 |
| PlayerSprintVFX.cs | [Gameplay/Entities/Players](modules/Gameplay_Entities_Players.md) | `PlayerSprintVFX` MB | — | 58 |
| PlayerStabAttack.cs | [Gameplay/Entities/Players](modules/Gameplay_Entities_Players.md) | `PlayerStabAttack` MB | — | 25 |
| QuestCompass.cs | [Gameplay/Entities/Players](modules/Gameplay_Entities_Players.md) | `QuestCompass` MB | — | 60 |
| ObjectWrapPoint.cs | [Gameplay/Entities/Players/OxygenTube](modules/Gameplay_Entities_Players_OxygenTube.md) | `ObjectWrapPoint` MB<br>`WrapPoint` | —<br>— | 201 |
| OxygenRope.cs | [Gameplay/Entities/Players/OxygenTube](modules/Gameplay_Entities_Players_OxygenTube.md) | `OxygenRope` MB<br>`WrapPin` struct | —<br>— | 468 |
| RopeCornerFinder.cs | [Gameplay/Entities/Players/OxygenTube](modules/Gameplay_Entities_Players_OxygenTube.md) | `RopeCornerFinder` static | — | 85 |
| VerletRope.cs | [Gameplay/Entities/Players/OxygenTube](modules/Gameplay_Entities_Players_OxygenTube.md) | `VerletRope`<br>`Settings` struct<br>`Seg` struct | —<br>—<br>— | 165 |
| Bird.cs | [Gameplay/Environments](modules/Gameplay_Environments.md) | `Bird` MB | — | 37 |
| ChestInteract.cs | [Gameplay/Environments](modules/Gameplay_Environments.md) | `ChestInteract` MB | Interactable | 61 |
| DoorInteract.cs | [Gameplay/Environments](modules/Gameplay_Environments.md) | `DoorInteract` MB | Interactable | 52 |
| DoorQuestMarker.cs | [Gameplay/Environments](modules/Gameplay_Environments.md) | `DoorQuestMarker` MB | QuestMarkerBase | 76 |
| NPCManager.cs | [Gameplay/Environments](modules/Gameplay_Environments.md) | `NPCManager` MB | Singleton | 62 |
| RevealAfterTotalDives.cs | [Gameplay/Environments](modules/Gameplay_Environments.md) | `RevealAfterTotalDives` MB | — | 19 |
| SimpleAnimLoop.cs | [Gameplay/Environments](modules/Gameplay_Environments.md) | `SimpleAnimLoop` MB | — | 50 |
| TrashInteract.cs | [Gameplay/Environments](modules/Gameplay_Environments.md) | `TrashInteract` MB | Interactable | 115 |
| GameManager.cs | [Gameplay/Game](modules/Gameplay_Game.md) | `GameManager` MB | Singleton | 158 |
| Interactable.cs | [Gameplay/Interaction](modules/Gameplay_Interaction.md) | `Interactable` MB/abstract | — | 86 |
| InteractionTargeting.cs | [Gameplay/Interaction](modules/Gameplay_Interaction.md) | `InteractionTargeting` static | — | 26 |
| InteractUI.cs | [Gameplay/Interaction](modules/Gameplay_Interaction.md) | `InteractUI` MB | Interactable | 15 |
| PlayerInteractor.cs | [Gameplay/Interaction](modules/Gameplay_Interaction.md) | `PlayerInteractor` MB | — | 56 |
| IInventory.cs | [Gameplay/Inventory](modules/Gameplay_Inventory.md) | `IInventory` interface | — | 20 |
| Inventory.cs | [Gameplay/Inventory](modules/Gameplay_Inventory.md) | `Inventory` static | — | 17 |
| InventoryModel.cs | [Gameplay/Inventory](modules/Gameplay_Inventory.md) | `InventoryModel` | — | 159 |
| InventorySaveMapper.cs | [Gameplay/Inventory](modules/Gameplay_Inventory.md) | `InventorySaveMapper` static | — | 55 |
| ItemStack.cs | [Gameplay/Inventory](modules/Gameplay_Inventory.md) | `ItemStack` struct | — | 17 |
| IItemEffect.cs | [Gameplay/Items](modules/Gameplay_Items.md) | `IItemEffect` interface | — | 14 |
| PlayerItemUse.cs | [Gameplay/Items](modules/Gameplay_Items.md) | `PlayerItemUse` static | — | 57 |
| CollectableItemBase.cs | [Gameplay/Items/Pickups](modules/Gameplay_Items_Pickups.md) | `CollectableItemBase` abstract/MB | Interactable | 15 |
| ItemDrop.cs | [Gameplay/Items/Pickups](modules/Gameplay_Items_Pickups.md) | `ItemDrop` MB | CollectableItemBase, IPoolable | 60 |
| ShelleController.cs | [Gameplay/Items/Pickups](modules/Gameplay_Items_Pickups.md) | `ShelleController` MB | IPoolable | 106 |
| MiniHubItem.cs | [Gameplay/Items/Placeables](modules/Gameplay_Items_Placeables.md) | `MiniHubItem` MB | — | 9 |
| CameraShakeManager.cs | [Gameplay/MainCameras](modules/Gameplay_MainCameras.md) | `CameraShakeManager` MB | Singleton | 19 |
| PlayerCamera.cs | [Gameplay/MainCameras](modules/Gameplay_MainCameras.md) | `PlayerCamera` MB | — | 31 |
| Bullet.cs | [Gameplay/Projectiles](modules/Gameplay_Projectiles.md) | `Bullet` MB/abstract | — | 80 |
| GranadeBullet.cs | [Gameplay/Projectiles](modules/Gameplay_Projectiles.md) | `GranadeBullet` MB | Bullet | 5 |
| GrapplingHookBullet.cs | [Gameplay/Projectiles](modules/Gameplay_Projectiles.md) | `GrapplingHookBullet` MB | Bullet | 148 |
| NormalBullet.cs | [Gameplay/Projectiles](modules/Gameplay_Projectiles.md) | `NormalBullet` MB | Bullet | 61 |
| TranquilizerBullet.cs | [Gameplay/Projectiles](modules/Gameplay_Projectiles.md) | `TranquilizerBullet` MB | Bullet | 5 |
| ExploreAreaTrigger.cs | [Gameplay/QuestSystems/Quest](modules/Gameplay_QuestSystems_Quest.md) | `ExploreAreaTrigger` MB<br>`Mode` enum<br>`DialogueLine` struct | —<br>—<br>— | 211 |
| PlayerInteriorState.cs | [Gameplay/QuestSystems/Quest](modules/Gameplay_QuestSystems_Quest.md) | `PlayerInterior` enum<br>`PlayerInteriorState` static | —<br>— | 50 |
| PlayerSelfDialogueUI.cs | [Gameplay/QuestSystems/Quest](modules/Gameplay_QuestSystems_Quest.md) | `PlayerSelfDialogueUI` MB<br>`Line` struct | —<br>— | 158 |
| Quest.cs | [Gameplay/QuestSystems/Quest](modules/Gameplay_QuestSystems_Quest.md) | `QuestType` enum<br>`DeliverySource` enum<br>`QuestStatus` enum<br>`Quest` | —<br>—<br>—<br>— | 251 |
| QuestData.cs | [Gameplay/QuestSystems/Quest](modules/Gameplay_QuestSystems_Quest.md) | `QuestCompletionMode` enum<br>`QuestCategory` enum<br>`QuestClassification` enum<br>`NextQuestMode` enum<br>`DialogueType` enum<br>`DeliveryCountMode` enum<br>`TalkCompletionMode` enum<br>`QuestItemRequirement`<br>`QuestDialogueLine`<br>`QuestStepData`<br>`QuestData` SO | —<br>—<br>—<br>—<br>—<br>—<br>—<br>—<br>—<br>—<br>— | 165 |
| QuestManager.cs | [Gameplay/QuestSystems/Quest](modules/Gameplay_QuestSystems_Quest.md) | `QuestManager` MB | Singleton, ISaveParticipant | 456 |
| QuestMarkerBase.cs | [Gameplay/QuestSystems/Quest](modules/Gameplay_QuestSystems_Quest.md) | `QuestMarkerBase` MB/abstract | — | 29 |
| QuestNavigation.cs | [Gameplay/QuestSystems/Quest](modules/Gameplay_QuestSystems_Quest.md) | `QuestNavigation` static | — | 90 |
| QuestProgressTracker.cs | [Gameplay/QuestSystems/Quest](modules/Gameplay_QuestSystems_Quest.md) | `QuestProgressTracker` | — | 230 |
| QuestSaveMapper.cs | [Gameplay/QuestSystems/Quest](modules/Gameplay_QuestSystems_Quest.md) | `QuestSaveMapper` static | — | 41 |
| QuestStep.cs | [Gameplay/QuestSystems/Quest](modules/Gameplay_QuestSystems_Quest.md) | `QuestStep` | — | 210 |
| QuestStepEffects.cs | [Gameplay/QuestSystems/Quest](modules/Gameplay_QuestSystems_Quest.md) | `QuestStepEffects` static | — | 91 |
| QuestVisuals.cs | [Gameplay/QuestSystems/Quest](modules/Gameplay_QuestSystems_Quest.md) | `QuestVisuals` static | — | 73 |
| ReviceQuestListUI.cs | [Gameplay/QuestSystems/Quest](modules/Gameplay_QuestSystems_Quest.md) | `ReviceQuestListUI` MB | — | 69 |
| ReviceQuestSlot.cs | [Gameplay/QuestSystems/Quest](modules/Gameplay_QuestSystems_Quest.md) | `ReviceQuestSlot` MB | — | 37 |
| TutorialActionMarker.cs | [Gameplay/QuestSystems/Quest](modules/Gameplay_QuestSystems_Quest.md) | `TutorialActionMarker` MB | QuestMarkerBase | 37 |
| TutorialPromptUI.cs | [Gameplay/QuestSystems/Quest](modules/Gameplay_QuestSystems_Quest.md) | `TutorialPromptUI` MB | — | 104 |
| TutorialStep.cs | [Gameplay/QuestSystems/Quest](modules/Gameplay_QuestSystems_Quest.md) | `TutorialStep` static | — | 37 |
| TutorialUIMarker.cs | [Gameplay/QuestSystems/Quest](modules/Gameplay_QuestSystems_Quest.md) | `TutorialUIMarker` MB | QuestMarkerBase | 24 |
| RopeSystemUtil.cs | [Gameplay/RopeSystems](modules/Gameplay_RopeSystems.md) | `RopeSystemUtil` static | — | 56 |
| SaveBridge.cs | [Gameplay/Save](modules/Gameplay_Save.md) | `SaveBridge` MB | Singleton | 255 |
| FeatureUnlockService.cs | [Gameplay/Services](modules/Gameplay_Services.md) | `FeatureUnlockService` | ISaveParticipant | 99 |
| UpgradeService.cs | [Gameplay/Services](modules/Gameplay_Services.md) | `UpgradeService` | ISaveParticipant | 119 |
| EnemySpawner.cs | [Gameplay/Spawning](modules/Gameplay_Spawning.md) | `ZoneSpawnEntry`<br>`EnemySpawner` MB | IWeightedEntry<br>— | 228 |
| GameObjectSpawnManager.cs | [Gameplay/Spawning](modules/Gameplay_Spawning.md) | `EnemySpawnEntry`<br>`ItemSpawnEntry`<br>`GameObjectSpawnManager` MB | —<br>—<br>Singleton | 138 |
| ItemSpawner.cs | [Gameplay/Spawning](modules/Gameplay_Spawning.md) | `ZoneItemEntry`<br>`ItemSpawner` MB | IWeightedEntry<br>— | 143 |
| WeightedPicker.cs | [Gameplay/Spawning](modules/Gameplay_Spawning.md) | `IWeightedEntry` interface<br>`WeightedPicker` static | —<br>— | 37 |

## UI (58 files)

| Script | Module | Types | Extends / implements | LOC |
|---|---|---|---|---|
| AmmoWheel.cs | [UI](modules/UI.md) | `AmmoWheel` MB | — | 58 |
| ButtonHoverScale.cs | [UI](modules/UI.md) | `ButtonHoverScale` MB | — | 73 |
| ConfirmationDialog.cs | [UI](modules/UI.md) | `ConfirmationDialog` MB | Singleton, IModalPrompt | 74 |
| DialogueSpeaker.cs | [UI](modules/UI.md) | `DialogueSpeaker` static | — | 32 |
| EnemyHealthDisplay.cs | [UI](modules/UI.md) | `EnemyHealthDisplay` MB | — | 16 |
| EscClosablePanel.cs | [UI](modules/UI.md) | `EscClosablePanel` MB | — | 23 |
| GameHUD.cs | [UI](modules/UI.md) | `GameHUD` MB | — | 222 |
| InteractPopupView.cs | [UI](modules/UI.md) | `InteractPopupView` MB | IInteractPopupView | 37 |
| LandUI.cs | [UI](modules/UI.md) | `LandUI` MB | — | 177 |
| MainMenu.cs | [UI](modules/UI.md) | `MainMenu` MB | — | 18 |
| NextDayAnimationUI.cs | [UI](modules/UI.md) | `NextDayAnimationUI` MB | — | 60 |
| NotificationUI.cs | [UI](modules/UI.md) | `NotificationUI` MB | — | 19 |
| PunchScale.cs | [UI](modules/UI.md) | `PunchScale` MB | — | 40 |
| SaveNotificationUI.cs | [UI](modules/UI.md) | `SaveNotificationUI` MB | — | 14 |
| TipPanelUI.cs | [UI](modules/UI.md) | `TipPanelUI` MB | — | 41 |
| UIFadeBackground.cs | [UI](modules/UI.md) | `UIFadeBackground` MB | — | 47 |
| UILock.cs | [UI](modules/UI.md) | `UILock` MB | — | 13 |
| DialoguePanelView.cs | [UI/Dialogue](modules/UI_Dialogue.md) | `DialoguePanelView` MB | IDialogueView | 28 |
| DamagePoint.cs | [UI/Interactions](modules/UI_Interactions.md) | `DamagePoint` MB | IDamagePopup | 34 |
| InventoryManager.cs | [UI/Inventory](modules/UI_Inventory.md) | `InventoryManager` MB | Singleton, ISaveParticipant, IInventory | 352 |
| InventoryUIController.cs | [UI/Inventory](modules/UI_Inventory.md) | `InventoryUIController` MB | — | 66 |
| SlotBuilder.cs | [UI/Inventory](modules/UI_Inventory.md) | `SlotBuilder` static | — | 23 |
| UISlotHoverHighlight.cs | [UI/Inventory](modules/UI_Inventory.md) | `UISlotHoverHighlight` MB | — | 29 |
| TrashBin.cs | [UI/Inventory/InventoryBin](modules/UI_Inventory_InventoryBin.md) | `TrashBin` MB | — | 64 |
| ShopBuySlot.cs | [UI/Inventory/InventoryBuy](modules/UI_Inventory_InventoryBuy.md) | `ShopBuySlot` MB | — | 43 |
| ShopInventoryBuy.cs | [UI/Inventory/InventoryBuy](modules/UI_Inventory_InventoryBuy.md) | `ShopInventoryBuy` MB | — | 46 |
| DraggableItem.cs | [UI/Inventory/InventoryPlayer](modules/UI_Inventory_InventoryPlayer.md) | `DraggableItem` MB | — | 54 |
| InventorySlot.cs | [UI/Inventory/InventoryPlayer](modules/UI_Inventory_InventoryPlayer.md) | `InventorySlot` MB | — | 65 |
| BootstrapLoader.cs | [UI/Loading](modules/UI_Loading.md) | `BootstrapLoader` MB | — | 16 |
| LoadingButton.cs | [UI/Loading](modules/UI_Loading.md) | `LoadingButton` MB | — | 45 |
| LoadingTextAnimation.cs | [UI/Loading/LoadingTextAnimation](modules/UI_Loading_LoadingTextAnimation.md) | `LoadingTextAnimation` MB | — | 26 |
| LoadingVisualController.cs | [UI/Loading/LoadingVisualController](modules/UI_Loading_LoadingVisualController.md) | `LoadingVisualController` MB<br>`LoadingProfile` | ILoadingVisual<br>— | 45 |
| MoneyUI.cs | [UI/Money](modules/UI_Money.md) | `MoneyUI` MB | — | 48 |
| NotificationLine.cs | [UI/Notifications](modules/UI_Notifications.md) | `NotificationLine` MB | — | 43 |
| NotificationView.cs | [UI/Notifications](modules/UI_Notifications.md) | `NotificationView` MB/abstract | — | 33 |
| ResourceNotificationPresenter.cs | [UI/Notifications](modules/UI_Notifications.md) | `ResourceNotificationPresenter` MB | — | 33 |
| StackNotificationView.cs | [UI/Notifications](modules/UI_Notifications.md) | `StackNotificationView` MB | NotificationView | 31 |
| ToastNotificationView.cs | [UI/Notifications](modules/UI_Notifications.md) | `ToastNotificationView` MB | NotificationView | 88 |
| RopeBreakCountdownUI.cs | [UI/Ocean](modules/UI_Ocean.md) | `RopeBreakCountdownUI` MB | — | 82 |
| QuestBoardUI.cs | [UI/Quest](modules/UI_Quest.md) | `QuestBoardUI` MB<br>`QuestCategoryUIData` | —<br>— | 281 |
| QuestCategorySlot.cs | [UI/Quest](modules/UI_Quest.md) | `QuestCategorySlot` MB | — | 37 |
| QuestDetailPanel.cs | [UI/Quest](modules/UI_Quest.md) | `QuestDetailPanel` MB | IQuestOfferView | 79 |
| QuestSlot.cs | [UI/Quest](modules/UI_Quest.md) | `QuestSlot` MB | — | 78 |
| QuestTrackerUI.cs | [UI/Quest](modules/UI_Quest.md) | `QuestTrackerUI` MB | — | 106 |
| QuestCompleteUI.cs | [UI/QuestCompleteUI](modules/UI_QuestCompleteUI.md) | `QuestCompleteUI` MB | Singleton | 127 |
| SaveMenuUI.cs | [UI/Save](modules/UI_Save.md) | `SaveMenuUI` MB | — | 136 |
| SaveSlotUI.cs | [UI/Save](modules/UI_Save.md) | `SaveSlotUI` MB | — | 56 |
| ShopManager.cs | [UI/Shop](modules/UI_Shop.md) | `ShopManager` MB | — | 318 |
| ShopTransactionService.cs | [UI/Shop](modules/UI_Shop.md) | `ShopTransactionService` | — | 87 |
| UIManager.cs | [UI/UIManager](modules/UI_UIManager.md) | `UIManager` MB<br>`UIManager_UIType` enum | Singleton<br>— | 447 |
| UIWindow.cs | [UI/UIManager](modules/UI_UIManager.md) | `UIWindow` MB<br>`UIWindow_UIType` enum | —<br>— | 43 |
| SettingMenuButton.cs | [UI/UIMenuSetting](modules/UI_UIMenuSetting.md) | `SettingMenuButton` MB | — | 18 |
| SettingTabManager.cs | [UI/UIMenuSetting](modules/UI_UIMenuSetting.md) | `SettingTabManager` MB | — | 40 |
| UISettingManager.cs | [UI/UIMenuSetting](modules/UI_UIMenuSetting.md) | `UISettingManager` MB | — | 98 |
| UpgradeManager.cs | [UI/Upgrades](modules/UI_Upgrades.md) | `UpgradeManager` MB | — | 49 |
| UpgradeSlot.cs | [UI/Upgrades](modules/UI_Upgrades.md) | `UpgradeSlot` MB | — | 118 |
| UpgradeTooltip.cs | [UI/Upgrades](modules/UI_Upgrades.md) | `UpgradeTooltip` MB | — | 346 |
| PlayerFeedbackVFX.cs | [UI/VFX](modules/UI_VFX.md) | `PlayerFeedbackVFX` MB | — | 119 |

## Editor (2 files)

| Script | Module | Types | Extends / implements | LOC |
|---|---|---|---|---|
| QuestDataEditor.cs | [Editor](modules/Editor.md) | `QuestDataEditor` Editor | — | 67 |
| QuestStepDataDrawer.cs | [Editor](modules/Editor.md) | `QuestStepDataDrawer` Drawer<br>`FieldVisibility` struct | —<br>— | 655 |


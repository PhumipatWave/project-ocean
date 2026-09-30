# Inheritance & interfaces

[← index](README.md)

93 types in project-internal inheritance / implementation. Unity bases (MonoBehaviour, ScriptableObject) are stereotypes.

```mermaid
classDiagram
  direction TB
  class AggressiveEnemyProfile {
    <<SO>>
  }
  class AudioManager {
    <<MB>>
  }
  class Boid {
    <<MB>>
  }
  class Bullet {
    <<MB>>
    <<abstract>>
  }
  class CameraShakeManager {
    <<MB>>
  }
  class ChestInteract {
    <<MB>>
  }
  class CollectableItemBase {
    <<abstract>>
    <<MB>>
  }
  class ConfirmationDialog {
    <<MB>>
  }
  class CutsceneManager {
    <<MB>>
  }
  class DamagePopup {
    <<MB>>
  }
  class DayManager {
    <<MB>>
  }
  class DialoguePanelView {
    <<MB>>
  }
  class DoorInteract {
    <<MB>>
  }
  class DoorQuestMarker {
    <<MB>>
  }
  class Enemy {
    <<MB>>
  }
  class EnemyAgent {
    <<MB>>
  }
  class EnemyAttackState {
  }
  class EnemyBaseProfile {
    <<SO>>
    <<abstract>>
  }
  class EnemyBrain {
  }
  class EnemyBrainBase {
    <<abstract>>
  }
  class EnemyFleeState {
  }
  class EnemyFollowState {
  }
  class EnemyKillTracker {
    <<MB>>
  }
  class EnemyLoot {
    <<MB>>
  }
  class EnemyNavAgentBase {
    <<MB>>
    <<abstract>>
  }
  class EnemyPrepareAttackState {
  }
  class EnemySearchState {
  }
  class EnemyStateBase {
    <<abstract>>
  }
  class EnemyWanderState {
  }
  class Entity {
    <<MB>>
    <<abstract>>
  }
  class EntityDeathHandler {
    <<MB>>
  }
  class EntityHealth {
    <<MB>>
  }
  class EventInteractable {
    <<MB>>
  }
  class FeatureUnlockService {
  }
  class FriendlyEnemyProfile {
    <<SO>>
  }
  class GameManager {
    <<MB>>
  }
  class GrapplingHookBullet {
    <<MB>>
  }
  class GrenadeBullet {
    <<MB>>
  }
  class IBrainAttack {
    <<interface>>
  }
  class IBrainHitReaction {
    <<interface>>
  }
  class IBrainVision {
    <<interface>>
  }
  class IDamagePopup {
    <<interface>>
  }
  class IDialogueView {
    <<interface>>
  }
  class IEnemyBrain {
    <<interface>>
  }
  class IEnemyState {
    <<interface>>
  }
  class IEntityHealth {
    <<interface>>
  }
  class IInteractPopupView {
    <<interface>>
  }
  class IInventory {
    <<interface>>
  }
  class ILoadingVisual {
    <<interface>>
  }
  class IModalPrompt {
    <<interface>>
  }
  class IPoolable {
    <<interface>>
  }
  class IQuestOfferView {
    <<interface>>
  }
  class ISaveParticipant {
    <<interface>>
  }
  class IWeightedEntry {
    <<interface>>
  }
  class InteractPopupView {
    <<MB>>
  }
  class Interactable {
    <<MB>>
    <<abstract>>
  }
  class InventoryManager {
    <<MB>>
  }
  class ItemDrop {
    <<MB>>
  }
  class KeyBindManager {
    <<MB>>
  }
  class KeyBindUI {
    <<MB>>
  }
  class LinearPatrolBrain {
  }
  class LinearPatrolProfile {
    <<SO>>
  }
  class LoadingManager {
    <<MB>>
  }
  class LoadingVisualController {
    <<MB>>
  }
  class MoveKeyBindUI {
    <<MB>>
  }
  class NPCManager {
    <<MB>>
  }
  class NormalBullet {
    <<MB>>
  }
  class NotificationView {
    <<MB>>
    <<abstract>>
  }
  class ObjectPoolManager {
    <<MB>>
  }
  class OneTimePickupTracker {
    <<MB>>
  }
  class Player {
    <<MB>>
  }
  class PlayerHealth {
    <<MB>>
  }
  class PlayerMoney {
    <<MB>>
  }
  class QuestCompleteUI {
    <<MB>>
  }
  class QuestDetailPanel {
    <<MB>>
  }
  class QuestManager {
    <<MB>>
  }
  class QuestMarkerBase {
    <<MB>>
    <<abstract>>
  }
  class RebindButtonBase {
    <<MB>>
    <<abstract>>
  }
  class SaveBridge {
    <<MB>>
  }
  class ScaredEnemyProfile {
    <<SO>>
  }
  class ShellPickup {
    <<MB>>
  }
  class Singleton {
    <<MB>>
    <<abstract>>
  }
  class SpawnCatalog {
    <<MB>>
  }
  class StackNotificationView {
    <<MB>>
  }
  class ToastNotificationView {
    <<MB>>
  }
  class TranquilizerBullet {
    <<MB>>
  }
  class TrashInteract {
    <<MB>>
  }
  class TutorialActionMarker {
    <<MB>>
  }
  class TutorialUIMarker {
    <<MB>>
  }
  class UIManager {
    <<MB>>
  }
  class UpgradeService {
  }
  class ZoneItemEntry {
  }
  class ZoneSpawnEntry {
  }
  EnemyBaseProfile <|-- AggressiveEnemyProfile
  Singleton <|-- AudioManager
  IPoolable <|.. Boid
  Singleton <|-- CameraShakeManager
  Interactable <|-- ChestInteract
  Interactable <|-- CollectableItemBase
  Singleton <|-- ConfirmationDialog
  IModalPrompt <|.. ConfirmationDialog
  Singleton <|-- CutsceneManager
  IDamagePopup <|.. DamagePopup
  Singleton <|-- DayManager
  ISaveParticipant <|.. DayManager
  IDialogueView <|.. DialoguePanelView
  Interactable <|-- DoorInteract
  QuestMarkerBase <|-- DoorQuestMarker
  Entity <|-- Enemy
  EnemyNavAgentBase <|-- EnemyAgent
  EnemyStateBase <|-- EnemyAttackState
  EnemyBrainBase <|-- EnemyBrain
  IBrainVision <|.. EnemyBrain
  IBrainAttack <|.. EnemyBrain
  IBrainHitReaction <|.. EnemyBrain
  IEnemyBrain <|.. EnemyBrainBase
  EnemyStateBase <|-- EnemyFleeState
  EnemyStateBase <|-- EnemyFollowState
  Singleton <|-- EnemyKillTracker
  ISaveParticipant <|.. EnemyKillTracker
  CollectableItemBase <|-- EnemyLoot
  IPoolable <|.. EnemyNavAgentBase
  EnemyStateBase <|-- EnemyPrepareAttackState
  EnemyStateBase <|-- EnemySearchState
  IEnemyState <|.. EnemyStateBase
  EnemyStateBase <|-- EnemyWanderState
  IPoolable <|.. EntityDeathHandler
  IEntityHealth <|.. EntityHealth
  IPoolable <|.. EntityHealth
  Interactable <|-- EventInteractable
  ISaveParticipant <|.. FeatureUnlockService
  EnemyBaseProfile <|-- FriendlyEnemyProfile
  Singleton <|-- GameManager
  Bullet <|-- GrapplingHookBullet
  Bullet <|-- GrenadeBullet
  IInteractPopupView <|.. InteractPopupView
  Singleton <|-- InventoryManager
  ISaveParticipant <|.. InventoryManager
  IInventory <|.. InventoryManager
  CollectableItemBase <|-- ItemDrop
  IPoolable <|.. ItemDrop
  Singleton <|-- KeyBindManager
  RebindButtonBase <|-- KeyBindUI
  IEnemyBrain <|.. LinearPatrolBrain
  IBrainHitReaction <|.. LinearPatrolBrain
  EnemyBaseProfile <|-- LinearPatrolProfile
  Singleton <|-- LoadingManager
  ILoadingVisual <|.. LoadingVisualController
  RebindButtonBase <|-- MoveKeyBindUI
  Singleton <|-- NPCManager
  Bullet <|-- NormalBullet
  Singleton <|-- ObjectPoolManager
  Singleton <|-- OneTimePickupTracker
  ISaveParticipant <|.. OneTimePickupTracker
  Entity <|-- Player
  EntityHealth <|-- PlayerHealth
  ISaveParticipant <|.. PlayerHealth
  ISaveParticipant <|.. PlayerMoney
  Singleton <|-- QuestCompleteUI
  IQuestOfferView <|.. QuestDetailPanel
  Singleton <|-- QuestManager
  ISaveParticipant <|.. QuestManager
  Singleton <|-- SaveBridge
  EnemyBaseProfile <|-- ScaredEnemyProfile
  IPoolable <|.. ShellPickup
  Singleton <|-- SpawnCatalog
  NotificationView <|-- StackNotificationView
  NotificationView <|-- ToastNotificationView
  Bullet <|-- TranquilizerBullet
  Interactable <|-- TrashInteract
  QuestMarkerBase <|-- TutorialActionMarker
  QuestMarkerBase <|-- TutorialUIMarker
  Singleton <|-- UIManager
  ISaveParticipant <|.. UpgradeService
  IWeightedEntry <|.. ZoneItemEntry
  IWeightedEntry <|.. ZoneSpawnEntry
```

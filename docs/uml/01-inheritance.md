# Inheritance & interfaces

[← index](README.md)

63 types in project-internal inheritance / implementation. Unity bases (MonoBehaviour, ScriptableObject) are stereotypes.

```mermaid
classDiagram
  direction TB
  class AggressiveEnemyProfile {
    <<SO>>
  }
  class Boid {
    <<MB>>
  }
  class Bullet {
    <<MB>>
    <<abstract>>
  }
  class CollectableItemBase {
    <<MB>>
    <<abstract>>
  }
  class ConfirmationDialog {
    <<MB>>
  }
  class DamagePoint {
    <<MB>>
  }
  class DayManager {
    <<MB>>
  }
  class DialoguePanelView {
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
  class FeatureUnlockService {
  }
  class FriendlyEnemyProfile {
    <<SO>>
  }
  class GranadeBullet {
    <<MB>>
  }
  class GrapplingHookBullet {
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
  class InventoryManager {
    <<MB>>
  }
  class ItemDrop {
    <<MB>>
  }
  class LinearPatrolBrain {
  }
  class LinearPatrolProfile {
    <<SO>>
  }
  class LoadingVisualController {
    <<MB>>
  }
  class NormalBullet {
    <<MB>>
  }
  class NotificationView {
    <<MB>>
    <<abstract>>
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
  class QuestDetailPanel {
    <<MB>>
  }
  class QuestManager {
    <<MB>>
  }
  class ScaredEnemyProfile {
    <<SO>>
  }
  class ShelleController {
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
  class UpgradeService {
  }
  EnemyBaseProfile <|-- AggressiveEnemyProfile
  IPoolable <|.. Boid
  IModalPrompt <|.. ConfirmationDialog
  IDamagePopup <|.. DamagePoint
  ISaveParticipant <|.. DayManager
  IDialogueView <|.. DialoguePanelView
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
  ISaveParticipant <|.. FeatureUnlockService
  EnemyBaseProfile <|-- FriendlyEnemyProfile
  Bullet <|-- GranadeBullet
  Bullet <|-- GrapplingHookBullet
  ISaveParticipant <|.. InventoryManager
  IInventory <|.. InventoryManager
  CollectableItemBase <|-- ItemDrop
  IPoolable <|.. ItemDrop
  IEnemyBrain <|.. LinearPatrolBrain
  IBrainHitReaction <|.. LinearPatrolBrain
  EnemyBaseProfile <|-- LinearPatrolProfile
  ILoadingVisual <|.. LoadingVisualController
  Bullet <|-- NormalBullet
  ISaveParticipant <|.. OneTimePickupTracker
  Entity <|-- Player
  EntityHealth <|-- PlayerHealth
  ISaveParticipant <|.. PlayerHealth
  ISaveParticipant <|.. PlayerMoney
  IQuestOfferView <|.. QuestDetailPanel
  ISaveParticipant <|.. QuestManager
  EnemyBaseProfile <|-- ScaredEnemyProfile
  IPoolable <|.. ShelleController
  NotificationView <|-- StackNotificationView
  NotificationView <|-- ToastNotificationView
  Bullet <|-- TranquilizerBullet
  ISaveParticipant <|.. UpgradeService
```

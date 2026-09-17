using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Shows the right target field for the step's type: the TutorialActionId dropdown
    /// for Tutorial steps (pick an action, don't type a matching string), a "Target NPC
    /// Id" text field for Talk steps (also the step's default dialogue speaker — see
    /// QuestStep.EffectiveNpcId), "Cutscene Id" for Cutscene steps (plays automatically
    /// and completes itself — no dialogue needed, unlike a cutscene attached to one Talk
    /// line via QuestDialogueLine.cutsceneId), and "Target Enemy Id" for ExploreArea
    /// (hidden for Delivery/KillCount — see below).
    ///
    /// The old singular Target Quest Item Id (QuestStepData.targetQuestItemId) is no longer
    /// shown for ANY step type. Delivery instead shows a Delivery Count Mode dropdown, then
    /// ONE of two lists depending on it — never both, they're mutually exclusive ways to author
    /// the same requirement (QuestManager live-checks whichever's active against the player's
    /// actual inventory via InventoryManager.OnInventoryChanged, not a one-shot event like
    /// every other step type): CombinedTotal shows Target Quest Item Ids (plain ids, no
    /// per-item count — any mix of them, summed, must reach the step's Target Count); PerItem
    /// shows Target Quest Item Requirements (id + count pairs, each independently required),
    /// and Target Count itself is derived there (hidden — it's just the requirement list's
    /// length) rather than authored. KillCount similarly authors its enemy id(s) through
    /// Target Enemy Ids (kills of ANY listed id count toward Target Count), not the singular
    /// Target Enemy Id. QuestStepDataDrawer still reads the old singular field as a fallback
    /// for legacy-authored steps (see Quest.BuildSteps) — existing assets built before these
    /// existed don't need to be re-authored, they just can't be edited as that field anymore.
    ///
    /// Per-type field visibility (see the FieldVisibility struct below — computed once per
    /// type and shared by OnGUI/GetPropertyHeight): Talk hides Target Count (a Talk step
    /// always completes in one exchange) and Notification
    /// Images (no guide banner icon for a conversation); Tutorial hides both Dialogue and
    /// Reward (it completes off TutorialAction alone, no dialogue box, no payout); KillCount
    /// and Delivery hide Notification Images (no guide banner for a grind objective) but keep
    /// Dialogue and Reward; Cutscene shows Reward Item Id/Reward Money directly (no foldout —
    /// it has no dialogue to tuck them behind). Everything else shows the full field set.
    ///
    /// Unlocks Features only shows for Tutorial/Talk steps — those are the checkpoint types a
    /// tutorial-style quest actually uses to gate features (see FeatureUnlockService). Is Guided
    /// Sequence is no longer author-controlled: it's forced true for Tutorial (the only type
    /// TutorialGate/TutorialPromptUI restrict input for) and false everywhere else, shown
    /// disabled so the Inspector still communicates the value without inviting a mismatch.
    ///
    /// Dialogue is per-step (tucked behind a foldout, since most steps are the generic
    /// case and don't need it expanded) — each checkpoint is self-contained: its own
    /// dialogue lines (keyed by character id + DialogueType), and its own payout, paid
    /// when that step completes.
    ///
    /// The dialogue list uses a ReorderableList rather than the default array GUI: each
    /// entry has a nested "lines" array of variable size, and Unity's default array-of-array
    /// GUI doesn't reliably self-report height for that at an absolute Rect inside another
    /// custom drawer — it was causing fields below it to overlap. ReorderableList measures
    /// and draws each element itself, so it stays correct regardless of nesting.
    /// </summary>
    [CustomPropertyDrawer(typeof(QuestStepData))]
    public class QuestStepDataDrawer : PropertyDrawer
    {
        /// <summary>
        /// Which fields show for a given step type — computed once here instead of as eight
        /// separate booleans duplicated by hand across OnGUI and GetPropertyHeight.
        /// </summary>
        private readonly struct FieldVisibility
        {
            public readonly bool TargetCount, NotificationImages, Dialogue, Reward, UnlocksFeatures,
                TargetField, TargetEnemyIds, TargetQuestItemIds;

            private FieldVisibility(QuestType type)
            {
                TargetCount = type != QuestType.Talk;
                NotificationImages = type != QuestType.Talk && type != QuestType.KillCount && type != QuestType.Delivery;
                Dialogue = type != QuestType.Tutorial;
                Reward = type != QuestType.Tutorial;
                UnlocksFeatures = type == QuestType.Tutorial || type == QuestType.Talk;
                // Delivery targets item(s), not an enemy — KillCount targets enemy id(s), not
                // an item. Both use a list field instead of the singular one (see class doc).
                TargetField = type != QuestType.Delivery && type != QuestType.KillCount;
                TargetEnemyIds = type == QuestType.KillCount;
                TargetQuestItemIds = type == QuestType.Delivery;
            }

            public static FieldVisibility For(QuestType type) => new(type);
        }

        private static readonly Dictionary<string, ReorderableList> DialogueLists = new();

        // Reward's own foldout state — Dialogue reuses property.isExpanded (the step's native
        // expand flag), so Reward needs a separate slot now that the two are split apart.
        private static readonly Dictionary<string, bool> RewardFoldouts = new();

        private static bool GetRewardExpanded(SerializedProperty property) =>
            !RewardFoldouts.TryGetValue(property.propertyPath, out var expanded) || expanded;

        private static ReorderableList GetDialogueList(SerializedProperty dialogueProp)
        {
            string key = dialogueProp.serializedObject.targetObject.GetInstanceID() + ":" + dialogueProp.propertyPath;

            if (DialogueLists.TryGetValue(key, out var list))
            {
                list.serializedProperty = dialogueProp;
                return list;
            }

            list = new ReorderableList(dialogueProp.serializedObject, dialogueProp, true, true, true, true);
            list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Dialogue");
            // Draws characterId/type/lines explicitly rather than the default recursive
            // PropertyField — QuestDialogueLine.cutsceneId stays a real, working field (still
            // set/read normally, e.g. by script), just not shown in this everyday list so it
            // doesn't clutter authoring a normal line.
            list.elementHeightCallback = index =>
            {
                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                var linesProp = element.FindPropertyRelative("lines");
                float lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                return lineHeight * 2 + EditorGUI.GetPropertyHeight(linesProp, true) + EditorGUIUtility.standardVerticalSpacing;
            };
            list.drawElementCallback = (rect, index, active, focused) =>
                {
                    var element = list.serializedProperty.GetArrayElementAtIndex(index);
                    var characterIdProp = element.FindPropertyRelative("characterId");
                    var typeProp = element.FindPropertyRelative("type");
                    var linesProp = element.FindPropertyRelative("lines");
                    float lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                    rect.y += EditorGUIUtility.standardVerticalSpacing;
                    rect.height = EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(rect, characterIdProp);
                    rect.y += lineHeight;

                    EditorGUI.PropertyField(rect, typeProp);
                    rect.y += lineHeight;

                    rect.height = EditorGUI.GetPropertyHeight(linesProp, true);
                    EditorGUI.PropertyField(rect, linesProp, true);
                };

            DialogueLists[key] = list;
            return list;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var typeProp = property.FindPropertyRelative("type");
            var targetEnemyIdProp = property.FindPropertyRelative("targetEnemyId");
            var targetEnemyIdsProp = property.FindPropertyRelative("targetEnemyIds");
            var targetQuestItemIdsProp = property.FindPropertyRelative("targetQuestItemIds");
            var targetQuestItemRequirementsProp = property.FindPropertyRelative("targetQuestItemRequirements");
            var deliveryCountModeProp = property.FindPropertyRelative("deliveryCountMode");
            var rewardItemIdProp = property.FindPropertyRelative("rewardItemId");
            var rewardMoneyProp = property.FindPropertyRelative("rewardMoney");
            var unlocksFeaturesProp = property.FindPropertyRelative("unlocksFeatures");
            var type = (QuestType)typeProp.enumValueIndex;

            var rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            float lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            EditorGUI.PropertyField(rect, typeProp);
            rect.y += lineHeight;

            // A Cutscene step has almost nothing else to author — it plays and completes
            // itself off this one id, no target count/completion mode/dialogue to configure —
            // but it can still reward an item like any other step.
            if (type == QuestType.Cutscene)
            {
                EditorGUI.PropertyField(rect, targetEnemyIdProp, new GUIContent("Cutscene Id",
                    "Matches a Cutscene component's Cutscene Id in the scene. Plays automatically when this step becomes current, and completes the step when it finishes."));
                rect.y += lineHeight;

                EditorGUI.PropertyField(rect, rewardItemIdProp, new GUIContent("Reward Item Id"));
                rect.y += lineHeight;

                EditorGUI.PropertyField(rect, rewardMoneyProp, new GUIContent("Reward Money"));

                EditorGUI.EndProperty();
                return;
            }

            var tutorialActionProp = property.FindPropertyRelative("tutorialAction");
            var targetCountProp = property.FindPropertyRelative("targetCount");
            var completionModeProp = property.FindPropertyRelative("completionMode");
            var isGuidedSequenceProp = property.FindPropertyRelative("isGuidedSequence");
            var stepDescriptionProp = property.FindPropertyRelative("stepDescription");
            var notificationImagesProp = property.FindPropertyRelative("notificationImages");
            var dialogueProp = property.FindPropertyRelative("dialogue");

            // Per-type field visibility — see class doc for what each step type actually uses.
            var vis = FieldVisibility.For(type);

            if (vis.TargetField)
            {
                if (type == QuestType.Tutorial)
                    EditorGUI.PropertyField(rect, tutorialActionProp, new GUIContent("Action"));
                else if (type == QuestType.Talk)
                    EditorGUI.PropertyField(rect, targetEnemyIdProp, new GUIContent("Target NPC Id",
                        "Use \"player\" for a self-talk step — no NPC needed, the line auto-shows via PlayerSelfDialogueUI and completes on the next Interact press."));
                else
                    EditorGUI.PropertyField(rect, targetEnemyIdProp, new GUIContent("Target Enemy Id"));
                rect.y += lineHeight;
            }

            if (vis.TargetEnemyIds)
            {
                rect.height = EditorGUI.GetPropertyHeight(targetEnemyIdsProp, true);
                EditorGUI.PropertyField(rect, targetEnemyIdsProp, new GUIContent("Target Enemy Ids",
                    "Killing any enemy whose id is in this list counts toward Target Count."), true);
                rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
                rect.height = EditorGUIUtility.singleLineHeight;
            }

            // PerItem's required count comes from each requirement's own Count, not this
            // shared field — hide it there so it can't be authored and left mismatched.
            bool isDeliveryPerItem = type == QuestType.Delivery &&
                (DeliveryCountMode)deliveryCountModeProp.enumValueIndex == DeliveryCountMode.PerItem;

            if (vis.TargetQuestItemIds)
            {
                EditorGUI.PropertyField(rect, deliveryCountModeProp, new GUIContent("Delivery Count Mode"));
                rect.y += lineHeight;

                // Each mode authors a different list — a plain id has no meaningful per-item
                // count in CombinedTotal, so PerItem is the only one that shows one.
                if (isDeliveryPerItem)
                {
                    rect.height = EditorGUI.GetPropertyHeight(targetQuestItemRequirementsProp, true);
                    EditorGUI.PropertyField(rect, targetQuestItemRequirementsProp, new GUIContent("Target Quest Item Requirements",
                        "Which items are required and how many of each, independently."), true);
                }
                else
                {
                    rect.height = EditorGUI.GetPropertyHeight(targetQuestItemIdsProp, true);
                    EditorGUI.PropertyField(rect, targetQuestItemIdsProp, new GUIContent("Target Quest Item Ids",
                        "Which items count toward the shared Target Count below — any mix of these, summed, must reach it."), true);
                }
                rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
                rect.height = EditorGUIUtility.singleLineHeight;
            }

            if (vis.UnlocksFeatures)
            {
                rect.height = EditorGUI.GetPropertyHeight(unlocksFeaturesProp, true);
                EditorGUI.PropertyField(rect, unlocksFeaturesProp, new GUIContent("Unlocks Features"), true);
                rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
                rect.height = EditorGUIUtility.singleLineHeight;
            }

            if (vis.TargetCount && !isDeliveryPerItem)
            {
                EditorGUI.PropertyField(rect, targetCountProp);
                rect.y += lineHeight;
            }

            EditorGUI.PropertyField(rect, completionModeProp);
            rect.y += lineHeight;

            // Guided sequence is derived from type, not author-controlled — only a Tutorial
            // step locks player input to one required action (see TutorialGate), so it's
            // forced true there and false (and disabled) for every other type.
            isGuidedSequenceProp.boolValue = type == QuestType.Tutorial;
            using (new EditorGUI.DisabledScope(true))
                EditorGUI.PropertyField(rect, isGuidedSequenceProp, new GUIContent("Is Guided Sequence"));
            rect.y += lineHeight;

            rect.height = EditorGUI.GetPropertyHeight(stepDescriptionProp);
            EditorGUI.PropertyField(rect, stepDescriptionProp);
            rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;

            if (vis.NotificationImages)
            {
                rect.height = EditorGUI.GetPropertyHeight(notificationImagesProp, true);
                EditorGUI.PropertyField(rect, notificationImagesProp, true);
                rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
            }

            if (!vis.Dialogue && !vis.Reward)
            {
                EditorGUI.EndProperty();
                return;
            }

            if (vis.Dialogue)
            {
                rect.height = EditorGUIUtility.singleLineHeight;
                property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, "Dialogue", true);
                rect.y += lineHeight;

                if (property.isExpanded)
                {
                    EditorGUI.indentLevel++;

                    var dialogueList = GetDialogueList(dialogueProp);
                    rect.height = dialogueList.GetHeight();
                    dialogueList.DoList(rect);
                    rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;

                    EditorGUI.indentLevel--;
                }
            }

            if (vis.Reward)
            {
                rect.height = EditorGUIUtility.singleLineHeight;
                bool rewardExpanded = EditorGUI.Foldout(rect, GetRewardExpanded(property), "Reward", true);
                RewardFoldouts[property.propertyPath] = rewardExpanded;
                rect.y += lineHeight;

                if (rewardExpanded)
                {
                    EditorGUI.indentLevel++;

                    EditorGUI.PropertyField(rect, rewardItemIdProp);
                    rect.y += lineHeight;

                    EditorGUI.PropertyField(rect, rewardMoneyProp);

                    EditorGUI.indentLevel--;
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var typeProp = property.FindPropertyRelative("type");
            var type = (QuestType)typeProp.enumValueIndex;
            float lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // Cutscene: type, Cutscene Id, Reward Item Id, Reward Money.
            if (type == QuestType.Cutscene)
                return lineHeight * 4;

            var vis = FieldVisibility.For(type);

            var stepDescriptionProp = property.FindPropertyRelative("stepDescription");

            var deliveryCountModeProp = property.FindPropertyRelative("deliveryCountMode");
            bool isDeliveryPerItem = type == QuestType.Delivery &&
                (DeliveryCountMode)deliveryCountModeProp.enumValueIndex == DeliveryCountMode.PerItem;
            bool showTargetCount = vis.TargetCount && !isDeliveryPerItem;

            // type, completionMode, isGuidedSequence = 3 single lines, +1 if target field
            // shows, +1 if targetCount shows, plus Unlocks Features (Tutorial/Talk only), then
            // description. Target Enemy Ids and the Delivery Count Mode + Target Quest Item
            // Requirements pair are variable height, added separately below.
            int lineCount = 3
                + (vis.TargetField ? 1 : 0)
                + (showTargetCount ? 1 : 0);
            float height = lineHeight * lineCount
                + EditorGUI.GetPropertyHeight(stepDescriptionProp) + EditorGUIUtility.standardVerticalSpacing;

            if (vis.TargetEnemyIds)
            {
                var targetEnemyIdsProp = property.FindPropertyRelative("targetEnemyIds");
                height += EditorGUI.GetPropertyHeight(targetEnemyIdsProp, true) + EditorGUIUtility.standardVerticalSpacing;
            }

            if (vis.TargetQuestItemIds)
            {
                height += lineHeight; // the "Delivery Count Mode" dropdown

                if (isDeliveryPerItem)
                {
                    var targetQuestItemRequirementsProp = property.FindPropertyRelative("targetQuestItemRequirements");
                    height += EditorGUI.GetPropertyHeight(targetQuestItemRequirementsProp, true) + EditorGUIUtility.standardVerticalSpacing;
                }
                else
                {
                    var targetQuestItemIdsProp = property.FindPropertyRelative("targetQuestItemIds");
                    height += EditorGUI.GetPropertyHeight(targetQuestItemIdsProp, true) + EditorGUIUtility.standardVerticalSpacing;
                }
            }

            if (vis.UnlocksFeatures)
            {
                var unlocksFeaturesProp = property.FindPropertyRelative("unlocksFeatures");
                height += EditorGUI.GetPropertyHeight(unlocksFeaturesProp, true) + EditorGUIUtility.standardVerticalSpacing;
            }

            if (vis.NotificationImages)
            {
                var notificationImagesProp = property.FindPropertyRelative("notificationImages");
                height += EditorGUI.GetPropertyHeight(notificationImagesProp, true) + EditorGUIUtility.standardVerticalSpacing;
            }

            // Tutorial stops here — see the matching early-return in OnGUI.
            if (!vis.Dialogue && !vis.Reward)
                return height;

            if (vis.Dialogue)
            {
                height += lineHeight; // the "Dialogue" foldout header

                if (property.isExpanded)
                {
                    var dialogueProp = property.FindPropertyRelative("dialogue");
                    height += GetDialogueList(dialogueProp).GetHeight() + EditorGUIUtility.standardVerticalSpacing;
                }
            }

            if (vis.Reward)
            {
                height += lineHeight; // the "Reward" foldout header

                if (GetRewardExpanded(property))
                    height += lineHeight * 2; // rewardItemId, rewardMoney
            }

            return height;
        }
    }
}

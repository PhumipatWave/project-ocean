#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Game.Core;
using Game.Gameplay;
using Game.UI;

namespace Game.Editor
{
    /// <summary>Quest step Inspector that shows only the fields the step's type and delivery source use.</summary>
    // Which fields show per type is decided once in FieldVisibility. Dialogue uses a ReorderableList because
    // Unity's default nested-array GUI misreports its height inside a custom drawer and overlaps the fields below.
    [CustomPropertyDrawer(typeof(QuestStepData))]
    public class QuestStepDataDrawer : PropertyDrawer
    {
        /// <summary>Which fields show for a step type, shared by OnGUI and GetPropertyHeight so they can't disagree.</summary>
        private readonly struct FieldVisibility
        {
            public readonly bool TargetCount, NotificationImages, Dialogue, Reward, UnlocksFeatures, LocksFeatures,
                TargetField, DeliverySource, TargetEnemyIds, TargetQuestItemIds, ConsumeOnComplete, LootItemId,
                TalkCompletionMode, RequiredMoney, WaitingQuestFields;

            private FieldVisibility(QuestType type, DeliverySource deliverySource, bool autoDisplayDialogue)
            {
                bool isDelivery = type == QuestType.Delivery;
                bool isEnemyKills = isDelivery && deliverySource == Game.Gameplay.DeliverySource.EnemyKills;
                bool isInventoryHeld = isDelivery && deliverySource == Game.Gameplay.DeliverySource.InventoryHeld;
                bool isTalkNonAuto = type == QuestType.Talk && !autoDisplayDialogue;
                bool isWaitingQuest = type == QuestType.WaitingQuest;

                TargetCount = type != QuestType.Talk && !isWaitingQuest;
                NotificationImages = type != QuestType.Talk && !isDelivery;
                Dialogue = type != QuestType.Tutorial;
                Reward = type != QuestType.Tutorial;
                UnlocksFeatures = type == QuestType.Tutorial || type == QuestType.Talk || type == QuestType.ExploreArea;
                LocksFeatures = UnlocksFeatures;
                // Delivery and non-auto Talk target ids through a list field instead; WaitingQuest has no target at all.
                TargetField = !isDelivery && !isTalkNonAuto && !isWaitingQuest;
                DeliverySource = isDelivery;
                TargetEnemyIds = isEnemyKills || isTalkNonAuto;
                TargetQuestItemIds = isInventoryHeld;
                ConsumeOnComplete = isDelivery;
                LootItemId = isEnemyKills;
                TalkCompletionMode = isTalkNonAuto;
                RequiredMoney = isDelivery;
                WaitingQuestFields = isWaitingQuest;
            }

            public static FieldVisibility For(QuestType type, DeliverySource deliverySource, bool autoDisplayDialogue) =>
                new(type, deliverySource, autoDisplayDialogue);
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
            // cutsceneId is left out to keep ordinary lines uncluttered; it still works when set from script.
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

        // Talk: one group per listed NPC, plus "Other" so a line with an unknown Character Id stays visible.
        // Add/delete run after the draw pass, so a click never shifts indices the loop is still using.
        private static (string label, string characterId, List<int> indices)[] BuildDialogueGroups(
            SerializedProperty dialogueProp, List<string> npcIds)
        {
            var used = new HashSet<int>();
            var groups = new List<(string, string, List<int>)>();

            foreach (var id in npcIds)
            {
                var indices = new List<int>();
                for (int i = 0; i < dialogueProp.arraySize; i++)
                {
                    if (used.Contains(i)) continue;
                    if (dialogueProp.GetArrayElementAtIndex(i).FindPropertyRelative("characterId").stringValue == id)
                    {
                        indices.Add(i);
                        used.Add(i);
                    }
                }
                groups.Add((id, id, indices));
            }

            var other = new List<int>();
            for (int i = 0; i < dialogueProp.arraySize; i++)
                if (!used.Contains(i)) other.Add(i);
            if (other.Count > 0)
                groups.Add(("Other (Character Id doesn't match a listed NPC)", null, other));

            return groups.ToArray();
        }

        private static float DialogueEntryHeight(SerializedProperty dialogueProp, int index, bool showCharacterId)
        {
            var linesProp = dialogueProp.GetArrayElementAtIndex(index).FindPropertyRelative("lines");
            float lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            float rows = showCharacterId ? 2 : 1; // + characterId row for the "Other" group only
            return lineHeight * rows + EditorGUI.GetPropertyHeight(linesProp, true) + EditorGUIUtility.standardVerticalSpacing;
        }

        private static float GetGroupedDialogueHeight(SerializedProperty dialogueProp, List<string> npcIds)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            float height = 0f;

            foreach (var (_, characterId, indices) in BuildDialogueGroups(dialogueProp, npcIds))
            {
                height += lineHeight; // group header label
                bool showCharacterId = characterId == null;
                foreach (var i in indices)
                    height += DialogueEntryHeight(dialogueProp, i, showCharacterId);
                if (characterId != null)
                    height += lineHeight; // "+ Add line" button — the Other group has none
            }

            return height;
        }

        private static void DrawGroupedDialogue(Rect rect, SerializedProperty dialogueProp, List<string> npcIds)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            int deleteIndex = -1;
            string addForCharacterId = null;

            foreach (var (groupLabel, characterId, indices) in BuildDialogueGroups(dialogueProp, npcIds))
            {
                bool showCharacterId = characterId == null;

                rect.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(rect, groupLabel, EditorStyles.boldLabel);
                rect.y += lineHeight;

                EditorGUI.indentLevel++;
                foreach (var i in indices)
                {
                    var element = dialogueProp.GetArrayElementAtIndex(i);
                    var characterIdProp = element.FindPropertyRelative("characterId");
                    var typeProp = element.FindPropertyRelative("type");
                    var linesProp = element.FindPropertyRelative("lines");

                    var removeRect = new Rect(rect.x + rect.width - 20f, rect.y, 20f, EditorGUIUtility.singleLineHeight);
                    var rowRect = new Rect(rect.x, rect.y, rect.width - 24f, EditorGUIUtility.singleLineHeight);

                    if (showCharacterId)
                    {
                        EditorGUI.PropertyField(rowRect, characterIdProp);
                        rowRect.y += lineHeight;
                    }

                    EditorGUI.PropertyField(rowRect, typeProp);
                    rowRect.y += lineHeight;

                    rowRect.height = EditorGUI.GetPropertyHeight(linesProp, true);
                    EditorGUI.PropertyField(rowRect, linesProp, true);

                    if (GUI.Button(removeRect, "-"))
                        deleteIndex = i;

                    rect.y += DialogueEntryHeight(dialogueProp, i, showCharacterId);
                }
                EditorGUI.indentLevel--;

                if (characterId != null) // no "add" for the Other/unmatched bucket
                {
                    var addRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
                    if (GUI.Button(addRect, $"+ Add line for {characterId}"))
                        addForCharacterId = characterId;
                    rect.y += lineHeight;
                }
            }

            if (deleteIndex >= 0)
            {
                dialogueProp.DeleteArrayElementAtIndex(deleteIndex);
            }
            else if (addForCharacterId != null)
            {
                int newIndex = dialogueProp.arraySize;
                dialogueProp.arraySize++;
                var element = dialogueProp.GetArrayElementAtIndex(newIndex);
                element.FindPropertyRelative("characterId").stringValue = addForCharacterId;
                element.FindPropertyRelative("type").enumValueIndex = 0;
                element.FindPropertyRelative("lines").arraySize = 0;
                element.FindPropertyRelative("cutsceneId").stringValue = "";
            }
        }

        private static List<string> GetStringList(SerializedProperty arrayProp)
        {
            var list = new List<string>(arrayProp.arraySize);
            for (int i = 0; i < arrayProp.arraySize; i++)
                list.Add(arrayProp.GetArrayElementAtIndex(i).stringValue);
            return list;
        }

        private static bool UsesGroupedDialogue(QuestType type, SerializedProperty autoDisplayDialogueProp, SerializedProperty targetEnemyIdsProp) =>
            type == QuestType.Talk && !autoDisplayDialogueProp.boolValue && targetEnemyIdsProp.arraySize > 0;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var typeProp = property.FindPropertyRelative("type");
            var targetEnemyIdProp = property.FindPropertyRelative("targetEnemyId");
            var targetEnemyIdsProp = property.FindPropertyRelative("targetEnemyIds");
            var targetQuestItemIdProp = property.FindPropertyRelative("targetQuestItemId");
            var targetQuestItemIdsProp = property.FindPropertyRelative("targetQuestItemIds");
            var targetQuestItemRequirementsProp = property.FindPropertyRelative("targetQuestItemRequirements");
            var deliverySourceProp = property.FindPropertyRelative("deliverySource");
            var deliveryCountModeProp = property.FindPropertyRelative("deliveryCountMode");
            var consumeOnCompleteProp = property.FindPropertyRelative("consumeOnComplete");
            var requiredMoneyProp = property.FindPropertyRelative("requiredMoney");
            var requiredDaysToWaitProp = property.FindPropertyRelative("requiredDaysToWait");
            var requiredDivesThatDayProp = property.FindPropertyRelative("requiredDivesThatDay");
            var rewardItemIdProp = property.FindPropertyRelative("rewardItemId");
            var rewardMoneyProp = property.FindPropertyRelative("rewardMoney");
            var unlocksFeaturesProp = property.FindPropertyRelative("unlocksFeatures");
            var locksFeaturesProp = property.FindPropertyRelative("locksFeatures");
            var talkCompletionModeProp = property.FindPropertyRelative("talkCompletionMode");
            var type = (QuestType)typeProp.enumValueIndex;
            var deliverySource = (DeliverySource)deliverySourceProp.enumValueIndex;

            var rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            float lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            EditorGUI.PropertyField(rect, typeProp);
            rect.y += lineHeight;

            // A Cutscene step completes itself from this id; Auto Display Dialogue turns it into a press-to-advance step.
            if (type == QuestType.Cutscene)
            {
                var cutsceneAutoDisplayDialogueProp = property.FindPropertyRelative("autoDisplayDialogue");
                var cutsceneDialogueProp = property.FindPropertyRelative("dialogue");

                EditorGUI.PropertyField(rect, targetEnemyIdProp, new GUIContent("Cutscene Id",
                    "Matches a Cutscene component's Cutscene Id in the scene. Plays automatically when this step becomes current, and completes the step when it finishes. Ignored while Auto Display Dialogue below is on."));
                rect.y += lineHeight;

                EditorGUI.PropertyField(rect, cutsceneAutoDisplayDialogueProp, new GUIContent("Auto Display Dialogue",
                    "OFF (default): plays the Timeline named by Cutscene Id above automatically, no interaction. ON: shows Dialogue below instead — one line per Interact press, completing on the last line; a line's own Cutscene Id can still chain into a Timeline once dialogue closes, same as Talk."));
                rect.y += lineHeight;

                EditorGUI.PropertyField(rect, rewardItemIdProp, new GUIContent("Reward Item Id"));
                rect.y += lineHeight;

                EditorGUI.PropertyField(rect, rewardMoneyProp, new GUIContent("Reward Money"));
                rect.y += lineHeight;

                if (cutsceneAutoDisplayDialogueProp.boolValue)
                {
                    rect.height = EditorGUIUtility.singleLineHeight;
                    property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, "Dialogue", true);
                    rect.y += lineHeight;

                    if (property.isExpanded)
                    {
                        EditorGUI.indentLevel++;
                        var dialogueList = GetDialogueList(cutsceneDialogueProp);
                        rect.height = dialogueList.GetHeight();
                        dialogueList.DoList(rect);
                        EditorGUI.indentLevel--;
                    }
                }

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
            var autoDisplayDialogueProp = property.FindPropertyRelative("autoDisplayDialogue");

            // Per-type field visibility — see class doc for what each step type actually uses.
            var vis = FieldVisibility.For(type, deliverySource, autoDisplayDialogueProp.boolValue);

            if (vis.DeliverySource)
            {
                EditorGUI.PropertyField(rect, deliverySourceProp, new GUIContent("Delivery Source",
                    "InventoryHeld: progress tracks the player's current inventory. EnemyKills: progress tracks kill events matching Target Enemy Ids below (the old KillCount type)."));
                rect.y += lineHeight;
            }

            if (vis.TargetField)
            {
                if (type == QuestType.Tutorial)
                    EditorGUI.PropertyField(rect, tutorialActionProp, new GUIContent("Action"));
                else if (type == QuestType.Talk)
                {
                    // Auto Display Dialogue talks to nobody, so it has no NPC id.
                    targetEnemyIdProp.stringValue = "";
                    using (new EditorGUI.DisabledScope(true))
                        EditorGUI.PropertyField(rect, targetEnemyIdProp, new GUIContent("Target NPC Id",
                            "Not used — Auto Display Dialogue has no NPC. Leave dialogue lines' Character Id blank."));
                }
                else if (type == QuestType.ExploreArea)
                    EditorGUI.PropertyField(rect, targetEnemyIdProp, new GUIContent("Area Id",
                        "Matches an ExploreAreaTrigger's Area Id in the scene - completes when the player interacts with it or walks into its zone."));
                else
                    EditorGUI.PropertyField(rect, targetEnemyIdProp, new GUIContent("Target Enemy Id"));
                rect.y += lineHeight;
            }

            if (type == QuestType.Talk)
            {
                EditorGUI.PropertyField(rect, autoDisplayDialogueProp, new GUIContent("Auto Display Dialogue",
                    "ON: the line shows by itself when this step becomes current (self-talk via PlayerSelfDialogueUI), completes on the next Interact press. No Target NPC Id (cleared) — author its dialogue lines with a blank Character Id. OFF: the player must walk up to any NPC in Target NPC Ids below and talk — talking to ANY of them completes the step, each showing its own marker/lines."));
                rect.y += lineHeight;
            }

            if (vis.TargetEnemyIds)
            {
                bool isTalk = type == QuestType.Talk;
                rect.height = EditorGUI.GetPropertyHeight(targetEnemyIdsProp, true);
                EditorGUI.PropertyField(rect, targetEnemyIdsProp, new GUIContent(isTalk ? "Target NPC Ids" : "Target Enemy Ids",
                    isTalk ? "Which NPCs count as this step's target — see Talk Completion Mode below for whether ANY or ALL of them must be talked to. Each NPC shows its own dialogue lines — author a separate Dialogue entry per NPC, keyed by that NPC's own Character Id."
                           : "Killing any enemy whose id is in this list counts toward Target Count."), true);
                rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
                rect.height = EditorGUIUtility.singleLineHeight;
            }

            if (vis.TalkCompletionMode)
            {
                EditorGUI.PropertyField(rect, talkCompletionModeProp, new GUIContent("Talk Completion Mode",
                    "AnyOne: talking to any single listed NPC completes the step. All: every listed NPC must be talked to (Target Count is derived from the list length, not authored)."));
                rect.y += lineHeight;
            }

            if (vis.LootItemId)
            {
                EditorGUI.PropertyField(rect, targetQuestItemIdProp, new GUIContent("Target Quest Item Id (optional loot)",
                    "Parsed as an int item id. If set and Consume On Complete is on, this many (Target Count) are removed from inventory when the step completes — e.g. a bounty's hand-in proof."));
                rect.y += lineHeight;
            }

            // PerItem's required count comes from each requirement's own Count, not this
            // shared field — hide it there so it can't be authored and left mismatched.
            bool isDeliveryPerItem = vis.TargetQuestItemIds &&
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

            if (vis.ConsumeOnComplete)
            {
                EditorGUI.PropertyField(rect, consumeOnCompleteProp, new GUIContent("Consume On Complete",
                    "ON: completing this step removes the delivered/killed-for item(s) from inventory. OFF: it still completes and pays out, nothing is removed."));
                rect.y += lineHeight;
            }

            if (vis.RequiredMoney)
            {
                EditorGUI.PropertyField(rect, requiredMoneyProp, new GUIContent("Required Money",
                    "Extra money required alongside the item/kill count above (0 = none). The step won't complete until both are satisfied; Consume On Complete also governs whether this is spent on completion."));
                rect.y += lineHeight;
            }

            if (vis.WaitingQuestFields)
            {
                EditorGUI.PropertyField(rect, requiredDaysToWaitProp, new GUIContent("Required Days To Wait",
                    "How many day-advances (sleeps) must happen after this step becomes current before it can complete."));
                rect.y += lineHeight;

                EditorGUI.PropertyField(rect, requiredDivesThatDayProp, new GUIContent("Required Dives That Day",
                    "How many dives must happen on the day the wait finishes. Keep at or below DayManager.MaxDivesPerDay."));
                rect.y += lineHeight;
            }

            if (vis.UnlocksFeatures)
            {
                rect.height = EditorGUI.GetPropertyHeight(unlocksFeaturesProp, true);
                EditorGUI.PropertyField(rect, unlocksFeaturesProp, new GUIContent("Unlocks Features"), true);
                rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
                rect.height = EditorGUIUtility.singleLineHeight;
            }

            if (vis.LocksFeatures)
            {
                rect.height = EditorGUI.GetPropertyHeight(locksFeaturesProp, true);
                EditorGUI.PropertyField(rect, locksFeaturesProp, new GUIContent("Locks Features",
                    "Locked when this checkpoint completes — always wins over an unlock, however/whenever that was granted."), true);
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

            // Only Tutorial steps lock input to one action, so guided follows the type instead of being authored.
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

                    if (UsesGroupedDialogue(type, autoDisplayDialogueProp, targetEnemyIdsProp))
                    {
                        var npcIds = GetStringList(targetEnemyIdsProp);
                        rect.height = GetGroupedDialogueHeight(dialogueProp, npcIds);
                        DrawGroupedDialogue(rect, dialogueProp, npcIds);
                        rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
                    }
                    else
                    {
                        var dialogueList = GetDialogueList(dialogueProp);
                        rect.height = dialogueList.GetHeight();
                        dialogueList.DoList(rect);
                        rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
                    }

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

            // Cutscene: type, Cutscene Id, Auto Display Dialogue, Reward Item Id, Reward Money —
            // plus the Dialogue foldout (+ expanded list) when Auto Display Dialogue is on.
            if (type == QuestType.Cutscene)
            {
                float cutsceneHeight = lineHeight * 5;
                var cutsceneAutoDisplayDialogueProp = property.FindPropertyRelative("autoDisplayDialogue");
                if (cutsceneAutoDisplayDialogueProp.boolValue)
                {
                    cutsceneHeight += lineHeight;
                    if (property.isExpanded)
                    {
                        var cutsceneDialogueProp = property.FindPropertyRelative("dialogue");
                        cutsceneHeight += GetDialogueList(cutsceneDialogueProp).GetHeight() + EditorGUIUtility.standardVerticalSpacing;
                    }
                }
                return cutsceneHeight;
            }

            var deliverySourceProp = property.FindPropertyRelative("deliverySource");
            var deliverySource = (DeliverySource)deliverySourceProp.enumValueIndex;
            var autoDisplayDialogueProp = property.FindPropertyRelative("autoDisplayDialogue");
            var vis = FieldVisibility.For(type, deliverySource, autoDisplayDialogueProp.boolValue);

            var stepDescriptionProp = property.FindPropertyRelative("stepDescription");

            var deliveryCountModeProp = property.FindPropertyRelative("deliveryCountMode");
            bool isDeliveryPerItem = vis.TargetQuestItemIds &&
                (DeliveryCountMode)deliveryCountModeProp.enumValueIndex == DeliveryCountMode.PerItem;
            bool showTargetCount = vis.TargetCount && !isDeliveryPerItem;

            // Fixed rows plus each optional row; variable-height lists are added below.
            int lineCount = 3
                + (vis.DeliverySource ? 1 : 0)
                + (vis.TargetField ? 1 : 0)
                + (vis.LootItemId ? 1 : 0)
                + (vis.ConsumeOnComplete ? 1 : 0)
                + (vis.RequiredMoney ? 1 : 0)
                + (vis.WaitingQuestFields ? 2 : 0)
                + (vis.TalkCompletionMode ? 1 : 0)
                + (type == QuestType.Talk ? 1 : 0) // Auto Display Dialogue
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

            if (vis.LocksFeatures)
            {
                var locksFeaturesProp = property.FindPropertyRelative("locksFeatures");
                height += EditorGUI.GetPropertyHeight(locksFeaturesProp, true) + EditorGUIUtility.standardVerticalSpacing;
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
                    var targetEnemyIdsProp = property.FindPropertyRelative("targetEnemyIds");

                    height += (UsesGroupedDialogue(type, autoDisplayDialogueProp, targetEnemyIdsProp)
                        ? GetGroupedDialogueHeight(dialogueProp, GetStringList(targetEnemyIdsProp))
                        : GetDialogueList(dialogueProp).GetHeight())
                        + EditorGUIUtility.standardVerticalSpacing;
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
#endif

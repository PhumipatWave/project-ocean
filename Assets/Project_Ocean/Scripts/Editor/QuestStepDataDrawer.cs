#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Shows the right target field for the step's type: the TutorialActionId dropdown
    /// for Tutorial steps (pick an action, don't type a matching string), "Cutscene Id"
    /// for Cutscene steps (plays automatically and completes itself — no dialogue needed,
    /// unlike a cutscene attached to one Talk line via QuestDialogueLine.cutsceneId), and
    /// "Target Enemy Id" for ExploreArea (hidden for Delivery — see below). Talk (non-auto)
    /// shows a "Target NPC Ids" list instead of a singular field — talking to ANY NPC in it
    /// completes the step, and each shows its own dialogue (author one Dialogue entry per
    /// NPC, keyed by that NPC's own Character Id; see QuestStep.EffectiveNpcIds). Talk+auto
    /// keeps the old singular field, disabled and cleared — Auto Display Dialogue talks to
    /// nobody, so there's nothing to list.
    ///
    /// Delivery shows a Delivery Source dropdown first (InventoryHeld/EnemyKills — see
    /// DeliverySource; EnemyKills replaces the old, now-removed KillCount QuestType, since it
    /// shared everything else with Delivery). InventoryHeld then shows a Delivery Count Mode
    /// dropdown, then ONE of two lists depending on it — never both, they're mutually exclusive
    /// ways to author the same requirement (QuestManager live-checks whichever's active against
    /// the player's actual inventory via InventoryManager.OnInventoryChanged, not a one-shot
    /// event like every other step type): CombinedTotal shows Target Quest Item Ids (plain ids,
    /// no per-item count — any mix of them, summed, must reach the step's Target Count); PerItem
    /// shows Target Quest Item Requirements (id + count pairs, each independently required), and
    /// Target Count itself is derived there (hidden — it's just the requirement list's length)
    /// rather than authored. EnemyKills instead shows Target Enemy Ids (kills of ANY listed id
    /// count toward Target Count) plus the singular Target Quest Item Id as an optional hand-in
    /// loot item. Either Delivery source shows a Consume On Complete toggle — OFF skips removing
    /// the delivered/killed-for item(s) when the step completes.
    ///
    /// Per-type field visibility (see the FieldVisibility struct below — computed once per
    /// type/source and shared by OnGUI/GetPropertyHeight): Talk hides Target Count (a Talk step
    /// always completes in one exchange) and Notification
    /// Images (no guide banner icon for a conversation); Tutorial hides both Dialogue and
    /// Reward (it completes off TutorialAction alone, no dialogue box, no payout); Delivery
    /// hides Notification Images (no guide banner for a grind objective) but keeps Dialogue and
    /// Reward; Cutscene shows Reward Item Id/Reward Money directly (no foldout —
    /// it has no dialogue to tuck them behind). Everything else shows the full field set.
    ///
    /// Unlocks Features and Locks Features show for Tutorial/Talk/ExploreArea steps — the
    /// checkpoint types a quest actually uses to gate features (see FeatureUnlockService; a
    /// lock always wins over an unlock, however/whenever granted). Is Guided
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
            public readonly bool TargetCount, NotificationImages, Dialogue, Reward, UnlocksFeatures, LocksFeatures,
                TargetField, DeliverySource, TargetEnemyIds, TargetQuestItemIds, ConsumeOnComplete, LootItemId,
                TalkCompletionMode, RequiredMoney;

            private FieldVisibility(QuestType type, DeliverySource deliverySource, bool autoDisplayDialogue)
            {
                bool isDelivery = type == QuestType.Delivery;
                bool isEnemyKills = isDelivery && deliverySource == Game.Testing.DeliverySource.EnemyKills;
                bool isInventoryHeld = isDelivery && deliverySource == Game.Testing.DeliverySource.InventoryHeld;
                bool isTalkNonAuto = type == QuestType.Talk && !autoDisplayDialogue;

                TargetCount = type != QuestType.Talk;
                NotificationImages = type != QuestType.Talk && !isDelivery;
                Dialogue = type != QuestType.Tutorial;
                Reward = type != QuestType.Tutorial;
                UnlocksFeatures = type == QuestType.Tutorial || type == QuestType.Talk || type == QuestType.ExploreArea;
                LocksFeatures = UnlocksFeatures;
                // Delivery and Talk (non-auto-display) target item/enemy/NPC id(s) through a
                // list field instead of the singular one (see class doc) — EnemyKills still
                // uses the singular Target Quest Item Id, but only as the optional hand-in loot
                // (LootItemId below). Talk+auto keeps showing the singular field (disabled,
                // cleared) same as always — it talks to nobody, there's nothing to list.
                TargetField = !isDelivery && !isTalkNonAuto;
                DeliverySource = isDelivery;
                TargetEnemyIds = isEnemyKills || isTalkNonAuto;
                TargetQuestItemIds = isInventoryHeld;
                ConsumeOnComplete = isDelivery;
                LootItemId = isEnemyKills;
                TalkCompletionMode = isTalkNonAuto;
                RequiredMoney = isDelivery;
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

        // ── Talk (non-auto) grouped dialogue editor ──
        //
        // Talk steps with a populated Target NPC Ids list replace the single flat
        // ReorderableList above with this: one group per listed NPC (Character Id implied by
        // the group, not shown per-row), plus a trailing "Other" group for any entry whose
        // Character Id doesn't match a listed NPC (so a stray/renamed id is still visible and
        // editable instead of silently vanishing from view). The underlying `dialogue` field
        // itself is untouched — still one flat List<QuestDialogueLine>, same as every other
        // step type — this only changes how it's grouped/rendered for Talk.
        //
        // No drag-reorder within a group: lookup is by (characterId, type) — see
        // QuestStep.GetLine/GetLines — so list position carries no meaning here, unlike
        // Auto Display Dialogue's Offer sequence, which is why a plain add/remove button pair
        // is enough (a full ReorderableList per group would be pure overhead).
        //
        // A pending delete/add is applied AFTER the full draw pass, not mid-loop, so a click
        // never mutates the array indices the rest of that same pass is still iterating over —
        // the next repaint recomputes fresh groups from the now-mutated array.
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
                    // Only reachable when auto is true — Talk+non-auto hides TargetField in
                    // favor of the Target NPC Ids list below (see FieldVisibility). Auto Display
                    // Dialogue talks to nobody — no NPC id, and its lines are authored with a
                    // blank characterId (see QuestStep.EffectiveNpcId).
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

            // Cutscene: type, Cutscene Id, Reward Item Id, Reward Money.
            if (type == QuestType.Cutscene)
                return lineHeight * 4;

            var deliverySourceProp = property.FindPropertyRelative("deliverySource");
            var deliverySource = (DeliverySource)deliverySourceProp.enumValueIndex;
            var autoDisplayDialogueProp = property.FindPropertyRelative("autoDisplayDialogue");
            var vis = FieldVisibility.For(type, deliverySource, autoDisplayDialogueProp.boolValue);

            var stepDescriptionProp = property.FindPropertyRelative("stepDescription");

            var deliveryCountModeProp = property.FindPropertyRelative("deliveryCountMode");
            bool isDeliveryPerItem = vis.TargetQuestItemIds &&
                (DeliveryCountMode)deliveryCountModeProp.enumValueIndex == DeliveryCountMode.PerItem;
            bool showTargetCount = vis.TargetCount && !isDeliveryPerItem;

            // type, completionMode, isGuidedSequence = 3 single lines, +1 if delivery source
            // shows, +1 if target field shows, +1 if loot item id shows, +1 if consume toggle
            // shows, +1 if targetCount shows, plus Unlocks/Locks Features (Tutorial/Talk/
            // ExploreArea only, variable height below), then description. Target Enemy Ids and
            // the Delivery Count Mode + Target Quest Item Requirements pair are variable height
            // too, added separately below.
            int lineCount = 3
                + (vis.DeliverySource ? 1 : 0)
                + (vis.TargetField ? 1 : 0)
                + (vis.LootItemId ? 1 : 0)
                + (vis.ConsumeOnComplete ? 1 : 0)
                + (vis.RequiredMoney ? 1 : 0)
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

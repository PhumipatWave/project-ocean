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
    /// line via QuestDialogueLine.cutsceneId), and "Target Enemy Id" for KillCount/Delivery/ExploreArea.
    ///
    /// Target Quest Item Id (QuestStepData.targetQuestItemId — the item removed from
    /// inventory when this checkpoint completes) shows for every step type, Cutscene
    /// included, right after the type-specific target field.
    ///
    /// Per-type field visibility (see showTargetCount/showNotificationImages/showDialogueReward/
    /// showUnlocksFeatures in OnGUI/GetPropertyHeight — keep both in sync): Talk hides Target
    /// Count (a Talk step always completes in one exchange) and Notification Images (no guide
    /// banner icon for a conversation); Tutorial hides the whole Dialogue & Reward foldout (it
    /// completes off TutorialAction alone, no dialogue box) but keeps Notification Images (used
    /// by the guide banner); Cutscene shows Reward Item Id/Reward Money directly (no foldout —
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
        private static readonly Dictionary<string, ReorderableList> DialogueLists = new();

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
            var targetQuestItemIdProp = property.FindPropertyRelative("targetQuestItemId");
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
            // but it can still target/reward an item like any other step.
            if (type == QuestType.Cutscene)
            {
                EditorGUI.PropertyField(rect, targetEnemyIdProp, new GUIContent("Cutscene Id",
                    "Matches a Cutscene component's Cutscene Id in the scene. Plays automatically when this step becomes current, and completes the step when it finishes."));
                rect.y += lineHeight;

                EditorGUI.PropertyField(rect, targetQuestItemIdProp, new GUIContent("Target Quest Item Id"));
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
            bool showTargetCount = type != QuestType.Talk;
            bool showNotificationImages = type != QuestType.Talk;
            bool showDialogueReward = type != QuestType.Tutorial;
            bool showUnlocksFeatures = type == QuestType.Tutorial || type == QuestType.Talk;

            if (type == QuestType.Tutorial)
                EditorGUI.PropertyField(rect, tutorialActionProp, new GUIContent("Action"));
            else if (type == QuestType.Talk)
                EditorGUI.PropertyField(rect, targetEnemyIdProp, new GUIContent("Target NPC Id",
                    "Use \"player\" for a self-talk step — no NPC needed, the line auto-shows via PlayerSelfDialogueUI and completes on the next Interact press."));
            else
                EditorGUI.PropertyField(rect, targetEnemyIdProp, new GUIContent("Target Enemy Id"));
            rect.y += lineHeight;

            EditorGUI.PropertyField(rect, targetQuestItemIdProp, new GUIContent("Target Quest Item Id"));
            rect.y += lineHeight;

            if (showUnlocksFeatures)
            {
                rect.height = EditorGUI.GetPropertyHeight(unlocksFeaturesProp, true);
                EditorGUI.PropertyField(rect, unlocksFeaturesProp, new GUIContent("Unlocks Features"), true);
                rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
                rect.height = EditorGUIUtility.singleLineHeight;
            }

            if (showTargetCount)
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

            if (showNotificationImages)
            {
                rect.height = EditorGUI.GetPropertyHeight(notificationImagesProp, true);
                EditorGUI.PropertyField(rect, notificationImagesProp, true);
                rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
            }

            if (!showDialogueReward)
            {
                EditorGUI.EndProperty();
                return;
            }

            rect.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, "Dialogue & Reward", true);
            rect.y += lineHeight;

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                var dialogueList = GetDialogueList(dialogueProp);
                rect.height = dialogueList.GetHeight();
                dialogueList.DoList(rect);
                rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;

                rect.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(rect, rewardItemIdProp);
                rect.y += lineHeight;

                EditorGUI.PropertyField(rect, rewardMoneyProp);

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var typeProp = property.FindPropertyRelative("type");
            var type = (QuestType)typeProp.enumValueIndex;
            float lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // Cutscene: type, Cutscene Id, Target Quest Item Id, Reward Item Id, Reward Money.
            if (type == QuestType.Cutscene)
                return lineHeight * 5;

            bool showTargetCount = type != QuestType.Talk;
            bool showNotificationImages = type != QuestType.Talk;
            bool showDialogueReward = type != QuestType.Tutorial;
            bool showUnlocksFeatures = type == QuestType.Tutorial || type == QuestType.Talk;

            var stepDescriptionProp = property.FindPropertyRelative("stepDescription");

            // type, target, targetQuestItemId, completionMode, isGuidedSequence = 5 single
            // lines (+1 if targetCount shows), plus Unlocks Features (Tutorial/Talk only), then description.
            float height = lineHeight * (showTargetCount ? 6 : 5)
                + EditorGUI.GetPropertyHeight(stepDescriptionProp) + EditorGUIUtility.standardVerticalSpacing;

            if (showUnlocksFeatures)
            {
                var unlocksFeaturesProp = property.FindPropertyRelative("unlocksFeatures");
                height += EditorGUI.GetPropertyHeight(unlocksFeaturesProp, true) + EditorGUIUtility.standardVerticalSpacing;
            }

            if (showNotificationImages)
            {
                var notificationImagesProp = property.FindPropertyRelative("notificationImages");
                height += EditorGUI.GetPropertyHeight(notificationImagesProp, true) + EditorGUIUtility.standardVerticalSpacing;
            }

            // Tutorial stops here — see the matching early-return in OnGUI.
            if (!showDialogueReward)
                return height;

            height += lineHeight; // the "Dialogue & Reward" foldout header

            if (property.isExpanded)
            {
                var dialogueProp = property.FindPropertyRelative("dialogue");

                height += GetDialogueList(dialogueProp).GetHeight() + EditorGUIUtility.standardVerticalSpacing;
                height += lineHeight * 2; // rewardItemId, rewardMoney
            }

            return height;
        }
    }
}

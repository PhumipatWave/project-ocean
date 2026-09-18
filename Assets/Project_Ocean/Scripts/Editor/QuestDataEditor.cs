using UnityEditor;
using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Groups QuestData's flat fields into labeled foldout sections instead of one long
    /// list. Presentation only — no serialized data changes. Identity/Steps default open
    /// (what you edit per-quest). All NPC identity, dialogue (accept, in-progress, turn-in)
    /// and reward now live inside each Steps entry (via QuestStepDataDrawer) — a step is
    /// fully self-contained, including the offer conversation, which only ever applies to
    /// the first step in practice (a quest is never New once past step 0).
    /// </summary>
    [CustomEditor(typeof(QuestData))]
    public class QuestDataEditor : Editor
    {
        private bool showIdentity = true;
        private bool showSteps = true;

        private SerializedProperty questId, questName, category, classification, status;
        private SerializedProperty nextQuestMode, nextQuestIds;
        private SerializedProperty steps;

        private void OnEnable()
        {
            questId = serializedObject.FindProperty("questId");
            questName = serializedObject.FindProperty("questName");
            category = serializedObject.FindProperty("category");
            classification = serializedObject.FindProperty("classification");
            status = serializedObject.FindProperty("status");

            nextQuestMode = serializedObject.FindProperty("nextQuestMode");
            nextQuestIds = serializedObject.FindProperty("nextQuestIds");

            steps = serializedObject.FindProperty("steps");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            showIdentity = EditorGUILayout.Foldout(showIdentity, "Identity", true);
            if (showIdentity)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(questId);
                EditorGUILayout.PropertyField(questName);
                EditorGUILayout.PropertyField(category);
                EditorGUILayout.PropertyField(classification);
                EditorGUILayout.PropertyField(status);
                EditorGUILayout.PropertyField(nextQuestMode);
                if ((NextQuestMode)nextQuestMode.enumValueIndex != NextQuestMode.None)
                    EditorGUILayout.PropertyField(nextQuestIds, true);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            showSteps = EditorGUILayout.Foldout(showSteps, "Steps (Checkpoints)", true);
            if (showSteps)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(steps, true);
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}

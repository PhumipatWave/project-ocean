using UnityEditor;
using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Shows the right target field for the step's type: the TutorialActionId dropdown
    /// for Tutorial steps (pick an action, don't type a matching string), a "Target NPC
    /// Id" text field for Talk steps, and "Target Enemy Id" for KillCount/Delivery/ExploreArea.
    /// </summary>
    [CustomPropertyDrawer(typeof(QuestStepData))]
    public class QuestStepDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var typeProp = property.FindPropertyRelative("type");
            var targetEnemyIdProp = property.FindPropertyRelative("targetEnemyId");
            var tutorialActionProp = property.FindPropertyRelative("tutorialAction");
            var targetCountProp = property.FindPropertyRelative("targetCount");
            var completionModeProp = property.FindPropertyRelative("completionMode");
            var stepDescriptionProp = property.FindPropertyRelative("stepDescription");

            var type = (QuestType)typeProp.enumValueIndex;

            var rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            float lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            EditorGUI.PropertyField(rect, typeProp);
            rect.y += lineHeight;

            if (type == QuestType.Tutorial)
                EditorGUI.PropertyField(rect, tutorialActionProp, new GUIContent("Action"));
            else if (type == QuestType.Talk)
                EditorGUI.PropertyField(rect, targetEnemyIdProp, new GUIContent("Target NPC Id"));
            else
                EditorGUI.PropertyField(rect, targetEnemyIdProp, new GUIContent("Target Enemy Id"));
            rect.y += lineHeight;

            EditorGUI.PropertyField(rect, targetCountProp);
            rect.y += lineHeight;

            EditorGUI.PropertyField(rect, completionModeProp);
            rect.y += lineHeight;

            rect.height = EditorGUI.GetPropertyHeight(stepDescriptionProp);
            EditorGUI.PropertyField(rect, stepDescriptionProp);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var stepDescriptionProp = property.FindPropertyRelative("stepDescription");
            float lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // type, target, targetCount, completionMode = 4 single lines, plus the multi-line description field.
            return lineHeight * 4 + EditorGUI.GetPropertyHeight(stepDescriptionProp);
        }
    }
}

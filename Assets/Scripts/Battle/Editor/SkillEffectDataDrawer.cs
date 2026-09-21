using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SkillClassificationEffectData))]
[CustomPropertyDrawer(typeof(SkillHitEffectData))]
public class SkillEffectDataDrawer : PropertyDrawer
{
    private static float LineHeight => EditorGUIUtility.singleLineHeight;
    private static float Spacing => EditorGUIUtility.standardVerticalSpacing;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
            return LineHeight;

        bool mixed = property.FindPropertyRelative("category").hasMultipleDifferentValues;
        int rows = property.FindPropertyRelative("optionValue") != null ? 6 : 4;
        return rows * LineHeight + (rows - 1) * Spacing
            + (mixed ? LineHeight : 0f);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        int originalIndent = EditorGUI.indentLevel;
        try
        {
            Rect row = new Rect(position.x, position.y, position.width, LineHeight);
            property.isExpanded = EditorGUI.Foldout(row, property.isExpanded, label, true);
            if (!property.isExpanded)
                return;

            EditorGUI.indentLevel++;
            DrawField(ref row, property, "target", "Target");
            DrawField(ref row, property, "category", "Category");

            SerializedProperty category = property.FindPropertyRelative("category");
            if (category.hasMultipleDifferentValues)
            {
                row.y += LineHeight + Spacing;
                row.height = LineHeight * 2f;
                EditorGUI.HelpBox(row, "Select a common Category to edit the effect.", MessageType.Info);
                row.y += LineHeight;
                row.height = LineHeight;
            }
            else
            {
                switch ((EffectCategoryType)category.intValue)
                {
                    case EffectCategoryType.Damage:
                        DrawField(ref row, property, "damageType", "Damage Effect");
                        break;
                    case EffectCategoryType.Buff:
                        DrawField(ref row, property, "buffType", "Buff Effect");
                        break;
                    case EffectCategoryType.Debuff:
                        DrawField(ref row, property, "debuffType", "Debuff Effect");
                        break;
                    case EffectCategoryType.Unique:
                        DrawField(ref row, property, "uniqueType", "Unique Effect");
                        break;
                    default:
                        row.y += LineHeight + Spacing;
                        EditorGUI.LabelField(row, "Unknown Category");
                        break;
                }
            }

            if (property.FindPropertyRelative("optionValue") != null)
            {
                DrawField(ref row, property, "optionValue", "Option Value");
                DrawField(ref row, property, "optionGrowthPerLevel", "Option Growth Per Level");
            }
        }
        finally
        {
            EditorGUI.indentLevel = originalIndent;
            EditorGUI.EndProperty();
        }
    }

    private static void DrawField(
        ref Rect row, SerializedProperty property, string fieldName, string label)
    {
        row.y += LineHeight + Spacing;
        EditorGUI.PropertyField(row, property.FindPropertyRelative(fieldName), new GUIContent(label));
    }
}

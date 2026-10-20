using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EquipmentData))]
public class EquipmentDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EquipmentDefinitionInspector.DrawProperties(serializedObject);
        serializedObject.ApplyModifiedProperties();
        var data = (EquipmentData)target;
        EquipmentDefinitionInspector.IdentityWarnings(data.equipmentCode, data.equipmentEnglishName);
        if (!EquipmentSlotUtility.IsValid(data.slot))
            EditorGUILayout.HelpBox("장착 부위를 신규 9종 중에서 지정하세요. 구형 부위 값은 자동 변환하지 않습니다.", MessageType.Warning);
    }
}

[CustomEditor(typeof(ItemData))]
public class ItemDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EquipmentDefinitionInspector.DrawProperties(serializedObject);
        serializedObject.ApplyModifiedProperties();
        var data = (ItemData)target;
        EquipmentDefinitionInspector.IdentityWarnings(data.itemCode, data.itemEnglishName);
    }
}

[CustomEditor(typeof(EquipmentSetData))]
public class EquipmentSetDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var seen = new HashSet<int>();
        var bonuses = ((EquipmentSetData)target).bonuses;
        if (bonuses == null) return;
        foreach (var bonus in bonuses)
        {
            if (bonus == null || bonus.requiredEquipmentCount < 1)
                EditorGUILayout.HelpBox("세트 단계의 필요 장착 수는 1 이상이어야 합니다.", MessageType.Warning);
            else if (!seen.Add(bonus.requiredEquipmentCount))
                EditorGUILayout.HelpBox($"필요 장착 수 {bonus.requiredEquipmentCount} 단계가 중복되었습니다.", MessageType.Warning);
        }
    }
}

internal static class EquipmentDefinitionInspector
{
    private static readonly Regex PascalCase = new Regex("^[A-Z][A-Za-z0-9]*$");
    internal static void DrawProperties(SerializedObject source)
    {
        var property = source.GetIterator();
        for (bool enter = true; property.NextVisible(enter); enter = false)
        {
            if (property.name == "rank")
                property.intValue = EditorGUILayout.IntField("Rank", property.intValue);
            else
            {
                using (new EditorGUI.DisabledScope(property.name == "m_Script"))
                    EditorGUILayout.PropertyField(property, true);
            }
        }
    }
    internal static void IdentityWarnings(int code, string englishName)
    {
        if (code < 0) EditorGUILayout.HelpBox("코드가 미지정입니다. 스프레드시트의 정수 코드를 입력하세요 (000 = 0).", MessageType.Warning);
        if (string.IsNullOrEmpty(englishName) || !PascalCase.IsMatch(englishName))
            EditorGUILayout.HelpBox("영문명은 GreenLeatherBracelet처럼 PascalCase로 입력하세요.", MessageType.Warning);
    }
}

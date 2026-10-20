using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemEquipmentRegistry))]
public sealed class ItemEquipmentRegistryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        foreach (var warning in ((ItemEquipmentRegistry)target).GetValidationWarnings())
            EditorGUILayout.HelpBox(warning, MessageType.Warning);
    }
}

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(StageDatabase))]
public class StageDatabaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        StageDatabase database = (StageDatabase)target;
        var warnings = database.GetValidationWarnings();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Stage Validation", EditorStyles.boldLabel);
        if (warnings.Count == 0)
        {
            EditorGUILayout.HelpBox("등록된 스테이지에서 검증 문제가 발견되지 않았습니다.", MessageType.Info);
            return;
        }

        foreach (string warning in warnings)
            EditorGUILayout.HelpBox(warning, MessageType.Warning);
    }
}

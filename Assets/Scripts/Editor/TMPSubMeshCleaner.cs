using TMPro;
using UnityEditor;
using UnityEngine;

public static class TMPSubMeshCleaner
{
    [MenuItem("Tools/Slayers/Clean TMP SubMeshUI")]
    private static void CleanSelected()
    {
        GameObject selected = Selection.activeGameObject;

        if (selected == null)
        {
            Debug.LogWarning("[TMPSubMeshCleaner] 정리할 GameObject를 선택하세요.");
            return;
        }

        TextMeshProUGUI[] texts =
            selected.GetComponentsInChildren<TextMeshProUGUI>(true);

        int removedCount = 0;
        int textCount = 0;

        foreach (TextMeshProUGUI text in texts)
        {
            if (text == null)
                continue;

            textCount++;

            TMP_SubMeshUI[] subMeshes =
                text.GetComponentsInChildren<TMP_SubMeshUI>(true);

            foreach (TMP_SubMeshUI subMesh in subMeshes)
            {
                if (subMesh == null)
                    continue;

                Undo.DestroyObjectImmediate(subMesh.gameObject);
                removedCount++;
            }

            // 현재 텍스트 상태를 기준으로 TMP가 필요한 Mesh/SubMesh를 다시 계산한다.
            text.SetVerticesDirty();
            text.SetLayoutDirty();
            text.ForceMeshUpdate(true, true);

            EditorUtility.SetDirty(text);
        }

        Debug.Log(
            $"[TMPSubMeshCleaner] 완료 - TMP {textCount}개 검사 / TMP_SubMeshUI {removedCount}개 제거"
        );
    }

    [MenuItem("Tools/Slayers/Clean TMP SubMeshUI", true)]
    private static bool ValidateCleanSelected()
    {
        return Selection.activeGameObject != null;
    }
}
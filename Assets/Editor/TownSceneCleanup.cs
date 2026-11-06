using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// One-off Town cleanup before moving panel controllers under [System].
// Report never modifies the scene; Apply records Undo and leaves saving to the user.
public static class TownSceneCleanup
{
    private const string TownScenePath = "Assets/Scenes/Town.unity";
    private const string StrayMethod = nameof(TownMenuController.OpenBattle);

    [MenuItem("Tools/Slayers/Town Cleanup/Report")]
    public static void Report() => Run(false);

    [MenuItem("Tools/Slayers/Town Cleanup/Apply")]
    public static void Apply()
    {
        if (!EditorUtility.DisplayDialog("Town Cleanup",
                "Remove unreferenced TownTitleController components and persistent " +
                $"TownMenuController.{StrayMethod} calls on Town buttons?\n\nRun Report first. The scene is not saved automatically.",
                "Apply", "Cancel"))
            return;
        Run(true);
    }

    private static void Run(bool apply)
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Town Cleanup] Exit Play Mode first."); return; }
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != TownScenePath)
        {
            if (scene.isDirty) { Debug.LogError("[Town Cleanup] Save or discard the active scene first."); return; }
            scene = EditorSceneManager.OpenScene(TownScenePath);
        }

        var behaviours = new List<MonoBehaviour>();
        foreach (GameObject root in scene.GetRootGameObjects())
            behaviours.AddRange(root.GetComponentsInChildren<MonoBehaviour>(true));

        var log = new StringBuilder(apply ? "[Town Cleanup] Applied\n" : "[Town Cleanup] Report (no changes)\n");
        int titles = CleanTitles(behaviours, apply, log);
        int calls = CleanStrayButtonCalls(behaviours, apply, log);
        log.Insert(0, $"Titles removed: {titles}, button calls removed: {calls}\n");
        Debug.Log(log.ToString());

        if (apply && (titles > 0 || calls > 0))
            EditorSceneManager.MarkSceneDirty(scene);
    }

    private static int CleanTitles(List<MonoBehaviour> behaviours, bool apply, StringBuilder log)
    {
        var referenced = new HashSet<Object>();
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour == null) continue;
            SerializedProperty property = new SerializedObject(behaviour).GetIterator();
            while (property.Next(true))
                if (property.propertyType == SerializedPropertyType.ObjectReference &&
                    property.objectReferenceValue is TownTitleController)
                    referenced.Add(property.objectReferenceValue);
        }

        log.AppendLine("Kept TownTitleController:");
        int removed = 0;
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is not TownTitleController title) continue;
            if (referenced.Contains(title)) { log.AppendLine("  " + PathOf(title)); continue; }

            // Components that belong to a prefab asset cannot be removed from the instance.
            if (PrefabUtility.IsPartOfPrefabInstance(title) && !PrefabUtility.IsAddedComponentOverride(title))
            {
                log.AppendLine("  [prefab, skipped] " + PathOf(title));
                continue;
            }

            removed++;
            if (apply) Undo.DestroyObjectImmediate(title);
        }

        if (referenced.Count != 1)
            log.AppendLine($"  WARNING: expected one referenced title, found {referenced.Count}.");
        return removed;
    }

    private static int CleanStrayButtonCalls(List<MonoBehaviour> behaviours, bool apply, StringBuilder log)
    {
        log.AppendLine($"Buttons with persistent TownMenuController.{StrayMethod}:");
        int removed = 0;
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is not Button button) continue;
            bool touched = false;
            for (int i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
            {
                if (button.onClick.GetPersistentTarget(i) is not TownMenuController ||
                    button.onClick.GetPersistentMethodName(i) != StrayMethod)
                    continue;

                log.AppendLine("  " + PathOf(button));
                removed++;
                if (!apply) continue;
                if (!touched) { Undo.RecordObject(button, "Town Cleanup"); touched = true; }
                UnityEventTools.RemovePersistentListener(button.onClick, i);
            }

            if (touched && PrefabUtility.IsPartOfPrefabInstance(button))
                PrefabUtility.RecordPrefabInstancePropertyModifications(button);
        }
        return removed;
    }

    private static string PathOf(Component component)
    {
        Transform t = component.transform;
        string path = t.name;
        while ((t = t.parent) != null) path = t.name + "/" + path;
        return path;
    }
}

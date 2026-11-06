using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Moves Town objects/components under [System] while keeping every serialized reference.
// Report never modifies the scene; Apply records one Undo group and leaves saving to the user.
// Later panel steps add their own entries to Steps.
public static class TownSystemRestructure
{
    private const string TownScenePath = "Assets/Scenes/Town.unity";
    private const string SystemRootName = "[System]";

    private sealed class Context
    {
        public bool Apply;
        public Transform System;
        public List<MonoBehaviour> Behaviours;
        public readonly StringBuilder Log = new();
        public int Changes;
    }

    // Step 2: group the existing [System] children. Objects are located by component type, not name.
    private static void SystemGroups(Context c)
    {
        MoveComponent<StageManager>(c, "Managers");
        MoveComponent<SceneLoader>(c, "Managers");
        Reparent<InputBindingManager>(c, "Managers");
        Reparent<ItemEquipmentRegistry>(c, "Data");
        Reparent<IconRegistry>(c, "Data");
        // Holds the Town preview CharacterStats; its TownCombatCharacterSelectController moves with the Battle panel step.
        Reparent<CharacterStats>(c, "Data");
        Reparent<TownMenuController>(c, "TownUI");
        Reparent<TownTestMenu>(c, "Debug");
    }

    [MenuItem("Tools/Slayers/Town Restructure/Step 2 - System Groups (Report)")]
    public static void ReportSystemGroups() => Run(SystemGroups, false);

    [MenuItem("Tools/Slayers/Town Restructure/Step 2 - System Groups (Apply)")]
    public static void ApplySystemGroups()
    {
        if (EditorUtility.DisplayDialog("Town Restructure",
                "Group [System] children into Managers / Data / TownUI / Debug?\n\nRun Report first. The scene is not saved automatically.",
                "Apply", "Cancel"))
            Run(SystemGroups, true);
    }

    private static void Run(Action<Context> step, bool apply)
    {
        if (EditorApplication.isPlaying) { Debug.LogError("[Town Restructure] Exit Play Mode first."); return; }
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != TownScenePath)
        {
            if (scene.isDirty) { Debug.LogError("[Town Restructure] Save or discard the active scene first."); return; }
            scene = EditorSceneManager.OpenScene(TownScenePath);
        }

        GameObject systemObject = Array.Find(scene.GetRootGameObjects(), root => root.name == SystemRootName);
        if (systemObject == null) { Debug.LogError($"[Town Restructure] {SystemRootName} root not found."); return; }

        var behaviours = new List<MonoBehaviour>();
        foreach (GameObject root in scene.GetRootGameObjects())
            behaviours.AddRange(root.GetComponentsInChildren<MonoBehaviour>(true));

        var context = new Context { Apply = apply, System = systemObject.transform, Behaviours = behaviours };
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Town Restructure");
        step(context);
        Undo.CollapseUndoOperations(undoGroup);

        context.Log.Insert(0, $"[Town Restructure] {(apply ? "Applied" : "Report (no changes)")}: {context.Changes} change(s)\n");
        Debug.Log(context.Log.ToString());
        if (apply && context.Changes > 0)
            EditorSceneManager.MarkSceneDirty(scene);
    }

    private static T FindSingle<T>(Context c) where T : MonoBehaviour
    {
        var found = c.Behaviours.FindAll(b => b is T);
        if (found.Count == 1) return (T)found[0];
        c.Log.AppendLine($"  SKIP {typeof(T).Name}: expected one in the scene, found {found.Count}.");
        return null;
    }

    // Report creates nothing, so a missing group is returned as null and only logged.
    private static Transform Group(Context c, string name)
    {
        Transform group = c.System.Find(name);
        if (group != null || !c.Apply) return group;
        var created = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(created, "Town Restructure");
        Undo.SetTransformParent(created.transform, c.System, "Town Restructure");
        created.transform.localPosition = Vector3.zero;
        return created.transform;
    }

    private static void Reparent<T>(Context c, string groupName) where T : MonoBehaviour
    {
        T component = FindSingle<T>(c);
        if (component == null) return;
        Transform target = component.transform;
        if (target == c.System)
        {
            c.Log.AppendLine($"  SKIP {typeof(T).Name}: lives on {SystemRootName} itself; move the component instead.");
            return;
        }
        if (target.parent != null && target.parent.name == groupName && target.parent.parent == c.System)
        {
            c.Log.AppendLine($"  OK   {PathOf(target)}");
            return;
        }

        c.Log.AppendLine($"  MOVE {PathOf(target)} -> {SystemRootName}/{groupName}/{target.name}");
        c.Changes++;
        if (c.Apply) Undo.SetTransformParent(target, Group(c, groupName), "Town Restructure");
    }

    private static void MoveComponent<T>(Context c, string groupName) where T : MonoBehaviour
    {
        T source = FindSingle<T>(c);
        if (source == null) return;
        Transform group = c.System.Find(groupName);
        if (group != null && source.transform == group)
        {
            c.Log.AppendLine($"  OK   {typeof(T).Name} on {PathOf(group)}");
            return;
        }

        int incoming = CountReferences(c, source);
        c.Log.AppendLine($"  MOVE {typeof(T).Name} {PathOf(source.transform)} -> {SystemRootName}/{groupName} ({incoming} reference(s) retargeted)");
        c.Changes++;
        if (!c.Apply) return;

        group = Group(c, groupName);
        T copy = Undo.AddComponent<T>(group.gameObject);
        ComponentUtility.CopyComponent(source);
        ComponentUtility.PasteComponentValues(copy);
        Retarget(c, source, copy);
        c.Behaviours.Remove(source);
        c.Behaviours.Add(copy);
        Undo.DestroyObjectImmediate(source);
    }

    private static int CountReferences(Context c, Object target)
    {
        int count = 0;
        foreach (MonoBehaviour behaviour in c.Behaviours)
        {
            if (behaviour == null || behaviour == target) continue;
            SerializedProperty property = new SerializedObject(behaviour).GetIterator();
            while (property.Next(true))
                if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue == target)
                    count++;
        }
        return count;
    }

    private static void Retarget(Context c, Object from, Object to)
    {
        foreach (MonoBehaviour behaviour in c.Behaviours)
        {
            if (behaviour == null || behaviour == from) continue;
            var serialized = new SerializedObject(behaviour);
            SerializedProperty property = serialized.GetIterator();
            bool changed = false;
            while (property.Next(true))
            {
                if (property.propertyType != SerializedPropertyType.ObjectReference || property.objectReferenceValue != from) continue;
                property.objectReferenceValue = to;
                changed = true;
            }
            if (!changed) continue;
            Undo.RecordObject(behaviour, "Town Restructure");
            serialized.ApplyModifiedProperties();
        }
    }

    private static string PathOf(Transform t)
    {
        string path = t.name;
        while ((t = t.parent) != null) path = t.name + "/" + path;
        return path;
    }
}

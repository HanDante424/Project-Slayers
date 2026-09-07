#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public static class AnimatorTransitionTool
{
    [MenuItem("Tools/Slayers/Animator/Reset All Transitions")]
    private static void ResetAllTransitions()
    {
        AnimatorController controller =
            Selection.activeObject as AnimatorController;

        if (controller == null)
        {
            EditorUtility.DisplayDialog(
                "Animator Transition Tool",
                "Project 창에서 Animator Controller를 선택한 뒤 실행하세요.",
                "OK"
            );

            return;
        }

        int changedCount = 0;

        Undo.RecordObject(
            controller,
            "Reset Animator Transitions"
        );

        foreach (AnimatorControllerLayer layer in controller.layers)
        {
            changedCount += ResetTransitionsRecursive(
                layer.stateMachine
            );
        }

        EditorUtility.SetDirty(controller);

        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog(
            "Completed",
            $"{controller.name}\n\n" +
            $"수정된 Transition: {changedCount}개\n\n" +
            "Has Exit Time = OFF\n" +
            "Transition Duration = 0",
            "OK"
        );

        Debug.Log(
            $"[{controller.name}] " +
            $"Reset {changedCount} transitions. " +
            $"Has Exit Time = OFF, Duration = 0."
        );
    }

    private static int ResetTransitionsRecursive(
        AnimatorStateMachine stateMachine)
    {
        int changedCount = 0;

        // 일반 State → State Transition
        foreach (ChildAnimatorState childState
                 in stateMachine.states)
        {
            foreach (AnimatorStateTransition transition
                     in childState.state.transitions)
            {
                bool changed = false;

                if (transition.hasExitTime)
                {
                    transition.hasExitTime = false;
                    changed = true;
                }

                if (transition.duration != 0f)
                {
                    transition.duration = 0f;
                    changed = true;
                }

                if (changed)
                {
                    EditorUtility.SetDirty(transition);
                    changedCount++;
                }
            }
        }

        // Any State → State Transition
        foreach (AnimatorStateTransition transition
                 in stateMachine.anyStateTransitions)
        {
            bool changed = false;

            if (transition.hasExitTime)
            {
                transition.hasExitTime = false;
                changed = true;
            }

            if (transition.duration != 0f)
            {
                transition.duration = 0f;
                changed = true;
            }

            if (changed)
            {
                EditorUtility.SetDirty(transition);
                changedCount++;
            }
        }

        // 서브 State Machine도 재귀적으로 처리
        foreach (ChildAnimatorStateMachine childStateMachine
                 in stateMachine.stateMachines)
        {
            changedCount += ResetTransitionsRecursive(
                childStateMachine.stateMachine
            );
        }

        return changedCount;
    }
}
#endif

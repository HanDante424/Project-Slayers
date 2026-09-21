using System.Collections;
using UnityEngine;

public abstract class SkillMechanism : ScriptableObject
{
    public abstract bool CanExecute(
        SkillExecutionContext context,
        SkillData skillData);

    public abstract IEnumerator Execute(
        SkillExecutionContext context,
        SkillData skillData);
}

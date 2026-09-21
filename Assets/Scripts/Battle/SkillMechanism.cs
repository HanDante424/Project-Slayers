using System.Collections;
using UnityEngine;

public abstract class SkillMechanism : ScriptableObject
{
    public abstract IEnumerator Execute(
        SkillExecutionContext context,
        SkillData skillData);
}

using System;
using UnityEngine;

// Session selection survives Town/combat scene changes; it does not mutate definition assets or saves.
public static class TownCharacterSelection
{
    public static PlayableCharacterData Character { get; private set; }
    public static int Level { get; private set; } = 1;
    public static event Action Changed;

    public static void Select(PlayableCharacterData character, int level)
    {
        if (character == null || character.statsData == null)
            return;
        int effectiveLevel = Mathf.Max(1, level);
        if (Character == character && Level == effectiveLevel)
            return;
        Character = character;
        Level = effectiveLevel;
        Changed?.Invoke();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        Character = null;
        Level = 1;
        Changed = null;
    }
}

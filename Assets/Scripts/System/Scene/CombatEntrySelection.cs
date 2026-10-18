using UnityEngine;

// One-shot scene handoff, not save data and never written into definition assets.
public static class CombatEntrySelection
{
    public sealed class Request
    {
        public MapData Map { get; }
        public PlayableCharacterData Character { get; }
        public GameObject PlayerPrefab { get; }
        public int Level { get; }
        public Request(MapData map, PlayableCharacterData character, GameObject prefab, int level)
        { Map = map; Character = character; PlayerPrefab = prefab; Level = level; }
    }

    private static Request pending;
    public static bool TrySet(MapData map, PlayableCharacterData character, GameObject prefab, int level)
    {
        if (map == null || character == null || character.statsData == null || prefab == null ||
            prefab.GetComponent<CharacterStats>() == null || prefab.GetComponent<PlayerActionController>() == null || level < 1)
            return false;
        pending = new Request(map, character, prefab, level);
        return true;
    }
    public static Request Consume() { var result = pending; pending = null; return result; }
    public static void Clear() => pending = null;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset() => Clear();
}

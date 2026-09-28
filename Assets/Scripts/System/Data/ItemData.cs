using UnityEngine;

[CreateAssetMenu(
    fileName = "ItemData",
    menuName = "Slayers/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Identity")]
    public Sprite thumbnail;
    public string itemName;
    [TextArea(3, 8)] public string description;

    [Header("Classification")]
    [Min(1)] public int rank = 1;
    public ItemCategory category;
}

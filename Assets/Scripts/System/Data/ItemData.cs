using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Slayers/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Spreadsheet numeric code. -1 means not assigned; 0 is a valid code.")]
    public int itemCode = -1;
    public string itemEnglishName;
    public Sprite thumbnail;
    public string itemName;
    [TextArea(3, 8)] public string description;

    [Header("Classification")]
    public int rank = 1;
    public ItemCategory category;

}

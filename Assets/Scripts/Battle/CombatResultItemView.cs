using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class CombatResultItemView : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text label;

    public void Bind(ItemData item, int count)
    {
        icon.sprite = item != null ? item.thumbnail : null;
        icon.enabled = icon.sprite != null;
        label.text = item != null ? $"{item.itemName}\n×{count}" : "";
    }
}

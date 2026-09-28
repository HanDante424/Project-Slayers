using TMPro;
using UnityEngine;

public sealed class TownTitleController : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;

    public void SetTitle(string title)
    {
        if (titleText != null)
            titleText.text = title;
    }
}

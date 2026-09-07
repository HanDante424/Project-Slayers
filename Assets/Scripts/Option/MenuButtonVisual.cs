using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuButtonVisual : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [SerializeField] private Image background;
    [SerializeField] private TMP_Text label;

    [Header("Background")]
    [SerializeField] private Color normalBackground = new Color(0.04f, 0.04f, 0.04f, 1f);
    [SerializeField] private Color hoverBackground = new Color(0.10f, 0.10f, 0.10f, 1f);
    [SerializeField] private Color pressedBackground = new Color(0.30f, 0.08f, 0.06f, 1f);

    [Header("Text")]
    [SerializeField] private Color normalText = new Color(0.85f, 0.84f, 0.81f, 1f);
    [SerializeField] private Color hoverText = Color.white;
    [SerializeField] private Color pressedText = new Color(0.89f, 0.60f, 0.55f, 1f);

    private bool isPointerInside;

    private void Awake()
    {
        SetNormal();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerInside = true;
        SetHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerInside = false;
        SetNormal();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        SetPressed();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isPointerInside)
            SetHover();
        else
            SetNormal();
    }

    private void SetNormal()
    {
        background.color = normalBackground;
        label.color = normalText;
    }

    private void SetHover()
    {
        background.color = hoverBackground;
        label.color = hoverText;
    }

    private void SetPressed()
    {
        background.color = pressedBackground;
        label.color = pressedText;
    }
}
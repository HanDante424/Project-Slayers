using BloodlinesUI;
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
    [SerializeField] private Color disabledText = new Color(0.21568628f, 0.21568628f, 0.21568628f, 1f);

    private Button button;
    private bool isPointerInside;
    private bool isPressed;
    private bool wasInteractable;
    private bool isSelected;

    // Used by Town buttons that do not yet have an authored visual component.
    public void Initialize(Image targetBackground, TMP_Text targetLabel)
    {
        background = targetBackground;
        label = targetLabel;
        if (background != null) normalBackground = background.color;
        if (label != null) normalText = label.color;
        ButtonTextColorChanger textColorChanger = GetComponent<ButtonTextColorChanger>();
        if (textColorChanger != null && textColorChanger.buttonText == label)
            textColorChanger.enabled = false;
        RefreshVisual();
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        RefreshVisual();
    }

    private bool IsInteractable => button == null || button.IsInteractable();

    private void Awake()
    {
        button = GetComponent<Button>();

        // Take over only the shared label; instances without this visual keep the asset behavior.
        ButtonTextColorChanger textColorChanger = GetComponent<ButtonTextColorChanger>();
        if (textColorChanger != null && textColorChanger.buttonText == label)
            textColorChanger.enabled = false;
    }

    private void OnEnable()
    {
        isPointerInside = false;
        isPressed = false;
        wasInteractable = IsInteractable;
        RefreshVisual();
    }

    private void OnDisable()
    {
        isPointerInside = false;
        isPressed = false;
        RefreshVisual();
    }

    private void Update()
    {
        // Button and parent CanvasGroups can change interactability without pointer events.
        bool interactable = IsInteractable;
        if (wasInteractable == interactable)
            return;

        wasInteractable = interactable;
        isPressed = false;
        RefreshVisual();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerInside = true;
        RefreshVisual();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerInside = false;
        RefreshVisual();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || !IsInteractable)
            return;

        isPressed = true;
        RefreshVisual();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        isPressed = false;
        RefreshVisual();
    }

    private void RefreshVisual()
    {
        bool interactable = IsInteractable;
        bool hover = interactable && isPointerInside;
        bool pressed = hover && isPressed;

        // Button retains ownership of Sprite Swap, including the disabled sprite.
        if (background != null)
            background.color = pressed ? pressedBackground : hover ? hoverBackground : interactable && isSelected ? pressedBackground : normalBackground;

        // Some Town instances intentionally replace/remove the prefab label.
        if (label != null)
            label.color = !interactable ? disabledText : pressed ? pressedText : hover ? hoverText : isSelected ? pressedText : normalText;
    }
}

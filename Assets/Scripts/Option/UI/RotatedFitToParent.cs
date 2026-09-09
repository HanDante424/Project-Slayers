using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class RotatedFitToParent : MonoBehaviour
{
    private RectTransform rect;
    private RectTransform parentRect;

    private void OnEnable()
    {
        Fit();
    }

    private void OnRectTransformDimensionsChange()
    {
        Fit();
    }

    private void Fit()
    {
        if (rect == null)
            rect = GetComponent<RectTransform>();

        if (parentRect == null)
            parentRect = rect.parent as RectTransform;

        if (parentRect == null)
            return;

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;

        rect.sizeDelta = new Vector2(
            parentRect.rect.height,
            parentRect.rect.width
        );

        rect.localRotation = Quaternion.Euler(0f, 0f, 90f);
        rect.localScale = Vector3.one;
    }
}
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Attach to a Button GameObject (the one with an Image/raycastTarget).
/// Finds the child TMP_Text automatically and changes its color + scale on hover.
/// </summary>
public class MenuButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Tooltip("Normal text color.")]
    public Color normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);

    [Tooltip("Text color when the cursor is over the button.")]
    public Color hoverColor = new Color(1f, 0.85f, 0.25f, 1f);

    [Tooltip("Uniform scale multiplier on hover.")]
    public float hoverScale = 1.06f;

    [Tooltip("Lerp speed for scale animation.")]
    public float scaleSpeed = 14f;

    private TMP_Text label;
    private Vector3 baseScale;
    private Vector3 targetScale;

    private void Awake()
    {
        label = GetComponentInChildren<TMP_Text>();
        baseScale   = transform.localScale;
        targetScale = baseScale;
        if (label != null) label.color = normalColor;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (label != null) label.color = hoverColor;
        targetScale = baseScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (label != null) label.color = normalColor;
        targetScale = baseScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (label != null) label.color = normalColor;
        targetScale = baseScale;
    }
}

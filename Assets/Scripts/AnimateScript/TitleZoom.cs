using UnityEngine;
using UnityEngine.EventSystems;

public class TitleZoom : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float hoverOffsetX = 20f;

    private RectTransform rectTransform;
    private Vector2 originalAnchoredPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            originalAnchoredPosition = rectTransform.anchoredPosition;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (rectTransform == null)
        {
            return;
        }

        rectTransform.anchoredPosition = originalAnchoredPosition + new Vector2(hoverOffsetX, 0f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (rectTransform == null)
        {
            return;
        }

        rectTransform.anchoredPosition = originalAnchoredPosition;
    }
}

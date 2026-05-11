using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IconZoom : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Color hoverColor = Color.white;
    [SerializeField] private Vector3 hoverScale = Vector3.one;

    private Image targetImage;
    private Color originalColor;
    private Vector3 originalScale;

    private void Awake()
    {
        targetImage = GetComponent<Image>();
        if (targetImage != null)
        {
            originalColor = targetImage.color;
        }

        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetImage != null)
        {
            targetImage.color = hoverColor;
        }

        transform.localScale = hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (targetImage != null)
        {
            targetImage.color = originalColor;
        }

        transform.localScale = originalScale;
    }
}

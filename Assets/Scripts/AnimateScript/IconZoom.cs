using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IconZoom : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Vector3 hoverScale = Vector3.one;
    [SerializeField] private Color hoverGlowColor = Color.white;
    [SerializeField] private Vector2 hoverGlowDistance = new Vector2(4f, -4f);

    private Vector3 originalScale;
    private Outline glowOutline;
    private bool originalGlowEnabled;
    private Color originalGlowColor;
    private Vector2 originalGlowDistance;

    private void Awake()
    {
        originalScale = transform.localScale;
        glowOutline = GetComponent<Outline>();
        if (glowOutline == null)
        {
            glowOutline = gameObject.AddComponent<Outline>();
        }

        originalGlowEnabled = glowOutline.enabled;
        originalGlowColor = glowOutline.effectColor;
        originalGlowDistance = glowOutline.effectDistance;
        glowOutline.enabled = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = hoverScale;
        if (glowOutline != null)
        {
            glowOutline.effectColor = hoverGlowColor;
            glowOutline.effectDistance = hoverGlowDistance;
            glowOutline.enabled = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalScale;
        if (glowOutline != null)
        {
            glowOutline.effectColor = originalGlowColor;
            glowOutline.effectDistance = originalGlowDistance;
            glowOutline.enabled = originalGlowEnabled;
        }
    }
}

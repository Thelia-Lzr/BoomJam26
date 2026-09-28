using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Handles only the S marker. IconZoom can continue to animate the button itself.
[RequireComponent(typeof(Button), typeof(LevelButtonLock))]
public class Newiconzoom : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image selectionMarker;
    [SerializeField, Range(0.01f, 1f)] private float initialScale = 0.05f;
    [SerializeField] private float animationDuration = 0.2f;

    private Button button;
    private LevelButtonLock levelLock;
    private LevelSelectUI levelSelectUI;
    private Vector3 fullScale;
    private bool selected;

    public int LevelIndex => levelLock.LevelIndex;

    private void Awake()
    {
        button = GetComponent<Button>();
        levelLock = GetComponent<LevelButtonLock>();
        levelSelectUI = GetComponentInParent<LevelSelectUI>();
        if (selectionMarker == null)
        {
            Transform marker = transform.Find("S");
            if (marker != null) selectionMarker = marker.GetComponent<Image>();
        }

        if (selectionMarker == null)
        {
            Debug.LogError($"[Newiconzoom] Missing S marker on {name}", this);
            return;
        }

        fullScale = selectionMarker.rectTransform.localScale;
        selectionMarker.raycastTarget = false;
        HideImmediately();
        button.onClick.AddListener(SelectThisLevel);
    }

    private void OnDestroy()
    {
        if (button != null) button.onClick.RemoveListener(SelectThisLevel);
        if (selectionMarker != null)
        {
            selectionMarker.DOKill();
            selectionMarker.rectTransform.DOKill();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button.interactable && levelLock.IsUnlocked) ShowMarker();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!selected) HideMarker();
    }

    public void SetSelected(bool value)
    {
        selected = value;
        if (value) ShowMarker();
        else HideMarker();
    }

    private void SelectThisLevel()
    {
        if (levelLock.IsUnlocked && levelSelectUI != null)
            levelSelectUI.SelectLevel(LevelIndex);
    }

    private void ShowMarker()
    {
        if (selectionMarker == null) return;
        selectionMarker.DOKill();
        selectionMarker.rectTransform.DOKill();
        selectionMarker.DOFade(1f, animationDuration);
        selectionMarker.rectTransform.DOScale(fullScale, animationDuration).SetEase(Ease.OutBack);
    }

    private void HideMarker()
    {
        if (selectionMarker == null) return;
        selectionMarker.DOKill();
        selectionMarker.rectTransform.DOKill();
        selectionMarker.DOFade(0f, animationDuration);
        selectionMarker.rectTransform.DOScale(fullScale * initialScale, animationDuration).SetEase(Ease.InQuad);
    }

    private void HideImmediately()
    {
        selectionMarker.rectTransform.localScale = fullScale * initialScale;
        Color color = selectionMarker.color;
        color.a = 0f;
        selectionMarker.color = color;
    }
}

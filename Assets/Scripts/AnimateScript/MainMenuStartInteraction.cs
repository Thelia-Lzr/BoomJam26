using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MainMenuStartInteraction : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private RectTransform interact;
    [SerializeField] private RectTransform scrollingMask;
    [SerializeField] private MainMenuManager mainMenuManager;

    [Header("Hover")]
    [SerializeField] private float hoverShift = 30f;
    [SerializeField] private float hoverDuration = 0.2f;
    [SerializeField] private float revealDuration = 0.15f;
    [SerializeField] private float scrollSpeed = 120f;

    [Header("Click")]
    [SerializeField] private float clickShift = 650f;
    [SerializeField] private float clickDuration = 0.65f;
    [SerializeField] private AnimationCurve clickCurve = CreateDefaultClickCurve();

    private Button startButton;
    private RectTransform startRect;
    private RectTransform hoverArea;
    private RectTransform[] maskCopies;
    private CanvasGroup interactGroup;
    private Canvas canvas;
    private Vector2 startPosition;
    private Vector2 maskPosition;
    private float maskWidth;
    private float scrollOffset;
    private bool hovered;
    private bool starting;

    private void Awake()
    {
        startButton = GetComponent<Button>();
        startRect = GetComponent<RectTransform>();
        startPosition = startRect.anchoredPosition;
        canvas = GetComponentInParent<Canvas>();
        if (clickCurve == null || clickCurve.length < 2)
            clickCurve = CreateDefaultClickCurve();

        if (interact == null || scrollingMask == null || mainMenuManager == null)
        {
            Debug.LogError("[MainMenuStartInteraction] Assign Interact, Mask and MainMenuManager.", this);
            enabled = false;
            return;
        }

        interactGroup = interact.GetComponent<CanvasGroup>();
        if (interactGroup == null) interactGroup = interact.gameObject.AddComponent<CanvasGroup>();
        interactGroup.alpha = 0f;
        interactGroup.blocksRaycasts = false;
        interactGroup.interactable = false;

        CreateHoverArea();
        CreateMaskCopies();
        startButton.onClick.AddListener(OnStartClicked);
    }

    private void Update()
    {
        if (!starting)
        {
            Camera eventCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera : null;
            bool pointerInside = RectTransformUtility.RectangleContainsScreenPoint(
                hoverArea, Input.mousePosition, eventCamera);
            if (pointerInside != hovered) SetHovered(pointerInside);
        }

        if ((hovered || starting) && maskCopies != null && maskWidth > 0f)
        {
            scrollOffset = Mathf.Repeat(scrollOffset + scrollSpeed * Time.unscaledDeltaTime, maskWidth);
            for (int i = 0; i < maskCopies.Length; i++)
                maskCopies[i].anchoredPosition = maskPosition + Vector2.right * (scrollOffset - i * maskWidth);
        }
    }

    private void OnDestroy()
    {
        if (startButton != null) startButton.onClick.RemoveListener(OnStartClicked);
        if (startRect != null) startRect.DOKill();
        if (interactGroup != null) interactGroup.DOKill();
    }

    private void SetHovered(bool value)
    {
        hovered = value;
        interactGroup.DOKill();
        interactGroup.DOFade(value ? 1f : 0f, revealDuration);
        startRect.DOKill();
        startRect.DOAnchorPos(startPosition + Vector2.right * (value ? hoverShift : 0f), hoverDuration)
            .SetEase(Ease.OutCubic);
    }

    private void OnStartClicked()
    {
        if (starting) return;
        starting = true;
        interactGroup.DOKill();
        interactGroup.DOFade(1f, revealDuration);
        startRect.DOKill();
        startRect.DOAnchorPos(startPosition + Vector2.right * clickShift, clickDuration)
            .SetEase(clickCurve)
            .OnComplete(mainMenuManager.OnClickStart);
    }

    private void CreateHoverArea()
    {
        GameObject area = new GameObject("Start Hover Area", typeof(RectTransform));
        hoverArea = area.GetComponent<RectTransform>();
        hoverArea.SetParent(startRect.parent, false);
        hoverArea.anchorMin = startRect.anchorMin;
        hoverArea.anchorMax = startRect.anchorMax;
        hoverArea.pivot = startRect.pivot;
        hoverArea.anchoredPosition = startPosition;
        hoverArea.sizeDelta = startRect.sizeDelta;
        hoverArea.localRotation = startRect.localRotation;
        hoverArea.localScale = startRect.localScale;
    }

    private void CreateMaskCopies()
    {
        Image maskImage = scrollingMask.GetComponent<Image>();
        if (maskImage == null)
        {
            Debug.LogError("[MainMenuStartInteraction] Mask needs a UI Image.", this);
            return;
        }

        maskImage.raycastTarget = false;
        Image interactImage = interact.GetComponent<Image>();
        if (interactImage != null) interactImage.raycastTarget = false;

        maskPosition = scrollingMask.anchoredPosition;
        maskWidth = scrollingMask.rect.width * scrollingMask.localScale.x;
        maskCopies = new RectTransform[3];
        maskCopies[0] = scrollingMask;
        for (int i = 1; i < maskCopies.Length; i++)
        {
            Image copy = Instantiate(maskImage, interact);
            copy.name = "Mask Repeat " + i;
            copy.raycastTarget = false;
            maskCopies[i] = copy.rectTransform;
            maskCopies[i].anchoredPosition = maskPosition - Vector2.right * (i * maskWidth);
        }
    }

    private static AnimationCurve CreateDefaultClickCurve()
    {
        return new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 0f),
            new Keyframe(0.55f, 0.1f, 0.35f, 0.35f),
            new Keyframe(1f, 1f, 2.6f, 2.6f));
    }
}

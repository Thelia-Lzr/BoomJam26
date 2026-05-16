using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragGuide : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private enum MovementMode
    {
        VerticalLoop,
        HorizontalPingPong
    }

    [SerializeField] private MovementMode movementMode = MovementMode.VerticalLoop;
    [SerializeField] private RectTransform guidePrefab;
    [SerializeField] private float riseHeight = 60f;
    [SerializeField] private float riseDuration = 1.4f;
    [SerializeField] private Ease riseEase = Ease.OutQuad;
    [SerializeField] private float horizontalRange = 60f;
    [SerializeField] private float horizontalDuration = 2f;
    [SerializeField] private Ease horizontalEase = Ease.InOutSine;

    private RectTransform guideInstance;
    private Tween guideTween;

    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowGuide();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideGuide();
    }

    private void OnDisable()
    {
        HideGuide();
    }

    private void OnDestroy()
    {
        guideTween?.Kill();
    }

    private void ShowGuide()
    {
        if (guidePrefab == null)
        {
            return;
        }

        if (guideInstance == null)
        {
            guideInstance = Instantiate(guidePrefab, transform);
            guideInstance.anchoredPosition = Vector2.zero;

            var canvasGroup = guideInstance.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = guideInstance.gameObject.AddComponent<CanvasGroup>();
            }

            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            var graphics = guideInstance.GetComponentsInChildren<Graphic>(true);
            foreach (var graphic in graphics)
            {
                graphic.raycastTarget = false;
            }
        }

        guideInstance.gameObject.SetActive(true);
        guideInstance.anchoredPosition = Vector2.zero;

        StartGuideTween();
    }

    private void HideGuide()
    {
        guideTween?.Kill();
        guideTween = null;

        if (guideInstance != null)
        {
            guideInstance.gameObject.SetActive(false);
        }
    }

    private void StartGuideTween()
    {
        guideTween?.Kill();
        guideInstance.anchoredPosition = Vector2.zero;

        switch (movementMode)
        {
            case MovementMode.HorizontalPingPong:
            {
                var sequence = DOTween.Sequence();
                sequence.Append(guideInstance.DOAnchorPosX(-horizontalRange, horizontalDuration).SetEase(horizontalEase));
                sequence.Append(guideInstance.DOAnchorPosX(horizontalRange, horizontalDuration).SetEase(horizontalEase));
                sequence.SetLoops(-1, LoopType.Yoyo);
                guideTween = sequence;
                break;
            }
            default:
            {
                guideTween = guideInstance
                    .DOAnchorPosY(riseHeight, riseDuration)
                    .SetEase(riseEase)
                    .SetLoops(-1, LoopType.Restart);
                break;
            }
        }
    }
}

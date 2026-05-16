using System.Collections;
using TMPro;
using UnityEngine;

public class AccountTextZoom : MonoBehaviour
{
    // 结算界面的用时与容量文本引用。
    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI memoryText;

    // 闪出动画参数设置。
    [Header("Animation")]
    [SerializeField] private float appearDuration = 0.25f;
    [SerializeField] private float delayBetween = 0.2f;
    [SerializeField] private float startScale = 0.6f;
    [SerializeField] private Vector2 startOffset = new Vector2(-200f, 0f);

    private Vector3 timeBaseScale = Vector3.one;
    private Vector3 memoryBaseScale = Vector3.one;
    private Vector2 timeBasePosition;
    private Vector2 memoryBasePosition;
    private Coroutine playCoroutine;

    private void Awake()
    {
        if (timeText != null)
        {
            timeBaseScale = timeText.rectTransform.localScale;
            timeBasePosition = timeText.rectTransform.anchoredPosition;
        }

        if (memoryText != null)
        {
            memoryBaseScale = memoryText.rectTransform.localScale;
            memoryBasePosition = memoryText.rectTransform.anchoredPosition;
        }
    }

    private void OnEnable()
    {
        Play();
    }

    // 开始按顺序播放闪出动画。
    public void Play()
    {
        if (playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
        }

        playCoroutine = StartCoroutine(PlaySequence());
    }

    // 先播放用时文本，再延迟播放容量文本。
    private IEnumerator PlaySequence()
    {
        PrepareText(timeText, timeBaseScale, timeBasePosition);
        PrepareText(memoryText, memoryBaseScale, memoryBasePosition);

        if (timeText != null)
        {
            yield return AnimateText(timeText, timeBaseScale, timeBasePosition);
            if (delayBetween > 0f)
            {
                yield return new WaitForSeconds(delayBetween);
            }
        }

        if (memoryText != null)
        {
            yield return AnimateText(memoryText, memoryBaseScale, memoryBasePosition);
        }

        playCoroutine = null;
    }

    // 播放前将文本重置为隐藏状态。
    private void PrepareText(TextMeshProUGUI text, Vector3 baseScale, Vector2 basePosition)
    {
        if (text == null) return;

        SetTextAlpha(text, 0f);
        text.rectTransform.localScale = baseScale * Mathf.Max(0.01f, startScale);
        text.rectTransform.anchoredPosition = basePosition + startOffset;
    }

    // 通过缩放与透明度播放闪出效果。
    private IEnumerator AnimateText(TextMeshProUGUI text, Vector3 baseScale, Vector2 basePosition)
    {
        float duration = Mathf.Max(0.01f, appearDuration);
        float elapsed = 0f;
        Vector2 startPosition = basePosition + startOffset;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            text.rectTransform.localScale = Vector3.Lerp(baseScale * Mathf.Max(0.01f, startScale), baseScale, t);
            text.rectTransform.anchoredPosition = Vector2.Lerp(startPosition, basePosition, t);
            SetTextAlpha(text, t);
            yield return null;
        }

        text.rectTransform.localScale = baseScale;
        text.rectTransform.anchoredPosition = basePosition;
        SetTextAlpha(text, 1f);
    }

    // 设置 TMP 文本颜色的透明度。
    private void SetTextAlpha(TextMeshProUGUI text, float alpha)
    {
        Color color = text.color;
        color.a = alpha;
        text.color = color;
    }
}

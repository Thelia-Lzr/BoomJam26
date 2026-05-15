using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QManager : MonoBehaviour
{
    [Header("1. 剧情触发")]
    [SerializeField] private InGameDialogue inGameDialogue;
    [SerializeField] private string dialogSegmentId = "Q_01";

    [Header("2. 隐藏设置")]
    [SerializeField] private bool hideAfterDialog = true;
    [SerializeField] private bool hideAtStart = true;

    [Header("3. 顺序播放")]
    [SerializeField] private bool playOnStart = false;
    [SerializeField] private float playDelay = 0f;
    [SerializeField] private bool revealBeforePlay = true;
    [SerializeField] private bool useSequence = false;
    [SerializeField] private QManager nextInSequence;

    private SpriteRenderer spriteRenderer;
    private Collider2D cachedCollider;
    private readonly Collider2D[] zoneResults = new Collider2D[8];
    private bool hasTriggered;
    private bool isVisible;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        cachedCollider = GetComponent<Collider2D>();

        if (hideAtStart && spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
            isVisible = false;
        }
    }

    private void Start()
    {
        if (playOnStart)
        {
            TriggerSequence(playDelay);
        }
    }

    private void FixedUpdate()
    {
        if (hasTriggered) return;
        if (cachedCollider == null || spriteRenderer == null) return;
        if (!IsTouchingSwapZone())
        {
            if (isVisible && hideAtStart)
            {
                spriteRenderer.enabled = false;
                isVisible = false;
            }
            return;
        }

        if (!isVisible)
        {
            spriteRenderer.enabled = true;
            isVisible = true;
        }

        if (inGameDialogue == null) return;

        hasTriggered = true;
        inGameDialogue.PlaySegment(dialogSegmentId, transform, ShouldRestoreCamera(), HandleDialogFinished);
    }

    public void TriggerSequence(float delaySeconds = 0f)
    {
        if (hasTriggered) return;
        StartCoroutine(PlaySequenceAfterDelay(delaySeconds));
    }

    private IEnumerator PlaySequenceAfterDelay(float delaySeconds)
    {
        if (delaySeconds > 0f)
        {
            yield return new WaitForSeconds(delaySeconds);
        }

        if (spriteRenderer != null && revealBeforePlay)
        {
            spriteRenderer.enabled = true;
            isVisible = true;
        }

        if (inGameDialogue == null) yield break;

        hasTriggered = true;
        inGameDialogue.PlaySegment(dialogSegmentId, transform, ShouldRestoreCamera(), HandleDialogFinished);
    }

    private bool IsTouchingSwapZone()
    {
        if (cachedCollider == null) return false;

        int count = cachedCollider.GetContacts(zoneResults);
        for (int i = 0; i < count; i++)
        {
            if (zoneResults[i] != null && zoneResults[i].TryGetComponent<SwapZone>(out _))
            {
                return true;
            }
        }

        return false;
    }

    private void HandleDialogFinished()
    {
        if (!hideAfterDialog) return;

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        if (cachedCollider != null)
        {
            cachedCollider.enabled = false;
        }

        if (useSequence && nextInSequence != null)
        {
            nextInSequence.TriggerSequence();
        }
    }

    private bool ShouldRestoreCamera()
    {
        if (!useSequence) return true;
        return nextInSequence == null;
    }
}

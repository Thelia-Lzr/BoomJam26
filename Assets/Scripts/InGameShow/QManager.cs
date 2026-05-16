using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QManager : MonoBehaviour
{
    private struct QueueEntry
    {
        public QManager Manager;
        public int Order;
        public int Ticket;
        public float Delay;
    }

    private static readonly List<QueueEntry> PendingQueue = new List<QueueEntry>();
    private static int NextTicket;
    private static bool dialogPlaying;
    private static bool autoStartScheduled;

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

    [Header("3.1 队列顺序")]
    [SerializeField] private bool useDialogQueue = true;
    [SerializeField] private int queueOrder = 0;

    [Header("4. 消失效果")]
    [SerializeField] private bool useDisappearEffect = false;
    [SerializeField] private float disappearDuration = 1f;
    [SerializeField] private float disappearDistance = 0.8f;

    private SpriteRenderer spriteRenderer;
    private Collider2D cachedCollider;
    private readonly Collider2D[] zoneResults = new Collider2D[8];
    private bool hasTriggered;
    private bool isVisible;
    private bool isQueued;

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
        if (hasTriggered || isQueued) return;
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
        RequestPlay(0f);
    }

    public void TriggerSequence(float delaySeconds = 0f)
    {
        if (hasTriggered || isQueued) return;
        RequestPlay(delaySeconds);
    }

    private void RequestPlay(float delaySeconds)
    {
        if (hasTriggered || isQueued) return;
        isQueued = true;

        if (!useDialogQueue)
        {
            StartCoroutine(PlaySequenceAfterDelay(delaySeconds));
            return;
        }

        PendingQueue.Add(new QueueEntry
        {
            Manager = this,
            Order = queueOrder,
            Ticket = NextTicket++,
            Delay = delaySeconds
        });

        PendingQueue.Sort((a, b) =>
        {
            int orderCompare = a.Order.CompareTo(b.Order);
            return orderCompare != 0 ? orderCompare : a.Ticket.CompareTo(b.Ticket);
        });

        ScheduleTryPlayNext();
    }

    private static void TryPlayNext()
    {
        if (dialogPlaying || PendingQueue.Count == 0) return;

        QueueEntry entry = PendingQueue[0];
        PendingQueue.RemoveAt(0);
        if (entry.Manager == null)
        {
            TryPlayNext();
            return;
        }

        dialogPlaying = true;
        entry.Manager.StartCoroutine(entry.Manager.PlaySequenceAfterDelay(entry.Delay));
    }

    private void ScheduleTryPlayNext()
    {
        if (dialogPlaying || autoStartScheduled) return;
        autoStartScheduled = true;
        StartCoroutine(DelayedTryPlayNext());
    }

    private IEnumerator DelayedTryPlayNext()
    {
        yield return null;
        autoStartScheduled = false;
        TryPlayNext();
    }

    private IEnumerator PlaySequenceAfterDelay(float delaySeconds)
    {
        if (delaySeconds > 0f)
        {
            yield return new WaitForSeconds(delaySeconds);
        }

        isQueued = false;

        if (spriteRenderer != null && revealBeforePlay)
        {
            spriteRenderer.enabled = true;
            isVisible = true;
        }

        if (inGameDialogue == null)
        {
            CompleteDialog();
            yield break;
        }

        hasTriggered = true;
        inGameDialogue.PlaySegment(dialogSegmentId, transform, ShouldRestoreCamera(), ShouldDeferDialogClose(), HandleDialogFinished);
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
        if (!hideAfterDialog)
        {
            CompleteDialog();
            if (useSequence && nextInSequence != null)
            {
                nextInSequence.TriggerSequence();
            }
            return;
        }

        if (useDisappearEffect)
        {
            StartCoroutine(PlayDisappearEffect());
            return;
        }

        FinalizeDisappear();
    }

    private bool ShouldRestoreCamera()
    {
        if (!useSequence) return true;
        return nextInSequence == null;
    }

    private bool ShouldDeferDialogClose()
    {
        return hideAfterDialog && useDisappearEffect;
    }

    private IEnumerator PlayDisappearEffect()
    {
        if (spriteRenderer == null)
        {
            FinalizeDisappear();
            yield break;
        }

        float duration = Mathf.Max(0.01f, disappearDuration);
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.left * disappearDistance;
        Color startColor = spriteRenderer.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector3.Lerp(startPos, endPos, t);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(startColor.a, 0f, t));
            yield return null;
        }

        spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
        transform.position = endPos;
        FinalizeDisappear();
    }

    private void FinalizeDisappear()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        if (cachedCollider != null)
        {
            cachedCollider.enabled = false;
        }

        if (inGameDialogue != null && ShouldDeferDialogClose())
        {
            inGameDialogue.CloseDialog();
        }

        CompleteDialog();

        if (useSequence && nextInSequence != null)
        {
            nextInSequence.TriggerSequence();
        }
    }

    private void CompleteDialog()
    {
        if (!useDialogQueue) return;
        dialogPlaying = false;
        TryPlayNext();
    }
}

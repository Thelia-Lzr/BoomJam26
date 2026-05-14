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

    private SpriteRenderer spriteRenderer;
    private Collider2D cachedCollider;
    private InvisibleDrawings invisibleDrawings;
    private bool hasTriggered;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        cachedCollider = GetComponent<Collider2D>();
        invisibleDrawings = GetComponent<InvisibleDrawings>();
    }

    private void Update()
    {
        if (hasTriggered) return;
        if (spriteRenderer == null || !spriteRenderer.enabled) return;
        if (inGameDialogue == null) return;

        hasTriggered = true;
        inGameDialogue.PlaySegment(dialogSegmentId, HandleDialogFinished);
    }

    private void HandleDialogFinished()
    {
        if (!hideAfterDialog) return;

        if (invisibleDrawings != null)
        {
            invisibleDrawings.enabled = false;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        if (cachedCollider != null)
        {
            cachedCollider.enabled = false;
        }
    }
}

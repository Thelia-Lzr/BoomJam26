using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameDialogue : MonoBehaviour
{
    [Serializable]
    private struct DialogRow
    {
        public int Id;
        public string Name;
        public string Content;
        public string Next;
    }

    [Header("1. 数据")]
    [SerializeField] private TextAsset dialogCsv;
    [SerializeField] private int startLineId = 0;

    [Header("2. UI 引用")]
    [SerializeField] private GameObject dialogRoot;
    [SerializeField] private GameObject inputBlocker;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private Button nextButton;

    [Header("3. 打字机")]
    [SerializeField] private float typeSpeed = 0.03f;

    [Header("4. 镜头聚焦")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Camera focusCamera;
    [SerializeField] private float cameraMoveDuration = 0.2f;
    [SerializeField] private bool enableCameraZoom = false;
    [SerializeField] private float zoomScale = 0.8f;
    [SerializeField] private float cameraZoomDuration = 0.2f;

    private readonly Dictionary<string, Dictionary<int, DialogRow>> dialogRowsBySegment =
        new Dictionary<string, Dictionary<int, DialogRow>>();

    private Coroutine typewriterCoroutine;
    private string currentFullContent = "";
    private string currentSegmentId = "";
    private int currentLineId;
    private int pendingNextId;
    private bool isFinished;
    private Action finishCallback;
    private bool restoreCameraOnFinish = true;
    private bool deferCloseOnFinish;
    private bool waitingForExternalClose;
    private Vector3? cameraOriginalPosition;
    private float? cameraOriginalSize;
    private Coroutine cameraMoveCoroutine;
    private Coroutine cameraZoomCoroutine;

    private const int ColSegment = 0;
    private const int ColId = 1;
    private const int ColName = 2;
    private const int ColContent = 3;
    private const int ColNext = 4;

    private void Awake()
    {
        ParseCsv();
        HideDialog();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (focusCamera == null)
        {
            focusCamera = Camera.main;
        }

        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnClickNext);
        }
    }

    public void PlaySegment(string segmentId, Action onFinished = null)
    {
        PlaySegment(segmentId, null, true, false, onFinished);
    }

    public void PlaySegment(string segmentId, Transform focusTarget, Action onFinished = null)
    {
        PlaySegment(segmentId, focusTarget, true, false, onFinished);
    }

    public void PlaySegment(string segmentId, Transform focusTarget, bool restoreCameraWhenDone, Action onFinished = null)
    {
        PlaySegment(segmentId, focusTarget, restoreCameraWhenDone, false, onFinished);
    }

    public void PlaySegment(string segmentId, Transform focusTarget, bool restoreCameraWhenDone, bool deferClose, Action onFinished = null)
    {
        if (string.IsNullOrWhiteSpace(segmentId))
        {
            Debug.LogWarning("[InGameDialogue] segmentId 不能为空。");
            return;
        }

        if (!dialogRowsBySegment.ContainsKey(segmentId))
        {
            Debug.LogWarning($"[InGameDialogue] 未找到段落: {segmentId}");
            return;
        }

        finishCallback = onFinished;
        restoreCameraOnFinish = restoreCameraWhenDone;
        deferCloseOnFinish = deferClose;
        waitingForExternalClose = false;
        currentSegmentId = segmentId.Trim();
        currentLineId = startLineId;
        isFinished = false;
        ShowDialogRoot(true);
        FocusCamera(focusTarget);
        ShowDialogRow();
    }

    private void ShowDialogRow()
    {
        if (!dialogRowsBySegment.TryGetValue(currentSegmentId, out var rows))
        {
            EndDialog();
            return;
        }

        if (!rows.TryGetValue(currentLineId, out var row))
        {
            EndDialog();
            return;
        }

        if (nameText != null)
        {
            nameText.text = row.Name;
        }

        currentFullContent = row.Content;
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        typewriterCoroutine = StartCoroutine(TypeText(currentFullContent));

        string nextToken = row.Next.Trim();
        if (nextToken.Equals("end", StringComparison.OrdinalIgnoreCase))
        {
            isFinished = true;
            pendingNextId = -1;
        }
        else if (!int.TryParse(nextToken, out pendingNextId))
        {
            isFinished = true;
            pendingNextId = -1;
        }
        else
        {
            isFinished = false;
        }
    }

    private void OnClickNext()
    {
        if (dialogRoot != null && !dialogRoot.activeSelf) return;
        if (waitingForExternalClose) return;

        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
            if (dialogText != null)
            {
                dialogText.text = currentFullContent;
            }
            return;
        }

        if (isFinished)
        {
            EndDialog();
            return;
        }

        currentLineId = pendingNextId;
        ShowDialogRow();
    }

    private IEnumerator TypeText(string text)
    {
        if (dialogText != null)
        {
            dialogText.text = "";
        }

        if (dialogText == null)
        {
            yield break;
        }

        foreach (char c in text)
        {
            dialogText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        typewriterCoroutine = null;
    }

    private void EndDialog()
    {
        if (deferCloseOnFinish)
        {
            waitingForExternalClose = true;
            if (nextButton != null)
            {
                nextButton.interactable = false;
            }
        }
        else
        {
            CloseDialog();
        }
        var callback = finishCallback;
        finishCallback = null;
        callback?.Invoke();
    }

    public void CloseDialog()
    {
        HideDialog();
        if (restoreCameraOnFinish)
        {
            RestoreCamera();
            RestoreCameraZoom();
        }

        if (nextButton != null)
        {
            nextButton.interactable = true;
        }

        waitingForExternalClose = false;
    }

    private void HideDialog()
    {
        ShowDialogRoot(false);
        if (dialogText != null)
        {
            dialogText.text = "";
        }
    }

    private void ShowDialogRoot(bool visible)
    {
        if (dialogRoot != null)
        {
            dialogRoot.SetActive(visible);
        }

        if (inputBlocker != null)
        {
            inputBlocker.SetActive(visible);
        }
    }

    private void ParseCsv()
    {
        dialogRowsBySegment.Clear();

        if (dialogCsv == null)
        {
            return;
        }

        string[] rows = dialogCsv.text.Split('\n');
        foreach (string rawRow in rows)
        {
            if (string.IsNullOrWhiteSpace(rawRow)) continue;

            string[] cells = rawRow.Split(',');
            if (cells.Length <= ColNext) continue;

            string segmentId = cells[ColSegment].Trim();
            if (string.IsNullOrWhiteSpace(segmentId)) continue;

            if (!int.TryParse(cells[ColId].Trim(), out int lineId)) continue;

            var row = new DialogRow
            {
                Id = lineId,
                Name = cells[ColName].Trim(),
                Content = cells[ColContent].Trim(),
                Next = cells[ColNext].Trim()
            };

            if (!dialogRowsBySegment.TryGetValue(segmentId, out var segmentRows))
            {
                segmentRows = new Dictionary<int, DialogRow>();
                dialogRowsBySegment.Add(segmentId, segmentRows);
            }

            segmentRows[lineId] = row;
        }
    }

    private void FocusCamera(Transform focusTarget)
    {
        if (cameraTransform == null || focusTarget == null) return;

        cameraOriginalPosition ??= cameraTransform.position;
        Vector3 targetPosition = focusTarget.position;
        targetPosition.z = cameraTransform.position.z;

        StartCameraMove(targetPosition);
        TryZoomCamera();
    }

    private void RestoreCamera()
    {
        if (cameraTransform == null || cameraOriginalPosition == null) return;

        StartCameraMove(cameraOriginalPosition.Value);
        cameraOriginalPosition = null;
    }

    private void TryZoomCamera()
    {
        if (!enableCameraZoom || focusCamera == null) return;

        cameraOriginalSize ??= GetCameraSize();
        float targetSize = cameraOriginalSize.Value * Mathf.Clamp(zoomScale, 0.1f, 1f);
        StartCameraZoom(targetSize);
    }

    private void RestoreCameraZoom()
    {
        if (!enableCameraZoom || focusCamera == null || cameraOriginalSize == null) return;

        StartCameraZoom(cameraOriginalSize.Value);
        cameraOriginalSize = null;
    }

    private float GetCameraSize()
    {
        return focusCamera.orthographic ? focusCamera.orthographicSize : focusCamera.fieldOfView;
    }

    private void SetCameraSize(float size)
    {
        if (focusCamera.orthographic)
        {
            focusCamera.orthographicSize = size;
        }
        else
        {
            focusCamera.fieldOfView = size;
        }
    }

    private void StartCameraMove(Vector3 targetPosition)
    {
        if (cameraMoveCoroutine != null)
        {
            StopCoroutine(cameraMoveCoroutine);
        }

        cameraMoveCoroutine = StartCoroutine(MoveCamera(targetPosition));
    }

    private IEnumerator MoveCamera(Vector3 targetPosition)
    {
        if (cameraTransform == null)
        {
            yield break;
        }

        float duration = Mathf.Max(0.01f, cameraMoveDuration);
        Vector3 start = cameraTransform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            cameraTransform.position = Vector3.Lerp(start, targetPosition, t);
            yield return null;
        }

        cameraTransform.position = targetPosition;
        cameraMoveCoroutine = null;
    }

    private void StartCameraZoom(float targetSize)
    {
        if (cameraZoomCoroutine != null)
        {
            StopCoroutine(cameraZoomCoroutine);
        }

        cameraZoomCoroutine = StartCoroutine(ZoomCamera(targetSize));
    }

    private IEnumerator ZoomCamera(float targetSize)
    {
        if (focusCamera == null)
        {
            yield break;
        }

        float duration = Mathf.Max(0.01f, cameraZoomDuration);
        float startSize = GetCameraSize();
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            SetCameraSize(Mathf.Lerp(startSize, targetSize, t));
            yield return null;
        }

        SetCameraSize(targetSize);
        cameraZoomCoroutine = null;
    }
}

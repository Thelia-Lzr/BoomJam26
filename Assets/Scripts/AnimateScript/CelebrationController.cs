using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CelebrationController : MonoBehaviour
{
    private enum CelebrationAction
    {
        [InspectorName("Jump")]
        Jump,
        [InspectorName("Flip")]
        Flip,
        [InspectorName("Shake")]
        Shake,
        [InspectorName("QQ Bounce Scale")]
        BounceScale
    }

    private enum FlipAxis
    {
        [InspectorName("Horizontal")]
        X,
        [InspectorName("Vertical")]
        Y
    }

    [Header("Play")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loop = true;
    [SerializeField] private float loopInterval = 0.18f;
    [Tooltip("Leave empty to animate this GameObject.")]
    [SerializeField] private Transform target;
    [Tooltip("Overall feel for jump and shake.")]
    [SerializeField] private Ease motionEase = Ease.OutQuad;

    [Header("Action Order")]
    [Tooltip("Drag items to change the celebration order. Each action uses the parameters below.")]
    [SerializeField] private List<CelebrationAction> actionOrder = new List<CelebrationAction>
    {
        CelebrationAction.BounceScale,
        CelebrationAction.Jump,
        CelebrationAction.Flip,
        CelebrationAction.Shake
    };

    [Header("Jump")]
    [SerializeField] private bool useJump = true;
    [SerializeField, Min(0f)] private float jumpDuration = 0.42f;
    [SerializeField] private float jumpHeight = 0.75f;
    [SerializeField, Min(1)] private int jumpCount = 1;

    [Header("Flip")]
    [Tooltip("Toggles facing and keeps it until the next Flip action.")]
    [SerializeField] private bool useFlip = false;
    [SerializeField, Min(0f)] private float flipDuration = 0.16f;
    [SerializeField] private FlipAxis flipAxis = FlipAxis.X;

    [Header("Shake")]
    [SerializeField] private bool useShake = true;
    [SerializeField, Min(0f)] private float shakeDuration = 0.18f;
    [SerializeField] private Vector3 shakeStrength = new Vector3(0.05f, 0.05f, 0f);
    [SerializeField, Min(1)] private int shakeVibrato = 8;
    [SerializeField, Range(0f, 180f)] private float shakeRandomness = 30f;

    [Header("QQ Bounce Scale")]
    [SerializeField] private bool useBounceScale = true;
    [SerializeField, Min(0f)] private float squashDuration = 0.12f;
    [SerializeField] private Vector3 squashScale = new Vector3(1.16f, 0.84f, 1f);
    [SerializeField, Min(0f)] private float stretchDuration = 0.14f;
    [SerializeField] private Vector3 stretchScale = new Vector3(0.92f, 1.08f, 1f);

    private Sequence celebrationSequence;
    private Vector3 startLocalPosition;
    private Vector3 startLocalRotation;
    private Vector3 startLocalScale;
    private Vector3 actionBaseScale;

    private void Awake()
    {
        if (target == null)
        {
            target = transform;
        }

        CacheStartTransform();
    }

    private void Start()
    {
        if (playOnStart)
        {
            Play();
        }
    }

    private void OnDisable()
    {
        Stop();
    }

    private void OnDestroy()
    {
        KillSequence();
    }

    [ContextMenu("Play Celebration")]
    public void Play()
    {
        if (target == null)
        {
            target = transform;
        }

        KillSequence();
        RestoreStartTransform();
        PlayOneRound();
    }

    [ContextMenu("Stop Celebration")]
    public void Stop()
    {
        KillSequence();
        RestoreStartTransform();
    }

    [ContextMenu("Use QQ Bounce Preset")]
    private void UseQQBouncePreset()
    {
        playOnStart = true;
        loop = true;
        loopInterval = 0.18f;
        motionEase = Ease.OutQuad;
        actionOrder = GetDefaultActionOrder();

        useJump = true;
        jumpDuration = 0.42f;
        jumpHeight = 0.75f;
        jumpCount = 1;

        useFlip = false;
        flipDuration = 0.16f;
        flipAxis = FlipAxis.X;

        useShake = true;
        shakeDuration = 0.18f;
        shakeStrength = new Vector3(0.05f, 0.05f, 0f);
        shakeVibrato = 8;
        shakeRandomness = 30f;

        useBounceScale = true;
        squashDuration = 0.12f;
        squashScale = new Vector3(1.16f, 0.84f, 1f);
        stretchDuration = 0.14f;
        stretchScale = new Vector3(0.92f, 1.08f, 1f);
    }

    private void CacheStartTransform()
    {
        if (target == null) return;

        startLocalPosition = target.localPosition;
        startLocalRotation = target.localEulerAngles;
        startLocalScale = target.localScale;
    }

    private void RestoreStartTransform()
    {
        if (target == null) return;

        target.localPosition = startLocalPosition;
        target.localEulerAngles = startLocalRotation;
        target.localScale = startLocalScale;
    }

    private void KillSequence()
    {
        if (celebrationSequence == null) return;

        celebrationSequence.Kill();
        celebrationSequence = null;
    }

    private void PlayOneRound()
    {
        if (target == null) return;

        actionBaseScale = target.localScale;
        celebrationSequence = DOTween.Sequence().SetTarget(target);
        List<CelebrationAction> activeActions = actionOrder;
        if (activeActions == null || activeActions.Count == 0)
        {
            activeActions = GetDefaultActionOrder();
        }

        foreach (CelebrationAction action in activeActions)
        {
            AppendAction(celebrationSequence, action);
        }

        celebrationSequence.Append(target.DOScale(actionBaseScale, 0.08f).SetEase(Ease.OutQuad));

        if (!loop) return;

        celebrationSequence.AppendInterval(Mathf.Max(0f, loopInterval));
        celebrationSequence.OnComplete(() =>
        {
            if (!isActiveAndEnabled || target == null) return;

            RestorePositionAndRotation();
            PlayOneRound();
        });
    }

    private void RestorePositionAndRotation()
    {
        if (target == null) return;

        target.localPosition = startLocalPosition;
        target.localEulerAngles = startLocalRotation;
    }

    private List<CelebrationAction> GetDefaultActionOrder()
    {
        return new List<CelebrationAction>
        {
            CelebrationAction.BounceScale,
            CelebrationAction.Jump,
            CelebrationAction.Shake
        };
    }

    private void AppendAction(Sequence sequence, CelebrationAction action)
    {
        switch (action)
        {
            case CelebrationAction.Jump:
                AppendJump(sequence);
                break;
            case CelebrationAction.Flip:
                AppendFlip(sequence);
                break;
            case CelebrationAction.Shake:
                AppendShake(sequence);
                break;
            case CelebrationAction.BounceScale:
                AppendBounceScale(sequence);
                break;
        }
    }

    private void AppendBounceScale(Sequence sequence)
    {
        if (!useBounceScale) return;

        sequence.Append(target
            .DOScale(Vector3.Scale(actionBaseScale, squashScale), Mathf.Max(0.01f, squashDuration))
            .SetEase(Ease.OutQuad));
        sequence.Append(target
            .DOScale(Vector3.Scale(actionBaseScale, stretchScale), Mathf.Max(0.01f, stretchDuration))
            .SetEase(Ease.OutBack));
        sequence.Append(target
            .DOScale(actionBaseScale, Mathf.Max(0.01f, stretchDuration * 0.5f))
            .SetEase(Ease.OutQuad));
    }

    private void AppendJump(Sequence sequence)
    {
        if (!useJump) return;

        sequence.Append(target
            .DOLocalJump(startLocalPosition, jumpHeight, Mathf.Max(1, jumpCount), Mathf.Max(0.01f, jumpDuration))
            .SetEase(motionEase));
    }

    private void AppendFlip(Sequence sequence)
    {
        if (!useFlip) return;

        Vector3 flipScale = actionBaseScale;
        if (flipAxis == FlipAxis.X)
        {
            flipScale.x *= -1f;
        }
        else
        {
            flipScale.y *= -1f;
        }

        sequence.Append(target
            .DOScale(flipScale, Mathf.Max(0.01f, flipDuration))
            .SetEase(Ease.InOutQuad));
        actionBaseScale = flipScale;
    }

    private void AppendShake(Sequence sequence)
    {
        if (!useShake) return;

        sequence.Append(target
            .DOShakePosition(Mathf.Max(0.01f, shakeDuration), shakeStrength, Mathf.Max(1, shakeVibrato), shakeRandomness)
            .SetEase(motionEase));
    }
}

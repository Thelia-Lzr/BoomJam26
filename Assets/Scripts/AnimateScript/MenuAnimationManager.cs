using UnityEngine;
using DG.Tweening; // 引入 DOTween 命名空间

public class MenuAnimationManager : MonoBehaviour
{
    [Header("动画对象")]
    public Transform car;        // 拖入“小车”
    public Transform textLeft;   // 拖入“再跑”
    public Transform textRight;  // 拖入“亿遍”

    [Header("目标位置 (Local Position)")]
    public Vector3 carTargetPos;
    public Vector3 textLeftTargetPos;
    public Vector3 textRightTargetPos;

    [Header("动画时间")]
    public float carMoveDuration = 1.0f;
    public float textMoveDuration = 0.8f;

    void Start()
    {
        PlayEntranceAnimation();
    }

    // public void PlayEntranceAnimation()
    // {
    //     Sequence enterSeq = DOTween.Sequence();
    //
    //     // 第一步：小车移动到目标位置（不变）
    //     enterSeq.Append(car.DOLocalMove(carTargetPos, carMoveDuration).SetEase(Ease.OutBack));
    //
    //     // 第二步：小车结束后 → 左标题先移动
    //     enterSeq.Append(textLeft.DOLocalMove(textLeftTargetPos, textMoveDuration).SetEase(Ease.OutCubic));
    //     // 第三步：左标题移动结束后 → 右标题再移动（核心修改：把 Join 改成 Append）
    //     enterSeq.Append(textRight.DOLocalMove(textRightTargetPos, textMoveDuration).SetEase(Ease.OutCubic));
    // }

    public void PlayEntranceAnimation()
    {
        Sequence enterSeq = DOTween.Sequence();

        // 第一步：小车和左标题一起移动
        enterSeq.Append(car.DOLocalMove(carTargetPos, carMoveDuration).SetEase(Ease.OutBack));
        enterSeq.Join(textLeft.DOLocalMove(textLeftTargetPos, textMoveDuration).SetEase(Ease.OutCubic));

        // 第二步：右标题再移动
        enterSeq.Append(textRight.DOLocalMove(textRightTargetPos, textMoveDuration).SetEase(Ease.OutCubic));
    }
}
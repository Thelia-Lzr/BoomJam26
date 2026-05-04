using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LevelButtonLock : MonoBehaviour
{
    private enum UnlockCondition
    {
        LevelUnlocked,
        LevelCleared
    }

    [Header("Level Lock Settings")]
    [SerializeField] private int levelIndex = 1;
    [SerializeField] private UnlockCondition unlockCondition = UnlockCondition.LevelUnlocked;
    [SerializeField] private bool hideWhenLocked = false;
    [SerializeField] private GameObject lockIcon;
    [SerializeField] private CanvasGroup visualGroup;
    [SerializeField] private float lockedAlpha = 0.45f;
    [SerializeField] private float unlockedAlpha = 1f;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (visualGroup == null) visualGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        RefreshLockState();
    }

    public void RefreshLockState()
    {
        if (button == null) button = GetComponent<Button>();

        bool isUnlocked = IsConditionMet();
        gameObject.SetActive(isUnlocked || !hideWhenLocked);
        button.interactable = isUnlocked;

        if (lockIcon != null)
        {
            lockIcon.SetActive(!isUnlocked);
        }

        if (visualGroup != null)
        {
            visualGroup.alpha = isUnlocked ? unlockedAlpha : lockedAlpha;
        }

        Debug.Log($"[LevelButtonLock] Level {levelIndex} unlocked: {isUnlocked}");
    }

    private bool IsConditionMet()
    {
        switch (unlockCondition)
        {
            case UnlockCondition.LevelCleared:
                return SaveManager.IsLevelCleared(levelIndex);
            case UnlockCondition.LevelUnlocked:
            default:
                return SaveManager.IsLevelUnlocked(levelIndex);
        }
    }
}

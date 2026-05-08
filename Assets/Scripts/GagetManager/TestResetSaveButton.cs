using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class TestResetSaveButton : MonoBehaviour
{
    [Header("Reset Test Save")]
    [SerializeField] private bool refreshLevelButtons = true;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (button == null) button = GetComponent<Button>();
        button.onClick.AddListener(OnClickResetSave);
    }

    private void OnDisable()
    {
        if (button != null) button.onClick.RemoveListener(OnClickResetSave);
    }

    public void OnClickResetSave()
    {
        SaveManager.ResetProgress();

        if (refreshLevelButtons)
        {
            LevelButtonLock[] levelButtons = FindObjectsOfType<LevelButtonLock>(true);
            for (int i = 0; i < levelButtons.Length; i++)
            {
                levelButtons[i].RefreshLockState();
            }
        }

        Debug.Log($"[TestResetSaveButton] Save reset. Unlocked level: {SaveManager.GetUnlockedLevel()}");
    }
}

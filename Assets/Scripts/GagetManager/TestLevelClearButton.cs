using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class TestLevelClearButton : MonoBehaviour
{
    private enum AfterClearAction
    {
        Stay,
        LevelSelect,
        Level1PostStory,
        Level2PostStory
    }

    [Header("Test Clear Settings")]
    [SerializeField] private int clearedLevel = 1;
    [SerializeField] private AfterClearAction afterClearAction = AfterClearAction.LevelSelect;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (button == null) button = GetComponent<Button>();
        button.onClick.AddListener(OnClickCompleteLevel);
    }

    private void OnDisable()
    {
        if (button != null) button.onClick.RemoveListener(OnClickCompleteLevel);
    }

    public void OnClickCompleteLevel()
    {
        SaveManager.CompleteLevel(clearedLevel);
        Debug.Log($"[TestLevelClearButton] Test cleared level {clearedLevel}. Unlocked level: {SaveManager.GetUnlockedLevel()}");

        RunAfterClearAction();
    }

    private void RunAfterClearAction()
    {
        if (afterClearAction == AfterClearAction.Stay)
        {
            return;
        }

        if (SceneController.Instance == null)
        {
            Debug.LogWarning("[TestLevelClearButton] After clear action needs a SceneController in the scene.");
            return;
        }

        switch (afterClearAction)
        {
            case AfterClearAction.LevelSelect:
                SceneController.Instance.GoToLevelSelect();
                break;
            case AfterClearAction.Level1PostStory:
                SceneController.Instance.GoToLevel1PostStory();
                break;
            case AfterClearAction.Level2PostStory:
                SceneController.Instance.GoToLevel2PostStory();
                break;
        }
    }
}

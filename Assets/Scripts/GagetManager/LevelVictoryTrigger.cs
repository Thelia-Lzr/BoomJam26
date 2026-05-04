using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class LevelVictoryTrigger : MonoBehaviour
{
    private enum AfterVictoryAction
    {
        WaitForPopup,
        LevelSelect,
        Level1PostStory,
        Level2PostStory
    }

    [Header("Victory Settings")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private string carTag = "Car";
    [SerializeField] private AfterVictoryAction afterVictoryAction = AfterVictoryAction.WaitForPopup;

    [Header("Popup Hook")]
    [SerializeField] private UnityEvent victoryPopupRequested;

    private bool victoryTriggered;
    private Collider2D triggerCollider;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider2D>();
        triggerCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (victoryTriggered || !other.CompareTag(carTag))
        {
            return;
        }

        TriggerVictory();
    }

    public void TriggerVictory()
    {
        if (victoryTriggered)
        {
            return;
        }

        victoryTriggered = true;
        SaveManager.CompleteLevel(currentLevel);
        Debug.Log($"[LevelVictoryTrigger] Level {currentLevel} cleared. Unlocked level: {SaveManager.GetUnlockedLevel()}");

        victoryPopupRequested?.Invoke();

        if (afterVictoryAction != AfterVictoryAction.WaitForPopup)
        {
            ContinueAfterVictory();
        }
    }

    public void ContinueAfterVictory()
    {
        if (SceneController.Instance == null)
        {
            Debug.LogWarning("[LevelVictoryTrigger] Continue needs a SceneController in the scene.");
            return;
        }

        switch (afterVictoryAction)
        {
            case AfterVictoryAction.LevelSelect:
            case AfterVictoryAction.WaitForPopup:
                SceneController.Instance.GoToLevelSelect();
                break;
            case AfterVictoryAction.Level1PostStory:
                SceneController.Instance.GoToLevel1PostStory();
                break;
            case AfterVictoryAction.Level2PostStory:
                SceneController.Instance.GoToLevel2PostStory();
                break;
        }
    }
}

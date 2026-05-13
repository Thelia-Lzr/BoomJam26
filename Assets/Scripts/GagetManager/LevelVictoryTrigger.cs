using UnityEngine;
[RequireComponent(typeof(Collider2D))]
public class LevelVictoryTrigger : MonoBehaviour
{
    [Header("Victory Settings，记得在场景内配置每一关的层级")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private string carTag = "Car";

    [Header("Popup Hook")]
    [SerializeField] private GameObject victoryPopupRoot;
    // [SerializeField] private MonoBehaviour victoryPopupHandler;

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
        Timer.Instance.EndTimer();
        float passtime = Timer.Instance.curTime;
        victoryTriggered = true;
        SaveManager.CompleteLevel(currentLevel);//解锁新关卡
        Debug.Log($"[LevelVictoryTrigger] Level {currentLevel} cleared. Unlocked level: {SaveManager.GetUnlockedLevel()}");

        if (victoryPopupRoot != null)
        {
            victoryPopupRoot.SetActive(true);
        }

        // if (victoryPopupHandler is IVictoryPopupHandler popupHandler)
        // {
        //     popupHandler.ShowVictoryPopup();
        // }
        // else
        // {
        //     Debug.Log("[LevelVictoryTrigger] Victory popup triggered.");
        // }

        Debug.Log("[LevelVictoryTrigger] Failure判定已封锁。");
    }
}

// public interface IVictoryPopupHandler
// {
//     void ShowVictoryPopup();
// }

using UnityEngine;
using UnityEngine.UI;

public class StartDisappear : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    private bool isSubscribed;

    private void Awake()
    {
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Update()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        if (isSubscribed && LevelManager.Instance != null)
        {
            LevelManager.Instance.StartButtonClicked -= Hide;
        }

        isSubscribed = false;
    }

    private void TrySubscribe()
    {
        if (isSubscribed || LevelManager.Instance == null)
        {
            return;
        }

        LevelManager.Instance.StartButtonClicked += Hide;
        isSubscribed = true;
    }

    private void Hide()
    {
        if (targetImage != null)
        {
            targetImage.enabled = false;
        }
    }
}

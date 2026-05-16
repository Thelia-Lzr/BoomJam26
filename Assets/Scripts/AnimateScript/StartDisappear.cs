using UnityEngine;
using UnityEngine.UI;

public class StartDisappear : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    [SerializeField] private SpriteRenderer targetSprite;
    [SerializeField] private Transform startTarget;
    private bool isSubscribed;

    private void Awake()
    {
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }

        if (targetSprite == null)
        {
            targetSprite = GetComponent<SpriteRenderer>();
        }

        if (startTarget == null)
        {
            GameObject startObject = GameObject.Find("Start");
            if (startObject != null)
            {
                startTarget = startObject.transform;
            }
        }
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Update()
    {
        TrySubscribe();
        UpdatePosition();
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

        if (targetSprite != null)
        {
            targetSprite.enabled = false;
        }
    }

    private void UpdatePosition()
    {
        if (startTarget != null)
        {
            transform.position = startTarget.position;
        }
    }
}

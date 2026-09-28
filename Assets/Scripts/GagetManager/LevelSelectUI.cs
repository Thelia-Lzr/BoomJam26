using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectUI : MonoBehaviour
{
    [SerializeField] private string levelSelectBgmName = "LevelSelect";
    [SerializeField] private string levelSelectSceneName = "LevelChoose";

    [Header("Level Selection")]
    [SerializeField] private LevelButtonLock[] levelButtons = new LevelButtonLock[5];
    [SerializeField] private GameObject[] levelDescriptions = new GameObject[5];
    [SerializeField] private RectTransform rightPanel;
    [SerializeField] private GameObject startButtonObject;
    [SerializeField] private float collapsedOffset = 350f;
    [SerializeField] private float slideDuration = 0.35f;

    [Header("Start Trigger")]
    [SerializeField] private Vector2 triggerPivot = new Vector2(1f, 0.5f);
    [SerializeField] private float hoverAngle = 3f;
    [SerializeField] private float hoverDuration = 0.12f;
    [SerializeField] private float pullAngle = 8f;
    [SerializeField] private float firedAngle = 22f;
    [SerializeField] private float pullDuration = 0.1f;
    [SerializeField] private float firedDuration = 0.16f;
    [SerializeField] private float flashInDuration = 0.07f;
    [SerializeField] private float flashOutDuration = 0.14f;

    private Vector2 expandedPosition;
    private int selectedLevel;
    private Button startButton;
    private RectTransform startButtonRect;
    private Image flashImage;
    private float restAngle;
    private bool starting;
    private bool startHovered;
    private bool startPressed;
    private Sequence startSequence;
    private Newiconzoom[] markers;

    private void Awake()
    {
        markers = new Newiconzoom[levelButtons.Length];
        for (int i = 0; i < levelButtons.Length; i++)
        {
            if (levelButtons[i] == null) continue;
            markers[i] = levelButtons[i].GetComponent<Newiconzoom>();
            if (markers[i] == null) markers[i] = levelButtons[i].gameObject.AddComponent<Newiconzoom>();
        }

        if (rightPanel != null)
        {
            expandedPosition = rightPanel.anchoredPosition;
            rightPanel.anchoredPosition = expandedPosition + Vector2.right * collapsedOffset;
        }

        foreach (GameObject description in levelDescriptions)
            if (description != null) description.SetActive(false);

        if (startButtonObject != null)
        {
            // Description graphics must not intercept clicks intended for Start.
            startButtonObject.transform.SetAsLastSibling();
            startButton = startButtonObject.GetComponent<Button>();
            if (startButton == null) startButton = startButtonObject.AddComponent<Button>();
            if (startButton.targetGraphic == null)
                startButton.targetGraphic = startButtonObject.GetComponent<Graphic>();
            startButtonRect = startButtonObject.GetComponent<RectTransform>();
            Vector2 oldPivot = startButtonRect.pivot;
            startButtonRect.anchoredPosition += Vector2.Scale(triggerPivot - oldPivot, startButtonRect.rect.size);
            startButtonRect.pivot = triggerPivot;
            restAngle = startButtonRect.localEulerAngles.z;

            EventTrigger trigger = startButtonObject.GetComponent<EventTrigger>();
            if (trigger == null) trigger = startButtonObject.AddComponent<EventTrigger>();
            EventTrigger.Entry enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener(_ => OnStartPointerEnter());
            trigger.triggers.Add(enter);
            EventTrigger.Entry exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exit.callback.AddListener(_ => OnStartPointerExit());
            trigger.triggers.Add(exit);
            EventTrigger.Entry down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            down.callback.AddListener(_ => OnStartPointerDown());
            trigger.triggers.Add(down);
            EventTrigger.Entry up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            up.callback.AddListener(_ => OnStartPointerUp());
            trigger.triggers.Add(up);

            startButton.interactable = false;
            startButton.onClick.AddListener(PlayStartAnimation);
            CreateFlashOverlay();
        }
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != levelSelectSceneName) return;

        if (SoundManager.SoundManager.Instance != null)
        {
            SoundManager.SoundManager.Instance.Play(levelSelectBgmName);
        }
    }

    private void OnDestroy()
    {
        if (rightPanel != null) rightPanel.DOKill();
        if (startSequence != null) startSequence.Kill();
        if (startButtonRect != null) startButtonRect.DOKill();
        if (flashImage != null) flashImage.DOKill();
        if (startButton != null) startButton.onClick.RemoveListener(PlayStartAnimation);
    }

    public void SelectLevel(int levelIndex)
    {
        if (starting) return;
        if (levelIndex < 1 || levelIndex > levelButtons.Length) return;

        LevelButtonLock chosen = levelButtons[levelIndex - 1];
        if (chosen == null || chosen.LevelIndex != levelIndex || !chosen.IsUnlocked)
            return;

        selectedLevel = levelIndex;
        for (int i = 0; i < markers.Length; i++)
            if (markers[i] != null) markers[i].SetSelected(i == levelIndex - 1);

        for (int i = 0; i < levelDescriptions.Length; i++)
            if (levelDescriptions[i] != null) levelDescriptions[i].SetActive(i == levelIndex - 1);

        if (rightPanel != null)
        {
            rightPanel.DOKill();
            rightPanel.anchoredPosition = expandedPosition + Vector2.right * collapsedOffset;
            rightPanel.DOAnchorPos(expandedPosition, slideDuration).SetEase(Ease.OutBack);
        }

        if (startButton != null) startButton.interactable = true;
    }

    public void StartSelectedLevel()
    {
        StartLevel(selectedLevel);
    }

    private void OnStartPointerEnter()
    {
        startHovered = true;
        if (starting || startPressed || startButton == null || !startButton.interactable) return;
        RotateStartButton(restAngle + hoverAngle, hoverDuration);
    }

    private void OnStartPointerExit()
    {
        startHovered = false;
        if (!starting && !startPressed) RotateStartButton(restAngle, hoverDuration);
    }

    private void OnStartPointerDown()
    {
        if (starting || startButton == null || !startButton.interactable) return;
        startPressed = true;
        RotateStartButton(restAngle + pullAngle, pullDuration);
    }

    private void OnStartPointerUp()
    {
        if (starting || startButton == null || !startButton.interactable) return;
        startPressed = false;
        RotateStartButton(restAngle + (startHovered ? hoverAngle : 0f), pullDuration);
    }

    private void RotateStartButton(float angle, float duration)
    {
        if (startButtonRect == null) return;
        startButtonRect.DOKill();
        startButtonRect.DOLocalRotate(new Vector3(0f, 0f, angle), duration)
            .SetEase(Ease.OutCubic);
    }

    private void PlayStartAnimation()
    {
        if (starting || selectedLevel < 1 || selectedLevel > levelButtons.Length) return;
        int levelToStart = selectedLevel;
        LevelButtonLock chosen = levelButtons[levelToStart - 1];
        if (chosen == null || !chosen.IsUnlocked) return;

        starting = true;
        startPressed = false;
        startButton.interactable = false;
        startButtonRect.DOKill();
        startSequence = DOTween.Sequence()
            .Append(startButtonRect.DOLocalRotate(
                new Vector3(0f, 0f, restAngle + firedAngle), firedDuration).SetEase(Ease.OutQuad))
            .Append(flashImage.DOFade(1f, flashInDuration))
            .Append(flashImage.DOFade(0f, flashOutDuration))
            .OnComplete(() => StartLevel(levelToStart));
    }

    private void CreateFlashOverlay()
    {
        GameObject overlay = new GameObject("Start Flash", typeof(RectTransform),
            typeof(CanvasRenderer), typeof(Image));
        RectTransform rect = overlay.GetComponent<RectTransform>();
        rect.SetParent(transform, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.SetAsLastSibling();
        flashImage = overlay.GetComponent<Image>();
        flashImage.color = new Color(1f, 1f, 1f, 0f);
        flashImage.raycastTarget = false;
    }

    private void StartLevel(int levelIndex)
    {
        if (levelIndex < 1 || levelIndex > levelButtons.Length) return;
        LevelButtonLock chosen = levelButtons[levelIndex - 1];
        if (chosen == null || !chosen.IsUnlocked) return;

        switch (levelIndex)
        {
            case 1: GoToLevel1PreStory(); break;
            case 2: GoToLevel2PreStory(); break;
            case 3: GoToLevel3PreStory(); break;
            case 4: GoToLevel4PreStory(); break;
            case 5: GoToLevel5PreStory(); break;
        }
    }

    public void GoToLevel1PreStory()
    {
        StopBgm();

        if (SceneController.Instance != null)
        {
            SceneController.Instance.GoToLevel1PreStory();
        }
    }

    public void GoToLevel1PostStory()
    {
        StopBgm();

        if (SceneController.Instance != null)
        {
            SceneController.Instance.GoToLevel1PostStory();
        }
    }

    public void GoToLevel1Battle()
    {
        StopBgm();

        if (SceneController.Instance != null)
        {
            SceneController.Instance.GoToLevel1Battle();
        }
    }

    public void GoToLevel2PreStory()
    {
        StopBgm();

        if (SceneController.Instance != null)
        {
            SceneController.Instance.GoToLevel2PreStory();
        }
    }

    public void GoToLevel2PostStory()
    {
        StopBgm();

        if (SceneController.Instance != null)
        {
            SceneController.Instance.GoToLevel2PostStory();
        }
    }

    public void GoToLevel2Battle()
    {
        StopBgm();

        if (SceneController.Instance != null)
        {
            SceneController.Instance.GoToLevel2Battle();
        }
    }

    public void GoToLevel3PreStory()
    {
        StopBgm();

        if (SceneController.Instance != null)
        {
            SceneController.Instance.GoToLevel3PreStory();
        }
    }

    public void GoToLevel3PostStory()
    {
        StopBgm();

        if (SceneController.Instance != null)
        {
            SceneController.Instance.GoToLevel3PostStory();
        }
    }

    public void GoToLevel3Battle()
    {
        StopBgm();

        if (SceneController.Instance != null)
        {
            SceneController.Instance.GoToLevel3Battle();
        }
    }

    public void GoToLevel4PreStory()
    {
        StopBgm();

        if (SceneController.Instance != null)
        {
            SceneController.Instance.GoToLevel4PreStory();
        }
    }

    public void GoToLevel4PostStory()
    {
        StopBgm();

        if (SceneController.Instance != null)
        {
            SceneController.Instance.GoToLevel4PostStory();
        }
    }

    public void GoToLevel4Battle()
    {
        StopBgm();

        if (SceneController.Instance != null)
        {
            SceneController.Instance.GoToLevel4Battle();
        }
    }

    public void GoToLevel5PreStory()
    {
        StopBgm();

        if (SceneController.Instance != null)
        {
            SceneController.Instance.GoToLevel5PreStory();
        }
    }

    public void GoToLevel5PostStory()
    {
        StopBgm();

        if (SceneController.Instance != null)
        {
            SceneController.Instance.GoToLevel5PostStory();
        }
    }

    public void GoToLevel5Battle()
    {
        StopBgm();

        if (SceneController.Instance != null)
        {
            SceneController.Instance.GoToLevel5Battle();
        }
    }

    public void GoToLevelSelect()
    {
        if (SoundManager.SoundManager.Instance != null)
        {
            SoundManager.SoundManager.Instance.StopAll();
        }

        if (SceneController.Instance != null)
        {
            SceneController.Instance.GoToLevelSelect();
        }
    }

    public void BackToMainMenu()
    {
        StopBgm();

        if (SceneController.Instance != null)
        {
            SceneController.Instance.BackToMainMenu();
        }
    }

    private void StopBgm()
    {
        if (SoundManager.SoundManager.Instance != null)
        {
            SoundManager.SoundManager.Instance.StopAll();
        }
    }
}

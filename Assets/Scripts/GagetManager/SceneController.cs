using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance;

    [Header("Scene Indices")]
    [SerializeField] private int mainMenuSceneIndex = 0;
    [SerializeField] private int levelSelectSceneIndex = 1;
    [SerializeField] private int storySceneIndex = 2;
    [SerializeField] private int level1BattleSceneIndex = 3;
    [SerializeField] private int level2BattleSceneIndex = 4;
    [SerializeField] private int level3BattleSceneIndex = 5;
    [SerializeField] private int level4BattleSceneIndex = 6;
    [SerializeField] private int level5BattleSceneIndex = 7;

    [Header("Story Ids")]
    [SerializeField] private int level1PreStoryId = 0;
    [SerializeField] private int level1PostStoryId = 1;
    [SerializeField] private int level2PreStoryId = 2;
    [SerializeField] private int level2PostStoryId = 3;
    [SerializeField] private int level3PreStoryId = 4;
    [SerializeField] private int level3PostStoryId = 5;
    [SerializeField] private int level4PreStoryId = 6;
    [SerializeField] private int level4PostStoryId = 7;
    [SerializeField] private int level5PreStoryId = 8;
    [SerializeField] private int level5PostStoryId = 9;

    private int nextSceneIndex = -1;
    private int currentStoryId = -1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public int GetCurrentStoryIndex()
    {
        return currentStoryId;
    }

    public void AdvanceStoryIndex()
    {
        currentStoryId++;
    }

    public int GetAndIncrementStoryIndex()
    {
        int indexToReturn = currentStoryId;
        currentStoryId++;
        Debug.Log($"<color=orange>Controller: issued story {indexToReturn}, pointer moved to {currentStoryId}</color>");
        return indexToReturn;
    }

    public void ResetStoryProgress()
    {
        currentStoryId = 0;
        nextSceneIndex = -1;
    }

    public void GoToLevelSelect()
    {
        SceneManager.LoadScene(levelSelectSceneIndex);
    }

    public void GoToLevel1PreStory()
    {
        StartStory(level1PreStoryId, level1BattleSceneIndex);
    }

    public void GoToBattle1()
    {
        GoToLevel1Battle();
    }

    public void GoToLevel1Battle()
    {
        SceneManager.LoadScene(level1BattleSceneIndex);
    }

    public void GoToLevel1PostStory()
    {
        StartStory(level1PostStoryId, levelSelectSceneIndex);
    }

    public void GoToLevel2PreStory()
    {
        StartStory(level2PreStoryId, level2BattleSceneIndex);
    }

    public void GoToBattle2()
    {
        GoToLevel2Battle();
    }

    public void GoToLevel2Battle()
    {
        SceneManager.LoadScene(level2BattleSceneIndex);
    }

    public void GoToLevel2PostStory()
    {
        StartStory(level2PostStoryId, levelSelectSceneIndex);
    }

    public void GoToLevel3PreStory()
    {
        StartStory(level3PreStoryId, level3BattleSceneIndex);
    }

    public void GoToBattle3()
    {
        GoToLevel3Battle();
    }

    public void GoToLevel3Battle()
    {
        SceneManager.LoadScene(level3BattleSceneIndex);
    }

    public void GoToLevel3PostStory()
    {
        StartStory(level3PostStoryId, levelSelectSceneIndex);
    }

    public void GoToLevel4PreStory()
    {
        StartStory(level4PreStoryId, level4BattleSceneIndex);
    }

    public void GoToBattle4()
    {
        GoToLevel4Battle();
    }

    public void GoToLevel4Battle()
    {
        SceneManager.LoadScene(level4BattleSceneIndex);
    }

    public void GoToLevel4PostStory()
    {
        StartStory(level4PostStoryId, levelSelectSceneIndex);
    }

    public void GoToLevel5PreStory()
    {
        StartStory(level5PreStoryId, level5BattleSceneIndex);
    }

    public void GoToBattle5()
    {
        GoToLevel5Battle();
    }

    public void GoToLevel5Battle()
    {
        SceneManager.LoadScene(level5BattleSceneIndex);
    }

    public void GoToLevel5PostStory()
    {
        StartStory(level5PostStoryId, levelSelectSceneIndex);
    }

    public void GoToNextStory()
    {
        SceneManager.LoadScene(storySceneIndex);
    }

    public void OnStoryFinished()
    {
        if (nextSceneIndex < 0)
        {
            Debug.LogWarning("<color=yellow>No next scene configured after story. Returning to level select.</color>");
            GoToLevelSelect();
            return;
        }

        SceneManager.LoadScene(nextSceneIndex);
    }

    public void FinishCurrentStory()
    {
        if (currentStoryId >= 0)
        {
            currentStoryId++;
        }

        OnStoryFinished();
    }

    public void BackToMainMenu()
    {
        ResetState();
        SceneManager.LoadScene(mainMenuSceneIndex);
        Debug.Log("<color=red>[System] Back to main menu</color>");
    }

    public void StartNewGame()
    {
        ResetState();
        GoToLevelSelect();
    }

    private void ResetState()
    {
        currentStoryId = -1;
        nextSceneIndex = -1;
    }

    private void StartStory(int storyId, int sceneIndexAfterStory)
    {
        currentStoryId = storyId;
        nextSceneIndex = sceneIndexAfterStory;
        SceneManager.LoadScene(storySceneIndex);
    }
}

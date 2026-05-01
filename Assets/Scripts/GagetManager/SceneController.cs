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

    [Header("Story Ids")]
    [SerializeField] private int level1PreStoryId = 0;
    [SerializeField] private int level1PostStoryId = 1;
    [SerializeField] private int level2PreStoryId = 2;
    [SerializeField] private int level2PostStoryId = 3;

    private int nextSceneIndex = -1;
    private int currentStoryId = -1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 确保它跨场景存在
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // 重点：如果已经有一个了，就把新出来的删掉
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
        Debug.Log($"<color=orange>控制器：发放剧本 {indexToReturn}，进度指针已移至 {currentStoryId}</color>");
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

    public void GoToBattle3()
    {
        Debug.LogWarning("<color=yellow>未配置第三关战斗场景，返回选关。</color>");
        GoToLevelSelect();
    }

    public void GoToNextStory()
    {
        SceneManager.LoadScene(storySceneIndex);
    }

    public void OnStoryFinished()
    {
        if (nextSceneIndex < 0)
        {
            Debug.LogWarning("<color=yellow>未配置剧情结束的跳转场景，返回选关。</color>");
            GoToLevelSelect();
            return;
        }

        SceneManager.LoadScene(nextSceneIndex);
    }

    public void BackToMainMenu()
    {
        ResetState();
        SceneManager.LoadScene(mainMenuSceneIndex);
        Debug.Log("<color=red>【系统】返回主菜单</color>");
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
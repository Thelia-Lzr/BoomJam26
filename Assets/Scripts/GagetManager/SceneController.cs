using UnityEngine;
using UnityEngine.SceneManagement;


//1.配置选关场景
//打开选关场景，确认已保存（Ctrl+S）。
//在 File > Build Settings 中把选关场景加入 Scenes In Build，并记住它的索引（例如 1）。
//2.配置 SceneController
//在常驻场景（通常是主界面）里放一个挂了 SceneController 的物体（确保它会被加载）。
//在 Inspector 中设置：
//          levelSelectSceneIndex = 选关场景的 Build Index
//          mainMenuSceneIndex、storySceneIndex 等按需配置
//3.给“开始游戏”按钮挂方法
//选中主界面的 Button。
//在 On Click() 里新增一条事件。
//把挂了 SceneController 的物体拖进去。
//选择 SceneController.GoToLevelSelect()（或 StartNewGame() 也可以）。
public class SceneController : MonoBehaviour
{
    public static SceneController Instance;

    [Header("Scene Indices")]
    [SerializeField] private int mainMenuSceneIndex = 0; //默认主菜单是第0个场景，根据实际情况调整
    [SerializeField] private int levelSelectSceneIndex = 1; //选关场景的 Build Index
    [SerializeField] private int storySceneIndex = 2; //剧情场景的 Build Index
    [SerializeField] private int level1BattleSceneIndex = 3; //关卡1战斗场景的 Build Index
    [SerializeField] private int level2BattleSceneIndex = 4; //关卡2战斗场景的 Build Index
    [Header("Story Ids")]
    [SerializeField] private int level1PreStoryId = 0;//关卡1前置剧情的 ID
    [SerializeField] private int level1PostStoryId = 1; //关卡1后置剧情的 ID
    [SerializeField] private int level2PreStoryId = 2; //关卡2前置剧情的 ID
    [SerializeField] private int level2PostStoryId = 3; //关卡2后置剧情的 ID

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

    public void GoToLevelSelect()
    {
        SceneManager.LoadScene(levelSelectSceneIndex);
    }

    public void GoToLevel1PreStory()
    {
        StartStory(level1PreStoryId, level1BattleSceneIndex);
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

    public void GoToLevel2Battle()
    {
        SceneManager.LoadScene(level2BattleSceneIndex);
    }

    public void GoToLevel2PostStory()
    {
        StartStory(level2PostStoryId, levelSelectSceneIndex);
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
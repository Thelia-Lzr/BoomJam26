using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private string mainMenuBgmName = "MainMenu";

    void Start()
    {
        if (SoundManager.SoundManager.Instance != null)
        {
            SoundManager.SoundManager.Instance.Play(mainMenuBgmName);
        }
    }

    public void OnClickStart()
    {
        Debug.Log(">>> 1. 按钮确实被点到了！");

        if (SoundManager.SoundManager.Instance != null)
        {
            SoundManager.SoundManager.Instance.StopAll();
        }

        if (SceneController.Instance != null)
        {
            Debug.Log(">>> 2. 找到了 SceneController 实例！");
            SceneController.Instance.ResetStoryProgress();

            Debug.Log(">>> 3. 准备跳转到选关场景...");
            SceneController.Instance.GoToLevelSelect();
        }
        else
        {
            Debug.LogError(">>> 2. 报错：找不到 SceneController 实例！请务必从 Init 场景启动！");
        }
    }

    // 绑定到“退出游戏”按钮
    public void OnClickQuit()
    {
        PlayClickSound();
        Debug.Log("退出游戏");
        Application.Quit(); // 仅在打包后有效
    }

    private void PlayClickSound()
    {
       // if (AudioManager.Instance != null)
        {
       //     AudioManager.Instance.PlaySFX(AudioManager.SfxType.UI, "sfx_ui_click");
        }
    }
}

using UnityEngine;

public class LevelSelectUI : MonoBehaviour
{
    [SerializeField] private string levelSelectBgmName = "LevelSelect";

    private void Start()
    {
        if (SoundManager.SoundManager.Instance != null)
        {
            SoundManager.SoundManager.Instance.Play(levelSelectBgmName);
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

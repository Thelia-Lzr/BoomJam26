using UnityEngine;

public class LevelSelectUI : MonoBehaviour
{
    public void GoToLevel1PreStory()
    {
        if (SceneController.Instance != null)
        {
            SceneController.Instance.GoToLevel1PreStory();
        }
    }

    public void GoToLevel1PostStory()
    {
        if (SceneController.Instance != null)
        {
            SceneController.Instance.GoToLevel1PostStory();
        }
    }

    public void GoToLevel1Battle()
    {
        if (SceneController.Instance != null)
        {
            SceneController.Instance.GoToLevel1Battle();
        }
    }

    public void GoToLevel2PreStory()
    {
        if (SceneController.Instance != null)
        {
            SceneController.Instance.GoToLevel2PreStory();
        }
    }

    public void GoToLevel2PostStory()
    {
        if (SceneController.Instance != null)
        {
            SceneController.Instance.GoToLevel2PostStory();
        }
    }

    public void GoToLevel2Battle()
    {
        if (SceneController.Instance != null)
        {
            SceneController.Instance.GoToLevel2Battle();
        }
    }

    public void GoToLevel3PreStory()
    {
        if (SceneController.Instance != null)
        {
            SceneController.Instance.GoToLevel3PreStory();
        }
    }

    public void GoToLevel3PostStory()
    {
        if (SceneController.Instance != null)
        {
            SceneController.Instance.GoToLevel3PostStory();
        }
    }

    public void GoToLevel3Battle()
    {
        if (SceneController.Instance != null)
        {
            SceneController.Instance.GoToLevel3Battle();
        }
    }

    public void GoToLevel4PreStory()
    {
        if (SceneController.Instance != null)
        {
            SceneController.Instance.GoToLevel4PreStory();
        }
    }

    public void GoToLevel4PostStory()
    {
        if (SceneController.Instance != null)
        {
            SceneController.Instance.GoToLevel4PostStory();
        }
    }

    public void GoToLevel4Battle()
    {
        if (SceneController.Instance != null)
        {
            SceneController.Instance.GoToLevel4Battle();
        }
    }

    public void GoToLevel5PreStory()
    {
        if (SceneController.Instance != null)
        {
            SceneController.Instance.GoToLevel5PreStory();
        }
    }

    public void GoToLevel5PostStory()
    {
        if (SceneController.Instance != null)
        {
            SceneController.Instance.GoToLevel5PostStory();
        }
    }

    public void GoToLevel5Battle()
    {
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
        if (SceneController.Instance != null)
        {
            SceneController.Instance.BackToMainMenu();
        }
    }
}

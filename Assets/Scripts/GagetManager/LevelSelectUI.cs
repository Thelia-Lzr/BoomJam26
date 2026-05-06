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

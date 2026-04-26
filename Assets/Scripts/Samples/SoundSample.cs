using UnityEngine;
using SoundManager;

public class SoundSample : MonoBehaviour
{
    private void Start()
    {
        SoundManager.SoundManager.Instance.Play("TIDT");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Star : MonoBehaviour
{
    public bool isCollected;
    public Collider2D checkcol;
    public Collider2D[] results = new Collider2D[10];
    void Start()
    {
        checkcol = GetComponent<Collider2D>();
    }

    void FixedUpdate()
    {
        if (LevelManager.Instance.currentMode == LevelManager.CurrentMode.PlayMode)
        {
            int count = checkcol.GetContacts(results);
            for (int i = 0; i < count; i++)
            {
                if (results[i].CompareTag("Car") && !isCollected)
                {
                    this.isCollected = true;
                    this.GetComponent<SpriteRenderer>().enabled = false;
                    StarUI.Instance.currentStar += 1;
                }
            }
        }
        else
        {
            this.isCollected = false;
            this.GetComponent<SpriteRenderer>().enabled = true;
        }
    }
}
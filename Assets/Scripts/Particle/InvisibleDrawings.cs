using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class InvisibleDrawings : MonoBehaviour
{
    [System.Serializable]
    public struct VisibleWhile
    {
        public bool Python;
        public bool Java;
        public bool C;
    }
    [SerializeField] private VisibleWhile visibleWhile;
    [SerializeField] private bool visibleAtStart = false;
    
    private SpriteRenderer thisRenderer;
    private Collider2D collider2d;
    
    private Collider2D[] zoneResults = new Collider2D[10];
    private bool swap;
    
    // Start is called before the first frame update
    void Start()
    {
        thisRenderer = GetComponent<SpriteRenderer>();
        collider2d = GetComponent<Collider2D>();
        if (!visibleAtStart)
        {
            thisRenderer.enabled = false;
        }
        else
        {
            thisRenderer.enabled = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {

        int count = collider2d.GetContacts(zoneResults);
        //初始化
        swap = false;
        for (int i = 0; i < count; i++)
        {
            if (zoneResults[i].TryGetComponent<SwapZone>(out var swapZone) && visibleWhile.Python)
            {
                swap = true;
            }
            
            if (zoneResults[i].TryGetComponent<GravityZone>(out var gravityZone) && visibleWhile.Java)
            {
                swap = true;
            }

            if (zoneResults[i].TryGetComponent<SpeedingZone>(out var speedingZone) && visibleWhile.C)
            {
                swap = true;
            }
        }

        if (swap ^ visibleAtStart)
        {
            thisRenderer.enabled = true;
        }
        else
        {
            thisRenderer.enabled = false;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleBase : MonoBehaviour
{

    [Header("透明状态下的图片透明度值")]
    [Range(0, 1)]
    public float transparentAlpha = 0.2f;
    //是否能被破坏
    public bool CanBeDestroy;
    //是否能被隐藏，默认打开
    public bool CanBeInvisible;
    
    [SerializeField] public bool notVisibleAtStart = false;
    private bool swap;

    // 内部组件引用
    public SpriteRenderer spriteRenderer;
    public Collider2D collider2d;
    private Collider2D[] zoneResults = new Collider2D[10];
    void Awake()
    {

    }

    public void Update()
    {
        //collider2d.enabled = false;
        int count = collider2d.GetContacts(zoneResults);
        swap = false;
        //可优化
        
        //初始化
        for (int i = 0; i < count; i++)
        {
            if (zoneResults[i].TryGetComponent<SwapZone>(out var swapZone))
            {
                swap = true;
            }
            
        }
        
        //处理
        if ((swap ^ notVisibleAtStart) && CanBeInvisible)
        {
            Color color = spriteRenderer.color;
            color.a = transparentAlpha;
            spriteRenderer.color = color;
            collider2d.isTrigger = true;
        }
        else
        {
            Color color = spriteRenderer.color;
            color.a = 1;
            spriteRenderer.color = color;
            collider2d.isTrigger = false;
        }
    }
    //接口：被破坏
    public void BeDestroy()
    {
        if (CanBeDestroy)
        {
            collider2d.enabled = false;
            spriteRenderer.enabled = false;
            //gameObject.SetActive(false);
            //播放破碎动画

        }
    }
    //接口：暂时透明
    public void BeInvisible()
    {
        if (CanBeInvisible)
        {
            Color color = spriteRenderer.color;
            color.a = transparentAlpha;
            spriteRenderer.color = color;
            collider2d.isTrigger = true;
            //gameObject.SetActive(false);

        }
    }
    //接口：恢复成默认状态
    public void BeReturn()
    {
        Color color = spriteRenderer.color;
        color.a = 1;
        spriteRenderer.color = color;
        collider2d.enabled = true;
    }
}

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

    // 内部组件引用
    public SpriteRenderer spriteRenderer;
    public Collider2D collider2d;
    void Awake()
    {

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
            collider2d.enabled = false;
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

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class In2Scene : MonoBehaviour
{
    private enum EnterEffectMode
    {
        PointReveal,
        CameraZoom
    }

    [Header("Mode")]
    [SerializeField] private EnterEffectMode effectMode = EnterEffectMode.PointReveal;

    [Header("Camera")]
    [SerializeField] private Camera targetCamera;

    [Header("Timing")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private float duration = 0.35f;
    [SerializeField] private AnimationCurve revealCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Point Reveal")]
    [SerializeField] private Color maskColor = Color.black;
    [SerializeField] private float startWindowScale = 0f;
    [SerializeField] private float edgeSoftness = 0.0002f;
    [SerializeField] private int sortingOrder = 999;

    [Header("Camera Zoom")]
    [SerializeField] private float startViewScale = 0.25f;

    private float targetOrthographicSize;
    private float targetFieldOfView;
    private Coroutine effectCoroutine;
    private Canvas revealCanvas;
    private RawImage revealMask;
    private Material revealMaterial;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void Start()
    {
        if (playOnStart)
        {
            Play();
        }
    }

    public void Play()
    {
        if (effectCoroutine != null)
        {
            StopCoroutine(effectCoroutine);
        }

        effectCoroutine = effectMode == EnterEffectMode.PointReveal
            ? StartCoroutine(PlayPointReveal())
            : StartCoroutine(PlayZoomIn());
    }

    private IEnumerator PlayPointReveal()
    {
        duration = Mathf.Max(0.01f, duration);
        startWindowScale = Mathf.Clamp(startWindowScale, 0f, 1f);

        CreatePointRevealCanvas();

        float aspect = (float)Screen.width / Screen.height;
        float endRadius = Mathf.Sqrt(0.25f * aspect * aspect + 0.25f);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float curvedT = revealCurve.Evaluate(t);

            float radius = Mathf.Lerp(startWindowScale, endRadius, curvedT);
            UpdateRevealMask(radius, aspect);

            yield return null;
        }

        DestroyPointRevealCanvas();
        effectCoroutine = null;
    }

    private void CreatePointRevealCanvas()
    {
        DestroyPointRevealCanvas();

        GameObject canvasObject = new GameObject("ScenePointReveal");
        revealCanvas = canvasObject.AddComponent<Canvas>();
        revealCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        revealCanvas.sortingOrder = sortingOrder;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject maskObject = new GameObject("BlackRevealMask");
        maskObject.transform.SetParent(revealCanvas.transform, false);
        revealMask = maskObject.AddComponent<RawImage>();
        revealMask.texture = Texture2D.whiteTexture;
        revealMask.raycastTarget = false;
        revealMaterial = new Material(Shader.Find("UI/ScenePointRevealMask"));
        revealMask.material = revealMaterial;
        StretchToScreen(revealMask.rectTransform);

        float aspect = (float)Screen.width / Screen.height;
        UpdateRevealMask(startWindowScale, aspect);
    }

    private void UpdateRevealMask(float radius, float aspect)
    {
        if (revealMaterial == null) return;

        revealMaterial.SetColor("_Color", maskColor);
        revealMaterial.SetFloat("_Radius", radius);
        revealMaterial.SetFloat("_Aspect", aspect);
        revealMaterial.SetFloat("_EdgeSoftness", edgeSoftness);
    }

    private void StretchToScreen(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    private void DestroyPointRevealCanvas()
    {
        if (revealCanvas != null)
        {
            Destroy(revealCanvas.gameObject);
            revealCanvas = null;
        }

        if (revealMaterial != null)
        {
            Destroy(revealMaterial);
            revealMaterial = null;
        }
    }

    private IEnumerator PlayZoomIn()
    {
        if (targetCamera == null) yield break;

        startViewScale = Mathf.Clamp(startViewScale, 0.01f, 1f);
        duration = Mathf.Max(0.01f, duration);

        if (targetCamera.orthographic)
        {
            targetOrthographicSize = targetCamera.orthographicSize;
            targetCamera.orthographicSize = targetOrthographicSize * startViewScale;
        }
        else
        {
            targetFieldOfView = targetCamera.fieldOfView;
            targetCamera.fieldOfView = targetFieldOfView * startViewScale;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float curvedT = revealCurve.Evaluate(t);

            if (targetCamera.orthographic)
            {
                targetCamera.orthographicSize = Mathf.Lerp(targetOrthographicSize * startViewScale, targetOrthographicSize, curvedT);
            }
            else
            {
                targetCamera.fieldOfView = Mathf.Lerp(targetFieldOfView * startViewScale, targetFieldOfView, curvedT);
            }

            yield return null;
        }

        if (targetCamera.orthographic)
        {
            targetCamera.orthographicSize = targetOrthographicSize;
        }
        else
        {
            targetCamera.fieldOfView = targetFieldOfView;
        }

        effectCoroutine = null;
    }
}

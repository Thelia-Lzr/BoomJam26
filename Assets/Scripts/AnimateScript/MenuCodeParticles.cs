using TMPro;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class MenuCodeParticles : MonoBehaviour
{
    [Header("Code Particles")]
    [SerializeField, Range(1, 80)] private int particleCount = 24;
    [SerializeField, Min(1f)] private float minFontSize = 14f;
    [SerializeField, Min(1f)] private float maxFontSize = 23f;
    [SerializeField] private float minLifetime = 3f;
    [SerializeField] private float maxLifetime = 7f;
    [SerializeField] private float maxDriftSpeed = 14f;
    [SerializeField, Range(0f, 1f)] private float minOpacity = 0.08f;
    [SerializeField, Range(0f, 1f)] private float maxOpacity = 0.22f;
    [SerializeField, Range(0f, 1f)] private float whiteChance = 0.25f;
    [SerializeField] private Color codeRed = new Color(0.55f, 0.08f, 0.13f);
    [SerializeField] private Color codeWhite = new Color(0.8f, 0.86f, 0.88f);

    private static readonly string[] Samples =
    {
        "011101", "10101011", "bootstrap", "Layer", "{ }", "//", "->", "[01]",
        "print", "Recover", "#include", "#07", "DATA", "::", "0101"
    };

    private sealed class CodeParticle
    {
        public RectTransform rect;
        public TextMeshProUGUI label;
        public Vector2 velocity;
        public Color color;
        public float age;
        public float lifetime;
        public float opacity;
        public float flickerSpeed;
        public float flickerPhase;
        public float fontSizeFactor;
    }

    private RectTransform layer;
    private CodeParticle[] particles;
    private float appliedMinFontSize;
    private float appliedMaxFontSize;

    private void Start()
    {
        GameObject layerObject = new GameObject("Code Particles", typeof(RectTransform));
        layer = layerObject.GetComponent<RectTransform>();
        layer.SetParent(transform, false);
        layer.anchorMin = Vector2.zero;
        layer.anchorMax = Vector2.one;
        layer.offsetMin = Vector2.zero;
        layer.offsetMax = Vector2.zero;
        layer.SetSiblingIndex(1); // Above BG, below the menu interface.

        Canvas.ForceUpdateCanvases();
        particles = new CodeParticle[particleCount];
        for (int i = 0; i < particleCount; i++)
        {
            GameObject glyph = new GameObject("Code Glyph", typeof(RectTransform),
                typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            RectTransform rect = glyph.GetComponent<RectTransform>();
            rect.SetParent(layer, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(180f, 36f);

            TextMeshProUGUI label = glyph.GetComponent<TextMeshProUGUI>();
            label.font = TMP_Settings.defaultFontAsset;
            label.alignment = TextAlignmentOptions.Center;
            label.enableWordWrapping = false;
            label.raycastTarget = false;
            label.color = Color.clear;

            particles[i] = new CodeParticle
            {
                rect = rect,
                label = label,
                fontSizeFactor = Random.value
            };
            ResetParticle(particles[i]);
            particles[i].age = Random.Range(0f, particles[i].lifetime);
        }
        appliedMinFontSize = minFontSize;
        appliedMaxFontSize = maxFontSize;
    }

    private void Update()
    {
        if (particles == null) return;
        if (minFontSize != appliedMinFontSize || maxFontSize != appliedMaxFontSize)
        {
            foreach (CodeParticle particle in particles) ApplyFontSize(particle);
            appliedMinFontSize = minFontSize;
            appliedMaxFontSize = maxFontSize;
        }
        float deltaTime = Time.unscaledDeltaTime;
        foreach (CodeParticle particle in particles)
        {
            particle.age += deltaTime;
            if (particle.age >= particle.lifetime) ResetParticle(particle);

            particle.rect.anchoredPosition += particle.velocity * deltaTime;
            float life = particle.age / particle.lifetime;
            float fade = Mathf.Sin(Mathf.PI * life);
            float flicker = 0.78f + 0.22f * Mathf.Sin(
                particle.age * particle.flickerSpeed + particle.flickerPhase);
            Color color = particle.color;
            color.a = particle.opacity * fade * flicker;
            particle.label.color = color;
        }
    }

    private void ResetParticle(CodeParticle particle)
    {
        Vector2 size = layer.rect.size;
        particle.rect.anchoredPosition = new Vector2(
            Random.Range(-size.x * 0.5f, size.x * 0.5f),
            Random.Range(-size.y * 0.5f, size.y * 0.5f));
        particle.label.text = Samples[Random.Range(0, Samples.Length)];
        particle.fontSizeFactor = Random.value;
        ApplyFontSize(particle);
        particle.color = Random.value < whiteChance ? codeWhite : codeRed;
        particle.velocity = new Vector2(
            Random.Range(-maxDriftSpeed, maxDriftSpeed),
            Random.Range(-maxDriftSpeed, maxDriftSpeed));
        particle.lifetime = Random.Range(
            Mathf.Max(0.1f, minLifetime), Mathf.Max(0.1f, maxLifetime));
        particle.opacity = Random.Range(minOpacity, maxOpacity);
        particle.flickerSpeed = Random.Range(5f, 11f);
        particle.flickerPhase = Random.Range(0f, Mathf.PI * 2f);
        particle.age = 0f;
    }

    private void ApplyFontSize(CodeParticle particle)
    {
        float min = Mathf.Max(1f, minFontSize);
        float max = Mathf.Max(min, maxFontSize);
        particle.label.fontSize = Mathf.Lerp(min, max, particle.fontSizeFactor);
        particle.rect.sizeDelta = new Vector2(
            Mathf.Max(180f, particle.label.preferredWidth + 16f),
            Mathf.Max(36f, particle.label.preferredHeight + 8f));
    }
}

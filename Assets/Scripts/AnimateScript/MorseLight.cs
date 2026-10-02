using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Image))]
public class MorseLight : MonoBehaviour
{
    // Edit this string to change the message: . = short, - = long,
    // space = next letter, / = next word. Example: SOS = ... --- ...
    private const string MorseCode = ".-. . -.-. --- ...- . .-.";

    [SerializeField, Min(0.02f)] private float unitDuration = 0.2f;
    [SerializeField, Range(0f, 1f)] private float dimOpacity = 0.12f;
    [SerializeField, Range(0f, 1f)] private float brightOpacity = 1f;
    [SerializeField, Min(0f)] private float repeatPause = 1.2f;

    private struct Step
    {
        public bool lit;
        public float duration;

        public Step(bool lit, float duration)
        {
            this.lit = lit;
            this.duration = duration;
        }
    }

    private readonly List<Step> steps = new List<Step>();
    private Image lightImage;
    private Color originalColor;
    private int stepIndex;
    private float stepTime;

    private void OnEnable()
    {
        lightImage = GetComponent<Image>();
        originalColor = lightImage.color;
        BuildSequence();
        stepIndex = 0;
        stepTime = 0f;
        UpdateOpacity();
    }

    private void Update()
    {
        if (steps.Count == 0) return;

        stepTime += Time.unscaledDeltaTime;
        while (stepTime >= steps[stepIndex].duration)
        {
            stepTime -= steps[stepIndex].duration;
            stepIndex = (stepIndex + 1) % steps.Count;
        }
        UpdateOpacity();
    }

    private void OnDisable()
    {
        if (lightImage != null) lightImage.color = originalColor;
    }

    private void BuildSequence()
    {
        steps.Clear();
        float unit = Mathf.Max(0.02f, unitDuration);

        for (int i = 0; i < MorseCode.Length; i++)
        {
            char symbol = MorseCode[i];
            if (symbol == '.' || symbol == '-')
            {
                steps.Add(new Step(true, unit * (symbol == '.' ? 1f : 3f)));
                if (i + 1 < MorseCode.Length &&
                    (MorseCode[i + 1] == '.' || MorseCode[i + 1] == '-'))
                    steps.Add(new Step(false, unit));
            }
            else if (symbol == ' ' || symbol == '/')
            {
                steps.Add(new Step(false, unit * (symbol == ' ' ? 3f : 7f)));
            }
        }

        if (steps.Count > 0) steps.Add(new Step(false, Mathf.Max(0.02f, repeatPause)));
    }

    private void UpdateOpacity()
    {
        float opacity = dimOpacity;
        if (steps.Count > 0 && steps[stepIndex].lit)
        {
            float progress = Mathf.Clamp01(stepTime / steps[stepIndex].duration);
            float breath = Mathf.SmoothStep(0f, 1f, Mathf.Sin(Mathf.PI * progress));
            opacity = Mathf.Lerp(dimOpacity, brightOpacity, breath);
        }

        Color color = originalColor;
        color.a *= opacity;
        lightImage.color = color;
    }
}

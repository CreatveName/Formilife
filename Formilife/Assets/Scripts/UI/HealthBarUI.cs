using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private AntNeeds needs;   // whose health to show
    [SerializeField] private Image fill;       // Image Type = Filled, Fill Method = Horizontal
    [SerializeField] private Image outline;    // optional: outline image that should also pulse red when low
    [SerializeField] private CanvasGroup group; // optional: hides just this bar, not the whole canvas

    [Header("Behaviour")]
    [SerializeField] private bool hideUntilGameStarted = true;
    [SerializeField] private float smoothing = 8f; // 0 = snap instantly; higher = snappier

    [Header("Low Glow")]
    [Tooltip("Fill pulses red when at or below this fraction.")]
    [Range(0f, 1f)]
    [SerializeField] private float lowGlowThreshold = 0.25f;
    [SerializeField] private Color lowGlowColor = new Color(1f, 0.15f, 0.15f);
    [SerializeField] private float lowGlowSpeed = 5f;
    [Tooltip("Optional Image shown around the bar when the stat is low (pulses with the same color).")]
    [SerializeField] private Image lowGlowOverlay;

    private Color baseColor;
    private Color baseOutlineColor;
    private bool baseColorCached;

    private void Update()
    {
        if (needs == null || fill == null) return;

        if (!baseColorCached)
        {
            baseColor = fill.color;
            if (outline != null) baseOutlineColor = outline.color;
            baseColorCached = true;
        }

        if (hideUntilGameStarted && group != null)
        {
            group.alpha = StartMenu.GameStarted ? 1f : 0f;
        }

        float target = Mathf.Clamp01(needs.GetHealthNormalized());
        fill.fillAmount = smoothing > 0f
            ? Mathf.Lerp(fill.fillAmount, target, 1f - Mathf.Exp(-smoothing * Time.deltaTime))
            : target;

        bool low = target <= lowGlowThreshold;
        if (low)
        {
            float t = (Mathf.Sin(Time.unscaledTime * lowGlowSpeed) + 1f) * 0.5f;
            fill.color = Color.Lerp(baseColor, lowGlowColor, t);
            if (outline != null) outline.color = Color.Lerp(baseOutlineColor, lowGlowColor, t);
            if (lowGlowOverlay != null)
            {
                if (!lowGlowOverlay.gameObject.activeSelf) lowGlowOverlay.gameObject.SetActive(true);
                Color c = lowGlowColor; c.a = t;
                lowGlowOverlay.color = c;
            }
        }
        else
        {
            fill.color = baseColor;
            if (outline != null) outline.color = baseOutlineColor;
            if (lowGlowOverlay != null && lowGlowOverlay.gameObject.activeSelf)
                lowGlowOverlay.gameObject.SetActive(false);
        }
    }
}

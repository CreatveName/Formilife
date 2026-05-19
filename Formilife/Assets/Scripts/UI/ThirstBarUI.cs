using UnityEngine;
using UnityEngine.UI;

// Drives a three-layer thirst bar (outline + fill + background) from an
// AntNeeds component. Only the Fill image needs wiring: it must be an
// Image with Image Type = Filled (Horizontal). The outline and background
// are just static images layered behind/in front in the hierarchy.
public class ThirstBarUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private AntNeeds needs;   // whose thirst to show
    [SerializeField] private Image fill;       // Image Type = Filled, Fill Method = Horizontal
    [SerializeField] private CanvasGroup group; // optional: hides just this bar, not the whole canvas

    [Header("Behaviour")]
    [SerializeField] private bool hideUntilGameStarted = true;
    [SerializeField] private float smoothing = 8f; // 0 = snap instantly; higher = snappier

    private void Update()
    {
        if (needs == null || fill == null) return;

        if (hideUntilGameStarted && group != null)
        {
            group.alpha = StartMenu.GameStarted ? 1f : 0f;
        }

        float target = Mathf.Clamp01(needs.GetThirstNormalized());
        fill.fillAmount = smoothing > 0f
            ? Mathf.Lerp(fill.fillAmount, target, 1f - Mathf.Exp(-smoothing * Time.deltaTime))
            : target;
    }
}

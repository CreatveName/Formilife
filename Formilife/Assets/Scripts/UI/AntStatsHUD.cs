using UnityEngine;

[RequireComponent(typeof(AntNeeds))]
public class AntStatsHUD : MonoBehaviour
{
    [SerializeField] private Vector2 padding = new Vector2(12, 12);
    [SerializeField] private float barWidth = 180f;
    [SerializeField] private float barHeight = 14f;
    [SerializeField] private float lineSpacing = 22f;
    [SerializeField] private int fontSize = 13;
    [SerializeField] private float labelWidth = 60f;

    [Header("Bar Colors")]
    [SerializeField] private Color healthColor = Color.red;
    [SerializeField] private Color hungerColor = new Color(1f, 0.6f, 0.1f);
    [SerializeField] private Color thirstColor = new Color(0.2f, 0.6f, 1f);

    private AntNeeds needs;
    private GUIStyle labelStyle;
    private Texture2D bgTex;
    private Texture2D fillTex;

    private void Awake()
    {
        needs = GetComponent<AntNeeds>();
    }

    private void OnGUI() {
        if (needs == null) return;
        EnsureStyles();
        float x = padding.x;
        float y = padding.y;

        DrawStat(ref y, x, "Health", needs.GetHealthNormalized(), healthColor);
        DrawStat(ref y, x, "Hunger", needs.GetHungerNormalized(), hungerColor);
        DrawStat(ref y, x, "Thirst", needs.GetThirstNormalized(), thirstColor);
    }

    private void DrawStat(ref float y, float x, string label, float normalized, Color fillColor)
    {
        normalized = Mathf.Clamp01(normalized);

        GUI.Label(new Rect(x, y, labelWidth, lineSpacing), label, labelStyle);

        Rect barRect = new Rect(x + labelWidth, y + 2f, barWidth, barHeight);
        GUI.DrawTexture(barRect, bgTex);

        Rect fillRect = new Rect(barRect.x, barRect.y, barRect.width * normalized, barRect.height);
        Color prev = GUI.color;
        GUI.color = fillColor;
        GUI.DrawTexture(fillRect, fillTex);
        GUI.color = prev;

        y += lineSpacing;
    }

    private void EnsureStyles()
    {
        if (labelStyle == null || labelStyle.fontSize != fontSize)
        {
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.normal.textColor = Color.white;
            labelStyle.fontSize = fontSize;
            labelStyle.fontStyle = FontStyle.Bold;
        }
        if (bgTex == null) bgTex = MakeTex(new Color(0f, 0f, 0f, 0.6f));
        if (fillTex == null) fillTex = MakeTex(Color.white);
    }

    private static Texture2D MakeTex(Color c)
    {
        Texture2D t = new Texture2D(1, 1);
        t.SetPixel(0, 0, c);
        t.Apply();
        return t;
    }
}

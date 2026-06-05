using System.Collections.Generic;
using UnityEngine;

// VN-style dialogue overlay. Lines are queued and shown one at a time at the
// bottom of the screen; clicking the box advances. The Queen "speaks" both at
// game start (intro + first quest) and whenever the active quest changes.
public class QueenDialogue : MonoBehaviour
{
    public static QueenDialogue Instance { get; private set; }

    [Header("Layout")]
    [SerializeField] private float boxHeight = 240f;
    [SerializeField] private float horizontalMargin = 80f;
    [SerializeField] private float bottomMargin = 30f;
    [SerializeField] private float padding = 24f;

    [Header("Font")]
    [Tooltip("If set, all dialogue text uses this font (Itim-Regular).")]
    [SerializeField] private Font font;

    [Header("Text")]
    [SerializeField] private string speakerName = "Queen";
    [SerializeField] private int nameFontSize = 26;
    [SerializeField] private int bodyFontSize = 20;
    [SerializeField] private int hintFontSize = 22;

    [Header("Background")]
    [Tooltip("Sprite drawn as the dialogue box background (Long board.png).")]
    [SerializeField] private Sprite backgroundSprite;
    [Tooltip("Clip drawn on the top edge of the board behind the speaker name (e.g. Quest Bar.png).")]
    [SerializeField] private Sprite nameBarSprite;
    [Tooltip("Width of the name bar in pixels.")]
    [SerializeField] private float nameBarWidth = 220f;
    [Tooltip("Nudge the name bar: +X moves right, +Y moves down.")]
    [SerializeField] private Vector2 nameBarOffset = new Vector2(40f, 0f);
    [Tooltip("Nudge the name text within the bar: +X right, +Y down.")]
    [SerializeField] private Vector2 nameTextOffset = Vector2.zero;

    [Header("Colors")]
    [SerializeField] private Color boxColor = new Color(0.08f, 0.06f, 0.04f, 0.92f);
    [SerializeField] private Color nameColor = new Color(1f, 0.85f, 0.4f);
    [SerializeField] private Color bodyColor = Color.black;

    [Header("Dialogue Lines")]
    [TextArea(2, 8)] [SerializeField] private string[] introLines = new[]
    {
        "Welcome to the world, my child! The time has come!",
        "This test tube will soon no longer be able to sustain our colony, and so our owner is moving us into a new Formicary. We need you to scout out our new environment and help us relocate.",
        "I will give you tasks to achieve our goal — in this case, <color=#176B23>move into the Formicary</color>! <b>[Press TAB to open the Quest Menu]</b>",
        "But don't forget about your own personal needs, such as hunger and thirst. We are the Harvester Ants, meaning we eat seeds.",
        "To eat a seed, you must first pick it up <b>[Press SPACE to pick up]</b> and then bite into it <b>[Press E to eat]</b>.",
        "<color=#7A1414>Be careful, my child, for you will die quickly if you do not sustain yourself in time.</color>",
    };

    [TextArea(2, 8)] [SerializeField] private string[] task1Lines = new[]
    {
        "Your first task is to <b>Assign a Food Storage</b> in our new home so that other ants know where to bring seeds and where to eat.",
        "<color=#176B23>Carry 4 seeds into a chamber to assign it as a Food Storage.</color> Seeds can be found on the arena — the large open space in the Formicary.",
        "Choose your location carefully. To prevent seeds from rotting, <color=#7A1414>select a dry chamber</color>. You can identify chamber types by their floor: humid chambers are raised with a yellowish hue, while dry chambers are lower and appear more white.",
        "Once done, return to me to receive your next task. Speak to me again if you need this explanation repeated.",
    };

    [TextArea(2, 8)] [SerializeField] private string[] task2Lines = new[]
    {
        "Great job! Your second task is to move our eggs from this tube into our new home and <b>Assign a Nursery</b>.",
        "As you may have guessed, you need to <color=#176B23>carry 4 eggs into a chamber to assign it as a Nursery</color>.",
        "For the eggs to hatch, they require humidity, so please <color=#7A1414>select a humid chamber to assign as a Nursery</color>.",
    };

    [TextArea(2, 8)] [SerializeField] private string[] task3Lines = new[]
    {
        "Great work! Now that you have assigned chambers in our new home, you need to <b>Pave a Trail</b> using your trail pheromones to let other ants know where it is safe to walk. <b>[Hold Z to activate your pheromone mapping mode.]</b>",
        "Using your pheromones, <color=#176B23>draw a trail leading into your assigned chambers and back to the arena</color>. <color=#7A1414>Make sure an ant brings a seed into the Food Storage</color> from the arena to confirm it is working correctly.",
        "Other ants will only walk along the trail you have marked.",
    };

    [TextArea(2, 8)] [SerializeField] private string[] task4Lines = new[]
    {
        "Well done, Scout! You're almost done! Our final step of moving is escorting me to the Royal Chamber.",
        "<color=#176B23>Recruit at least two ants to help carry me to the chamber</color> you want to <b>assign as the Royal Chamber</b>.",
        "<b>[Press R while near an ant to recruit them.]</b> When you recruit an ant, they will start to follow you and help you carry heavy objects by increasing your carrying speed and strength.",
        "The deeper the Royal Chamber is, the better, as it will be safer in the case of an intruder.",
        "I will stay in this chamber most of the time, so please <color=#7A1414>select a humid chamber</color> so I have a supply of water to drink from.",
    };

    private readonly Queue<string> queue = new Queue<string>();
    private string currentLine;
    private bool isOpen;

    private GUIStyle nameStyle, bodyStyle, hintStyle;
    private Texture2D boxTex;

    private void Awake() { Instance = this; }
    private void OnDestroy() { if (Instance == this) Instance = null; }

    // ---------- Public API ----------

    public void PlayIntro()
    {
        Enqueue(introLines);
        Enqueue(task1Lines);
        BeginIfNeeded();
    }

    // Replays the explanation for whatever task is currently active (used when
    // the player walks up to the queen and presses E).
    public void ReplayCurrentTask()
    {
        if (QuestButton.Instance == null || !QuestButton.Instance.HasActiveTask) return;
        PlayForTask(QuestButton.Instance.CurrentTaskIndex);
    }

    public void PlayForTask(int taskIndex)
    {
        string[] lines = taskIndex switch
        {
            0 => task1Lines,
            1 => task2Lines,
            2 => task3Lines,
            3 => task4Lines,
            _ => null,
        };
        if (lines == null) return;
        Enqueue(lines);
        BeginIfNeeded();
    }

    // ---------- Internals ----------

    private void Enqueue(string[] lines)
    {
        if (lines == null) return;
        foreach (var line in lines)
        {
            if (!string.IsNullOrWhiteSpace(line))
                queue.Enqueue(Recolor(line.Trim()));
        }
    }

    // Lines are serialized in the scene, so old bright highlight colors persist
    // even after the defaults change. Swap them to the dark variants at runtime.
    private static string Recolor(string line)
    {
        return line
            .Replace("#5CC75F", "#176B23")   // green  -> dark green
            .Replace("#FF5C5C", "#7A1414");  // red    -> dark red
    }

    private void BeginIfNeeded()
    {
        if (isOpen) return;
        Advance();
    }

    private void Advance()
    {
        if (queue.Count == 0)
        {
            isOpen = false;
            currentLine = null;
            return;
        }
        currentLine = queue.Dequeue();
        isOpen = true;
    }

    private void OnGUI()
    {
        if (!isOpen || string.IsNullOrEmpty(currentLine)) return;
        // Hide the dialogue while the start menu is up (returned to menu mid-line).
        if (!StartMenu.GameStarted) return;
        EnsureStyles();

        GUI.depth = -2000;

        float w = Screen.width - horizontalMargin * 2f;
        float h = boxHeight;
        float x = horizontalMargin;
        float y = Screen.height - h - bottomMargin;
        Rect box = new Rect(x, y, w, h);

        if (backgroundSprite != null)
        {
            Texture tex = backgroundSprite.texture;
            Rect tr = backgroundSprite.textureRect;
            Rect coords = new Rect(
                tr.x / tex.width,
                tr.y / tex.height,
                tr.width / tex.width,
                tr.height / tex.height);
            GUI.DrawTextureWithTexCoords(box, tex, coords);
        }
        else
        {
            GUI.DrawTexture(box, boxTex);
        }

        Rect nameRect;
        if (nameBarSprite != null)
        {
            Rect btr = nameBarSprite.textureRect;
            float barW = nameBarWidth;
            float barH = btr.width > 0f ? barW * (btr.height / btr.width) : nameFontSize + 12f;
            float barX = x + padding + nameBarOffset.x;
            float barY = y - barH * 0.5f + nameBarOffset.y; // straddles the top edge
            Rect barRect = new Rect(barX, barY, barW, barH);
            DrawSprite(barRect, nameBarSprite);
            nameRect = new Rect(barRect.x + nameTextOffset.x, barRect.y + nameTextOffset.y, barRect.width, barRect.height);
            GUI.Label(nameRect, speakerName, nameStyle);
        }
        else
        {
            nameRect = new Rect(x + padding, y + padding * 0.5f, w - padding * 2f, nameFontSize + 6f);
            GUI.Label(nameRect, speakerName, nameStyle);
        }

        float bodyTop = nameRect.yMax + 6f;
        float bodyBottom = y + h - padding - hintFontSize - 4f;
        Rect bodyRect = new Rect(x + padding, bodyTop, w - padding * 2f, bodyBottom - bodyTop);
        GUI.Label(bodyRect, currentLine, bodyStyle);

        string hint = queue.Count > 0 ? "Click to continue ▼" : "Click to close ▼";
        Vector2 hintSize = hintStyle.CalcSize(new GUIContent(hint));
        Rect hintRect = new Rect(x + w - hintSize.x - padding, y + h - hintSize.y - padding * 0.5f, hintSize.x, hintSize.y);
        GUI.Label(hintRect, hint, hintStyle);

        if (!PauseMenu.IsPaused && Event.current.type == EventType.MouseDown && box.Contains(Event.current.mousePosition))
        {
            Advance();
            Event.current.Use();
        }
    }

    // Keep a label's color fixed across all interaction states so text doesn't
    // change color when the mouse hovers over the dialogue box.
    private static void LockTextColor(GUIStyle s)
    {
        Color c = s.normal.textColor;
        s.hover.textColor = c;
        s.active.textColor = c;
        s.focused.textColor = c;
        s.onNormal.textColor = c;
        s.onHover.textColor = c;
        s.onActive.textColor = c;
        s.onFocused.textColor = c;
    }

    private static void DrawSprite(Rect rect, Sprite sprite)
    {
        Texture tex = sprite.texture;
        Rect tr = sprite.textureRect;
        Rect coords = new Rect(
            tr.x / tex.width,
            tr.y / tex.height,
            tr.width / tex.width,
            tr.height / tex.height);
        GUI.DrawTextureWithTexCoords(rect, tex, coords);
    }

    private void EnsureStyles()
    {
        if (boxTex == null)
        {
            boxTex = new Texture2D(1, 1);
            boxTex.SetPixel(0, 0, boxColor);
            boxTex.Apply();
        }
        if (nameStyle == null || nameStyle.fontSize != nameFontSize)
        {
            nameStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = nameFontSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false,
            };
            nameStyle.normal.textColor = nameColor;
            LockTextColor(nameStyle);
        }
        if (bodyStyle == null || bodyStyle.fontSize != bodyFontSize)
        {
            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = bodyFontSize,
                alignment = TextAnchor.UpperLeft,
                wordWrap = true,
                richText = true,
            };
            bodyStyle.normal.textColor = bodyColor;
            LockTextColor(bodyStyle);
        }
        if (hintStyle == null || hintStyle.fontSize != hintFontSize)
        {
            hintStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = hintFontSize,
                fontStyle = FontStyle.Italic,
                alignment = TextAnchor.MiddleRight,
            };
            hintStyle.normal.textColor = new Color(1f, 1f, 1f, 0.7f);
            LockTextColor(hintStyle);
        }

        if (font != null)
        {
            nameStyle.font = font;
            bodyStyle.font = font;
            hintStyle.font = font;
        }
    }
}

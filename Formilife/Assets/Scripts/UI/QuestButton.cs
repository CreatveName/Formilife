using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class QuestButton : MonoBehaviour
{
    [Serializable]
    public class QuestTask
    {
        public string name = "Task";
        [Tooltip("Total amount needed to finish this task (e.g. seeds to carry).")]
        public float target = 1f;
        [Tooltip("If not Unassigned, this task auto-completes when a chamber in the scene has this role.")]
        public ChamberRole requiredChamberRole = ChamberRole.Unassigned;
        [Tooltip("If true, this task auto-completes once the player draws a pheromone path (hold Z, connect two zones).")]
        public bool completeOnPathDrawn = false;
        [HideInInspector] public float current = 0f;

        public float Normalized => target <= 0f ? 1f : Mathf.Clamp01(current / target);
        public bool IsComplete => current >= target;
    }

    [Header("Layout")]
    [SerializeField] private Vector2 margin = new Vector2(16f, 16f);
    [SerializeField] private Vector2 buttonSize = new Vector2(260f, 68f);
    [SerializeField] private float panelWidth = 480f;
    [SerializeField] private float panelPadding = 24f;

    [Header("Background")]
    [Tooltip("Assign the Square_plank_0 sprite (from Art/UI/Square_plank.png) here.")]
    [SerializeField] private Sprite backgroundSprite;
    [Tooltip("Clip drawn on the top edge of the board (Quest Bar.png / Untitled_Artwork_0).")]
    [SerializeField] private Sprite titleBarSprite;
    [Tooltip("Clip width as a fraction of the panel width.")]
    [Range(0.2f, 1f)]
    [SerializeField] private float titleBarWidth = 0.5f;
    [Tooltip("Leaf icon used as the close button (leaf.png / leaf_0).")]
    [SerializeField] private Sprite closeSprite;
    [Tooltip("Height of the leaf close button, in pixels.")]
    [SerializeField] private float closeButtonHeight = 56f;
    [Tooltip("Nudge the leaf: +X moves it right, +Y moves it down.")]
    [SerializeField] private Vector2 closeButtonOffset = Vector2.zero;
    [Tooltip("Font size of the 'Close' text on the leaf.")]
    [SerializeField] private int closeFontSize = 18;
    [Tooltip("Nudge the 'Close' text within the leaf: +X right, +Y down.")]
    [SerializeField] private Vector2 closeTextOffset = Vector2.zero;

    [Header("Text")]
    [SerializeField] private string buttonLabel = "Quests";
    [SerializeField] private string menuTitle = "Quest Chain";
    [SerializeField] private string milestoneName = "Move in";
    [SerializeField] private int buttonFontSize = 28;
    [SerializeField] private int titleFontSize = 32;
    [SerializeField] private int labelFontSize = 22;
    [SerializeField] private int valueFontSize = 24;

    [Header("Bars")]
    [SerializeField] private float barHeight = 26f;
    [SerializeField] private Color overallBarColor = new Color(1f, 0.55f, 0.1f);   // orange
    [SerializeField] private Color taskBarColor = new Color(1f, 0.85f, 0.15f);     // yellow

    [Header("Bar Art (optional — same sprites as the stat bars)")]
    [Tooltip("Shared trough/background behind the fill, e.g. Progress (empty).")]
    [SerializeField] private Sprite barEmptySprite;
    [Tooltip("Shared outline drawn on top, e.g. Progress (Frame).")]
    [SerializeField] private Sprite barFrameSprite;
    [Tooltip("Fill for the orange overall quest bar, e.g. Progress Quest (full).")]
    [SerializeField] private Sprite overallFillSprite;
    [Tooltip("Fill for the yellow task bar, e.g. Progress Task (full).")]
    [SerializeField] private Sprite taskFillSprite;

    [Header("Tasks (in order)")]
    [SerializeField]
    private List<QuestTask> tasks = new List<QuestTask>
    {
        new QuestTask { name = "Assign Food Storage",  target = 1f, requiredChamberRole = ChamberRole.FoodStorage },
        new QuestTask { name = "Assign Nursery",       target = 1f, requiredChamberRole = ChamberRole.Nursery },
        new QuestTask { name = "Pave the Path",        target = 1f, completeOnPathDrawn = true },
        new QuestTask { name = "Assign Royal Chamber", target = 1f, requiredChamberRole = ChamberRole.ThroneRoom },
    };

    // Toggle key is Tab (read via the new Input System in Update).

    [Header("New-Task Glow")]
    [SerializeField] private Color glowColor = new Color(1f, 0.85f, 0.15f);
    [SerializeField] private float glowDuration = 2.5f;
    [SerializeField] private float glowSpeed = 4f;

    [Header("Auto-Complete")]
    [Tooltip("How often (seconds) to scan the scene's chambers for the current task's required role.")]
    [SerializeField] private float chamberCheckInterval = 0.5f;

    private int currentTaskIndex = 0;
    private bool isOpen = false;
    private float glowEndTime = -1f;
    private float nextChamberCheck = 0f;
    private bool pathDrawn = false;

    public static QuestButton Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // Cached styles / textures
    private GUIStyle buttonStyle;
    private GUIStyle titleStyle;
    private GUIStyle labelStyle;
    private GUIStyle valueStyle;
    private GUIStyle closeStyle;
    private GUIStyle closeLabelStyle;
    private Texture2D panelTex;
    private Texture2D barBgTex;
    private Texture2D barFillTex;

    // ---------- Public API (call these from gameplay systems) ----------

    /// <summary>Adds progress to the current task; auto-advances when complete.</summary>
    public void AddTaskProgress(float amount)
    {
        if (!HasCurrentTask) return;
        CurrentTask.current += amount;
        if (CurrentTask.IsComplete) CompleteCurrentTask();
    }

    /// <summary>Sets the current task's progress to an absolute amount.</summary>
    public void SetTaskProgress(float amount)
    {
        if (!HasCurrentTask) return;
        CurrentTask.current = amount;
        if (CurrentTask.IsComplete) CompleteCurrentTask();
    }

    /// <summary>Called by the pheromone system when the player draws a path (Z + connect two zones).</summary>
    public void NotifyPathDrawn()
    {
        pathDrawn = true;
        CheckPathCompletion();
    }

    /// <summary>Marks the current task done and moves to the next one.</summary>
    public void CompleteCurrentTask()
    {
        if (!HasCurrentTask) return;
        CurrentTask.current = CurrentTask.target;
        currentTaskIndex++;
        if (HasCurrentTask) TriggerGlow();
    }

    // ---------- Helpers ----------

    private bool HasCurrentTask => currentTaskIndex >= 0 && currentTaskIndex < tasks.Count;
    private QuestTask CurrentTask => tasks[currentTaskIndex];
    private bool AllComplete => currentTaskIndex >= tasks.Count;

    /// <summary>Overall progress across the whole quest chain (completed tasks + fraction of current).</summary>
    private float OverallNormalized
    {
        get
        {
            if (tasks.Count == 0) return 1f;
            float done = currentTaskIndex + (HasCurrentTask ? CurrentTask.Normalized : 0f);
            return Mathf.Clamp01(done / tasks.Count);
        }
    }

    private void TriggerGlow() => glowEndTime = Time.unscaledTime + glowDuration;
    private bool IsGlowing => Time.unscaledTime < glowEndTime;

    private void Update()
    {
        if (!StartMenu.GameStarted) return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.tabKey.wasPressedThisFrame)
        {
            isOpen = !isOpen;
        }

        if (Time.unscaledTime >= nextChamberCheck)
        {
            nextChamberCheck = Time.unscaledTime + Mathf.Max(0.1f, chamberCheckInterval);
            CheckChamberCompletion();
            CheckPathCompletion();
        }
    }

    // Completes the current task if it's a "draw a path" task and the player has
    // already drawn one (handles drawing the path before this task is active).
    private void CheckPathCompletion()
    {
        if (!HasCurrentTask) return;
        if (pathDrawn && CurrentTask.completeOnPathDrawn)
            CompleteCurrentTask();
    }

    // Auto-completes the current task if a chamber in the scene matches its
    // required role (e.g. a chamber assigned FoodStorage finishes "Assign Food Storage").
    private void CheckChamberCompletion()
    {
        if (!HasCurrentTask) return;

        ChamberRole required = CurrentTask.requiredChamberRole;
        if (required == ChamberRole.Unassigned) return;

        Chamber[] chambers = FindObjectsByType<Chamber>(FindObjectsInactive.Exclude);
        foreach (Chamber c in chambers)
        {
            if (c.current == required)
            {
                CompleteCurrentTask();
                return;
            }
        }
    }

    private void OnGUI()
    {
        if (!StartMenu.GameStarted) return;
        EnsureStyles();

        if (isOpen) DrawMenu();
        else DrawButton();
    }

    // The collapsed icon/button, anchored top-right.
    private void DrawButton()
    {
        float bx = Screen.width - buttonSize.x - margin.x;
        float by = margin.y;
        Rect r = new Rect(bx, by, buttonSize.x, buttonSize.y);

        Color prevBg = GUI.backgroundColor;
        if (IsGlowing)
        {
            float t = (Mathf.Sin(Time.unscaledTime * glowSpeed) + 1f) * 0.5f;
            GUI.backgroundColor = Color.Lerp(prevBg, glowColor, t);
        }

        if (GUI.Button(r, buttonLabel, buttonStyle)) isOpen = true;

        GUI.backgroundColor = prevBg;
    }

    // The expanded pop-up menu, occupying the icon's top-right slot.
    private void DrawMenu()
    {
        float sectionGap = 18f;
        float w = panelWidth - panelPadding * 2f;

        // The title clip straddles the board's top edge (clipboard style); only
        // its lower half overlaps the board, so reserve that much header space.
        float clipW = 0f, clipH = 0f;
        if (titleBarSprite != null)
        {
            Rect btr = titleBarSprite.textureRect;
            clipW = w * titleBarWidth;
            clipH = btr.width > 0f ? clipW * (btr.height / btr.width) : titleFontSize + 6f;
        }
        // Space at the top of the board taken up by the dipped-in part of the clip.
        float headerSpace = titleBarSprite != null
            ? Mathf.Max(panelPadding, clipH * 0.5f + 8f)
            : panelPadding + titleFontSize + 6f + sectionGap;

        // Bar heights honor the art's native aspect ratio (shared trough/frame,
        // so both bars match); falls back to barHeight when no art is assigned.
        float overallBarH = BarHeight(w, overallFillSprite);
        float taskBarH = BarHeight(w, taskFillSprite);

        // Compute height top-down.
        float height = headerSpace;                        // top pad / clip overlap
        height += labelFontSize + 4f;                      // "Quest:" line
        height += overallBarH + sectionGap;                // overall bar
        height += labelFontSize + 4f;                      // "Task:" line
        height += taskBarH + sectionGap;                   // task bar
        height += panelPadding;                            // bottom pad
        // (the leaf close button sits on top of the bottom edge, not inside)

        float px = Screen.width - panelWidth - margin.x;
        float py = margin.y;
        Rect panel = new Rect(px, py, panelWidth, height);
        if (backgroundSprite != null)
            DrawSprite(panel, backgroundSprite);
        else
            GUI.DrawTexture(panel, panelTex);

        float x = px + panelPadding;
        float y = py + headerSpace;

        // Title clip straddling the board's top edge, with the title on it.
        if (titleBarSprite != null)
        {
            float clipX = px + (panelWidth - clipW) * 0.5f;
            float clipY = py - clipH * 0.5f;   // half above the edge, half on the board
            Rect clipRect = new Rect(clipX, clipY, clipW, clipH);
            DrawSprite(clipRect, titleBarSprite);
            GUI.Label(clipRect, menuTitle, titleStyle);
        }
        else
        {
            GUI.Label(new Rect(x, py + panelPadding, w, titleFontSize + 6f), menuTitle, titleStyle);
        }

        // Overall quest
        GUI.Label(new Rect(x, y, w, labelFontSize + 4f), "Quest: " + milestoneName, labelStyle);
        y += labelFontSize + 4f;
        DrawProgressBar(new Rect(x, y, w, overallBarH), OverallNormalized, overallFillSprite, overallBarColor);
        y += overallBarH + sectionGap;

        // Current task
        string taskName = AllComplete ? "All tasks complete!" : CurrentTask.name;
        GUI.Label(new Rect(x, y, w, labelFontSize + 4f), "Task: " + taskName, labelStyle);
        y += labelFontSize + 4f;
        float taskFill = AllComplete ? 1f : CurrentTask.Normalized;
        DrawProgressBar(new Rect(x, y, w, taskBarH), taskFill, taskFillSprite, taskBarColor);
        y += taskBarH + sectionGap;

        // Close: leaf icon resting on top of the board's bottom-right edge.
        // Width is derived from the sprite so the aspect ratio is preserved.
        float closeH = closeButtonHeight;
        float closeW = closeH;
        if (closeSprite != null)
        {
            Rect ctr = closeSprite.textureRect;
            if (ctr.height > 0f) closeW = closeH * (ctr.width / ctr.height);
        }
        Rect closeRect = new Rect(
            px + panelWidth - panelPadding - closeW + closeButtonOffset.x,
            py + height - closeH * 0.5f + closeButtonOffset.y,   // straddles the bottom edge
            closeW, closeH);

        if (closeSprite != null)
        {
            DrawSprite(closeRect, closeSprite);
            Rect textRect = new Rect(
                closeRect.x + closeTextOffset.x,
                closeRect.y + closeTextOffset.y,
                closeRect.width, closeRect.height);
            GUI.Label(textRect, "Close", closeLabelStyle);
            if (GUI.Button(closeRect, GUIContent.none, GUIStyle.none)) isOpen = false;
        }
        else if (GUI.Button(closeRect, "Close", closeStyle))
        {
            isOpen = false;
        }
    }

    // Draws a single sprite (handles sliced sprite-sheet sub-rects) into rect.
    private void DrawSprite(Rect rect, Sprite sprite)
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

    // Bar height that preserves the art's native width:height ratio at the given
    // width. Prefers the shared trough/frame so both bars match; uses the fill
    // sprite otherwise, and falls back to the fixed barHeight with no art.
    private float BarHeight(float width, Sprite fill)
    {
        Sprite s = barEmptySprite != null ? barEmptySprite
                 : barFrameSprite != null ? barFrameSprite
                 : fill;
        if (s == null) return barHeight;

        Rect tr = s.textureRect;
        if (tr.width <= 0f) return barHeight;
        return width * (tr.height / tr.width);
    }

    // Uses the sprite art (trough + clipped fill + frame) when a fill sprite is
    // assigned; otherwise falls back to the solid-color bar.
    private void DrawProgressBar(Rect rect, float normalized, Sprite fillSprite, Color fallbackColor)
    {
        if (fillSprite == null)
        {
            DrawBar(rect, normalized, fallbackColor);
            return;
        }

        if (barEmptySprite != null) DrawSprite(rect, barEmptySprite);
        DrawSpriteFilled(rect, fillSprite, normalized);
        if (barFrameSprite != null) DrawSprite(rect, barFrameSprite);
    }

    // Draws the left `normalized` fraction of a sprite, clipping both the
    // destination rect and the texture coords so the fill grows horizontally.
    private void DrawSpriteFilled(Rect rect, Sprite sprite, float normalized)
    {
        normalized = Mathf.Clamp01(normalized);
        if (normalized <= 0f) return;

        Texture tex = sprite.texture;
        Rect tr = sprite.textureRect;
        Rect dst = new Rect(rect.x, rect.y, rect.width * normalized, rect.height);
        Rect coords = new Rect(
            tr.x / tex.width,
            tr.y / tex.height,
            (tr.width / tex.width) * normalized,
            tr.height / tex.height);
        GUI.DrawTextureWithTexCoords(dst, tex, coords);
    }

    private void DrawBar(Rect rect, float normalized, Color fill)
    {
        normalized = Mathf.Clamp01(normalized);
        GUI.DrawTexture(rect, barBgTex);

        Rect fillRect = new Rect(rect.x, rect.y, rect.width * normalized, rect.height);
        Color prev = GUI.color;
        GUI.color = fill;
        GUI.DrawTexture(fillRect, barFillTex);
        GUI.color = prev;
    }

    private void EnsureStyles()
    {
        if (buttonStyle == null || buttonStyle.fontSize != buttonFontSize)
        {
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = buttonFontSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
        }
        if (titleStyle == null || titleStyle.fontSize != titleFontSize)
        {
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = titleFontSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            titleStyle.normal.textColor = Color.white;
        }
        if (labelStyle == null || labelStyle.fontSize != labelFontSize)
        {
            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = labelFontSize,
                fontStyle = FontStyle.Bold
            };
            labelStyle.normal.textColor = Color.white;
        }
        if (valueStyle == null || valueStyle.fontSize != valueFontSize)
        {
            valueStyle = new GUIStyle(GUI.skin.label) { fontSize = valueFontSize };
            valueStyle.normal.textColor = Color.white;
        }
        if (closeStyle == null || closeStyle.fontSize != labelFontSize)
        {
            closeStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = labelFontSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
        }
        if (closeLabelStyle == null || closeLabelStyle.fontSize != closeFontSize)
        {
            closeLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = closeFontSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            closeLabelStyle.normal.textColor = Color.white;
        }
        if (panelTex == null) panelTex = MakeTex(new Color(0.08f, 0.08f, 0.1f, 0.95f));
        if (barBgTex == null) barBgTex = MakeTex(new Color(0f, 0f, 0f, 0.6f));
        if (barFillTex == null) barFillTex = MakeTex(Color.white);
    }

    private static Texture2D MakeTex(Color c)
    {
        Texture2D t = new Texture2D(1, 1);
        t.SetPixel(0, 0, c);
        t.Apply();
        return t;
    }

    // ---------- Editor testing ----------
    [ContextMenu("Test: Add 25% to current task")]
    private void TestAddProgress() => AddTaskProgress((HasCurrentTask ? CurrentTask.target : 0f) * 0.25f);

    [ContextMenu("Test: Complete current task")]
    private void TestCompleteTask() => CompleteCurrentTask();
}

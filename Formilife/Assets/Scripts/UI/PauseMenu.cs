using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// In-game pause overlay. Toggled with Esc once the game has started. Freezes
// gameplay with Time.timeScale = 0 (movement lives in FixedUpdate and the camera
// uses Time.deltaTime, so both stop on their own); discrete key actions are gated
// elsewhere via PauseMenu.IsPaused. Mirrors StartMenu's plank/board styling.
public class PauseMenu : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    [Header("Title")]
    [SerializeField] private string titleText = "Paused";
    [SerializeField] private int titleFontSize = 56;

    [Header("Font")]
    [Tooltip("If set, all menu text uses this font (Itim-Regular).")]
    [SerializeField] private Font font;

    [Header("Buttons")]
    [SerializeField] private int buttonFontSize = 24;
    [SerializeField] private Vector2 buttonSize = new Vector2(260f, 56f);
    [SerializeField] private float buttonSpacing = 14f;
    [Tooltip("Sprite used as the button background (Plank Medium.png).")]
    [SerializeField] private Sprite buttonBackground;

    [Header("Panel")]
    [Tooltip("Sprite used as the sub-panel background (Medium Board.png). If unset, a flat dark box is drawn.")]
    [SerializeField] private Sprite panelBackground;
    [Tooltip("Clip drawn straddling the top edge of the panel, behind the title (Quest Bar.png).")]
    [SerializeField] private Sprite titleBarSprite;
    [Tooltip("Title bar width as a fraction of the panel width.")]
    [Range(0.2f, 1f)]
    [SerializeField] private float titleBarWidth = 0.6f;
    [Tooltip("Leaf icon used as the Back button (leaf.png). If unset, a plank button is drawn.")]
    [SerializeField] private Sprite backSprite;
    [Tooltip("Height of the leaf Back button, in pixels.")]
    [SerializeField] private float backButtonHeight = 56f;
    [Tooltip("Nudge the leaf Back button: +X right, +Y down.")]
    [SerializeField] private Vector2 backButtonOffset = Vector2.zero;
    [Tooltip("Font size of the 'Back' text on the leaf.")]
    [SerializeField] private int backFontSize = 18;
    [Tooltip("Nudge the 'Back' text within the leaf: +X right, +Y down.")]
    [SerializeField] private Vector2 backTextOffset = Vector2.zero;

    [Header("Credits")]
    [Tooltip("Text shown on the Credits panel.")]
    [TextArea(3, 12)]
    [SerializeField] private string creditsText = "";

    private enum Panel { Main, Options, Controls, Credits }
    private Panel currentPanel = Panel.Main;

    private GUIStyle titleStyle;
    private GUIStyle headerStyle;
    private GUIStyle buttonStyle;
    private GUIStyle plankLabelStyle;
    private GUIStyle bodyStyle;
    private GUIStyle creditsBodyStyle;
    private GUIStyle backLabelStyle;
    private Texture2D dimTex;
    private Texture2D panelTex;

    private void Update()
    {
        // Only allow pausing once the start menu has handed off to gameplay.
        if (!StartMenu.GameStarted) return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            if (IsPaused && currentPanel != Panel.Main)
                currentPanel = Panel.Main;   // back out of a sub-panel first
            else if (IsPaused)
                Resume();
            else
                Pause();
        }
    }

    private void Pause()
    {
        IsPaused = true;
        currentPanel = Panel.Main;
        Time.timeScale = 0f;
    }

    private void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;
    }

    private void RestartGame()
    {
        // Full reset; StartMenu reloads the scene and drops back into a fresh game.
        IsPaused = false;
        StartMenu.RestartGame();
    }

    private void QuitToMainMenu()
    {
        // Return to the menu without resetting; the game stays loaded (and frozen
        // via StartMenu.GameStarted) so the player can Resume from the menu.
        IsPaused = false;
        Time.timeScale = 1f;
        currentPanel = Panel.Main;
        if (StartMenu.Instance != null)
            StartMenu.Instance.ReturnToMenu();
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);   // fallback
    }

    private void OnDisable()
    {
        // Don't leave the game frozen if this object is disabled while paused.
        if (IsPaused)
        {
            IsPaused = false;
            Time.timeScale = 1f;
        }
    }

    private void OnGUI()
    {
        if (!IsPaused) return;
        EnsureStyles();

        // Lower depth draws on top in IMGUI; sit below QueenDialogue (-2000) so the
        // pause menu and its dim overlay cover the dialogue box too.
        GUI.depth = -3000;

        // Dim the whole screen behind the menu.
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), dimTex);

        switch (currentPanel)
        {
            case Panel.Main: DrawMain(); break;
            case Panel.Options: DrawSubPanel("Options", "Coming in a future update!"); break;
            case Panel.Controls: DrawSubPanel("Controls", ControlsBody()); break;
            case Panel.Credits: DrawSubPanel("Credits", ResolveCreditsText(), creditsBodyStyle); break;
        }
    }

    private void DrawMain()
    {
        float cx = Screen.width * 0.5f;
        float titleY = Screen.height * 0.22f;

        Vector2 titleSize = titleStyle.CalcSize(new GUIContent(titleText));
        GUI.Label(new Rect(cx - titleSize.x * 0.5f, titleY, titleSize.x, titleSize.y), titleText, titleStyle);

        float bx = cx - buttonSize.x * 0.5f;
        float by = titleY + titleSize.y + 50f;

        if (PlankButton(new Rect(bx, by, buttonSize.x, buttonSize.y), "Resume")) Resume();
        by += buttonSize.y + buttonSpacing;
        if (PlankButton(new Rect(bx, by, buttonSize.x, buttonSize.y), "Options")) currentPanel = Panel.Options;
        by += buttonSize.y + buttonSpacing;
        if (PlankButton(new Rect(bx, by, buttonSize.x, buttonSize.y), "Controls")) currentPanel = Panel.Controls;
        by += buttonSize.y + buttonSpacing;
        if (PlankButton(new Rect(bx, by, buttonSize.x, buttonSize.y), "Credits")) currentPanel = Panel.Credits;
        by += buttonSize.y + buttonSpacing;
        if (PlankButton(new Rect(bx, by, buttonSize.x, buttonSize.y), "Restart Game")) RestartGame();
        by += buttonSize.y + buttonSpacing;
        if (PlankButton(new Rect(bx, by, buttonSize.x, buttonSize.y), "Quit to Menu")) QuitToMainMenu();
    }

    // Use this menu's own credits if set; otherwise fall back to StartMenu's so
    // the text only has to be maintained in one place.
    private StartMenu cachedStartMenu;
    private string ResolveCreditsText()
    {
        if (!string.IsNullOrWhiteSpace(creditsText)) return creditsText;
        if (cachedStartMenu == null) cachedStartMenu = FindFirstObjectByType<StartMenu>(FindObjectsInactive.Include);
        return cachedStartMenu != null ? cachedStartMenu.CreditsText : creditsText;
    }

    private static string ControlsBody()
    {
        return
            "Movement\n" +
            "    W / Up — Move forward\n" +
            "    S / Down — Move backward\n" +
            "    A / Left — Turn left\n" +
            "    D / Right — Turn right\n" +
            "    Shift or Ctrl — Run\n" +
            "\n" +
            "Items\n" +
            "    Space — Pick up / Drop\n" +
            "    E — Eat held food / Drink nearby\n" +
            "\n" +
            "Colony\n" +
            "    R — Recruit nearby ant\n" +
            "    Q — Dismiss all recruits\n" +
            "\n" +
            "Camera\n" +
            "    Mouse Wheel — Zoom\n" +
            "    - / = — Zoom in / out\n" +
            "    9 / 0 — Min / Max zoom\n" +
            "    C — Toggle 2D / 3D view";
    }

    private void DrawSubPanel(string header, string body, GUIStyle bodyStyleOverride = null)
    {
        GUIStyle bs = bodyStyleOverride != null ? bodyStyleOverride : bodyStyle;
        const float pad = 24f;
        const float gap = 18f;

        float w = Mathf.Min(560f, Screen.width - 80f);
        float bodyW = w - 2f * pad;

        Vector2 headerSize = headerStyle.CalcSize(new GUIContent(header));
        float bodyH = bs.CalcHeight(new GUIContent(body), bodyW);

        // Title clip straddles the top edge (clipboard style); only its lower half
        // overlaps the board, so reserve that much header space.
        float clipW = 0f, clipH = 0f;
        if (titleBarSprite != null)
        {
            Rect btr = titleBarSprite.textureRect;
            clipW = w * titleBarWidth;
            clipH = btr.width > 0f ? clipW * (btr.height / btr.width) : headerSize.y;
        }
        float headerSpace = titleBarSprite != null
            ? Mathf.Max(pad, clipH * 0.5f + 8f)
            : pad + headerSize.y;

        // The leaf Back button rests on the bottom edge (half on, half off), so it
        // only needs half its height of reserved space; the plank fallback sits inside.
        float backH = backSprite != null ? backButtonHeight : 48f;
        float bottomSpace = backSprite != null ? backH * 0.5f + pad : backH + pad;

        float h = headerSpace + gap + bodyH + gap + bottomSpace;
        h = Mathf.Min(h, Screen.height - 80f);

        float x = (Screen.width - w) * 0.5f;
        float y = (Screen.height - h) * 0.5f;

        Rect panelRect = new Rect(x, y, w, h);
        if (panelBackground != null) DrawSprite(panelRect, panelBackground);
        else GUI.DrawTexture(panelRect, panelTex);

        // Title — on the straddling clip if assigned, otherwise plain at the top.
        if (titleBarSprite != null)
        {
            float clipX = x + (w - clipW) * 0.5f;
            float clipY = y - clipH * 0.5f;   // half above the edge, half on the board
            Rect clipRect = new Rect(clipX, clipY, clipW, clipH);
            DrawSprite(clipRect, titleBarSprite);
            GUI.Label(clipRect, header, headerStyle);
        }
        else
        {
            GUI.Label(new Rect(x + (w - headerSize.x) * 0.5f, y + pad, headerSize.x, headerSize.y), header, headerStyle);
        }

        Rect bodyRect = new Rect(x + pad, y + headerSpace + gap, bodyW, bodyH);
        GUI.Label(bodyRect, body, bs);

        // Back — leaf resting on the bottom edge (like the quest Close button),
        // otherwise the plank fallback inside the panel.
        if (backSprite != null)
        {
            float backW = backH;
            Rect ctr = backSprite.textureRect;
            if (ctr.height > 0f) backW = backH * (ctr.width / ctr.height);
            Rect backRect = new Rect(
                x + (w - backW) * 0.5f + backButtonOffset.x,
                y + h - backH * 0.5f + backButtonOffset.y,
                backW, backH);
            DrawSprite(backRect, backSprite);
            Rect textRect = new Rect(backRect.x + backTextOffset.x, backRect.y + backTextOffset.y, backRect.width, backRect.height);
            GUI.Label(textRect, "Back", backLabelStyle);
            if (GUI.Button(backRect, GUIContent.none, GUIStyle.none)) currentPanel = Panel.Main;
        }
        else
        {
            float backW = 180f;
            Rect backRect = new Rect(x + (w - backW) * 0.5f, y + h - backH - pad, backW, backH);
            if (PlankButton(backRect, "Back")) currentPanel = Panel.Main;
        }
    }

    // Draws a sprite (handles sliced sprite-sheet sub-rects) into rect.
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

    private bool PlankButton(Rect rect, string label)
    {
        if (buttonBackground == null)
            return GUI.Button(rect, label, buttonStyle);

        Texture tex = buttonBackground.texture;
        Rect tr = buttonBackground.textureRect;
        Rect coords = new Rect(
            tr.x / tex.width,
            tr.y / tex.height,
            tr.width / tex.width,
            tr.height / tex.height);
        GUI.DrawTextureWithTexCoords(rect, tex, coords);

        GUI.Label(rect, label, plankLabelStyle);
        return GUI.Button(rect, GUIContent.none, GUIStyle.none);
    }

    private void EnsureStyles()
    {
        if (dimTex == null) dimTex = MakeTex(new Color(0f, 0f, 0f, 0.55f));
        if (panelTex == null) panelTex = MakeTex(new Color(0.08f, 0.08f, 0.1f, 0.92f));

        if (titleStyle == null || titleStyle.fontSize != titleFontSize)
        {
            titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = titleFontSize;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.normal.textColor = Color.white;
            LockTextColor(titleStyle);
        }
        if (buttonStyle == null || buttonStyle.fontSize != buttonFontSize)
        {
            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = buttonFontSize;
            buttonStyle.fontStyle = FontStyle.Bold;
            buttonStyle.alignment = TextAnchor.MiddleCenter;
        }
        if (plankLabelStyle == null || plankLabelStyle.fontSize != buttonFontSize)
        {
            plankLabelStyle = new GUIStyle(GUI.skin.label);
            plankLabelStyle.fontSize = buttonFontSize;
            plankLabelStyle.fontStyle = FontStyle.Bold;
            plankLabelStyle.alignment = TextAnchor.MiddleCenter;
            plankLabelStyle.normal.textColor = Color.white;
            LockTextColor(plankLabelStyle);
        }
        if (headerStyle == null || headerStyle.fontSize != 34)
        {
            headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontSize = 34;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.alignment = TextAnchor.MiddleCenter;
            headerStyle.normal.textColor = Color.white;
            LockTextColor(headerStyle);
        }
        if (bodyStyle == null)
        {
            bodyStyle = new GUIStyle(GUI.skin.label);
            bodyStyle.fontSize = 26;
            bodyStyle.alignment = TextAnchor.UpperLeft;
            bodyStyle.normal.textColor = Color.black;
            bodyStyle.wordWrap = true;
            bodyStyle.richText = true;
            LockTextColor(bodyStyle);
        }
        if (creditsBodyStyle == null)
        {
            creditsBodyStyle = new GUIStyle(bodyStyle);
            creditsBodyStyle.alignment = TextAnchor.UpperCenter;
            creditsBodyStyle.normal.textColor = Color.black;
            LockTextColor(creditsBodyStyle);
        }
        if (backLabelStyle == null || backLabelStyle.fontSize != backFontSize)
        {
            backLabelStyle = new GUIStyle(GUI.skin.label);
            backLabelStyle.fontSize = backFontSize;
            backLabelStyle.fontStyle = FontStyle.Bold;
            backLabelStyle.alignment = TextAnchor.MiddleCenter;
            backLabelStyle.normal.textColor = Color.white;
            LockTextColor(backLabelStyle);
        }

        if (font != null)
        {
            titleStyle.font = font;
            buttonStyle.font = font;
            plankLabelStyle.font = font;
            headerStyle.font = font;
            bodyStyle.font = font;
            creditsBodyStyle.font = font;
            backLabelStyle.font = font;
        }
    }

    // Keep a label's color fixed across all interaction states so non-clickable
    // text doesn't change color when the mouse hovers over it.
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

    private static Texture2D MakeTex(Color c)
    {
        Texture2D t = new Texture2D(1, 1);
        t.SetPixel(0, 0, c);
        t.Apply();
        return t;
    }
}

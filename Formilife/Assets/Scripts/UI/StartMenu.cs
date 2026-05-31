using UnityEngine;

public class StartMenu : MonoBehaviour
{
    public static bool GameStarted { get; private set; } = true;


    [Header("Refs")]
    [SerializeField] private TopDownCamera topDownCamera;
    [SerializeField] private Camera gameCamera;
    [SerializeField] private Transform menuFocusTarget;

    [Header("Menu Camera")]
    [SerializeField] private float menuOrthoSize = 40f;
    [SerializeField] private Vector2 menuFocusOffset = Vector2.zero;

    [Header("Gameplay Camera")]
    [SerializeField] private float gameStartZoom = 8f;

    [Header("Title")]
    [Tooltip("If set, this image is shown instead of the title text (FormilifeTitle.png).")]
    [SerializeField] private Texture2D titleImage;
    [Tooltip("Width of the title image in pixels; height keeps the image's aspect ratio.")]
    [SerializeField] private float titleImageWidth = 380f;
    [SerializeField] private string titleText = "Formilife";
    [SerializeField] private int titleFontSize = 72;

    [Header("Font")]
    [Tooltip("If set, all menu text uses this font (Itim-Regular).")]
    [SerializeField] private Font font;

    [Header("Buttons")]
    [SerializeField] private int buttonFontSize = 24;
    [SerializeField] private Vector2 buttonSize = new Vector2(260f, 56f);
    [SerializeField] private float buttonSpacing = 14f;
    [Tooltip("Sprite used as the button background (Plank Medium.png).")]
    [SerializeField] private Sprite buttonBackground;

    private enum Panel { Main, Options, Controls, Credits }
    private Panel currentPanel = Panel.Main;
    private bool isOpen = true;

    private GUIStyle titleStyle;
    private GUIStyle headerStyle;
    private GUIStyle buttonStyle;
    private GUIStyle plankLabelStyle;
    private GUIStyle bodyStyle;
    private GUIStyle panelStyle;
    private Texture2D dimTex;
    private Texture2D panelTex;

    private void Awake()
    {
        if (gameCamera == null) gameCamera = Camera.main;
        if (topDownCamera == null && gameCamera != null) topDownCamera = gameCamera.GetComponent<TopDownCamera>();
    }

    private void Start()
    {
        OpenMenu();
    }

    private void OpenMenu()
    {
        isOpen = true;
        currentPanel = Panel.Main;
        GameStarted = false;

        if (topDownCamera != null) topDownCamera.enabled = false;

        if (gameCamera != null)
        {
            if (gameCamera.orthographic) gameCamera.orthographicSize = menuOrthoSize;
            if (menuFocusTarget != null)
            {
                Vector3 t = menuFocusTarget.position;
                gameCamera.transform.position = new Vector3(t.x + menuFocusOffset.x, t.y + menuFocusOffset.y, gameCamera.transform.position.z);
            }
        }
    }

    private void StartGame()
    {
        isOpen = false;
        GameStarted = true;
        if (topDownCamera != null)
        {
            topDownCamera.SetFov(gameStartZoom, allowBeyondMax: true);
            topDownCamera.setCameraType(false);
            topDownCamera.enabled = true;
        }
        if (QueenDialogue.Instance != null) QueenDialogue.Instance.PlayIntro();
    }

    private void OnGUI()
    {
        if (!isOpen) return;
        EnsureStyles();

        GUI.depth = -1000;

        switch (currentPanel)
        {
            case Panel.Main: DrawMain(); break;
            case Panel.Options: DrawOptions(); break;
            case Panel.Controls: DrawControls(); break;
            case Panel.Credits: DrawCredits(); break;
        }
    }

    private void DrawMain()
    {
        float cx = Screen.width * 0.25f;
        float titleY = Screen.height * 0.18f;

        float titleH;
        if (titleImage != null)
        {
            float titleW = titleImageWidth;
            titleH = titleImage.width > 0
                ? titleW * ((float)titleImage.height / titleImage.width)
                : titleImageWidth;
            GUI.DrawTexture(new Rect(cx - titleW * 0.5f, titleY, titleW, titleH), titleImage, ScaleMode.ScaleToFit);
        }
        else
        {
            Vector2 titleSize = titleStyle.CalcSize(new GUIContent(titleText));
            GUI.Label(new Rect(cx - titleSize.x * 0.5f, titleY, titleSize.x, titleSize.y), titleText, titleStyle);
            titleH = titleSize.y;
        }

        float bx = cx - buttonSize.x * 0.5f;
        float by = titleY + titleH + 60f;

        if (PlankButton(new Rect(bx, by, buttonSize.x, buttonSize.y), "Start")) StartGame();
        by += buttonSize.y + buttonSpacing;
        if (PlankButton(new Rect(bx, by, buttonSize.x, buttonSize.y), "Options")) currentPanel = Panel.Options;
        by += buttonSize.y + buttonSpacing;
        if (PlankButton(new Rect(bx, by, buttonSize.x, buttonSize.y), "Controls")) currentPanel = Panel.Controls;
        by += buttonSize.y + buttonSpacing;
        if (PlankButton(new Rect(bx, by, buttonSize.x, buttonSize.y), "Credits")) currentPanel = Panel.Credits;
        by += buttonSize.y + buttonSpacing;
        if (PlankButton(new Rect(bx, by, buttonSize.x, buttonSize.y), "Quit")) QuitGame();
    }

    private void DrawOptions()
    {
        DrawSubPanel("Options", "Coming in a future update!");
    }

    private void DrawControls()
    {
        DrawSubPanel("Controls", ControlsBody());
    }

    private void DrawCredits()
    {
        DrawSubPanel("Credits", CreditsBody());
    }

    private static string CreditsBody()
    {
        return "";
    }

    // Single source of truth for the control list so the Options and
    // Controls panels can't drift out of sync with each other or the code.
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

    private void DrawSubPanel(string header, string body)
    {
        const float pad = 24f;
        const float gap = 18f;
        const float backH = 48f;

        float halfW = Screen.width * 0.5f;
        float w = Mathf.Min(560f, halfW - 80f);
        float bodyW = w - 2f * pad;

        Vector2 headerSize = headerStyle.CalcSize(new GUIContent(header));
        float bodyH = bodyStyle.CalcHeight(new GUIContent(body), bodyW);

        // Size the panel to its contents (clamped to the screen) so longer
        // lists don't clip.
        float h = pad + headerSize.y + gap + bodyH + gap + backH + pad;
        h = Mathf.Min(h, Screen.height - 80f);

        float x = (halfW - w) * 0.5f;
        float y = (Screen.height - h) * 0.5f;

        GUI.DrawTexture(new Rect(x, y, w, h), panelTex);

        GUI.Label(new Rect(x + (w - headerSize.x) * 0.5f, y + pad, headerSize.x, headerSize.y), header, headerStyle);

        Rect bodyRect = new Rect(x + pad, y + pad + headerSize.y + gap, bodyW, bodyH);
        GUI.Label(bodyRect, body, bodyStyle);

        float backW = 180f;
        Rect backRect = new Rect(x + (w - backW) * 0.5f, y + h - backH - pad, backW, backH);
        if (PlankButton(backRect, "Back")) currentPanel = Panel.Main;
    }

    // Button with the Plank Medium sprite as a background and a transparent
    // GUI.Button on top so the label still renders and clicks still register.
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

        // Label on top of the plank (label-style, no background), then an invisible
        // button on the same rect for clicks.
        GUI.Label(rect, label, plankLabelStyle);
        return GUI.Button(rect, GUIContent.none, GUIStyle.none);
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
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
        }
        if (headerStyle == null || headerStyle.fontSize != 34)
        {
            headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontSize = 34;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.alignment = TextAnchor.MiddleCenter;
            headerStyle.normal.textColor = Color.white;
        }
        if (bodyStyle == null)
        {
            bodyStyle = new GUIStyle(GUI.skin.label);
            bodyStyle.fontSize = 18;
            bodyStyle.alignment = TextAnchor.UpperLeft;
            bodyStyle.normal.textColor = Color.white;
            bodyStyle.wordWrap = true;
        }
        if (panelStyle == null)
        {
            panelStyle = new GUIStyle(GUI.skin.box);
        }

        if (font != null)
        {
            titleStyle.font = font;
            buttonStyle.font = font;
            plankLabelStyle.font = font;
            headerStyle.font = font;
            bodyStyle.font = font;
        }
    }

    private static Texture2D MakeTex(Color c)
    {
        Texture2D t = new Texture2D(1, 1);
        t.SetPixel(0, 0, c);
        t.Apply();
        return t;
    }
}

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

    [Header("Buttons")]
    [SerializeField] private int buttonFontSize = 24;
    [SerializeField] private Vector2 buttonSize = new Vector2(260f, 56f);
    [SerializeField] private float buttonSpacing = 14f;

    private enum Panel { Main, Options, Controls }
    private Panel currentPanel = Panel.Main;
    private bool isOpen = true;

    private GUIStyle titleStyle;
    private GUIStyle buttonStyle;
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

        if (GUI.Button(new Rect(bx, by, buttonSize.x, buttonSize.y), "Start", buttonStyle)) StartGame();
        by += buttonSize.y + buttonSpacing;
        if (GUI.Button(new Rect(bx, by, buttonSize.x, buttonSize.y), "Options", buttonStyle)) currentPanel = Panel.Options;
        by += buttonSize.y + buttonSpacing;
        if (GUI.Button(new Rect(bx, by, buttonSize.x, buttonSize.y), "Controls", buttonStyle)) currentPanel = Panel.Controls;
        by += buttonSize.y + buttonSpacing;
        if (GUI.Button(new Rect(bx, by, buttonSize.x, buttonSize.y), "Quit", buttonStyle)) QuitGame();
    }

    private void DrawOptions()
    {
        DrawSubPanel("Options", "Options will go here.");
    }

    private void DrawControls()
    {
        string body =
            "WASD / Arrow Keys — Move\n" +
            "Mouse Wheel — Zoom\n" +
            "Space — Pick up";
        DrawSubPanel("Controls", body);
    }

    private void DrawSubPanel(string header, string body)
    {
        float halfW = Screen.width * 0.5f;
        float w = Mathf.Min(560f, halfW - 80f);
        float h = Mathf.Min(420f, Screen.height - 160f);
        float x = (halfW - w) * 0.5f;
        float y = (Screen.height - h) * 0.5f;

        GUI.DrawTexture(new Rect(x, y, w, h), panelTex);

        Vector2 headerSize = titleStyle.CalcSize(new GUIContent(header));
        GUI.Label(new Rect(x + (w - headerSize.x) * 0.5f, y + 20f, headerSize.x, headerSize.y), header, titleStyle);

        Rect bodyRect = new Rect(x + 30f, y + 30f + headerSize.y, w - 60f, h - 130f - headerSize.y);
        GUI.Label(bodyRect, body, bodyStyle);

        float backW = 180f;
        float backH = 48f;
        Rect backRect = new Rect(x + (w - backW) * 0.5f, y + h - backH - 24f, backW, backH);
        if (GUI.Button(backRect, "Back", buttonStyle)) currentPanel = Panel.Main;
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
    }

    private static Texture2D MakeTex(Color c)
    {
        Texture2D t = new Texture2D(1, 1);
        t.SetPixel(0, 0, c);
        t.Apply();
        return t;
    }
}

using UnityEngine;

public class QuestButton : MonoBehaviour
{
    [SerializeField] private Vector2 position = new Vector2(12f, 90f);
    [SerializeField] private Vector2 size = new Vector2(140f, 36f);
    [SerializeField] private int fontSize = 16;
    [SerializeField] private string label = "Quests";

    private GUIStyle buttonStyle;

    private void OnGUI()
    {
        if (!StartMenu.GameStarted) return;
        EnsureStyles();

        Rect r = new Rect(position.x, position.y, size.x, size.y);
        if (GUI.Button(r, label, buttonStyle))
        {
            Debug.Log("[QuestButton] Quest button clicked");
        }
    }

    private void EnsureStyles()
    {
        if (buttonStyle == null || buttonStyle.fontSize != fontSize)
        {
            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = fontSize;
            buttonStyle.fontStyle = FontStyle.Bold;
            buttonStyle.alignment = TextAnchor.MiddleCenter;
        }
    }
}

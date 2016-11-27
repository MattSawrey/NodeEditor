using UnityEngine;
using UnityEditor;

public class EditorMessage : EditorWindow
{
    //References
    private static EditorMessage window;

    private static string message;
    private static Color color;
    private static bool selfDelete = false;

    public static void Init(NodeEditor editor, string editorMessage)
    { Init(editor, editorMessage, Color.white, Vector2.one, false); }

    public static void Init(NodeEditor editor, string editorMessage, Color textColor)
    { Init(editor, editorMessage, textColor, Vector2.one, false); }

    public static void Init(NodeEditor editor, string editorMessage, Color textColor, Vector2 windowPosition)
    { Init(editor, editorMessage, textColor, windowPosition, false); }

    public static void Init(NodeEditor editor, string editorMessage, Color textColor, Vector2 windowPosition, bool windowSelfDelete)
    {
        if (window == null)
            window = GetWindow<EditorMessage>();

        //Assign Vars
        message = editorMessage;
        color = textColor;
        selfDelete = windowSelfDelete;

        window.minSize = new Vector2(180f, 100f);
        window.position = new Rect(windowPosition, window.minSize);
        window.Show();
    }

    void OnGUI()
    {
        GUI.contentColor = color;
        GUILayout.Label(message);
        if(!selfDelete)
        {
            if (GUILayout.Button("Okay"))
                window.Close();
        }
    }

    //TODO - To be completed when I have editor coroutines working
    private void DeleteAfterTime(float timeToDelete)
    {
        float timer = 0f;
        while (timer < timeToDelete)
        {
            timer += Time.fixedDeltaTime;
        }
        window.Close();
    }
}

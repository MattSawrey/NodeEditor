using UnityEngine;
using UnityEditor;

public class EditorMessage : EditorWindow
{
    //References
    private static EditorMessage window;
    private static NodeEditor callingEditor;

    private static string message;
    private static Color color;
    private static bool selfDelete = false;
    private static float timeToSelfDelete = 0f;

    public static void Init(NodeEditor editor, string editorMessage)
    { Init(editor, editorMessage, Color.white, Vector2.one, false, 0f); }

    public static void Init(NodeEditor editor, string editorMessage, Color textColor)
    { Init(editor, editorMessage, textColor, Vector2.one, false, 0f); }

    public static void Init(NodeEditor editor, string editorMessage, Color textColor, Vector2 windowPosition)
    { Init(editor, editorMessage, textColor, windowPosition, false, 0f); }

    public static void Init(NodeEditor editor, string editorMessage, Color textColor, Vector2 windowPosition, bool windowSelfDelete, float windowTimeToSelfDelete)
    {
        if (window == null)
            window = GetWindow<EditorMessage>();

        callingEditor = editor;

        //Assign Vars
        message = editorMessage;
        color = textColor;
        selfDelete = windowSelfDelete;
        timeToSelfDelete = windowTimeToSelfDelete;

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

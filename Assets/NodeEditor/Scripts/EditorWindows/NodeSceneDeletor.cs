using UnityEngine;
using UnityEditor;

public class NodeSceneDeletor : EditorWindow
{
    

    //References
    private static NodeSceneDeletor window;
    private static NodeEditor callingEditor;

    private static string nameOfSceneToDelete;
    private static int deleteSceneActionOption = 0;

    public static void Init(NodeEditor editor, string sceneToDeleteName)
    {
        if (window == null)
            window = GetWindow<NodeSceneDeletor>();

        callingEditor = editor;

        nameOfSceneToDelete = sceneToDeleteName;

        window.minSize = new Vector2(200f, 120f);
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("You are about to delete the following scene:");
        GUILayout.Space(2f);
        EditorGUIStatics.DrawLine(new Vector2(0f, 20f), new Vector2(window.position.width, 20f), Color.black, 2f);

        GUI.skin.label.fontStyle = FontStyle.Bold;
        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
        GUI.contentColor = Color.yellow;
        GUILayout.Label(nameOfSceneToDelete);
        GUI.contentColor = Color.white;
        GUI.skin.label.alignment = TextAnchor.UpperLeft;
        GUI.skin.label.fontStyle = FontStyle.Normal;
        EditorGUIStatics.DrawLine(new Vector2(0f, 40f), new Vector2(window.position.width, 40f), Color.black, 2f);

        GUILayout.Space(2f);
        GUILayout.Label("Are you sure you want to delete this scene?"); 

        deleteSceneActionOption = EditorGUILayout.Popup("Please select and action:", deleteSceneActionOption, new string[] { "No", "No, certainly not", "Yes, I am sure I want to delete this scene", "Absolutely not" });

        if (GUILayout.Button("Commit Action"))
        {
            switch (deleteSceneActionOption)
            {
                case 0:
                    window.Close();
                    break;
                case 1:
                    window.Close();
                    break;
                case 2:
                    DeleteScene();
                    break;
                case 3:
                    window.Close();
                    break;
            }            
        }
    }

    private void DeleteScene()
    {
        //Debug.Log(NodeEditor.nodeSceneSaveFilePath + "/" + nameOfSceneToDelete);
        bool isSceneDeleted = AssetDatabase.DeleteAsset(NodeEditor.nodeSceneSaveFilePath + "/" + nameOfSceneToDelete + ".asset");

        if (isSceneDeleted)
        {
            EditorMessage.Init(callingEditor, "Deleted scene - '" + nameOfSceneToDelete + "'.");
            AssetDatabase.SaveAssets();
            callingEditor.ReAssignVarsOnSceneDelete();
        }
        else
        {
            Debug.Log("Couldn't delete scene. Error with editor");
        }            

        window.Close();
    }
}




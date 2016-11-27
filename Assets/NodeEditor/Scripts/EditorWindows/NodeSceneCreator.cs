using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class NodeSceneCreator : EditorWindow
{
    //Scenes
    private static string nodeSceneSaveFilePath = "Assets/Resources/DialogueScenes/";

    private static string nodeSceneName = "";
    private static List<string> currentSceneNames;
    private static bool doesSceneNameAlreadyExist
    {
        get
        {
            for (int i = 0; i < currentSceneNames.Count; i++)
                if (currentSceneNames[i] == nodeSceneName)
                    return true;

            return false;
        }
    }

    //References
    private static NodeSceneCreator window;
    private static NodeEditor callingEditor;

    public static void Init(NodeEditor editor)
    {
        if (window == null)
            window = GetWindow<NodeSceneCreator>();

        GetCurrentSceneNames();

        callingEditor = editor;

        window.minSize = new Vector2(400f, 100f);
        window.Show();
    }

    void OnGUI()
    {
        float maxButtonWidth = 100f;

        nodeSceneName = EditorGUILayout.TextField("New Scene Name", nodeSceneName);

        GUILayout.BeginVertical(GUILayout.ExpandHeight(true));
        {
            //Control buttons
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (!doesSceneNameAlreadyExist)
                {
                    if (GUILayout.Button("Create Scene", GUILayout.MaxWidth(maxButtonWidth)))
                    {
                        CreateNewScene();
                    }
                }
                else
                {
                    GUI.color = Color.red;
                    EditorStyles.label.wordWrap = true;
                    EditorGUILayout.LabelField("Scene Name Already Exists. Pick Another Name!");
                    GUI.color = Color.white;
                }
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
    }

    private void CreateNewScene()
    {
        NodeScene newScene = NodeScene.CreateInstance(nodeSceneName);
        AssetDatabase.CreateAsset(newScene, nodeSceneSaveFilePath + nodeSceneName + ".asset");
        EditorUtility.SetDirty(newScene);
        AssetDatabase.SaveAssets();
        callingEditor.LoadScenes();
        callingEditor.LoadScene(callingEditor.numScenes-1);
        callingEditor.Repaint();
        window.Close();
    }

    private static void GetCurrentSceneNames()
    {
        if (currentSceneNames == null)
            currentSceneNames = new List<string>();
        else
            currentSceneNames.Clear();

        var sceneDirectories = Directory.GetFiles("Assets/Resources/DialogueScenes");
        for (int i = 0; i < sceneDirectories.Length; i += 2)
        {
            var sceneNameDirSplit = sceneDirectories[i].Split('\\');
            var sceneNamestring = sceneNameDirSplit[sceneNameDirSplit.Length - 1].Split('.')[0];
            currentSceneNames.Add(sceneNamestring);
        }
    }
}

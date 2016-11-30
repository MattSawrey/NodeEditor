using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;

public class NodeEditor : EditorWindow
{
    #region - Vars and init

    //Scenes Management
    public static string nodeSceneSaveFilePath = "Assets/Resources/DialogueScenes";
    private List<NodeScene> nodeScenes;
    public int numScenes { get { return nodeScenes.Count; } }
    private NodeScene selectedScene;
    private int selectedSceneIndex = 0;

    //Mouse positioning
    public static Vector2 mousePosition;    
    public static Vector2 zoomScrollCorrectedMousePosition;
    public static Vector2 zoomScrollCorrectedMenuPosition;

    //ScrollingWindow
    private Rect scrollViewRect;
    private Vector2 scrollPos = Vector2.zero;
    private Vector2 scrollStartMousePos;
    private bool isWindowScrollActive = false;
    
    //Zoom
    public static float zoomScale = 1f;
    private static float zoomScaleLowerLimit = 0.6f;
    private static float zoomScaleUpperLimit = 1.4f;

    //Lerping Cam Position
    private bool posLerp = false;
    private Vector2 targetPosition;
    private float counter = 0f;

    //Editor Message
    private bool showEditorMessage = false;
    private string editorMessage;
    private Color editorMessageColor = Color.white;
    private float editorMessageTimer = 0f;

    //Sizings
    private int topPanelHeight = 22;
    private static float leftPaneWidth = 200f;
    GUILayoutOption[] DrawSceneControlButtonsizing = new GUILayoutOption[]
    {
        GUILayout.Width(60f),
        GUILayout.Height(40f),
    };

    //Node Connection Types
    public static NodeConnectionTypes nodeConnectionTypes;
    public static NodeConnectionType selectedConnectionType;

    //REFERENCES
    public static NodeEditor window;

    [MenuItem("Window/Custom/NodeEditor")]
    static void Init()
    {
        LoadNodeConnectionTypes();
        window = GetWindow<NodeEditor>();
        window.Show();
    }

    #endregion

    #region - Creation and deletion

    //Scenes
    private void CreateNewScene()
    {
        NodeSceneCreator.Init(this, mousePosition);
    }
    
    public List<string> GetSceneNames()
    {
        List<string> sceneNames = new List<string>();
        for (int i = 0; i < nodeScenes.Count; i++)
            sceneNames.Add(nodeScenes[i].sceneName);
        return sceneNames;
    }

    public static void LoadNodeConnectionTypes()
    {
        nodeConnectionTypes = AssetDatabase.LoadAssetAtPath<NodeConnectionTypes>(NodeConnectionTypes.assetFilePath);
    }

    public void LoadScenes()
    {
        if (nodeScenes == null)
            nodeScenes = new List<NodeScene>();
        else 
            nodeScenes.Clear();

        nodeScenes = Resources.LoadAll<NodeScene>("DialogueScenes").ToList();
        LoadScene(selectedSceneIndex);
    }

    public void LoadScene(int sceneIndex)
    {
        if (nodeScenes.Count > 0)
        {
            selectedScene = nodeScenes[sceneIndex];
            selectedSceneIndex = sceneIndex;
            selectedScene.SetupScene();
        }
    }

    private void SaveSelectedScene()
    {
        if (selectedScene != null)
        {
            EditorUtility.SetDirty(selectedScene);
            for (int i = 0; i < selectedScene.nodes.Count; i++)
                EditorUtility.SetDirty(selectedScene.nodes[i]);
            AssetDatabase.SaveAssets();
            DisplayEditorMessage("Scene Saved!", Color.green);
            //EditorMessage.Init(this, "Scene Saved!", Color.green, mousePosition, new Vector2(60f, 60f));
        }
        else
            EditorMessage.Init(this, "No scene selected to save!", Color.red, mousePosition, new Vector2(60f, 60f));
    }

    private void DeleteSelectedScene()
    {
        if (selectedScene != null)
            NodeSceneDeletor.Init(this, selectedScene.sceneName);
        else
            EditorMessage.Init(this, "No scene selected to delete!", Color.red, mousePosition, new Vector2(60f, 60f));
    }

    public void ReAssignVarsOnSceneDelete()
    {
        if(selectedSceneIndex > 0)
            selectedSceneIndex--;
        LoadScenes();
        Repaint();
    }

    #endregion

    #region - GUI Events and Control

    void OnGUI()
    {
        if (nodeScenes == null)
            LoadScenes();

        if (nodeConnectionTypes == null)
            LoadNodeConnectionTypes();

        Event e = Event.current;
        mousePosition = e.mousePosition;
        zoomScrollCorrectedMousePosition = (new Vector2(mousePosition.x - 200f, mousePosition.y - 21f)/ zoomScale) + scrollPos + new Vector2(200f, 21f);
        zoomScrollCorrectedMenuPosition = ((new Vector2(mousePosition.x - 200f, mousePosition.y - 40f) + scrollPos) / zoomScale) + new Vector2(200f, 40f);

        GUILayout.BeginHorizontal();
        {
            //Scene Control Panel
            GUILayout.BeginVertical(GUILayout.Width(leftPaneWidth));
            {
                GUILayout.BeginVertical(GUILayout.Width(leftPaneWidth));
                {
                    DrawSceneControlButtons();
                    DrawSelectionGrids();
                }
                GUILayout.EndVertical();

                //Editor Message
                GUILayout.BeginVertical(GUILayout.Width(leftPaneWidth), GUILayout.Height(20f));
                {
                    DrawEditorMessage();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();
            EditorGUIStatics.DrawLine(new Vector2(leftPaneWidth, 0f), new Vector2(leftPaneWidth, position.height), Color.black, 2f);
            
            //Scene Name and Zoom Bar    
            GUILayout.BeginHorizontal(GUILayout.Height(topPanelHeight));
            {
                DrawScrollAreaHeader();
            }
            GUILayout.EndHorizontal();

            EditorGUIStatics.DrawLine(new Vector2(leftPaneWidth, topPanelHeight), new Vector2(position.width, topPanelHeight), Color.black, 2f);

            //Scrollable window area
            if (!(position.width < leftPaneWidth+50f))
                DrawScrollAreaGUI(e);
        }
        GUILayout.EndHorizontal();
    }
    
    private void DrawSceneControlButtons()
    {
        GUILayout.Label("Scene Control:");

        GUILayout.BeginHorizontal();
        {
            GUI.backgroundColor = Color.blue;
            if (GUILayout.Button("Create\nScene", DrawSceneControlButtonsizing))
                CreateNewScene();
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Save\nScene", DrawSceneControlButtonsizing))
                SaveSelectedScene();
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Delete\nScene", DrawSceneControlButtonsizing))
                DeleteSelectedScene();
            GUI.backgroundColor = Color.white;
        }
        GUILayout.EndHorizontal();
    }

    private void DrawSelectionGrids()
    {
        GUILayout.BeginScrollView(Vector2.right, GUILayout.ExpandHeight(false));
        {
            GUILayout.Label("Scene Selection:");
            if (nodeScenes.Count > 0)
            {
                int selectedSceneSelection = selectedSceneIndex;
                selectedSceneIndex = GUILayout.SelectionGrid(selectedSceneIndex, GetSceneNames().ToArray(), 1, GUILayout.ExpandHeight(false));

                if (selectedSceneSelection != selectedSceneIndex)
                    LoadScene(selectedSceneIndex);
            }
        }
        GUILayout.EndScrollView();

        GUILayout.BeginScrollView(Vector2.right, GUILayout.ExpandHeight(false));
        {
            GUILayout.Label("Node Selection:");
            if (selectedScene.nodes.Count > 0)
            {
                int currentNodeSelection = selectedScene.SelectedNodeIndex;
                selectedScene.SelectedNodeIndex = GUILayout.SelectionGrid(selectedScene.SelectedNodeIndex, selectedScene.GetNodeNames().ToArray(), 1, GUILayout.ExpandHeight(false));

                if (currentNodeSelection != selectedScene.SelectedNodeIndex)
                {
                    selectedScene.nodes[currentNodeSelection].DeselectWindow();
                    LerpToNode(selectedScene.SelectedNodeIndex);
                    selectedScene.nodes[selectedScene.SelectedNodeIndex].SelectWindow();
                }
            }
        }
        GUILayout.EndScrollView();
    }

    private void DrawScrollAreaHeader()
    {
        GUILayout.Space(8f);

        if (nodeScenes.Count > 0)
        {
            EditorGUILayout.LabelField("Scene Name: ", GUILayout.MinWidth(85f), GUILayout.MaxWidth(85f), GUILayout.ExpandWidth(false));
            selectedScene.sceneName = GUILayout.TextField(selectedScene.sceneName, GUILayout.MaxWidth(180f), GUILayout.MinWidth(180f), GUILayout.ExpandWidth(false));
        }

        GUILayout.Label("Zoom: ", GUILayout.Width(40f), GUILayout.ExpandWidth(false));
        zoomScale = GUILayout.HorizontalSlider(zoomScale, zoomScaleLowerLimit, zoomScaleUpperLimit, GUILayout.MinWidth(200f), GUILayout.ExpandWidth(true));
        GUILayout.Label(Math.Round(zoomScale, 2).ToString(), GUILayout.Width(40f), GUILayout.ExpandWidth(false));
    }

    private void DrawEditorMessage()
    {
        if (showEditorMessage)
        {
            GUI.contentColor = editorMessageColor;
            GUI.skin.label.wordWrap = true;
            GUILayout.Label(editorMessage);
            GUI.skin.label.wordWrap = false;
            GUI.contentColor = Color.white;
        }        
    }

    private void DrawScrollAreaGUI(Event e)
    {
        scrollViewRect = new Rect(leftPaneWidth, topPanelHeight, position.width - (leftPaneWidth), position.height- topPanelHeight);
        scrollViewRect = EditorZoomArea.Begin(zoomScale, scrollViewRect);

        using (var scope = new GUI.ScrollViewScope(scrollViewRect, scrollPos, new Rect(leftPaneWidth, topPanelHeight, 10000f, 10000f)))
        {
            scope.handleScrollWheel = false;
            scrollPos = scope.scrollPosition;

            EditorGUIStatics.DrawBackgroundGrid(scrollViewRect, scrollPos, 40f, Color.gray, 2f);

            if(selectedScene != null)
                if (selectedScene.nodes != null)
                {
                    selectedScene.DrawNodeGUI(e, zoomScrollCorrectedMousePosition);
                    ScrollViewGUIEvents(e);
                }
        }
        EditorZoomArea.End();
    }

    public void ScrollViewGUIEvents(Event currentEvent)
    {
        //Right Click
        if (currentEvent.button == 1)
        {
            if (currentEvent.type == EventType.MouseDown)
            {
                if (!selectedScene.clickedOnNode)
                {
                    GenericMenu menu = new GenericMenu();
                    
                    menu.AddItem(new GUIContent("Nodes/Basic Node"), false, ContextCallback, "Add Node");
                    menu.DropDown(new Rect(zoomScrollCorrectedMenuPosition, Vector2.zero));
                }
                currentEvent.Use();
            }
        }
        //Left Click
        else if (currentEvent.button == 0)
        {
            if (currentEvent.type == EventType.MouseDown)
            {
                if (scrollViewRect.Contains(mousePosition))
                {
                    if (GUI.GetNameOfFocusedControl() != "nothing")
                        GUI.FocusControl("nothing");

                    if (!selectedScene.clickedOnNode)
                    {
                        if (!isWindowScrollActive && new Rect(scrollViewRect.x, scrollViewRect.y + 14f, scrollViewRect.width, scrollViewRect.height - 14f).Contains(mousePosition))
                            isWindowScrollActive = true;
                        scrollStartMousePos = mousePosition;
                    }
                }
            }
            else if (currentEvent.type == EventType.MouseDrag)
            {
                if (!selectedScene.isConnectionModeActive && !selectedScene.isNodeDragActive)
                    if (isWindowScrollActive)
                    {
                        if (posLerp)
                            posLerp = false;
                        Vector2 mouseMovementDifference = (mousePosition - scrollStartMousePos);
                        scrollPos -= new Vector2(mouseMovementDifference.x/zoomScale, mouseMovementDifference.y/zoomScale);
                        scrollStartMousePos = mousePosition;
                        currentEvent.Use();
                    }
            }
            else if (currentEvent.type == EventType.MouseUp || currentEvent.type == EventType.DragExited)
            {
                if (isWindowScrollActive)
                    isWindowScrollActive = false;

                if (GUIUtility.hotControl != 0)
                    GUIUtility.hotControl = 0;

                currentEvent.Use();
            }
        }
        //Mouse Wheel
        if (currentEvent.type == EventType.ScrollWheel && currentEvent.control)
        {
            zoomScale += -(currentEvent.delta.y / 100f);
            zoomScale = Mathf.Clamp(zoomScale, zoomScaleLowerLimit, zoomScaleUpperLimit);            
            currentEvent.Use();
        }

        if (currentEvent.modifiers == EventModifiers.Control && currentEvent.keyCode == KeyCode.S)
        {
            Debug.Log("CTRL+S Keys Pressed");
        }
    }

    private void ContextCallback(object obj)
    {
        string callback = obj.ToString();
        switch (callback)
        {
            case "Add Node":
                selectedScene.CreateNode(zoomScrollCorrectedMousePosition);
                break;
        }
    }

    public void ReAssignConnection(NodeConnectionType connectionType, Node outputNode)
    {
        selectedConnectionType = connectionType;
        selectedScene.selectedNode = outputNode;
        selectedScene.isConnectionModeActive = true;
    }

    protected virtual void DrawNodeWindow(int id)
    {
        selectedScene.nodes[id].DrawNode();
    }

    public void DisplayEditorMessage(string message, Color color)
    {
        showEditorMessage = true;
        editorMessage = message;
        editorMessageColor = color;
    }

    private void LerpToNode(int index)
    {
        posLerp = true;
        counter = 0f;

        Node selectedNode = selectedScene.nodes[index];        
        Vector2 offset = new Vector2(position.width / 2f, position.height / 2f);
        targetPosition = selectedNode.nodeRect.position - offset;
    }

    void Update()
    {
        if (posLerp)
        {
            if (counter > 1f)
            {
                posLerp = false;
                counter = 0f;
            }
            else
            {
                Repaint();
                scrollPos = Vector2.Lerp(scrollPos, targetPosition, counter);
                counter += 0.002f;
            }
        }
        if (showEditorMessage)
        {
            if (editorMessageTimer > 3f)
            {
                showEditorMessage = false;
                editorMessageTimer = 0f;
                editorMessageColor = Color.white;
                Repaint();
            }
            else      
                editorMessageTimer += 0.03f;
        }
    }
    #endregion
}

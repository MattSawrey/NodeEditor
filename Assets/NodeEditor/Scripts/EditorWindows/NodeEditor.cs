using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class NodeEditor : EditorWindow
{
    #region - Vars and init

    //Scenes
    public static string nodeSceneSaveFilePath = "Assets/Resources/DialogueScenes";
    private List<NodeScene> nodeScenes;
    public int numScenes { get { return nodeScenes.Count; } }
    private NodeScene selectedScene;
    private int selectedSceneIndex = 0;

    //Nodes
    private static NodeWindow selectedNode;
    private int selectedNodeIndex = 0;
    private int mouseOverNodeIndex = 0;
    private int lastSelectedNodeIndex = 0;
    private bool clickedOnNode = false;
    private static Color selectedNodeColor = new Color(0.4f, 0.95f, 0.5f);

    //Event Control
    private static bool showScenePanel = true;
    private static bool isConnectionModeActive = false;

    //Mouse positioning
    private Vector2 mousePosition;    
    private Vector2 zoomScrollCorrectedMousePosition;
    private Vector2 zoomScrollCorrectedMenuPosition;

    //ScrollingWindow
    private Rect nodeScrollViewRect;
    private Vector2 scrollPos = Vector2.zero;
    private Vector2 scrollStartMousePos;
    private bool isWindowScrollActive = false;
    private bool isNodeDragActive = false;
    public static float zoomScale = 1f;

    //Side Panel
    private List<string> sceneSelectionGridContent = new List<string>();
    private List<string> nodeSelectionGridContent = new List<string>();

    //Lerping Cam Position
    private bool posLerp = false;
    private Vector2 targetPosition;
    private float counter = 0f;

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
    private static NodeEditor window;

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
        //Call up the NodeSceneCreator Menu
        NodeSceneCreator.Init(this);
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
        ReAssignsceneSelectionGridContent();
        LoadScene(selectedSceneIndex);
        ReAssignnodeSelectionGridContent();
    }

    public void LoadScene(int sceneIndex)
    {
        if (nodeScenes.Count > 0)
        {
            selectedScene = nodeScenes[sceneIndex];
            selectedSceneIndex = sceneIndex;
            ReAssignnodeSelectionGridContent();
            selectedNodeIndex = selectedScene.GetSelectedNodeIndex();
            lastSelectedNodeIndex = selectedNodeIndex;
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
        }
        else
        {
            EditorMessage.Init(this, "No scene selected to save!", Color.red, mousePosition);
        }
    }

    private void DeleteSelectedScene()
    {
        if (selectedScene != null)
        {
            NodeSceneDeletor.Init(this, selectedScene.sceneName);
        }
        else
        {
            EditorMessage.Init(this, "No scene selected to delete!", Color.red, mousePosition);
        }
    }

    public void ReAssignVarsOnSceneDelete()
    {
        if(selectedSceneIndex > 0)
            selectedSceneIndex--;

        LoadScenes();
        Repaint();
    }

    //Scene Selection grid
    public void ReAssignsceneSelectionGridContent()
    {
        sceneSelectionGridContent = new List<string>();
        for (int i = 0; i < nodeScenes.Count; i++)
            sceneSelectionGridContent.Add(nodeScenes[i].name);
    }

    public void ReAssignnodeSelectionGridContent()
    {
        nodeSelectionGridContent = new List<string>();
        if(selectedScene != null)
            for (int i = 0; i < selectedScene.nodes.Count; i++)
                nodeSelectionGridContent.Add(selectedScene.nodes[i].nodeName);
    }

    //Nodes
    private void AddNode()
    {
        NodeWindow newNode = NodeWindow.CreateInstance(new Rect(zoomScrollCorrectedMousePosition, new Vector2(NodeWindow.minWidth, NodeWindow.minHeight)));
        newNode.nodeName = (selectedScene.nodes.Count + 1).ToString();
        selectedScene.nodes.Add(newNode);
        AssetDatabase.AddObjectToAsset(newNode, selectedScene);
        EditorUtility.SetDirty(selectedScene);
        EditorUtility.SetDirty(newNode);
        AssetDatabase.SaveAssets();
        selectedNode = newNode;

        lastSelectedNodeIndex = selectedNodeIndex;
        selectedNodeIndex = selectedScene.nodes.Count-1;
        selectedScene.nodes[lastSelectedNodeIndex].DeselectWindow();
        selectedScene.nodes[selectedNodeIndex].SelectWindow();

        ReAssignnodeSelectionGridContent();
    }

    private void DeleteNode(int nodeIndex)
    {
        NodeWindow nodeToDelete = selectedScene.nodes[selectedNodeIndex];

        foreach (NodeWindow nw in selectedScene.nodes)
            nw.DeleteConnectionsToNode(nodeToDelete);

        selectedScene.nodes.RemoveAt(nodeIndex);

        EditorUtility.SetDirty(selectedScene);
        AssetDatabase.SaveAssets();

        ReAssignnodeSelectionGridContent();
        Repaint();
    }

    #endregion

    #region - GUI Events and Control

    void OnGUI()
    {
        //Checks to load data
        if (nodeScenes == null)
            LoadScenes();

        if (nodeConnectionTypes == null)
            LoadNodeConnectionTypes();

        //Vars to track
        Event e = Event.current;

        mousePosition = e.mousePosition;
                
        zoomScrollCorrectedMousePosition = new Vector2(mousePosition.x - 200f, mousePosition.y - 21f);
        zoomScrollCorrectedMousePosition = (zoomScrollCorrectedMousePosition / zoomScale);
        zoomScrollCorrectedMousePosition = new Vector2(zoomScrollCorrectedMousePosition.x + 200f, zoomScrollCorrectedMousePosition.y + 21f) + scrollPos;

        zoomScrollCorrectedMenuPosition = new Vector2(mousePosition.x - 200f, mousePosition.y - 40f);
        zoomScrollCorrectedMenuPosition = (zoomScrollCorrectedMenuPosition + scrollPos) / zoomScale;
        zoomScrollCorrectedMenuPosition = new Vector2(zoomScrollCorrectedMenuPosition.x + 200f, zoomScrollCorrectedMenuPosition.y + 40f);

        GUILayout.BeginHorizontal();
        {
            //Scene Control Panel
            GUILayout.BeginVertical(GUILayout.Width(leftPaneWidth));
            {
                showScenePanel = EditorGUILayout.Foldout(showScenePanel, "Scene Panel");
                if (showScenePanel)
                {
                    GUILayout.BeginVertical(GUILayout.Width(leftPaneWidth));
                    {
                        DrawSceneControlButtons();
                        DrawSelectionGrids();
                    }
                    GUILayout.EndVertical();
                }
            }
            GUILayout.EndVertical();
            EditorGUIStatics.DrawLine(new Vector2(leftPaneWidth, 0f), new Vector2(leftPaneWidth, position.height), Color.black, 2f);
            
            //Scene Name and Zoom Bar    
            GUILayout.BeginHorizontal(GUILayout.Height(topPanelHeight));
            {
                GUILayout.Space(8f);

                if (nodeScenes.Count > 0)
                {
                    GUILayout.Label("Scene Name: ", GUILayout.Width(85f));
                    selectedScene.sceneName = GUILayout.TextField(selectedScene.name, GUILayout.MaxWidth(180f));
                }

                GUILayout.Label("Zoom: ", GUILayout.Width(40f));
                zoomScale = EditorGUILayout.Slider(zoomScale, 0.5f, 1.5f);

                if (GUI.changed)
                    ReAssignsceneSelectionGridContent();
            }
            GUILayout.EndHorizontal();
            EditorGUIStatics.DrawLine(new Vector2(leftPaneWidth, topPanelHeight), new Vector2(position.width, topPanelHeight), Color.black, 2f);

            //Scrollable window area
            if (!(position.width < leftPaneWidth+50f))
            {
                ScrollAreaGUI(e);
            }
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
            {
                CreateNewScene();
            }
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Save\nScene", DrawSceneControlButtonsizing))
            {
                SaveSelectedScene();
            }
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Delete\nScene", DrawSceneControlButtonsizing))
            {
                DeleteSelectedScene();
            }
            GUI.backgroundColor = Color.white;
        }
        GUILayout.EndHorizontal();
    }

    private void DrawSelectionGrids()
    {
        GUILayout.BeginScrollView(Vector2.right, GUILayout.ExpandHeight(false));
        {
            GUILayout.Label("Scene Selection:");

            if (sceneSelectionGridContent.Count > 0)
            {
                int selectedSceneSelection = selectedSceneIndex;
                selectedSceneIndex = GUILayout.SelectionGrid(selectedSceneIndex, sceneSelectionGridContent.ToArray(), 1, GUILayout.ExpandHeight(false));

                if (selectedSceneSelection != selectedSceneIndex)
                    LoadScene(selectedSceneIndex);
            }
        }
        GUILayout.EndScrollView();

        GUILayout.BeginScrollView(Vector2.right, GUILayout.ExpandHeight(false));
        {
            GUILayout.Label("Node Selection:");

            if (nodeSelectionGridContent.Count != 0)
            {
                int currentNodeSelection = selectedNodeIndex;
                selectedNodeIndex = GUILayout.SelectionGrid(selectedNodeIndex, nodeSelectionGridContent.ToArray(), 1, GUILayout.ExpandHeight(false));

                if (currentNodeSelection != selectedNodeIndex)
                {
                    selectedScene.nodes[currentNodeSelection].DeselectWindow();
                    LerpToNode(selectedNodeIndex);
                }
            }
        }
        GUILayout.EndScrollView();
    }

    private void ScrollAreaGUI(Event e)
    {
        nodeScrollViewRect = new Rect(leftPaneWidth, topPanelHeight, position.width - (leftPaneWidth), position.height- topPanelHeight);
        nodeScrollViewRect = EditorZoomArea.Begin(zoomScale, nodeScrollViewRect);

        using (var scope = new GUI.ScrollViewScope(nodeScrollViewRect, scrollPos, new Rect(leftPaneWidth, topPanelHeight, 10000f, 10000f)))
        {
            scope.handleScrollWheel = false;
            scrollPos = scope.scrollPosition;

            EditorGUIStatics.DrawBackgroundGrid(nodeScrollViewRect, scrollPos, 40f, Color.gray, 2f);

            if(selectedScene != null)
                if (selectedScene.nodes != null)
                {
                    for (int i = 0; i < selectedScene.nodes.Count; i++)
                        selectedScene.nodes[i].DrawCurves();

                    ScrollViewGUIEvents(e);

                    BeginWindows();
                    for (int i = 0; i < selectedScene.nodes.Count; i++)
                    {
                        GUI.backgroundColor = selectedScene.nodes[i].isSelected ? selectedNodeColor : Color.white;                 
                        selectedScene.nodes[i].nodeRect = GUI.Window(i, selectedScene.nodes[i].nodeRect, DrawNodeWindow, selectedScene.nodes[i].nodeTitle);

                        if (GUI.changed)
                            ReAssignnodeSelectionGridContent();
                    }
                    GUI.backgroundColor = Color.white;
                    EndWindows();
                }
        }

        EditorZoomArea.End();

    }

    public void ScrollViewGUIEvents(Event currentEvent)
    {
        if (isConnectionModeActive && selectedNode != null)
        {
            Rect mouseRect = new Rect(zoomScrollCorrectedMousePosition.x, zoomScrollCorrectedMousePosition.y, 10, 10);
            EditorGUIStatics.DrawNodeCurve(selectedNode.nodeRect, mouseRect);
            Repaint();
        }

        //Right Click
        if (currentEvent.button == 1)
        {
            if (currentEvent.type == EventType.MouseDown)
            {
                UpdateEventCheckVars();
                if (!clickedOnNode)
                {
                    GenericMenu menu = new GenericMenu();
                    
                    menu.AddItem(new GUIContent("Nodes/Basic Node"), false, ContextCallback, "Add Node");
                    menu.DropDown(new Rect(zoomScrollCorrectedMenuPosition, Vector2.zero));
                }
                else
                {
                    lastSelectedNodeIndex = selectedNodeIndex;
                    selectedNodeIndex = mouseOverNodeIndex;
                    selectedScene.nodes[lastSelectedNodeIndex].DeselectWindow();
                    selectedScene.nodes[selectedNodeIndex].SelectWindow();

                    if (!isNodeDragActive)
                    {
                        isNodeDragActive = true;
                    }

                    GenericMenu menu = new GenericMenu();

                    for(int i=0; i < nodeConnectionTypes.connectionTypes.Count; i++)
                        if(selectedScene.nodes[selectedNodeIndex].MoreConnectionsOfTypeAllowed(nodeConnectionTypes.connectionTypes[i]))
                            menu.AddItem(new GUIContent("Add Connection/" + nodeConnectionTypes.connectionTypes[i].connectionName), false, ContextCallback, "Connection-" + nodeConnectionTypes.connectionTypes[i].connectionName);

                    if (selectedScene.nodes[selectedNodeIndex].hasoutputNodes)
                        menu.AddItem(new GUIContent("Delete All Connections"), false, ContextCallback, "Delete Connections");

                    menu.AddSeparator("");

                    menu.AddItem(new GUIContent("Delete Node"), false, ContextCallback, "Delete Node");
                    menu.DropDown(new Rect(zoomScrollCorrectedMenuPosition, Vector2.one));
                    //zoomScrollCorrectedMousePosition / (1f - ((1f - zoomScale) / 2f))
                }
                currentEvent.Use();
            }
        }
        //Left Click
        else if (currentEvent.button == 0)
        {
            UpdateEventCheckVars();
            if (currentEvent.type == EventType.MouseDown)
            {
                //TODO - removing the control of the editorwindow from the panels
                if (nodeScrollViewRect.Contains(mousePosition))
                    if (GUI.GetNameOfFocusedControl() != "nothing")
                        GUI.FocusControl("nothing");

                if (isConnectionModeActive)
                {
                    lastSelectedNodeIndex = selectedNodeIndex;
                    selectedNodeIndex = mouseOverNodeIndex;
                    //Add a connection
                    if (clickedOnNode && !selectedScene.nodes[selectedNodeIndex].Equals(selectedNode))
                    {
                        selectedScene.nodes[lastSelectedNodeIndex].DeselectWindow();
                        selectedScene.nodes[selectedNodeIndex].SelectWindow();

                        selectedNode.SetConnection(selectedScene.nodes[selectedNodeIndex], selectedConnectionType);
                        isConnectionModeActive = false;
                        selectedNode = null;
                    }

                    if (!clickedOnNode)
                    {
                        isConnectionModeActive = false;
                        selectedNode = null;
                    }
                }
                else
                {
                    if (clickedOnNode)
                    {
                        if (!isNodeDragActive)
                        {
                            lastSelectedNodeIndex = selectedNodeIndex;
                            selectedNodeIndex = mouseOverNodeIndex;
                            selectedScene.nodes[lastSelectedNodeIndex].DeselectWindow();
                            selectedScene.nodes[selectedNodeIndex].SelectWindow();

                            isNodeDragActive = true;
                        }
                    }

                    //Slight addition to the nodescrollviewrect y pos to account for the zoom slider
                    if (!isWindowScrollActive && new Rect(nodeScrollViewRect.x, nodeScrollViewRect.y + 14f, nodeScrollViewRect.width, nodeScrollViewRect.height - 14f).Contains(mousePosition))
                    {
                        isWindowScrollActive = true;
                    }
                    scrollStartMousePos = mousePosition;
                }
            }
            else if (currentEvent.type == EventType.MouseDrag)
            {
                if (!isConnectionModeActive && !isNodeDragActive)
                {
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
            }
            else if (currentEvent.type == EventType.MouseUp || currentEvent.type == EventType.DragExited)
            {
                if (isWindowScrollActive)
                    isWindowScrollActive = false;

                if (isNodeDragActive)
                    isNodeDragActive = false;

                if (GUIUtility.hotControl != 0)
                    GUIUtility.hotControl = 0;

                currentEvent.Use();
            }
        }
        //Mouse Wheel
        if (currentEvent.type == EventType.ScrollWheel && currentEvent.control)
        {
            zoomScale += -(currentEvent.delta.y / 100f);
            currentEvent.Use();
        }
       
        ResetEventCheckVars();
    }

    private void ContextCallback(object obj)
    {
        string[] splitString = obj.ToString().Split('-');
        string callback = splitString[0];
        UpdateEventCheckVars();
        switch (callback)
        {
            case "Add Node":
                AddNode();
                break;
            case "Delete Node":
                DeleteNode(selectedNodeIndex);
                break;
            case "Connection":
                for (int i = 0; i < nodeConnectionTypes.connectionTypes.Count; i++)
                {
                    if (splitString[1] == nodeConnectionTypes.connectionTypes[i].connectionName)
                    {
                        selectedConnectionType = nodeConnectionTypes.connectionTypes[i];
                    }
                }

                selectedNode = selectedScene.nodes[selectedNodeIndex];
                isConnectionModeActive = true;
                break;
            case "Delete Connections":
                selectedScene.nodes[selectedNodeIndex].ClearConnections();
                break;
        }
    }

    public static void ReAssignConnection(NodeConnectionType connectionType, NodeWindow outputNode)
    {
        selectedConnectionType = connectionType;
        selectedNode = outputNode;
        isConnectionModeActive = true;
    }

    private void UpdateEventCheckVars()
    {
        if (selectedScene.nodes != null)
            for (int i = 0; i < selectedScene.nodes.Count; i++)
                if (selectedScene.nodes[i].nodeRect.Contains(zoomScrollCorrectedMousePosition))
                {
                    mouseOverNodeIndex = i;
                    clickedOnNode = true;
                    break;
                }
    }

    private void ResetEventCheckVars()
    {
        //selectedNodeIndex = -1;
        clickedOnNode = false;
    }

    public void DrawNodeWindow(int id)
    {
        selectedScene.nodes[id].DrawNode();
    }

    private void LerpToNode(int index)
    {
        posLerp = true;
        counter = 0f;

        NodeWindow selectedNode = selectedScene.nodes[index];
        
        Vector2 offset = new Vector2(position.width / 2f, position.height / 2f);
        targetPosition = selectedNode.nodeRect.position - offset;

        selectedScene.nodes[selectedNodeIndex].DeselectWindow();
        selectedNodeIndex = index;
        selectedScene.nodes[selectedNodeIndex].SelectWindow();
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
    }
    #endregion
}

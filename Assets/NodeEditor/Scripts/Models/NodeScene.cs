using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Scene", menuName ="Custom/NodeScene", order = 1)]
public class NodeScene : ScriptableObject
{
    #region - Vars and Initialisation 

    //Vars and Props
    public string sceneName = "";
    public List<Node> nodes = new List<Node>();
    public Node selectedNode;

    //Node Events
    public bool nodesShownInSelectionGrid = false;
    public bool clickedOnNode = false;
    public bool isConnectionModeActive = false;
    public bool isNodeDragActive = false;

    //Selected Indexes
    private int selectedNodeIndex = 0;
    public int SelectedNodeIndex { get { return selectedNodeIndex; } set { selectedNodeIndex = value; } }
    private int mouseOverNodeIndex = 0;
    private int lastSelectedNodeIndex = 0;

    private static Color selectedNodeColor = new Color(0.4f, 0.95f, 0.5f);

    public static NodeScene CreateScene(string sceneName)
    {
        var instance = CreateInstance<NodeScene>();
        instance.sceneName = sceneName;
        instance.name = sceneName;
        return instance;
    }

    public List<string> GetNodeNames()
    {
        List<string> nodeNames = new List<string>();
        for (int i = 0; i < nodes.Count; i++)
            nodeNames.Add(nodes[i].nodeName);
        return nodeNames;
    }

    public int LoadSelectedNodeIndex()
    {
        for (int i = 0; i < nodes.Count; i++)
            if (nodes[i].isSelected)
                return i;
        return 0;
    }

    public void SetupScene()
    {
        selectedNodeIndex = LoadSelectedNodeIndex();
        lastSelectedNodeIndex = selectedNodeIndex;
    }

    #endregion

    #region - Creation and Deletion

    public Node CreateNode(Vector2 position)
    {
        Node newNode = Node.CreateInstance(new Rect(position, new Vector2(Node.minWidth, Node.minHeight)));
        newNode.nodeName = (nodes.Count + 1).ToString();
        nodes.Add(newNode);

        AssetDatabase.AddObjectToAsset(newNode, this);
        EditorUtility.SetDirty(this);
        EditorUtility.SetDirty(newNode);
        AssetDatabase.SaveAssets();

        lastSelectedNodeIndex = selectedNodeIndex;
        selectedNodeIndex = nodes.Count - 1;
        nodes[lastSelectedNodeIndex].DeselectWindow();
        nodes[selectedNodeIndex].SelectWindow();
        return newNode;
    }

    public void DeleteNode(int nodeIndex)
    {
        for(int i = 0; i < nodes.Count; i++)
            nodes[i].DeleteConnectionsToNode(nodes[nodeIndex]);

        nodes.RemoveAt(nodeIndex);

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();

        if (selectedNodeIndex != 0)
            selectedNodeIndex--;

        lastSelectedNodeIndex = selectedNodeIndex;
        nodes[selectedNodeIndex].SelectWindow();
    }

    #endregion

    #region - Scene GUI

    public virtual void DrawNodeGUI(Event currentEvent, Vector2 mousePosition)
    {
        ProcessNodeGUIEvents(currentEvent, mousePosition);

        for (int i = 0; i < nodes.Count; i++)
            nodes[i].DrawCurves();

        NodeEditor.window.BeginWindows();
        for (int i = 0; i < nodes.Count; i++)
        {
            GUI.backgroundColor = nodes[i].isSelected ? selectedNodeColor : Color.white;
            nodes[i].nodeRect = GUI.Window(i, nodes[i].nodeRect, DrawNodeWindow, nodes[i].nodeTitle);
        }
        GUI.backgroundColor = Color.white;
        NodeEditor.window.EndWindows();
    }

    private void DrawNodeWindow(int nodeIndex)
    {
        nodes[nodeIndex].DrawNode();
    }

    public virtual void ProcessNodeGUIEvents(Event currentEvent, Vector2 mousePosition)
    {

        if (isConnectionModeActive && selectedNode != null)
        {
            Rect mouseRect = new Rect(NodeEditor.zoomScrollCorrectedMousePosition.x, NodeEditor.zoomScrollCorrectedMousePosition.y, 10, 10);
            EditorGUIStatics.DrawNodeCurve(new Rect(NodeConnectionPoint.GetConnectionPoint(NodeEditor.selectedConnectionType.outputConnectionPoint,
                selectedNode.nodeRect), Vector2.one), mouseRect);
            NodeEditor.window.Repaint();
        }

        //Right Click
        if (currentEvent.button == 1)
        {
            if (currentEvent.type == EventType.MouseDown)
            {
                UpdateNodeGUIEventsStatus(mousePosition);
                //Conext menu for this node
                if (clickedOnNode)
                {
                    lastSelectedNodeIndex = selectedNodeIndex;
                    selectedNodeIndex = mouseOverNodeIndex;
                    nodes[lastSelectedNodeIndex].DeselectWindow();
                    nodes[selectedNodeIndex].SelectWindow();

                    GenericMenu menu = new GenericMenu();

                    for (int i = 0; i < NodeEditor.nodeConnectionTypes.connectionTypes.Count; i++)
                        if (nodes[selectedNodeIndex].MoreConnectionsOfTypeAllowed(NodeEditor.nodeConnectionTypes.connectionTypes[i]))
                            menu.AddItem(new GUIContent("Add Connection/" +
                                                        NodeEditor.nodeConnectionTypes.connectionTypes[i].connectionName), false, NodeGUIEventsContextCallback, "Connection-" +
                                                        NodeEditor.nodeConnectionTypes.connectionTypes[i].connectionName);

                    if (nodes[selectedNodeIndex].hasoutputNodes)
                        menu.AddItem(new GUIContent("Delete All Connections"), false, NodeGUIEventsContextCallback, "Delete Connections");

                    menu.AddSeparator("");

                    menu.AddItem(new GUIContent("Delete Node"), false, NodeGUIEventsContextCallback, "Delete Node");
                    menu.DropDown(new Rect(NodeEditor.zoomScrollCorrectedMenuPosition, Vector2.one));

                    currentEvent.Use();
                }
            }
        }
        //Left Click
        else if (currentEvent.button == 0)
        {
            UpdateNodeGUIEventsStatus(mousePosition);
            if (currentEvent.type == EventType.MouseDown)
            {
                if (isConnectionModeActive)
                {
                    lastSelectedNodeIndex = selectedNodeIndex;
                    selectedNodeIndex = mouseOverNodeIndex;

                    //Add a connection
                    if (clickedOnNode && !nodes[selectedNodeIndex].Equals(selectedNode))
                    {
                        nodes[lastSelectedNodeIndex].DeselectWindow();
                        nodes[selectedNodeIndex].SelectWindow();

                        selectedNode.SetConnection(nodes[selectedNodeIndex], NodeEditor.selectedConnectionType);
                        isConnectionModeActive = false;
                        selectedNode = null;
                    }
                    //Stop connection mode
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
                        //Select node and allow for drag to occur
                        if (!isNodeDragActive)
                        {
                            lastSelectedNodeIndex = selectedNodeIndex;
                            selectedNodeIndex = mouseOverNodeIndex;
                            nodes[lastSelectedNodeIndex].DeselectWindow();
                            nodes[selectedNodeIndex].SelectWindow();

                            isNodeDragActive = true;
                        }
                    }
                }
            }
            else if (currentEvent.type == EventType.MouseUp || currentEvent.type == EventType.DragExited)
            {
                if (isNodeDragActive)
                    isNodeDragActive = false;
            }
        }

        ResetNodeGUIEventsStatus();
    }

    private void UpdateNodeGUIEventsStatus(Vector2 mousePosToCheck)
    {
        for (int i = 0; i < nodes.Count; i++)
            if (nodes[i].nodeRect.Contains(mousePosToCheck))
            {
                mouseOverNodeIndex = i;
                clickedOnNode = true;
                break;
            }
    }

    private void ResetNodeGUIEventsStatus()
    {
        clickedOnNode = false;
    }

    private void NodeGUIEventsContextCallback(object obj)
    {
        string[] splitString = obj.ToString().Split('-');
        string callback = splitString[0];
        UpdateNodeGUIEventsStatus(NodeEditor.zoomScrollCorrectedMenuPosition);
        switch (callback)
        {
            case "Delete Node":
                DeleteNode(selectedNodeIndex);
                //NodeEditor.window.Repaint();
                break;
            case "Connection":
                for (int i = 0; i < NodeEditor.nodeConnectionTypes.connectionTypes.Count; i++)
                    if (splitString[1] == NodeEditor.nodeConnectionTypes.connectionTypes[i].connectionName)
                        NodeEditor.selectedConnectionType = NodeEditor.nodeConnectionTypes.connectionTypes[i];

                selectedNode = nodes[selectedNodeIndex];
                isConnectionModeActive = true;
                break;
            case "Delete Connections":
                nodes[selectedNodeIndex].ClearConnections();
                break;
        }
    }

    #endregion
}
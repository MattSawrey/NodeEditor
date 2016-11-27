using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "this", menuName ="Custom/This", order = 1)]
public class NodeScene : ScriptableObject
{
    public string sceneName = "";
    public List<NodeWindow> nodes = new List<NodeWindow>();

    public static NodeScene CreateScene(string sceneName)
    {
        var instance = CreateInstance<NodeScene>();
        instance.sceneName = sceneName;
        instance.name = sceneName;
        return instance;
    }

    public void DeleteNode()
    {

    }

    public int GetSelectedNodeIndex()
    {
        for (int i = 0; i < nodes.Count; i++)
            if (nodes[i].isSelected)
                return i;

        //If no node is currently selected
        return 0;
    }
}
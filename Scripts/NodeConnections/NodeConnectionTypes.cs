using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Asset that contains the list pf possible connection types
/// </summary>
public class NodeConnectionTypes : ScriptableObject
{
    public const string assetFilePath = "Assets/NodeEditor/Assets/NodeConnectionTypes.asset";
    public List<NodeConnectionType> connectionTypes;

    [MenuItem("Assets/Create/Custom/NodeEditor/NodeConnectionTypes")]
    public static void CreateNodeConnectionTypesAsset()
    {
        var existingAsset = AssetDatabase.LoadAssetAtPath<NodeConnectionTypes>(assetFilePath);

        if (existingAsset == null)
        {
            Debug.Log(existingAsset);
            var instance = CreateInstance<NodeConnectionTypes>();
            AssetDatabase.CreateAsset(instance, assetFilePath);
        }
        else
            Debug.Log("Asset already exists you sausage!");
    }
}

using UnityEngine;
using UnityEditor;
using System;

[Serializable]
public class NodeConnection
{
    public Node outputNode;
    public Node inputNode;
    public NodeConnectionType connectionType;

    private Vector2 outputPoint;
    private Vector2 inputPoint;
    private Vector2 midPoint;

    public NodeConnection(Node outputNode, Node inputNode, NodeConnectionType connectionType)
    {
        this.outputNode = outputNode;
        this.inputNode = inputNode;
        this.connectionType = connectionType;
    }

    public void DrawConnection()
    {
        //Calc positions
        outputPoint = NodeConnectionPoint.GetConnectionPoint(connectionType.outputConnectionPoint, outputNode.nodeRect);
        inputPoint = NodeConnectionPoint.GetConnectionPoint(connectionType.inputConnectionPoint, inputNode.nodeRect);
        midPoint = Vector2.Lerp(outputPoint, inputPoint, 0.5f);

        //Draw the curve
        switch (connectionType.lineType)
        {
            case NodeConnectionLineType.Straight:
                EditorGUIStatics.DrawNodeLine(new Rect(outputPoint, Vector2.one),
                    new Rect(inputPoint, Vector2.one), connectionType.connectionColor);
                break;
            case NodeConnectionLineType.Curved:
                EditorGUIStatics.DrawNodeCurve(new Rect(outputPoint, Vector2.one),
                    new Rect(inputPoint, Vector2.one), connectionType.connectionColor, connectionType.curveStrength);
                break;
            default:
                EditorGUIStatics.DrawNodeCurve(new Rect(outputPoint, Vector2.one),
                    new Rect(inputPoint, Vector2.one), connectionType.connectionColor, 0.5f);
                break;
        }

        //Draw Connection Points
        Handles.color = Color.gray;
        Handles.DrawSolidDisc(outputPoint, Vector3.forward, 6f);
        Handles.DrawSolidDisc(inputPoint, Vector3.forward, 6f);
        Handles.color = Color.white;

        //Draw the mid-connection handle button
        Handles.color = new Color(1f, 1f, 1f, 0.4f);
        Handles.DrawSolidDisc(midPoint - new Vector2(10f, -4f), -Vector3.forward, 8f);
        GUI.contentColor = Color.black;
        GUI.Label(new Rect(midPoint - new Vector2(16.3f, 4.2f), new Vector2(80f, 80f)), "+");
        if (Handles.Button(midPoint - new Vector2(10f, -4f), Quaternion.identity, 8f, 8f, Handles.CircleCap))
        {
            NodeEditor.window.ReAssignConnection(connectionType, outputNode);
            outputNode.nodeConnections.Remove(this);
        }

        //Connection Name Label
        GUI.contentColor = Color.white;
        Rect midPointRect = new Rect((outputPoint + inputPoint) / 2f, new Vector2(100f, 40f));
        GUI.skin.label.fontStyle = FontStyle.Bold;
        GUI.Label(midPointRect, connectionType.connectionName);
        GUI.skin.label.fontStyle = FontStyle.Normal;
    }
}
using UnityEngine;
using System;

[Serializable]
public class NodeConnectionType
{
    public string connectionName;
    public int maxNumConnections;
    public NodeConnectionLineType lineType;
    [Range(0f, 1f)]
    public float curveStrength;
    public NodeConnectionPointsStandard outputConnectionPoint;
    public NodeConnectionPointsStandard inputConnectionPoint;
    public Color connectionColor;
}

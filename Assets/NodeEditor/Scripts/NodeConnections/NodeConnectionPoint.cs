using UnityEngine;

public static class NodeConnectionPoint
{
    public static Vector2 GetConnectionPoint(NodeConnectionPointsStandard connectionPos, Rect WindowRect)
    {
        switch (connectionPos)
        {
            case NodeConnectionPointsStandard.LeftMid:
                return new Vector2(WindowRect.x - 1f, WindowRect.y + (WindowRect.height / 2f));
            case NodeConnectionPointsStandard.LeftLowerMid:
                return new Vector2(WindowRect.x - 1f, WindowRect.y + (WindowRect.height / 1.4f));
            case NodeConnectionPointsStandard.LeftUpperMid:
                return new Vector2(WindowRect.x - 1f, WindowRect.y + (WindowRect.height / 3.2f));
            case NodeConnectionPointsStandard.RightMid:
                return new Vector2(WindowRect.x + WindowRect.width + 1f, WindowRect.y + (WindowRect.height / 2f));
            case NodeConnectionPointsStandard.RightLowerMid:
                return new Vector2(WindowRect.x + WindowRect.width + 1f, WindowRect.y + (WindowRect.height / 1.4f));
            case NodeConnectionPointsStandard.RightUpperMid:
                return new Vector2(WindowRect.x + WindowRect.width + 1f, WindowRect.y + (WindowRect.height / 3.2f));
            case NodeConnectionPointsStandard.BottomMid:
                return new Vector2(WindowRect.x + (WindowRect.width/2f), WindowRect.y + WindowRect.height);
            case NodeConnectionPointsStandard.TopMid:
                return new Vector2(WindowRect.x + (WindowRect.width / 2f), WindowRect.y - 1f);
            default:
                //Default returns right mid.
                return new Vector2(WindowRect.x + WindowRect.width + 1f, WindowRect.y + (WindowRect.height / 2f));
        }
    }
}
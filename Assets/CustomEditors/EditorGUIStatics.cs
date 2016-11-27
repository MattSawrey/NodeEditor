using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class EditorGUIStatics
{
    public static string AddHorizontalSeperationLine()
    {
        return EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
    }

    public static void AddVerticalSeperatorLine()
    {
        GUI.color = Color.gray;
        EditorGUILayout.LabelField("|", EditorStyles.boldLabel, GUILayout.MaxWidth(10f), GUILayout.MaxHeight(4f));
        GUI.color = Color.white;
    }

    public static void DrawLine(Vector2 startPos, Vector2 endPos, Color colour, float thickness)
    {
        Handles.color = colour;
        Handles.DrawAAPolyLine(thickness, new Vector3[] { startPos, endPos });
        Handles.color = Color.white;
    }
 
    public static void DrawBackgroundGrid(Rect scrollViewRect, Vector2 scrollPos, float gridSquareWidth, Color lineColour, float lineThickness)
    { DrawBackgroundGrid(scrollViewRect, scrollPos, gridSquareWidth, lineColour, lineThickness/2.5f, 1); }

    public static void DrawBackgroundGrid(Rect scrollViewRect, Vector2 scrollPos, float gridSquareWidth, Color lineColour, float lineThickness, int thickerLineInterval)
    {
        if (Event.current.type == EventType.Repaint)
        {
            Vector2 offset = new Vector2(Mathf.Abs(scrollPos.x % gridSquareWidth - gridSquareWidth),
                                            Mathf.Abs(scrollPos.y % gridSquareWidth - gridSquareWidth));

            int numXLines = Mathf.CeilToInt((scrollViewRect.width + (gridSquareWidth - offset.x)) / gridSquareWidth);
            int numYLines = Mathf.CeilToInt((scrollViewRect.height + (gridSquareWidth - offset.y)) / gridSquareWidth);

            for (int x = 0; x < numXLines; x++)
            {
                float lineThicknessToUse = x % thickerLineInterval == 0 ? lineThickness * 2.5f : lineThickness;
                DrawLine(new Vector2(offset.x + (x * gridSquareWidth) + scrollViewRect.x, 0f) + scrollPos,
                    new Vector2(offset.x + (x * gridSquareWidth) + scrollViewRect.x, scrollViewRect.height) + scrollPos, lineColour, lineThicknessToUse);
            }

            for (int y = 0; y < numYLines; y++)
            {
                float lineThicknessToUse = y % thickerLineInterval == 0 ? lineThickness * 2.5f : lineThickness;
                DrawLine(new Vector2(scrollViewRect.x, offset.y + (y * gridSquareWidth) + scrollViewRect.y) + scrollPos,
                    new Vector2(scrollViewRect.x + scrollViewRect.width, offset.y + (y * gridSquareWidth) + scrollViewRect.y) + scrollPos, lineColour, lineThicknessToUse);
            }
        }
    }

    public static void DrawNodeCurve(Rect start, Rect end)
    {
        DrawNodeCurve(start, end, Color.black, 0.5f);
    }

    public static void DrawNodeCurve(Rect start, Rect end, Color color, float curveStrength)
    {
        Vector3 startPos = new Vector3(start.x + start.width / 2, start.y + start.height / 2, 0);
        Vector3 endPos = new Vector3(end.x + end.width / 2, end.y + end.height / 2, 0);
        Vector3 startTan = startPos + Vector3.right * (curveStrength*100);
        Vector3 endTan = endPos + Vector3.left * (curveStrength * 100);
        Color shadowCol = new Color(0, 0, 0, .1f);

        for (int i = 0; i < 3; i++)
        {
            Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * 5);
        }
        Handles.color = color;
        Handles.DrawBezier(startPos, endPos, startTan, endTan, color, null, 2);
        Handles.color = Color.white;
    }

    public static void DrawNodeLine(Rect start, Rect end, Color color)
    {
        Vector3 startPos = new Vector3(start.x + start.width / 2, start.y + start.height / 2, 0);
        Vector3 endPos = new Vector3(end.x + end.width / 2, end.y + end.height / 2, 0);

        Handles.color = color;
        Handles.DrawLine(startPos, endPos);
        Handles.color = Color.white;
    }
}

/// <summary>
/// A simple class providing static access to functions that will provide a 
/// zoomable area similar to Unity's built in BeginVertical and BeginArea
/// Systems. Code based off of article found at:
/// http://martinecker.com/martincodes/unity-editor-window-zooming/
///  
/// (Site may be down)
/// </summary>
public class EditorZoomArea
{
    private static Stack<Matrix4x4> previousMatrices = new Stack<Matrix4x4>();
    public static Rect Begin(float zoomScale, Rect screenCoordsArea)
    {
        GUI.EndGroup();

        Rect clippedArea = screenCoordsArea.ScaleSizeBy(1.0f / zoomScale, screenCoordsArea.min);
        //clippedArea.y += 21f;

        //Handles.DrawSolidRectangleWithOutline(clippedArea, Color.clear, Color.yellow);

        GUI.BeginGroup(new Rect(0f, 21f / zoomScale, clippedArea.width + clippedArea.x, clippedArea.height + clippedArea.y));

        previousMatrices.Push(GUI.matrix);
        Matrix4x4 translation = Matrix4x4.TRS(screenCoordsArea.min, Quaternion.identity, Vector3.one);
        Matrix4x4 scale = Matrix4x4.Scale(new Vector3(zoomScale, zoomScale, 1.0f));
        GUI.matrix = translation * scale * translation.inverse;

        return clippedArea;
    }

    /// <summary>
    /// Ends the zoom area
    /// </summary>
    public static void End()
    {
        GUI.matrix = previousMatrices.Pop();
        GUI.EndGroup();
        GUI.BeginGroup(new Rect(0, 21, Screen.width, Screen.height));
    }
}
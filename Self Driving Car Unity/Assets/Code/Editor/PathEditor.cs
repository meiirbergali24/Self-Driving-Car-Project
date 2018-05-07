using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Rasul.Bezier;

[CustomEditor(typeof(PathCreater))]
public class PathEditor : Editor
{
    private PathCreater creater;
    private Path Path
    {
        get
        {
            return creater.path;
        }
    }

    private const float segmentSelectDistanceTreshold = .1f;
    private int selectedSegmentIndex = -1;
    private bool isEditing;

    private void OnEnable()
    {
        creater = (PathCreater)target;
        if (creater.path == null)
        {
            creater.CreatePath();
        }
    }

    private void Input()
    {
        Event current = Event.current;
        Ray screenRay = HandleUtility.GUIPointToWorldRay(current.mousePosition);
        Vector3 mouse = screenRay.direction * ( (creater.transform.position.y - screenRay.origin.y)/screenRay.direction.y ) + screenRay.origin;
        if (current.type == EventType.MouseDown && current.button == 0 && current.shift)
        {
            if (selectedSegmentIndex != -1)
            {
                Undo.RecordObject(creater, $"Split Segment {selectedSegmentIndex}");
                Path.SplitSegment(mouse, selectedSegmentIndex);
            }
            else if (!Path.IsClosed)
            {
                Undo.RecordObject(creater, "Add segment");
                Debug.DrawRay(screenRay.origin,mouse - screenRay.origin, Color.black, 5f);
                Path.AddSegment(mouse);
            }
        }
        if (current.type == EventType.MouseDown && current.button == 1 && current.shift)
        {
            float minDistToAnchor = creater.anchorDiameter * 0.5f;
            int closestAnchorIndex = -1;

            for (int i = 0; i < Path.NumberOfPoints; i += 3)
            {
                float distance = Vector3.Distance(mouse, Path[i]);
                if (distance < minDistToAnchor)
                {
                    minDistToAnchor = distance;
                    closestAnchorIndex = i;
                }
            }

            if (closestAnchorIndex != -1)
            {
                Debug.Log("Deletion");
                Undo.RecordObject(creater, "Delete Segment");
                Path.RemoveSegment(closestAnchorIndex);
            }
        }

        if (current.type == EventType.MouseMove)
        {
            float minDistanceToSegment = segmentSelectDistanceTreshold;
            int newSelectedSegmentIndex = -1;

            float distance;
            for (int i = 0; i < Path.NumberOfSegments; i++)
            {
                Vector3[] points = Path.GetPointsInSegment(i, false);
                distance = HandleUtility.DistancePointBezier(mouse, points[0], points[3], points[1], points[2]);
                if (distance < minDistanceToSegment)
                {
                    minDistanceToSegment = distance;
                    newSelectedSegmentIndex = i;
                }
            }
            if (newSelectedSegmentIndex != selectedSegmentIndex)
            {
                selectedSegmentIndex = newSelectedSegmentIndex;
                HandleUtility.Repaint();
            }
        }

        HandleUtility.AddDefaultControl(0); // prevent miss selection
    }

    private void Draw()
    {
        Vector3[] points;
        for (int i = 0; i < Path.NumberOfSegments; i++)
        {
            points = Path.GetPointsInSegment(i, false);
            if (creater.displayControls)
            {
                Handles.color = Color.black;
                Handles.DrawLine(points[1], points[0]);
                Handles.DrawLine(points[2], points[3]);
            }
            Color segmentColor = i == selectedSegmentIndex && Event.current.shift ? creater.selectedSegmentColor : creater.segmentColor;
            Handles.DrawBezier(points[0], points[3], points[1], points[2], segmentColor, null, 2);
        }

        Vector3 position;
        for (int i = 0; i < Path.NumberOfPoints; i++)
        {
            if (i % 3 == 0 || creater.displayControls)
            {
                Handles.color = i % 3 == 0 ? (i == 0 || i == Path.NumberOfPoints - 1 ? creater.initialColor : creater.anchorColor) : creater.controlColor;
                position = Handles.FreeMoveHandle(Path[i], Quaternion.identity, 
                    i % 3 == 0 ? creater.anchorDiameter : creater.controlDiameter , Vector3.one, Handles.SphereHandleCap);
                if (Path[i] != position)
                {
                    Undo.RecordObject(creater, "Move point");
                    Path.MovePoint(i, position);
                }
            }
        }
    }

    public void OnSceneGUI()
    {
        if (isEditing)
        {
            Input();
            Draw();
        }
    }
    
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button($"Edit Mode: {isEditing}"))
        {
            isEditing = !isEditing;
        }

        if (!isEditing)
        {
            DrawPropertiesExcluding(serializedObject,
                "m_Script",
                nameof(creater.initialColor),
                nameof(creater.anchorColor),
                nameof(creater.controlColor),
                nameof(creater.controlDiameter),
                nameof(creater.anchorColor),
                nameof(creater.displayControls),
                nameof(creater.segmentColor),
                nameof(creater.selectedSegmentColor),
                nameof(creater.anchorDiameter)
                );
        }
        else
        {
            DrawPropertiesExcluding(serializedObject, "m_Script");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button($"Flatness : {Path.isFlat}"))
            {
                Path.isFlat = !Path.isFlat;
            }
            if (GUILayout.Button("Normalize height"))
            {
                Undo.RecordObject(creater, "Normalize height");
                Vector3 normalized;
                for (int i = 0; i < Path.NumberOfPoints; i++)
                {
                    normalized = Path[i];
                    normalized.y = 0;
                    Path.MovePoint(i, normalized);
                }
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(2f);
        }

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button($"{ (Path.IsClosed ? "Disconnect" : "Connect") }"))
            {
                Undo.RecordObject(creater, $"Is Closed: {!Path.IsClosed}");
                Path.IsClosed = !Path.IsClosed;
            }
            if (GUILayout.Button($"Auto Control: {Path.AutoSetControls}"))
            {
                Undo.RecordObject(creater, $"Auto Control: {Path.AutoSetControls}");
                Path.AutoSetControls = !Path.AutoSetControls;
            }
        }
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("Create new"))
        {
            Undo.RecordObject(creater, "Create New");
            creater.CreatePath();
        }
        if (EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll();
        }

        serializedObject.ApplyModifiedProperties();
    }
}

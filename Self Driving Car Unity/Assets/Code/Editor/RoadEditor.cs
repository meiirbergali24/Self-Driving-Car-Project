using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Rasul.Bezier;

[CustomEditor(typeof(RoadCreater))]
public class RoadEditor : Editor
{
    private RoadCreater creater;

    private void OnEnable()
    {
        creater = (RoadCreater)target;
    }

    private void OnSceneGUI()
    {
        if (creater.autoUpdate && Event.current.type == EventType.Repaint)
        {
            creater.UpdateRoad();
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Update"))
        {
            creater.UpdateRoad();
        }
    }

}

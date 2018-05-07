using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rasul.Bezier
{
    public class PathCreater : MonoBehaviour
    {
        [HideInInspector]
        public Path path;

        public Color
            initialColor = Color.blue,
            anchorColor = Color.red,
            controlColor = Color.white,
            segmentColor = Color.green,
            selectedSegmentColor = Color.yellow;

        public float
            anchorDiameter = 0.1f,
            controlDiameter = 0.1f;

        public bool displayControls = true;

        public void CreatePath()
        {
            path = new Path(transform);
        }

        public void Reset()
        {
            CreatePath();
        }
    }
}
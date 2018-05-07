using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rasul.Bezier
{
    public class PathView : MonoBehaviour
    {
        public float spacing = 0.1f;
        public float resolution = 1f;

        private void Start()
        {
            Vector3[] points = FindObjectOfType<PathCreater>().path.GetEvenlySpacedPoints(spacing, resolution);
            foreach (var p in points)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.transform.position = p;
                go.transform.parent = transform;
                go.transform.localScale = Vector3.one * spacing * 0.5f;
            }
        }
    }
}
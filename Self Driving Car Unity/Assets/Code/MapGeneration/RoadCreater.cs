using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rasul.Bezier
{
    [RequireComponent(
        typeof(PathCreater),
        typeof(MeshFilter),
        typeof(MeshRenderer))]
    public class RoadCreater : MonoBehaviour
    {
        [Range(.05f, 1.5f)]
        public float spacing = 1f;
        public float roadWidth = 1;
        public float tiling = 1f;
        public bool autoUpdate;

        public void UpdateRoad()
        {
            Path path = GetComponent<PathCreater>().path;
            Vector3[] points = path.GetEvenlySpacedPoints(spacing);
            GetComponent<MeshFilter>().mesh = CreateMesh(points, path.IsClosed);

            int textureYScale = Mathf.RoundToInt(tiling * points.Length * spacing * 0.05f);
            GetComponent<MeshRenderer>().sharedMaterial.mainTextureScale = new Vector2(1, textureYScale);
        }

        private Mesh CreateMesh(Vector3[] points, bool isClosed)
        {
            Vector3[] vertices = new Vector3[points.Length * 2];
            Vector2[] uv = new Vector2[vertices.Length];
            int trianglesCount = 2 * (points.Length - 1) + (isClosed ? 2 : 0);
            int[] triangles = new int[trianglesCount * 3];
            int vertexIndex = 0;
            int triangleIndex = 0;

            Vector3 forward;
            float height = points[0].y;
            for (int i = 0; i < points.Length; i++)
            {
                forward = Vector3.zero;
                if (i < points.Length - 1 || isClosed)
                {
                    forward += points[(i + 1) % points.Length] - points[i];
                }
                if (i > 0 || isClosed)
                {
                    forward += points[i] - points[(i - 1 + points.Length) % points.Length];
                }
                forward.y = height;
                forward.Normalize();
                Vector3 left = new Vector3(-forward.z, height, forward.x);
                vertices[vertexIndex] = points[i] + left * roadWidth * 0.5f;
                vertices[vertexIndex + 1] = points[i] - left * roadWidth * 0.5f;

                float completionPercent = i / (float)(points.Length - 1);
                float clampedComletionPrecent = 1 - Mathf.Abs((2 * completionPercent) - 1);
                uv[vertexIndex] = new Vector2(0, clampedComletionPrecent);
                uv[vertexIndex + 1] = new Vector2(1, clampedComletionPrecent);

                if (i < points.Length - 1 || isClosed)
                {
                    triangles[triangleIndex] = vertexIndex;
                    triangles[triangleIndex + 1] = (vertexIndex + 2) % vertices.Length;
                    triangles[triangleIndex + 2] = vertexIndex + 1;

                    triangles[triangleIndex + 3] = vertexIndex + 1;
                    triangles[triangleIndex + 4] = (vertexIndex + 2) % vertices.Length;
                    triangles[triangleIndex + 5] = (vertexIndex + 3) % vertices.Length;
                }

                vertexIndex += 2;
                triangleIndex += 6;
            }

            return new Mesh
            {
                vertices = vertices,
                triangles = triangles,
                uv = uv,
            };
        }

    }
}
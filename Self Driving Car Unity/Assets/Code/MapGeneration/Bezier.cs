using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rasul.Bezier
{
    public static class Bezier
    {
        public static Vector3 EvaluateQuadratic(Vector3 a, Vector3 b, Vector3 c, float t)
        {
            Vector3 point1 = Vector3.Lerp(a, b, t);
            Vector3 point2 = Vector3.Lerp(b, c, t);
            return Vector3.Lerp(point1, point2, t);
        }

        public static Vector3 EvaluateCubic(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
        {
            Vector3 point1 = EvaluateQuadratic(a, b, c, t);
            Vector3 point2 = EvaluateQuadratic(b, c, d, t);
            return Vector3.Lerp(point1, point2, t);
        }


    }
}
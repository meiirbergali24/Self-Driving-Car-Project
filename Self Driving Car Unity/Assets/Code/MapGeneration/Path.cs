using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rasul.Bezier
{
    [System.Serializable]
    public class Path
    {
        //members
        [SerializeField, HideInInspector]
        private List<Vector3> points;
        [SerializeField]
        private Transform pivot;
        [SerializeField, HideInInspector]
        private bool isClosed;
        [SerializeField, HideInInspector]
        public bool isFlat;

        public bool IsClosed
        {
            get
            {
                return isClosed;
            }
            set
            {
                if (isClosed != value)
                {
                    isClosed = value;
                    if (isClosed)
                    {
                        points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
                        points.Add(points[0] * 2 - points[1]);
                        if (autoSetControls)
                        {
                            AutoSetAnchorControls(0);
                            AutoSetAnchorControls(points.Count - 1);
                        }
                    }
                    else
                    {
                        points.RemoveRange(points.Count - 2, 2);
                        if (autoSetControls)
                        {
                            AutoSetStartAndEndControls();
                        }
                    }
                }
            }
        }
        [SerializeField, HideInInspector]
        private bool autoSetControls;
        public bool AutoSetControls
        {
            get
            {
                return autoSetControls;
            }
            set
            {
                if (autoSetControls != value)
                {
                    autoSetControls = value;
                    if (autoSetControls)
                    {
                        AutoSetAllControls();
                    }
                }
            }
        }

        //properties
        public int NumberOfPoints
        {
            get
            {
                return points.Count;
            }
        }

        public int NumberOfSegments
        {
            get
            {
                return points.Count / 3;
            }
        }

        //constructor
        public Path(Transform pivot)
        {
            this.pivot = pivot;
            points = new List<Vector3>()
        {
            -Vector3.right,
            new Vector3(-0.5f, 0f, 0.5f),
            new Vector3(0.5f, 0f, -0.5f),
            Vector3.right
        };
        }

        public Vector3 this[int index]
        {
            get
            {
                return points[index] + pivot.position;
            }
        }

        //functionality
        public void AddSegment(Vector3 anchor)
        {
            anchor -= pivot.position;

            points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
            points.Add((points[points.Count - 1] + anchor) * 0.5f);
            points.Add(anchor);

            if (autoSetControls)
            {
                AutoSetAllAffectedControlPoints(points.Count - 1);
            }
        }

        public void SplitSegment(Vector3 anchor, int segmentIndex)
        {
            anchor -= pivot.position;

            points.InsertRange(segmentIndex * 3 + 2, new Vector3[] { Vector3.zero, anchor, Vector3.zero });
            if (autoSetControls)
            {
                AutoSetAllAffectedControlPoints(segmentIndex * 3 + 3);
            }
            else
            {
                AutoSetAnchorControls(segmentIndex * 3 + 3);
            }
        }

        public void RemoveSegment(int index)
        {
            if (NumberOfSegments > 2 || isClosed && NumberOfSegments > 1)
            {
                if (index == 0)
                {
                    if (isClosed)
                    {
                        points[points.Count - 1] = points[2];
                    }
                    points.RemoveRange(0, 3);
                }
                else if (index == points.Count - 1 && !isClosed)
                {
                    points.RemoveRange(index - 2, 3);
                }
                else
                {
                    points.RemoveRange(index - 1, 3);
                }
            }
        }

        public Vector3[] GetPointsInSegment(int index, bool localSpace)
        {
            if (localSpace)
            {
                return new Vector3[]
                {
                points[index * 3]               ,
                points[index * 3 + 1]           ,
                points[index * 3 + 2]           ,
                points[LoopIndex(index * 3 + 3)]
                };
            }
            else
            {
                return new Vector3[]
                {
                points[index * 3]                + pivot.position,
                points[index * 3 + 1]            + pivot.position,
                points[index * 3 + 2]            + pivot.position,
                points[LoopIndex(index * 3 + 3)] + pivot.position
                };
            }
        }

        public void MovePoint(int index, Vector3 position)
        {
            position -= pivot.position;
            if (isFlat)
            {
                position.y = 0;
            }

            Vector3 movement = position - points[index];
            if (index % 3 == 0 || !autoSetControls)
            {
                points[index] = position;

                if (autoSetControls)
                {
                    AutoSetAllAffectedControlPoints(index);
                }
                else
                {
                    if (index % 3 == 0) // it is anchor point
                    {
                        if (index + 1 < points.Count || isClosed)
                        {
                            points[LoopIndex(index + 1)] += movement;
                        }
                        if (index - 1 >= 0 || isClosed)
                        {
                            points[LoopIndex(index - 1)] += movement;
                        }
                    }
                    else
                    {
                        bool nextPointIsAnchor = (index + 1) % 3 == 0;
                        int correspondingControlIndex, anchorIndex;

                        if (nextPointIsAnchor)
                        {
                            correspondingControlIndex = index + 2;
                            anchorIndex = index + 1;
                        }
                        else
                        {
                            correspondingControlIndex = index - 2;
                            anchorIndex = index - 1;
                        }

                        if (correspondingControlIndex >= 0 && correspondingControlIndex < points.Count || isClosed)
                        {
                            anchorIndex = LoopIndex(anchorIndex);
                            correspondingControlIndex = LoopIndex(correspondingControlIndex);

                            float distance = (points[anchorIndex] - points[correspondingControlIndex]).magnitude;
                            Vector3 direction = (points[anchorIndex] - position).normalized;
                            points[correspondingControlIndex] = points[anchorIndex] + direction * distance;
                        }
                    }
                }
            }
        }

        public Vector3[] GetEvenlySpacedPoints(float spacing, float resolution = 1)
        {
            List<Vector3> evenlySpacedPoints = new List<Vector3>
        {
            points[0]
        };
            Vector3 previousPoint = points[0];
            float lastDistance = 0f;

            for (int segmentIndex = 0; segmentIndex < NumberOfSegments; segmentIndex++)
            {
                Vector3[] points = GetPointsInSegment(segmentIndex, true);
                float controlNetLength = Vector3.Distance(points[0], points[1]) + Vector3.Distance(points[1], points[2]) + Vector3.Distance(points[2], points[3]);
                float esstimatedCurveLength = Vector3.Distance(points[0], points[3]) + controlNetLength / 2f;
                int divisions = Mathf.CeilToInt(esstimatedCurveLength * resolution * 10);
                float t = 0;
                while (t <= 1)
                {
                    t += 1f / divisions;
                    Vector3 pointOnCurve = Bezier.EvaluateCubic(points[0], points[1], points[2], points[3], t);
                    lastDistance += Vector3.Distance(previousPoint, pointOnCurve);

                    while (lastDistance >= spacing)
                    {
                        float overshoot = lastDistance - spacing;
                        Vector3 evenlySpacePoint = pointOnCurve + (previousPoint - pointOnCurve).normalized * overshoot;
                        evenlySpacedPoints.Add(evenlySpacePoint);
                        lastDistance = overshoot;
                        previousPoint = evenlySpacePoint;
                    }
                    previousPoint = pointOnCurve;
                }
            }

            return evenlySpacedPoints.ToArray();
        }

        private void AutoSetAllAffectedControlPoints(int anchorIndex)
        {
            for (int i = anchorIndex - 3; i <= anchorIndex + 3; i += 3)
            {
                if (i >= 0 && i < points.Count || IsClosed)
                {
                    AutoSetAnchorControls(LoopIndex(i));
                }
            }

            AutoSetStartAndEndControls();
        }

        private void AutoSetAllControls()
        {
            for (int i = 0; i < points.Count; i += 3)
            {
                AutoSetAnchorControls(i);
            }
            AutoSetStartAndEndControls();
        }

        private void AutoSetAnchorControls(int anchorIndex)
        {
            Vector3 anchor = points[anchorIndex];
            Vector3 direction = Vector3.zero;
            float[] neighbourDistances = new float[2];

            if (anchorIndex - 3 >= 0 || IsClosed)
            {
                Vector3 offset = points[LoopIndex(anchorIndex - 3)] - anchor;
                direction += offset.normalized;
                neighbourDistances[0] = offset.magnitude;
            }
            if (anchorIndex + 3 >= 0 || IsClosed)
            {
                Vector3 offset = points[LoopIndex(anchorIndex + 3)] - anchor;
                direction -= offset.normalized;
                neighbourDistances[1] = -offset.magnitude;
            }

            direction.Normalize();

            int controlIndex;
            for (int i = 0; i < 2; i++)
            {
                controlIndex = anchorIndex + i * 2 - 1;
                if (controlIndex >= 0 && controlIndex < points.Count || isClosed)
                {
                    points[LoopIndex(controlIndex)] = anchor + direction * neighbourDistances[i] * 0.5f;
                }
            }
        }

        private void AutoSetStartAndEndControls()
        {
            if (!isClosed)
            {
                points[1] = (points[0] + points[2]) * 0.5f;
                points[points.Count - 2] = (points[points.Count - 1] + points[points.Count - 3]) * 0.5f;
            }
        }

        private int LoopIndex(int index)
        {
            return (index + points.Count) % points.Count;
        }
    }
}
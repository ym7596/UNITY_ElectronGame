using System.Collections.Generic;
using UnityEngine;

namespace Internal.Scripts.Core.ElecSystem
{
    [RequireComponent(typeof(LineRenderer))]
    public class Wire : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private int pointsPerSegment = 10;

        private List<Vector3> points = new List<Vector3>();

        private void Awake()
        {
            if (lineRenderer == null)
                lineRenderer = GetComponent<LineRenderer>();
            
            // Default settings for wire-like appearance
            lineRenderer.startWidth = 0.2f;
            lineRenderer.endWidth = 0.2f;
            lineRenderer.useWorldSpace = true;
            lineRenderer.textureMode = LineTextureMode.Tile;
        }

        public void SetPoints(List<Vector3> newPoints)
        {
            points = newPoints;
            UpdateLine();
        }

        private void UpdateLine()
        {
            if (points == null || points.Count < 2)
            {
                lineRenderer.positionCount = 0;
                return;
            }

            List<Vector3> smoothedPoints = GenerateSmoothedPoints(points);
            lineRenderer.positionCount = smoothedPoints.Count;
            lineRenderer.SetPositions(smoothedPoints.ToArray());
        }

        private List<Vector3> GenerateSmoothedPoints(List<Vector3> inputPoints)
        {
            if (inputPoints.Count < 3) return inputPoints;

            List<Vector3> result = new List<Vector3>();

            // For each segment, use Catmull-Rom
            for (int i = 0; i < inputPoints.Count - 1; i++)
            {
                Vector3 p0 = i == 0 ? inputPoints[i] - (inputPoints[i+1] - inputPoints[i]) : inputPoints[i - 1];
                Vector3 p1 = inputPoints[i];
                Vector3 p2 = inputPoints[i + 1];
                Vector3 p3 = i + 2 < inputPoints.Count ? inputPoints[i + 2] : inputPoints[i + 1] + (inputPoints[i + 1] - inputPoints[i]);

                for (int j = 0; j < pointsPerSegment; j++)
                {
                    float t = j / (float)pointsPerSegment;
                    result.Add(CalculateCatmullRom(p0, p1, p2, p3, t));
                }
            }

            result.Add(inputPoints[inputPoints.Count - 1]);
            return result;
        }

        private Vector3 CalculateCatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;

            return 0.5f * (
                (2f * p1) +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3
            );
        }
    }
}

//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jannik Boysen
//

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OTBT.Framework.Utils
{
    /// <summary>
    /// Gizmo drawing methods that are missing in Unity's own implementations, such as circles or polygon lines.
    /// </summary>
    public static class GizmoUtility
    {
        const float k_Tau = Mathf.PI * 2f;
        const int k_Resolution = 32;

        /// <summary>
        /// Draw a circle.
        /// </summary>
        /// <param name="center">Center of the circle.</param>
        /// <param name="normal">The plane to draw the circle on.</param>
        /// <param name="radius">Size of the circle.</param>
        /// <param name="resolution">How many points should be used to approximate the circle? (optional)</param>
        public static void DrawCircle(Vector3 center, Vector3 normal, float radius, int resolution = k_Resolution)
        {
            resolution = Mathf.Clamp(resolution, 1, int.MaxValue);

            float stepAngle = k_Tau / resolution;

            var points = new Vector3[resolution];
            for (int i = 0; i < points.Length; i++)
            {
                float sin = Mathf.Sin(stepAngle * i);
                float cos = Mathf.Cos(stepAngle * i);

                points[i] = new Vector3(cos * radius, 0, sin * radius);
            }

            Gizmos.matrix = Matrix4x4.Translate(center) * Matrix4x4.Rotate(Quaternion.FromToRotation(Vector3.up, normal));

            DrawLines(points, true);
        }

        /// <summary>
        /// Connects a series of points.
        /// </summary>
        /// <param name="points">Array of points, at least 2. The array is converted into a list internally.</param>
        /// <param name="closedLoop">Connect the last point to the first?</param>
        public static void DrawLines(Vector3[] points, bool closedLoop = false) => DrawLines(points.ToList(), closedLoop);

        /// <summary>
        /// Connects a series of points.
        /// </summary>
        /// <param name="points">List of points, at least 2.</param>
        /// <param name="closedLoop">Connect the last point to the first?</param>
        public static void DrawLines(List<Vector3> points, bool closedLoop = false)
        {
            if (points.Count < 2) return;

            int steps = points.Count - 1;
            if (closedLoop) steps++;

            for (int i = 0; i < steps; i++)
            {
                Gizmos.DrawLine(points[i], points[(i + 1) % points.Count]);
            }
        }
    }
}

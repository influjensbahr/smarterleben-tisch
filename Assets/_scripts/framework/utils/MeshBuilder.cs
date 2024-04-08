//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jannik Boysen
//

using System.Collections.Generic;
using UnityEngine;

namespace OTBT.Framework.Utils
{
    /// <summary>
    /// Mesh generators for primitive shapes.
    /// </summary>
    public static class MeshBuilder
    {
        public static Mesh Box(float depth, float width, float height)
        {
            Vector3[] vertices = new Vector3[8];

            vertices[0] = new Vector3(width / 2, height / 2, 0);
            vertices[1] = new Vector3(-width / 2, height / 2, 0);
            vertices[2] = new Vector3(-width / 2, -height / 2, 0);
            vertices[3] = new Vector3(width / 2, -height / 2, 0);
            vertices[4] = new Vector3(width / 2, -height / 2, depth);
            vertices[5] = new Vector3(-width / 2, -height / 2, depth);
            vertices[6] = new Vector3(-width / 2, height / 2, depth);
            vertices[7] = new Vector3(width / 2, height / 2, depth);

            int[] triangles = {
                0,
                2,
                1,
                0,
                3,
                2,
                2,
                3,
                4,
                2,
                4,
                5,
                1,
                2,
                5,
                1,
                5,
                6,
                0,
                7,
                4,
                0,
                4,
                3,
                5,
                4,
                7,
                5,
                7,
                6,
                0,
                6,
                7,
                0,
                1,
                6
            };

            //finalizing
            Mesh mesh = new Mesh {
                name = "Box"
            };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);

            return mesh;
        }

        public static Mesh HollowCylinder(float length, float radius, int circleVertices = 32)
        {
            Vector3[] vertices = new Vector3[2 * circleVertices];
            int[] triangles = new int[circleVertices * 2 * 3];

            float angleStep = 360f / circleVertices;
            for (int i = 0; i < circleVertices; i++)
            {
                int vi = i * 2;
                int ti = i * 3 * 2;

                Vector3 vec =
                    new Vector3(
                        Mathf.Sin(Mathf.Deg2Rad * angleStep * i),
                        Mathf.Cos(Mathf.Deg2Rad * angleStep * i),
                        0) * radius;

                vertices[vi] = vec;
                triangles[ti] = vi;
                triangles[ti + 1] = (vi + 1) % vertices.Length;
                triangles[ti + 2] = (vi + 2) % vertices.Length;

                ti += 3;
                vi++;
                vertices[vi] = Vector3.forward * length + vec;
                triangles[ti] = (vi + 2) % vertices.Length;
                triangles[ti + 1] = (vi + 1) % vertices.Length;
                triangles[ti + 2] = vi;
            }

            //finalizing
            Mesh mesh = new Mesh {
                name = "Hollow Cylinder"
            };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);

            return mesh;
        }

        // using https://lindenreid.wordpress.com/2017/11/07/procedural-sphere-ellipsoid-tutorial/
        public static Mesh Sphere(float radius, int resolution = 8)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

            float theta = Mathf.PI / resolution;
            float phi = Mathf.PI * 2 / resolution;

            //TOP CAP
            vertices.Add(Vector3.up * radius);

            //middle
            for (int stack = 1; stack < resolution; stack++)
            {
                float stackRadius = Mathf.Sin(theta * stack) * radius;
                for (int slice = 0; slice < resolution; slice++)
                {
                    vertices.Add(new Vector3(Mathf.Cos(phi * slice) * stackRadius,
                        Mathf.Cos(theta * stack) * radius,
                        Mathf.Sin(phi * slice) * stackRadius));
                }
            }

            //bot cap
            vertices.Add(Vector3.down * radius);

            //TODO: TRIANGLES
            //top cap
            for (int slice = 0; slice < resolution - 1; slice++)
            {
                triangles.Add(0);
                triangles.Add(slice + 2);
                triangles.Add(slice + 1);
            }

            triangles.Add(0);
            triangles.Add(1);
            triangles.Add(resolution);


            //middle
            for (int stack = 0; stack < resolution - 2; stack++)
            {
                int t1;
                int t2;
                int t3;
                int t4;
                for (int slice = 0; slice < resolution - 1; slice++)
                {
                    t1 = 1 + slice + (resolution * stack);
                    t2 = t1 + 1;
                    t3 = 1 + slice + (resolution * (stack + 1));
                    t4 = t3 + 1;

                    triangles.Add(t1);
                    triangles.Add(t2);
                    triangles.Add(t4);

                    triangles.Add(t1);
                    triangles.Add(t4);
                    triangles.Add(t3);
                }

                //last
                t1 = resolution * (stack + 1);
                t2 = 1 + (resolution * stack);
                t3 = resolution * (stack + 2);
                t4 = 1 + (resolution * (stack + 1));

                triangles.Add(t1);
                triangles.Add(t2);
                triangles.Add(t4);

                triangles.Add(t1);
                triangles.Add(t4);
                triangles.Add(t3);
            }

            //bottom 
            int lvi = vertices.Count - 1;
            for (int slice = 0; slice < resolution - 1; slice++)
            {
                int t2 = (resolution - 2) * resolution + slice + 1;
                int t3 = (resolution - 2) * resolution + slice + 2;
                triangles.Add(lvi);
                triangles.Add(t2);
                triangles.Add(t3);
            }

            triangles.Add(lvi);
            triangles.Add((resolution - 1) * resolution);
            triangles.Add((resolution - 2) * resolution + 1);


            //finalizing
            Mesh mesh = new Mesh {
                name = "Sphere"
            };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);

            return mesh;
        }

        public static Mesh HollowCone(float length, float angle, int circleVertices = 32)
        {
            // ASA: a = angle at point, s = length, b = right angle at base
            // known: angle c = 360-90-a
            // rule : a/sina = b/sinb = c/sinc;
            float c = 180f - 90 - angle;
            float clength = length / Mathf.Sin(c * Mathf.Deg2Rad);

            // clength = x / sin (angle) == clength * sin(angle) = x
            float baseRadius = clength * Mathf.Sin(angle * Mathf.Deg2Rad);

            Vector3[] vertices = new Vector3[1 + circleVertices];
            int[] triangles = new int[circleVertices * 2 * 3];

            vertices[0] = Vector3.zero;

            float angleStep = 360f / circleVertices;
            for (int i = 0; i < circleVertices; i++)
            {
                Vector3 vec = Vector3.forward * length +
                              new Vector3(
                                  Mathf.Sin(Mathf.Deg2Rad * angleStep * i),
                                  Mathf.Cos(Mathf.Deg2Rad * angleStep * i),
                                  0) * baseRadius;
                vertices[i + 1] = vec;
                triangles[i * 3] = i;
                triangles[i * 3 + 1] = (i + 1);
                triangles[i * 3 + 2] = 0;
            }

            //close up cone
            triangles[circleVertices * 3] = circleVertices;
            triangles[circleVertices * 3 + 1] = 1;
            triangles[circleVertices * 3 + 2] = 0;

            //finalizing
            Mesh mesh = new Mesh {
                name = "Hollow Cone"
            };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);

            return mesh;
        }
    }
}

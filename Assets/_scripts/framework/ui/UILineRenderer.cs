using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace OTBT.Framework.UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    [ExecuteAlways]
    public class UILineRenderer : Graphic
    {
        [SerializeField] List<Vector2> m_Points;
        [SerializeField,Min(float.Epsilon)] float m_Thickness;

        public List<Vector2> points
        {
            get => m_Points;
            set => m_Points = value;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            SetVerticesDirty();
        }
#endif


        protected override void OnPopulateMesh(VertexHelper vh)
        {
            if (m_Points.Count < 2) return;

            vh.Clear();
            for (var index = 0; index < m_Points.Count; index++)
            {
                var point = m_Points[index];

                var normalPrev = m_Points[Mathf.Clamp(index - 1, 0, m_Points.Count - 1)] - point;
                var normalNext = point - m_Points[Mathf.Clamp(index + 1, 0, m_Points.Count - 1)];

                DrawVerticesForPoint(point, (normalPrev + normalNext) / 2f, vh);
            }

            for (var index = 0; index < m_Points.Count - 1; index++)
            {
                var vi = index * 2;
                vh.AddTriangle(vi + 0, vi + 1, vi + 3);
                vh.AddTriangle(vi + 3, vi + 2, vi + 0);
            }
        }

        void DrawVerticesForPoint(Vector2 point, Vector2 direction, VertexHelper vh)
        {
            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = color;

            var normal = new Vector2(direction.y, direction.x).normalized;

            vertex.position = point + normal * (m_Thickness / 2f);
            vh.AddVert(vertex);

            vertex.position = point - normal * (m_Thickness / 2f);
            vh.AddVert(vertex);
        }


    }
}
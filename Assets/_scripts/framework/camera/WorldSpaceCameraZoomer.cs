//
// Copyright (c) 2023 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr / Annika Neumann
//

using System.Collections.Generic;
using OTBT.Framework.Utils;
using Sparrow.Verification;
using UnityEditor;
using UnityEngine;

namespace OTBT.Framework.Cameras
{
    [RequireComponent(typeof(Camera))]
    public class WorldSpaceCameraZoomer : MonoBehaviour, IVerify, IExtendDefaultEditor
    {
        [SerializeField] Camera m_WorldSpaceCamera = default;

        [Header("Maximum that should be visible")]
        [SerializeField] public Transform m_BackgroundLeftTop = default;
        [SerializeField] public Transform m_BackgroundRightBottom = default;

        [Header("Things that should be visible")]
        [SerializeField] public List<Vector3> m_VisiblePoints = new List<Vector3>();
        [Tooltip("How many percent of border should be made around these points. 1 = super tight packing")]
        [SerializeField] float m_BorderPercentSides = 1.2f;
        [SerializeField] float m_BorderPercentTopBottom = 1.2f;

        [SerializeField] float m_ToleranceArea = 0.2f;

        [SerializeField] float m_LerpTime = 1f;

        public Camera cam => m_WorldSpaceCamera;

        private float m_CurrentLerpTime = 0f;

        private Vector3 m_StartPosition = Vector3.zero;
        private float m_StartProjection = 1f;

        private Vector3 m_TargetPosition = Vector3.zero;
        private float m_TargetProjection = 1f;

        private const string k_CornerIconName = "../" + Glyphicons.GeneratedIconPath + "circles_7.png";
        private const string k_VisiblePointIconName = "../" + Glyphicons.IconPath + Glyphicons.MenuClose;

        private void OnDrawGizmos()
        {
            for(int i = 0; i < m_VisiblePoints.Count; i++)
            {
                if (m_VisiblePoints[i] == null) continue;
                // Draws a pink line from this transform to the target
                Gizmos.color = Color.magenta;
                Gizmos.DrawIcon(m_VisiblePoints[i], k_VisiblePointIconName, true);
                Gizmos.DrawLine(m_VisiblePoints[i], m_VisiblePoints[(i + 1 >= m_VisiblePoints.Count ? 0 : i+1)]);
            }
         
            if(m_BackgroundLeftTop)
                Gizmos.DrawIcon(m_BackgroundLeftTop.position, k_CornerIconName, false);
            if(m_BackgroundRightBottom)
                Gizmos.DrawIcon(m_BackgroundRightBottom.position, k_CornerIconName, false); 
        }

        private void OnValidate()
        {
            if (m_WorldSpaceCamera == null)
                m_WorldSpaceCamera = GetComponent<Camera>();
        }

        public void ClearVisiblePoints()
        {
            m_VisiblePoints.Clear();
        }

        public void AddVisiblePoint(Vector3 point)
        {
            m_VisiblePoints.Add(point);
        }

        /// <summary>
        /// Checks if all points are within the tolerance area or if we should zoom out/in
        /// </summary>
        /// <returns>false if an update should be performed</returns>
        public bool CheckIfAllInBounds()
        {
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);
            foreach (Vector3 point in m_VisiblePoints)
            {
                // get where the points are on screen
                Vector2 screenPoint = m_WorldSpaceCamera.WorldToScreenPoint(point);

                // check if they are outside of screen, that's a red flag!
                if (screenPoint.x < 0f || screenPoint.y < 0f)
                {
                    return false;
                }

                if (screenPoint.x > m_WorldSpaceCamera.pixelWidth) return false;
                if (screenPoint.y > m_WorldSpaceCamera.pixelHeight) return false;

                // collect min and max
                min.x = min.x < screenPoint.x ? min.x : screenPoint.x;
                min.y = min.y < screenPoint.y ? min.y : screenPoint.y;
                max.x = max.x > screenPoint.x ? max.x : screenPoint.x;
                max.y = max.y > screenPoint.y ? max.y : screenPoint.y;
            }

            if (max.x - min.x > m_WorldSpaceCamera.pixelWidth || max.x - min.x < (1f - 2f * m_ToleranceArea) * m_WorldSpaceCamera.pixelWidth)
                return false;

            if (max.y - min.y > m_WorldSpaceCamera.pixelHeight || max.y - min.y < (1f - 2f * m_ToleranceArea) * m_WorldSpaceCamera.pixelHeight)
                return false;

            return true;
        }

        public void UpdateFrame()
        {
            m_CurrentLerpTime += Time.deltaTime;
            
            float timer = Mathf.SmoothStep(0f, 1f, Mathf.SmoothStep(0f, 1f, m_CurrentLerpTime / m_LerpTime));

            m_WorldSpaceCamera.orthographicSize = Mathf.Lerp(m_StartProjection, m_TargetProjection, timer);
            m_WorldSpaceCamera.transform.position = Vector3.Lerp(m_StartPosition, m_TargetPosition, timer);
        }

        public void UpdateCameraScale(bool setInstant = true)
        {
            m_CurrentLerpTime = setInstant ? 10000f : 0f;
            m_StartPosition = m_WorldSpaceCamera.transform.position;
            m_StartProjection = m_WorldSpaceCamera.orthographicSize;
        
            // MAXIMUM ORTHO SCALE
            // get screen target width:
            Vector3 m_Projection1 = m_WorldSpaceCamera.WorldToScreenPoint(m_BackgroundLeftTop.position);
            Vector3 m_Projection2 = m_WorldSpaceCamera.WorldToScreenPoint(m_BackgroundRightBottom.position);

            // get proportions
            float prop_x = Mathf.Abs(m_Projection1.x - m_Projection2.x)/ m_WorldSpaceCamera.pixelWidth;
            float prop_y = Mathf.Abs(m_Projection1.y - m_Projection2.y)/ m_WorldSpaceCamera.pixelHeight;
            float maxWorldSpaceOrthoSize = Mathf.Min(m_WorldSpaceCamera.orthographicSize * prop_x,
                m_WorldSpaceCamera.orthographicSize * prop_y);

            if (m_VisiblePoints.Count == 0)
            {
                // HERE: only maximum bounds defined
                Vector3 middlePoint = (m_Projection1 + m_Projection2) * 0.5f;
                Vector3 m_Projection = m_WorldSpaceCamera.ScreenToWorldPoint(middlePoint);
                m_Projection.z = m_WorldSpaceCamera.transform.position.z;

                if (setInstant)
                {
                    m_WorldSpaceCamera.transform.position = m_Projection;
                    m_WorldSpaceCamera.orthographicSize = maxWorldSpaceOrthoSize;
                }
                m_TargetPosition = m_Projection;
                m_TargetProjection = maxWorldSpaceOrthoSize;
            }
            else
            {
                // HERE: also partial bounds defined
                Vector2 m_PointsMin = new Vector2(float.MaxValue, float.MaxValue);
                Vector2 m_PointsMax = new Vector2(float.MinValue, float.MinValue);
                Vector3 m_Projection;
                foreach (Vector3 t in m_VisiblePoints)
                {
                    m_Projection = m_WorldSpaceCamera.WorldToScreenPoint(t);
                    m_PointsMin.x = m_Projection.x < m_PointsMin.x ? m_Projection.x : m_PointsMin.x;
                    m_PointsMin.y = m_Projection.y < m_PointsMin.y ? m_Projection.y : m_PointsMin.y;
                    m_PointsMax.x = m_Projection.x > m_PointsMax.x ? m_Projection.x : m_PointsMax.x;
                    m_PointsMax.y = m_Projection.y > m_PointsMax.y ? m_Projection.y : m_PointsMax.y;
                }
                // if visible points is empty, this is super huge and filtered below
                prop_x = (Mathf.Abs(m_PointsMin.x - m_PointsMax.x)* m_BorderPercentSides) / m_WorldSpaceCamera.pixelWidth;
                prop_y = (Mathf.Abs(m_PointsMin.y - m_PointsMax.y)* m_BorderPercentTopBottom) / m_WorldSpaceCamera.pixelHeight;
                float minWorldSpaceOrthoSize = m_WorldSpaceCamera.orthographicSize * Mathf.Max(prop_x, prop_y);

                // FIND IDEAL POSITION
                Vector3 middlePoint = (m_PointsMin + m_PointsMax) * 0.5f;
                middlePoint.z = m_WorldSpaceCamera.transform.position.z;
                m_Projection = m_WorldSpaceCamera.ScreenToWorldPoint(middlePoint);
                m_Projection.z = m_WorldSpaceCamera.transform.position.z;

                m_WorldSpaceCamera.transform.position = m_Projection;
                m_WorldSpaceCamera.orthographicSize = Mathf.Min(minWorldSpaceOrthoSize, maxWorldSpaceOrthoSize);

                // MAKE SURE POSITION IS WITHIN ALLOWED BOUNDS
                Vector2 delta = Vector2.zero;
                m_Projection = m_WorldSpaceCamera.ScreenToWorldPoint(new Vector2(0, m_WorldSpaceCamera.pixelHeight));


                if (m_Projection.x < m_BackgroundLeftTop.position.x)
                    delta.x = m_Projection.x - m_BackgroundLeftTop.position.x;
                if (m_Projection.y > m_BackgroundLeftTop.position.y)
                    delta.y = m_Projection.y - m_BackgroundLeftTop.position.y;

                m_Projection = m_WorldSpaceCamera.ScreenToWorldPoint(new Vector2(m_WorldSpaceCamera.pixelWidth, 0));
                if (m_Projection.x > m_BackgroundRightBottom.position.x)
                {
                    if (delta.x != 0f) Debug.LogError("Camera delta is too small and too large at the same time! X");
                    delta.x = m_Projection.x - m_BackgroundRightBottom.position.x;
                }
                if (m_Projection.y < m_BackgroundRightBottom.position.y)
                {
                    if (delta.y != 0f) Debug.LogError("Camera delta is too small and too large at the same time! Y");
                    delta.y = m_Projection.y - m_BackgroundRightBottom.position.y;
                }

                // move by delta
                m_WorldSpaceCamera.transform.position -= new Vector3(delta.x, delta.y, 0f);

                m_TargetPosition = m_WorldSpaceCamera.transform.position;
                m_TargetProjection = m_WorldSpaceCamera.orthographicSize;
            
                if (setInstant)
                {
                    return;
                }

                m_WorldSpaceCamera.transform.position = m_StartPosition;
                m_WorldSpaceCamera.orthographicSize = m_StartProjection;
            }
        }

        public void Verify(CheckVerifyInterface checker)
        {
            checker.CheckNotNull(m_WorldSpaceCamera, "WorldSpaceCamera", gameObject);
            checker.CheckNotNull(m_BackgroundLeftTop, "BackgroundLeftTop", gameObject);
            checker.CheckNotNull(m_BackgroundRightBottom, "BackgroundRightBottom", gameObject);
        }

#if UNITY_EDITOR
        public void ExtendDefaultEditor()
        {
            if (GUILayout.Button("Rename positioning gizmos"))
            {
                if (m_BackgroundLeftTop != null) m_BackgroundLeftTop.gameObject.name = "top_left";
                if (m_BackgroundRightBottom != null) m_BackgroundRightBottom.gameObject.name = "bottom_right";
                EditorUtility.SetDirty(gameObject);
            }
            if (GUILayout.Button("Reposition Camera Now"))
            {
                if (CheckIfAllInBounds())
                {
                    return;
                }
                UpdateCameraScale();
                UpdateFrame();
                EditorUtility.SetDirty(gameObject);
            }
        }
#endif
    }
}

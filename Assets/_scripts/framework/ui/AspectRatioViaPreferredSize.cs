using OTBT.Framework.Utils;
using Sparrow.Verification;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OTBT.Framework.UI
{
    [RequireComponent(typeof(LayoutElement))]
    [RequireComponent(typeof(RectTransform))]
    public class AspectRatioViaPreferredSize : MonoBehaviour, IVerify, IExtendDefaultEditor
    {
        enum Mode { WidthControlsHeight, HeightControlsWidth, WidthControlsHeightForce, HeightControlsWidthForce }
        [SerializeField] LayoutElement m_LayoutElement;
        [SerializeField] RectTransform m_RectTransform;

        [SerializeField] Mode m_ScaleMode = Mode.WidthControlsHeight;
        [SerializeField] float m_AspectRatio = 1f;

        private void OnValidate()
        {
            m_LayoutElement = GetComponent<LayoutElement>();
            m_RectTransform = GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            UpdateDisplay();
        }

        public void SetAspectRatio(float aspectRatio)
        {
            m_AspectRatio = aspectRatio;
            UpdateDisplay();
        }

        private void OnGUI()
        {
            UpdateDisplay();
        }

        void UpdateDisplay()
        {
            Vector2 size = m_RectTransform.sizeDelta;
            switch (m_ScaleMode)
            {
                case Mode.WidthControlsHeight:
                    m_LayoutElement.preferredWidth = -1;
                    m_LayoutElement.preferredHeight = m_RectTransform.rect.width / m_AspectRatio;
                    break;
                case Mode.HeightControlsWidth:
                    m_LayoutElement.preferredHeight = -1;
                    m_LayoutElement.preferredWidth = m_RectTransform.rect.height / m_AspectRatio;
                    break;

                case Mode.WidthControlsHeightForce:
                    m_LayoutElement.preferredWidth = -1;
                    m_LayoutElement.preferredHeight = m_RectTransform.rect.width / m_AspectRatio;
                    size.y = m_LayoutElement.preferredHeight;
                    m_RectTransform.sizeDelta = size;
                    break;
                case Mode.HeightControlsWidthForce:
                    m_LayoutElement.preferredHeight = -1;
                    m_LayoutElement.preferredWidth = m_RectTransform.rect.height / m_AspectRatio;
                    size.x = m_LayoutElement.preferredWidth;
                    m_RectTransform.sizeDelta = size;
                    break;
            }

            m_LayoutElement.minWidth = m_LayoutElement.preferredWidth;
            m_LayoutElement.minHeight = m_LayoutElement.preferredHeight;
        }
        public void Verify(CheckVerifyInterface checker)
        {
            checker.CheckNotNull(m_LayoutElement, "layout elemtn", this);
            checker.CheckNotNull(m_RectTransform, "rect transform", this);
        }

#if UNITY_EDITOR

        public void ExtendDefaultEditor()
        {
            if(GUILayout.Button("Update now"))
            {
                UpdateDisplay();
            }
        }
#endif
    }
}
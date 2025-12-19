using DG.Tweening;
using OTBT.Framework.UI;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays details for a project with simple entrance/exit animations and optional images.
/// </summary>
public class ProjectInfosView : MonoBehaviour
{
    [SerializeField] Image m_ArrowLeft = default;
    [SerializeField] Image m_ArrowRight = default;
    [SerializeField] Image m_ProjectImageOne = default;
    [SerializeField] Image m_ProjectImageTwo = default;
    [SerializeField] GameObject m_ProjectImagesBoth = default;
    [SerializeField] TextMeshProUGUI m_ProjectNameText, m_ProjectDescriptionText;
    [SerializeField] CanvasGroup m_MainCanvasGroup = default;
    [SerializeField] GameObject m_HideButton = default;
    string m_ProjectName, m_ProjectDescription;

    private Tweener showTweener;
    private Tweener hideTweener;

    Vector2 m_LeftArrowStart = new Vector2(-75f, 29f);

    private void Start()
    {
        if (m_MainCanvasGroup != null)
        {
            m_MainCanvasGroup.alpha = 0f;
            m_MainCanvasGroup.interactable = false;
        }
        if (m_ArrowLeft != null)
        {
            m_LeftArrowStart = m_ArrowLeft.rectTransform.anchoredPosition;
            m_ArrowLeft.DOFade(0f, 0f);
        }
        if (m_ArrowRight != null)
        {
            m_ArrowRight.DOFade(0f, 0f);
        }
    }

    public void Show(bool showHideButton = true)
    {
        if (m_HideButton != null) m_HideButton.SetActive(showHideButton);
        // Stop active tweens if any
        hideTweener?.Kill();
        if (m_MainCanvasGroup != null) m_MainCanvasGroup.interactable = true;

        // MainCanvasGroup einfaden
        if (m_MainCanvasGroup != null)
            showTweener = m_MainCanvasGroup.DOFade(1f, 0.5f).OnKill(() => m_MainCanvasGroup.alpha = 1f);

        // Arrows bewegen und einfaden
        if (m_ArrowLeft != null)
        {
            m_ArrowLeft.rectTransform.anchoredPosition = m_LeftArrowStart + Vector2.left * 355f;
            m_ArrowLeft.rectTransform.DOAnchorPosX(m_LeftArrowStart.x, 0.85f, true);
            m_ArrowLeft.DOFade(1f, 0.5f);
        }

        if (m_ArrowRight != null)
        {
            m_ArrowRight.rectTransform.anchoredPosition = m_LeftArrowStart + Vector2.left * 175f;
            m_ArrowRight.rectTransform.DOAnchorPosX(m_LeftArrowStart.x + 75f, 0.85f, true);
            m_ArrowRight.DOFade(1f, 0.5f);
        }
    }

    public Sequence Hide()
    {
        // Stop active tweens if any
        showTweener?.Kill();
        var sequence2 = DOTween.Sequence();
        if (m_MainCanvasGroup != null) m_MainCanvasGroup.interactable = false;
        // MainCanvasGroup ausfaden
        if (m_MainCanvasGroup != null)
        {
            hideTweener = m_MainCanvasGroup.DOFade(0f, 0.5f).OnKill(() => m_MainCanvasGroup.alpha = 0f);
            sequence2.Join(hideTweener);
        }
        // Arrows bewegen und ausfaden
        if (m_ArrowLeft != null)
        {
            sequence2.Join(m_ArrowLeft.rectTransform.DOAnchorPosX(m_LeftArrowStart.x + 355f, 0.85f, true));
            sequence2.Join(m_ArrowLeft.DOFade(0f, 0.5f));
        }

        if (m_ArrowRight != null)
        {
            sequence2.Join(m_ArrowRight.rectTransform.DOAnchorPosX(m_LeftArrowStart.x + 175f, 0.85f, true));
            sequence2.Join(m_ArrowRight.DOFade(0f, 0.5f));
        }
        return sequence2;
    }

    public void SetProjectInfos(KielRegionProjectData projectInfo)
    {
        m_ProjectName = projectInfo.title;
        m_ProjectDescription = projectInfo.shortDescription;
        if (m_ProjectNameText != null) m_ProjectNameText.text = m_ProjectName;
        if (m_ProjectDescriptionText != null) m_ProjectDescriptionText.text = m_ProjectDescription;

        if(projectInfo.projectImages.Count > 0)
        {
            if (m_ProjectImagesBoth != null) m_ProjectImagesBoth.SetActive(true);
            if (m_ProjectImageOne != null)
            {
                m_ProjectImageOne.gameObject.SetActive(true);
                m_ProjectImageOne.sprite = projectInfo.projectImages[0];
                var ratio = projectInfo.projectImages[0].rect.width / projectInfo.projectImages[0].rect.height;
                var aspect = m_ProjectImageOne.GetComponent<AspectRatioViaPreferredSize>();
                if (aspect != null) aspect.SetAspectRatio(ratio);
            }
        } else
        {
            if (m_ProjectImageOne != null) m_ProjectImageOne.gameObject.SetActive(false);
            if (m_ProjectImagesBoth != null) m_ProjectImagesBoth.SetActive(false);
        }

        if (projectInfo.projectImages.Count > 1)
        {
            if (m_ProjectImageTwo != null)
            {
                m_ProjectImageTwo.gameObject.SetActive(true);
                m_ProjectImageTwo.sprite = projectInfo.projectImages[1];
                var ratio2 = projectInfo.projectImages[1].rect.width / projectInfo.projectImages[1].rect.height;
                var aspect2 = m_ProjectImageTwo.GetComponent<AspectRatioViaPreferredSize>();
                if (aspect2 != null) aspect2.SetAspectRatio(ratio2);
            }
        }
        else
        {
            if (m_ProjectImageTwo != null) m_ProjectImageTwo.gameObject.SetActive(false);
        }
    }
}

using DG.Tweening;
using OTBT.Framework.UI;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        m_MainCanvasGroup.alpha = 0f;
        m_MainCanvasGroup.interactable = false;
        m_LeftArrowStart = m_ArrowLeft.rectTransform.anchoredPosition;
        m_ArrowLeft.DOFade(0f, 0f);
        m_ArrowRight.DOFade(0f, 0f);
    }

    public void Show(bool showHideButton = true)
    {
        m_HideButton.SetActive(showHideButton);
        // Stop active tweens if any
        hideTweener?.Kill();
        m_MainCanvasGroup.interactable = true;

        // MainCanvasGroup einfaden
        showTweener = m_MainCanvasGroup.DOFade(1f, 0.5f).OnKill(() => m_MainCanvasGroup.alpha = 1f);

        // Arrows bewegen und einfaden
        m_ArrowLeft.rectTransform.anchoredPosition = m_LeftArrowStart + Vector2.left * 355f;
        m_ArrowLeft.rectTransform.DOAnchorPosX(m_LeftArrowStart.x, 0.85f, true);
        m_ArrowLeft.DOFade(1f, 0.5f);

        m_ArrowRight.rectTransform.anchoredPosition = m_LeftArrowStart + Vector2.left * 175f;
        m_ArrowRight.rectTransform.DOAnchorPosX(m_LeftArrowStart.x + 75f, 0.85f, true);
        m_ArrowRight.DOFade(1f, 0.5f);
    }

    public Sequence Hide()
    {
        // Stop active tweens if any
        showTweener?.Kill();
        var sequence2 = DOTween.Sequence();
        m_MainCanvasGroup.interactable = false;
        // MainCanvasGroup ausfaden
        hideTweener = m_MainCanvasGroup.DOFade(0f, 0.5f).OnKill(() => m_MainCanvasGroup.alpha = 0f);
        sequence2.Join(hideTweener);
        // Arrows bewegen und ausfaden
        sequence2.Join(m_ArrowLeft.rectTransform.DOAnchorPosX(m_LeftArrowStart.x + 355f, 0.85f, true));
        sequence2.Join(m_ArrowLeft.DOFade(0f, 0.5f));

        sequence2.Join(m_ArrowRight.rectTransform.DOAnchorPosX(m_LeftArrowStart.x + 175f, 0.85f, true));
        sequence2.Join(m_ArrowRight.DOFade(0f, 0.5f));
        return sequence2;
    }

    public void SetProjectInfos(KielRegionProjectDataObject projectInfo)
    {
        m_ProjectName = projectInfo.title;
        m_ProjectDescription = projectInfo.shortDescription;
        m_ProjectNameText.text = m_ProjectName;
        m_ProjectDescriptionText.text = m_ProjectDescription;

        if(projectInfo.projectImage.Count > 0)
        {
            m_ProjectImagesBoth.SetActive(true);
            m_ProjectImageOne.gameObject.SetActive(true);
            m_ProjectImageOne.sprite = projectInfo.projectImage[0];
            m_ProjectImageOne.GetComponent<AspectRatioViaPreferredSize>().SetAspectRatio(projectInfo.projectImage[0].rect.width / projectInfo.projectImage[0].rect.height);
        } else
        {
            m_ProjectImageOne.gameObject.SetActive(false);
            m_ProjectImagesBoth.SetActive(false);
        }

        if (projectInfo.projectImage.Count > 1)
        {
            m_ProjectImageTwo.gameObject.SetActive(true);
            m_ProjectImageTwo.sprite = projectInfo.projectImage[1];
            m_ProjectImageTwo.GetComponent<AspectRatioViaPreferredSize>().SetAspectRatio(projectInfo.projectImage[1].rect.width / projectInfo.projectImage[1].rect.height);
        }
        else
        {
            m_ProjectImageTwo.gameObject.SetActive(false);
        }
    }
}

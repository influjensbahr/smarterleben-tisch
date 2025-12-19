using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles fade/scale show-hide animations for a group of category icon images.
/// </summary>
class StoreScale : MonoBehaviour
{
    public Vector3 startScale;
    public void Set(Transform t)
    {
        startScale = t.localScale;
    }
}

public class CategoryIcons : MonoBehaviour
{
    [SerializeField] ProjectCategory m_ImageCategory = default;
    readonly List<Image> m_SubspriteList = new List<Image>();
    public ProjectCategory category => m_ImageCategory;

    bool m_IsShowing = false;

    private void Start()
    {
        m_SubspriteList.Clear();
        foreach (var img in GetComponentsInChildren<Image>())
        {
            m_SubspriteList.Add(img);
            img.gameObject.AddComponent<StoreScale>().Set(img.transform);
            img.DOFade(0f, 0f);
            //img.transform.localScale = Vector3.zero;
        }
        m_IsShowing = false;
    }

    public async void Show()
    {
        if (m_IsShowing) return;
        m_IsShowing = true;
        foreach (var img in m_SubspriteList)
        {
            if (img == null) continue;
            img.transform.localScale = Vector3.zero;
            img.DOFade(1f, .5f);
            img.transform.DOScale(img.GetComponent<StoreScale>().startScale, .5f);
            await Task.Delay(25);
        }
    }

    public async void Hide()
    {
        if (!m_IsShowing) return;
        m_IsShowing = false;
        foreach (var img in m_SubspriteList)
        {
            if (img == null) continue;
            img.DOFade(0f, .5f);
            img.transform.DOScale(0f, .5f);
            await Task.Delay(25);
        }
    }
}

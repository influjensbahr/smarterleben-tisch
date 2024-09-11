using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


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
    List<Image> m_SubspriteList = new List<Image>();
    public ProjectCategory category => m_ImageCategory;

    bool m_IsShowing = false;

    private void Start()
    {
        foreach(var obj in gameObject.GetComponentsInChildren<Image>())
        {
            m_SubspriteList.Add(obj);
            obj.AddComponent<StoreScale>().Set(obj.transform);
            obj.DOFade(0f, 0f);
        }
        m_IsShowing = false;
    }

    public async void Show()
    {
        if (m_IsShowing) return;
        m_IsShowing = true;
        Debug.Log("START SHOW: " + gameObject.name);
        foreach (var obj in gameObject.GetComponentsInChildren<Image>())
        {
            obj.transform.DOScale(0f, 0f);
            obj.DOFade(1f, .5f);
            obj.transform.DOScale(obj.GetComponent<StoreScale>().startScale, .5f);
            await Task.Delay(25);
        }
    }

    public async void Hide()
    {
        if (!m_IsShowing) return;
        m_IsShowing = false;
        foreach (var obj in gameObject.GetComponentsInChildren<Image>())
        {
            obj.DOFade(0f, .5f);
            obj.transform.DOScale(0f, .5f);
            await Task.Delay(25);
        }
    }
}

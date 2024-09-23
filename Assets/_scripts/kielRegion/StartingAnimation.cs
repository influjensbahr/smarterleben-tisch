using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class StartingAnimation : MonoBehaviour
{
    [SerializeField] CanvasRenderer[] m_Sprites;
    [SerializeField] CanvasRenderer m_Logo;
    [SerializeField] Image m_LogoBg;
    [SerializeField] float m_AnimSpeed = 1f;
    [SerializeField] float m_Delay = 0.2f;

    private void Start()
    {
        StartLogoAnimation();
    }

    /** 
     * Last: This method was used to animate KielRegion Arrow sprites from left to right.
     */
    void StartAnimation()
    {
        for (int i = 0; i < m_Sprites.Length; i++)
        {
            var sprite = m_Sprites[i];
            var startPos = new Vector3(-Screen.width / 2, sprite.transform.position.y, sprite.transform.position.z);
            var endPos = new Vector3(Screen.width * 2, sprite.transform.position.y, sprite.transform.position.z);

            sprite.transform.position = startPos;
            var sequence = DOTween.Sequence();
            sequence.AppendInterval(i * m_Delay);
            sequence.Append(sprite.transform.DOMove(endPos, m_AnimSpeed).SetEase(Ease.InOutSine));
            sequence.Play();
        }
    }

    /** 
     * Current: This method is used to animate the KielRegion logo, scaling it up and moving it to the right bottom corner.
     */
    void StartLogoAnimation()
    {
        m_Logo.transform.position = new Vector3(Screen.width / 2, Screen.height / 2, m_Logo.transform.position.z);
        m_Logo.transform.localScale = Vector3.zero;
        
        var sequence = DOTween.Sequence();
        sequence.Append(m_Logo.transform.DOScale(Vector3.one, m_AnimSpeed).SetEase(Ease.OutBack));
        sequence.AppendInterval(1f);
        sequence.Append(m_LogoBg.DOFade(0, m_AnimSpeed));
        sequence.Join(m_Logo.transform.DOScale(new Vector3(0.3f, 0.3f, 1), m_AnimSpeed));
        sequence.Join(m_Logo.transform.DOMove(new Vector3(3400, 222, m_Logo.transform.position.z), m_AnimSpeed));
        sequence.AppendCallback(()=> m_LogoBg.gameObject.SetActive(false));
        sequence.Play();
    }
}

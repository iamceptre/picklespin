using TMPro;
using UnityEngine;
using DG.Tweening;

public class TextFadeInOnEnable : MonoBehaviour
{
    private TMP_Text m_Text;
    [SerializeField] private float animationTime;

    private void Awake()
    {
        m_Text = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        m_Text.alpha = 0;
       Tween myTween = m_Text.DOFade(1, animationTime);
        myTween.SetUpdate(UpdateType.Normal, true);
    }

}

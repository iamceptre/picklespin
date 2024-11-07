using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BarEase : MonoBehaviour
{
    public Slider sliderToFollow;
    [HideInInspector] public Slider me;
    private float velocity;
    private float myValue;
    [SerializeField] private Image easeFill;

    private void Awake()
    {
        me = GetComponent<Slider>();
    }

    private void Start()
    {
        myValue = sliderToFollow.value;
        me.value = myValue;
        easeFill.enabled = true;
    }

    private void Update()
    {
        myValue = Mathf.SmoothDamp(myValue, sliderToFollow.value, ref velocity, 0.3f);
        me.value = myValue;
        easeFill.enabled = Mathf.Abs(myValue - sliderToFollow.value) >= 0.01f;
    }

    public void FadeOut()
    {
        easeFill.DOFade(0, 0.5f);
    }

    public void FadeIn()
    {
        easeFill.DOFade(1, 0.2f);
    }
}
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
        me.value = sliderToFollow.maxValue;
        myValue = sliderToFollow.maxValue;
        easeFill.enabled = true;
    }

    private void Update() //change it so each bar calls a routine that loops until easing is needed and then it disables itself
    {
        if (sliderToFollow.value<sliderToFollow.maxValue) {
            myValue = Mathf.SmoothDamp(myValue, sliderToFollow.value, ref velocity, 0.3f); //delay it in time
        }


        if (myValue-0.1f <= sliderToFollow.value)
        {
            easeFill.enabled = false;
        }
        else
        {
            easeFill.enabled = true;
            me.value = myValue;
        }   
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

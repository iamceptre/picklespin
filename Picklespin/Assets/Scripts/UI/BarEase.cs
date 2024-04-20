using UnityEngine;
using UnityEngine.UI;

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
        //me.value = sliderToFollow.value-1;
        //easeFill.enabled = false;
    }

    private void LateUpdate()
    {

        myValue = Mathf.SmoothDamp(myValue, sliderToFollow.value, ref velocity, 0.3f); //delay it in time

        if (myValue > sliderToFollow.value + 0.1f) {
            
            me.value = myValue;
            easeFill.enabled = true;
        }
        else
        {
            easeFill.enabled = false;
        }

    }

}

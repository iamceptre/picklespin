using UnityEngine;
using UnityEngine.UI;

public class BarEase : MonoBehaviour
{
    public Slider sliderToFollow;
    [HideInInspector] public Slider me;
    private float velocity;
    private float myValue;


    private void Awake()
    {
        me = GetComponent<Slider>();
    }

    private void Start()
    {
        me.value = sliderToFollow.value-1;
    }

    private void LateUpdate()
    {
        myValue = Mathf.SmoothDamp(myValue, sliderToFollow.value, ref velocity, 0.3f);

        if (myValue > sliderToFollow.value)
        {
           //Debug.Log("gui update");

          me.value = myValue; //Delay this line in time, efficiently in the future
        }
    }

}

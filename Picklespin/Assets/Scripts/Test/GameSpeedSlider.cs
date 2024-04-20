using UnityEngine;
using UnityEngine.UI;

public class GameSpeedSlider : MonoBehaviour
{

    private Slider me;

    private void Awake()
    {
        me = GetComponent<Slider>();
    }

    public void ApplyNewSpeed()
    {
        Time.timeScale = me.value;
    }

}

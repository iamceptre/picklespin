using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EscapeTimer : MonoBehaviour
{
    private Color sexyRed = new Color(0.76f, 0.235f, 0.235f);
    private Color startingColor;
    private bool startedColoring = false;

    [SerializeField] private Slider mySlider;
    [SerializeField] private Image sliderFill;
    private Death death;

    UIFadeFlicker sliderFillFlicker;
    private bool startedFlickering = false;

    [SerializeField] private TextFadeFlicker textFadeFlicker;

    [SerializeField] private float countdownSpeed = 1;
    [SerializeField] private float countdownDelayTime = 2;

    [SerializeField] private GameObject[] enableWithMe;
    [SerializeField] private GameObject[] disableWithMe;

    private IEnumerator colorRoutine;


    private void EnableOtherObjects()
    {
        for (int i = 0; i < enableWithMe.Length; i++)
        {
            enableWithMe[i].SetActive(true);
        }
    }

    private void DisableOtherObjects()
    {
        for (int i = 0; i < disableWithMe.Length; i++)
        {
            disableWithMe[i].SetActive(false);
        }
    }


   private IEnumerator Start()
    {

        colorRoutine = ColorTheFill();

        death = Death.instance;

        startingColor = sliderFill.color;

        sliderFillFlicker = sliderFill.gameObject.GetComponent<UIFadeFlicker>();

        if (enableWithMe.Length > 0)
        {
            EnableOtherObjects();
        }

        if (disableWithMe.Length > 0)
        {
            DisableOtherObjects();
        }

        yield return new WaitForSeconds(countdownDelayTime);

        while (true)
        {
            mySlider.value -= Time.deltaTime * countdownSpeed;

            if(mySlider.value <= 0.5f && !startedColoring)
            {
                startedColoring = true;
                StartCoroutine(colorRoutine);
            }


            if (mySlider.value <= 0)
            {
                StopAllCoroutines();
                death.PlayerDeath();
                enabled = false;
            }

            yield return null;
        }
    }


    private IEnumerator ColorTheFill()
    {
        while (true)
        {
            var t = (0.625f / mySlider.value) - 1.25f;
            sliderFill.color = Color.Lerp(startingColor, sexyRed, t);


            if (mySlider.value <= 0.25f && !startedFlickering)
            {
                textFadeFlicker.animationTime = 0.1f;
                textFadeFlicker.RestartTweening();
                sliderFillFlicker.StartFlicker();
                sliderFill.color = sexyRed;
                startedFlickering = true;
                StopCoroutine(colorRoutine);
            }

            yield return null;
        }
    }

}

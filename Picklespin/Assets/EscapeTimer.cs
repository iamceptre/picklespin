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

    [SerializeField] private float countdownSpeed = 1;
    [SerializeField] private float countdownDelayTime = 2;

    [SerializeField] private GameObject[] enableWithMe;
    [SerializeField] private GameObject[] disableWithMe;


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

        death = Death.instance;

        startingColor = sliderFill.color;

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
                var colorRoutine = StartCoroutine(ColorTheFill());
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
            var t = 0.5f / mySlider.value - 1;
            sliderFill.color = Color.Lerp(startingColor, sexyRed, t);
            yield return null;
        }
    }

}

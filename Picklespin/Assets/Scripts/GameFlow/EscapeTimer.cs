using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EscapeTimer : MonoBehaviour
{
    public static EscapeTimer instance { get; private set; }

    private Color sexyRed = new Color(0.76f, 0.235f, 0.235f);
    private Color startingColor;
    private bool startedColoring = false;

    [SerializeField] private Slider mySlider;
    [SerializeField] private Image sliderFill;

    UIFadeFlicker sliderFillFlicker;
    private bool startedFlickering = false;

    [SerializeField] private TextFadeFlicker textFadeFlicker;
    private PortalAfterClosing portalAfterClosing;

     public float countdownSpeed = 1;
    private WaitForSeconds countdownDelayTime = new WaitForSeconds(2);

    [SerializeField] private GameObject[] enableWithMe;
    [SerializeField] private GameObject[] disableWithMe;
    [SerializeField] private Canvas[] disableCanvasesWithMe;

    private IEnumerator colorRoutine;


    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }


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
            disableCanvasesWithMe[i].enabled = false;
        }
    }


   private IEnumerator Start()
    {

        portalAfterClosing = PortalAfterClosing.instance;

        colorRoutine = ColorTheFill();

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

        yield return countdownDelayTime;

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
                portalAfterClosing.PortalClosed();
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

using TMPro;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using FMODUnity;
using UnityEngine.UI;

public class KeyHasBeenSpawned : MonoBehaviour
{
    public static KeyHasBeenSpawned instance;

    [SerializeField] private Image glowImage;
    [SerializeField] private RectTransform glowImageTransform;

    private TMP_Text myText;
    private RectTransform myRectTransform;

    private WaitForSeconds timeBeforeAnimating= new WaitForSeconds(0.2f);

    [SerializeField] private EventReference appearSound;

    public bool imHere = false;

    private Color transparentWhite = new Color(1, 1, 1, 0);
    private Vector3 glowStartingSize = new Vector3(0.2f, 1, 1);

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

        myText = gameObject.GetComponent<TMP_Text>();
        myRectTransform = gameObject.GetComponent<RectTransform>();
    }

    private void Start()
    {
        myText.DOFade(0, 0);
        myText.enabled = false;
    }


    public void Animate()
    {
        RuntimeManager.PlayOneShot(appearSound);
        imHere = true;
        myText.enabled = true;
        myText.DOKill();
        myText.DOFade(0, 0);
        myRectTransform.localScale = Vector3.one;
        StartCoroutine(WaitAndAnimate());
        AnimateGlow();
    }


    private void AnimateGlow()
    {
        glowImage.enabled = true;

        glowImageTransform.localScale = glowStartingSize;
        glowImageTransform.DOScaleX(15, 1).SetEase(Ease.OutExpo);

        glowImage.color = transparentWhite;
        glowImage.DOFade(1, 0.1f).OnComplete(() =>
        {
            glowImage.DOFade(0, 1f).OnComplete(() =>
            {
                glowImage.enabled = false;
            });
        });
    }

    private IEnumerator WaitAndAnimate()
    {
        yield return timeBeforeAnimating;
        myText.DOFade(1, 0.32f).SetEase(Ease.InSine).OnComplete(FadeOut);
        myRectTransform.DOScale(1.1f, 3.2f);
    }

    private void FadeOut()
    {
        myText.DOFade(0, 3.2f).SetEase(Ease.InSine).OnComplete(() =>
        {
            myText.DOKill();
            myRectTransform.DOKill();
           myText.enabled=false;
            imHere = false;
        });
    }


}

using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Reborn : MonoBehaviour
{
    private TMP_Text myText;
    private RectTransform myRectTransform;

    private Tween myTween;

    private float howMuchToSpaceout = 4;

    private float animationTime = 1f;

    private EventReference rebornEvent;
    [SerializeField] private EventReference defaultSnapshot;
    private EventInstance snapshotInstance;

    private void Awake()
    {
        myText = GetComponent<TMP_Text>();
        myRectTransform = myText.rectTransform;
    }
    void Start()
    {
        myTween = DOTween.To(() => myText.characterSpacing, x => myText.characterSpacing = x, howMuchToSpaceout, animationTime).SetLoops(-1, LoopType.Yoyo);
        myTween.SetUpdate(UpdateType.Normal, true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            myTween.Kill();
            myTween = myRectTransform.DOScale(1.618f, 2).SetEase(Ease.OutExpo);
            myTween.SetUpdate(UpdateType.Normal, true);
            Color transparentMe = new Color(myText.color.r, myText.color.g, myText.color.b, 0);
            myTween = myText.DOColor(transparentMe, 2).SetEase(Ease.OutExpo).OnComplete(() =>
            {
                DOTween.KillAll();
                snapshotInstance = RuntimeManager.CreateInstance(defaultSnapshot);
                snapshotInstance.start();
                int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
                SceneManager.LoadScene(currentSceneIndex);
                Time.timeScale = 1;
            });
            myTween.SetUpdate(UpdateType.Normal, true);
            RuntimeManager.PlayOneShot(rebornEvent);
        }
    }



}

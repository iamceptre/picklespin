using DG.Tweening;
using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Reborn : MonoBehaviour
{
    private PortalAfterClosing portalAfterClosing;
    private TMP_Text myText;
    private RectTransform myRectTransform;

    private Tween myTween;

    private float howMuchToSpaceout = 4;

    private float animationTime = 1f;

    [SerializeField] private EventReference rebornEvent;

    [SerializeField] [Tooltip("if -1, it restarts the current scene")] private int sceneindex = -1;

    private SnapshotManager snapshotManager;

    [SerializeField] private UnityEvent OnClickEvent;

    private bool clickable = true;

    private void Awake()
    {
        myText = GetComponent<TMP_Text>();
        myRectTransform = myText.rectTransform;
    }
    void Start()
    {
        snapshotManager = SnapshotManager.instance;
        portalAfterClosing = PortalAfterClosing.instance;
        myTween = DOTween.To(() => myText.characterSpacing, x => myText.characterSpacing = x, howMuchToSpaceout, animationTime).SetLoops(-1, LoopType.Yoyo);
        myTween.SetUpdate(UpdateType.Normal, true);
        System.GC.Collect();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && clickable)
        {
            clickable = false;
            OnClickEvent.Invoke();
            myTween.Kill();
            myTween = myRectTransform.DOScale(1.618f, 2).SetEase(Ease.OutExpo);
            myTween.SetUpdate(UpdateType.Normal, true);
            Color transparentMe = new Color(myText.color.r, myText.color.g, myText.color.b, 0);
            RuntimeManager.PlayOneShot(rebornEvent);

            myTween = myText.DOColor(transparentMe, 2).SetEase(Ease.OutExpo).OnComplete(() =>
            {
                clickable = true;
                Time.timeScale = 1;
                DOTween.KillAll();
                FMODResetManager.instance.ResetFMOD(false);
                SetScene();
            });
            myTween.SetUpdate(UpdateType.Normal, true);
        }
    }


    private void SetScene()
    {
        snapshotManager.StopAllSnapshots();

        if (sceneindex == -1)
        {
            //Debug.Log("reloading current scene");
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentSceneIndex);
        }
        else
        {
            //Debug.Log("loading selected scene " + sceneindex);
            SceneManager.LoadScene(sceneindex);
        }
    }



}

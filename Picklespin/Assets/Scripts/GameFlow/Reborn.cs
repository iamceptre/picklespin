using DG.Tweening;
using FMODUnity;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Reborn : MonoBehaviour
{
    private TMP_Text myText;
    private RectTransform myRectTransform;

    private Tween myTween;

    private readonly float howMuchToSpaceout = 4;

    private readonly float animationTime = 1f;

    [SerializeField] private EventReference rebornEvent;

    [SerializeField][Tooltip("if -1, it restarts the current scene")] private int sceneindex = -1;

    private AudioSnapshotManager audioSnapshotManager;

    [SerializeField] private UnityEvent OnClickEvent;

    private bool clickable = true;

    private void Awake()
    {
        myText = GetComponent<TMP_Text>();
        myRectTransform = myText.rectTransform;
    }

    private void Start()
    {
        audioSnapshotManager = AudioSnapshotManager.Instance;
        myTween = DOTween.To(() => myText.characterSpacing, x => myText.characterSpacing = x, howMuchToSpaceout, animationTime).SetLoops(-1, LoopType.Yoyo);
        _ = myTween.SetUpdate(UpdateType.Normal, true);
        System.GC.Collect();
    }

    private void Update()
    {
        if (InputCompat.GetKeyDown(KeyCode.Return) && clickable)
        {
            clickable = false;
            OnClickEvent.Invoke();
            myTween.Kill();
            myTween = myRectTransform.DOScale(1.6f, 2).SetEase(Ease.OutExpo);
            _ = myTween.SetUpdate(UpdateType.Normal, true);
            Color transparentMe = new(myText.color.r, myText.color.g, myText.color.b, 0);
            RuntimeManager.PlayOneShot(rebornEvent);

            myTween = myText.DOColor(transparentMe, 2).SetEase(Ease.OutExpo).OnComplete(() =>
            {
                clickable = true;
                Time.timeScale = 1;
                _ = DOTween.KillAll();
                FMODResetManager.instance.ResetFMOD(false);
                StartCoroutine(SetSceneAtEndOfFrame());
            });
            _ = myTween.SetUpdate(UpdateType.Normal, true);
        }
    }

    // scene unload only applies next frame, so scripts running later this frame
    // (state ticks, eye/torch fades) can still spawn tweens whose targets die with
    // the scene — kill again at end of frame or DOTween logs "Tween startup failed"
    private IEnumerator SetSceneAtEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        _ = DOTween.KillAll();
        SetScene();
    }

    private void SetScene()
    {
        audioSnapshotManager.Clear();

        if (sceneindex == -1)
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentSceneIndex);
        }
        else
        {
            SceneManager.LoadScene(sceneindex);
        }
    }

}

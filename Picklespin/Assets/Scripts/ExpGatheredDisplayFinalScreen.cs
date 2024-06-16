using UnityEngine;
using DG.Tweening;
using TMPro;
using System.Text;

public class ExpGatheredDisplayFinalScreen : MonoBehaviour //THIS SHIT DOES SHIT
{
    private TMP_Text _text;
    [HideInInspector] public int currentlyAnimatedExp = 0;
    [SerializeField] private CanvasGroup[] _canvasGroup;
    [SerializeField] private GameObject inputPrompt;

    [SerializeField] private PlayerLevelDisplayFinalScreen playerLevelDisplayFinalScreen;

    private Tween myTween;

    [SerializeField] private float animationTime = 6;

    StringBuilder sb = new StringBuilder();

    private float timeScaleBefore;

    private void Awake()
    {
        _text = GetComponent<TMP_Text>();

        for (int i = 0; i < _canvasGroup.Length; i++)
        {
            _canvasGroup[i].alpha = 0;
        }
    }


    void Start()
    {
        timeScaleBefore = Time.timeScale;
        Pause.instance.PauseGame();
        Pause.instance.gameObject.SetActive(false);
        //Time.timeScale = timeScaleBefore;

         myTween = DOTween.To(() => currentlyAnimatedExp, x => currentlyAnimatedExp = x, PlayerEXP.instance.playerExpAmount, animationTime).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            FinishedAnimating();
        });

        myTween.SetUpdate(UpdateType.Normal, true);
    }

    
    void Update()
    {

        if (Input.anyKeyDown)
        {
            myTween.Kill();
            FinishedAnimating();
        }

        UpdateText();
    }

    private void FinishedAnimating()
    {
        //save the exp somewhere
        currentlyAnimatedExp = PlayerEXP.instance.playerExpAmount;
        UpdateText();
        inputPrompt.SetActive(true);
        playerLevelDisplayFinalScreen.FinishedAnimating();
        enabled = false;
    }

    private void UpdateText()
    {
        sb.Clear();
        sb.Append("you gathered<br>");
        sb.Append(currentlyAnimatedExp);
        sb.Append(" exp");
        _text.text = sb.ToString();
    }
}

using UnityEngine;
using FMODUnity;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerHP : MonoBehaviour
{
    public static PlayerHP instance { get; private set; }
    public int hp;
    public int maxHp;

    private Death death;

    [Header("Assets")]
    [SerializeField] private EventReference hurtSound;
    [SerializeField] private Image hurtOverlay;
    [SerializeField] private Sprite[] hurtOverlays;

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

    private void Start()
    {
        death = Death.instance;
        hurtOverlay.enabled = false;
    }


    public void CheckIfPlayerIsDead()
    {
        if (hp <= 0)
        {
            death.PlayerDeath();
        }
    }


    public void PlayerHurtSound()
    {
        RuntimeManager.PlayOneShot(hurtSound);
        PlayerHurtVisual();
    }

    public void PlayerHurtVisual()
    {
        hurtOverlay.enabled = true;
        hurtOverlay.sprite = hurtOverlays[Random.Range(0, hurtOverlays.Length)];
        hurtOverlay.DOKill();
        hurtOverlay.DOFade(0, 0);
        hurtOverlay.DOFade(0.6f, 0.1f).OnComplete(() =>
        {
            hurtOverlay.DOFade(0, 1).OnComplete(() =>
            {
                hurtOverlay.enabled = false;
            });
        });
    }

}

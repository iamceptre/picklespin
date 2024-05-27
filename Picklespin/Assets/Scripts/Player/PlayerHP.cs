using UnityEngine;
using FMODUnity;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerHP : MonoBehaviour
{
    private BarLightsAnimation barLightsAnimation;
    public static PlayerHP instance { get; private set; }
    public int hp;
    public int maxHp;

    private Death death;
    private HpBarDisplay hpBarDisplay;

    [Header("Assets")]
    [SerializeField] private EventReference hurtSound;
    [SerializeField] private Image hurtOverlay;
    [SerializeField] private Sprite[] hurtOverlays;
    [SerializeField] private EventReference hpAqquiredSound;

    public bool godMode = false;

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

    public void SetGodmode(bool isGodmode)
    {
        godMode = isGodmode;
    }

    private void Start()
    {
        death = Death.instance;
        hurtOverlay.enabled = false;
        barLightsAnimation = BarLightsAnimation.instance;
        hpBarDisplay = HpBarDisplay.instance;
    }


    private void CheckIfPlayerIsDead()
    {
        if (hp <= 0)
        {
            death.PlayerDeath();
        }
    }

    private void CheckIfPlayerIsLowHP()
    {

    }


    public void PlayerHurtSound() //initial signal goes here
    {
        if (!godMode) {
            RuntimeManager.PlayOneShot(hurtSound);
            PlayerHurtVisual();
            CheckIfPlayerIsDead();
        }
    }

    private void PlayerHurtVisual()
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

    public void GiveHPToPlayer(int howMuchHPIGive)
    {
        if (hp < maxHp)
        {

            if (hp + howMuchHPIGive < maxHp)
            {
                //Debug.Log("raz");
                hp += howMuchHPIGive;
                hpBarDisplay.Refresh(true);
                barLightsAnimation.PlaySelectedBarAnimation(0, howMuchHPIGive, false); //hp = 0, stamina = 1, mana = 2
            }
            else
            {
                //Debug.Log("dwa");
                hp = maxHp;
                barLightsAnimation.PlaySelectedBarAnimation(0, howMuchHPIGive, true);
            }

            hpBarDisplay.Refresh(true);
        }

        //RuntimeManager.PlayOneShot(hpAqquiredSound);
    }

}

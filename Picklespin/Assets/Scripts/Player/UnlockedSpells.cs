using UnityEngine;
using FMODUnity;
using UnityEngine.UI;
using DG.Tweening;

public class UnlockedSpells : MonoBehaviour
{
    [SerializeField] private Ammo ammo;
    [SerializeField] private AmmoDisplay ammoDisplay;
    [SerializeField] private ManaLightAnimation manaLightAnimation;

    [SerializeField] private RectTransform[] invSlotRect;
    [SerializeField] private Image[] spellIcon;

    [SerializeField] private Image spellLockedIcon;
    private RectTransform lockedRect;

    [SerializeField] private Image alreadyUnlockedIcon;
    private RectTransform alreadyUnlockedRect;

    public bool[] spellUnlocked;

    [SerializeField] private GameObject[] lockedSpellTint; //unserialize, get objects using Find Function

    [Header("Spell Unlock Light GUI")]
    [SerializeField] private Image spellUnlockedLight;
    private RectTransform lightRect;
    [SerializeField] private EventReference spellUnlockedSound;

    private void Awake()
    {
        lightRect = spellUnlockedLight.GetComponent<RectTransform>();
        lockedRect = spellLockedIcon.GetComponent<RectTransform>();
        alreadyUnlockedRect = alreadyUnlockedIcon.GetComponent<RectTransform>();
    }

    public void UnlockASpell(int spellID)
    {
        if (spellUnlocked[spellID]) {
            ManaBonus();
        }
        else
        {
            spellUnlocked[spellID] = true;

            lockedSpellTint[spellID].SetActive(false);
            RuntimeManager.PlayOneShot(spellUnlockedSound);

            spellUnlockedLight.enabled = true;
            spellUnlockLight();
            lightRect.anchoredPosition = invSlotRect[spellID].anchoredPosition;

            spellIcon[spellID].enabled = true;
            spellIconFadeIn(spellID);
        }

    }

    private void spellIconFadeIn(int spellID)
    {
        spellIcon[spellID].DOFade(0, 0);
        spellIcon[spellID].DOFade(1, 0.5f);
    }

    private void spellUnlockLight()
    {
        spellUnlockedLight.enabled = true;
        spellUnlockedLight.DOKill();
        lightRect.DOKill();
        lightRect.localScale = Vector3.zero;
        lightRect.DOScaleY(1f, 0.3f).SetEase(Ease.OutExpo);
        lightRect.DOScaleX(1f, 1).SetEase(Ease.OutExpo);
        spellUnlockedLight.DOFade(1, 0.1f).OnComplete(LightFadeOut);
    }

    private void LightFadeOut()
    {
        spellUnlockedLight.DOFade(0, 2.5f).OnComplete(DisableLight);
    }

    private void DisableLight()
    {
        spellUnlockedLight.enabled = false;
    }

    private void ManaBonus()
    {

        //gives 50 mana for now

            if (ammo.maxAmmo - ammo.ammo <= 50)
            {
                ammo.ammo = ammo.maxAmmo;

            }
            else
            {
                ammo.ammo += 50;
            }

            ammoDisplay.RefreshManaValueSmooth();
            manaLightAnimation.LightAnimation();
        
    }


    public void spellLockedIconAnimation(int spellID)
    {
        spellLockedIcon.enabled = true;
        spellLockedIcon.DOKill();
        lockedRect.DOKill();
        spellLockedIcon.color = new Color(255, 255, 255, 0);
        lockedRect.anchoredPosition = invSlotRect[spellID].anchoredPosition;
        lockedRect.localScale = new Vector2(0.25f, 0.25f);
        spellLockedIcon.DOFade(1, 0.2f).OnComplete(lockFadeOut);
        lockedRect.DOScale(0.4f, 0.7f);
    }

    private void lockFadeOut()
    {
        spellLockedIcon.DOFade(0, 0.5f).OnComplete(DisableLock);
    }

    private void DisableLock()
    {
        spellLockedIcon.enabled = false;
    }

    public void SelectingUnlockedAuraAnimation(int spellID)
    {
        alreadyUnlockedIcon.enabled = true;
        alreadyUnlockedIcon.DOKill();
        alreadyUnlockedRect.DOKill();
        alreadyUnlockedIcon.DOFade(0, 0);
        alreadyUnlockedRect.anchoredPosition = invSlotRect[spellID].anchoredPosition - new Vector2(1.5f,-1);
        alreadyUnlockedRect.localScale = new Vector2(0.5f, 0.5f);
        alreadyUnlockedIcon.DOFade(0.6f, 0.1f).OnComplete(AlreadyUnlockedFadeOut);
        alreadyUnlockedRect.DOScale(0.7f, 0.35f);
    }


    private void AlreadyUnlockedFadeOut()
    {
        alreadyUnlockedIcon.DOFade(0, 0.3f).OnComplete(DisableAlreadyUnlocked);
    }

    private void DisableAlreadyUnlocked()
    {
        alreadyUnlockedIcon.enabled = false;
    }

}

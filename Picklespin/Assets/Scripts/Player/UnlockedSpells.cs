using UnityEngine;
using FMODUnity;
using UnityEngine.UI;
using DG.Tweening;

public class UnlockedSpells : MonoBehaviour
{
    public static UnlockedSpells instance { get; private set; }

    private Ammo ammo;

    [SerializeField] private RectTransform[] invSlotRect;
    [SerializeField] private Image[] spellIcon;

    [SerializeField] private Image spellLockedIcon;
    private RectTransform lockedRect;

    [SerializeField] private Image alreadyUnlockedIcon;
    private RectTransform alreadyUnlockedRect;
    [SerializeField] private RectTransform currentlySelectedSlotIndicator;

    public bool[] spellUnlocked;

    [SerializeField] private GameObject[] lockedSpellTint;

    [Header("Spell Unlock Light GUI")]
    [SerializeField] private Image spellUnlockedLight;
    private RectTransform lightRect;

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

        lightRect = spellUnlockedLight.GetComponent<RectTransform>();
        lockedRect = spellLockedIcon.GetComponent<RectTransform>();
        alreadyUnlockedRect = alreadyUnlockedIcon.GetComponent<RectTransform>();
    }

    private void Start()
    {
        ammo = Ammo.instance;
    }

    public void UnlockASpell(int spellID)
    {
        if (spellUnlocked[spellID])
        {
            ammo.GiveManaToPlayer(50, false);
            RuntimeManager.PlayOneShot("event:/ITEMS/POTIONS/POTION_PICKUP_BASE_LAYER");
        }
        else
        {
            spellUnlocked[spellID] = true;

            lockedSpellTint[spellID].SetActive(false);

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
        lightRect.DOScaleY(1, 0.3f).SetEase(Ease.OutExpo);
        lightRect.DOScaleX(1, 1).SetEase(Ease.OutExpo);
        spellUnlockedLight.DOFade(1, 0.1f).OnComplete(LightFadeOut);
    }

    private void LightFadeOut()
    {
        spellUnlockedLight.DOFade(0, 4).OnComplete(DisableLight);
    }

    private void DisableLight()
    {
        spellUnlockedLight.enabled = false;
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
        alreadyUnlockedRect.anchoredPosition = invSlotRect[spellID].anchoredPosition - new Vector2(1.5f, -1);
        alreadyUnlockedRect.localScale = new Vector2(0.5f, 0.5f);
        alreadyUnlockedIcon.DOFade(0.6f, 0.1f).OnComplete(AlreadyUnlockedFadeOut);
        alreadyUnlockedRect.DOScale(0.7f, 0.35f);

        currentlySelectedSlotIndicator.DOMoveX(alreadyUnlockedRect.position.x, 0.1f).SetEase(Ease.OutExpo);
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

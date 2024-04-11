using UnityEngine;
using FMODUnity;
using UnityEngine.UI;
using DG.Tweening;

public class UnlockedSpells : MonoBehaviour
{
    [SerializeField] private RectTransform[] invSlotRect;
    [SerializeField] private Image[] spellIcon;

    [SerializeField] private Image spellLockedIcon;
    private RectTransform lockedRect;

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
    }

    public void UnlockASpell(int spellID)
    {
        spellUnlocked[spellID] = true;
        lockedSpellTint[spellID].SetActive(false);
        RuntimeManager.PlayOneShot(spellUnlockedSound);

        spellUnlockedLight.enabled = true;
        spellUnlockLight();
        lightRect.anchoredPosition = invSlotRect[spellID].anchoredPosition;

        spellIcon[spellID].enabled = true; //fade in
        spellIconFadeIn(spellID);
    }

    private void spellIconFadeIn(int spellID)
    {
        spellIcon[spellID].color = new Color(255,255,255,0);
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
        spellUnlockedLight.DOFade(0, 1.37f).OnComplete(DisableLight);
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

}

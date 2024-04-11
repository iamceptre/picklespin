using UnityEngine;
using FMODUnity;
using UnityEngine.UI;

public class UnlockedSpells : MonoBehaviour
{
    [SerializeField] private RectTransform[] invSlotRect;

    public bool[] spellUnlocked;

    [SerializeField] private GameObject[] lockedSpellTint; //unserialize, get objects using Find Function
                                                           // private RectTransform lockedSpellRect;

    [Header("Spell Unlock Light GUI")]
    [SerializeField] private Image spellUnlockedLight;
    private SpellUnlockedLightAnimation spellUnlockedLightAnimation;
    private RectTransform lightRect;
    [SerializeField] private EventReference spellUnlockedSound;

    private void Awake()
    {
        lightRect = spellUnlockedLight.GetComponent<RectTransform>();
        spellUnlockedLightAnimation = spellUnlockedLight.GetComponent<SpellUnlockedLightAnimation>();
    }

    public void UnlockASpell(int spellID)
    {
        spellUnlocked[spellID] = true;
        lockedSpellTint[spellID].SetActive(false);
        RuntimeManager.PlayOneShot(spellUnlockedSound);
        spellUnlockedLight.enabled = true;
        spellUnlockedLightAnimation.LightAnimation();
        lightRect.anchoredPosition = invSlotRect[spellID].anchoredPosition;
        //show icon of spell instead of ? sign
    }

}

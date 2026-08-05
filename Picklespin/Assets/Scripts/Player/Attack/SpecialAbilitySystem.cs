using FMODUnity;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpecialAbilitySystem : MonoBehaviour
{
    public static SpecialAbilitySystem instance { get; private set; }

    [Header("References")]
    [SerializeField] private AngelHeal angelHeal;
    [SerializeField] private SpecialAbilitySlot slot;
    [SerializeField] private HandShakeWhenCannotHeal handShake;
    [SerializeField] private GrapplingHook grapplingHook;

    [Header("Input")]
    [SerializeField, Tooltip("RMB - heals when aiming at an angel in reach, otherwise fires the class ability. Owned here, not by AngelHeal, so it keeps working while AngelHeal is disabled away from angels")]
    private InputActionReference useAction;

    [Header("Audio")]
    [SerializeField, Tooltip("played when the ability is triggered while it can't be used - same event as the locked-spell sound")]
    private EventReference lockedSoundEvent;

    [Header("Icons")]
    [SerializeField, Tooltip("shown on the slot whenever Angel Heal is possible right now - it overrides whatever the current class ability is")]
    private Sprite angelHealIcon;
    [SerializeField, Tooltip("which special ability each class currently grants, index = PlayerClassId - leave an entry None, or stop the array short, for classes without one")]
    private SpecialAbilityId[] classAbility;
    [SerializeField, Tooltip("icon per special ability, index = SpecialAbilityId - may stop short or be left empty, a missing one just shows no icon")]
    private Sprite[] abilityIcons;

    private Attack attack;
    private SpecialAbilityId currentAbility;
    private Sprite classIcon;
    private bool healOverriding;
    private bool shownUsable;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }
        instance = this;
    }

    private void OnEnable()
    {
        PlayerClasses.Changed += RefreshClassIcon;
        ClassUpgrades.LevelChanged += RefreshClassIcon;
        useAction.action.performed += OnUsePerformed;
        useAction.action.canceled += OnUseCanceled;
        useAction.action.Enable();
    }

    private void OnDisable()
    {
        PlayerClasses.Changed -= RefreshClassIcon;
        ClassUpgrades.LevelChanged -= RefreshClassIcon;
        useAction.action.performed -= OnUsePerformed;
        useAction.action.canceled -= OnUseCanceled;
        useAction.action.Disable();
    }

    private void Start()
    {
        attack = Attack.instance;
        RefreshClassIcon();
    }

    private void Update() => Refresh(false);

    private void OnUsePerformed(InputAction.CallbackContext ctx)
    {
        if (Time.timeScale == 0f || (attack && !attack.enabled)) return;

        if (grapplingHook && grapplingHook.CancelPull()) return;

        if (angelHeal && angelHeal.CanHealNow)
        {
            angelHeal.StartHealing();
            PlayUsedFeedback();
            return;
        }

        TryActivate();
    }

    private void OnUseCanceled(InputAction.CallbackContext ctx)
    {
        if (angelHeal && !angelHeal.IsBoosting) angelHeal.CancelHealing();
    }

    private void Refresh(bool force)
    {
        bool healPossible = angelHeal && angelHeal.CanHealNow;
        bool usable = healPossible || AbilityUsable;
        if (!force && healPossible == healOverriding && usable == shownUsable) return;

        healOverriding = healPossible;
        shownUsable = usable;
        slot.Assign(healPossible ? angelHealIcon : classIcon);
        slot.ApplyState(usable);
    }

    public void TryActivate()
    {
        if (!AbilityUsable)
        {
            if (handShake) handShake.ShakeHand();
            else PlayLockFeedback();
            return;
        }

        if (TryUseClassAbility()) PlayUsedFeedback();
        else if (handShake) handShake.Shake();
    }

    public void PlayLockFeedback()
    {
        slot.PlayDeny();
        RuntimeManager.PlayOneShot(lockedSoundEvent);
    }

    public void PlayUsedFeedback() => slot.PlaySelectedAura();

    private bool AbilityUsable => currentAbility switch
    {
        SpecialAbilityId.GrapplingHook => BlastfoolUpgrades.GrapplingHookUnlocked && grapplingHook && grapplingHook.IsReady,
        _ => false
    };

    private bool TryUseClassAbility() => currentAbility switch
    {
        SpecialAbilityId.GrapplingHook => grapplingHook.TryFire(),
        _ => false
    };

    private void RefreshClassIcon()
    {
        int classIndex = (int)PlayerClasses.Chosen;
        currentAbility = classAbility != null && classIndex >= 0 && classIndex < classAbility.Length
            ? classAbility[classIndex]
            : SpecialAbilityId.None;

        int iconIndex = (int)currentAbility;
        classIcon = currentAbility != SpecialAbilityId.None
                    && abilityIcons != null && iconIndex < abilityIcons.Length
            ? abilityIcons[iconIndex]
            : null;

        Refresh(true);
    }
}

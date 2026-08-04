using UnityEngine;

public class ClassUpgradeMenu : AngelChoiceMenu
{
    public static ClassUpgradeMenu Instance { get; private set; }

    [Header("Upgrade")]
    [SerializeField, Tooltip("line 2 - walk away and keep what you have; the same level is offered again three angels later")]
    private string skipMessage = "Not yet. I am deep enough.";
    [SerializeField, Tooltip("the note that fades in once the offer has been refused for want of EXP - {0} required, {1} carried, {2} missing. Nothing about EXP is shown before that; blank falls back to the built-in wording")]
    private string tooPoorFormat = DefaultTooPoorFormat;

    private const string DefaultTooPoorFormat = "The rite asks {0} EXP. You carry {1}. {2} short.";

    private const int OfferSlot = 0;
    private const int SkipSlot = 1;

    private bool thenAskForWish = true;

    protected override int SlotCount => 2;

    public bool CanOffer => IsWired && ClassUpgrades.IsLevelDue;

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        ClassUpgrades.ResetAll();

        base.Awake();
        if (!IsWired) return;

        Instance = this;
    }

    private void OnEnable() => PlayerClasses.Changed += ClassUpgrades.ClassChanged;

    private void OnDisable() => PlayerClasses.Changed -= ClassUpgrades.ClassChanged;

    public bool AskForUpgrade(bool followedByWish = true)
    {
        thenAskForWish = followedByWish;
        Ask();
        return IsAsking;
    }

    protected override bool RollOptions() => ClassUpgrades.Next != null;

    protected override string BuildLine(int slot)
    {
        if (slot == SkipSlot) return skipMessage;

        ClassUpgrade offered = ClassUpgrades.Next;
        return offered == null ? null : offered.Name + NameSeparator + offered.Effect;
    }

    protected override bool CanChoose(int slot) =>
        slot != OfferSlot || ClassUpgrades.CanAfford(ClassUpgrades.NextLevel);

    protected override void OnDenied(int slot)
    {
        int level = ClassUpgrades.NextLevel;
        string format = string.IsNullOrWhiteSpace(tooPoorFormat) ? DefaultTooPoorFormat : tooPoorFormat;

        ShowDenialNote(string.Format(format,
                                     ClassUpgrades.RequiredExp(level),
                                     ClassUpgrades.CarriedExp,
                                     ClassUpgrades.MissingExp(level)));
    }

    protected override void OnChosen(int slot)
    {
        if (slot == OfferSlot) ClassUpgrades.TakeNext();
    }

    protected override void AfterChoice() { }

    protected override void OnClosed()
    {
        if (thenAskForWish) HandOverToWishMenu();
        else LockPlayerControls(false);
    }
}

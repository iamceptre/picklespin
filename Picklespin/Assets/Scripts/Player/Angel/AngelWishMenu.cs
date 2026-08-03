using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerClassId;

public enum WishCategory
{
    Resources,
    Mobility,
    Dash,
    Restore,
    Spellcasting,
    Damage,
    RocketJump,
    Enemies,
    Experience
}

public class AngelWishMenu : AngelChoiceMenu
{
    public static AngelWishMenu Instance { get; private set; }

    [Header("Wishes")]
    [SerializeField, Tooltip("off = drop the flavour name and show only what the wish actually does")]
    private bool showWishNames = true;

    private const int WishSlots = 3;

    private class Wish
    {
        public WishCategory Category;
        public string Name;
        public string Effect;
        public Action Grant;
        public Func<bool> IsAvailable;

        public (PlayerClassId Class, int MaxTakes)[] Limits;
        public int TimesTaken;
    }

    private const int Unlimited = int.MaxValue;

    private readonly List<Wish> catalog = new();
    private readonly Wish[] offered = new Wish[WishSlots];
    private readonly List<WishCategory> categoryPool = new();
    private readonly List<Wish> candidateBuffer = new();

    protected override int SlotCount => WishSlots;

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        WishUpgrades.ResetAll();

        base.Awake();
        if (!IsWired) return;

        Instance = this;
        BuildCatalog();
    }

    public void AskForWish() => Ask();

    protected override string BuildLine(int slot)
    {
        Wish wish = offered[slot];
        if (wish == null) return null;

        return showWishNames && !string.IsNullOrEmpty(wish.Name)
            ? wish.Name + NameSeparator + wish.Effect
            : wish.Effect;
    }

    protected override void OnChosen(int slot)
    {
        Wish wish = offered[slot];
        if (wish == null) return;

        wish.TimesTaken++;
        wish.Grant();
    }

    protected override bool RollOptions()
    {
        Array.Clear(offered, 0, offered.Length);

        categoryPool.Clear();
        foreach (Wish wish in catalog)
        {
            if (wish.Category == WishCategory.Experience) continue;
            if (!IsOfferable(wish)) continue;
            if (!categoryPool.Contains(wish.Category)) categoryPool.Add(wish.Category);
        }

        for (int slot = 0; slot < WishSlots - 1 && categoryPool.Count > 0; slot++)
        {
            int pick = UnityEngine.Random.Range(0, categoryPool.Count);
            WishCategory category = categoryPool[pick];
            categoryPool.RemoveAt(pick);
            offered[slot] = PickFrom(category);
        }

        offered[WishSlots - 1] = PickFrom(WishCategory.Experience);

        return offered[0] != null || offered[1] != null || offered[2] != null;
    }

    private Wish PickFrom(WishCategory category)
    {
        candidateBuffer.Clear();
        foreach (Wish wish in catalog)
        {
            if (wish.Category == category && IsOfferable(wish)) candidateBuffer.Add(wish);
        }
        return candidateBuffer.Count == 0
            ? null
            : candidateBuffer[UnityEngine.Random.Range(0, candidateBuffer.Count)];
    }

    private static bool IsOfferable(Wish wish) =>
        wish.TimesTaken < MaxTakesFor(wish) && (wish.IsAvailable == null || wish.IsAvailable());

    private static int MaxTakesFor(Wish wish)
    {
        foreach ((PlayerClassId playerClass, int maxTakes) in wish.Limits)
        {
            if (playerClass == PlayerClasses.Chosen) return maxTakes;
        }
        return 0;
    }

    private IEnumerator FullRestoreRoutine()
    {
        var wait = new WaitForSeconds(0.2f);

        if (PlayerHP.Instance) PlayerHP.Instance.RestoreToFull();
        yield return wait;
        if (PlayerMovement.Instance)
        {
            PlayerMovement.Instance.GiveStaminaToPlayer(Mathf.CeilToInt(PlayerMovement.Instance.maxStamina));
        }
        yield return wait;
        if (Ammo.instance) Ammo.instance.GiveManaToPlayer(Ammo.instance.maxAmmo);
    }

    private static bool AnyBarNotFull()
    {

        if (PlayerHP.Instance && PlayerHP.Instance.HealthFraction < 1f) return true;
        if (Ammo.instance && Ammo.instance.ammo < Ammo.instance.maxAmmo) return true;
        return PlayerMovement.Instance && PlayerMovement.Instance.stamina < PlayerMovement.Instance.maxStamina;
    }

    private void Add(WishCategory category, string name, string effect, Action grant,
                     (PlayerClassId, int)[] limits, Func<bool> isAvailable = null)
    {
        catalog.Add(new Wish
        {
            Category = category,
            Name = name,
            Effect = effect,
            Grant = grant,
            Limits = limits,
            IsAvailable = isAvailable
        });
    }

    private static readonly PlayerClassId[] EveryClass =
        { None, Vesper, Lightfoot, Umbral, Blastfool, Bastion, Sanctus };

    private static (PlayerClassId, int)[] All(int maxTakes) => AllExcept(maxTakes);

    private static (PlayerClassId, int)[] AllExcept(int maxTakes, params PlayerClassId[] excluded)
    {
        var limits = new List<(PlayerClassId, int)>(EveryClass.Length);
        foreach (PlayerClassId playerClass in EveryClass)
        {
            if (Array.IndexOf(excluded, playerClass) < 0) limits.Add((playerClass, maxTakes));
        }
        return limits.ToArray();
    }

    private static (PlayerClassId, int)[] Only(int maxTakes, params PlayerClassId[] classes)
    {
        var limits = new (PlayerClassId, int)[classes.Length];
        for (int i = 0; i < classes.Length; i++) limits[i] = (classes[i], maxTakes);
        return limits;
    }

    private void BuildCatalog()
    {
        catalog.Clear();

        Add(WishCategory.Mobility, "Quicken my stride", "Max speed +10%",
            () => { if (PlayerMovement.Instance) PlayerMovement.Instance.MultiplyMaxSpeed(1.1f); },
            All(3));
        Add(WishCategory.Mobility, "Lighten my step", "Jump power +15%",
            () => { if (PlayerMovement.Instance) PlayerMovement.Instance.MultiplyJumpPower(1.15f); },
            All(2));

        Add(WishCategory.Resources, "Lengthen my breath", "Max stamina +10%",
            () => { if (PlayerMovement.Instance) PlayerMovement.Instance.MultiplyMaxStamina(1.1f); },
            new[] { (None, 7), (Vesper, 7), (Lightfoot, 7), (Blastfool, 1), (Bastion, 7), (Sanctus, 7) });
        Add(WishCategory.Resources, "Deepen my well", "Max magicka +10%",
            () => { if (Ammo.instance) Ammo.instance.MultiplyMaxMana(1.1f); },
            AllExcept(5, Lightfoot, Blastfool));
        Add(WishCategory.Resources, "Spare me the labour", "Magicka cost -5%",
            () => WishUpgrades.MultiplyMagickaCost(0.95f),
            Only(5, None, Lightfoot, Blastfool, Vesper, Sanctus));

        Add(WishCategory.Dash, "Sharpen my blink", "Dash speed and duration +15%",
            () => { if (PlayerDash) PlayerDash.MultiplyDashPower(1.15f); },
            Only(2, None, Lightfoot, Blastfool),
            () => PlayerDash);
        Add(WishCategory.Dash, "Widen my blink", "Dash-Stun radius +25%",
            () => { if (PlayerDash) PlayerDash.MultiplyDashRadius(1.25f); },
            All(2),
            () => PlayerDash);

        Add(WishCategory.Restore, "Make me whole", "Refill health, stamina and magicka",
            () => StartCoroutine(FullRestoreRoutine()),
            AllExcept(Unlimited, Vesper, Umbral),
            AnyBarNotFull);
        Add(WishCategory.Restore, "Mend my body", "Refill health",
            () => { if (PlayerHP.Instance) PlayerHP.Instance.RestoreToFull(); },
            AllExcept(Unlimited, Vesper, Umbral),
            () => PlayerHP.Instance && PlayerHP.Instance.HealthFraction < 1f);
        Add(WishCategory.Restore, "Fill my veins", "Refill magicka",
            RefillMagicka,
            Only(Unlimited, None, Vesper),
            MagickaNotFull);

        Add(WishCategory.Restore, "Fill the void", "Refill dark energy",
            RefillMagicka,
            Only(Unlimited, Umbral),
            MagickaNotFull);

        Add(WishCategory.Spellcasting, "Hasten my words", "Casting time -15%",
            () => WishUpgrades.MultiplyCastDuration(0.85f),
            AllExcept(3, Umbral));
        Add(WishCategory.Spellcasting, "Steady my hand", "Spell recoil -10%",
            () => WishUpgrades.MultiplyRecoil(0.9f),
            All(4));
        Add(WishCategory.Spellcasting, "Spare me the wait", "Cooldown -10%",
            () => WishUpgrades.MultiplyCooldown(0.9f),
            All(5));

        Add(WishCategory.Damage, "Feed the netherlight", "Netherlight damage +15%",
            () => WishUpgrades.MultiplySpellDamage(SpellId.Netherlight, 1.15f),
            AllExcept(2, Umbral));
        Add(WishCategory.Damage, "Stoke the flame", "Fireball damage +15%",
            () => WishUpgrades.MultiplySpellDamage(SpellId.Fireball, 1.15f),
            AllExcept(2, Umbral));
        Add(WishCategory.Damage, "Guide me", "Critical chance +10%",
            () => WishUpgrades.AddCriticalChance(0.1f),
            AllExcept(4, Sanctus));

        Add(WishCategory.RocketJump, "Spare me the blast", "No rocket jump self-damage",
            WishUpgrades.DisableRocketJumpSelfDamage,
            AllExcept(1, Blastfool),
            () => WishUpgrades.RocketJumpSelfDamage);
        Add(WishCategory.RocketJump, "Double the blast", "Rocket jump force +100%",
            () => WishUpgrades.MultiplyRocketJumpForce(2f),
            AllExcept(2, Blastfool));

        Add(WishCategory.Enemies, "Shackle their feet", "Enemy speed -20%",
            () => WishUpgrades.MultiplyEnemySpeed(0.8f),
            AllExcept(1, Lightfoot));

        AddExpWish(111);
        AddExpWish(222);
        AddExpWish(333);
        AddExpWish(444);
        AddExpWish(555);
        AddExpWish(777);
        AddExpWish(888);
        Add(WishCategory.Experience, "Sharpen my eye", "Experience gathered +5%",
            () => WishUpgrades.MultiplyExpGather(1.05f),
            All(3));
    }

    private void AddExpWish(int amount)
    {
        Add(WishCategory.Experience, "Grant me insight", $"{amount} EXP",
            () => { if (PlayerEXP.instance) PlayerEXP.instance.GivePlayerExp(amount, "Angel's Wish"); },
            All(Unlimited));
    }

    private static void RefillMagicka()
    {
        if (Ammo.instance) Ammo.instance.GiveManaToPlayer(Ammo.instance.maxAmmo - Ammo.instance.ammo);
    }

    private static bool MagickaNotFull() => Ammo.instance && Ammo.instance.ammo < Ammo.instance.maxAmmo;
}

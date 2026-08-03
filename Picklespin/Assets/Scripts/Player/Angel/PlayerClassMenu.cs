using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClassMenu : AngelChoiceMenu
{
    public static PlayerClassMenu Instance { get; private set; }

    [Header("Classes")]
    [SerializeField, Tooltip("line 4 - the refusal, which costs nothing and closes the menu")]
    private string skipMessage = "Nothing. I am as I was.";
    [SerializeField, Tooltip("Umbral's black shell - the only spell they get. Missing from Attack.bulletPrefab = Umbral keeps the normal spells.")]
    private SpellId umbralSpell = SpellId.Sin;

    private const int ClassSlots = 3;
    private const int SkipSlot = ClassSlots;

    private class PlayerClassOption
    {
        public PlayerClassId Id;
        public string Name;
        public string Effect;
    }

    private readonly List<PlayerClassOption> catalog = new();
    private readonly PlayerClassOption[] offered = new PlayerClassOption[ClassSlots];
    private readonly List<PlayerClassOption> drawPile = new();

    protected override int SlotCount => ClassSlots + 1;

    public bool CanOffer => IsWired && !PlayerClasses.WasOffered;

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        PlayerClasses.ResetAll();

        base.Awake();
        if (!IsWired) return;

        Instance = this;
        BuildCatalog();
    }

    public void AskForClass()
    {
        if (!CanOffer) return;

        Ask();

        if (IsAsking) DevLog.Info($"{nameof(PlayerClassMenu)} is open: press R to draw three other classes", this);
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    protected override void DebugKeys()
    {
        if (InputCompat.GetKeyDown(KeyCode.R)) Reroll();
    }
#endif

    protected override bool RollOptions()
    {
        Array.Clear(offered, 0, offered.Length);

        drawPile.Clear();
        drawPile.AddRange(catalog);

        for (int slot = 0; slot < ClassSlots && drawPile.Count > 0; slot++)
        {
            int pick = UnityEngine.Random.Range(0, drawPile.Count);
            offered[slot] = drawPile[pick];
            drawPile.RemoveAt(pick);
        }

        return true;
    }

    protected override string BuildLine(int slot)
    {
        if (slot == SkipSlot) return skipMessage;
        PlayerClassOption option = offered[slot];
        return option == null ? null : option.Name + NameSeparator + option.Effect;
    }

    protected override void OnChosen(int slot)
    {
        if (slot == SkipSlot)
        {
            PlayerClasses.Skip();
            return;
        }

        Take(offered[slot]);
    }

    public bool Take(PlayerClassId id)
    {
        foreach (PlayerClassOption option in catalog)
        {
            if (option.Id != id) continue;
            Take(option);
            return true;
        }
        return false;
    }

    private void Take(PlayerClassOption option)
    {
        if (option == null) return;

        PlayerClasses.Choose(option.Id, option.Id == PlayerClassId.Umbral ? LockUmbralSpell() : null);
    }

    protected override void AfterChoice() { }

    protected override void OnClosed()
    {
        if (AngelWishMenu.Instance)
        {
            AngelWishMenu.Instance.AskForWish();

            if (AngelWishMenu.Instance.IsAsking) return;
        }

        LockPlayerControls(false);
    }

    private SpellId? LockUmbralSpell()
    {
        if (!PlayerAttack || !PlayerAttack.LockToSpell(umbralSpell)) return null;
        return umbralSpell;
    }

    private void Add(PlayerClassId id, string name, string effect)
    {
        catalog.Add(new PlayerClassOption { Id = id, Name = name, Effect = effect });
    }

    private void BuildCatalog()
    {
        catalog.Clear();

        Add(PlayerClassId.Vesper, "<b>Vesper</b>",
            "no flesh to lose - magicka is your life");

        Add(PlayerClassId.Lightfoot, "<b>Lightfoot</b>",
            "speed becomes damage - dash farther, go faster, you feel weak");

        Add(PlayerClassId.Umbral, "<b>Umbral</b>",
            "one black bar, one black spell - you thrive while the dark runs deep");

        Add(PlayerClassId.Blastfool, "<b>Blastfool</b>",
            "rocket-jumps throw you far and hurt you more - deal real damage only when airborne");

        Add(PlayerClassId.Bastion, "<b>Bastion</b>",
            "flesh enough for two, shots that run men through and hit twice as hard - slow to ready, slow on your feet");

        Add(PlayerClassId.Sanctus, "<b>Sanctus</b>",
            "your light turns enemies to your side and sends them where it lands - your own spells barely sting");
    }
}

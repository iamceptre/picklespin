using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClassMenu : AngelChoiceMenu
{
    public static PlayerClassMenu Instance { get; private set; }

    [Header("Classes")]
    [SerializeField, Tooltip("line 4 - the refusal, which costs nothing and closes the menu")]
    private string skipMessage = "Nothing. I am as I was.";
    [SerializeField, Tooltip("index into Attack.bulletPrefab of Umbral's black shell - the only spell they get. Out of range = Umbral keeps the normal spells.")]
    private int umbralSpellIndex = 3;

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

        // once per run, and ahead of the wiring check: even an unwired menu has to
        // leave behind a class a scene reload carried over
        PlayerClasses.ResetAll();

        base.Awake();
        if (!IsWired) return; // leave Instance null: AngelHeal then goes straight to the wish

        Instance = this;
        BuildCatalog();
    }

    public void AskForClass()
    {
        if (CanOffer) Ask();
    }

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

    // the class cheat comes through here, so what it hands out can never drift from
    // what the angel does. False = no such class in the catalog.
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

        // the stat changes the class carries come with Choose - the menu only names them
        PlayerClasses.Choose(option.Id, option.Id == PlayerClassId.Umbral ? LockUmbralSpell() : -1);
    }

    // controls stay locked through the fade - the wish menu takes them next
    protected override void AfterChoice() { }

    protected override void OnClosed()
    {
        if (AngelWishMenu.Instance)
        {
            AngelWishMenu.Instance.AskForWish();
            // it declines to open with nothing left to grant, and only the menu
            // that is up may hold the controls
            if (AngelWishMenu.Instance.IsAsking) return;
        }

        LockPlayerControls(false);
    }

    private int LockUmbralSpell()
    {
        return PlayerAttack && PlayerAttack.LockToSpell(umbralSpellIndex) ? umbralSpellIndex : -1;
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
            "speed becomes damage - dash farther, break sooner, your feel weak");

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

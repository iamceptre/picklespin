using System.Collections.Generic;
using FMODUnity;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class AngelChoiceMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField, Tooltip("the shared angel menu - one canvas and one set of lines for every choice menu; found in the scene if left empty")]
    private AngelChoiceUi ui;

    [Header("Presentation")]
    [SerializeField] private string promptMessage = "Speak, and it shall be granted.";
    [SerializeField, Tooltip("optional - left empty means no sound")]
    private EventReference chosenSound;
    [SerializeField, Tooltip("optional - played when a line is picked that cannot be taken, e.g. an upgrade there is not enough EXP for. The menu stays open")]
    private EventReference deniedSound;
    [SerializeField, Tooltip("AudioSnapshotManager key held while the menu is up - the short key it is registered under, not the FMOD path; empty means no snapshot")]
    private string snapshotKey = "AngelChoice";

    private Attack attack;
    private Dash dash;

    private static readonly Dictionary<string, int> snapshotHolders = new();
    private bool holdsSnapshot;

    public bool IsAsking { get; private set; }

    protected bool IsWired { get; private set; }

    protected string NameSeparator => ui.NameSeparator;
    protected Attack PlayerAttack => attack;
    protected Dash PlayerDash => dash;

    protected abstract int SlotCount { get; }

    protected abstract bool RollOptions();

    protected abstract string BuildLine(int slot);

    protected abstract void OnChosen(int slot);

    protected virtual bool CanChoose(int slot) => true;

    protected virtual void OnDenied(int slot) { }

    protected void ShowDenialNote(string text) => ui.FadeInNote(text);

    protected virtual void AfterChoice() => LockPlayerControls(false);

    protected virtual void OnClosed() { }

    protected virtual void Awake()
    {
        if (!ui) ui = FindFirstObjectByType<AngelChoiceUi>(FindObjectsInactive.Include);

        IsWired = ui && ui.CanShow(SlotCount);
        if (!IsWired)
        {
            DevLog.Error($"{GetType().Name}: needs an {nameof(AngelChoiceUi)} with a CanvasGroup and {SlotCount} option lines. " +
                           "This menu stays disabled until then.", this);
            enabled = false;
            return;
        }

        ui.HideImmediate();
    }

    protected virtual void Start()
    {
        attack = Attack.instance;
        dash = Dash.Instance;
    }

    private void Update()
    {
        if (!IsAsking) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        DebugKeys();
#endif

        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            int pointedAt = ui.SlotUnderPointer(mouse.position.ReadValue(), SlotCount);
            ui.Hover(pointedAt);

            if (pointedAt >= 0 && mouse.leftButton.wasPressedThisFrame)
            {
                Choose(pointedAt);
                return;
            }
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        int slots = Mathf.Min(SlotCount, ui.LineCount);
        for (int i = 0; i < slots; i++)
        {
            if (keyboard[Key.Digit1 + i].wasPressedThisFrame || keyboard[Key.Numpad1 + i].wasPressedThisFrame)
            {
                Choose(i);
                return;
            }
        }
    }

    public void Ask()
    {
        if (IsAsking || !IsWired) return;
        if (ui.IsClosing) ui.CompleteClose();
        if (IsAsking || (ui.ActiveMenu && ui.ActiveMenu != this)) return;
        if (!RollOptions()) return;

        ui.Claim(this);
        ui.ShowPrompt(promptMessage);
        WriteLines();

        IsAsking = true;
        PauseGate.Block(this);
        LockPlayerControls(true);
        HoldSnapshot();

        ui.FadeIn();
    }

    private void WriteLines()
    {
        ui.Hover(-1);

        for (int i = 0; i < ui.LineCount; i++)
        {
            ui.WriteLine(i, i < SlotCount ? BuildLine(i) : null);
        }
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    protected virtual void DebugKeys() { }

    protected void Reroll()
    {
        if (!IsAsking || !RollOptions()) return;

        WriteLines();
    }
#endif

    private void Choose(int slot)
    {
        if (!IsAsking || ui.IsClosing || !ui.IsLineActive(slot)) return;

        if (!CanChoose(slot))
        {
            ui.Deny(slot);
            if (!deniedSound.IsNull) RuntimeManager.PlayOneShot(deniedSound);
            OnDenied(slot);
            return;
        }

        IsAsking = false;
        ui.Hover(-1);
        OnChosen(slot);
        if (!chosenSound.IsNull) RuntimeManager.PlayOneShot(chosenSound);
        AfterChoice();
        ui.HighlightChosen(slot, FinishClosing);
    }

    private void FinishClosing()
    {
        OnClosed();
        ReleaseSnapshot();
        PauseGate.Release(this);
    }

    private void HoldSnapshot()
    {
        if (holdsSnapshot || string.IsNullOrEmpty(snapshotKey)) return;

        holdsSnapshot = true;
        snapshotHolders.TryGetValue(snapshotKey, out int held);
        snapshotHolders[snapshotKey] = held + 1;
        if (held == 0 && AudioSnapshotManager.Instance)
        {
            AudioSnapshotManager.Instance.EnableSnapshot(snapshotKey);
        }
    }

    private void ReleaseSnapshot()
    {
        if (!holdsSnapshot) return;

        holdsSnapshot = false;
        snapshotHolders.TryGetValue(snapshotKey, out int held);
        held = Mathf.Max(0, held - 1);
        snapshotHolders[snapshotKey] = held;
        if (held == 0 && AudioSnapshotManager.Instance)
        {
            AudioSnapshotManager.Instance.DisableSnapshot(snapshotKey);
        }
    }

    private void OnDestroy()
    {
        ReleaseSnapshot();
        PauseGate.Release(this);
    }

    protected void HandOverToWishMenu()
    {
        if (AngelWishMenu.Instance) AngelWishMenu.Instance.AskForWish();

        LockPlayerControls(false);
    }

    protected void LockPlayerControls(bool locked) => PlayerControlLock.Set(this, locked);
}

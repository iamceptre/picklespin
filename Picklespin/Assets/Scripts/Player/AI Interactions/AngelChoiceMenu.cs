using DG.Tweening;
using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public abstract class AngelChoiceMenu : MonoBehaviour //ABSTRACTION THAT BOTH CLASS SELECTION AND ANGEL WISH INHERITS
{
    [Header("UI")]
    [SerializeField, FormerlySerializedAs("wishCanvas")] private Canvas menuCanvas;
    [SerializeField, FormerlySerializedAs("wishCanvasGroup")] private CanvasGroup menuCanvasGroup;
    [SerializeField, Tooltip("the angel's question - optional")]
    private TMP_Text promptText;
    [SerializeField, Tooltip("one line per slot, top to bottom; chosen with keys 1, 2, 3...")]
    private TMP_Text[] optionLines = new TMP_Text[3];

    [Header("Presentation")]
    [SerializeField] private string promptMessage = "Speak, and it shall be granted.";
    [SerializeField, Tooltip("separator between the flavour name and the effect")]
    private string nameSeparator = " - ";
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField, Tooltip("how long the picked line stays highlighted before the menu fades")]
    private float highlightDuration = 0.35f;
    [SerializeField] private Color highlightColor = new(1f, 0.92f, 0.55f);
    [SerializeField, FormerlySerializedAs("wishGrantedSound"), Tooltip("optional - left empty means no sound")]
    private EventReference chosenSound;

    [Header("Controls locked while choosing (auto-found if left empty)")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Attack attack;
    [SerializeField] private SpellSelector spellSelector;
    [SerializeField] private Dash dash;
    [SerializeField] private MouselookXY mouselook;

    private Color[] lineStartColors;
    private int hoveredSlot = -1;

    public bool IsAsking { get; private set; }

    // false when nothing was wired: a menu that can never be answered would strip
    // the player of movement and attacking for the rest of the run
    protected bool IsWired { get; private set; }

    protected string NameSeparator => nameSeparator;
    protected Attack PlayerAttack => attack;
    protected Dash PlayerDash => dash;

    protected abstract int SlotCount { get; }

    /// picks what to offer this time; false = nothing to offer, so the menu stays shut
    protected abstract bool RollOptions();

    /// the text for a slot, without its number. Null or empty hides that line.
    protected abstract string BuildLine(int slot);

    /// grant the choice. Only ever called with a slot whose line was shown.
    protected abstract void OnChosen(int slot);

    /// runs the moment the choice is taken, before the fade-out
    protected virtual void AfterChoice() => LockPlayerControls(false);

    protected virtual void OnClosed() { }

    protected virtual void Awake()
    {
        IsWired = menuCanvasGroup && optionLines != null && optionLines.Length >= SlotCount;
        if (!IsWired)
        {
            Debug.LogError($"{GetType().Name}: assign the CanvasGroup and {SlotCount} option lines. " +
                           "This menu stays disabled until then.", this);
            enabled = false;
            return;
        }

        lineStartColors = new Color[optionLines.Length];
        for (int i = 0; i < optionLines.Length; i++)
        {
            if (optionLines[i]) lineStartColors[i] = optionLines[i].color;
        }

        HideImmediate();
    }

    protected virtual void Start()
    {
        if (!playerMovement) playerMovement = PlayerMovement.Instance;
        if (!attack) attack = Attack.instance;
        if (!spellSelector) spellSelector = FindFirstObjectByType<SpellSelector>();
        if (!dash) dash = FindFirstObjectByType<Dash>();
        if (!mouselook) mouselook = MouselookXY.instance;
    }

    private void Update()
    {
        if (!IsAsking) return;

        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            int pointedAt = SlotUnderPointer(mouse.position.ReadValue());
            Hover(pointedAt);

            if (pointedAt >= 0 && mouse.leftButton.wasPressedThisFrame)
            {
                Choose(pointedAt);
                return;
            }
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        int slots = Mathf.Min(SlotCount, optionLines.Length);
        for (int i = 0; i < slots; i++)
        {
            if (keyboard[Key.Digit1 + i].wasPressedThisFrame || keyboard[Key.Numpad1 + i].wasPressedThisFrame)
            {
                Choose(i);
                return;
            }
        }
    }

    // hit-tested against the lines themselves: no EventSystem, no raycaster and no
    // per-line component to wire, and the lines need not be raycast targets
    private int SlotUnderPointer(Vector2 screenPoint)
    {
        Camera camera = menuCanvas && menuCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? menuCanvas.worldCamera
            : null;

        int slots = Mathf.Min(SlotCount, optionLines.Length);
        for (int i = 0; i < slots; i++)
        {
            TMP_Text line = optionLines[i];
            if (line && line.enabled &&
                RectTransformUtility.RectangleContainsScreenPoint(line.rectTransform, screenPoint, camera))
            {
                return i;
            }
        }
        return -1;
    }

    private void Hover(int slot)
    {
        if (slot == hoveredSlot) return;

        if (hoveredSlot >= 0 && optionLines[hoveredSlot]) optionLines[hoveredSlot].color = lineStartColors[hoveredSlot];
        hoveredSlot = slot;
        if (hoveredSlot < 0 || !optionLines[hoveredSlot]) return;

        // softer than the colour the chosen line takes, so the two never read alike
        optionLines[hoveredSlot].color = Color.Lerp(lineStartColors[hoveredSlot], highlightColor, 0.5f);
    }

    public void Ask()
    {
        if (IsAsking || !IsWired) return;
        if (!RollOptions()) return;

        if (promptText) promptText.text = promptMessage;

        for (int i = 0; i < optionLines.Length; i++)
        {
            TMP_Text line = optionLines[i];
            if (!line) continue;

            line.DOKill();
            line.color = lineStartColors[i];
            line.rectTransform.DOKill();
            line.rectTransform.localScale = Vector3.one;

            string text = i < SlotCount ? BuildLine(i) : null;
            line.enabled = !string.IsNullOrEmpty(text);
            if (line.enabled) line.text = $"{i + 1}.  {text}";
        }

        hoveredSlot = -1;
        IsAsking = true;
        LockPlayerControls(true);

        menuCanvasGroup.DOKill();
        menuCanvasGroup.alpha = 0f;
        if (menuCanvas) menuCanvas.enabled = true;
        // unscaled: a menu can be up when something else freezes time
        menuCanvasGroup.DOFade(1f, fadeInDuration).SetUpdate(true);
    }

    private void Choose(int slot)
    {
        if (slot >= optionLines.Length || !optionLines[slot] || !optionLines[slot].enabled) return;

        IsAsking = false;
        Hover(-1); // before the highlight, or the hover colour would be what fades out
        OnChosen(slot);
        if (!chosenSound.IsNull) RuntimeManager.PlayOneShot(chosenSound);
        AfterChoice();
        HighlightChosen(slot);
    }

    private void HighlightChosen(int slot)
    {
        for (int i = 0; i < optionLines.Length; i++)
        {
            TMP_Text line = optionLines[i];
            if (!line || !line.enabled) continue;

            if (i == slot)
            {
                line.DOColor(highlightColor, highlightDuration * 0.4f).SetUpdate(true);
                line.rectTransform.DOScale(1.12f, highlightDuration * 0.4f).SetEase(Ease.OutBack).SetUpdate(true);
            }
            else
            {
                line.DOFade(0f, highlightDuration).SetUpdate(true);
            }
        }

        menuCanvasGroup.DOKill();
        menuCanvasGroup.DOFade(0f, fadeOutDuration)
                       .SetDelay(highlightDuration)
                       .SetUpdate(true)
                       .OnComplete(() =>
                       {
                           HideImmediate();
                           OnClosed();
                       });
    }

    protected void HideImmediate()
    {
        menuCanvasGroup.DOKill();
        menuCanvasGroup.alpha = 0f;
        if (menuCanvas) menuCanvas.enabled = false;
    }

    protected void LockPlayerControls(bool locked)
    {
        if (playerMovement) playerMovement.enabled = !locked;
        if (attack) attack.enabled = !locked;
        if (spellSelector) spellSelector.enabled = !locked;
        if (dash) dash.enabled = !locked;
        if (mouselook) mouselook.enabled = !locked;

        // the lines can be clicked, so the pointer comes back for as long as they are up
        Cursor.lockState = locked ? CursorLockMode.Confined : CursorLockMode.Locked;
        Cursor.visible = locked;
    }
}

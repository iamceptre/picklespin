using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class AngelChoiceUi : MonoBehaviour
{
    [Header("UI")]
    public Canvas menuCanvas;
    [SerializeField] private CanvasGroup menuCanvasGroup;
    [SerializeField, Tooltip("the angel's question - optional")]
    private TMP_Text promptText;
    [SerializeField, Tooltip("one line per slot, top to bottom; chosen with keys 1, 2, 3... - the wish menu leaves the fourth line empty")]
    private TMP_Text[] optionLines = new TMP_Text[4];
    [SerializeField, Tooltip("optional - the line that stays blank until a choice is refused, then fades in to say why (the EXP a class upgrade still wants). Left empty, the first option line the open menu is not using takes the message instead")]
    private TMP_Text noteText;

    [Header("Presentation")]
    [SerializeField, Tooltip("separator between the flavour name and the effect")]
    private string nameSeparator = " - ";
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField, Tooltip("how long the picked line stays highlighted before the menu fades")]
    private float highlightDuration = 0.35f;
    [SerializeField] private Color highlightColor = GameColors.Highlight;

    [Header("Refusal")]
    [SerializeField, Tooltip("what a refused line flashes - it shakes and returns to its own colour afterwards")]
    private Color denyColor = GameColors.Health;
    [SerializeField, Tooltip("how long the refused line flashes and shakes for")]
    private float denyDuration = 0.4f;
    [SerializeField, Tooltip("how far the refused line shakes sideways")]
    private float denyShakeStrength = 18f;
    [SerializeField, Tooltip("how long the note takes to fade in once a choice has been refused")]
    private float noteFadeDuration = 2f;

    private Color[] lineStartColors;
    private Vector2[] lineHomePositions;
    private Color noteStartColor = Color.white;
    private TMP_Text activeNote;
    private float denyUntil;
    private int hoveredSlot = -1;
    private Action closing;

    public static AngelChoiceUi Instance { get; private set; }

    public string NameSeparator => nameSeparator;
    public int LineCount => optionLines == null ? 0 : optionLines.Length;
    public bool IsClosing => closing != null;
    public AngelChoiceMenu ActiveMenu { get; private set; }
    public bool IsChoosing => ActiveMenu && ActiveMenu.IsAsking;

    private void Awake()
    {
        Instance = this;
        CaptureLineDefaults();
        if (noteText) noteStartColor = Visible(noteText.color);
        HideNote();
    }

    public bool CanShow(int slotCount) => menuCanvasGroup && LineCount >= slotCount;

    public void Claim(AngelChoiceMenu menu) => ActiveMenu = menu;

    public void ShowPrompt(string message)
    {
        if (promptText) promptText.text = message;
    }

    public void WriteLine(int slot, string text)
    {
        if (slot < 0 || slot >= LineCount) return;

        TMP_Text line = optionLines[slot];
        if (!line) return;

        ResetLine(slot);

        line.enabled = !string.IsNullOrEmpty(text);
        if (line.enabled) line.text = $"{slot + 1}.  {text}";
    }

    private void ResetLine(int slot)
    {
        TMP_Text line = optionLines[slot];
        if (!line) return;

        CaptureLineDefaults();
        line.DOKill();
        line.color = lineStartColors[slot];

        RectTransform rect = line.rectTransform;
        rect.DOKill();
        rect.localScale = Vector3.one;
        rect.anchoredPosition = lineHomePositions[slot];
    }

    public bool IsLineActive(int slot) =>
        slot >= 0 && slot < LineCount && optionLines[slot] && optionLines[slot].enabled;

    public int SlotUnderPointer(Vector2 screenPoint, int slotCount)
    {
        Camera camera = menuCanvas && menuCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? menuCanvas.worldCamera
            : null;

        int slots = Mathf.Min(slotCount, LineCount);
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

    public void Hover(int slot)
    {
        if (Time.unscaledTime < denyUntil || slot == hoveredSlot) return;

        CaptureLineDefaults();
        if (hoveredSlot >= 0 && optionLines[hoveredSlot]) optionLines[hoveredSlot].color = lineStartColors[hoveredSlot];
        hoveredSlot = slot;
        if (hoveredSlot < 0 || !optionLines[hoveredSlot]) return;

        optionLines[hoveredSlot].color = Color.Lerp(lineStartColors[hoveredSlot], highlightColor, 0.5f);
    }

    public void FadeIn()
    {
        HideNote();
        menuCanvasGroup.DOKill();
        menuCanvasGroup.alpha = 0f;
        if (menuCanvas) menuCanvas.enabled = true;

        menuCanvasGroup.DOFade(1f, fadeInDuration).SetUpdate(true);
    }

    public void Deny(int slot)
    {
        if (slot < 0 || slot >= LineCount) return;

        TMP_Text line = optionLines[slot];
        if (!line) return;

        ResetLine(slot);
        hoveredSlot = -1;
        denyUntil = Time.unscaledTime + denyDuration;

        line.DOColor(denyColor, denyDuration * 0.5f).SetLoops(2, LoopType.Yoyo).SetUpdate(true);
        line.rectTransform.DOShakeAnchorPos(denyDuration, new Vector2(denyShakeStrength, 0f), 18, 90f, false, true,
                                            ShakeRandomnessMode.Harmonic).SetUpdate(true);
    }

    public void FadeInNote(string text)
    {
        if (!activeNote) activeNote = ResolveNote();
        if (!activeNote)
        {
            DevLog.Warn($"{nameof(AngelChoiceUi)}: a choice was refused but there is nowhere to say why - " +
                        $"assign {nameof(noteText)}, or leave one of the option lines free.", this);
            return;
        }

        activeNote.text = text;
        if (activeNote.enabled) return;

        activeNote.DOKill();
        activeNote.enabled = true;
        activeNote.color = noteStartColor.WithAlpha(0f);
        activeNote.DOFade(noteStartColor.a, noteFadeDuration).SetUpdate(true);
    }

    private static Color Visible(Color color) => color.a < 0.01f ? color.WithAlpha(1f) : color;

    private TMP_Text ResolveNote()
    {
        if (noteText) return noteText;

        CaptureLineDefaults();
        for (int i = 0; i < LineCount; i++)
        {
            if (!optionLines[i] || optionLines[i].enabled) continue;

            noteStartColor = Visible(lineStartColors[i]);
            return optionLines[i];
        }
        return null;
    }

    public void HideNote()
    {
        TMP_Text note = activeNote ? activeNote : noteText;
        activeNote = null;
        if (!note) return;

        note.DOKill();
        note.text = string.Empty;
        note.color = noteStartColor;
        note.enabled = false;
    }

    public void HighlightChosen(int slot, Action onClosed)
    {
        for (int i = 0; i < LineCount; i++)
        {
            TMP_Text line = optionLines[i];
            if (!line || !line.enabled) continue;

            ResetLine(i);

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
        menuCanvasGroup.alpha = 1f;
        if (menuCanvas) menuCanvas.enabled = true;

        closing = onClosed;
        menuCanvasGroup.DOFade(0f, fadeOutDuration)
                       .SetDelay(highlightDuration)
                       .SetUpdate(true)
                       .OnComplete(CompleteClose);
    }

    public void CompleteClose()
    {
        Action onClosed = closing;
        HideImmediate();
        onClosed?.Invoke();
    }

    public void HideImmediate()
    {
        closing = null;
        ActiveMenu = null;
        hoveredSlot = -1;
        denyUntil = 0f;
        HideNote();

        if (!menuCanvasGroup) return;

        menuCanvasGroup.DOKill();
        menuCanvasGroup.alpha = 0f;
        if (menuCanvas) menuCanvas.enabled = false;
    }

    private void CaptureLineDefaults()
    {
        if (lineStartColors != null && lineStartColors.Length == LineCount) return;

        lineStartColors = new Color[LineCount];
        lineHomePositions = new Vector2[LineCount];
        for (int i = 0; i < LineCount; i++)
        {
            if (!optionLines[i]) continue;

            lineStartColors[i] = optionLines[i].color;
            lineHomePositions[i] = optionLines[i].rectTransform.anchoredPosition;
        }
    }
}

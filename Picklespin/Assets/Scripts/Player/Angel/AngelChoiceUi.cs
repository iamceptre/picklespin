using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class AngelChoiceUi : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Canvas menuCanvas;
    [SerializeField] private CanvasGroup menuCanvasGroup;
    [SerializeField, Tooltip("the angel's question - optional")]
    private TMP_Text promptText;
    [SerializeField, Tooltip("one line per slot, top to bottom; chosen with keys 1, 2, 3... - the wish menu leaves the fourth line empty")]
    private TMP_Text[] optionLines = new TMP_Text[4];

    [Header("Presentation")]
    [SerializeField, Tooltip("separator between the flavour name and the effect")]
    private string nameSeparator = " - ";
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField, Tooltip("how long the picked line stays highlighted before the menu fades")]
    private float highlightDuration = 0.35f;
    [SerializeField] private Color highlightColor = GameColors.Highlight;

    private Color[] lineStartColors;
    private int hoveredSlot = -1;
    private Action closing;

    public string NameSeparator => nameSeparator;
    public int LineCount => optionLines == null ? 0 : optionLines.Length;
    public bool IsClosing => closing != null;
    public AngelChoiceMenu ActiveMenu { get; private set; }

    private void Awake() => CaptureStartColors();

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

        CaptureStartColors();
        line.DOKill();
        line.color = lineStartColors[slot];
        line.rectTransform.DOKill();
        line.rectTransform.localScale = Vector3.one;

        line.enabled = !string.IsNullOrEmpty(text);
        if (line.enabled) line.text = $"{slot + 1}.  {text}";
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
        if (slot == hoveredSlot) return;

        CaptureStartColors();
        if (hoveredSlot >= 0 && optionLines[hoveredSlot]) optionLines[hoveredSlot].color = lineStartColors[hoveredSlot];
        hoveredSlot = slot;
        if (hoveredSlot < 0 || !optionLines[hoveredSlot]) return;

        optionLines[hoveredSlot].color = Color.Lerp(lineStartColors[hoveredSlot], highlightColor, 0.5f);
    }

    public void FadeIn()
    {
        menuCanvasGroup.DOKill();
        menuCanvasGroup.alpha = 0f;
        if (menuCanvas) menuCanvas.enabled = true;

        menuCanvasGroup.DOFade(1f, fadeInDuration).SetUpdate(true);
    }

    public void HighlightChosen(int slot, Action onClosed)
    {
        for (int i = 0; i < LineCount; i++)
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

        if (!menuCanvasGroup) return;

        menuCanvasGroup.DOKill();
        menuCanvasGroup.alpha = 0f;
        if (menuCanvas) menuCanvas.enabled = false;
    }

    private void CaptureStartColors()
    {
        if (lineStartColors != null && lineStartColors.Length == LineCount) return;

        lineStartColors = new Color[LineCount];
        for (int i = 0; i < LineCount; i++)
        {
            if (optionLines[i]) lineStartColors[i] = optionLines[i].color;
        }
    }
}

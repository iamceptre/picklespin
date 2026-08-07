using UnityEngine;

// Turns a button into a step between two menu pages. Works in the main menu and, because
// MenuScreen fades on unscaled time, just as well in a pause menu with the game frozen.
public class MoveToAnotherCanvas : MonoBehaviour
{
    [Tooltip("Canvas to fade out, aka me. Optional - leave empty to only open the other one")]
    [SerializeField] private Canvas canvasToGoFrom;

    [Tooltip("Canvas to fade in. Leave empty for a back button: it returns to whichever page opened this one, so a screen shared between menus needs no target of its own")]
    [SerializeField] private Canvas canvasToGoTo;

    private MenuScreen from;
    private MenuScreen to;

    private void Awake()
    {
        from = MenuScreen.Of(canvasToGoFrom);
        to = MenuScreen.Of(canvasToGoTo);

        if (!from && !to) DevLog.Error($"{nameof(MoveToAnotherCanvas)}: no canvas either side, this button leads nowhere", this);
    }

    public void Do()
    {
        if (to) MenuScreen.Step(from, to);
        else MenuScreen.StepBack(from);
    }
}

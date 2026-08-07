using UnityEngine;

// A piece of a page that only runs while that page is up. A page is never shown by activating its
// GameObject - it stays active for the whole run and shows itself by its Canvas - so anything on it
// that polls input or drives a tween has to be told when the page opens, or it runs the whole time
// behind a closed screen.
public abstract class MenuScreenPart : MonoBehaviour
{
    private MenuScreen page;

    protected MenuScreen Page => page;

    protected virtual void Awake() => Bind(GetComponentInParent<MenuScreen>(true));

    // MenuScreen.Of makes a page out of something that has never been one, so a part can wake up
    // before its page exists. Whichever of the two runs second does the binding
    public void Bind(MenuScreen screen)
    {
        if (page || !screen || GetComponentInParent<MenuScreen>(true) != screen) return;

        page = screen;
        page.Opened += PageOpened;
        page.Closed += PageClosed;
        enabled = false;
    }

    // binding is settled by the time any Start runs, so this is the one place that can tell a part
    // with no page from one that is merely waiting - a bound part gets its Start late, on the first
    // open, because disabling in Awake defers Start rather than cancelling it
    protected virtual void Start()
    {
        if (page) return;

        DevLog.Warn($"{GetType().Name}: sits under no {nameof(MenuScreen)}, so nothing gates it and it runs from the moment the scene loads", this);
    }

    protected virtual void OnDestroy()
    {
        if (!page) return;

        page.Opened -= PageOpened;
        page.Closed -= PageClosed;
    }

    protected virtual void PageOpened() => enabled = true;

    protected virtual void PageClosed() => enabled = false;
}

using UnityEngine;

// The settings button on any menu. It looks the screen up in the scene instead of holding a
// reference, so the pause menu prefab keeps working in a level that has no settings screen yet
// and starts working the moment one is dropped in.
public class OpenSettingsScreen : MonoBehaviour
{
    [SerializeField, Tooltip("the page this button lives on - it fades out before the settings come up. Found among the parents if left empty")]
    private MenuScreen from;

    private bool searched;

    public void Do()
    {
        if (!SettingsScreen.Instance)
        {
            DevLog.Warn($"{nameof(OpenSettingsScreen)}: this scene has no {nameof(SettingsScreen)} - drop the settings prefab into it", this);
            return;
        }

        SettingsScreen.Instance.Open(Page);
    }

    // resolved on the first press, not in Awake: the page a pause menu lives on is put together
    // by Pause during its own Awake, and the two orders are not ours to pick
    private MenuScreen Page
    {
        get
        {
            if (!from && !searched)
            {
                searched = true;
                from = GetComponentInParent<MenuScreen>(true);
            }
            return from;
        }
    }
}

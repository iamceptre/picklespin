using UnityEngine;

// Runs in Awake so a MenuScreen on the same Canvas reads the closed state as its starting page.
// The GameObject stays active - a disabled Canvas already costs nothing, and turning the object
// off would stop whatever has to answer when the page is asked to open again.
public class DisableOnStart : MonoBehaviour
{
    private void Awake()
    {
        if (TryGetComponent(out Canvas canvas)) canvas.enabled = false;
    }
}

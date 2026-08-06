using UnityEngine;
using UnityEngine.UI;

// Sits on the settings Toggle: point its OnValueChanged (dynamic bool) at Set.
public class HardModeSetting : MonoBehaviour
{
    private Toggle toggle;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
    }

    private void Start()
    {
        if (toggle) toggle.SetIsOnWithoutNotify(HardMode.Enabled);
    }

    public void Set(bool on) => HardMode.Set(on);
}

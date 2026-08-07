using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// One definition of "the player is not driving right now", shared by the pause menu and by
// every angel menu, so hands, aim and cursor always come back the same way they went away.
public static class PlayerControlLock
{
    private static readonly HashSet<UnityEngine.Object> owners = new();

    private static SpellSelector spellSelector;
    private static AngelHeal angelHeal;
    private static bool resolved;

    private static bool lastLocked;
    private static bool angelHealWasEnabled;

    public static bool Locked => owners.Count > 0;

    public static void Set(UnityEngine.Object owner, bool locked)
    {
        if (!owner) return;

        bool changed = locked ? owners.Add(owner) : owners.Remove(owner);
        if (changed) Apply();
    }

    private static void Apply()
    {
        bool locked = Locked;

        Resolve();

        if (PlayerMovement.Instance) PlayerMovement.Instance.enabled = !locked;
        if (Attack.instance) Attack.instance.enabled = !locked;
        if (Dash.Instance) Dash.Instance.enabled = !locked;
        if (spellSelector) spellSelector.enabled = !locked;

        if (MouselookXY.instance)
        {
            MouselookXY.instance.enabled = !locked;
            if (!locked) MouselookXY.instance.RestoreSensitivity();
        }

        // this one is not ours to switch on: isCloseToAngel owns it by proximity, so the state
        // it was in when the controls went away is the state it gets back
        if (angelHeal)
        {
            if (locked && !lastLocked) angelHealWasEnabled = angelHeal.enabled;
            angelHeal.enabled = !locked && angelHealWasEnabled;
        }

        lastLocked = locked;

        ApplyCursor();
    }

    // Unity releases the cursor lock itself on the frame Escape is pressed, undoing whatever the
    // pause menu just asked for - so whoever pauses on Escape asks for it again once that frame is over
    public static void ReapplyCursor() => ApplyCursor();

    private static void ApplyCursor()
    {
        bool locked = Locked;

        Cursor.lockState = locked ? CursorLockMode.Confined : CursorLockMode.Locked;
        Cursor.visible = locked;
    }

    private static void Resolve()
    {
        if (resolved) return;

        spellSelector = Object.FindFirstObjectByType<SpellSelector>(FindObjectsInactive.Include);
        angelHeal = Object.FindFirstObjectByType<AngelHeal>(FindObjectsInactive.Include);
        resolved = true;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay()
    {
        Forget();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // the new scene owns its own controls: drop the old locks without touching anything,
    // the fresh components already start unlocked
    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Forget();

    private static void Forget()
    {
        owners.Clear();
        spellSelector = null;
        angelHeal = null;
        resolved = false;
        lastLocked = false;
        angelHealWasEnabled = false;
    }
}

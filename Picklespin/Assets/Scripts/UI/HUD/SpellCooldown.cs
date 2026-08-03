using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SpellCooldown : MonoBehaviour
{

    private Slider me;
    private float currentCooldown;
    public Canvas myCanvas;

    Attack attack;
    private Coroutine routine;

    // the bar is shared: a dash spends the same cooldown a spell does
    public bool IsCoolingDown => currentCooldown > 0f;

    private void Awake()
    {
        me = GetComponent<Slider>();
        myCanvas = GetComponent<Canvas>();
    }

    private void Start()
    {
        attack = Attack.instance;
    }

    public void StartCooldown(float selectedSpellCooldownTime)
    {
        // the wish is applied here, so it shortens the dash's turn on the bar too
        float cooldown = selectedSpellCooldownTime * WishUpgrades.CooldownMultiplier;

        myCanvas.enabled = true;
        attack.castCooldownAllow = false;
        currentCooldown = cooldown;
        // one timer only: a second would unlock casting when the shorter of the two ran out
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Cooldown(cooldown));
    }

    private IEnumerator Cooldown(float selectedSpellCooldownTime)
    {
        while (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
            me.value = currentCooldown / selectedSpellCooldownTime;
            yield return null;
        }

        attack.castCooldownAllow = true;
        myCanvas.enabled = false;
        routine = null;
        yield break;
    }

    // an abandoned cast gives the canvas back, but must not cancel a running
    // cooldown - that would make cancelling a cast a way to skip it
    public void DisableComponents()
    {
        if (IsCoolingDown) return;
        myCanvas.enabled = false;
    }

}

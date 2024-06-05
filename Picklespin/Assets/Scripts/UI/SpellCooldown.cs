using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SpellCooldown : MonoBehaviour
{

    private Slider me;
    private float currentCooldown;
    public Canvas myCanvas;

    Attack attack;

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
        myCanvas.enabled = true;
        attack.castCooldownAllow = false;
        currentCooldown = selectedSpellCooldownTime;
        StartCoroutine(Cooldown(selectedSpellCooldownTime));
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
        yield break;
    }

    public void DisableComponents()
    {
        currentCooldown = 0;
        myCanvas.enabled = false;
    }

}

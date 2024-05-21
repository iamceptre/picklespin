using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SpellCooldown : MonoBehaviour
{

    private Slider me;
    //[HideInInspector] public float selectedSpellCooldownTime;
    private float currentCooldown;
    public Canvas myCanvas;
    //[SerializeField] private GameObject[] myComponents;

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
        while (true)
        {
            currentCooldown -= Time.deltaTime;
            me.value = currentCooldown / selectedSpellCooldownTime;

            if (currentCooldown<=0) 
            {
                attack.castCooldownAllow = true;
                myCanvas.enabled = false;
                yield break;
            }

            yield return null;
        }
    }

    public void DisableComponents()
    {
        currentCooldown = 0;
        myCanvas.enabled = false;
    }

}

using UnityEngine;
using UnityEngine.UI;

public class SpellCooldown : MonoBehaviour
{

    private Slider me;
    [HideInInspector] public float selectedSpellCooldownTime;
    private float currentCooldown;
    [SerializeField] private GameObject[] myComponents;

    private void Awake()
    {
        me = GetComponent<Slider>();
    }


    [SerializeField]private Attack attack;
    void Update()
    {
        currentCooldown -= Time.deltaTime;
        me.value = currentCooldown/selectedSpellCooldownTime;

        if (me.value <= 0)
        {
            DisableComponents();
            enabled = false;
        }
    }

    public void StartCooldowning()
    {
        EnableComponents();
        currentCooldown = selectedSpellCooldownTime;
    }

    public void EnableComponents()
    {
        for (int i = 0; i < myComponents.Length; i++)
        {
            myComponents[i].SetActive(true);
        }
    }

    public void DisableComponents()
    {
        for (int i = 0; i < myComponents.Length; i++)
        {
            myComponents[i].SetActive(false);
        }
    }
}

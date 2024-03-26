using UnityEngine;
using UnityEngine.UI;

public class SpellCooldown : MonoBehaviour
{

    private Slider me;
    [HideInInspector] public float selectedSpellCooldownTime;
    private float currentCooldown;
    private RectTransform myTransform;
    private Vector3 startingScale;

    private void Awake()
    {
        me = GetComponent<Slider>();
        myTransform = GetComponent<RectTransform>();
    }
    private void Start()
    {
        startingScale = myTransform.localScale;
    }

    [SerializeField]private Attack attack;
    void Update()
    {
        currentCooldown -= Time.deltaTime;
        me.value = currentCooldown/selectedSpellCooldownTime;

        if (me.value <= 0)
        {
            myTransform.localScale = Vector3.zero;
            enabled = false;
        }
    }

    public void StartCooldowning()
    {
        myTransform.localScale = startingScale;
        currentCooldown = selectedSpellCooldownTime;
    }
}

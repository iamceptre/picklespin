using UnityEngine;
using FMODUnity;

public class AngelMind : MonoBehaviour
{
    private PlayerHP playerHP;
    private Ammo ammo;
    private PlayerMovement playerMovement;

    public bool healed = false;
    public bool isDead = false;

    public EventReference angelHealedEvent;
    private FMOD.Studio.EventInstance angelInstance;

    [SerializeField] private ParticleSystem healedParticles;

    private Renderer angelRenderer;

    private void Start()
    {
        angelRenderer = gameObject.GetComponent<Renderer>();
        playerHP = PlayerHP.instance;
        ammo = Ammo.instance;
        playerMovement = PlayerMovement.instance;   
    }


    public void AfterHealedAction()
    {

        if (healedParticles != null)
        {
            healedParticles.Play();
        }
        angelInstance = RuntimeManager.CreateInstance(angelHealedEvent);
        angelInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
        angelRenderer.material.SetColor("_Color", Color.green);
        angelInstance.start();

        GiveHPToPlayer();
        Invoke("GiveStaminaToPlayer", 0.2f);
        Invoke("GiveManaToPlayer", 0.4f);
    }


    private void GiveHPToPlayer()
    {
        playerHP.GiveHPToPlayer(Random.Range(80, 100));
    }

    private void GiveStaminaToPlayer()
    {
        playerMovement.GiveStaminaToPlayer(Random.Range(80, 100));
    }

    private void GiveManaToPlayer()
    {
        ammo.GiveManaToPlayer(Random.Range(80, 100)); 
    }

}

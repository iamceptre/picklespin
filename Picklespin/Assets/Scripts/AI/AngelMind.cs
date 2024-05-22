using UnityEngine;
using FMODUnity;
using UnityEngine.Events;

public class AngelMind : MonoBehaviour
{
    private PlayerHP playerHP;
    private Ammo ammo;
    private PlayerMovement playerMovement;
    private PlayerEXP playerEXP;

    public bool healed = false;
    public bool isDead = false;

    [SerializeField] private EventReference angelHealedSoundEvent;
    private FMOD.Studio.EventInstance angelInstance;


    [SerializeField] private ParticleSystem healedParticles;

    private Renderer angelRenderer;

    private void Start()
    {
        angelRenderer = gameObject.GetComponent<Renderer>();
        playerHP = PlayerHP.instance;
        ammo = Ammo.instance;
        playerMovement = PlayerMovement.instance;   
        playerEXP = PlayerEXP.instance;
    }


    public void AfterHealedAction()
    {

        if (healedParticles != null)
        {
            healedParticles.Play();
        }
        angelInstance = RuntimeManager.CreateInstance(angelHealedSoundEvent);
        angelInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
        angelRenderer.material.SetColor("_Color", Color.green);
        angelInstance.start();

        GiveHPToPlayer();
        Invoke("GiveStaminaToPlayer", 0.2f);
        Invoke("GiveManaToPlayer", 0.4f);
        playerEXP.GivePlayerExp(200, "healed an Angel");
    }


    private void GiveHPToPlayer()
    {
        playerHP.GiveHPToPlayer(playerHP.maxHp);
    }

    private void GiveStaminaToPlayer()
    {
        playerMovement.GiveStaminaToPlayer(100);
    }

    private void GiveManaToPlayer()
    {
        ammo.GiveManaToPlayer(ammo.maxAmmo);
    }

}

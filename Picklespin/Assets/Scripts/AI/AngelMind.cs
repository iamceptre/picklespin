using UnityEngine;
using FMODUnity;

public class AngelMind : MonoBehaviour
{
    private PlayerHP playerHP;
    private Ammo ammo;
    private PlayerMovement playerMovement;
    private PlayerEXP playerEXP;
    private AngelTorchManager angelTorchManager;

    public bool healed = false;
    public bool isDead = false;

    [SerializeField] private EventReference angelHealedSoundEvent;
    private FMOD.Studio.EventInstance angelInstance;


    [SerializeField] private ParticleSystem healedParticles;
    [SerializeField] private GiveExpToPlayer giveExpAfterHeal;

    [SerializeField] private BoxCollider scriptActivationTrigger;

    private Renderer angelRenderer;

    private void Awake()
    {
        angelTorchManager = GetComponent<AngelTorchManager>();
    }

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

        angelTorchManager.OffTorch();

        GiveHPToPlayer();
        Invoke("GiveStaminaToPlayer", 0.2f);
        Invoke("GiveManaToPlayer", 0.4f);
        giveExpAfterHeal.GiveExp();
        scriptActivationTrigger.size = Vector3.zero;
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

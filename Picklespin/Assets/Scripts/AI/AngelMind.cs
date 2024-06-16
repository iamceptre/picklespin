using UnityEngine;
using FMODUnity;

public class AngelMind : MonoBehaviour
{
    [Header("References")]
    [SerializeField]private Torch torch;
    private PlayerHP playerHP;
    private Ammo ammo;
    private PlayerMovement playerMovement;
    private PlayerEXP playerEXP;
    private AngelTorchManager angelTorchManager;
    [SerializeField] private EventReference angelHealedSoundEvent;
    private FMOD.Studio.EventInstance angelInstance;
    [SerializeField] private ParticleSystem healedParticles;
    [SerializeField] private GiveExpToPlayer giveExpAfterHeal;
    [SerializeField] private BoxCollider scriptActivationTrigger;
    private Renderer angelRenderer;
    private Helper_Arrow helperArrow;

    [Header("Logic")]
    public bool healed = false;
    public bool isDead = false;

    [Header("Spawner Logic and Refrences")]
    public bool isActive;
    private Renderer _rend;
    private Collider _collider;
    private BoxCollider _activationTrigger;


    public void SetActive(bool state)
    {
        _activationTrigger.enabled = state;
        _rend.enabled = state;
        _collider.enabled = state;
        isActive = state;

        if (state)
        {
            torch.On();
        }
        else
        {
            torch.Off();
        }
    }


    private void Awake()
    {
        _rend = GetComponent<Renderer>();
        _collider = GetComponent<SphereCollider>();
        angelTorchManager = GetComponent<AngelTorchManager>();
        _activationTrigger = GetComponentInChildren<BoxCollider>();
    }

    private void Start()
    {
        angelRenderer = gameObject.GetComponent<Renderer>();
        helperArrow = Helper_Arrow.instance;
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

        helperArrow.HideArrow();

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

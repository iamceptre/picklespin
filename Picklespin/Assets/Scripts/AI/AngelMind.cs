using UnityEngine;
using FMODUnity;

public class AngelMind : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LookAtPlayer lookAtPlayer;
    [SerializeField] private Animator angelRingsAnimator;
    [SerializeField] private Renderer[] additionalElements;
    [SerializeField] private Torch torch;
    [SerializeField] private AngelRingEyesOpenClose eyesManager;
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
    private AngelHealingMinigame minigame;

    [Header("Emmiter References")]

    [SerializeField] private StudioEventEmitter unhealedLoopEmmiter;
    [SerializeField] private StudioEventEmitter healedLoopEmmiter;

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
        if (isActive)
        {
            return;
        }

        unhealedLoopEmmiter.Play();

        for (int i = 0; i < additionalElements.Length; i++)
        {
            additionalElements[i].enabled = state;
        }

        _activationTrigger.enabled = state;
        _rend.enabled = state;
        unhealedLoopEmmiter.gameObject.SetActive(state);
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
        minigame = AngelHealingMinigame.instance;
        helperArrow = Helper_Arrow.instance;
        playerHP = PlayerHP.instance;
        ammo = Ammo.instance;
        playerMovement = PlayerMovement.instance;
        playerEXP = PlayerEXP.instance;
        eyesManager.Close();
    }


    public void AfterHealedAction()
    {
        if (healedParticles != null)
        {
            healedParticles.Play();
        }

        angelRingsAnimator.SetTrigger("Healed");

        helperArrow.HideArrow();
        lookAtPlayer.enabled = true;

        unhealedLoopEmmiter.Stop();
        healedLoopEmmiter.Play();

        angelInstance = RuntimeManager.CreateInstance(angelHealedSoundEvent);
        angelInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
        angelRenderer.material.SetColor("_Color", Color.green);
        angelInstance.start();

        eyesManager.Open();

        angelTorchManager.OffTorch();

        GiveHPToPlayer();
        Invoke("GiveStaminaToPlayer", 0.2f);
        Invoke("GiveManaToPlayer", 0.4f);
        giveExpAfterHeal.GiveExp();
        scriptActivationTrigger.size = Vector3.zero;
        minigame.RandomizeTurboAreaPosition();
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


    public void StopMySound()
    {
        unhealedLoopEmmiter.Stop();
        healedLoopEmmiter.Stop();
    }

}

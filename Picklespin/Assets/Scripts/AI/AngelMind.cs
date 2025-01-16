using UnityEngine;
using FMODUnity;
using UnityEngine.Events;

public class AngelMind : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Door angelRoomDoor;
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
    [SerializeField] private ParticleSystem healedParticles;
    [SerializeField] private GiveExpToPlayer giveExpAfterHeal;
    [SerializeField] private BoxCollider scriptActivationTrigger;
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
    private Collider _collider;
    private BoxCollider _activationTrigger;

    [Header("Additional Event")]
    [SerializeField] private UnityEvent additionalHealedEvent;


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

        angelRoomDoor.isLocked = !state;
        _activationTrigger.enabled = state;
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
        _collider = GetComponent<SphereCollider>();
        angelTorchManager = GetComponent<AngelTorchManager>();
        _activationTrigger = GetComponentInChildren<BoxCollider>();
    }

    private void Start()
    {
        minigame = AngelHealingMinigame.Instance;
        helperArrow = Helper_Arrow.Instance;
        playerHP = PlayerHP.instance;
        ammo = Ammo.instance;
        playerMovement = PlayerMovement.Instance;
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

        RuntimeManager.PlayOneShot(angelHealedSoundEvent);

        eyesManager.Open();

        angelTorchManager.OffTorch();

        GiveHPToPlayer();
        Invoke(nameof(GiveStaminaToPlayer), 0.2f);
        Invoke(nameof(GiveManaToPlayer), 0.4f);
        //giveExpAfterHeal.GiveExp();
        scriptActivationTrigger.size = Vector3.zero;
        minigame.RandomizeTurboAreaPosition();
        additionalHealedEvent.Invoke();
    }


    private void GiveHPToPlayer()
    {
        playerHP.ModifyHP(playerHP.maxHp);
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

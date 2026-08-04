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
    private PlayerEXP playerEXP;
    private AngelTorchManager angelTorchManager;
    [SerializeField] private EventReference angelHealedSoundEvent;
    [SerializeField] private ParticleSystem healedParticles;
    [SerializeField] private BoxCollider scriptActivationTrigger;
    private AngelPointerHelper pointerHelper;
    private AngelHealingMinigame minigame;

    [Header("Emmiter References")]

    [SerializeField] private StudioEventEmitter unhealedLoopEmmiter;
    [SerializeField] private StudioEventEmitter healedLoopEmmiter;

    [Header("Logic")]
    public bool healed = false;

    [Header("Spawner Logic and Refrences")]
    public bool isActive;
    private Collider _collider;
    private BoxCollider _activationTrigger;
    private AiHealth aiHealth;

    public bool IsDead => aiHealth && !aiHealth.IsAlive;

    [Header("Additional Event")]
    [SerializeField] private UnityEvent additionalHealedEvent;

    public void SetActive(bool state)
    {
        if (isActive || IsDead)
        {
            return;
        }

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
            unhealedLoopEmmiter.Play();
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
        aiHealth = GetComponent<AiHealth>();
        if (aiHealth) aiHealth.deathEvent.AddListener(HandleDeath);
    }

    private void HandleDeath()
    {
        isActive = false;
        StopMySound();
        if (pointerHelper) pointerHelper.StopPointingAt(transform);
    }

    private void Start()
    {
        minigame = AngelHealingMinigame.Instance;
        pointerHelper = AngelPointerHelper.Instance;
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

        pointerHelper.Stop();
        lookAtPlayer.enabled = true;

        unhealedLoopEmmiter.Stop();
        healedLoopEmmiter.Play();

        RuntimeManager.PlayOneShot(angelHealedSoundEvent);

        eyesManager.Open();

        angelTorchManager.OffTorch();

        // the three bars are no longer refilled here — a full restore is one of
        // the wishes AngelWishMenu offers once the angel asks
        scriptActivationTrigger.size = Vector3.zero;
        minigame.RandomizeTurboAreaPosition();
        additionalHealedEvent.Invoke();
    }

    public void StopMySound()
    {
        unhealedLoopEmmiter.Stop();
        healedLoopEmmiter.Stop();
    }

}

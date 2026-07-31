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

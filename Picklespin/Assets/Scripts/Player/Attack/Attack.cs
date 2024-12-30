using UnityEngine;
using FMODUnity;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using FMOD.Studio;
using UnityEngine.InputSystem;

public class Attack : MonoBehaviour
{
    private Animator handAnimator;
    public static Attack instance { get; private set; }
    private PlayCastBlast playCastBlast;
    private SpellProjectileSpawner spellProjectileSpawner;
    //[SerializeField] private UnityEvent shootEvent;
    [SerializeField] private UnityEvent changeSelectedSpell;
    private Ammo ammo;
    private AmmoDisplay ammoDisplay;
    [SerializeField] private EventReference shootFailEvent;
    private EventInstance pullupEventInstance;
    private EventInstance spellcastInstance;
    public Bullet[] bulletPrefab;
    public int selectedBulletIndex;
    public float castCooldownTime = 0.1f;
    public bool castCooldownAllow = true;
    [SerializeField] private UnlockedSpells unlockedSpells;
    [SerializeField] private SpellCooldown spellCooldown;
    [SerializeField] private NoManaLightAnimation noManaLightAnimation;
    public float castingProgress = 0;
    private float currentlySelectedCastDuration;
    [SerializeField] private Slider castingSlider;
    [SerializeField] private UnityEvent castingCompleted;
    [SerializeField] private UnityEvent CancelCasting;
    public Bullet currentBullet;
    private CameraShakeManagerV2 camShakeManager;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference primaryAction;
    [SerializeField] private InputActionReference secondaryAction;

    private bool isPrimaryPressed;
    private bool isSecondaryPressed;
    private Coroutine castingRoutine;

    private void Awake()
    {
        currentBullet = bulletPrefab[selectedBulletIndex];
        if (instance != null && instance != this) Destroy(this); else instance = this;
    }

    private void Start()
    {
        handAnimator = PublicPlayerHandAnimator.instance._animator;
        ammo = Ammo.instance;
        playCastBlast = PlayCastBlast.instance;
        ammoDisplay = AmmoDisplay.instance;
        spellProjectileSpawner = SpellProjectileSpawner.instance;
        camShakeManager = CameraShakeManagerV2.instance;
    }

    private void OnEnable()
    {
        primaryAction.action.performed += OnPrimaryPerformed;
        primaryAction.action.canceled += OnPrimaryCanceled;
        primaryAction.action.Enable();
        secondaryAction.action.performed += OnSecondaryPerformed;
        secondaryAction.action.canceled += OnSecondaryCanceled;
        secondaryAction.action.Enable();
    }

    private void OnDisable()
    {
        primaryAction.action.performed -= OnPrimaryPerformed;
        primaryAction.action.canceled -= OnPrimaryCanceled;
        primaryAction.action.Disable();
        secondaryAction.action.performed -= OnSecondaryPerformed;
        secondaryAction.action.canceled -= OnSecondaryCanceled;
        secondaryAction.action.Disable();
    }

    private void OnPrimaryPerformed(InputAction.CallbackContext ctx)
    {
        isPrimaryPressed = true;
        if (!castCooldownAllow) return;
        if (currentlySelectedCastDuration == 0) TryShoot();
        else if (!isSecondaryPressed)
        {
            ClearCasting();
            castingRoutine = StartCoroutine(SpellCasting());
        }
    }

    private void OnPrimaryCanceled(InputAction.CallbackContext ctx)
    {
        isPrimaryPressed = false;
    }

    private void OnSecondaryPerformed(InputAction.CallbackContext ctx)
    {
        isSecondaryPressed = true;
    }

    private void OnSecondaryCanceled(InputAction.CallbackContext ctx)
    {
        isSecondaryPressed = false;
    }

    private void TryShoot()
    {
        if (ammo.ammo >= currentBullet.magickaCost) SuccesfulShoot(); else ShootFail();
    }

    private void ShootFail()
    {
        handAnimator.SetTrigger("Hand_Fail");
        noManaLightAnimation.LightAnimation();
        spellcastInstance = RuntimeManager.CreateInstance(shootFailEvent);
        spellcastInstance.start();
        spellcastInstance.release();
    }

    private void SuccesfulShoot()
    {
        handAnimator.SetTrigger("Spell_Shot_Quick");
        if (playCastBlast.castingParticles[selectedBulletIndex] != null) playCastBlast.StopCastingParticles(selectedBulletIndex);
        playCastBlast.Play(selectedBulletIndex);
        castCooldownTime = currentBullet.myCooldown;
       // shootEvent.Invoke();
        ammo.ammo -= currentBullet.magickaCost;
        ammoDisplay.Refresh(false);
        spellCooldown.StartCooldown(castCooldownTime);
        spellProjectileSpawner.SpawnSpell(selectedBulletIndex);
        SendShakeSignalShoot(selectedBulletIndex);
    }

    public void SelectSpell(int selectedSpell)
    {
        selectedBulletIndex = selectedSpell;
        currentBullet = bulletPrefab[selectedBulletIndex];
        currentlySelectedCastDuration = currentBullet.castDuration;
        changeSelectedSpell.Invoke();
        pullupEventInstance = RuntimeManager.CreateInstance(currentBullet.pullupSound);
        pullupEventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        pullupEventInstance.start();
    }

    private IEnumerator SpellCasting()
    {
        if (ammo.ammo >= currentBullet.magickaCost)
        {
            spellCooldown.myCanvas.enabled = true;
            playCastBlast.StartCastingParticles(selectedBulletIndex);
            PlayerMovement.instance.SlowMeDown();
            handAnimator.SetTrigger("Spell_Casting");
            SendShakeSignalCastStart(selectedBulletIndex);
            while (castingProgress < currentlySelectedCastDuration)
            {
                if (!isPrimaryPressed || isSecondaryPressed)
                {
                    handAnimator.SetTrigger("Spell_Casting_Stop");
                    ClearCasting();
                    yield break;
                }
                castingProgress += Time.deltaTime;
                castingSlider.value = castingProgress / currentlySelectedCastDuration;
                yield return null;
            }
            castingCompleted.Invoke();
            while (isPrimaryPressed)
            {
                if (isSecondaryPressed)
                {
                    handAnimator.SetTrigger("Spell_Casting_Stop");
                    ClearCasting();
                    yield break;
                }
                yield return null;
            }
            ClearCasting();
            TryShoot();
        }
        else ShootFail();
    }

    private void ClearCasting()
    {
        PlayerMovement.instance.SpeedMeBackUp();
        handAnimator.ResetTrigger("Spell_Casting");
        spellCooldown.myCanvas.enabled = false;
        castingSlider.value = 0;
        castingProgress = 0;
        playCastBlast.StopCastingParticles(selectedBulletIndex);
        spellCooldown.DisableComponents();
        CancelCasting.Invoke();
        if (castingRoutine != null) StopCoroutine(castingRoutine);
    }

    private void SendShakeSignalShoot(int index)
    {
        switch (index)
        {
            case 0:
                camShakeManager.ShakeSelected(4);
                camShakeManager.ShakeHand(0.3f, 0.2f, 30);
                break;
            case 1:
                camShakeManager.ShakeSelected(5);
                camShakeManager.ShakeHand(0.4f, 0.2f, 15);
                break;
            default:
                break;
        }
    }

    private void SendShakeSignalCastStart(int index)
    {
        switch (index)
        {
            case 1:
                camShakeManager.ShakeSelected(7);
                break;
            default:
                break;
        }
    }
}

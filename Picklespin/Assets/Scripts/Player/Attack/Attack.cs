using UnityEngine;
using FMODUnity;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using FMOD.Studio;
using UnityEngine.InputSystem;

public class Attack : MonoBehaviour
{
    Animator handAnimator;
    public static Attack instance { get; private set; }
    PlayCastBlast playCastBlast;
    SpellProjectileSpawner spellProjectileSpawner;
    [SerializeField] UnityEvent changeSelectedSpell;
    Ammo ammo;
    AmmoDisplay ammoDisplay;
    [SerializeField] EventReference shootFailEvent;
    EventInstance pullupEventInstance;
    EventInstance spellcastInstance;
    public Bullet[] bulletPrefab;
    public int selectedBulletIndex;
    public float castCooldownTime = 0.1f;
    public bool castCooldownAllow = true;
    [SerializeField] UnlockedSpells unlockedSpells;
    [SerializeField] SpellCooldown spellCooldown;
    [SerializeField] NoManaLightAnimation noManaLightAnimation;
    public float castingProgress = 0;
    float currentlySelectedCastDuration;
    [SerializeField] Slider castingSlider;
    [SerializeField] UnityEvent castingCompleted;
    [SerializeField] UnityEvent CancelCasting;
    public Bullet currentBullet;
    CameraShakeManagerV2 camShakeManager;
    [SerializeField] InputActionReference primaryAction;
    [SerializeField] InputActionReference secondaryAction;
    bool isPrimaryPressed;
    bool isSecondaryPressed;
    Coroutine castingRoutine;

    void Awake()
    {
        currentBullet = bulletPrefab[selectedBulletIndex];
        if (instance != null && instance != this) Destroy(this); else instance = this;
    }

    void Start()
    {
        handAnimator = PublicPlayerHandAnimator.instance._animator;
        ammo = Ammo.instance;
        playCastBlast = PlayCastBlast.instance;
        ammoDisplay = AmmoDisplay.instance;
        spellProjectileSpawner = SpellProjectileSpawner.instance;
        camShakeManager = CameraShakeManagerV2.instance;
    }

    void OnEnable()
    {
        primaryAction.action.performed += OnPrimaryPerformed;
        primaryAction.action.canceled += OnPrimaryCanceled;
        primaryAction.action.Enable();
        secondaryAction.action.performed += OnSecondaryPerformed;
        secondaryAction.action.canceled += OnSecondaryCanceled;
        secondaryAction.action.Enable();
    }

    void OnDisable()
    {
        primaryAction.action.performed -= OnPrimaryPerformed;
        primaryAction.action.canceled -= OnPrimaryCanceled;
        primaryAction.action.Disable();
        secondaryAction.action.performed -= OnSecondaryPerformed;
        secondaryAction.action.canceled -= OnSecondaryCanceled;
        secondaryAction.action.Disable();
    }

    void OnPrimaryPerformed(InputAction.CallbackContext ctx)
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

    void OnPrimaryCanceled(InputAction.CallbackContext ctx)
    {
        isPrimaryPressed = false;
    }

    void OnSecondaryPerformed(InputAction.CallbackContext ctx)
    {
        isSecondaryPressed = true;
    }

    void OnSecondaryCanceled(InputAction.CallbackContext ctx)
    {
        isSecondaryPressed = false;
    }

    void TryShoot()
    {
        if (ammo.ammo >= currentBullet.magickaCost) SuccesfulShoot(); else ShootFail();
    }

    void ShootFail()
    {
        handAnimator.SetTrigger("Hand_Fail");
        noManaLightAnimation.LightAnimation();
        spellcastInstance = RuntimeManager.CreateInstance(shootFailEvent);
        spellcastInstance.start();
        spellcastInstance.release();
    }

    void SuccesfulShoot()
    {
        handAnimator.SetTrigger("Spell_Shot_Quick");
        if (playCastBlast.castingParticles[selectedBulletIndex] != null) playCastBlast.StopCastingParticles(selectedBulletIndex);
        playCastBlast.Play(selectedBulletIndex);
        castCooldownTime = currentBullet.myCooldown;
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

    IEnumerator SpellCasting()
    {
        if (ammo.ammo >= currentBullet.magickaCost)
        {
            spellCooldown.myCanvas.enabled = true;
            playCastBlast.StartCastingParticles(selectedBulletIndex);
            PlayerMovement.Instance.SlowMeDown();
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

    void ClearCasting()
    {
        PlayerMovement.Instance.SpeedMeBackUp();
        handAnimator.ResetTrigger("Spell_Casting");
        spellCooldown.myCanvas.enabled = false;
        castingSlider.value = 0;
        castingProgress = 0;
        playCastBlast.StopCastingParticles(selectedBulletIndex);
        spellCooldown.DisableComponents();
        CancelCasting.Invoke();
        if (castingRoutine != null) StopCoroutine(castingRoutine);
    }

    void SendShakeSignalShoot(int index)
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
        }
    }

    void SendShakeSignalCastStart(int index)
    {
        switch (index)
        {
            case 1:
                camShakeManager.ShakeSelected(7);
                break;
        }
    }
}

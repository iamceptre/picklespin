using UnityEngine;
using FMODUnity;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using FMOD.Studio;

public class Attack : MonoBehaviour
{

    private Animator handAnimator;
    public static Attack instance { get; private set; }

    private PlayCastBlast playCastBlast;
    private SpellProjectileSpawner spellProjectileSpawner;

    [SerializeField] private UnityEvent shootEvent;
    [SerializeField] private UnityEvent changeSelectedSpell;

    private Ammo ammo;
    AmmoDisplay ammoDisplay;

    [SerializeField] private EventReference shootFailEvent;
    private EventInstance pullupEventInstance;
    private EventInstance spellcastInstance;

    public Bullet[] bulletPrefab;
    public int selectedBulletIndex;

    [HideInInspector] public float castCooldownTime = 0.1f;
    public bool castCooldownAllow = true;

    [SerializeField] private UnlockedSpells unlockedSpells;
    [SerializeField] private SpellCooldown spellCooldown;

    [SerializeField] private NoManaLightAnimation noManaLightAnimation;

    //Long Casting
    [HideInInspector] public float castingProgress = 0;
    private float currentlySelectedCastDuration;
    [SerializeField] private Slider castingSlider;

    [SerializeField] private UnityEvent castingCompleted;
    [SerializeField] private UnityEvent CancelCasting;

    public Bullet currentBullet;

    private CameraShakeManagerV2 camShakeManager;


    private void Awake()
    {

        currentBullet = bulletPrefab[selectedBulletIndex];

        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && castCooldownAllow)
        {
            if (currentlySelectedCastDuration == 0)
            {
                TryShoot(); 
            }
            else
            {
                if (!Input.GetKey(KeyCode.Mouse1))
                {
                    ClearCasting();
                    StartCoroutine(SpellCasting());
                }
            }
        }


    }

    private void TryShoot()
    {
        if (ammo.ammo >= currentBullet.magickaCost)
        {
            SuccesfulShoot();
        }
        else
        {
            ShootFail();
        }
    }

    private void ShootFail()
    {
        //handAnimator.SetTrigger("Shoot_Fail");
        handAnimator.SetTrigger("Hand_Fail");
        noManaLightAnimation.LightAnimation();
        spellcastInstance = RuntimeManager.CreateInstance(shootFailEvent);
        spellcastInstance.start();
        spellcastInstance.release();
    }

    private void SuccesfulShoot()
    {
        handAnimator.SetTrigger("Spell_Shot_Quick");

        if (playCastBlast.castingParticles[selectedBulletIndex] != null) {
            playCastBlast.StopCastingParticles(selectedBulletIndex);
        }

        playCastBlast.Play(selectedBulletIndex);
        castCooldownTime = currentBullet.myCooldown;
        shootEvent.Invoke();
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

                if (Input.GetKeyUp(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1))
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

            while (Input.GetKey(KeyCode.Mouse0))
            {

                if (Input.GetKeyDown(KeyCode.Mouse1))
                {
                    handAnimator.SetTrigger("Spell_Casting_Stop");
                    ClearCasting();
                    yield break;
                }

                yield return null;
            }

                ClearCasting();
                TryShoot();

            yield break;
        }
        else
        {
            ShootFail();
            yield break;
        }
    }


    private void ClearCasting()
    {
        PlayerMovement.instance.SpeedMeBackUp();
        handAnimator.ResetTrigger("Spell_Casting");
        //handAnimator.SetTrigger("Spell_Casting_Stop");
        spellCooldown.myCanvas.enabled = false;
        castingSlider.value = 0;
        castingProgress = 0;
        playCastBlast.StopCastingParticles(selectedBulletIndex);
        spellCooldown.DisableComponents();
        CancelCasting.Invoke();
    }


    private void SendShakeSignalShoot(int selectedBulletIndex)
    {
        switch (selectedBulletIndex)
        {
            case 0: //purple
                camShakeManager.ShakeSelected(4);
                camShakeManager.ShakeHand(0.3f, 0.2f, 30);
                break;

            case 1: //fireball
                camShakeManager.ShakeSelected(5);
                camShakeManager.ShakeHand(0.4f, 0.2f, 15);
                break;

            case 2: //light?

                break;

            default:
                Debug.Log("there is not such a bullet indexed");
                break;
        }
    }

    private void SendShakeSignalCastStart(int selectedBulletIndex)
    {
        switch (selectedBulletIndex)
        {
            case 0: //purple
                
                break;

            case 1: //fireball
                camShakeManager.ShakeSelected(7);
                break;

            case 2: //light?

                break;

            default:
                Debug.Log("there is not such a bullet indexed");
                break;
        }
    }

}

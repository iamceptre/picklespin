using UnityEngine;
using FMODUnity;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.UI;

public class Attack : MonoBehaviour
{
    public static Attack instance { get; private set; }

    [SerializeField] private Transform handCastingPoint;

    [SerializeField] private UnityEvent shootEvent;
    [SerializeField] private UnityEvent changeSelectedSpell;

    private Ammo ammo;
    [SerializeField] AmmoDisplay ammoDisplay;

    [SerializeField] private EventReference shootFailEvent;
    [SerializeField] private EventReference spellLockedEvent;
    private FMOD.Studio.EventInstance spellcastInstance;

    [SerializeField] private Transform bulletSpawnPoint;
    public GameObject[] bulletPrefab;
    public int selectedBullet;

    [HideInInspector] public float castCooldownTime = 0.1f;
    private bool castCooldownAllow = true;

    [SerializeField] private UnlockedSpells unlockedSpells;
    [SerializeField] private SpellCooldown spellCooldown;

    [SerializeField] private NoManaLightAnimation noManaLightAnimation;

    //Long Casting
    [HideInInspector] public float castingPercentage = 0;
    private float currentlySelectedCastDuration;
    private bool autofirePrevent;
    [SerializeField] private Slider castingSlider;
    private RectTransform castingSliderRectTransform;
    private SpellCooldown sliderScript;
    private bool castLoaded = false;

   [SerializeField] private UnityEvent castingCompleted;
   [SerializeField] private UnityEvent CancelCasting;
   [SerializeField] private UnityEvent StartCasting;

    private Bullet bullet;

    private void Awake()
    {
        ammo = GetComponent<Ammo>();
        bullet = bulletPrefab[selectedBullet].GetComponent<Bullet>();
        castingSliderRectTransform = castingSlider.GetComponent<RectTransform>();
        sliderScript = castingSlider.GetComponent<SpellCooldown>();

        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }

    }

    void Update()
    {
        ChooseSpell();
        
        if (Input.GetKeyDown(KeyCode.Mouse0) && castCooldownAllow)
        {
            autofirePrevent = false;

            if (currentlySelectedCastDuration == 0)
            {
                Shoot(); //instant Cast
            }
        }

        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            if (castCooldownAllow)
            {
                castingPercentage = 0;
                spellCooldown.DisableComponents();
                CancelCasting.Invoke();
            }

            if (castLoaded && !Input.GetKey(KeyCode.Mouse1))
            {
                Shoot();
            }
        }
        

        if (Input.GetKey(KeyCode.Mouse0) && castCooldownAllow && currentlySelectedCastDuration != 0 && !autofirePrevent && !Input.GetKey(KeyCode.Mouse1))
        {
            CastingSpell(currentlySelectedCastDuration); //long Casting
        }


        if (Input.GetKeyDown(KeyCode.Mouse1) && castLoaded)
        {
            castingPercentage = 0;
            spellCooldown.DisableComponents();
            CancelCasting.Invoke();
            autofirePrevent = false;
        }

    }

    private void Shoot()
    {
        CancelCasting.Invoke();
        castLoaded = false;
        castingPercentage = 0;
        autofirePrevent = true;

        if (ammo.ammo >= bullet.magickaCost) 
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
        noManaLightAnimation.LightAnimation();
        spellcastInstance = RuntimeManager.CreateInstance(shootFailEvent);
        spellcastInstance.start();
    }

    private void SuccesfulShoot()
    {
        castCooldownTime = bullet.myCooldown;
        shootEvent.Invoke();
        ammo.ammo -= bullet.magickaCost;

        spellCooldown.enabled = true;
        spellCooldown.selectedSpellCooldownTime = bullet.myCooldown;
        spellCooldown.StartCooldowning();

        var spawnedBullet = Instantiate(bulletPrefab[selectedBullet], bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        spawnedBullet.GetComponent<Rigidbody>().velocity = bulletSpawnPoint.forward * bullet.speed;
        Bullet spawnedBulletScript = spawnedBullet.GetComponent<Bullet>();

        if (ammo.ammo <= ammo.maxAmmo*0.15f) { //WHEN MAGICKA IS BELOW 15% THE SHOT WILL ALWAYS BE CRITICAL
            spawnedBulletScript.iWillBeCritical = true;
        }
        else
        {
            spawnedBulletScript.iWillBeCritical = false;
        }

        ammoDisplay.RefreshManaValue();
        StartCoroutine(CastCooldown());
    }


    private void ChooseSpell()
    {

        if (!Input.GetKey(KeyCode.Mouse0))
        { 

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                    unlockedSpells.SelectingUnlockedAuraAnimation(0);
                    selectedBullet = 0;
                    SelectSpell();
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                if (unlockedSpells.spellUnlocked[1] == true)
                {
                    unlockedSpells.SelectingUnlockedAuraAnimation(1);
                    selectedBullet = 1;
                    SelectSpell();
                }
                else
                {
                    RuntimeManager.PlayOneShot(spellLockedEvent);
                    unlockedSpells.spellLockedIconAnimation(1);
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                if (unlockedSpells.spellUnlocked[2] == true)
                {
                    unlockedSpells.SelectingUnlockedAuraAnimation(2);
                    selectedBullet = 2;
                    SelectSpell();
                }
                else
                {
                    RuntimeManager.PlayOneShot(spellLockedEvent);
                    unlockedSpells.spellLockedIconAnimation(2);
                }
            }

        }
    }


    private void SelectSpell()
    {
        bullet = bulletPrefab[selectedBullet].GetComponent<Bullet>();
        currentlySelectedCastDuration = bullet.castDuration;
        changeSelectedSpell.Invoke();
        RuntimeManager.PlayOneShot(bulletPrefab[selectedBullet].GetComponentInChildren<Bullet>().pullupSound);
    }

    private IEnumerator CastCooldown()
    {
        castCooldownAllow = false;
        yield return new WaitForSeconds(castCooldownTime);
        castCooldownAllow = true;
    }


    private void CastingSpell(float castDuration)
    {
        if (ammo.ammo >= bullet.magickaCost)
        {
            if (castingPercentage < currentlySelectedCastDuration)
            {
                if (castingPercentage == 0)
                {
                    //one signal tick
                    var spawnedCastingParticle = Instantiate(bullet.CastingParticle, handCastingPoint);
                    StartCasting.Invoke();
                    spellCooldown.EnableComponents();
                }
                //constant ticks during casting
                castLoaded = false; 
                castingPercentage += Time.deltaTime;
                castingSlider.value = castingPercentage / castDuration;
            }
            else
            {
                if (!castLoaded) {
                    castingCompleted.Invoke(); //single signal after fully loading cast
                    castLoaded = true;
                }
            }
        }
        else
        {
            ShootFail();
            autofirePrevent = true;
        }
    }

 }

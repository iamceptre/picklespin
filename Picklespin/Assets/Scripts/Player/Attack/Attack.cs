using UnityEngine;
using FMODUnity;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class Attack : MonoBehaviour
{
    public static Attack instance { get; private set; }

    [SerializeField] private Transform handCastingPoint;

    [SerializeField] private UnityEvent shootEvent;
    [SerializeField] private UnityEvent changeSelectedSpell;

    private Ammo ammo;
    AmmoDisplay ammoDisplay;

    [SerializeField] private EventReference shootFailEvent;
    [SerializeField] private EventReference spellLockedEvent;
    private FMOD.Studio.EventInstance spellcastInstance;

    [SerializeField] private Transform bulletSpawnPoint;
    public GameObject[] bulletPrefab;
    public int selectedBullet;

    [HideInInspector] public float castCooldownTime = 0.1f;
    public bool castCooldownAllow = true;

    [SerializeField] private UnlockedSpells unlockedSpells;
    [SerializeField] private SpellCooldown spellCooldown;

    [SerializeField] private NoManaLightAnimation noManaLightAnimation;

    //Long Casting
    [HideInInspector] public float castingProgress = 0;
    private float currentlySelectedCastDuration;
    [SerializeField] private Slider castingSlider;

    private bool castLoaded = false;

    [SerializeField] private UnityEvent castingCompleted;
    [SerializeField] private UnityEvent CancelCasting;
    [SerializeField] private UnityEvent StartCasting;

    private Bullet currentBullet;

    private RecoilMultiplier recoilMultiplier;

    private void Awake()
    {
        ammo = GetComponent<Ammo>();
        currentBullet = bulletPrefab[selectedBullet].GetComponent<Bullet>();

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
        ammoDisplay = AmmoDisplay.instance;
        recoilMultiplier = RecoilMultiplier.instance;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && castCooldownAllow)
        {
            if (currentlySelectedCastDuration == 0)
            {
                Shoot(); //instant Cast
                return;
            }
            else
            {
                if (!Input.GetKey(KeyCode.Mouse1))
                {
                    StartCoroutine(SpellCasting(currentlySelectedCastDuration));
                }
            }
        }

        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            if (castCooldownAllow)
            {
                spellCooldown.DisableComponents();
                CancelCasting.Invoke();
            }

            if (castLoaded && !Input.GetKey(KeyCode.Mouse1))
            {
                Shoot();
            }
        }


        if (Input.GetKeyDown(KeyCode.Mouse1) && castLoaded)
        {
            spellCooldown.DisableComponents();
            CancelCasting.Invoke();
        }

    }

    private void Shoot()
    {
        CancelCasting.Invoke();
        castLoaded = false;
        spellCooldown.DisableComponents();

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
        noManaLightAnimation.LightAnimation();
        spellcastInstance = RuntimeManager.CreateInstance(shootFailEvent);
        spellcastInstance.start();
    }

    private void SuccesfulShoot()
    {
        castCooldownTime = currentBullet.myCooldown;
        shootEvent.Invoke();
        ammo.ammo -= currentBullet.magickaCost;

        var spawnedBullet = Instantiate(bulletPrefab[selectedBullet], bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        Bullet bulletScript = spawnedBullet.GetComponent<Bullet>();
        bulletScript.handCastingPoint = handCastingPoint;
        recoilMultiplier.UpdateRecoil();

        Vector3 randomDirection = new Vector3(
    Random.Range(-recoilMultiplier.currentRecoil, recoilMultiplier.currentRecoil),
    Random.Range(-recoilMultiplier.currentRecoil, recoilMultiplier.currentRecoil),
    Random.Range(-recoilMultiplier.currentRecoil, recoilMultiplier.currentRecoil)
);

        randomDirection = randomDirection.normalized * (recoilMultiplier.currentRecoil * Mathf.Deg2Rad);

        Vector3 desiredDirection = bulletSpawnPoint.forward + randomDirection;

        spawnedBullet.GetComponent<Rigidbody>().velocity = desiredDirection * currentBullet.speed;
        Bullet spawnedBulletScript = spawnedBullet.GetComponent<Bullet>();

        if (ammo.ammo <= ammo.maxAmmo * 0.15f)
        { //WHEN MANA IS BELOW 15% THE SHOT WILL ALWAYS BE CRITICAL
            spawnedBulletScript.iWillBeCritical = true;
        }
        else
        {
            spawnedBulletScript.iWillBeCritical = false;
        }

        ammoDisplay.Refresh(false);
        spellCooldown.StartCooldown(castCooldownTime);
    }


    public void SelectSpell(int selectedSpell)
    {
        selectedBullet = selectedSpell;
        currentBullet = bulletPrefab[selectedBullet].GetComponent<Bullet>();
        currentlySelectedCastDuration = currentBullet.castDuration;
        changeSelectedSpell.Invoke();
        RuntimeManager.PlayOneShot(currentBullet.pullupSound);
    }

    private IEnumerator SpellCasting(float castDuration)
    {
        if (ammo.ammo >= currentBullet.magickaCost)
        {
            castingSlider.value = 0;
            castingProgress = 0;
            var spawnedCastingParticle = Instantiate(currentBullet.CastingParticle, handCastingPoint);
            StartCasting.Invoke();
            spellCooldown.myCanvas.enabled = true;
            castLoaded = false;

            while (true)
            {
                if (castingProgress < currentlySelectedCastDuration)
                {
                    castingProgress += Time.deltaTime;
                    castingSlider.value = castingProgress / castDuration;
                }
                else
                {
                    castingCompleted.Invoke();
                    castLoaded = true;
                    yield break;
                }

                yield return null;
            }
        }
        else
        {
            ShootFail();
            yield break;
        }
    }

}

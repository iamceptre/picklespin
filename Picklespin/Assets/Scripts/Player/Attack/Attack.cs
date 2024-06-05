using UnityEngine;
using FMODUnity;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using FMOD.Studio;

public class Attack : MonoBehaviour
{
    public static Attack instance { get; private set; }

    private PlayCastBlast playCastBlast;
    private SpellProjectileSpawner spellProjectileSpawner;

    [SerializeField] private Transform handCastingPoint;

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

    private bool castLoaded = false;

    [SerializeField] private UnityEvent castingCompleted;
    [SerializeField] private UnityEvent CancelCasting;
    [SerializeField] private UnityEvent StartCasting;

    public Bullet currentBullet;

    private void Awake()
    {
        ammo = GetComponent<Ammo>();

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
        playCastBlast = PlayCastBlast.instance;
        ammoDisplay = AmmoDisplay.instance;
        spellProjectileSpawner = SpellProjectileSpawner.instance;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && castCooldownAllow)
        {
            if (currentlySelectedCastDuration == 0)
            {
                TryShoot(); //instant Cast
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
                TryShoot();
            }
        }


        if (Input.GetKeyDown(KeyCode.Mouse1) && castLoaded)
        {
            spellCooldown.DisableComponents();
            CancelCasting.Invoke();
        }

    }

    private void TryShoot()
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
        if (playCastBlast.castingParticles[selectedBulletIndex] != null) {
            playCastBlast.StopCastingParticles(selectedBulletIndex);
        }

        playCastBlast.Play(selectedBulletIndex);
        castCooldownTime = currentBullet.myCooldown;
        shootEvent.Invoke();
        ammo.ammo -= currentBullet.magickaCost;

        spellProjectileSpawner.SpawnSpell(selectedBulletIndex);

        ammoDisplay.Refresh(false);
        spellCooldown.StartCooldown(castCooldownTime);
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

    private IEnumerator SpellCasting(float castDuration) //longer casting
    {
        if (ammo.ammo >= currentBullet.magickaCost)
        {
            castingSlider.value = 0;
            castingProgress = 0;
            playCastBlast.StartCastingParticles(selectedBulletIndex);
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

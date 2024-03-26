using UnityEngine;
using FMODUnity;
using UnityEngine.Events;
using System.Collections;

public class Attack : MonoBehaviour
{
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

     [HideInInspector]public float castCooldownTime = 0.1f;
    private bool castCooldownAllow = true;

    [SerializeField] private UnlockedSpells unlockedSpells;
    [SerializeField] private SpellCooldown spellCooldown;

    [SerializeField] private NoManaLightAnimation noManaLightAnimation;

    private void Start()
    {
        ammo = GetComponent<Ammo>();
    }

    void Update()
    {
        ChooseSpell();

        if (Input.GetKeyDown(KeyCode.Mouse0) && castCooldownAllow)
        {
            Shoot();
            StartCoroutine(CastCooldown());
        }
    }

    private void Shoot()
    {
        Bullet bullet = bulletPrefab[selectedBullet].GetComponent<Bullet>();


            if (ammo.ammo >= bullet.magickaCost) //Shooting
            {
                castCooldownTime = bullet.myCooldown;
                shootEvent.Invoke();
                ammo.ammo -= bullet.magickaCost;

                spellCooldown.enabled = true;
                spellCooldown.selectedSpellCooldownTime = bullet.myCooldown;
                spellCooldown.StartCooldowning();

            var spawnedBullet = Instantiate(bulletPrefab[selectedBullet], bulletSpawnPoint.position, bulletSpawnPoint.rotation);
                spawnedBullet.GetComponent<Rigidbody>().velocity = bulletSpawnPoint.forward * bullet.speed;
                ammoDisplay.RefreshManaValue();
            }
            else //Shoot Failing
            {
                noManaLightAnimation.LightAnimation();
                spellcastInstance = RuntimeManager.CreateInstance(shootFailEvent);
                spellcastInstance.start();
            }
    }


    private void ChooseSpell()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && selectedBullet != 0)
        {
            selectedBullet = 0;
            SelectSpell();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) && selectedBullet != 1)
        {
            if (unlockedSpells.spellUnlocked[1] == true)
            {
                selectedBullet = 1;
                SelectSpell();
            }
            else
            {
                RuntimeManager.PlayOneShot(spellLockedEvent);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha3) && selectedBullet != 2)
        {
            if (unlockedSpells.spellUnlocked[2] == true)
            {
                selectedBullet = 2;
                SelectSpell();
            }
            else
            {
                RuntimeManager.PlayOneShot(spellLockedEvent);
            }

        }
    }


    private void SelectSpell()
    {
            changeSelectedSpell.Invoke();
            RuntimeManager.PlayOneShot(bulletPrefab[selectedBullet].GetComponentInChildren<Bullet>().pullupSound);
    }

    IEnumerator CastCooldown()
    {
        castCooldownAllow = false;
        yield return new WaitForSeconds(castCooldownTime);
        castCooldownAllow = true;
    }

}

using UnityEngine;
using FMODUnity;
using UnityEngine.Events;
using System.Collections;

public class Attack : MonoBehaviour
{
    public AmmoDisplay ammoDisplay;
   [SerializeField] private UnityEvent castCameraShake;
   [SerializeField] private UnityEvent changeSelectedSpell;

    private Ammo ammo;

    [SerializeField] private EventReference shootFailEvent;
    private FMOD.Studio.EventInstance spellcastInstance;

    [SerializeField] private Transform bulletSpawnPoint;
    public GameObject[] bulletPrefab;
    public int selectedBullet;

    private float castCooldownTime = 0.1f;
    private bool castCooldownAllow = true;

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
                castCameraShake.Invoke();
                ammo.ammo -= bullet.magickaCost;
                var spawnedBullet = Instantiate(bulletPrefab[selectedBullet], bulletSpawnPoint.position, bulletSpawnPoint.rotation);
                spawnedBullet.GetComponent<Rigidbody>().velocity = bulletSpawnPoint.forward * bullet.speed;
            }
            else //Shoot Failing
            {
                spellcastInstance = RuntimeManager.CreateInstance(shootFailEvent);
                spellcastInstance.start();
            }
    }


    private void ChooseSpell()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            selectedBullet = 0;
            SelectSpell();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selectedBullet = 1;
            SelectSpell();
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

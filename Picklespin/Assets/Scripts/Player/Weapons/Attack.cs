using UnityEngine;
using FMODUnity;

public class Attack : MonoBehaviour
{
    public AmmoDisplay ammoDisplay;
    public CameraShake cameraShake;

    private Ammo ammo;

    [SerializeField] private EventReference shootFailEvent;
    private FMOD.Studio.EventInstance spellcastEvent;

    [SerializeField] private Transform bulletSpawnPoint;
    public GameObject[] bulletPrefab;
    [SerializeField]private int selectedBullet;

    private void Start()
    {
        ammo = GetComponent<Ammo>();
    }

    void Update()
    {
        ChooseSpell();

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Shoot(); 
        } 
    }

    private void Shoot()
    {
        Bullet bullet = bulletPrefab[selectedBullet].GetComponent<Bullet>();

        if (ammo.ammo >= bullet.magickaCost) //Shooting
        {
            cameraShake.shakeMultiplier = 4;
            CameraShake.Invoke(); //REPLACE THE SHAKE WITH AN EVENT INVOKE OF SPECIFIC FUNTION IN CAMERA SHAKE CLASS

            ammo.ammo -= bullet.magickaCost;
            var spawnedBullet = Instantiate(bulletPrefab[selectedBullet], bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            spawnedBullet.GetComponent<Rigidbody>().velocity = bulletSpawnPoint.forward * bullet.speed;
        }
        else //Shoot Failing
        {
            spellcastEvent = RuntimeManager.CreateInstance(shootFailEvent);
            spellcastEvent.start();
        }
    }


    private void ChooseSpell()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            selectedBullet = 0;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selectedBullet = 1;
        }
    }


}

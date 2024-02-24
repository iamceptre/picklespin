using UnityEngine;
using FMODUnity;

public class Attack : MonoBehaviour
{
    public AmmoDisplay ammoDisplay;

    private Ammo ammo;
    public int currentAmmoCost;

    public EventReference shootEvent;
    public EventReference shootFailEvent;
    private FMOD.Studio.EventInstance shootInstance;

    [SerializeField] private Transform bulletSpawnPoint;
    public GameObject[] bulletPrefab;
    public int selectedBullet;
    public float bulletSpeed;

    private void Start()
    {
        ammo = GetComponent<Ammo>();
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (ammo.ammo >= currentAmmoCost) //Shooting
            {
                ammo.ammo -= currentAmmoCost;
                var bullet = Instantiate(bulletPrefab[selectedBullet], bulletSpawnPoint.position, bulletSpawnPoint.rotation);
                bullet.GetComponent<Rigidbody>().velocity = bulletSpawnPoint.forward * bulletSpeed;
                shootInstance = RuntimeManager.CreateInstance(shootEvent); //if audio event 3d, assign 3d space for the sounds
                shootInstance.start();

            }
            else //Shoot Failing
            {
                shootInstance = RuntimeManager.CreateInstance(shootFailEvent);
                shootInstance.start();
            }
 
        }

        
    }


}

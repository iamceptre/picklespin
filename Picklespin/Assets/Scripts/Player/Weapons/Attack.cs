using UnityEngine;
using FMODUnity;

public class Attack : MonoBehaviour
{

    private Ammo ammo;
    public int currentAmmoCost;
    public AmmoDisplay ammoDisplay;

    public EventReference shootEvent;
    public EventReference shootFailEvent;
    private FMOD.Studio.EventInstance shootInstance;

    [SerializeField]private Transform bulletSpawnPoint;
    public GameObject bulletPrefab;
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
                var bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
                bullet.GetComponent<Rigidbody>().velocity = bulletSpawnPoint.forward * bulletSpeed;
                shootInstance = RuntimeManager.CreateInstance(shootEvent);
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

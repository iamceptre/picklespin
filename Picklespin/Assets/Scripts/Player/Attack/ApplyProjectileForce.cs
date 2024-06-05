using UnityEngine;

public class ApplyProjectileForce : MonoBehaviour
{

    private Rigidbody rb;
    private RecoilMultiplier recoilMultiplier;
    private CachedCameraMain cachedCameraMain;
    private Bullet bullet;

    private bool gotRefs = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        bullet = GetComponent<Bullet>();
    }



    public void Set()
    {
        if (!gotRefs)
        {
            recoilMultiplier = RecoilMultiplier.instance;
            cachedCameraMain = CachedCameraMain.instance;
            gotRefs = true;
        }

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        //Debug.Log("setting rb");
        recoilMultiplier.UpdateRecoil();
        Vector3 randomDirection = new Vector3(
Random.Range(-recoilMultiplier.currentRecoil, recoilMultiplier.currentRecoil),
Random.Range(-recoilMultiplier.currentRecoil, recoilMultiplier.currentRecoil),
Random.Range(-recoilMultiplier.currentRecoil, recoilMultiplier.currentRecoil)
);

        Vector3 desiredDirection = cachedCameraMain.cachedTransform.forward;//+ randomDirection;
        rb.velocity = desiredDirection * bullet.speed;
    }

}

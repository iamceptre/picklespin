using UnityEngine;

public class CollapseBridgeIfNoSupport : MonoBehaviour
{

    private Rigidbody[] rb;

    private DisablePhysicsAfterTime[] disablePhysicsAfterTime;
    [SerializeField] private int supportsNeededToCollapse; //much many supports needs to be destroyed in order for the bridge to collapse
    private int currentlyDestroyedSupportsCount = 0;
    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponentsInChildren<Rigidbody>();
        disablePhysicsAfterTime = GetComponentsInChildren<DisablePhysicsAfterTime>();
    }


    public void DecreaseSupport()
    {
        //Debug.Log("bridge acknowlegind loss of support");
        currentlyDestroyedSupportsCount++;

        if (currentlyDestroyedSupportsCount >= supportsNeededToCollapse)
        {

            foreach (var rb in rb)
            {
                float randomX = Random.Range(-2, 2);
                float randomY = Random.Range(-2, 2);
                float randomZ = Random.Range(-2, 2);
                Vector3 randomForce = new Vector3(randomX, randomY, randomZ);
                rb.isKinematic = false;
                rb.velocity = randomForce;
            }

            foreach (var physicsFreeze in disablePhysicsAfterTime)
            {
                physicsFreeze.StartCoroutine(physicsFreeze.StartCountdown());
            }
        }
    }
}

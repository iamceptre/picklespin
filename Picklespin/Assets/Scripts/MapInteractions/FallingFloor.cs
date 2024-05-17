using FMOD.Studio;
using FMODUnity;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class FallingFloor : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private EventReference fallingFloorEvent;
    private EventInstance fallingFloorEventInstance;

    [SerializeField] private float timeBeforeFloorFalls = 2;

    [Range(0, 10)] private float fallingFloorCountdown;

    private GameObject currentlyOccupiedFloor;
    private GameObject previouslyOccupiedFloor;

    [SerializeField] private LayerMask fallingFloorLayer;
    [SerializeField] private DisablePhysicsAfterTime disablePhysicsAfterTime;

    private Tween floorShakeTween;

    private void Start()
    {
        fallingFloorEventInstance = RuntimeManager.CreateInstance(fallingFloorEvent);
        InvokeRepeating("RaycastCheck", 0, 0.2f);
    }

    private void RaycastCheck()
    {
        Vector3 rayStart = transform.position;
        Vector3 rayDirection = Vector3.down;
        float rayDistance = 2;

        RaycastHit hit;

        Debug.DrawRay(rayStart, rayDirection * rayDistance, Color.red);

        if (Physics.Raycast(rayStart, rayDirection, out hit, rayDistance, fallingFloorLayer))
        {
            currentlyOccupiedFloor = hit.transform.gameObject;
        }
        else
        {
            floorShakeTween.Kill();
            floorShakeTween = null;
            fallingFloorCountdown = timeBeforeFloorFalls;
            return;
        }

        if (currentlyOccupiedFloor != previouslyOccupiedFloor)
        {
            OccupiedFloorChanged();
            previouslyOccupiedFloor = currentlyOccupiedFloor;
        }
    }

    private void OccupiedFloorChanged()
    {
        StopAllCoroutines();
        StartCoroutine(BeforeFallTimer());
        floorShakeTween.Kill();
        floorShakeTween = null;
        if (previouslyOccupiedFloor != null)
        {
            previouslyOccupiedFloor.transform.rotation = Quaternion.identity;
        }
        fallingFloorCountdown = timeBeforeFloorFalls;
    }

    private void isFallingFloorSupportingAbridge()
    {
        if (currentlyOccupiedFloor.TryGetComponent<BridgeSupport>(out BridgeSupport bridgeSupport))
        {
            //Debug.Log("tryGetComp");
            bridgeSupport.myCollapse();
        }

    }

    private IEnumerator BeforeFallTimer()
    {
        while (fallingFloorCountdown > 0)
        {
            fallingFloorCountdown -= Time.deltaTime;

            if (fallingFloorCountdown < timeBeforeFloorFalls * 0.5f && floorShakeTween == null)
            {
                floorShakeTween = currentlyOccupiedFloor.transform.DOShakeRotation(timeBeforeFloorFalls * 0.5f, 1, 40, 90, false);
            }

            if (fallingFloorCountdown <= 0)
            {
                Fall();
                StopAllCoroutines();
            }
            yield return null;
        }
    }




    private void Fall()
    {
        fallingFloorCountdown = timeBeforeFloorFalls;

        rb = currentlyOccupiedFloor.AddComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        disablePhysicsAfterTime = currentlyOccupiedFloor.GetComponent<DisablePhysicsAfterTime>();
        disablePhysicsAfterTime.StartCoroutine(disablePhysicsAfterTime.StartCountdown());

        rb.velocity = Vector3.down;

        isFallingFloorSupportingAbridge();

        //gameObject.isStatic = false; make it work

        float maxRandomRotation = 15;
        Vector3 randomRot = new Vector3(Random.Range(0f, maxRandomRotation),
            Random.Range(0f, maxRandomRotation),
            Random.Range(0f, maxRandomRotation));
        Transform floorTransform = currentlyOccupiedFloor.transform;
        floorTransform.DOLocalRotate(randomRot, 0.2f);
        Vector3 scaleVector = new Vector3(floorTransform.localScale.x * 0.8f, floorTransform.localScale.y * 0.8f, floorTransform.localScale.z * 0.8f);
        floorTransform.DOScale(scaleVector, 0.2f);

        fallingFloorEventInstance.set3DAttributes(RuntimeUtils.To3DAttributes(currentlyOccupiedFloor));
        fallingFloorEventInstance.start();
    }


}

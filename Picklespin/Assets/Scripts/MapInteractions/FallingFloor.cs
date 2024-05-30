using FMOD.Studio;
using FMODUnity;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class FallingFloor : MonoBehaviour
{
    //private Rigidbody rb;
    [SerializeField] private EventReference fallingFloorEvent;
    private EventInstance fallingFloorEventInstance;

    [SerializeField] private float timeBeforeFloorFalls = 2;

    [Range(0, 10)] private float fallingFloorCountdown;

    private GameObject currentlyOccupiedFloor;
    private GameObject previouslyOccupiedFloor;

    [SerializeField] private LayerMask fallingFloorLayer;

    private Tween floorShakeTween;

    private void Start()
    {
        fallingFloorEventInstance = RuntimeManager.CreateInstance(fallingFloorEvent);
        InvokeRepeating("RaycastCheck", 0, 0.2f);
    }

    private void RaycastCheck()
    {
        if (transform.position.y < 5)
        {
            return;
        }

            Vector3 rayStart = transform.position;
            Vector3 rayDirection = Vector3.down;
            float rayDistance = 3;

            RaycastHit hit;

            if (Physics.Raycast(rayStart, rayDirection, out hit, rayDistance, fallingFloorLayer))
            {
                currentlyOccupiedFloor = hit.transform.gameObject;
            }
            else
            {
                floorShakeTween.Kill();
                StopAllCoroutines();
                floorShakeTween = null;
                fallingFloorCountdown = timeBeforeFloorFalls;
                currentlyOccupiedFloor = null;
                previouslyOccupiedFloor = null;
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
                AlmostFallingIncidations();
            }

            if (fallingFloorCountdown <= 0)
            {
                Fall();
                StopAllCoroutines();
            }
            yield return null;
        }
    }


    private void AlmostFallingIncidations()
    {
        ParticleSystem almostFaliingParticle = currentlyOccupiedFloor.GetComponentInChildren<ParticleSystem>();
        almostFaliingParticle.Stop();
        almostFaliingParticle.Clear();
        ParticleSystem.MainModule main = almostFaliingParticle.main;
        main.duration = timeBeforeFloorFalls * 0.5f;
        almostFaliingParticle.Play();
        floorShakeTween = currentlyOccupiedFloor.transform.DOShakeRotation(timeBeforeFloorFalls * 0.5f, 1, 40, 90, false);
    }



    private void Fall()
    {
        fallingFloorCountdown = timeBeforeFloorFalls;

        Rigidbody rb = currentlyOccupiedFloor.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        currentlyOccupiedFloor.GetComponent<DisablePhysicsAfterTime>().enabled = true;

        rb.velocity = Vector3.down + ApplyRandomForce();

        isFallingFloorSupportingAbridge();

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



    private Vector3 ApplyRandomForce()
    {
        Vector3 randomForce = new Vector3(
            Random.Range(-1f, 1f),
            0,
            Random.Range(-1f, 1f)
        ) * 3;

        return randomForce;
    }


}

using System.Collections;
using UnityEngine;

public class DisablePhysicsAfterTime : MonoBehaviour
{
    [SerializeField] private float timeBeforeFreezing = 3;

    [SerializeField] private FallingFloorCoyoteTime coyoteTime;

    private void OnEnable()
    {
        if (coyoteTime != null)
        {
            coyoteTime.enabled = true;
        }
        StartCoroutine(StartCountdown());
    }

    private IEnumerator StartCountdown()
    {
        yield return new WaitForSeconds(timeBeforeFreezing);
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        rb.isKinematic = true;
        enabled = false;
        yield break;
    }
}

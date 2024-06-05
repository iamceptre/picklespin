using System.Collections;
using UnityEngine;

public class DisablePhysicsAfterTime : MonoBehaviour
{
    private WaitForSeconds timeBeforeFreezing = new WaitForSeconds(3);

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
        yield return timeBeforeFreezing;
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        rb.isKinematic = true;
        enabled = false;
        yield break;
    }
}

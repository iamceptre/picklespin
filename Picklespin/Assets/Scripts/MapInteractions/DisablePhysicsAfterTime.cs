using System.Collections;
using UnityEngine;

public class DisablePhysicsAfterTime : MonoBehaviour
{
    [SerializeField] private float timeBeforeFreezing = 3;

    private void OnEnable()
    {
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

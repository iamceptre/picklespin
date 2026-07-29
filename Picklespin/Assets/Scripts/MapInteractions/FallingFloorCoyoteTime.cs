using UnityEngine;
using System.Collections;

public class FallingFloorCoyoteTime : MonoBehaviour
{
    [SerializeField] private LayerMask playerLayer;
    private Rigidbody rb;

    private WaitForSeconds coyoteTime = new WaitForSeconds(0.15f);

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        StartCoroutine(WaitAndExclude());
    }

    private IEnumerator WaitAndExclude()
    {
        yield return coyoteTime;
        rb.excludeLayers = playerLayer;
        enabled = false;
    }

}

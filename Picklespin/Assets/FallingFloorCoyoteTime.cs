using UnityEngine;
using System.Collections;

public class FallingFloorCoyoteTime : MonoBehaviour
{
    [SerializeField] private LayerMask playerLayer;
    private Rigidbody rb;

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
        yield return new WaitForSeconds(0.15f);
        rb.excludeLayers = playerLayer;
        enabled = false;
    }

}

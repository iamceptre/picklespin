using System.Collections;
using UnityEngine;

public class AngelRingFalloff : MonoBehaviour
{
    [SerializeField] private Collider[] _collider;
    [SerializeField] private Rigidbody[] _rb;
    [SerializeField] private TrailRenderer[] _trail;
    [SerializeField] private Animator animator;

    private WaitForSeconds timeBeforeDisablingCollider = new WaitForSeconds(3);


    public void Falloff()
    {
        transform.SetParent(null);
        animator.StopPlayback();
        animator.enabled = false;

        for (int i = 0; i < _collider.Length; i++)
        {
            _rb[i].isKinematic = false;
            _collider[i].enabled = true;
            _trail[i].enabled = false;
        }

        StartCoroutine(WaitAndDisableCollider());
    }

    
    private IEnumerator WaitAndDisableCollider()
    {
        yield return timeBeforeDisablingCollider;

        for (int i = 0; i < _collider.Length; i++)
        {
            _rb[i].isKinematic = true;
            _collider[i].enabled = false;
        }
    }
}

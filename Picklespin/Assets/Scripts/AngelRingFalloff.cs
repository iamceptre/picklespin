using System.Collections;
using UnityEngine;

public class AngelRingFalloff : MonoBehaviour
{
    [SerializeField] private Collider[] _collider;
    [SerializeField] private Rigidbody[] _rb;
    [SerializeField] private TrailRenderer[] _trail;
    [SerializeField] private PlayFmodOnCollision[] _playFmodOnCollision;
    [SerializeField] private Animator animator;

    private WaitForSeconds timeBeforeDisablingCollider = new WaitForSeconds(5);


    public void Falloff()
    {
        animator.StopPlayback();
        animator.enabled = false;
        transform.SetParent(null);

        for (int i = 0; i < _collider.Length; i++)
        {
            _rb[i].isKinematic = false;
            _rb[i].velocity = Vector3.one * 1;
            _collider[i].enabled = true;
            _trail[i].enabled = false;
            _playFmodOnCollision[i].enabled = true;
        }

        _rb[2].rotation = new Quaternion(30, _rb[2].rotation.y, _rb[2].rotation.z, _rb[2].rotation.w).normalized;

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

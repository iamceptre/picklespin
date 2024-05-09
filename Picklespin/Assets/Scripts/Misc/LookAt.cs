using UnityEngine;

public class LookAt : MonoBehaviour
{

    [SerializeField] private Transform whatToLookAt;

    private void Awake()
    {
        if (whatToLookAt == null)
        {
            //whatToLookAt = GameObject.FindGameObjectWithTag("MainCamera").transform;
            whatToLookAt = Camera.main.transform;
        }
    }

    private void LateUpdate()
    {
        transform.LookAt(whatToLookAt);
    }

}

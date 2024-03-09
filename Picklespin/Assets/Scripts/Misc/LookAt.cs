using UnityEngine;

public class LookAt : MonoBehaviour
{

    [SerializeField] private Transform whatToLookAt;

    private void Update()
    {
        transform.LookAt(whatToLookAt);
    }

}

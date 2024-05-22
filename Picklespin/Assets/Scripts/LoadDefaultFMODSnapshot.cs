using FMODUnity;
using UnityEngine;

public class LoadDefaultFMODSnapshot : MonoBehaviour
{
    [SerializeField] private EventReference defaultSnapshot;
    void Start()
    {
        RuntimeManager.PlayOneShot(defaultSnapshot);
    }

}

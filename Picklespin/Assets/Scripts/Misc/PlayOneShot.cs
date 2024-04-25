using UnityEngine;
using FMODUnity;

public class PlayOneShot : MonoBehaviour
{

    [SerializeField] private EventReference sound;

    void Start()
    {
        RuntimeManager.PlayOneShot(sound); 
    }
}

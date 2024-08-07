using FMODUnity;
using UnityEngine;

public class PauseMyEmitters : MonoBehaviour
{

    [SerializeField] private StudioEventEmitter[] emmiters;


    public void Pause()
    {

        for (int i = 0; i < emmiters.Length; i++)
        {
            emmiters[i].Stop();
        }

    }

}

using UnityEngine;
using FMODUnity;
using System.Collections;

public class AngelMind : MonoBehaviour
{

    [Range(0, 100)] public float hp = 1;
    public bool healed = false;
    public bool isDead = false;

    public EventReference angelHealedEvent;
    private FMOD.Studio.EventInstance angelInstance;

    private bool routineRunning = false;



    void Update()
    {

        if (healed && ! routineRunning)
        {
            StartCoroutine(AfterHealedAction());
        }

        IEnumerator AfterHealedAction(){
            angelInstance = RuntimeManager.CreateInstance(angelHealedEvent);
            routineRunning = true;
            transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            angelInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
            angelInstance.start();
            yield return null;
        }

    }

    //add death when hp reaches 0, update it only when something is happeninig, not on every frame

}

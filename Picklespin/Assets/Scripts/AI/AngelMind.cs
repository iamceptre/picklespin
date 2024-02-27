using UnityEngine;
using FMODUnity;
using System.Collections;

public class AngelMind : MonoBehaviour
{

    public bool healed = false;
    public bool isDead = false;

    public EventReference angelHealedEvent;
    private FMOD.Studio.EventInstance angelInstance;

    [SerializeField] private GameObject healedParticles;

    private Renderer angelRenderer;

    private void Start()
    {
        angelRenderer = gameObject.GetComponent<Renderer>();
    }


       public IEnumerator AfterHealedAction(){
            Instantiate(healedParticles, transform.position, Quaternion.identity);
            angelInstance = RuntimeManager.CreateInstance(angelHealedEvent);
            angelRenderer.material.SetColor("_Color", Color.green);
            angelInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
            angelInstance.start();
            yield return null;
        }

}

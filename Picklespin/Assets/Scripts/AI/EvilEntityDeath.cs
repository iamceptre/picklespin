using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.Events;

public class EvilEntityDeath : MonoBehaviour
{
    [SerializeField] private EventReference evilEntityDeathSound;
    private EventInstance evilEntityDeathSoundReference;
    [SerializeField] private GameObject aiUi;
    [SerializeField] private UnityEvent deathEvent;
    [SerializeField] private AiHealthUiBar AiHealthUiBar;
    public void Die()
    {

        if (aiUi == null)
        {
            aiUi = transform.Find("AI_UI").gameObject;
        }

        deathEvent.Invoke(); //custom death behaviour
        aiUi.transform.SetParent(null);
        AiHealthUiBar.FadeOut();
        aiUi.transform.position += new Vector3(0,0.75f);
        gameObject.SetActive(false);
        evilEntityDeathSoundReference = RuntimeManager.CreateInstance(evilEntityDeathSound);
        RuntimeManager.AttachInstanceToGameObject(evilEntityDeathSoundReference, GetComponent<Transform>());
        evilEntityDeathSoundReference.start();
    }
}

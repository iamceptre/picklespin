using UnityEngine;
using UnityEngine.Events;
using System.Collections;

// The one place an enemy dies. Everything it touches is optional — an enemy
// without a Dissolver, an HP bar or a SetOnFire just skips that part.
public class EvilEntityDeath : MonoBehaviour
{
    [SerializeField] private UnityEvent deathEvent;
    [Tooltip("all auto-found on this object or its children if left empty")]
    [SerializeField] private AiReferences refs;

    private CameraShakeManagerV2 camShakeManager;
    private ScreenFlashTint screenFlashTint;

    // Die() is reachable from Inspector-wired events as well as AiHealth, and
    // nothing downstream is idempotent: a second StartDissolve() would capture
    // the dissolve material as the enemy's "alive" material and the pooled
    // enemy would respawn wearing it.
    private bool died;

    private void Awake()
    {
        if (!refs) refs = GetComponentInChildren<AiReferences>(true);
    }

    private void OnEnable()
    {
        died = false; // pooled reuse
    }

    private void Start()
    {
        camShakeManager = CameraShakeManagerV2.instance;
        screenFlashTint = ScreenFlashTint.instance;
    }


    public void Die()
    {
        if (died) return;
        died = true;

        if (refs)
        {
            // first frame of death: the corpse must stop blocking the player
            refs.DisableAllColliders();

            // stop the corpse thinking, seeing and burning before it dissolves
            if (refs.stateManager) refs.stateManager.StopAI();
            if (refs.Vision) refs.Vision.ResetVisionState();
            if (refs.setOnFire) refs.setOnFire.Extinguish();

            if (refs.HpUiBar)
            {
                refs.HpUiBar.Detach();
                refs.HpUiBar.FadeOut();
            }

            if (refs.Dissolver) refs.Dissolver.StartDissolve();
        }

        if (screenFlashTint) screenFlashTint.Flash(6);
        StartCoroutine(ShakeLater());

        deathEvent.Invoke(); //additional death behaviour
    }


    private IEnumerator ShakeLater()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        if (camShakeManager) camShakeManager.ShakeSelected(6);
    }
}

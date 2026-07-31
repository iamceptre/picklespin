using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class EvilEntityDeath : MonoBehaviour
{
    [SerializeField] private UnityEvent deathEvent;
    [Tooltip("all auto-found on this object or its children if left empty")]
    [SerializeField] private AiReferences refs;

    private CameraShakeManagerV2 camShakeManager;
    private ScreenFlashTint screenFlashTint;

    // Die() is reachable from Inspector events too, and nothing downstream is
    // idempotent: a second StartDissolve() latches the dissolve material for good
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
            refs.DisableAllColliders();

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

        deathEvent.Invoke();
    }


    private IEnumerator ShakeLater()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        if (camShakeManager) camShakeManager.ShakeSelected(6);
    }
}

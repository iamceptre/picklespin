using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class EvilEntityDeath : MonoBehaviour
{
    [SerializeField] private UnityEvent deathEvent;
    [Tooltip("all auto-found on this object or its children if left empty")]
    [SerializeField] private AiReferences refs;
    [SerializeField] private CameraShakeSettings deathShake = new()
    {
        rotationAmount = new Vector3(2.43f, 2.43f, 5f), numberOfShakes = 8, speed = 65f, decay = 0.55f, uiShakeModifier = 1f
    };
    [SerializeField, Tooltip("no death shake or screen flash past this distance from the camera; 0 = at any distance")]
    private float reactionMaxDistance = 40f;

    private CameraShakeManagerV2 camShakeManager;
    private ScreenFlashTint screenFlashTint;

    private bool died;

    private void Awake()
    {
        if (!refs) refs = GetComponentInChildren<AiReferences>(true);
    }

    private void OnEnable()
    {
        died = false;
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

            if (refs.TryGetComponent(out ConvertedAlly ally)) ally.Revert();

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

        if (WithinReactionRange())
        {
            if (screenFlashTint) screenFlashTint.Flash(6);
            StartCoroutine(ShakeLater());
        }

        deathEvent.Invoke();
    }

    private bool WithinReactionRange()
    {
        if (reactionMaxDistance <= 0f) return true;

        var camera = CachedCameraMain.instance;
        return !camera || Vector3.Distance(transform.position, camera.cachedTransform.position) <= reactionMaxDistance;
    }

    private IEnumerator ShakeLater()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        if (camShakeManager) camShakeManager.Shake(deathShake);
    }
}

using UnityEngine;

public class SpellCameraShake : MonoBehaviour
{
    [System.Serializable]
    public class Shake
    {
        public CameraShakeSettings cameraShake = new() { strength = 0f };
        [Tooltip("hand kick strength; 0 = the hand keeps still")]
        public float handStrength;
        public float handDuration = 0.2f;
        public int handVibrato = 10;
    }

    [SerializeField, Tooltip("when the charge-up begins - only a spell with a cast time ever gets here")]
    private Shake castStart = new();
    [SerializeField, Tooltip("when the spell leaves the hand")]
    private Shake shoot = new();
    [SerializeField, Tooltip("when the spell lands")]
    private Shake impact = new();
    [SerializeField, Tooltip("no impact shake past this distance from the camera; 0 = at any distance")]
    private float impactMaxDistance;

    public void PlayCastStart() => Play(castStart);

    public void PlayShoot() => Play(shoot);

    public void PlayImpact(Vector3 impactPoint)
    {
        if (impactMaxDistance > 0f)
        {
            var camera = CachedCameraMain.instance;
            if (camera && Vector3.Distance(impactPoint, camera.cachedTransform.position) > impactMaxDistance) return;
        }

        Play(impact);
    }

    private static void Play(Shake trigger)
    {
        var shakeManager = CameraShakeManagerV2.instance;
        if (trigger == null || !shakeManager) return;

        shakeManager.Shake(trigger.cameraShake);
        if (trigger.handStrength > 0f) shakeManager.ShakeHand(trigger.handStrength, trigger.handDuration, trigger.handVibrato);
    }
}

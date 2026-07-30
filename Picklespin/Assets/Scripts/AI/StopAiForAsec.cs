using Pathfinding;
using System.Collections;
using UnityEngine;

// Freezes the enemy in place for a moment (the player's dash shockwave), with an
// optional effect that runs for exactly as long as the freeze does.
public class StopAiForAsec : MonoBehaviour
{
    [SerializeField] private AIPath aiPath;
    [SerializeField, Tooltip("optional — emits for the whole freeze, then stops emitting and lets its live particles fade out on their own")]
    private ParticleSystem stunParticle;
    [SerializeField] private float stopDuration = 3f;

    private WaitForSeconds waitTime;
    private Coroutine stopRoutine;

    private void Awake()
    {
        waitTime = new WaitForSeconds(stopDuration);
    }

    private void OnEnable()
    {
        // the effect is driven by the freeze alone, never by playOnAwake, and a
        // pooled enemy must not respawn wearing the last one's particles
        if (stunParticle) stunParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void OnDisable()
    {
        // the coroutine dies with the object; make sure the freeze does not
        // outlive it and strand a pooled enemy standing still
        stopRoutine = null;
        if (aiPath) aiPath.isStopped = false;
    }

    public void StopMeForASec()
    {
        // being hit again extends the freeze rather than restarting it — the
        // particles keep streaming instead of re-bursting from the beginning
        if (stopRoutine != null) StopCoroutine(stopRoutine);
        stopRoutine = StartCoroutine(Routine());
    }

    private IEnumerator Routine()
    {
        if (aiPath) aiPath.isStopped = true;
        // Play on an already-running system is a no-op, and on one that is still
        // fading out it simply resumes emission — either way, no visible restart
        if (stunParticle) stunParticle.Play(true);

        yield return waitTime;

        if (aiPath) aiPath.isStopped = false;
        // StopEmitting rather than Clear: emission ends now, particles already in
        // the air live out their lifetime instead of vanishing mid-flight
        if (stunParticle) stunParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        stopRoutine = null;
    }
}

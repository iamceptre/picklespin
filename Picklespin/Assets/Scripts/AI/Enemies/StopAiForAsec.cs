using Pathfinding;
using System.Collections;
using UnityEngine;

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
        // a pooled enemy must not respawn wearing the last one's particles
        if (stunParticle) stunParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void OnDisable()
    {
        stopRoutine = null;
        if (aiPath) aiPath.isStopped = false;
    }

    public void StopMeForASec()
    {
        // being hit again extends the freeze rather than restarting it
        if (stopRoutine != null) StopCoroutine(stopRoutine);
        stopRoutine = StartCoroutine(Routine());
    }

    private IEnumerator Routine()
    {
        if (aiPath) aiPath.isStopped = true;
        // Play on a running system is a no-op and on a fading one resumes emission
        if (stunParticle) stunParticle.Play(true);

        yield return waitTime;

        if (aiPath) aiPath.isStopped = false;
        // StopEmitting, not Clear: particles in the air live out their lifetime
        if (stunParticle) stunParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        stopRoutine = null;
    }
}

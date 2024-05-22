using System.Collections;
using UnityEngine;

public class HealingParticles : MonoBehaviour
{

    private ParticleSystem myParticleSystem;
    private ParticleSystem.MainModule myMain;
    private ParticleSystem.EmissionModule myEmission;

    private ParticleSystem.Particle[] particles;

    private bool isEmitting = false;

    [SerializeField] private ParticleSystem initialHealingParticle;

    private void Awake()
    {
        myParticleSystem = GetComponent<ParticleSystem>();
        myMain = myParticleSystem.main;
        myEmission = myParticleSystem.emission;
    }

    private void Start()
    {
        particles = new ParticleSystem.Particle[myMain.maxParticles];
    }

    public void StartEmitting(Transform currentlyHealingAngel)
    {
        if (!isEmitting)
        {
            isEmitting = true;
            myEmission.enabled = true;
            myParticleSystem.Play();
            initialHealingParticle.Play();
            StartCoroutine(ParticlesMoveTowardsAngel(currentlyHealingAngel));
        }
    }

    public void StopEmitting()
    {
        if (isEmitting)
        {
            isEmitting = false;
            myEmission.enabled=false;
            myParticleSystem.Stop();
            StopAllCoroutines();
        }
    }

    private IEnumerator ParticlesMoveTowardsAngel(Transform currentlyHealingAngel)
    {
        while (isEmitting)
        {
            int numParticlesAlive = myParticleSystem.GetParticles(particles);
            Vector3 targetPosition = currentlyHealingAngel.position;

            for (int i = 0; i < numParticlesAlive; i++)
            {
                Vector3 direction = targetPosition - particles[i].position;
                float distance = direction.magnitude;

                particles[i].velocity = (direction / distance) * 10;

            }

            myParticleSystem.SetParticles(particles, numParticlesAlive);
            yield return null;
        }
    }
}
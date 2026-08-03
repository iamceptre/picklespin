using FMODUnity;
using System.Collections;
using UnityEngine;

public class ParticleMoveTowards : MonoBehaviour
{
    private ParticleSystem myParticleSystem;
    private ParticleSystem.MainModule mainModule;

    private PublicPlayerTransform publicPlayerTransform;

    private ParticleSystem.Particle[] particles;

    private int numParticlesAlive;

    private float velocityMultiplier = 0;

    [SerializeField] private StudioEventEmitter particleSuckedInSoundEmitter;

    private void Awake()
    {
        myParticleSystem = GetComponent<ParticleSystem>();
        mainModule = myParticleSystem.main;
        particles = new ParticleSystem.Particle[myParticleSystem.main.maxParticles];
    }
    void Start()
    {
        publicPlayerTransform = PublicPlayerTransform.Instance;
    }

    private void OnEnable()
    {
        StartCoroutine(LazyStart());
    }

    void Update()
    {
        numParticlesAlive = myParticleSystem.GetParticles(particles);

        for (int i = 0; i < numParticlesAlive; i++)
        {
            float distanceToPlayer = Vector3.Distance(publicPlayerTransform.PlayerTransform.position, particles[i].position);


            if (distanceToPlayer < 0.75f)
            {
                particles[i].remainingLifetime = 0f;

                if (particleSuckedInSoundEmitter != null)
                {
                    particleSuckedInSoundEmitter.Play();
                }
            }

            Vector3 directionToTarget = (publicPlayerTransform.PlayerTransform.position - particles[i].position).normalized;
            particles[i].velocity = directionToTarget * myParticleSystem.main.startSpeed.constant;
        }

        myParticleSystem.SetParticles(particles, numParticlesAlive);


    }


    private IEnumerator LazyStart()
    {
        while (velocityMultiplier < 1)
        {
            velocityMultiplier += Time.deltaTime;
            mainModule.simulationSpeed = velocityMultiplier;
            yield return null;
        }
    }


}
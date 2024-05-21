using System.Collections;
using UnityEngine;

public class ParticleMoveTowards : MonoBehaviour
{
    //[SerializeField] private Transform targetPoint;
    private ParticleSystem myParticleSystem;
    private ParticleSystem.MainModule mainModule;

    private PublicPlayerTransform publicPlayerTransform;

    private ParticleSystem.Particle[] particles;

    private int numParticlesAlive;

    private float velocityMultiplier = 0;

    void Start()
    {
        publicPlayerTransform = PublicPlayerTransform.instance;

       // if (targetPoint == null)
       // {
       //     targetPoint = publicPlayerTransform.PlayerTransform;
       // }

        myParticleSystem = GetComponent<ParticleSystem>();
        mainModule = myParticleSystem.main;
        particles = new ParticleSystem.Particle[myParticleSystem.main.maxParticles];
        StartCoroutine(LazyStart());
    }

    void Update()
    {
        numParticlesAlive = myParticleSystem.GetParticles(particles);

        for (int i = 0; i < numParticlesAlive; i++)
        {
            float distanceToPlayer = Vector3.Distance(publicPlayerTransform.PlayerTransform.position, particles[i].position);

            if (distanceToPlayer < 0.1f)
            {
                particles[i].remainingLifetime = 0f;
            }

            Vector3 directionToTarget = (publicPlayerTransform.PlayerTransform.position - particles[i].position).normalized;
            particles[i].velocity = directionToTarget * myParticleSystem.main.startSpeed.constant;
        }

        myParticleSystem.SetParticles(particles, numParticlesAlive);


    }


    private IEnumerator LazyStart()
    {
        while (velocityMultiplier<1)
        {
            velocityMultiplier += Time.deltaTime;
            mainModule.simulationSpeed = velocityMultiplier;
            yield return null;
        }
    }


}
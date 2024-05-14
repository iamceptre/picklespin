using UnityEngine;

public class ParticleMoveTowards : MonoBehaviour
{
    [SerializeField] private Transform targetPoint;
    private ParticleSystem myParticleSystem;

    private PublicPlayerTransform publicPlayerTransform;

    private ParticleSystem.Particle[] particles;

    private int numParticlesAlive;

    void Start()
    {
        publicPlayerTransform = PublicPlayerTransform.instance;

        if (targetPoint == null)
        {
           targetPoint = publicPlayerTransform.PlayerTransform;
        }

        myParticleSystem = GetComponent<ParticleSystem>();
        particles = new ParticleSystem.Particle[myParticleSystem.main.maxParticles];
    }

    void Update()
    {
        numParticlesAlive = myParticleSystem.GetParticles(particles);

        for (int i = 0; i < numParticlesAlive; i++)
        {
            float distanceToPlayer = Vector3.Distance(targetPoint.position, particles[i].position);

            if (distanceToPlayer < 0.1f)
            {
                particles[i].remainingLifetime = 0f;
            }

            Vector3 directionToTarget = (targetPoint.position - particles[i].position).normalized;
            particles[i].velocity = directionToTarget * myParticleSystem.main.startSpeed.constant;
        }

        myParticleSystem.SetParticles(particles, numParticlesAlive);


    }


}
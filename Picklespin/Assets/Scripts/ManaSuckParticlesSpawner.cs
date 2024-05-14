using UnityEngine;

public class ManaSuckParticlesSpawner : MonoBehaviour
{
    public static ManaSuckParticlesSpawner instance { get; private set; }
    [SerializeField] private ParticleSystem manaSuckParticle;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }
    public void Spawn(Vector3 spawnPoint, int HowMuchManaWasGiven)
    {
        var spawnedParticles = Instantiate(manaSuckParticle, spawnPoint, Quaternion.identity);
        var emission = spawnedParticles.emission;
        emission.rateOverTime = HowMuchManaWasGiven;
        spawnedParticles.Play();
    }

}

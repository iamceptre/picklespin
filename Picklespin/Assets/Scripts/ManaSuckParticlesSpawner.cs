using UnityEngine;

public class ManaSuckParticlesSpawner : MonoBehaviour
{
    public static ManaSuckParticlesSpawner instance { get; private set; }
    [SerializeField] private ParticleSystem manaSuckParticle;

    [SerializeField] private Transform target;

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
        spawnPoint = new Vector3(spawnPoint.x, spawnPoint.y + 1.5f, spawnPoint.z);
        var spawnedParticles = Instantiate(manaSuckParticle, spawnPoint, Quaternion.identity);
        var emission = spawnedParticles.emission;
        var main = spawnedParticles.main;
        emission.rateOverTime = HowMuchManaWasGiven/main.duration;
        spawnedParticles.Play();
    }

}

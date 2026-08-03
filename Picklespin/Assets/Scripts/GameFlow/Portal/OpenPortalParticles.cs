using UnityEngine;

public class OpenPortalParticles : MonoBehaviour
{
    private ParticleSystem myParticleSystem;
    private ParticleSystem.EmissionModule myEmissionModule;
    private ParticleSystem.MainModule myMainModule;
    private ParticleSystem.ShapeModule myShapeModule;


    [Header("Open Portal Particle Settings")]

    [SerializeField] private float openedRate;
    [SerializeField] private float openedSpeed;
    [SerializeField] private float openedSize;
    [SerializeField] private Color openedColor;
    [SerializeField] private Vector3 openedShapeSize;

    private void Awake()
    {
        myParticleSystem = gameObject.GetComponent<ParticleSystem>();
        myEmissionModule = myParticleSystem.emission;
        myMainModule = myParticleSystem.main;
        myShapeModule = myParticleSystem.shape;
    }

    public void SetOpenPortalParticles()
    {
        myEmissionModule.rateOverTime = openedRate;
        myMainModule.startSpeed = openedSpeed;
        myMainModule.startSize = openedSize;
        myMainModule.startColor = openedColor;
        myShapeModule.scale = openedShapeSize;
    }

}

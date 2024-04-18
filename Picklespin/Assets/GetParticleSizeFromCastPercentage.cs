using UnityEngine;

public class GetParticleSizeFromCastPercentage : MonoBehaviour
{

    private ParticleSystem particleSystem;
    private Attack attack;
    private void Awake()
    {
        particleSystem = GetComponent<ParticleSystem>();
    }

    private void Start()
    {
        attack = Attack.instance;
    }

    void Update()
    {
        particleSystem.startSize = attack.castingPercentage * 5;
    }
}

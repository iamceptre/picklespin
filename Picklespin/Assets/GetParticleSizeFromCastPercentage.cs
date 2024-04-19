using UnityEngine;

public class GetParticleSizeFromCastPercentage : MonoBehaviour
{

    private ParticleSystem ps;
    private Attack attack;
    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    private void Start()
    {
        attack = Attack.instance;
    }

    void Update()
    {
        var main = ps.main;
        main.startSizeMultiplier = attack.castingPercentage * 2;
    }
}

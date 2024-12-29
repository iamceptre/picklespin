using System.Collections;
using UnityEngine;

public class GetParticleSizeFromCastPercentage : MonoBehaviour
{
    private ParticleSystem ps;
    private ParticleSystem.MainModule mainModule;
    private Attack attack;

    public Light castingLight;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    private void Start()
    {
        attack = Attack.instance;
        mainModule = ps.main;
    }


    public IEnumerator StartDoingShit()
    {
        if (castingLight != null)
        {
            castingLight.enabled = true;
        }

        while (attack.castingProgress < 1)
        {
            mainModule.startSizeMultiplier = attack.castingProgress * 2;
            yield return null;
        }

        yield break;
    }
}

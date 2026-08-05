using UnityEngine;
using System;
using DG.Tweening;

public class SpellDecalDissolve : MonoBehaviour
{
    [SerializeField] private Renderer decalRenderer;
    [SerializeField] private ParticleSystem[] particleSystems;
    [SerializeField] private Light decalLight;

    [SerializeField] private GameObject particleSpecialEffect;

    [Header("Dissolve Time Multipliers")]
    [SerializeField] private float woodFadeMultiplier = 4.0f;
    [SerializeField] private float carpetFadeMultiplier = 2.0f;
    [SerializeField] private float concreteFadeMultiplier = 1.0f;

    // Precomputed hashes for tags
    private static readonly int woodTagHash = "Wood".GetHashCode();
    private static readonly int carpetTagHash = "Carpet".GetHashCode();
    private static readonly int concreteTagHash = "Concrete".GetHashCode();

    private float fadeDelay;
    private float fadeDuration;
    private Action<SpellDecalDissolve> onFadeComplete;
    private Material decalMaterial;

    private void Awake()
    {
        if (decalRenderer != null) decalMaterial = decalRenderer.material;
    }

    public void Initialize(Action<SpellDecalDissolve> fadeCompleteCallback, int hitTagHash)
    {
        onFadeComplete = fadeCompleteCallback;

       //Debug.Log(hitTagHash);

        if (hitTagHash == woodTagHash)
        {
            fadeDelay = woodFadeMultiplier;
            fadeDuration = woodFadeMultiplier;
        }
        else if (hitTagHash == carpetTagHash)
        {
            fadeDelay = carpetFadeMultiplier;
            fadeDuration = carpetFadeMultiplier;
        }
        else if (hitTagHash == concreteTagHash)
        {
            fadeDelay = concreteFadeMultiplier;
            fadeDuration = concreteFadeMultiplier;
        }
        else
        {
            fadeDelay = 1;
            fadeDuration = 1;
        }

        if (particleSpecialEffect) particleSpecialEffect.SetActive(true);

        ResetDecal();
        AdjustParticleSystemLifetimes();
        Invoke(nameof(StartFade), fadeDelay);
    }

    private void AdjustParticleSystemLifetimes()
    {
        for (int i = 0; i < particleSystems.Length; i++)
        {
            var ps = particleSystems[i];
            var mainModule = ps.main;
            mainModule.duration = fadeDelay;
            mainModule.startLifetime = fadeDuration;
            ps.Play();
        }
    }

    private void ResetDecal()
    {
        if (decalMaterial != null)
        {
            Color initialColor = decalMaterial.color;
            initialColor.a = 1f;
            decalMaterial.color = initialColor;
        }

        for (int i = 0; i < particleSystems.Length; i++)
        {
            var ps = particleSystems[i];
            ps.Stop();
            ps.Clear();
        }

        if (decalLight != null)
        {
            decalLight.enabled = true;
            decalLight.intensity = 1f;
        }
    }

    private void StartFade()
    {
        if (decalMaterial != null)
        {
            Color targetColor = decalMaterial.color;
            targetColor.a = 0f;
            decalMaterial.DOColor(targetColor, fadeDuration).OnComplete(() =>
            {
                if (particleSpecialEffect) particleSpecialEffect.SetActive(false);
                onFadeComplete?.Invoke(this);
            });
        }
        else
        {
            if (particleSpecialEffect) particleSpecialEffect.SetActive(false);
            onFadeComplete?.Invoke(this);
        }
    }
}

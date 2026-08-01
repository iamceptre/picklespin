using UnityEngine;

[DisallowMultipleComponent]
public class ConvertedAllyGlow : MonoBehaviour
{
    private static readonly Color BodyEmission = new(0.2f, 0.14f, 0.05f);
    private static readonly Color HaloColor = new(1f, 0.87f, 0.55f);

    private const float HaloHeight = 1.3f;
    private const float HaloRange = 5f;
    private const float HaloMinIntensity = 0.3f;
    private const float HaloMaxIntensity = 1f;
    private const float PulseSpeed = 1.6f;

    private static readonly Color EyeLightColor = new(1f, 0.776f, 0.153f);

    private MaterialFlashWhenHit bodyFlash;
    private Renderer eyeRenderer;
    private Material alliedEyeMaterial;
    private Material hostileEyeMaterial;
    private Light eyeLight;
    private Color hostileEyeLightColor;
    private Light halo;
    private float pulseTime;
    private bool resolved;

    public void Show(AiReferences owner)
    {
        Resolve(owner);

        if (bodyFlash) bodyFlash.SetBaseEmission(BodyEmission);
        SetEye(true);
        if (halo)
        {
            halo.intensity = HaloMinIntensity;
            halo.enabled = true;
        }

        pulseTime = Random.Range(0f, Mathf.PI * 2f);
        enabled = true;
    }

    public void Hide()
    {
        if (bodyFlash) bodyFlash.SetBaseEmission(Color.black);
        SetEye(false);
        if (halo) halo.enabled = false;
        enabled = false;
    }

    private void SetEye(bool allied)
    {
        if (eyeRenderer && alliedEyeMaterial)
        {
            eyeRenderer.sharedMaterial = allied ? alliedEyeMaterial : hostileEyeMaterial;
        }
        if (eyeLight) eyeLight.color = allied ? EyeLightColor : hostileEyeLightColor;
    }

    private void Update()
    {
        if (!halo) return;

        pulseTime += Time.deltaTime * PulseSpeed;
        float waveA = Mathf.Sin(pulseTime);
        float waveB = Mathf.Sin(pulseTime * PhiMath.PHI);
        float wave = 0.5f + 0.25f * (waveA + waveB);
        halo.intensity = Mathf.Lerp(HaloMinIntensity, HaloMaxIntensity, wave);
    }

    private void Resolve(AiReferences owner)
    {
        if (resolved) return;
        resolved = true;

        bodyFlash = owner.MaterialFlash;

        eyeRenderer = owner.EyeRenderer;
        alliedEyeMaterial = owner.AlliedEyeMaterial;
        if (eyeRenderer) hostileEyeMaterial = eyeRenderer.sharedMaterial;

        eyeLight = owner.EyeLight;
        if (eyeLight) hostileEyeLightColor = eyeLight.color;

        GameObject haloObject = new("AllyHalo");
        haloObject.transform.SetParent(transform, false);
        haloObject.transform.localPosition = new Vector3(0f, HaloHeight, 0f);

        halo = haloObject.AddComponent<Light>();
        halo.type = LightType.Point;
        halo.color = HaloColor;
        halo.range = HaloRange;
        halo.shadows = LightShadows.None;
        halo.enabled = false;
    }
}

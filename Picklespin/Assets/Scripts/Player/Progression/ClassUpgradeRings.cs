using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ClassUpgradeRings : MonoBehaviour
{
    [Header("Rings")]
    [SerializeField, Tooltip("one ring per class-upgrade level, in the order they are earned - place each one where it should sit on its finger, that pose is its home")]
    private GameObject[] rings = new GameObject[ClassUpgrades.MaxLevel];

    [Header("Arrival")]
    [SerializeField, Tooltip("where the ring starts, offset from its home in the ring's own parent space - up the finger, so it slides down onto it")]
    private Vector3 slideFrom = new(0f, 0.015f, 0f);
    [SerializeField] private float slideDuration = 0.7f;
    [SerializeField] private float fadeDuration = 0.7f;
    [SerializeField] private Ease slideEase = Ease.OutCubic;

    private static readonly int ColorProperty = Shader.PropertyToID("_Color");

    private Transform[] ringTransforms;
    private Vector3[] ringHomes;
    private Material[][] ringMaterials;
    private float[][] ringAlphas;

    private void Awake()
    {
        int count = rings.Length;
        ringTransforms = new Transform[count];
        ringHomes = new Vector3[count];
        ringMaterials = new Material[count][];
        ringAlphas = new float[count][];

        for (int i = 0; i < count; i++)
        {
            if (!rings[i]) continue;

            ringTransforms[i] = rings[i].transform;
            ringHomes[i] = ringTransforms[i].localPosition;
            CacheMaterials(i);
            rings[i].SetActive(false);
        }
    }

    private void CacheMaterials(int ring)
    {
        List<Material> found = new();
        foreach (Renderer renderer in rings[ring].GetComponentsInChildren<Renderer>(true))
        {
            found.AddRange(renderer.materials);
        }

        ringMaterials[ring] = found.ToArray();
        ringAlphas[ring] = new float[found.Count];
        for (int i = 0; i < found.Count; i++)
        {
            ringAlphas[ring][i] = found[i].HasProperty(ColorProperty) ? found[i].GetColor(ColorProperty).a : 1f;
        }
    }

    private void OnEnable() => ClassUpgrades.LevelChanged += Apply;

    private void OnDisable() => ClassUpgrades.LevelChanged -= Apply;

    private void Start() => Apply();

    private void Apply()
    {
        int level = ClassUpgrades.Level;

        for (int i = 0; i < rings.Length; i++)
        {
            bool wanted = i < level;
            if (!rings[i] || wanted == rings[i].activeSelf) continue;

            if (wanted) Wear(i);
            else TakeOff(i);
        }
    }

    private void Wear(int ring)
    {
        Transform ringTransform = ringTransforms[ring];
        ringTransform.DOKill();
        ringTransform.localPosition = ringHomes[ring] + slideFrom;
        rings[ring].SetActive(true);
        ringTransform.DOLocalMove(ringHomes[ring], slideDuration).SetEase(slideEase);

        Material[] materials = ringMaterials[ring];
        for (int i = 0; i < materials.Length; i++)
        {
            if (!materials[i] || !materials[i].HasProperty(ColorProperty)) continue;

            materials[i].DOKill();
            Color color = materials[i].GetColor(ColorProperty);
            color.a = 0f;
            materials[i].SetColor(ColorProperty, color);
            materials[i].DOFade(ringAlphas[ring][i], fadeDuration);
        }
    }

    private void TakeOff(int ring)
    {
        ringTransforms[ring].DOKill();

        Material[] materials = ringMaterials[ring];
        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i]) materials[i].DOKill();
        }

        rings[ring].SetActive(false);
    }
}

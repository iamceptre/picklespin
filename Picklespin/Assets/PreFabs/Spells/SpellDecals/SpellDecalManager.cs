using UnityEngine;
using System.Collections.Generic;
public class SpellDecalManager : MonoBehaviour
{
    public static SpellDecalManager Instance { get; private set; }

    [Header("Pool Setup")]
    [SerializeField] private SpellDecalDissolve decalPrefab;
    [SerializeField] private int poolSize = 100;

    [Header("Fade Settings")]
    [Tooltip("Time (sec) to wait before fade starts")]
    [SerializeField] private float fadeDelay = 1f;
    [Tooltip("Time (sec) over which alpha fades to 0")]
    [SerializeField] private float fadeDuration = 1f;

    [Header("Optional Shared Material")]
    [Tooltip("Use one material to reduce draw calls if possible.")]
    [SerializeField] private Material sharedDecalMaterial;

    private readonly Queue<SpellDecalDissolve> _decalPool = new Queue<SpellDecalDissolve>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            SpellDecalDissolve decal = CreateNewDecal();
            _decalPool.Enqueue(decal);
        }
    }
    private SpellDecalDissolve CreateNewDecal()
    {
        var newDecal = Instantiate(decalPrefab, transform);
        // Optionally assign a shared material to reduce draw calls
        if (sharedDecalMaterial && newDecal.TryGetComponent(out SpriteRenderer sr))
        {
            sr.sharedMaterial = sharedDecalMaterial;
        }

        newDecal.gameObject.SetActive(false);
        return newDecal;
    }
    public SpellDecalDissolve SpawnDecal(Vector3 position, Quaternion rotation)
    {
        SpellDecalDissolve decal;
        if (_decalPool.Count > 0)
        {
            decal = _decalPool.Dequeue();
        }
        else
        {
            decal = CreateNewDecal();
        }

        decal.transform.SetPositionAndRotation(position, rotation);
        decal.gameObject.SetActive(true);
        decal.Initialize(fadeDelay, fadeDuration, ReturnDecal);

        return decal;
    }

    private void ReturnDecal(SpellDecalDissolve decal)
    {
        decal.gameObject.SetActive(false);
        _decalPool.Enqueue(decal);
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Dissolver : MonoBehaviour
{
    [SerializeField] private UnityEvent afterDissolveEvent;

    [SerializeField] private Material deadMaterial;
    private float dissolveProgress; //0 is visible, 1 is not
    [SerializeField] private Renderer myRenderer;

    [SerializeField][Range(0.01f, 3)] private float dissolveSpeed = 0.7f;

    [SerializeField] private bool destroyAfterDissolve = false;

    [Tooltip("the look of the ash left behind - the shared field is filled with copies of it, this one never shows")]
    [SerializeField] private GameObject ashPile;

    private static readonly int progress = Shader.PropertyToID("_DissolveAmount");

    private Material dissolveMaterialInstance;
    private Material aliveMaterial;
    private int ashHandle = -1;

    private void Awake()
    {
        if (!ashPile) return;

        AshPileField.Prewarm(ashPile);
        if (ashPile.TryGetComponent(out Renderer templateRenderer)) templateRenderer.enabled = false;
    }

    private void OnDisable()
    {
        if (ashHandle < 0) return;

        AshPileField.SetDissolve(ashHandle, 0);
        ashHandle = -1;
    }

    public void StartDissolve()
    {
        aliveMaterial = myRenderer.material; // captured here so pooled reuse can restore it
        myRenderer.material = deadMaterial;
        dissolveMaterialInstance = myRenderer.material;
        dissolveProgress = 0;
        StartCoroutine(Animate());

        if (ashPile != null)
        {
            SpawnAshBeneath();
        }
    }

    // restores the pre-death state so a pooled enemy can be respawned
    public void ResetDissolveState()
    {
        StopAllCoroutines();
        dissolveProgress = 0;
        ashHandle = -1;
        if (aliveMaterial != null)
        {
            myRenderer.material = aliveMaterial;
        }
    }



    private IEnumerator Animate()
    {
        while (dissolveProgress <= 1)
        {
            dissolveProgress += Time.deltaTime * dissolveSpeed;
            dissolveMaterialInstance.SetFloat(progress, dissolveProgress);
            yield return null;
        }
        WhatToDoAfterDissolve();
        yield break;
    }

    private void WhatToDoAfterDissolve()
    {
        if (destroyAfterDissolve)
        {
            if (!EnemiesSpawner.TryDespawn(gameObject)) Destroy(gameObject);
        }
        else
        {
            afterDissolveEvent.Invoke();
        }
    }

    private void SpawnAshBeneath()
    {
        //+ Vector3.up to make the raycast shoot from above the ground
        if (!Physics.Raycast(transform.position + Vector3.up, -Vector3.up, out RaycastHit hit, Mathf.Infinity)) return;

        Vector3 positionOffset = new(0, Random.Range(0.28f, 0.35f));
        ashHandle = AshPileField.Place(
            hit.point + positionOffset,
            Quaternion.Euler(0, Random.Range(0f, 360f), 0),
            Random.Range(0.8f, 1.2f));

        if (ashHandle >= 0) StartCoroutine(UndissolveAsh());
    }

    private IEnumerator UndissolveAsh()
    {
        float ashDissolveProgress = AshPileField.HiddenAmount;
        while (ashDissolveProgress > 0)
        {
            ashDissolveProgress = AshPileField.HiddenAmount - dissolveProgress; //mirrors the enemies dissolve
            AshPileField.SetDissolve(ashHandle, ashDissolveProgress);
            yield return null;
        }
        ashHandle = -1;
        yield break;
    }

}

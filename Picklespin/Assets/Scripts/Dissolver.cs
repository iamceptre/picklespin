using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Dissolver : MonoBehaviour
{
    [SerializeField] private UnityEvent afterDissolveEvent;

    [SerializeField] private Material deadMaterial;
    private float dissolveProgress; //0 is visible, 1 is not
    private float ashDissolveProgress;
    [SerializeField] private Renderer myRenderer;

    [SerializeField][Range(0.01f, 3)] private float dissolveSpeed = 0.7f;

    [SerializeField] private bool destroyAfterDissolve = false;

    [SerializeField] private GameObject ashPile;
    [SerializeField] private Renderer ashPileRenderer;
    [SerializeField] private Transform ashPileTransform; //make it GetComponent after moving to pooling


    private int progress = Shader.PropertyToID("_DissolveAmount");


    public void StartDissolve()
    {
        myRenderer.material = deadMaterial;
        dissolveProgress = 0;
        StartCoroutine(Animate());

        if (ashPile != null)
        {
            SpawnAshBeneath();
        }
    }



    private IEnumerator Animate()
    {
        while (dissolveProgress <= 1)
        {
            dissolveProgress += Time.deltaTime * dissolveSpeed;
            myRenderer.material.SetFloat(progress, dissolveProgress);
            yield return null;
        }
        WhatToDoAfterDissolve();
        yield break;
    }

    private void WhatToDoAfterDissolve()
    {

        if (destroyAfterDissolve)
        {
            Destroy(gameObject);
        }
        else
        {
            afterDissolveEvent.Invoke();
        }
    }

    private void SpawnAshBeneath()
    {

        Vector3 positionOffset = new Vector3(0, Random.Range(0.28f, 0.35f));

        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, -Vector3.up, out hit, Mathf.Infinity)) //+ Vector3.up to make the raycast shoot from above the ground
        {
            float randomY = Random.Range(0, 360);
            ashPileRenderer.enabled = true;
            ashPileTransform.position = hit.point + positionOffset;
            ashPileTransform.rotation = new Quaternion(0, randomY, 0, 0);
            float randomScale = Random.Range(0.8f, 1.2f);
            ashPileTransform.localScale = new Vector3(randomScale, randomScale, randomScale);
            ashDissolveProgress = 0;
            ashPile.transform.SetParent(null, true);
            StartCoroutine(UndissolveAsh());
        }
    }

    private IEnumerator UndissolveAsh()
    {
        ashDissolveProgress = 0.9f;
        while (ashDissolveProgress > 0)
        {
            ashDissolveProgress = 0.9f - dissolveProgress; //mirrors the enemies dissolve
            ashPileRenderer.material.SetFloat(progress, ashDissolveProgress);
            yield return null;
        }
        yield break;
    }

}

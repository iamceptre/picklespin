using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class Dissolver : MonoBehaviour
{

    [SerializeField] private UnityEvent afterDissolveEvent;

    [SerializeField] private Material deadMaterial;
    [SerializeField] private Material ashDissolveMaterial;
    private float dissolveProgress; //1 is visible, 0 is not
    private float ashDissolveProgress;
    [SerializeField] private Renderer myRenderer;
    private Renderer ashRenderer;

    [SerializeField][Range(0.01f, 3)] private float dissolveSpeed = 0.7f;

    [SerializeField] private bool destroyAfterDissolve = false;

    [SerializeField] private GameObject ashPile;

    private bool dudeDissolved = false;

    [SerializeField] private NavMeshAgent myNavMeshAgent;
    [SerializeField] private StateManager myStateManager;


    public void StartDissolve()
    {
        if (myStateManager != null)
        {
            Destroy(myStateManager);
        }

        if (myNavMeshAgent != null)
        {
            myNavMeshAgent.enabled = false;
        }

        myRenderer.material = deadMaterial;
        dissolveProgress = 0.7f;
        StartCoroutine(Animate());
        SpawnAshBeneath();
    }



    private IEnumerator Animate()
    {
        while (dissolveProgress >= 0)
        {
            dissolveProgress -= Time.deltaTime * dissolveSpeed;
            myRenderer.material.SetFloat("_Progress", dissolveProgress);

            if (dissolveProgress <= 0 && !dudeDissolved)
            {
                dudeDissolved = true;
                WhatToDoAfterDissolve();
            }

            yield return null;
        }
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
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, Mathf.Infinity))
        {
            float randomY = Random.Range(0, 360);
            var spawnedAsh = Instantiate(ashPile, hit.point + positionOffset, new Quaternion(0, randomY, 0, 0));
            float randomScale = Random.Range(0.8f, 1.2f);
            spawnedAsh.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
            ashRenderer = spawnedAsh.GetComponent<Renderer>();
            ashDissolveProgress = 0;
            ashRenderer.material = ashDissolveMaterial;
            StartCoroutine(UndissolveAsh());
        }
    }

    private IEnumerator UndissolveAsh()
    {
        while (ashDissolveProgress < 1)
        {
            ashDissolveProgress = 0.7f - dissolveProgress;
            ashRenderer.material.SetFloat("_Progress", ashDissolveProgress);
            yield return null;
        }
    }

}

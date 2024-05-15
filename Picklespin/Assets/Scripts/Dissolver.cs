using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Dissolver : MonoBehaviour
{

    [SerializeField] private UnityEvent afterDissolveEvent;

    [SerializeField] private Material deadMaterial;
    private float dissolveProgress; //1 is visible, 0 is not
    [SerializeField] private Renderer myRenderer;

    [SerializeField] [Range(0.01f,3)] private float dissolveSpped = 0.7f;

    [SerializeField] private bool destroyAfterDissolve = false;


    public void StartDissolve()
    {
        myRenderer.material = deadMaterial;
        dissolveProgress = 0.7f;
        StartCoroutine(Animate());
    }



    private IEnumerator Animate()
    {
        while (true)
        {
            dissolveProgress -= Time.deltaTime * dissolveSpped;
            myRenderer.material.SetFloat("_Progress", dissolveProgress);

            if (dissolveProgress <= 0)
            {
                StopAllCoroutines();
                enabled = false;
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

}

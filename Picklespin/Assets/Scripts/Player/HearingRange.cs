using System.Collections;
using UnityEngine;

public class HearingRange : MonoBehaviour
{

    private SphereCollider hearingRange;
    [SerializeField] private PlayerMovement playerMovement;
    private float myRange;
 
    void Start()
    {
        hearingRange = GetComponent<SphereCollider>();
        myRange = hearingRange.radius;
        StopAllCoroutines();
        StartCoroutine(LazyUpdate());
    }

    private IEnumerator LazyUpdate()
    {
        while (true)
        {
            Debug.Log("lazy update");
            if (hearingRange.radius != myRange * playerMovement.movementStateForFMOD)
            {
                hearingRange.radius = myRange * playerMovement.movementStateForFMOD;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator ExtendedHearingRange()
    {
        Debug.Log("jump");
        hearingRange.radius = 20;
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(LazyUpdate());
    }

    public void RunExtendedHearingRange()
    {
        StopAllCoroutines();
        StartCoroutine(ExtendedHearingRange());
    }
}

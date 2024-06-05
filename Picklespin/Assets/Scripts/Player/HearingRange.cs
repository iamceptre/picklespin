using System.Collections;
using UnityEngine;

public class HearingRange : MonoBehaviour
{

    private SphereCollider hearingRange;
    [SerializeField] private PlayerMovement playerMovement;
    private float myRange;

    private WaitForSeconds refreshTime = new WaitForSeconds(0.5f);
    private WaitForSeconds extendedRangeTime = new WaitForSeconds(0.5f);

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
            if (hearingRange.radius != myRange * playerMovement.movementStateForFMOD)
            {
                hearingRange.radius = myRange * playerMovement.movementStateForFMOD;
            }
            yield return refreshTime;
        }
    }

    private IEnumerator ExtendedHearingRange()
    {
        hearingRange.radius = 20;
        yield return extendedRangeTime;
        StartCoroutine(LazyUpdate());
    }

    public void RunExtendedHearingRange()
    {
        StopAllCoroutines();
        StartCoroutine(ExtendedHearingRange());
    }
}

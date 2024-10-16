using Pathfinding;
using System.Collections;
using UnityEngine;

public class StopAiForAsec : MonoBehaviour
{
    private WaitForSeconds waitTime = new WaitForSeconds(1.4f);

    [SerializeField] private AIPath aiPath;

    public void StopMeForASec()
    {
        StopAllCoroutines();
        StartCoroutine(Routine());
    }

    private IEnumerator Routine()
    {
        //Debug.Log("succesfully dashed enemy, stuning");
        aiPath.isStopped = true;
        yield return waitTime;
        aiPath.isStopped = false;
    }
}

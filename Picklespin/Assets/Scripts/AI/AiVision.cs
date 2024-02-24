using System;
using UnityEngine;

public class AiVision : MonoBehaviour
{
    public float radius;
    [Range(0, 360)]
    public float angle; //make angle go 360 when player is closer than 5f

    [HideInInspector] public GameObject playerRef;

    private LayerMask targetMask;
    private LayerMask obstructionMask;

    public bool seeingPlayer;

    private void Start()
    {
        playerRef = GameObject.FindGameObjectWithTag("MainCamera");
        targetMask |= 0x1 << LayerMask.NameToLayer("Player");
        obstructionMask |= 0x1 << LayerMask.NameToLayer("Default");
    }


    public void FieldOfViewCheck()
    {

        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radius, targetMask);

        if (rangeChecks.Length != 0)
        {
            Transform target = rangeChecks[0].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, directionToTarget) < angle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
                    seeingPlayer = true;
                else
                    seeingPlayer = false;
            }
            else
                seeingPlayer = false;
        }
        else if (seeingPlayer)
            seeingPlayer = false;
    }
}
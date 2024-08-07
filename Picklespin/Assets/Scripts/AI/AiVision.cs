using System;
using UnityEngine;

public class AiVision : MonoBehaviour
{
    public float radius;
    [Range(0, 360)]
    public float angle;

    [HideInInspector] public Transform playerRef;

    private LayerMask targetMask;
    private LayerMask obstructionMask;

    public bool seeingPlayer;
    public bool playerJustHitMe = false;
   [SerializeField] private bool playerIsVeryClose = false;
    public float hitMeCooldown = 0;

    private void Start()
    {
        playerRef = CachedCameraMain.instance.cachedTransform;
        targetMask |= 0x1 << LayerMask.NameToLayer("Player");
        obstructionMask |= 0x1 << LayerMask.NameToLayer("Default");
    }

    private void Update()
    {
        if (hitMeCooldown >= 0)
        {
            PlayerJustHitMeCooldown();
        }
    }


    public void FieldOfViewCheck()
    {
        
        

        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radius, targetMask);


        if (!playerJustHitMe && !playerIsVeryClose) //this shit makes the ai ignore everything and see you
        {

            if (rangeChecks.Length != 0)
            {
                Transform target = rangeChecks[0].transform;
                Vector3 directionToTarget = (target.position - transform.position).normalized;

                isPlayerVeryCloseCheck();

                if (Vector3.Angle(transform.forward, directionToTarget) < angle / 2)
                {
                    float distanceToTarget = Vector3.Distance(transform.position, playerRef.position);

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
        else
        {
            seeingPlayer = true;
        }
        

        seeingPlayer = true;

    }

    public void PlayerJustHitMeCooldown()
    {
        hitMeCooldown -= Time.deltaTime;

        if (hitMeCooldown<=0)
        {
            playerJustHitMe = false;
        }
        
        seeingPlayer = true;
    }


   private void isPlayerVeryCloseCheck()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerRef.position); //change it to Player Sound Radius Trigger with AI Hear Trigger

        if (distanceToPlayer <= (radius * 0.5))
        { 
            playerIsVeryClose = true;
        }
        else
        {
            playerIsVeryClose = false;
        }
    }

}
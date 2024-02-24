using System.Collections;
using UnityEngine;

public class EvilEntityMind : MonoBehaviour
{

    [Range(0, 100)] public float hp = 1;
    private float speed = 5f;
    private float step;
  
    private bool dashing = false;
    private bool dashed = false;
    private bool waitingInPlace = false;
    private bool isIdle = true;
    public bool isDead = false;
    private bool seeingPlayer;

    public Transform[] waypoints;
    private int waypointIndex;


    private Vector3 directionToPlayer;
    private Vector3 currentWaypoint; //Add NavMesh
    private float distanceToPlayer;

    private GameObject player;

    private float seeRadius = 50f;
    [Range(0, 360)] private float seeAngle = 100;

    private LayerMask playerLayer;
    private LayerMask obstacle;

    [Range(0, 3)] private float remainingCooldown;


    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("MainCamera");
        playerLayer |= 0x1 << LayerMask.NameToLayer("Player");
        obstacle |= 0x1 << LayerMask.NameToLayer("Default");

        InvokeRepeating("UpdateVision", 0, 0.2f);
        InvokeRepeating("UpdateWaypoint", 0, 1f);

        waypointIndex = 0;
        currentWaypoint = waypoints[waypointIndex].position;
    }

    private void Update()
    {

        if (!waitingInPlace)
        {
            step = speed * Time.deltaTime;

            if (seeingPlayer)
            {
                if (distanceToPlayer <= 15 || dashed)
                {
                    RunToPlayer();
                } else
                {
                    StartCoroutine(ChargeAndDash(0));
                }
            }
            else
            {
                WaypointNavigation();
            }

        }


        if (remainingCooldown >= 0 && !seeingPlayer) {
            remainingCooldown -= Time.deltaTime;
            waitingInPlace = true;
        }
        else
        {
            waitingInPlace = false;
        }

    }

    void RunToPlayer()
    {
        transform.position = Vector3.MoveTowards(transform.position, player.transform.position, step); //Hunt player
        transform.LookAt(player.transform);
    }

    void WaypointNavigation()
    {
        transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, step);
        transform.LookAt(currentWaypoint);
    }

    IEnumerator ChargeAndDash(float originalSpeed)
    {
        //Charging
        waitingInPlace = true; 
        originalSpeed = speed;
        transform.LookAt(player.transform);
        yield return new WaitForSeconds(2); //Charge Duration
        waitingInPlace = false;

        //Dashing
        speed = originalSpeed * 4;
        Dash();
        yield return new WaitForSeconds(1); //Dash duration
        dashing = false;
        dashed = true;
        speed = originalSpeed;
    }

    void Dash()
    {
        if (dashing)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.transform.position, step);
        }
    }


    void UpdateVision()
    {

        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, seeRadius, playerLayer);

        if (rangeChecks.Length !=0)
        {

            distanceToPlayer = Vector3.Distance(transform.position, player.transform.position); //use it later to determine whenever enemies charge their dash or simply hunt down the player running, make the enemies angle 360 when you are very close
            directionToPlayer = (player.transform.position - transform.position).normalized;

         // if(Vector3.Angle(transform.forward, directionToPlayer) < seeAngle * 0.5f )
         if (true)
          {

                if (!Physics.Raycast(transform.position, directionToPlayer, seeRadius, obstacle))
                {
                    seeingPlayer = true;
                    Debug.DrawRay(transform.position, directionToPlayer, Color.red, seeRadius);
                }
                else
                {
                    seeingPlayer = false;
                }
            }
            else
            {
                seeingPlayer = false;
            }
        }
        else if (seeingPlayer)
        {
            seeingPlayer = false;
        }



        //Add hearing system
        //if they hear you but not see you, then they are guessing where the player is, setting their waypoints to a rough direction of last heared sound from the player

        if (seeingPlayer)
        {
            isIdle = false;
        }
        else
        {
            StartCoroutine(WaitAndReturn());
            isIdle = true;
        }

    }



    void UpdateWaypoint()
    {

        var distanceToWaypoint = Vector3.Distance(transform.position, currentWaypoint);

        if (distanceToWaypoint <= 0.8f) {
            if (waypointIndex < waypoints.Length-1)
            {
                waypointIndex++;
            }
            else
            {
                waypointIndex = 0;
            }

            currentWaypoint = waypoints[waypointIndex].position;

        }
    }




    IEnumerator WaitAndReturn() {

        if (!isIdle)
        {
            remainingCooldown = 3;
        }

        yield return null;
    }


    IEnumerator ComeBackToClosestWaypoint()
    {
       // currentWaypoint = ClosestWaypointPosition
        yield return null;
    }




}

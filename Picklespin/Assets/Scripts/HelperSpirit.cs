using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class HelperSpirit : MonoBehaviour
{
    public static HelperSpirit instance;

    private NavMeshAgent _agent;
    private Vector3 _targetAngel;
    private PublicPlayerTransform playerTransform;
    private WaitForSeconds refreshEverySeconds = new WaitForSeconds(0.25f);

    private Vector3 currentTarget;

    private bool currentlyGoingToAngel = true;

    private float distanceTreshold;

    private float angelDistanceTreshold = 12;
    private float playerDistanceTreshold = 2;

    private TrailRenderer _trail;

    private float startingSpeed;


    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _trail = GetComponent<TrailRenderer>();


        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    void Start()
    {
        distanceTreshold = angelDistanceTreshold;
        playerTransform = PublicPlayerTransform.instance;
        startingSpeed = _agent.speed;
    }

    private IEnumerator Checker()
    {
        while (enabled)
        {
            if (Vector3.Distance(transform.position, currentTarget) < distanceTreshold)
            {
                CycleWaypoint();
            }

            yield return refreshEverySeconds;
        }
    }

    private void CycleWaypoint()
    {
        if (currentlyGoingToAngel)
        {
            currentlyGoingToAngel = false;
            currentTarget = playerTransform.PlayerTransform.position;
            _agent.SetDestination(currentTarget);
            _agent.transform.position = currentTarget;
            //_agent.speed = 999;
            distanceTreshold = playerDistanceTreshold;
            _trail.Clear();
        }
        else
        {
            //_agent.speed = startingSpeed;
            currentlyGoingToAngel = true;
            currentTarget = _targetAngel;
            _agent.SetDestination(currentTarget);
            distanceTreshold = angelDistanceTreshold;
        }

    }

    public void HideSpirit()
    {
        _agent.isStopped = true;
        gameObject.SetActive(false);
        StopAllCoroutines();
    }

    public void ShowSpirit(Transform _target)
    {
        gameObject.SetActive(true);
        _agent.isStopped = false;
        currentTarget = _target.position;
        _targetAngel = currentTarget;
        _agent.SetDestination(_targetAngel);
        StartCoroutine(Checker());
    }

}

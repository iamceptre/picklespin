using Pathfinding;
using System.Collections;
using UnityEngine;

public class HelperSpirit : MonoBehaviour
{
    public static HelperSpirit instance;

    private AIDestinationSetter aIDestinationSetter;
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
        aIDestinationSetter = GetComponent<AIDestinationSetter>();
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
            distanceTreshold = playerDistanceTreshold;
            _trail.Clear();
        }
        else
        {
            currentlyGoingToAngel = true;
            currentTarget = _targetAngel;
            distanceTreshold = angelDistanceTreshold;
        }

    }

    public void HideSpirit()
    {
        gameObject.SetActive(false);
        StopAllCoroutines();
    }

    public void ShowSpirit(Transform _target)
    {
        gameObject.SetActive(true);
        currentTarget = _target.position;
        _targetAngel = currentTarget;
        StartCoroutine(Checker());
    }

}

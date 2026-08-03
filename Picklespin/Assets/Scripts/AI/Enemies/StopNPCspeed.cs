using Pathfinding;
using UnityEngine;

public class StopNPCspeed : MonoBehaviour
{

    [SerializeField] private AIPath aiPath;

    public void Stop()
    {
        if (aiPath != null)
        {
            aiPath.maxSpeed = 0;
            aiPath.isStopped = true;
            aiPath.enabled = false;
        }
        else
        {
            DevLog.Warn("ai path reference is not set, skipping on stopping it");
        }
    }
}

using FMODUnity;
using UnityEngine;

public class FloorTypeDetector : MonoBehaviour
{
    private FootstepSystem footstepSystem;

    [SerializeField] private StudioEventEmitter concreteEvent;
    [SerializeField] private StudioEventEmitter carpetEvent;
    [SerializeField] private StudioEventEmitter woodEvent;

    private float raycastLength = 3f;
    private string hitTagCached;
    private int hitTagCachedHash;

    private static readonly int ConcreteTagHash = "Concrete".GetHashCode();
    private static readonly int CarpetTagHash = "Carpet".GetHashCode();
    private static readonly int WoodTagHash = "Wood".GetHashCode();

    void Start()
    {
        footstepSystem = FootstepSystem.instance;
    }

    public void Check()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, raycastLength))
        {
            int hitTagHash = hit.collider.tag.GetHashCode();

            if (hitTagHash != hitTagCachedHash)
            {
                ChangeFootstepSound(hitTagHash);
                hitTagCachedHash = hitTagHash;
            }
        }
    }

    private void ChangeFootstepSound(int hitTagHash)
    {
        if (hitTagHash == ConcreteTagHash)
        {
            footstepSystem.footstepEmitter = concreteEvent;
            return;
        }

        if (hitTagHash == CarpetTagHash)
        {
            footstepSystem.footstepEmitter = carpetEvent;
            return;
        }

        if (hitTagHash == WoodTagHash)
        {
            footstepSystem.footstepEmitter = woodEvent;
            return;
        }

    }
}

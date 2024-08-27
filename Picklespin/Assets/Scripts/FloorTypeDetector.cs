using FMODUnity;
using UnityEngine;

public class FloorTypeDetector : MonoBehaviour
{
    public static FloorTypeDetector instance;

    private FootstepSystem footstepSystem;
    private JumpLandSignals jumpLand;

    [SerializeField] private StudioEventEmitter concreteEvent;
    [SerializeField] private StudioEventEmitter carpetEvent;
    [SerializeField] private StudioEventEmitter woodEvent;

    [SerializeField] private StudioEventEmitter concreteLandingEvent;
    [SerializeField] private StudioEventEmitter carpetLandingEvent;
    [SerializeField] private StudioEventEmitter woodLandingEvent;

    private int hitTagCached;

    private static readonly int ConcreteTagHash = "Concrete".GetHashCode();
    private static readonly int CarpetTagHash = "Carpet".GetHashCode();
    private static readonly int WoodTagHash = "Wood".GetHashCode();


    private void Awake()
    {
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
        jumpLand = JumpLandSignals.instance;
        footstepSystem = FootstepSystem.instance;
    }

    public void Check()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            int hitTag = hit.collider.tag.GetHashCode();

            if (hitTag != hitTagCached)
            {
                ChangeFootstepSound(hitTag);
                hitTagCached = hitTag;
            }
        }
    }



    private void ChangeFootstepSound(int hitTagHash)
    {
        if (hitTagHash == ConcreteTagHash)
        {
            jumpLand.landSoftEmitter = concreteLandingEvent;
            footstepSystem.footstepEmitter = concreteEvent;
            return;
        }

        if (hitTagHash == CarpetTagHash)
        {
            jumpLand.landSoftEmitter = carpetLandingEvent;
            footstepSystem.footstepEmitter = carpetEvent;
            return;
        }

        if (hitTagHash == WoodTagHash)
        {
            jumpLand.landSoftEmitter = woodLandingEvent;
            footstepSystem.footstepEmitter = woodEvent;
            return;
        }

    }
}

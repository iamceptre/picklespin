using UnityEngine;
using FMODUnity;

public class FootstepSystem : MonoBehaviour
{
    public static FootstepSystem instance { private set; get; }
    private FloorTypeDetector floorTypeDetector;

    [SerializeField] private CharacterController controller;
    [SerializeField] private PlayerMovement playerMovement;
    public StudioEventEmitter footstepEmitter;
    [SerializeField] private StudioEventEmitter clothEmitter;
    [SerializeField] private StudioEventEmitter evenFootstepLayerEmitter;
    [SerializeField] private StudioEventEmitter jumpEventEmitter;

    private int footstepCount = 0;
    //public enum MovementState { Sneak = 0, Walk = 1, Run = 2 }

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

    private void Start()
    {
        floorTypeDetector = FloorTypeDetector.instance;

        if (CameraBob.instance != null)
        {
            CameraBob.instance.OnFootstep += PlayFootstepSound;
        }
        else
        {
            Debug.LogError("CameraBob instance not found!");
        }
    }

    private void OnDestroy()
    {
        if (CameraBob.instance != null)
        {
            CameraBob.instance.OnFootstep -= PlayFootstepSound;
        }
    }

    private void PlayFootstepSound()
    {
        if (controller.isGrounded && playerMovement.anyMovementKeysPressed)
        {
            footstepEmitter.Play();

            footstepCount++;
            if (footstepCount % 2 == 0)
            {
                evenFootstepLayerEmitter.Play();
            }

            floorTypeDetector.Check();
        }
    }

    public void SendJumpSignal()
    {
        jumpEventEmitter.Play();
    }

}

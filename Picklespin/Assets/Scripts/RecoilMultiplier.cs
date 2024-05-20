using UnityEngine;

public class RecoilMultiplier : MonoBehaviour
{
    public static RecoilMultiplier instance { get; private set; }
    private CharacterControllerVelocity speedometer;
    private PlayerMovement playerMovement;

    public float currentRecoil;

    [SerializeField] private float jumpingRecoil = 1.2f;
    [SerializeField] private float sprintingRecoil = 0.3f;

    private void Start()
    {
        playerMovement = PlayerMovement.instance;
        speedometer = CharacterControllerVelocity.instance;
        //InvokeRepeating("UpdateRecoil", 0, 0.1f);
    }


    public void UpdateRecoil()
    {
        if (playerMovement.movementStateForFMOD != 2) {
            currentRecoil = speedometer.verticalVelocity * jumpingRecoil;
        }
        else
        {
            currentRecoil = speedometer.horizontalVelocity * sprintingRecoil;
        }
    }


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

}

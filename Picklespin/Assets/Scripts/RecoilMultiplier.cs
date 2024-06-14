using UnityEngine;

public class RecoilMultiplier : MonoBehaviour
{
    public static RecoilMultiplier instance { get; private set; }
    private CharacterControllerVelocity speedometer;

    public float currentRecoil;

     private float jumpingRecoil = 1.5f;
     private float sprintingRecoil = 0.2f;

    private void Start()
    {
        speedometer = CharacterControllerVelocity.instance;
    }


    public void UpdateRecoil()
    {
        currentRecoil = ((speedometer.verticalVelocity * jumpingRecoil) + (speedometer.horizontalVelocity * sprintingRecoil)) * 0.01f;
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

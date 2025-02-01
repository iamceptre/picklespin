using UnityEngine;

public class RecoilMultiplier : MonoBehaviour
{
    public static RecoilMultiplier instance { get; private set; }
    private CharacterControllerVelocity speedometer;

    public float currentRecoil;

     private readonly float jumpingRecoil = 1.5f;
     private readonly float sprintingRecoil = 0.2f;
    private float oldVert, oldHor;

    private void Start()
    {
        speedometer = CharacterControllerVelocity.instance;
    }


    private void Update()
    {
        float newVert = speedometer.verticalVelocity;
        float newHor = speedometer.horizontalVelocity;

        if (!Mathf.Approximately(oldVert, newVert) || !Mathf.Approximately(oldHor, newHor))
        {
            oldVert = newVert;
            oldHor = newHor;
            currentRecoil = ((newVert * jumpingRecoil) + (newHor * sprintingRecoil*2)) * 0.01f;
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

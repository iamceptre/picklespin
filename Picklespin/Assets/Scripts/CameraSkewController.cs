using UnityEngine;

public class CameraSkewController : MonoBehaviour
{
    private PlayerMovement playerMovement;
    [SerializeField] private float maxSkewAngle = 10f; 
    [SerializeField] private float skewIntensity = 0.1f; 
    [SerializeField] private float skewSmoothSpeed = 20f; 


    private void Start()
    {
        playerMovement = PlayerMovement.Instance;
    }

    void Update()
    {
        Vector3 moveDirection = playerMovement.moveDirection;


        Vector3 skew = new Vector3(
            Mathf.Clamp(-moveDirection.z * -skewIntensity, -maxSkewAngle, maxSkewAngle),
            0,
            Mathf.Clamp(moveDirection.x * -skewIntensity, -maxSkewAngle, maxSkewAngle)
        );

        Quaternion targetRotation = Quaternion.Euler(
            skew.x,  
            0,       
            skew.z    
        );

        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * skewSmoothSpeed);
    }
}
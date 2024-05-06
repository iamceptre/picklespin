using UnityEngine;

public class CameraBob : MonoBehaviour {

    [SerializeField] private CharacterController characterController;
    
   [SerializeField] private float height = 0.5f;
    public float bobSpeed = 2;

    [SerializeField] private Transform hand;

    private Vector3 originalPosition = new Vector3 ();
    private Vector3 originalHandPosition = new Vector3();
    private Vector3 tempPos = new Vector3 ();

    public Transform toBob;

    private Vector3 handVelocity;
    private Vector3 camVelocity;

    [SerializeField] private float smoothing;

    private CharacterControllerVelocity speedometer;


    private void Start(){
        originalPosition = toBob.localPosition;
        originalHandPosition = hand.localPosition;
        speedometer = CharacterControllerVelocity.instance;
    }


    private void Update() {

        if (characterController.isGrounded)
        {
           Bob();
            HandBob();
        }
    }

    private void Bob()
    {
        tempPos.y = Mathf.Sin(Time.fixedTime * Mathf.PI * bobSpeed) * height * 0.3f * speedometer.horizontalVelocity;
        tempPos.x = Mathf.Sin(Time.fixedTime * Mathf.PI * bobSpeed * 0.5f) * height * speedometer.horizontalVelocity;


        toBob.localPosition = Vector3.SmoothDamp(toBob.localPosition, originalPosition + (tempPos), ref camVelocity, smoothing);

        toBob.transform.localEulerAngles += new Vector3(0,0, -tempPos.x * 0.5f );

    }

private void HandBob()
    {
        hand.localPosition = Vector3.SmoothDamp(hand.localPosition, originalHandPosition + (tempPos * 0.3f), ref handVelocity, smoothing);
    }


}
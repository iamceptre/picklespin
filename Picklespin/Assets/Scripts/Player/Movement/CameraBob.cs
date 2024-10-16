using UnityEngine;

public class CameraBob : MonoBehaviour
{
    public static CameraBob instance { private set; get; }

    [SerializeField] private CharacterController characterController;

    [SerializeField] private float height = 0.5f;
    public float bobSpeed = 2;

    [SerializeField] private Transform hand;

    private Vector3 originalPosition = new Vector3();
    private Vector3 originalHandPosition = new Vector3();
    private Vector3 tempPos = new Vector3();
    [SerializeField] private Transform referenceObject;

    public Transform toBob;

    private Vector3 handVelocity;
    private Vector3 camVelocity;

    [SerializeField] private float smoothing;

    private CharacterControllerVelocity speedometer;

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
        originalPosition = toBob.localPosition;
        originalHandPosition = hand.localPosition;
        speedometer = CharacterControllerVelocity.instance;
    }


    private void LateUpdate()
    {

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

        Quaternion referenceRotation = referenceObject.rotation;
        Vector3 relativeBobPosition = referenceRotation * tempPos;
        Vector3 targetPosition = originalPosition + relativeBobPosition;
        toBob.localPosition = Vector3.SmoothDamp(toBob.localPosition, targetPosition, ref camVelocity, smoothing);

    }

    private void HandBob()
    {
        hand.localPosition = Vector3.SmoothDamp(hand.localPosition, originalHandPosition + (tempPos * 0.3f), ref handVelocity, smoothing);
    }


}
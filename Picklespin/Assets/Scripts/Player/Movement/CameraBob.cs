using System;
using UnityEngine;

public class CameraBob : MonoBehaviour
{
    public static CameraBob instance { private set; get; }

    [SerializeField] private CharacterController characterController;
    [SerializeField] private float baseHeight = 0.5f;
    //public float bobSpeed = 2; 
    [SerializeField] private Transform hand;
    [SerializeField] private Transform referenceObject;
    public Transform toBob;
    [SerializeField] private float smoothing = 0.1f;

    [Header("Scaling Settings")]
    [SerializeField] private float minSpeed = 1f;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float minHeightScale = 0f;
    [SerializeField] private float maxHeightScale = 1f;

    [Header("Frequency Settings")]
    [SerializeField] private float minFrequency = 1f;
    [SerializeField] private float maxFrequency = 3f;

    [Header("Amplitude Settings")]
    [SerializeField] private float amplitudeY = 0.38f;
    [SerializeField] private float amplitudeX = 0.15f;

    [Header("Footstep Detection Settings")]
    [SerializeField, Range(0f, 1f)] private float footstepThreshold = 0f;

    public event Action OnFootstep;

    private Vector3 originalPosition;
    private Vector3 originalHandPosition;
    private Vector3 tempPos;
    private Vector3 handVelocity;
    private Vector3 camVelocity;
    private float bobTimer;
    private CharacterControllerVelocity speedometer;

    private float previousSineY;
    private float previousSpeed;

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

        // Initialize previousSineY and previousSpeed
        bobTimer = 0f;
        previousSineY = Mathf.Sin(bobTimer * Mathf.PI * 2f);
        previousSpeed = speedometer.horizontalVelocity;
    }

    private void LateUpdate()
    {
        if (characterController.isGrounded)
        {
            UpdateBobbing();
            UpdateHandBobbing();
        }
        else
        {
            ResetBobbing();
        }
    }

    private void UpdateBobbing()
    {
        float speed = speedometer.horizontalVelocity;

        if (previousSpeed < minSpeed && speed >= minSpeed)
        {
            ResetBobbingTimer();
        }

        if (speed < minSpeed)
        {
            ResetBobbing();
            previousSpeed = speed;
            return;
        }

        float speedClamped = Mathf.Clamp(speed, minSpeed, maxSpeed);

        float heightScale = Mathf.Lerp(minHeightScale, maxHeightScale,
                                       (speedClamped - minSpeed) / (maxSpeed - minSpeed));

        float frequency = Mathf.Lerp(minFrequency, maxFrequency,
                                      (speedClamped - minSpeed) / (maxSpeed - minSpeed));

        bobTimer += Time.deltaTime * frequency;

        float sineValueY = Mathf.Sin(bobTimer * Mathf.PI * 2f);
        float sineValueX = Mathf.Sin(bobTimer * Mathf.PI) * amplitudeX * heightScale;

        // Detect when sineValueY crosses above the threshold from below (mid-cycle)
        if (previousSineY < footstepThreshold && sineValueY >= footstepThreshold)
        {
            OnFootstep?.Invoke();
        }

        previousSineY = sineValueY;
        previousSpeed = speed;

        tempPos.y = sineValueY * baseHeight * amplitudeY * heightScale;
        tempPos.x = sineValueX * baseHeight;

        Quaternion referenceRotation = referenceObject.rotation;
        Vector3 relativeBobPosition = referenceRotation * tempPos;
        Vector3 targetPosition = originalPosition + relativeBobPosition;

        toBob.localPosition = Vector3.SmoothDamp(toBob.localPosition, targetPosition,
                                                ref camVelocity, smoothing);
    }

    private void UpdateHandBobbing()
    {
        Vector3 targetHandPos = originalHandPosition + (tempPos * 0.3f);
        hand.localPosition = Vector3.SmoothDamp(hand.localPosition, targetHandPos,
                                               ref handVelocity, smoothing);
    }

    public void ResetBobbing()
    {
        bobTimer = 0f;
        toBob.localPosition = Vector3.Lerp(toBob.localPosition, originalPosition,
                                          Time.deltaTime * smoothing);
        hand.localPosition = Vector3.Lerp(hand.localPosition, originalHandPosition,
                                         Time.deltaTime * smoothing);
    }

    private void ResetBobbingTimer()
    {
        bobTimer = 0f;
    }
}

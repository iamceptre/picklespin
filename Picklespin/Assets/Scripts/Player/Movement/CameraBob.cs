using UnityEngine;

public class CameraBob : MonoBehaviour {

    public CharacterController characterController;
    public FootstepSystem footstepSystem;
    
    public float height = 0.5f;
    public float bobSpeed = 2;

    public Transform hand;

    private Vector3 originalPosition = new Vector3 ();
    private Vector3 originalHandPosition = new Vector3();
    private Vector3 tempPos = new Vector3 ();

    public Transform toBob;


    private void Start(){
        originalPosition = toBob.localPosition;
        originalHandPosition = hand.localPosition;
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
        tempPos.y = Mathf.Sin(Time.fixedTime * Mathf.PI * bobSpeed) * height * 0.3f * footstepSystem.horizontalSpeed;
        tempPos.x = Mathf.Sin(Time.fixedTime * Mathf.PI * bobSpeed * 0.5f) * height * footstepSystem.horizontalSpeed;
        toBob.transform.localPosition = originalPosition + (tempPos);
    }

private void HandBob()
    {
        hand.localPosition = originalHandPosition - (tempPos * 0.2f);
    }


}
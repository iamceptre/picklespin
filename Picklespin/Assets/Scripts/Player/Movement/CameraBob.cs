using UnityEngine;

public class CameraBob : MonoBehaviour {

    public CharacterController characterController;
    public FootstepSystem footstepSystem;
    
   [SerializeField] private float height = 0.5f;
    public float bobSpeed = 2;

    [SerializeField] private Transform hand;

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
            //HandBob();
        }

    }

    private void Bob()
    {
        tempPos.y = Mathf.Sin(Time.fixedTime * Mathf.PI * bobSpeed) * height * 0.3f * footstepSystem.horizontalSpeed;
        tempPos.x = Mathf.Sin(Time.fixedTime * Mathf.PI * bobSpeed * 0.5f) * height * footstepSystem.horizontalSpeed;

        toBob.transform.localPosition = originalPosition + (tempPos);
        toBob.transform.localEulerAngles += new Vector3(0,0, -tempPos.x);
    }

private void HandBob()
    {
        hand.localPosition = originalHandPosition - (tempPos * 0.9f);
        //hand.transform.localEulerAngles = new Vector3(0, 0, -tempPos.x);
    }


}
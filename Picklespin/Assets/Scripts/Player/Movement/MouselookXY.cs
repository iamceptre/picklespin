using UnityEngine;

public class MouselookXY : MonoBehaviour
{
    Vector2 rotation = Vector2.zero;
    public float sensitivity = 3;

    public Transform body;
    public Transform mainCamera;
    public Transform hand;


   [SerializeField] private float damping;


    //float smooth = 0.6f;
    // float yVelocity = 0.0f;




    void Update()         //Rotation Action
    {
        rotation.y += Input.GetAxis("Mouse X");

        rotation.x -= Input.GetAxis("Mouse Y");


        body.rotation = Quaternion.Euler(0, rotation.y * sensitivity, 0);

        mainCamera.rotation = Quaternion.Euler(Mathf.Clamp(rotation.x, -89/sensitivity, 89/sensitivity) * sensitivity, rotation.y * sensitivity, 0);
     //   Debug.Log(mainCamera.rotation.x + " RX");

        // handTempQuaternion = Quaternion.Euler(Mathf.Clamp(rotation.x, -89 / sensitivity, 89 / sensitivity) * sensitivity * 0.95f, rotation.y * sensitivity, 0);
         hand.rotation = Quaternion.Lerp(hand.rotation, mainCamera.rotation, Time.deltaTime * damping); //fix hand jitter when high damping

       // float finalHandRotationX = Mathf.SmoothDamp(hand.rotation.x, mainCamera.rotation.x, ref yVelocity, smooth);
       // Debug.Log(hand.localEulerAngles.x + " HX");
        //Debug.Log(finalHandRotationX + " FHRX");
      //  float finalHandRotationY = Mathf.SmoothDamp(hand.rotation.y, mainCamera.rotation.y, ref yVelocity, smooth);

        //hand.rotation = Quaternion.Euler(finalHandRotationX, finalHandRotationY, 0);

    }
}
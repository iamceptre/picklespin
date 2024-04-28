using UnityEngine;

public class Bhop : MonoBehaviour
{

    [SerializeField] private CharacterController controller;

    public bool canBhop = false;

    [SerializeField] float timeWindowToBhop;

    private void Update()
    {
        if (!canBhop && !controller.isGrounded)
        {
            canBhop = true;
            Invoke("ResetCanBhop", timeWindowToBhop);
        }
    }

    private void ResetCanBhop()
    {
        canBhop = false;
    }

}

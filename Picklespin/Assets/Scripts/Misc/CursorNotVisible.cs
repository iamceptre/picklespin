using UnityEngine;

public class CursorNotVisible : MonoBehaviour
{
   private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}

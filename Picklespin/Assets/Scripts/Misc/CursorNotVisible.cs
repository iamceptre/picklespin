using UnityEngine;

public class CursorNotVisible : MonoBehaviour
{
   private void Start()
    {
        LockCursor();
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}

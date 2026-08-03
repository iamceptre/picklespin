using UnityEngine;

public class CursorNotVisible : MonoBehaviour
{
    [SerializeField] private bool unlockOnStart = false;
   private void Start()
    {
        if (!unlockOnStart)
        {
            LockCursor();
        }
        else
        {
            UnlockCursor();
        }
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

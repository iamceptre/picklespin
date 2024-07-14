using UnityEngine;

public class ResetFMODonEnable : MonoBehaviour
{
    private FMODResetManager fmodResetManager;

    private void Awake()
    {
        fmodResetManager = FMODResetManager.instance;
    }
    private void OnEnable()
    {
        fmodResetManager.ResetFMOD(false);
    }
}

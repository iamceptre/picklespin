using UnityEngine;
using Thinksquirrel.CShake;

public class DebugTestCameraShake : MonoBehaviour
{

    private CameraShake _camShake;
    [SerializeField]private KeyCode _key;

    private void Awake()
    {
        _camShake = GetComponent<CameraShake>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(_key))
        {
            _camShake.Shake();
        }
    }

}

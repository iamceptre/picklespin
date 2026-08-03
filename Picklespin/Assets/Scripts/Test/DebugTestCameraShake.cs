using UnityEngine;
using Thinksquirrel.CShake;

public class DebugTestCameraShake : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD

    private CameraShake _camShake;
    [SerializeField]private KeyCode _key;

    private void Awake()
    {
        _camShake = GetComponent<CameraShake>();
        DevLog.Info($"{nameof(DebugTestCameraShake)} armed: {_key} triggers a test camera shake", this);
    }

    private void Update()
    {
        if (InputCompat.GetKeyDown(_key))
        {
            _camShake.Shake();
        }
    }

#endif
}

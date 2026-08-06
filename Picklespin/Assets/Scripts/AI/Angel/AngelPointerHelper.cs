using UnityEngine;

public class AngelPointerHelper : MonoBehaviour
{
    public static AngelPointerHelper Instance { get; private set; }

    private HelperSpirit helperSpirit;
    private Transform angel;
    private bool guiding;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        EnsureSpirit();
    }

    private void EnsureSpirit()
    {
        if (!helperSpirit) helperSpirit = HelperSpirit.instance;
    }

    public void PointTo(Transform target)
    {
        angel = target;
        guiding = true;
        EnsureSpirit();
        if (helperSpirit) helperSpirit.ShowSpirit(angel);
    }

    public void StopPointingAt(Transform target)
    {
        if (angel == target) Stop();
    }

    public void Stop()
    {
        guiding = false;
        angel = null;
        EnsureSpirit();
        if (helperSpirit) helperSpirit.HideSpirit();
    }

    public void Pause()
    {
        EnsureSpirit();
        if (helperSpirit) helperSpirit.HideSpirit();
    }

    public void Resume()
    {
        EnsureSpirit();
        if (helperSpirit && guiding && angel) helperSpirit.ShowSpirit(angel);
    }
}

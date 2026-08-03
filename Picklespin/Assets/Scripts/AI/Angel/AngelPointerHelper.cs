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
        helperSpirit = HelperSpirit.instance;
    }

    public void PointTo(Transform target)
    {
        angel = target;
        guiding = true;
        if (helperSpirit) helperSpirit.ShowSpirit(angel);
    }

    public void Stop()
    {
        guiding = false;
        angel = null;
        if (helperSpirit) helperSpirit.HideSpirit();
    }

    public void Pause()
    {
        if (helperSpirit) helperSpirit.HideSpirit();
    }

    public void Resume()
    {
        if (helperSpirit && guiding && angel) helperSpirit.ShowSpirit(angel);
    }
}

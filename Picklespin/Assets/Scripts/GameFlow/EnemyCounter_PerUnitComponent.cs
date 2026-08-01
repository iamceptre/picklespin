using UnityEngine;

public class EnemyCounter_PerUnitComponent : MonoBehaviour
{
    private bool counted;

    private void OnEnable() => Register();

    private void Start() => Register();

    public void deCountMe()
    {
        Deregister();
    }

    public void StopCounting() => Deregister();

    public void CountAgain() => Register();

    private void Register()
    {
        if (counted || EnemyCounter.instance == null) return;
        counted = true;
        EnemyCounter.instance.Register();
    }

    private void OnDisable()
    {
        Deregister();
    }

    private void Deregister()
    {
        if (!counted) return;
        counted = false;
        if (EnemyCounter.instance != null) EnemyCounter.instance.Deregister();
    }
}

using UnityEngine;

public class EnemyCounter : MonoBehaviour
{
    public static EnemyCounter instance;
    private RoundSystem roundSystem;
    public int EnemyCount;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        roundSystem = RoundSystem.instance;
        UpdateRoundSystemSpeedMultiplier();
    }

    public void UpdateRoundSystemSpeedMultiplier()
    {
        if (EnemyCount<=0)
        {
            roundSystem.speedMultiplier = 3;
        }
    }
}

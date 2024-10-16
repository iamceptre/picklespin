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
        UpdateRoundSystemSpeedMultiplier(); //commenting this out will make the first round longer (good for showcase)
    }

    public void UpdateRoundSystemSpeedMultiplier() //boundle this with enemyCount modification
    {
        if (EnemyCount <= 0)
        {
            roundSystem.speedMultiplier = 3.3f;
            //Debug.Log(EnemyCount);
        }
    }
}

using UnityEngine;

public class EnemyCounter_PerUnitComponent : MonoBehaviour
{
    private EnemyCounter enemyCounter;
    private RoundSystem roundSystem;

    void Start()
    {
        enemyCounter = EnemyCounter.instance;
        roundSystem = RoundSystem.instance;
        enemyCounter.EnemyCount++;
    }

     public void deCountMe()
    {
        enemyCounter.EnemyCount--;
        enemyCounter.UpdateRoundSystemSpeedMultiplier();
    }

}

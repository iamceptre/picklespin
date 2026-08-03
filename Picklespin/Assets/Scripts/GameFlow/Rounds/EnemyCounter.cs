using UnityEngine;

// Tracks how many enemies are alive. While the arena is cleared, the round
// timer runs faster so the next wave arrives sooner.
public class EnemyCounter : MonoBehaviour
{
    public static EnemyCounter instance;

    [SerializeField, Tooltip("round-timer speed while the arena is cleared")]
    private float clearedArenaTimerSpeed = 5f;

    public int EnemyCount; // kept public for compatibility; use Register/Deregister to modify

    private RoundSystem roundSystem;

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

    public void Register()
    {
        EnemyCount++;
    }

    public void Deregister()
    {
        EnemyCount--;
        UpdateRoundSystemSpeedMultiplier();
    }

    public void UpdateRoundSystemSpeedMultiplier()
    {
        if (EnemyCount <= 0)
        {
            roundSystem.speedMultiplier = clearedArenaTimerSpeed;
        }
    }
}

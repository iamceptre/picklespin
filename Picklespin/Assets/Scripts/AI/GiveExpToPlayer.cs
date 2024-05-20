using UnityEngine;

public class GiveExpToPlayer : MonoBehaviour
{
    private PlayerEXP playerEXP;
    [SerializeField] private int howMuchXpIGive;
    [SerializeField] private string expSourceName;

    [HideInInspector] public bool wasLastShotAHeadshot;

    private Ammo ammo;
    private ManaSuckParticlesSpawner manaSuckParticlesSpawner;
    private PlayerHP playerHp;

    void Start()
    {
        playerEXP = PlayerEXP.instance;
        ammo = Ammo.instance;
        manaSuckParticlesSpawner = ManaSuckParticlesSpawner.instance;
        playerHp = PlayerHP.instance;   
    }

    public void GiveExp()
    {
        int howMuchStatsIGive = (int)(howMuchXpIGive * 0.1f);
        if (!wasLastShotAHeadshot)
        {
            ammo.GiveManaToPlayer(howMuchStatsIGive);
            manaSuckParticlesSpawner.Spawn(transform.position, howMuchStatsIGive);
            playerEXP.GivePlayerExp(howMuchXpIGive, expSourceName);
            playerHp.GiveHPToPlayer(howMuchStatsIGive);
            return;
        }
        else
        {
            int howMuchIGiveAfterHeadshot = (int)(howMuchStatsIGive * 1.5f);
            int headshotXP = (int)(howMuchXpIGive * 1.5f);
            playerEXP.GivePlayerExp(headshotXP, expSourceName+", eyeshot!");
            ammo.GiveManaToPlayer(howMuchIGiveAfterHeadshot);
            manaSuckParticlesSpawner.Spawn(transform.position, howMuchIGiveAfterHeadshot);
            playerHp.GiveHPToPlayer(howMuchIGiveAfterHeadshot);
        }
    }

}

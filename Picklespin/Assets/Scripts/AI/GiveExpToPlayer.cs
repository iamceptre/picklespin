using UnityEngine;

public class GiveExpToPlayer : MonoBehaviour
{
    private PlayerEXP playerEXP;
    [SerializeField] private int howMuchXpIGive;
    [SerializeField] private string expSourceName;

    [HideInInspector] public bool wasLastShotAHeadshot;

    private Ammo ammo;
    private PlayerHP playerHp;

    [SerializeField] private ParticleSystem manaSuckParticles;
    private ParticleMoveTowards particleMoveTowardsScript;

    void Start()
    {
        particleMoveTowardsScript = manaSuckParticles.gameObject.GetComponent<ParticleMoveTowards>();
        manaSuckParticles.Pause();
        manaSuckParticles.Clear();
        particleMoveTowardsScript.enabled = false;
        playerEXP = PlayerEXP.instance;
        ammo = Ammo.instance;
        playerHp = PlayerHP.instance;   
    }

    private void OnEnable()
    {
        manaSuckParticles.transform.parent = transform;
        manaSuckParticles.transform.localPosition = Vector3.zero;
    }

    public void GiveExp()
    {
        int howMuchStatsIGive = (int)(howMuchXpIGive * 0.1f);


        if (!wasLastShotAHeadshot)
        {
            ammo.GiveManaToPlayer(howMuchStatsIGive);
            playerEXP.GivePlayerExp(howMuchXpIGive, expSourceName);
            playerHp.GiveHPToPlayer(howMuchStatsIGive);
            GiveManaSuckParticles(howMuchStatsIGive);
            return;
        }
        else
        {
            int howMuchIGiveAfterHeadshot = (int)(howMuchStatsIGive * 1.5f);
            int headshotXP = (int)(howMuchXpIGive * 1.5f);
            playerEXP.GivePlayerExp(headshotXP, expSourceName+", eyeshot!");
            ammo.GiveManaToPlayer(howMuchIGiveAfterHeadshot);
            playerHp.GiveHPToPlayer(howMuchIGiveAfterHeadshot);
            GiveManaSuckParticles(howMuchIGiveAfterHeadshot);
        }
    }

    private void GiveManaSuckParticles(int particleCount)
    {
        particleMoveTowardsScript.enabled = true;
        manaSuckParticles.Clear();
        manaSuckParticles.Emit(particleCount);
        manaSuckParticles.Play();
        manaSuckParticles.transform.parent = null;
    }


}

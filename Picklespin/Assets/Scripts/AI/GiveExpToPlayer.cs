using UnityEngine;

public class GiveExpToPlayer : MonoBehaviour
{
    private PlayerEXP playerEXP;
    private Ammo ammo;
    private PlayerHP playerHp;

    [SerializeField] private int howMuchXpIGive;
    [SerializeField] private string expSourceName;
    [SerializeField] private Color expTextColor = Color.white;
    [SerializeField] private Color criticalExpTextColor = Color.black;
    [SerializeField] private ParticleSystem manaSuckParticles;

    [HideInInspector] public bool wasLastShotAHeadshot;

    private ParticleMoveTowards particleMoveTowardsScript;

    private void Start()
    {
        particleMoveTowardsScript = manaSuckParticles.GetComponent<ParticleMoveTowards>();
        playerEXP = PlayerEXP.instance;
        ammo = Ammo.instance;
        playerHp = PlayerHP.instance;

        ConfigureManaSuckParticles();
    }

    private void ConfigureManaSuckParticles()
    {
        manaSuckParticles.Pause();
        manaSuckParticles.Clear();
        particleMoveTowardsScript.enabled = false;
        manaSuckParticles.transform.parent = transform;
        manaSuckParticles.transform.localPosition = Vector3.zero;
    }

    public void GiveExp()
    {
        int howMuchStatsIGive = (int)(howMuchXpIGive * 0.2f);
        string colorHex = ColorUtility.ToHtmlStringRGBA(wasLastShotAHeadshot ? criticalExpTextColor : expTextColor);
        string coloredMessage = $"<color=#{colorHex}>{expSourceName}</color>";
        string eyeshotMessage = $"<color=#{colorHex}>, Eyeshot!</color>";

        int expAmount = wasLastShotAHeadshot ? (int)(howMuchXpIGive * 1.5f) : howMuchXpIGive;
        int statAmount = wasLastShotAHeadshot ? (int)(howMuchStatsIGive * 1.5f) : howMuchStatsIGive;

        playerEXP.GivePlayerExp(expAmount, wasLastShotAHeadshot ? coloredMessage + eyeshotMessage : coloredMessage);
        ammo.GiveManaToPlayer(statAmount);
        playerHp.GiveHPToPlayer(statAmount);

        GiveManaSuckParticles(statAmount);
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
using UnityEngine;

public class GiveExpToPlayer : MonoBehaviour
{
    private PlayerEXP playerEXP;
    [SerializeField] private int howMuchXpIGive;
    [SerializeField] private string expSourceName;

    [HideInInspector] public bool wasLastShotAHeadshot;

    void Start()
    {
        playerEXP = PlayerEXP.instance;
    }

    public void GiveExp()
    {
        if (!wasLastShotAHeadshot)
        {
            playerEXP.GivePlayerExp(howMuchXpIGive, expSourceName);
            return;
        }
        else
        {
            int headshotXP = (int)(howMuchXpIGive * 1.5f);
            playerEXP.GivePlayerExp(headshotXP, expSourceName+" eyeshot!");
        }
    }

}

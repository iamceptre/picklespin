using UnityEngine;

public class GiveExpToPlayer : MonoBehaviour
{
    private PlayerEXP playerEXP;
    [SerializeField] private int howMuchXpIGive;
    [SerializeField] private string expSourceName;

    void Start()
    {
        playerEXP = PlayerEXP.instance;
    }

    public void GiveExp()
    {
        playerEXP.GivePlayerExp(howMuchXpIGive,expSourceName);
    }

}

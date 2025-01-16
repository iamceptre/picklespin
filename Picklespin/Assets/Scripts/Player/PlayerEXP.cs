using UnityEngine;

public class PlayerEXP : MonoBehaviour
{
    public static PlayerEXP instance { get; private set; }
    public int playerExpAmount;
    private AddedEXP_Gui addedEXP_Gui;
    public int playerLevel;
    public int PlayerLevelStarting;

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    private void Start()
    {
        playerExpAmount = 0;
        CalculatePlayerLevel();
        PlayerLevelStarting = playerLevel;
        addedEXP_Gui = AddedEXP_Gui.instance;
    }

    public void GivePlayerExp(int howMuchExp, string ExpSource)
    {
        playerExpAmount += howMuchExp;
        addedEXP_Gui.DisplayAddedEXP(howMuchExp, ExpSource);
        CalculatePlayerLevel();
    }

    private void CalculatePlayerLevel()
    {
        playerLevel = (int)(playerExpAmount * 0.001f);
    }
}

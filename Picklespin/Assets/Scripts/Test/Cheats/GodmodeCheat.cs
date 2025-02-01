using UnityEngine;

public class GodmodeCheat : MonoBehaviour
{
    private const string targetWord = "god";
    private string inputString = "";
    private int targetIndex = 0;

    private CheatActivatedFeedback cheatActivatedFeedback;
    private PlayerHP playerHp;

    private void Start()
    {
        cheatActivatedFeedback = CheatActivatedFeedback.instance;
        playerHp = PlayerHP.Instance;
    }

    void Update()
    {
        if (Input.anyKeyDown && targetIndex < targetWord.Length)
        {
            char inputChar = Input.inputString.Length > 0 ? Input.inputString[0] : '\0';

            if (char.IsLetter(inputChar) || inputChar == ' ')
            {
                inputString += inputChar;

                if (inputChar == targetWord[targetIndex])
                {
                    targetIndex++;

                    if (targetIndex == targetWord.Length)
                    {
                        ActivateCheat();
                        ResetInput();
                    }
                }
                else
                {
                    ResetInput();
                }
            }
        }
    }

    void ResetInput()
    {
        inputString = "";
        targetIndex = 0;
    }

    void ActivateCheat()
    {
        cheatActivatedFeedback.Do("godmode");
        playerHp.godMode = true;
        enabled = false;
    }
}


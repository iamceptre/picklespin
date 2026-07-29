using UnityEngine;

public class GodmodeCheat : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
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
        if (InputCompat.AnyKeyDown && targetIndex < targetWord.Length)
        {
            char inputChar = InputCompat.TypedCharThisFrame;

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
#endif
}


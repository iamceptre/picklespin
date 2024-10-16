using UnityEngine;

public class GorillaCheat : MonoBehaviour
{
    private const string targetWord = "gorilla"; 
    private string inputString = ""; 
    private int targetIndex = 0;

    [SerializeField] private GameObject gorilla;

    private CheatActivatedFeedback cheatActivatedFeedback;

    private void Start()
    {
        cheatActivatedFeedback = CheatActivatedFeedback.instance;
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
        cheatActivatedFeedback.Do("gorilla");
       gorilla.SetActive(true);
       enabled = false;
    }
}


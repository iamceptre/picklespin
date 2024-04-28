using UnityEngine;

public class GorillaCheat : MonoBehaviour
{
    private const string targetWord = "gorilla"; 
    private string inputString = ""; 
    private int targetIndex = 0;

    [SerializeField] private GameObject gorilla;

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
                        DoSomething();
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

    void DoSomething()
    {
       gorilla.SetActive(true);
       enabled = false;
    }
}


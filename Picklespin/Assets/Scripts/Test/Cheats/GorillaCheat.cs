using UnityEngine;

public class GorillaCheat : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
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
        cheatActivatedFeedback.Do("gorilla");
       gorilla.SetActive(true);
       enabled = false;
    }
#endif
}


using UnityEngine;
using FMODUnity;

public class ActivateCheats : MonoBehaviour
{

    private const string targetWord = "oioi";
    private string inputString = "";
    private int targetIndex = 0;

    [SerializeField] private GameObject cheats;

    private CheatActivatedFeedback cheatActivatedFeedback;

    [SerializeField] private EventReference cheatmodeActivatedSound;

    private void Start()
    {
        cheatActivatedFeedback = CheatActivatedFeedback.instance;
        cheats.SetActive(false);
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
        RuntimeManager.PlayOneShot(cheatmodeActivatedSound);
        cheats.SetActive(true);
        Destroy(this);
    }
}

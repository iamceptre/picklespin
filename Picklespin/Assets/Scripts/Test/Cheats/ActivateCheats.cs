using UnityEngine;
using FMODUnity;

public class ActivateCheats : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD

    private const string targetWord = "poi";
    private string inputString = "";
    private int targetIndex = 0;

    [SerializeField] private GameObject cheats;

    private CheatActivatedFeedback cheatActivatedFeedback;

    [SerializeField] private EventReference cheatmodeActivatedSound;

    [SerializeField] private GameSpeedSlider gameSpeedSlider;

    private void Start()
    {
        cheatActivatedFeedback = CheatActivatedFeedback.instance;
        cheats.SetActive(false);
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
        RuntimeManager.PlayOneShot(cheatmodeActivatedSound);
        cheats.SetActive(true);
        gameSpeedSlider.Show();
        Destroy(this);
    }
#endif
}

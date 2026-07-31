using UnityEngine;

// Type a class name to take it: vesper, lightfoot, umbral, blastfool, bastion, sanctus.
public class PlayerClassCheat : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private static readonly (string Word, PlayerClassId Class)[] classWords =
    {
        ("vesper", PlayerClassId.Vesper),
        ("lightfoot", PlayerClassId.Lightfoot),
        ("umbral", PlayerClassId.Umbral),
        ("blastfool", PlayerClassId.Blastfool),
        ("bastion", PlayerClassId.Bastion),
        ("sanctus", PlayerClassId.Sanctus)
    };

    // one place in each word, so words that start alike ("bastion", "blastfool") are
    // never chasing the same letter
    private readonly int[] targetIndex = new int[classWords.Length];

    private CheatActivatedFeedback cheatActivatedFeedback;

    private void Start()
    {
        cheatActivatedFeedback = CheatActivatedFeedback.instance;
    }

    void Update()
    {
        if (!InputCompat.AnyKeyDown) return;

        char inputChar = char.ToLowerInvariant(InputCompat.TypedCharThisFrame);
        if (!char.IsLetter(inputChar)) return;

        for (int i = 0; i < classWords.Length; i++)
        {
            string word = classWords[i].Word;

            if (inputChar == word[targetIndex[i]])
            {
                targetIndex[i]++;
                if (targetIndex[i] < word.Length) continue;

                ActivateCheat(classWords[i].Word, classWords[i].Class);
                ResetInput();
                return;
            }

            // a wrong letter can still be the start of the same word ("bbastion")
            targetIndex[i] = inputChar == word[0] ? 1 : 0;
        }
    }

    void ResetInput()
    {
        for (int i = 0; i < targetIndex.Length; i++) targetIndex[i] = 0;
    }

    void ActivateCheat(string word, PlayerClassId playerClass)
    {
        // through the menu, so the one-off stat changes are the angel's own; an unwired
        // menu still switches every rule PlayerClasses owns, it just grants no stats
        if (!PlayerClassMenu.Instance || !PlayerClassMenu.Instance.Take(playerClass))
        {
            PlayerClasses.Choose(playerClass);
        }

        if (cheatActivatedFeedback) cheatActivatedFeedback.Do(word);
        // stays enabled, unlike the one-shot cheats: switching classes is the point
    }
#endif
}

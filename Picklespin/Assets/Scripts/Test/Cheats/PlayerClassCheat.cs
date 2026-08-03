using UnityEngine;

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

    private readonly int[] targetIndex = new int[classWords.Length];

    private static PlayerClassCheat host;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (host) return;

        GameObject go = new(nameof(PlayerClassCheat)) { hideFlags = HideFlags.HideInHierarchy };
        DontDestroyOnLoad(go);
        host = go.AddComponent<PlayerClassCheat>();

        string words = string.Join(", ", System.Array.ConvertAll(classWords, entry => entry.Word));
        DevLog.Info($"{nameof(PlayerClassCheat)} armed: type a class name to take it - {words}", host);
    }

    void Update()
    {
        if (host != this) return;
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

            targetIndex[i] = inputChar == word[0] ? 1 : 0;
        }
    }

    void ResetInput()
    {
        for (int i = 0; i < targetIndex.Length; i++) targetIndex[i] = 0;
    }

    void ActivateCheat(string word, PlayerClassId playerClass)
    {
        if (!PlayerClassMenu.Instance || !PlayerClassMenu.Instance.Take(playerClass))
        {
            PlayerClasses.Choose(playerClass);
        }

        if (CheatActivatedFeedback.instance) CheatActivatedFeedback.instance.Do(word);
    }
#endif
}

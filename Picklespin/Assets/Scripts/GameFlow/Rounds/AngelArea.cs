using UnityEngine;

public static class AngelArea
{
    public static bool PlayerInside
    {
        get
        {
            RoundSystem rounds = RoundSystem.instance;
            return rounds && rounds.PlayerInAngelArea;
        }
    }

    public static bool Shelters(AiReferences refs) => PlayerInside && refs && !refs.IsAngel;
}

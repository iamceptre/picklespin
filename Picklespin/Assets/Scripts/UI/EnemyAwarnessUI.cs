using System.Collections;
using UnityEngine;

public class EnemyAwarnessUI : MonoBehaviour
{
    [SerializeField] CanvasFader canvasFader;
    // realtime: the win screen freezes time the moment the player escapes, and
    // the icon still has to notice the enemies gave up and fade out
    readonly WaitForSecondsRealtime refreshRate = new(0.42f);
    bool isVisible, wasVisible;

    IEnumerator Start()
    {
        while (true)
        {
            isVisible = StateManager.IsAnyAIInAttackOrLoosing();
            if (isVisible != wasVisible)
            {
                if (isVisible) canvasFader.FadeIn();
                else canvasFader.FadeOut();
                wasVisible = isVisible;
            }
            yield return refreshRate;
        }
    }
}

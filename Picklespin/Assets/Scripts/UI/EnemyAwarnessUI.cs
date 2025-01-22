using System.Collections;
using UnityEngine;

public class EnemyAwarnessUI : MonoBehaviour
{
    [SerializeField] CanvasFader canvasFader;
    readonly WaitForSeconds refreshRate = new(0.42f);
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

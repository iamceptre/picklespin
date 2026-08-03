using System.Collections;
using UnityEngine;

public class WhiteEnemyEyeCycler : MonoBehaviour
{
    private WaitForSeconds onTimeQuick = new WaitForSeconds(3f);
    private WaitForSeconds onTimeLong = new WaitForSeconds(5f);

    private WaitForSeconds offTimeQuick = new WaitForSeconds(3f);
    private WaitForSeconds offTimeLong = new WaitForSeconds(7f);

    [SerializeField] private WhiteEnemyEye eye;

    [SerializeField, Tooltip("renderer used to decide if this enemy is on screen (assign the body). If empty, the eye cycles regardless of visibility.")]
    private Renderer visibilitySource;

    [SerializeField] private AiHealth aiHp;

    // polled directly instead of OnBecameVisible events: the eye renderer starts
    // disabled on pooled reuse, so renderer events on this object can never fire
    private bool OnScreen => visibilitySource == null || visibilitySource.isVisible;

    private void OnEnable()
    {
        // per-activation, not Start: pooled enemies must restart the eye cycle every life
        eye.ResetEye();
        StartCoroutine(CycleEye());
    }

    private IEnumerator CycleEye()
    {
        while (enabled)
        {
            int random1 = Random01();
            int random2 = Random01();

            if (random1 == 0)
            {
                yield return offTimeLong;
            }
            else
            {
                yield return offTimeQuick;
            }

            // a corpse must not re-open its weak point while it dissolves
            if (aiHp && !aiHp.IsAlive) yield break;

            if (OnScreen)
            {
                eye.On();

                if (random2 == 0)
                {
                    yield return onTimeLong;
                }
                else
                {
                    yield return onTimeQuick;
                }

                eye.Off();
            }
        }
    }

    private int Random01()
    {
        int randomized = Random.Range(0, 2);
        return randomized;
    }
}

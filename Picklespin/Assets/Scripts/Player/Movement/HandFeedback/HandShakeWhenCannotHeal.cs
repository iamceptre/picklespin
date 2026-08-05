using UnityEngine;

public class HandShakeWhenCannotHeal : MonoBehaviour
{
    private Animator handAnimator;

    private void Start()
    {
        handAnimator = PublicPlayerHandAnimator.instance._animator;
    }

    public void ShakeHand()
    {
        Shake();
        if (SpecialAbilitySystem.instance) SpecialAbilitySystem.instance.PlayLockFeedback();
    }

    public void Shake() => handAnimator.SetTrigger("Hand_Fail");
}

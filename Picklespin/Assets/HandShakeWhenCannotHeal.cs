using UnityEngine;

public class HandShakeWhenCannotHeal : MonoBehaviour
{
    [SerializeField] private AngelHeal angelHeal;
    private Animator handAnimator;

    private void Start()
    {
        handAnimator = PublicPlayerHandAnimator.instance._animator;
    }
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            if (!angelHeal.enabled)
            {
                ShakeHand();
            }
        }
        
    }

    public void ShakeHand()
    {
        handAnimator.SetTrigger("Hand_Fail");
    }
}

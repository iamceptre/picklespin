using UnityEngine;

public class BridgeSupport : MonoBehaviour
{
    [SerializeField] CollapseBridgeIfNoSupport theBridgeImHolding;


    public void myCollapse()
    {
        //Debug.Log("SupportSendingSignal");
        theBridgeImHolding.DecreaseSupport();
    }
}

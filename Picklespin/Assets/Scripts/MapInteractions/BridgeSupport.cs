using UnityEngine;

public class BridgeSupport : MonoBehaviour
{
    [SerializeField] CollapseBridgeIfNoSupport theBridgeImHolding;

    public void myCollapse()
    {
        theBridgeImHolding.DecreaseSupport();
    }
}

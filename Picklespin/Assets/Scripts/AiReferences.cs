using UnityEngine;

public class AiReferences : MonoBehaviour
{

    public AiHealth Health;
    public AiVision Vision;
    public AiHealthUiBar HpUiBar;
    [Tooltip("when dying")]public GiveExpToPlayer GiveExp;
    public MaterialFlashWhenHit MaterialFlash;

}


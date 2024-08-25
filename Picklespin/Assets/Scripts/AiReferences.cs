using FMODUnity;
using UnityEngine;

public class AiReferences : MonoBehaviour
{

    public AiHealth Health;
    public AiVision Vision;
    public AiHealthUiBar HpUiBar;
    [Tooltip("when dying")]public GiveExpToPlayer GiveExp;
    public MaterialFlashWhenHit MaterialFlash;
    public ParticleSystem HeadshotParticle;
    public SetOnFire setOnFire;

    public StudioEventEmitter damageTakenBig;
    public StudioEventEmitter damageTakenSmall;
    public StudioEventEmitter damageTakenEyeshot;
    public StudioEventEmitter damageTakenCritical;

}


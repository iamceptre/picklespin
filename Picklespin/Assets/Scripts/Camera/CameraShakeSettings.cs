using UnityEngine;

[System.Serializable]
public class CameraShakeSettings
{
    [Tooltip("master multiplier for this shake - 0 means it never runs at all")]
    public float strength = 1f;
    [Tooltip("how far the camera rocks on each axis")]
    public Vector3 rotationAmount = new(0.2f, 0.2f, 0.2f);
    [Tooltip("how many times it rocks back and forth")]
    public int numberOfShakes = 3;
    public float speed = 50f;
    [Tooltip("0-1: higher settles sooner")]
    public float decay = 0.6f;
    [Tooltip("how much the HUD rides along; 0 = it stays put")]
    public float uiShakeModifier = 1f;

    public bool IsSilent => strength <= 0f || rotationAmount == Vector3.zero;
}

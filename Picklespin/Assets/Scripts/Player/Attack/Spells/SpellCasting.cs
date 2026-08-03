using UnityEngine;

public class SpellCasting : MonoBehaviour
{
    [SerializeField, Tooltip("seconds of hold before the shot is charged")]
    private float castDuration = 1f;

    public float Duration => castDuration;

    public bool IsCharged => castDuration != 0f;
}

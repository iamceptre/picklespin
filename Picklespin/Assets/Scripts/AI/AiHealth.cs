using UnityEngine;
using UnityEngine.Events;

public class AiHealth : MonoBehaviour
{

    public UnityEvent deathEvent;

    [Range(0, 100)] public float hp = 100;

}

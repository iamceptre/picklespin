using UnityEngine;

public class CollectGcStart : MonoBehaviour
{
    void Start()
    {
       System.GC.Collect();
    }
}

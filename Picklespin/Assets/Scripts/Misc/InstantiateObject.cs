using UnityEngine;

public class InstantiateObject : MonoBehaviour
{

    [SerializeField] private GameObject objectToInstantiate;
    void Start()
    {
        Instantiate(objectToInstantiate, transform);
    }

}

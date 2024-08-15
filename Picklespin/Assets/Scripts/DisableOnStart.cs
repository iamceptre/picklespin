using UnityEngine;

public class DisableOnStart : MonoBehaviour
{
    void Start()
    {
        gameObject.GetComponent<Canvas>().enabled = false;
        gameObject.SetActive(false);
    }

}

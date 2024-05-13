using UnityEngine;

public class PickleActivator : MonoBehaviour
{
public void Toggle()
    {
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}

using UnityEngine;

public class PickleActivator : MonoBehaviour
{
    public void Toggle() => gameObject.SetActive(!gameObject.activeSelf);
}

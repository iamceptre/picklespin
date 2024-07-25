using UnityEngine;

public class ResetPlayerPrefs : MonoBehaviour
{
    public void Do()
    {
        PlayerPrefs.DeleteAll();
    }
}

using UnityEngine;
using DG.Tweening;

public class QuickLightFadeOut : MonoBehaviour
{
    void Start()
    {
        Light light = GetComponent<Light>();
        light.DOColor(Color.black, 1f);
    }

}

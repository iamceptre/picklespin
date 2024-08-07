using UnityEngine;
using DG.Tweening;
public class AngelRingEyesOpenClose : MonoBehaviour
{

    [SerializeField] private Material closedEyeMaterial;
    private Material openEyeMaterial;
    [SerializeField] private Renderer[] eyeRends;
    private Color closedMatColor;
    [SerializeField] private AngelEyeOpen angelEyeOpenManager;

    [SerializeField] private TrailRenderer[] ringTrails;

    private bool firstSet = false;

    private void Awake()
    {

        for (int i = 0; i < eyeRends.Length; i++)
        {
            openEyeMaterial = eyeRends[i].material;
        }

        closedMatColor = closedEyeMaterial.color;

    }

    public void Open()
    {
        angelEyeOpenManager.OpenEye(true);
        for (int i = 0; i < eyeRends.Length; i++)
        {
            eyeRends[i].material = openEyeMaterial;
            eyeRends[i].material.color = Color.black;
            eyeRends[i].material.DOColor(Color.white, 1);

            ringTrails[i].emitting = true;
        }
    }

    public void Close()
    {
        if (firstSet)
        {
            angelEyeOpenManager.OpenEye(false);
        }
        firstSet = true;

        for (int i = 0; i < eyeRends.Length; i++)
        {
            eyeRends[i].material = closedEyeMaterial;
            ringTrails[i].emitting = false;
        }
    }

}

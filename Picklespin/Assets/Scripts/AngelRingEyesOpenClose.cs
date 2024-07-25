using UnityEngine;
using DG.Tweening;
public class AngelRingEyesOpenClose : MonoBehaviour
{

    [SerializeField] private Material closedEyeMaterial;
    private Material openEyeMaterial;
    [SerializeField] private Renderer[] eyeRends;
    private Color closedMatColor;

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
        for (int i = 0; i < eyeRends.Length; i++)
        {
            eyeRends[i].material = openEyeMaterial;
            eyeRends[i].material.color = Color.black;
            eyeRends[i].material.DOColor(Color.white, 1);
        }
    }

    public void Close()
    {
        for (int i = 0; i < eyeRends.Length; i++)
        {
            eyeRends[i].material = closedEyeMaterial;
        }
    }

}

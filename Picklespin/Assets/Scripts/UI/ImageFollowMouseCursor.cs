using UnityEngine;

public class ImageFollowMouseCursor : MonoBehaviour
{

    private Canvas myCanvas;


    private void Awake()
    {
        myCanvas = GetComponentInParent<Canvas>();  
    }

    void Update()
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(myCanvas.transform as RectTransform, Input.mousePosition, myCanvas.worldCamera, out pos);
        transform.position = myCanvas.transform.TransformPoint(pos);
    }
}

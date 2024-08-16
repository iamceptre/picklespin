using UnityEngine;

public class FloatUpDown : MonoBehaviour
{
    public float height = 0.5f;
    public float speed = 1f;

    private Vector3 posOffset;
    private float sinMultiplier;

    void Start()
    {
        posOffset = transform.localPosition;
        sinMultiplier = Mathf.PI * speed;
    }

    void Update()
    {
        float newY = posOffset.y + Mathf.Sin(Time.time * sinMultiplier) * height;
        Vector3 newPos = transform.localPosition;
        newPos.y = newY;
        transform.localPosition = newPos;
    }
}
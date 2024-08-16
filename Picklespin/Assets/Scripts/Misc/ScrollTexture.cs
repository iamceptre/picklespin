using UnityEngine;

public class ScrollTexture : MonoBehaviour
{
    [SerializeField] private float scrollSpeedY = 0.4f;
    [SerializeField] private float scrollSpeedX = 0.3f;
    [SerializeField] private bool randomUVoffsetAtEnable = false;

    private Renderer rend;
    private Vector2 offset;
    private bool isVisible;

    private void Awake()
    {
        rend = GetComponent<Renderer>() ?? GetComponent<SkinnedMeshRenderer>();

        if (randomUVoffsetAtEnable)
        {
            offset = new Vector2(Random.value, Random.value);
        }
        else
        {
            offset = Vector2.zero;
        }
    }

    private void Update()
    {
        if (!isVisible)
        {
            return;
        }

        offset.x += scrollSpeedX * Time.deltaTime;
        offset.y += scrollSpeedY * Time.deltaTime;

        rend.material.SetTextureOffset("_MainTex", offset);
    }

    private void OnBecameVisible()
    {
        isVisible = true;
    }

    private void OnBecameInvisible()
    {
        isVisible = false;
    }
}
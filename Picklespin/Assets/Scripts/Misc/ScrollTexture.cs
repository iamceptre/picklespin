using UnityEngine;

public class ScrollTexture : MonoBehaviour
{
    [SerializeField] private float scrollSpeedY = 0.4f;
    [SerializeField] private float scrollSpeedX = 0.3f;

    private Renderer rend;
    private Vector2 offset;

    private float offsetY;
    private float offsetX;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        //offset = new Vector2(0, 0);
        RandomizeStartingOffset();
    }

    private void RandomizeStartingOffset()
    {
        offset = new Vector2(Random.Range(-1,1), Random.Range(-1, 1));
    }

    private void Update()
    {
        
        offsetY = Time.time * scrollSpeedY;
        offsetX = Time.time * scrollSpeedX;

        offset.y = offsetY;
        offset.x = offsetX;

        
        rend.material.SetTextureOffset("_MainTex", offset);
    }
}
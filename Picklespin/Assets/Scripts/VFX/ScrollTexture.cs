using UnityEngine;

public class ScrollTexture : MonoBehaviour
{
    [SerializeField] private float scrollSpeedY = 0.4f;
    [SerializeField] private float scrollSpeedX = 0.3f;
    [SerializeField] private bool randomUVoffsetAtEnable = false;

    private Material materialInstance;
    private Vector2 offset;
    private bool isVisible;

    private static readonly int mainTexID = Shader.PropertyToID("_MainTex");

    private void Awake()
    {
        if (!TryGetComponent(out Renderer rend))
        {
            DevLog.Warn($"{nameof(ScrollTexture)} on {name} has no Renderer to scroll - disabling", this);
            enabled = false;
            return;
        }

        materialInstance = rend.material;

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

        materialInstance.SetTextureOffset(mainTexID, offset);
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
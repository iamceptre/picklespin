using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class HeadshotMarker : MonoBehaviour
{
    [Tooltip("switched off together with the sprite so an idle marker ticks nothing - the LookAt on this object if left empty")]
    [SerializeField] private Behaviour billboard;

    public SpriteRenderer Sprite { get; private set; }

    private void Awake()
    {
        Sprite = GetComponent<SpriteRenderer>();
        if (!billboard) billboard = GetComponent<LookAt>();
        SetVisible(false);
    }

    private void OnEnable() => HeadshotLesson.Register(this);

    private void OnDisable() => HeadshotLesson.Unregister(this);

    public void SetVisible(bool visible)
    {
        Sprite.enabled = visible;
        if (billboard) billboard.enabled = visible;
    }
}

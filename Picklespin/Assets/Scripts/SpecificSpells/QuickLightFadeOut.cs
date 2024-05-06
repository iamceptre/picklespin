using UnityEngine;
using DG.Tweening;

public class QuickLightFadeOut : MonoBehaviour
{
    private Light me;
    [SerializeField] private Bullet bullet;
    private bool faded=false;
    void Awake()
    {
        me = GetComponent<Light>();
    }

    private void Start()
    {
        bullet = transform.parent.GetComponent<Bullet>();
        me.DOColor(Color.black, 1.5f).OnComplete(() =>
        {
            faded = true;
        });
    }

    private void Update()
    {
        if (bullet.hitSomething && gameObject.transform.parent != null)
        {
            gameObject.transform.parent = null;
            Destroy(bullet.gameObject);
        }

        if (faded && gameObject.transform.parent == null)
        {
            Destroy(gameObject);
        }

    }

}

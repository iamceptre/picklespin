using UnityEngine;
using DG.Tweening;

public class Portal_ShrinkInnerRing : MonoBehaviour
{
    [SerializeField] private EscapeTimer escapeTimer;
    private ParticleSystem myParticleSystem;

    private void Awake()
    {
        myParticleSystem = GetComponent<ParticleSystem>();
    }


    private void OnEnable()
    {
        float duration = 1 / escapeTimer.countdownSpeed;
        DOTween.To(() => 0.3f, x => SetShapeScale(x), 0, duration);
        DOTween.To(() => 0, x => SetColor(x), 0.07f, 3).SetEase(Ease.InExpo);
    }

    void SetShapeScale(float scale)
    {
        var shape = myParticleSystem.shape;
        shape.radius = scale;
    }

    void SetColor(float alpha)
    {
        var main = myParticleSystem.main;
        main.startColor = new Color(1, 1, 1, alpha);
    }
}

using UnityEngine;
using DG.Tweening;

public class AngelEyeOpen : MonoBehaviour
{
    private SkinnedMeshRenderer _skinnedMeshRenderer;
    private float eyeOpenness;
    private float maxEyeOpenness = 44;
    private float actualAnimationTarget;
    private float animationTime = 3;

    private void Awake()
    {
        _skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
    }

    public void OpenEye(bool open)
    {
        if (!open)
        {
            actualAnimationTarget = 0;
        }
        else
        {
            actualAnimationTarget = maxEyeOpenness;
        }

        //Debug.Log("opening eye " + actualAnimationTarget);

        DOTween.To(() => eyeOpenness, x => eyeOpenness = x, actualAnimationTarget, animationTime).OnUpdate(() =>
        {
            _skinnedMeshRenderer.SetBlendShapeWeight(2, eyeOpenness);
        });
    }

}

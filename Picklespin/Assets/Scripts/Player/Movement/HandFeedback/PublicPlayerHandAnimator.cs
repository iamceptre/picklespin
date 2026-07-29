using UnityEngine;

public class PublicPlayerHandAnimator : MonoBehaviour
{
    public static PublicPlayerHandAnimator instance { get; private set; }
    public Animator _animator;

    private void Awake()
    {
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }

        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

}

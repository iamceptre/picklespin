using UnityEngine;

public class Helper_Arrow : MonoBehaviour //works only with objects that are not moving
{
    public static Helper_Arrow instance {  get; private set; }

    public Transform target;
    private Renderer _rend;

    private void Awake()
    {
        _rend = GetComponent<Renderer>();

        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    private void Update()
    {
        Vector3 targetDirection = (target.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(targetDirection);
    }

    public void HideArrow()
    {
        //Debug.Log("hideing arrow");
        _rend.enabled = false;
        enabled = false;
    }

    public void ShowArrow(Transform _target) {
        target = _target;
        ShowArrow();
    }

    public void ShowArrow()
    {
        //Debug.Log("showing arrow without updating da target");
        enabled = true;
        _rend.enabled = true;
    }

}

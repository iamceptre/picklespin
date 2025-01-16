using UnityEngine;

public class Helper_Arrow : MonoBehaviour //works only with objects that are not moving
{
    public static Helper_Arrow Instance {  get; private set; }

    public Transform target;
    private Renderer _rend;

    private HelperSpirit helperSpirit;

    private bool showing;

    private void Awake()
    {
        _rend = GetComponent<Renderer>();

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        helperSpirit = HelperSpirit.instance;
    }

    private void Update()
    {
        Vector3 targetDirection = (target.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(targetDirection);
    }

    public void HideArrow()
    {
        if (helperSpirit != null)
        {
            helperSpirit.HideSpirit();
        }
        //Debug.Log("hideing arrow");
        _rend.enabled = false;
        enabled = false;
        showing = false;
    }

    public void ShowArrow(Transform _target) {
        if (helperSpirit != null)
        {
            helperSpirit.ShowSpirit(_target);
        }
        target = _target;
        ShowArrow();
    }

    public void ShowArrow()
    {
        //Debug.Log("showing arrow without updating da target");
        enabled = true;
        _rend.enabled = true;
        showing = true;
    }
    public void UnHideArrowWhenUnpausing()
    {
        if (showing)
        {
            _rend.enabled = true;
        }
    }

}

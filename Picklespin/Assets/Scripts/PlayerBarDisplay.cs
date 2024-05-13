using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBarDisplay : MonoBehaviour
{
    [Header("For Mana Bar: ")]
    [SerializeField] private Ammo ammo;
    [Header("For Hp Bar: ")]
    [SerializeField] private PlayerHP playerHP;
    [Header("For Stamina Bar: ")]
    [SerializeField] private PlayerMovement playerMovement;

    private Slider mySlider;
    private float desiredBarPosition;

    private float velocity;


    private void Awake()
    {
        mySlider = GetComponentInChildren<Slider>();
    }

    private void Start()
    {
        RefreshBarValue();
    }

    public void Refresh(bool smooth)
    {
        if (smooth)
        {
            RefreshBarValueSmooth();
        }
        else
        {
            RefreshBarValue();
        }
    }

    public void RefreshBarValue()
    {
        CommonRefreshCode();
        mySlider.value = desiredBarPosition;
    }

    public void RefreshBarValueSmooth()
    {
        CommonRefreshCode();
        StartCoroutine(Smoother());
    }

    private IEnumerator Smoother()
    {
        while (mySlider.value < desiredBarPosition - 1)
        {
            mySlider.value = Mathf.SmoothDamp(mySlider.value, desiredBarPosition, ref velocity, 0.3f);
            yield return null;
        }
    }


    private void CommonRefreshCode()
    {
        if (ammo != null)
        {
            desiredBarPosition = ((float)ammo.ammo / (float)ammo.maxAmmo) * 100;
        }

        if (playerHP != null)
        {
            desiredBarPosition = ((float)playerHP.hp / (float)playerHP.maxHp) * 100;
        }

        if (playerMovement != null)
        {
            desiredBarPosition = playerMovement.stamina;
        }

    }

}

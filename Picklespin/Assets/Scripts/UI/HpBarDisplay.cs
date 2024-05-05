using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HpBarDisplay : MonoBehaviour
{
    public static HpBarDisplay instance;
    private Slider hpSlider;
    [SerializeField] private BarEase barEase;
    private PlayerHP playerHP;


    private float desiredSliderValue;


    private float velocity;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }

        hpSlider = GetComponent<Slider>();
    }

    private void Start()
    {
       playerHP = PlayerHP.instance;
       RefreshHPBar();
    }

    public void RefreshHPBar()
    {
            CommonRefreshCode();
            hpSlider.value = desiredSliderValue;
    }


    public void RefreshHPBarSmooth()
    {
        CommonRefreshCode();
        StartCoroutine(Smoother());
    }


    private void CommonRefreshCode()
    {
        desiredSliderValue = ((float)playerHP.hp / (float)playerHP.maxHp) * 100;
        playerHP.CheckIfPlayerIsDead();
    }

    private IEnumerator Smoother()
    {
        while (hpSlider.value < desiredSliderValue - 1)
        {
            hpSlider.value = Mathf.SmoothDamp(hpSlider.value, desiredSliderValue, ref velocity, 0.3f);
            yield return null;
        }
    }


}

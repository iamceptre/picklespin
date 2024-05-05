using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AmmoDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text ammoDisplayText;
    [SerializeField] private Ammo ammo;
    [SerializeField]private Slider manaBar;
    private float velocity;

    private float desiredManaBarPosition;

    public static AmmoDisplay instance { get; private set; }


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
    }


    private void Start()
    {
        ammoDisplayText = GetComponent<TMP_Text>();
        RefreshManaValue(); 
    }

    public void RefreshManaValue()
    {
        CommonRefreshCode();
        manaBar.value = desiredManaBarPosition;
    }

    public void RefreshManaValueSmooth()
    {
        CommonRefreshCode();
        StartCoroutine(Smoother());
    }

    private IEnumerator Smoother()
    {
        while (manaBar.value < desiredManaBarPosition - 1)
        {
            manaBar.value = Mathf.SmoothDamp(manaBar.value, desiredManaBarPosition, ref velocity, 0.3f);
            yield return null;
        }
    }


    private void CommonRefreshCode()
    {
        ammoDisplayText.text = (ammo.ammo + "/" + ammo.maxAmmo);
        desiredManaBarPosition = ((float)ammo.ammo / (float)ammo.maxAmmo)*100;
    }

}

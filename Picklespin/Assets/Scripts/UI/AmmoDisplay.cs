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
    private float desiredManaBarPositionTimes100;
  
    

    private void Start()
    {
        ammoDisplayText = GetComponent<TMP_Text>();
        RefreshManaValue(); 
    }

    public void RefreshManaValue()
    {
        CommonRefreshCode();
        manaBar.value = desiredManaBarPositionTimes100;
    }

    public void RefreshManaValueSmooth()
    {
        CommonRefreshCode();
        StartCoroutine(Smoother());
    }


    private IEnumerator Smoother()
    {
      //  Debug.Log("rutyna");
        if (manaBar.value < desiredManaBarPositionTimes100-1)
        {
           // Debug.Log("damping");
            manaBar.value = Mathf.SmoothDamp(manaBar.value, desiredManaBarPositionTimes100, ref velocity, 0.3f);
            yield return null;
            StartCoroutine(Smoother());
        }
        else
        {
            //Debug.Log("damped");
            manaBar.value = desiredManaBarPositionTimes100;
            yield return null;
            StopAllCoroutines();
        }
    }


    private void CommonRefreshCode()
    {
        ammoDisplayText.text = (ammo.ammo + "/" + ammo.maxAmmo);
        desiredManaBarPosition = (float)ammo.ammo / (float)ammo.maxAmmo;
        desiredManaBarPositionTimes100 = desiredManaBarPosition * 100;
    }

}

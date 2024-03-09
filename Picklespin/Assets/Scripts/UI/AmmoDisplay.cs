using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AmmoDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text ammoDisplayText;
    [SerializeField] private Ammo ammo;
    [SerializeField]private Slider manaBar;

    private float desiredManaBarPosition;
    private float velocity;
    

    private void Start()
    {
        ammoDisplayText = GetComponent<TMP_Text>();
        StartCoroutine(RefreshText()); 
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            StartCoroutine(RefreshText());  
        }

        if (manaBar.value != desiredManaBarPosition) {
            manaBar.value = Mathf.SmoothDamp(manaBar.value, desiredManaBarPosition, ref velocity, 0.1f);
        }
    }

    public IEnumerator RefreshText()
    {
        ammoDisplayText.text = (ammo.ammo + "/" + ammo.maxAmmo);
        desiredManaBarPosition = (float)ammo.ammo / (float)ammo.maxAmmo;
        yield return null;
    }

}


//Make it refresh with events later on, to avoid a big mess

using System.Collections;
using TMPro;
using UnityEngine;

public class AmmoDisplay : MonoBehaviour
{
    public TMP_Text ammoDisplayText;
    public Ammo ammo;

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
    }

    public IEnumerator RefreshText()
    {
        ammoDisplayText.text = ("magicka: " + "<br>" + ammo.ammo + "/" + ammo.maxAmmo);
        yield return null;
    }

}


//Make it refresh with delegates later on, to avoid a big mess

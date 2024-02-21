using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AmmoDisplay : MonoBehaviour
{
    public TMP_Text ammoDisplayText;
    public Ammo ammo;

    private void Start()
    {
        ammoDisplayText = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        // ammoDisplayText.text = (ammo.ammo.ToString());
        ammoDisplayText.text = ("magicka: " + "<br>" + ammo.ammo);

    }

}


//Make it refresh with delegates later on, to avoid a big mess

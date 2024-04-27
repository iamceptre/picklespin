using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EscapeTimer : MonoBehaviour
{
    [SerializeField] private Slider mySlider;
    private Death death;

    [SerializeField] private float countdownSpeed = 1;
    [SerializeField] private float countdownDelayTime = 2;

    [SerializeField] private GameObject[] enableWithMe;
    [SerializeField] private GameObject[] disableWithMe;


    private void EnableOtherObjects()
    {
        for (int i = 0; i < enableWithMe.Length; i++)
        {
            enableWithMe[i].SetActive(true);
        }
    }

    private void DisableOtherObjects()
    {
        for (int i = 0; i < disableWithMe.Length; i++)
        {
            disableWithMe[i].SetActive(false);
        }
    }


   private IEnumerator Start()
    {

        death = Death.instance;

        if (enableWithMe.Length > 0)
        {
            EnableOtherObjects();
        }

        if (disableWithMe.Length > 0)
        {
            DisableOtherObjects();
        }

        yield return new WaitForSeconds(countdownDelayTime);

        while (true)
        {
            mySlider.value -= Time.deltaTime * countdownSpeed;

            if (mySlider.value <= 0)
            {
                death.PlayerDeath();
                enabled = false;
            }

            yield return null;
        }
    }

}

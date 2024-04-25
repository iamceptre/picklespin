using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ExitGame : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(Surprise());
    }

    private IEnumerator Surprise()
    {
        //SURPRISE MOTHERFUCKER!
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        Image me = gameObject.GetComponent<Image>();
        me.color = Color.black;
        Application.Quit();
    }

}

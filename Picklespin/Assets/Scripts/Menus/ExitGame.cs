using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ExitGame : MonoBehaviour
{

    private WaitForEndOfFrame waitFrame = new WaitForEndOfFrame();
    void Start()
    {
        StartCoroutine(Surprise());
    }

    private IEnumerator Surprise()
    {
        //SURPRISE MOTHERFUCKER!
        yield return waitFrame;
        yield return waitFrame;
        yield return waitFrame;
        yield return waitFrame;
        Image me = gameObject.GetComponent<Image>();
        me.color = Color.black;
        Application.Quit();
    }

}

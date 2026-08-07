using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ExitGame : MonoBehaviour
{
    private readonly WaitForEndOfFrame waitFrame = new();

    private void Start() => StartCoroutine(Surprise());

    private IEnumerator Surprise()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        //SURPRISE MOTHERFUCKER!
        yield return waitFrame;
        yield return waitFrame;
        yield return waitFrame;

        if (TryGetComponent(out Image me)) me.color = Color.black;

        SceneFlow.Quit();
    }
}

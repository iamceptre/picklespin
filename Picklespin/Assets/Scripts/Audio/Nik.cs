using FMODUnity;
using System.Collections;
using UnityEngine;

public class Nik : MonoBehaviour
{
    private SpriteRenderer _renderer;

    private WaitForSeconds showtime = new WaitForSeconds(0.1f);
    private WaitForSeconds hidetime = new WaitForSeconds(5);

    private KeyCode nikKey = KeyCode.N;

    [SerializeField] private StudioEventEmitter _emitter;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _renderer.enabled = false;
    }
    private IEnumerator Start()
    {
        while (true)
        {
            yield return hidetime;

            if (Input.GetKey(nikKey))
            {
                _emitter.Play();
                _renderer.enabled = true;
                yield return showtime;
                _renderer.enabled = false;
            }

        }

    }
}

using UnityEngine;

public class ApplyFpsLimit : MonoBehaviour
{

    private CheatActivatedFeedback feedback;
    [SerializeField] private FPSLimit fpsLimit;
    private void Start()
    {
        feedback = CheatActivatedFeedback.instance;
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            fpsLimit.SetFramerate();
        }
    }
}

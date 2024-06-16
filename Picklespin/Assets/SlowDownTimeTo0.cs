using UnityEngine;

public class SlowDownTimeTo0 : MonoBehaviour
{
    private float slowdownDuration = 2;
    private float currentAnimationTime = 0f;
    private float startScale;


    void Update()
    {
            currentAnimationTime += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(startScale, 0f, currentAnimationTime / slowdownDuration);

            if (currentAnimationTime >= slowdownDuration)
            {
                Time.timeScale = 0f;
                enabled = false;
            }
    }

    void Start()
    {
        currentAnimationTime = 0f;
        startScale = Time.timeScale;
    }
}

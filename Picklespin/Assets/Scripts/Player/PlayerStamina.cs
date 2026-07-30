//MOVE REST OF THE STAMINA CODE FROM PLAYER MOVEMENT HERE
using UnityEngine;
using FMODUnity;

public class PlayerStamina : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private bool canPlayExhaustedSound = true;
    [SerializeField] private EventReference exhaustedSound;

    void Start()
    {
        playerMovement = PlayerMovement.Instance;
    }

    void Update()
    {
        // PlayerMovement owns the exhausted state (bar emptied, until it has
        // recovered) — this used to keep its own copy of the rule with its own
        // hardcoded threshold, so the sound could drift out of step with it
        if (!playerMovement) return;

        if (playerMovement.IsExhausted)
        {
            if (canPlayExhaustedSound)
            {
                canPlayExhaustedSound = false;
                ExhaustedPlayer();
            }
        }
        else
        {
            canPlayExhaustedSound = true;
        }
    }


    private void ExhaustedPlayer()
    {
        //Play Red Light of exhausted bar
        RuntimeManager.PlayOneShot(exhaustedSound);
    }
}
